using System;
using System.Collections.Generic;

namespace AiurObserver.Models
{
    public class Observable<T> : IObservable<T>
    {
        private readonly List<IObserver<T>> _observers = new List<IObserver<T>>();

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
            return new Subscription<T>(_observers, observer);
        }

        public void Boradcast(T newEvent)
        {
            _observers.ForEach(t => t.OnNext(newEvent));
        }
    }
}
