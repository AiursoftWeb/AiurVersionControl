using AiurStore.Models;

namespace AiurStore.Providers.FileProvider
{
    public static class Extends
    {
        public static void UseFileStore<T>(this InOutDbOptions<T> options, string path)
        {
            options.Provider = new FileStoreProvider<T>(path);
        }
    }
}
