using System.Collections.Concurrent;

namespace AiurObserver
{
    public class AsyncSubscription<T> : IDisposable
    {
        private readonly ConcurrentBag<IAsyncObserver<T>> _observers;
        private IAsyncObserver<T> _observer;

        internal AsyncSubscription(ConcurrentBag<IAsyncObserver<T>> observers, IAsyncObserver<T> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observers.Contains(_observer))
            {
                var removed = _observers.TryTake(out _observer);
                if (!removed) throw new Exception("Failed to remove observer.");
            }
        }
    }
}
