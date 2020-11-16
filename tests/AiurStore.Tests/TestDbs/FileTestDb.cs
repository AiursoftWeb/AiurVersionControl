using AiurStore.Models;
using AiurStore.Providers.FileProvider;

namespace AiurStore.Tests.TestDbs
{
    public class FileTestDb : InOutDatabase<string>
    {
        protected override void OnConfiguring(InOutDbOptions options)
            => options.UseFileStore("test.txt");
    }
}
