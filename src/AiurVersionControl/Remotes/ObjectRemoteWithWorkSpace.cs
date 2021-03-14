using AiurEventSyncer.ConnectionProviders;
using AiurVersionControl.Models;

namespace AiurVersionControl.Remotes
{
    public class ObjectRemoteWithWorkSpace<T> : RemoteWithWorkSpace<T> where T : WorkSpace, new()
    {
        public ObjectRemoteWithWorkSpace(ControlledRepository<T> localRepository, bool autoPush = false, bool autoPull = false)
            : base(new FakeConnection<IModification<T>>(localRepository), autoPush, autoPull)
        {

        }
    }

    public class WebSocketRemoteWithWorkSpace<T> : RemoteWithWorkSpace<T> where T : WorkSpace, new()
    {
        public string EndPoint { get; private set; }
        public WebSocketRemoteWithWorkSpace(string endPoint) : base(new WebSocketConnection<IModification<T>>(endPoint), true, true)
        {
            EndPoint = endPoint;
        }
    }
}
