using Aiursoft.AiurEventSyncer.Abstract;
using Aiursoft.AiurObserver;
using Aiursoft.AiurStore.Models;
using Aiursoft.AiurStore.Providers;
using Aiursoft.AiurEventSyncer.Tools;

// ReSharper disable InconsistentlySynchronizedField

namespace Aiursoft.AiurEventSyncer.Models
{
    /// <summary>
    /// The collection of the commits which helps you sync between other repositories.
    /// </summary>
    /// <typeparam name="T">The inner object type of the commit.</typeparam>
    public class Repository<T> : IRepository<T>
    {
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
            lock (this)
            {
                _commits.Add(commitObject); 
            }
            OnAppendCommits([commitObject]);
        }

        protected virtual void OnAppendCommits(List<Commit<T>> newCommits)
        {
            _notifyingQueue.QueueNew(() => _subscribersManager.BroadcastAsync(newCommits));
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
                        remoteRecord.OnPullPointerMovedForwardOnce(pointer);
                    }
                    else
                    {
                        throw new InvalidOperationException("Update pointer failed. Seems it inserted to the same position.");
                    }

                    if (pushingPushPointer)
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
                if (localAfter.Id == subtract.Id)
                {
                    return (InsertMode.Ignored, localAfter);
                }
                _commits.InsertAfter(position, subtract);
                return (InsertMode.MiddleInserted, subtract);

            }

            _commits.Add(subtract);
            return (InsertMode.Appended, subtract);
        }

        public void OnPushed(IEnumerable<Commit<T>> commitsToPush)
        {
            var newCommitsAppended = new List<Commit<T>>();
            lock (this)
            {
                foreach (var commit in commitsToPush)
                {
                    OnPushedCommit(commit);
                    newCommitsAppended.Add(commit);
                }
            }
            if (newCommitsAppended.Any())
            {
                OnAppendCommits(newCommitsAppended);
            }
        }

        private void OnPushedCommit(Commit<T> subtract)
        {
            _commits.Add(subtract);
        }
    }
}
