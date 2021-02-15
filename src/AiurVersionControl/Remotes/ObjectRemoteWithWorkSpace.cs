using AiurEventSyncer.ConnectionProviders;
using AiurEventSyncer.Models;
using AiurVersionControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiurVersionControl.Remotes
{
    public class ObjectRemoteWithWorkSpace<T> : RemoteWithWorkSpace<T> where T : WorkSpace, new()
    {
        public ObjectRemoteWithWorkSpace(ControlledRepository<T> localRepository, bool autoPush = false, bool autoPull = false)
            : base(new FakeConnection<IModification<T>>(localRepository), autoPush, autoPull)
        {

        }
    }
}
