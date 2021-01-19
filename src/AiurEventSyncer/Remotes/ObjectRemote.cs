using AiurEventSyncer.ConnectionProviders;
using AiurEventSyncer.Models;
using AiurEventSyncer.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AiurEventSyncer.Remotes
{
    public class ObjectRemote<T> : Remote<T>
    {
        public ObjectRemote(Repository<T> localRepository, bool autoPush = false, bool autoPull = false)
            :base (new FakeConnection<T>(localRepository), autoPush, autoPull)
        {

        }
    }
}
