using AiurStore.Models;

namespace AiurStore.Providers.DbQueryProvider
{
    public class DummyQueryDb<T> : InOutDatabase<T>
    {
        protected override void OnConfiguring(InOutDbOptions options)
        {

        }
    }
}
