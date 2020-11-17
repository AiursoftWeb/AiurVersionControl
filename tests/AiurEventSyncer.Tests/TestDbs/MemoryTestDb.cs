using AiurEventSyncer.Models;
using AiurStore.Models;
using AiurStore.Providers.MemoryProvider;

namespace AiurEventSyncer.Tests.TestDbs
{
    public class MemoryTestDb : InOutDatabase<Commit<int>>
    {
        protected override void OnConfiguring(InOutDbOptions options)
            => options.UseMemoryStore();
    }
}
