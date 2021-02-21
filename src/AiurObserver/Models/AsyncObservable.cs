using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AiurObserver
{
    public class AsyncObservable<T> : IAsyncObservable<T>
    {
        private readonly List<IAsyncObserver<T>> _observers = new List<IAsyncObserver<T>>();

        public IDisposable Subscribe(IAsyncObserver<T> observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
            return new Subscription<T>(_observers, observer);
        }

        public IEnumerable<Task> Boradcast(T newEvent)
        {
            return _observers.Select(t => t.OnHappen(newEvent));
        }
    }
}
