using Aiursoft.AiurEventSyncer.ConnectionProviders;
using Aiursoft.AiurVersionControl.Models;

namespace Aiursoft.AiurVersionControl.Remotes
{
    public class ObjectRemoteWithWorkSpace<T> : RemoteWithWorkSpace<T> where T : WorkSpace, new()
    {
        public ObjectRemoteWithWorkSpace(ControlledRepository<T> localRepository, bool autoPush = false, bool autoPull = false)
            : base(new FakeConnection<IModification<T>>(localRepository), autoPush, autoPull)
        {

        }
    }
}
