using AiurEventSyncer.Abstract;
using AiurEventSyncer.Tools;
using AiurObserver;
using AiurStore.Models;
using AiurStore.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AiurEventSyncer.Models
{
    public class Repository<T> : IRepository<T>
    {
        public string Name { get; init; } = string.Empty;
        public IOutOnlyDatabase<Commit<T>> Commits => _commits;
        public Commit<T> Head => Commits.LastOrDefault();
        public IAsyncObservable<List<Commit<T>>> AppendCommitsHappened => _subscribersManager;

        private readonly InOutDatabase<Commit<T>> _commits;
        private readonly AsyncObservable<List<Commit<T>>> _subscribersManager = new();
        private readonly TaskQueue _notifyingQueue = new();

        public Repository(InOutDatabase<Commit<T>> dbProvider = null)
        {
            dbProvider ??= new MemoryAiurStoreDb<Commit<T>>();
            _commits = dbProvider;
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

        protected virtual void OnAppendCommits(List<Commit<T>> newCommits)
        {
            var subscriberTasks = _subscribersManager.Boradcast(newCommits);
            if (subscriberTasks.Any())
            {
                _notifyingQueue.QueueNew(() => Task.WhenAll(subscriberTasks));
            }
        }

        public void OnPulled(IEnumerable<Commit<T>> subtraction, IRemote<T> remoteRecord)
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
            if (inserted)
            {
                remoteRecord.OnPullInsert();
            }
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
                foreach (var commit in commitsToPush)
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
