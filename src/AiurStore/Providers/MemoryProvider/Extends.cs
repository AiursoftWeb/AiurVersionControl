using AiurStore.Models;

namespace AiurStore.Providers.MemoryProvider
{
    public static class Extends
    {
        public static void UseMemoryStore<T>(this InOutDbOptions<T> options)
        {
            options.Provider = new MemoryStoreProvider<T>();
        }
    }
}
