using Aiursoft.AiurEventSyncer.ConnectionProviders;
using Aiursoft.AiurEventSyncer.Models;

namespace Aiursoft.AiurEventSyncer.Remotes
{
    public class WebSocketRemote<T> : Remote<T>
    {
        public WebSocketRemote(string endPoint, bool autoRetry = false)
            : base(autoRetry ?
                  new RetryableWebSocketConnection<T>(endPoint) :
                  new WebSocketConnection<T>(endPoint), 
                  autoPush: true, 
                  autoPull: true)
        {
        }
    }
}
