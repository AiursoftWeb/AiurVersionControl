namespace AiurObserver
{
    internal class AsyncObserver<T> : IAsyncObserver<T>
    {
        public Func<T, Task> OnTrigger { get; }

        internal AsyncObserver(Func<T, Task> onTrigger)
        {
            OnTrigger = onTrigger;
        }
    }
}
