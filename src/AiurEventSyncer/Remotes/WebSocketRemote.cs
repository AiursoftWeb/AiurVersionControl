using AiurEventSyncer.ConnectionProviders;
using AiurEventSyncer.Models;
using AiurEventSyncer.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

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
