using AiurStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiurStore.Providers.MemoryProvider
{
    public class MemoryAiurStoreDb<T> : InOutDatabase<T>
    {
        protected override void OnConfiguring(InOutDbOptions options)
            => options.UseMemoryStore();
    }
}
