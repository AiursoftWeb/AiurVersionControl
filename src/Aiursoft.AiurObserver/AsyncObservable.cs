namespace AiurObserver
{
    public class AsyncObservable<T> : IAsyncObservable<T>
    {
        private readonly List<IAsyncObserver<T>> _observers = new();
        private readonly object _lock = new();

        public ISubscription Subscribe(IAsyncObserver<T> observer)
        {
            lock (_lock)
            {
                if (!_observers.Contains(observer))
                {
                    _observers.Add(observer);
                }
                else
                {
                    throw new Exception("This observer is already subscribed!");
                }
            }

            return new AsyncSubscription(unRegisterAction: () =>
            {
                lock (_lock)
                {
                    if (_observers.Contains(observer))
                    {
                        var removed = _observers.Remove(observer);
                        if (!removed) throw new Exception("Failed to remove observer.");
                    }
                    else
                    {
                        throw new Exception("This observer is not subscribed!");
                    }
                }
            });
        }

        public IEnumerable<Task> Broadcast(T newEvent)
        {
            return _observers.Select(t => t.OnTrigger(newEvent));
        }

        public int GetListenerCount()
        {
            return _observers.Count;
        }
    }
}