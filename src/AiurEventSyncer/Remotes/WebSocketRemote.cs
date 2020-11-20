using AiurEventSyncer.Abstract;
using AiurEventSyncer.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace AiurEventSyncer.Remotes
{
    public class WebSocketRemote<T> : IRemote<T>
    {
        private readonly string _endpointUrl;

        public string Name { get; set; } = "WebSocket Origin Default Name";
        public bool AutoPushToIt { get; set; }
        public Action OnRemoteChanged { get; set; }
        public Commit<T> LocalPointer { get; set; }

        public WebSocketRemote(string endpointUrl, bool autoPush = false)
        {
            _endpointUrl = endpointUrl;
            AutoPushToIt = autoPush;
        }

        public IEnumerable<Commit<T>> DownloadFrom(string localPointerPosition)
        {
            var json = new WebClient().DownloadString($"{_endpointUrl}?method=syncer-pull&{nameof(localPointerPosition)}={localPointerPosition}");
            return JsonSerializer.Deserialize<List<Commit<T>>>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        public string UploadFrom(string startPosition, IEnumerable<Commit<T>> commitsToPush)
        {
            var task = new HttpClient().PostAsync($"{_endpointUrl}?method=syncer-push&{nameof(startPosition)}={startPosition}", JsonContent.Create(commitsToPush));
            var response = task.Result.Content.ReadAsStringAsync().Result;
            return response;
        }
    }
}
