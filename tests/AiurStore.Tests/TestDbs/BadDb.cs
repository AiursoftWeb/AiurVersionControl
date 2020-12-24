using AiurStore.Models;

namespace AiurStore.Tests.TestDbs
{
    public class BadDb : InOutDatabase<string>
    {
        protected override void OnConfiguring(InOutDbOptions<string> options)
        {

        }
    }
}
