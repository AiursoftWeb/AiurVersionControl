using Aiursoft.AiurEventSyncer.ConnectionProviders;
using Aiursoft.AiurVersionControl.Models;

namespace Aiursoft.AiurVersionControl.Remotes
{
    public class WebSocketRemoteWithWorkSpace<T> : RemoteWithWorkSpace<T> where T : WorkSpace, new()
    {
        public string EndPoint { get; private set; }

        public WebSocketRemoteWithWorkSpace(string endPoint, bool autoRetry = false) :
            base(autoRetry
                ? new RetryableWebSocketConnection<IModification<T>>(endPoint)
                : new WebSocketConnection<IModification<T>>(endPoint), true, true)
        {
            EndPoint = endPoint;
        }
    }
}
