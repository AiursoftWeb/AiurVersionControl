using AiurEventSyncer.Abstract;
using AiurEventSyncer.Models;
using AiurEventSyncer.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AiurEventSyncer.Remotes
{
    public class WebSocketRemote<T> : IRemote<T>
    {
        private readonly string _wsEndpointUrl;
        private readonly ClientWebSocket _ws;

        public string Name { get; init; } = "WebSocket Origin Default Name";
        public bool AutoPush => true;
        public string Position { get; set; }
        public Repository<T> ContextRepository { get; set; }

        public WebSocketRemote(string endpointUrl)
        {
            _wsEndpointUrl = endpointUrl;
            _ws = new ClientWebSocket();
            var https = new Regex("^https://", RegexOptions.Compiled);
            var http = new Regex("^http://", RegexOptions.Compiled);
            _wsEndpointUrl = https.Replace(_wsEndpointUrl, "wss://");
            _wsEndpointUrl = http.Replace(_wsEndpointUrl, "ws://");

            Console.WriteLine("Preparing websocket connection for: " + this.Name);
            _ws.ConnectAsync(new Uri(_wsEndpointUrl + "?start=" + Position), CancellationToken.None).Wait();
            Console.WriteLine("[WebSocket Event] Websocket connected! " + this.Name);
        }

        public async Task PullAndMonitor()
        {
            if (ContextRepository == null)
            {
                throw new ArgumentNullException(nameof(ContextRepository), "Please add this remote to a repository.");
            }
            while (_ws.State == WebSocketState.Open)
            {
                var rawJson = await WebSocketExtends.GetMessage(_ws);
                var commits = JsonSerializer.Deserialize<List<Commit<T>>>(rawJson, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                await ContextRepository.OnPulled(commits, this);
            }
            throw new InvalidOperationException("Websocket dropped!");
        }

        public Task Pull()
        {
            throw new InvalidOperationException("You can't manually pull a websocket remote. Because all websocket remotes are updated automatically!");
        }

        public async Task Push(IReadOnlyList<Commit<T>> commitsToPush)
        {
            if (_ws.State != WebSocketState.Open)
            {
                throw new InvalidOperationException($"[{Name}] Websocket not connected! State: {_ws.State}");
            }
            var rawJson = JsonSerializer.Serialize(commitsToPush.ToList(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            await WebSocketExtends.SendMessage(_ws, rawJson);
        }
    }
}
