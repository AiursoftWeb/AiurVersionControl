using AiurStore.Abstracts;

namespace AiurStore.Models
{
    public class InOutDbOptions<T>
    {
        public IStoreProvider<T> Provider { get; set; }
    }
}
