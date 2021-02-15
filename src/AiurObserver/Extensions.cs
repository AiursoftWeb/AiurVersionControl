using System;
using System.Threading.Tasks;

namespace AiurObserver
{
    public static class Extensions
    {
        public static IDisposable Subscribe<T>(this IAsyncObservable<T> source, Func<T, Task> onHappen)
        {
            return source.Subscribe(new AsyncObserver<T>(onHappen));
        }
    }
}
