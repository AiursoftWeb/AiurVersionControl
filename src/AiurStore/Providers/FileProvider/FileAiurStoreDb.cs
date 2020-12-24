using AiurStore.Models;

namespace AiurStore.Providers.FileProvider
{
    public class FileAiurStoreDb<T> : InOutDatabase<T>
    {
        protected override void OnConfiguring(InOutDbOptions<T> options)
            => options.UseFileStore("aiur-store.txt");
    }
}
