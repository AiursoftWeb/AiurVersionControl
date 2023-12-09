namespace AiurObserver
{
    public interface IAsyncObserver<in T>
    {
        public Func<T, Task> OnTrigger { get; }
    }
}
