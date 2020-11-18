using AiurEventSyncer.Abstract;
using AiurEventSyncer.Models;
using System;
using System.Collections.Generic;

namespace AiurEventSyncer.Remotes
{
    public class WebSocketRemote<T> : IRemote<T>
    {
        private readonly string _endpointUrl;

        public string Name { get; set; } = "WebSocket Origin Default Name";
        public bool AutoPush { get; set; }
        public Action OnRemoteChanged { get; set; }
        public Commit<T> LocalPointer { get; set; }

        public WebSocketRemote(string endpointUrl)
        {
            _endpointUrl = endpointUrl;
        }

        public IEnumerable<Commit<T>> DownloadFrom(string localPointerPosition)
        {
            throw new NotImplementedException();
        }

        public string UploadFrom(string startPosition, IEnumerable<Commit<T>> commitsToPush)
        {
            throw new NotImplementedException();
        }
    }
}
