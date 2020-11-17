using AiurStore.Models;
using AiurStore.Providers.MemoryProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiurEventSyncer.Tests.TestDbs
{
    public class MemoryTestDb : InOutDatabase<Commit<int>>
    {
        protected override void OnConfiguring(InOutDbOptions options)
            => options.UseMemoryStore();
    }
}
