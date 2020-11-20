using AiurEventSyncer.Abstract;
using AiurEventSyncer.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AiurEventSyncer.Remotes
{
    public class WebSocketRemote<T> : IRemote<T>
    {
        private readonly string _endpointUrl;
        private readonly string _wsEndpointUrl;

        public string Name { get; set; } = "WebSocket Origin Default Name";
        public bool AutoPush { get; set; }
        public bool AutoPull { get; set; }
        public Func<Task> OnRemoteChanged { get; set; }
        public Commit<T> LocalPointer { get; set; }

        public WebSocketRemote(string endpointUrl, bool autoPush = false, bool autoPull = false)
        {
            _wsEndpointUrl = _endpointUrl = endpointUrl;
            var https = new Regex("^https://", RegexOptions.Compiled);
            var http = new Regex("^http://", RegexOptions.Compiled);

            _wsEndpointUrl = https.Replace(_wsEndpointUrl, "wss://");
            _wsEndpointUrl = http.Replace(_wsEndpointUrl, "ws://");

            AutoPush = autoPush;
            AutoPull = autoPull;
            if (autoPull)
            {
                Task.Factory.StartNew(MonitorRemoteChanges);
            }
        }

        public async Task MonitorRemoteChanges()
        {
            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(new Uri(_wsEndpointUrl), CancellationToken.None);
            var buffer = new ArraySegment<byte>(new byte[2048]);
            while (true)
            {
                var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
                else
                {
                    if (OnRemoteChanged != null)
                    {
                        await OnRemoteChanged();
                    }
                }
            }
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
