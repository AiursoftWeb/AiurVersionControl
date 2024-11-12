using Aiursoft.AiurObserver;
using Aiursoft.AiurStore.Models;

namespace Aiursoft.AiurEventSyncer.Abstract
{
    public interface IRepository<T>
    {
        IOutOnlyDatabase<Commit<T>> Commits { get; }
        IAsyncObservable<List<Commit<T>>> AppendCommitsHappened { get; }
        void OnPulled(IEnumerable<Commit<T>> subtraction, IRemote<T> remoteRecord);
        void OnPushed(IEnumerable<Commit<T>> commitsToPush);
    }
}
