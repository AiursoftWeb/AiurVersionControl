using AiurEventSyncer.Abstract;
using AiurEventSyncer.Tools;
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
        public InOutDatabase<Commit<T>> Commits => _commits;
        public Commit<T> Head => Commits.LastOrDefault();

        private readonly InOutDatabase<Commit<T>> _commits;
        private readonly ConcurrentDictionary<Guid, Func<List<Commit<T>>, Task>> _onAppendCommitsAsyncSubscribers = new();
        private readonly ConcurrentDictionary<Guid, Action<List<Commit<T>>>> _onAppendCommitsSubscribers = new();
        private readonly TaskQueue _notifyingQueue = new TaskQueue(1);

        public Repository(InOutDatabase<Commit<T>> dbProvider)
        {
            _commits = dbProvider;
        }

        public Repository() : this(new MemoryAiurStoreDb<Commit<T>>()) { }
        public void RegisterAsyncTask(Guid key, Func<List<Commit<T>>, Task> action)
        {
            _onAppendCommitsAsyncSubscribers[key] = action;
        }

        public void Register(Guid key, Action<List<Commit<T>>> action)
        {
            _onAppendCommitsSubscribers[key] = action;
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
                var tasks = notiyTasks.Select(t => Task.Run(() => t.Value(newCommits))).ToArray();
                Task.WhenAll(tasks).Wait();
            }
        }

        public bool OnPulled(IEnumerable<Commit<T>> subtraction, IRemote<T> remoteRecord)
        {
            var newCommitsAppended = new List<Commit<T>>();
            var pushingPushPointer = false;
            var inserted = false;

            lock (this)
            {
                foreach (var commit in subtraction)
                {
                    var (resultMode, pointer) = OnPulledCommit(commit, remoteRecord.PullPointer);
                    if (remoteRecord.PullPointer == remoteRecord.PushPointer)
                    {
                        pushingPushPointer = true;
                    }
                    if (remoteRecord.PullPointer != pointer)
                    {
                        remoteRecord.PullPointer = pointer;
                        remoteRecord.OnPullPointerMoved(pointer);
                    }
                    else
                    {
                        throw new InvalidOperationException("Update pointer failed. Seems it inserted to the same position.");
                    }

                    if (pushingPushPointer == true)
                    {
                        remoteRecord.PushPointer = remoteRecord.PullPointer;
                    }
                    if (resultMode == InsertMode.Appended)
                    {
                        newCommitsAppended.Add(commit);
                    }
                    if (resultMode == InsertMode.MiddleInserted)
                    {
                        inserted = true;
                    }
                }
            }
            if (newCommitsAppended.Any())
            {
                OnAppendCommits(newCommitsAppended);
            }

            return inserted;
        }

        private (InsertMode result, Commit<T> pointer) OnPulledCommit(Commit<T> subtract, Commit<T> position)
        {
            var localAfter = _commits.GetAllAfter(position).FirstOrDefault();
            if (localAfter is not null)
            {
                if (localAfter.Id != subtract.Id)
                {
                    _commits.InsertAfter(position, subtract);
                    return (InsertMode.MiddleInserted, subtract);
                }
                else
                {
                    return (InsertMode.Ignored, localAfter);
                }
            }
            else
            {
                _commits.Add(subtract);
                return (InsertMode.Appended, subtract);
            }
        }

        public void OnPushed(IEnumerable<Commit<T>> commitsToPush, string startPosition)
        {
            var newCommitsAppended = new List<Commit<T>>();
            lock (this)
            {
                foreach (var commit in commitsToPush) // 4,5,6
                {
                    var appended = OnPushedCommit(commit, startPosition);
                    startPosition = commit.Id;
                    if (appended)
                    {
                        newCommitsAppended.Add(commit);
                    }
                }
            }
            if (newCommitsAppended.Any())
            {
                OnAppendCommits(newCommitsAppended);
            }
        }

        private bool OnPushedCommit(Commit<T> subtract, string position)
        {
            var localAfter = _commits.GetCommitsAfterId<Commit<T>, T>(position).FirstOrDefault();
            if (localAfter is not null)
            {
                if (subtract.Id == localAfter.Id)
                {
                    return false;
                }
                else
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
        }
    }
}
