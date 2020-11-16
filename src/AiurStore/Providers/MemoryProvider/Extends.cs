using AiurStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
