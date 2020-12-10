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
        private ClientWebSocket _ws;

        public string Name { get; set; } = "WebSocket Origin Default Name";
        public bool AutoPush { get; set; }
        public string Position { get; set; }

        public WebSocketRemote(string endpointUrl, bool autoPush = false)
        {
            _wsEndpointUrl = endpointUrl;
            var https = new Regex("^https://", RegexOptions.Compiled);
            var http = new Regex("^http://", RegexOptions.Compiled);

            _wsEndpointUrl = https.Replace(_wsEndpointUrl, "wss://");
            _wsEndpointUrl = http.Replace(_wsEndpointUrl, "ws://");
            _ws = new ClientWebSocket();

            AutoPush = autoPush;
        }

        public async Task DownloadAndSaveTo( bool keepAlive, Repository<T> repository)
        {
            Console.WriteLine("Preparing websocket connection for: " + this.Name);
            if (_ws.State == WebSocketState.Open)
            {
                throw new InvalidOperationException("Can't pull because there is a alive pull already monitoring!");
            }
            await _ws.ConnectAsync(new Uri(_wsEndpointUrl + "?start=" + Position), CancellationToken.None);
            Console.WriteLine("[WebSocket Event] Websocket connected! " + this.Name);
            while (_ws.State == WebSocketState.Open)
            {
                var rawJson = await WebSocketExtends.GetMessage(_ws);
                var commits = JsonSerializer.Deserialize<List<Commit<T>>>(rawJson, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                await repository.OnPulled(commits, this);
                if (!keepAlive)
                {
                    await _ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
            }
        }

        public async Task UploadFromAsync(IReadOnlyList<Commit<T>> commitsToPush)
        {
            bool connected = true;
            if (_ws.State != WebSocketState.Open)
            {
                connected = false;
                _ws = new ClientWebSocket();
                await _ws.ConnectAsync(new Uri(_wsEndpointUrl + "?start=" + Position), CancellationToken.None);
            }
            var rawJson = JsonSerializer.Serialize(commitsToPush.ToList(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            await WebSocketExtends.SendMessage(_ws, rawJson);
            if (!connected)
            {
                await _ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
        }
    }
}
