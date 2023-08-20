namespace AiurObserver
{
    public interface IAsyncObserver<in T>
    {
        Task OnHappen(T value);
    }
}
