using System;

namespace AiurObserver
{
    public interface IAsyncObservable<out T>
    {
        IDisposable Subscribe(IAsyncObserver<T> observer);
    }
}
