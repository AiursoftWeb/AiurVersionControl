using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AiurEventSyncer.Abstract
{
    public interface IAsyncObservable<out T>
    {
        IDisposable Subscribe(IAsyncObserver<T> observer);
    }
    public interface IAsyncObserver<in T>
    {
        Task OnHappen(T value);
    }

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

    public class Subscription<T> : IDisposable
    {
        private readonly List<IAsyncObserver<T>> _observers;
        private readonly IAsyncObserver<T> _observer;

        internal Subscription(List<IAsyncObserver<T>> observers, IAsyncObserver<T> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observers.Contains(_observer))
            {
                _observers.Remove(_observer);
            }
        }
    }

    public class AsyncObserver<T> : IAsyncObserver<T>
    {
        private readonly Func<T, Task> onHappen;

        public AsyncObserver(Func<T, Task> onHappen)
        {
            this.onHappen = onHappen;
        }

        public Task OnHappen(T value)
        {
            return onHappen(value);
        }
    }

    public static class Extensions
    {
        public static IDisposable Subscribe<T>(this IAsyncObservable<T> source, Func<T, Task> onHappen)
        {
            return source.Subscribe(new AsyncObserver<T>(onHappen));
        }
    }
}
