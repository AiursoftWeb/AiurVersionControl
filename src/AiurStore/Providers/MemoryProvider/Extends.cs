using AiurStore.Models;

namespace AiurStore.Providers.MemoryProvider
{
    public static class Extends
    {
        public static void UseMemoryStore(this InOutDbOptions options)
        {
            options.Provider = new MemoryStoreProvider();
        }
    }
}
