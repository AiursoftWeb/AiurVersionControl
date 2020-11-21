using AiurEventSyncer.Abstract;
using AiurEventSyncer.Models;
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
        private readonly string _endpointUrl;
        private readonly string _wsEndpointUrl;
        private readonly SemaphoreSlim readLock = new SemaphoreSlim(1, 1);

        public string Name { get; set; } = "WebSocket Origin Default Name";
        public bool AutoPush { get; set; }
        public bool AutoPull { get; set; }
        public Func<string, Task> OnRemoteChanged { get; set; }
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
            Console.WriteLine("Preparing websocket connection for: " + this.Name);
            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(new Uri(_wsEndpointUrl), CancellationToken.None);
            Console.WriteLine("Websocket connected! " + this.Name);
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
                        Console.WriteLine($"[WebSocket Event] Remote '{Name}' repo changed!");
                        string message = Encoding.UTF8.GetString(
                            buffer.Skip(buffer.Offset).Take(buffer.Count).ToArray())
                            .Trim('\0')
                            .Trim();
                        await OnRemoteChanged(message);
                    }
                }
            }
        }

        public async Task<IReadOnlyList<Commit<T>>> DownloadFromAsync(string localPointerPosition)
        {
            var client = new HttpClient();
            var json = await client.GetStringAsync($"{_endpointUrl}?method=syncer-pull&{nameof(localPointerPosition)}={localPointerPosition}");
            var result = JsonSerializer.Deserialize<List<Commit<T>>>(json, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            foreach (var commit in result)
            {
                Console.WriteLine($"Downloaded a new commit from remote '{Name}': " + commit.Item.ToString());
            }
            if (!result.Any())
            {
                Console.WriteLine("[WARNING] Downloaded nothing!");
            }
            return result;
        }

        public async Task<string> UploadFromAsync(string startPosition, IReadOnlyList<Commit<T>> commitsToPush, string state)
        {
            await readLock.WaitAsync();
            try
            {
                foreach (var commit in commitsToPush)
                {
                    Console.WriteLine("Uploading new commit: " + commit.Item.ToString());
                }
                if (!commitsToPush.Any())
                {
                    Console.WriteLine("[WARNING] Uploaded nothing!");
                }
                var client = new HttpClient();
                var result = await client.PostAsync($"{_endpointUrl}?method=syncer-push&{nameof(startPosition)}={startPosition}&state={state}", JsonContent.Create(commitsToPush));
                var response = await result.Content.ReadAsStringAsync();
                return response;
            }
            finally
            {
                readLock.Release();
            }

        }
    }
}
