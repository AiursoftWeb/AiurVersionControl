using AiurObserver;
using AiurStore.Models;
using System.Collections.Generic;

namespace AiurEventSyncer.Abstract
{
    public interface IRepository<T>
    {
        IOutDatabase<Commit<T>> Commits { get; }
        IAsyncObservable<List<Commit<T>>> AppendCommitsHappened { get; }
        void OnPulled(IEnumerable<Commit<T>> subtraction, IRemote<T> remoteRecord);
        void OnPushed(IEnumerable<Commit<T>> commitsToPush, string startPosition);
    }
}
