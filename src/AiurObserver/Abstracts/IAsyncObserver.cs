using System.Threading.Tasks;

namespace AiurObserver
{
    public interface IAsyncObserver<in T>
    {
        Task OnHappen(T value);
    }
}
