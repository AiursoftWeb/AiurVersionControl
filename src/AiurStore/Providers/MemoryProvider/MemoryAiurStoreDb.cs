using AiurStore.Models;

namespace AiurStore.Providers.MemoryProvider
{
    public class MemoryAiurStoreDb<T> : InOutDatabase<T>
    {
        protected override void OnConfiguring(InOutDbOptions options)
            => options.UseMemoryStore();
    }
}
