using AiurStore.Models;

namespace AiurStore.Providers.FileProvider
{
    public static class Extends
    {
        public static void UseFileStore(this InOutDbOptions options, string path)
        {
            options.Provider = new FileStoreProvider(path);
        }
    }
}
