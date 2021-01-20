using AiurEventSyncer.ConnectionProviders;
using AiurEventSyncer.Models;

namespace AiurEventSyncer.Remotes
{
    public class WebSocketRemote<T> : Remote<T>
    {
        public WebSocketRemote(string endPoint) 
            : base(new WebSocketConnection<T>(endPoint), true, true)
        {
        }

    }
}
