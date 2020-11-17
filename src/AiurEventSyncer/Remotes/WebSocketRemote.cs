using AiurEventSyncer.Abstract;
using AiurEventSyncer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiurEventSyncer.Remotes
{
    public class WebSocketRemote<T> : IRemote<T>
    {
        private readonly string _endpointUrl;

        public WebSocketRemote(string endpointUrl)
        {
            _endpointUrl = endpointUrl;
        }

        public Commit<T> LocalPointerPosition { get; set; }

        public IEnumerable<Commit<T>> DownloadFrom(string localPointerPosition)
        {
            throw new NotImplementedException();
        }

        public string GetRemotePointerPositionId()
        {
            throw new NotImplementedException();
        }
    }
}
