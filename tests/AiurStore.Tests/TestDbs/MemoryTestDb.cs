using AiurStore.Models;
using AiurStore.Providers.FileProvider;
using AiurStore.Providers.MemoryProvider;

namespace AiurStore.Tests.TestDbs
{
    public class MemoryTestDb : InOutDatabase<string>
    {
        protected override void OnConfiguring(InOutDbOptions options)
            => options.UseMemoryStore();
    }
}
