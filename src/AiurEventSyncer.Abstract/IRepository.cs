using AiurStore.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AiurEventSyncer.Abstract
{
    public interface IRepository<T>
    {
        InOutDatabase<Commit<T>> Commits { get; }
        IAsyncObservable<List<Commit<T>>> AppendCommitsHappened { get; }
        void OnPulled(IEnumerable<Commit<T>> subtraction, IRemote<T> remoteRecord);
        void OnPushed(IEnumerable<Commit<T>> commitsToPush, string startPosition);
    }
}
