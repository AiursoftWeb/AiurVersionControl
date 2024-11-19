using Aiursoft.AiurEventSyncer.ConnectionProviders;
using Aiursoft.AiurVersionControl.Models;

namespace Aiursoft.AiurVersionControl.Remotes
{
    public class WebSocketRemoteWithWorkSpace<T>(string endPoint)
        : RemoteWithWorkSpace<T>(new WebSocketConnection<IModification<T>>(endPoint), true, true)
        where T : WorkSpace, new()
    {
        public string EndPoint { get; private set; } = endPoint;
    }
}
