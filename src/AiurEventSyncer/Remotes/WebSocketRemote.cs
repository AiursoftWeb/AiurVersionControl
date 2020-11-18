using AiurEventSyncer.Abstract;
using AiurEventSyncer.Models;
using System;
using System.Collections.Generic;

namespace AiurEventSyncer.Remotes
{
    public class WebSocketRemote<T> : IRemote<T>
    {
        private readonly string _endpointUrl;

        public WebSocketRemote(string endpointUrl)
        {
            _endpointUrl = endpointUrl;
        }

        public Commit<T> LocalPointer { get; set; }

        public IEnumerable<Commit<T>> DownloadFrom(string localPointerPosition)
        {
            throw new NotImplementedException();
        }

        public void UploadFrom(string startPosition, IEnumerable<Commit<T>> commitsToPush)
        {
            throw new NotImplementedException();
        }
    }
}
