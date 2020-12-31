using AiurEventSyncer.Abstract;
using AiurEventSyncer.Tools;
using AiurStore.Abstracts;
using AiurStore.Models;
using AiurStore.Providers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AiurEventSyncer.Models
{
    public class Repository<T> : IRepository<T>
    {
        public string Name { get; init; } = string.Empty;
        public IAfterable<Commit<T>> Commits => _commits;
        public Commit<T> Head => Commits.LastOrDefault();

        private readonly InOutDatabase<Commit<T>> _commits;
        private readonly ConcurrentDictionary<Guid, Func<List<Commit<T>>, Task>> _onAppendCommitsAsyncSubscribers = new();
        private readonly ConcurrentDictionary<Guid, Func<List<Commit<T>>, Task>> _onAppendCommitsSubscribers = new();
        private readonly SemaphoreSlim _pullingLock = new SemaphoreSlim(1);
        private readonly TaskQueue _notifyingQueue = new TaskQueue(1);

        public Repository(InOutDatabase<Commit<T>> dbProvider)
        {
            _commits = dbProvider;
        }

        public Repository() : this(new MemoryAiurStoreDb<Commit<T>>()) { }

        public void Register(Guid key, Func<List<Commit<T>>, Task> action, bool async = true)
        {
            if (async)
            {
                _onAppendCommitsAsyncSubscribers[key] = action;
            }
            else
            {
                _onAppendCommitsSubscribers[key] = action;
            }
        }

        public void UnRegister(Guid key)
        {
            _ = _onAppendCommitsAsyncSubscribers.TryRemove(key, out _) || _onAppendCommitsSubscribers.TryRemove(key, out _);
        }

        public void Commit(T content)
        {
            CommitObject(new Commit<T> { Item = content });
        }

        public void CommitObject(Commit<T> commitObject)
        {
            _commits.Add(commitObject);
            OnAppendCommits(new List<Commit<T>> { commitObject });
        }

        private void OnAppendCommits(List<Commit<T>> newCommits)
        {
            var asyncNotiyTasks = _onAppendCommitsAsyncSubscribers.ToList();
            var notiyTasks = _onAppendCommitsSubscribers.ToList();
            if (asyncNotiyTasks.Any())
            {
                _notifyingQueue.QueueNew(async () =>
                {
                    await Task.WhenAll(tasks: asyncNotiyTasks.Select(t => t.Value(newCommits)));
                });
            }
            if (notiyTasks.Any())
            {
                Task.WhenAll(notiyTasks.Select(t => t.Value(newCommits))).Wait();
            }
        }

        public async Task OnPulled(List<Commit<T>> subtraction, IRemote<T> remoteRecord)
        {
            var newCommitsSaved = new List<Commit<T>>();
            var pushingPushPointer = false;

            await _pullingLock.WaitAsync();
            foreach (var commit in subtraction)
            {
                var (appended, inserted) = OnPulledCommit(commit, remoteRecord.PullPointer);
                if (remoteRecord.PullPointer == remoteRecord.PushPointer)
                {
                    pushingPushPointer = true;
                }
                remoteRecord.PullPointer = commit.Id;
                if (pushingPushPointer == true)
                {
                    remoteRecord.PushPointer = remoteRecord.PullPointer;
                }
                if (appended)
                {
                    newCommitsSaved.Add(commit);
                }
            }
            _pullingLock.Release();
            if (newCommitsSaved.Any())
            {
                OnAppendCommits(newCommitsSaved);
            }
        }

        private (bool appended, bool inserted) OnPulledCommit(Commit<T> subtract, string position)
        {
            var localAfter = _commits.AfterCommitId(position).FirstOrDefault();
            if (localAfter is not null)
            {
                if (localAfter.Id != subtract.Id)
                {
                    _commits.InsertAfterCommitId(position, subtract);
                    return (false, true);
                }
            }
            else
            {
                _commits.Add(subtract);
                return (true, false);
            }
            return (false, false);
        }

        public async Task OnPushed(IEnumerable<Commit<T>> commitsToPush, string startPosition)
        {
            var newCommitsSaved = new List<Commit<T>>();
            await _pullingLock.WaitAsync();
            foreach (var commit in commitsToPush) // 4,5,6
            {
                var inserted = OnPushedCommit(commit, startPosition);
                startPosition = commit.Id;
                if (inserted)
                {
                    newCommitsSaved.Add(commit);
                }
            }
            _pullingLock.Release();
            if (newCommitsSaved.Any())
            {
                OnAppendCommits(newCommitsSaved);
            }
        }

        private bool OnPushedCommit(Commit<T> subtract, string position)
        {
            var localAfter = _commits.AfterCommitId(position).FirstOrDefault();
            if (localAfter is not null)
            {
                if (subtract.Id != localAfter.Id)
                {
                    _commits.Add(subtract);
                    return true;
                }
            }
            else
            {
                _commits.Add(subtract);
                return true;
            }
            return false;
        }
    }
}
