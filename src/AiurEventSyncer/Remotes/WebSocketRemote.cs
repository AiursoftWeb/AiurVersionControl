using AiurEventSyncer.Abstract;
using AiurEventSyncer.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace AiurEventSyncer.Remotes
{
    public class WebSocketRemote<T> : IRemote<T>
    {
        private readonly string _endpointUrl;

        public string Name { get; set; } = "WebSocket Origin Default Name";
        public bool AutoPushToIt { get; set; }
        public Func<Task> OnRemoteChanged { get; set; }
        public Commit<T> LocalPointer { get; set; }

        public WebSocketRemote(string endpointUrl, bool autoPush = false)
        {
            _endpointUrl = endpointUrl;
            AutoPushToIt = autoPush;
        }

        public async Task<IReadOnlyList<Commit<T>>> DownloadFromAsync(string localPointerPosition)
        {
            var client = new HttpClient();
            var json = await client.GetStringAsync($"{_endpointUrl}?method=syncer-pull&{nameof(localPointerPosition)}={localPointerPosition}");
            return JsonSerializer.Deserialize<List<Commit<T>>>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        public async Task<string> UploadFromAsync(string startPosition, IReadOnlyList<Commit<T>> commitsToPush)
        {
            var client = new HttpClient();
            var result = await client.PostAsync($"{_endpointUrl}?method=syncer-push&{nameof(startPosition)}={startPosition}", JsonContent.Create(commitsToPush));
            var response = await result.Content.ReadAsStringAsync();
            return response;
        }
    }
}
