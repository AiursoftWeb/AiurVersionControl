using System.Collections.Concurrent;

namespace AiurObserver
{
    public class AsyncObservable<T> : IAsyncObservable<T>
    {
        private readonly ConcurrentBag<IAsyncObserver<T>> _observers = new ConcurrentBag<IAsyncObserver<T>>();

        public IDisposable Subscribe(IAsyncObserver<T> observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
            return new AsyncSubscription<T>(_observers, observer);
        }

        public IEnumerable<Task> Broadcast(T newEvent)
        {
            return _observers.Select(t => t.OnHappen(newEvent));
        }

        public int GetListenerCount()
        {
            return _observers.Count;
        }
    }
}
