using System;
using System.Collections.Generic;

namespace AiurObserver
{
    public class AsyncObservable<T> : IAsyncObservable<T>
    {
        public readonly List<IAsyncObserver<T>> Observers = new List<IAsyncObserver<T>>();

        public IDisposable Subscribe(IAsyncObserver<T> observer)
        {
            if (!Observers.Contains(observer))
            {
                Observers.Add(observer);
            }
            return new Subscription<T>(Observers, observer);
        }
    }
}
