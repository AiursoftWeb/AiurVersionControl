using System;
using System.Threading.Tasks;

namespace AiurObserver
{
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
}
