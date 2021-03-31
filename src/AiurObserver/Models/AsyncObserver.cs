using System;
using System.Threading.Tasks;

namespace AiurObserver
{
    public class AsyncObserver<T> : IAsyncObserver<T>
    {
        private readonly Func<T, Task> _onHappen;

        public AsyncObserver(Func<T, Task> onHappen)
        {
            this._onHappen = onHappen;
        }

        public Task OnHappen(T value)
        {
            return _onHappen(value);
        }
    }
}
