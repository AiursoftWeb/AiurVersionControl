using AiurEventSyncer.Abstract;
using AiurEventSyncer.Tools;
using AiurStore.Abstracts;
using AiurStore.Models;
using AiurStore.Providers.MemoryProvider;
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
        public ConcurrentDictionary<Guid, Func<List<Commit<T>>, Task>> OnNewCommitsSubscribers { get; set; } = new ConcurrentDictionary<Guid, Func<List<Commit<T>>, Task>>();

        private readonly InOutDatabase<Commit<T>> _commits;
        private readonly SemaphoreSlim _pullingLock = new SemaphoreSlim(1);
        private readonly TaskQueue _notifyingQueue = new TaskQueue(1);

        public Repository(InOutDatabase<Commit<T>> dbProvider)
        { 
            _commits = dbProvider;
        }

        public Repository() : this(new MemoryAiurStoreDb<Commit<T>>()) { }

        public void Commit(T content)
        {
            CommitObject(new Commit<T> { Item = content });
        }

        public void CommitObject(Commit<T> commitObject)
        {
            _commits.Add(commitObject);
            OnNewCommits(new List<Commit<T>>{ commitObject });
        }

        private void OnNewCommits(List<Commit<T>> newCommits)
        {
            var notiyTasks = OnNewCommitsSubscribers.ToList();
            if (notiyTasks.Any())
            {
                _notifyingQueue.QueueNew(async () =>
                {
                    await Task.WhenAll(notiyTasks.Select(t => t.Value(newCommits)));
                });
            }
        }

        public async Task OnPulled(List<Commit<T>> subtraction, Remote<T> remoteRecord)
        {
            var newCommitsSaved = new List<Commit<T>>();
            var pushingPushPointer = false;

            await _pullingLock.WaitAsync();
            foreach (var commit in subtraction)
            {
                var inserted = OnPulledCommit(commit, remoteRecord.PullPointer);
                if (remoteRecord.PullPointer == remoteRecord.PushPointer)
                {
                    pushingPushPointer = true;
                }
                remoteRecord.PullPointer = commit.Id;
                if (pushingPushPointer == true)
                {
                    remoteRecord.PushPointer = remoteRecord.PullPointer;
                }
                if (inserted)
                {
                    newCommitsSaved.Add(commit);
                }
            }
            _pullingLock.Release();
            if (newCommitsSaved.Any())
            {
                OnNewCommits(newCommitsSaved);
            }
        }

        private bool OnPulledCommit(Commit<T> subtract, string position)
        {
            var localAfter = _commits.AfterCommitId(position).FirstOrDefault();
            if (localAfter is not null)
            {
                if (localAfter.Id != subtract.Id)
                {
                    _commits.InsertAfterCommitId(position, subtract);
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
                OnNewCommits(newCommitsSaved);
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
