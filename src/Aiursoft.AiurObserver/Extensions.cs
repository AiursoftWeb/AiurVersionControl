namespace AiurObserver
{
    public static class Extensions
    {
        public static ISubscription Subscribe<T>(this IAsyncObservable<T> source, Func<T, Task> onHappen)
        {
            return source.Subscribe(new AsyncObserver<T>(onHappen));
        }
    }
}
