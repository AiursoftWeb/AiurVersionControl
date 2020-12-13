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
        public string HEAD { get; set; }
        public string PushPointer { get; set; }
        public Repository<T> ContextRepository { get; set; }
        public SemaphoreSlim PushLock { get; } = new SemaphoreSlim(1);

        public WebSocketRemote(string endpointUrl)
        {
            _wsEndpointUrl = endpointUrl;
            _ws = new ClientWebSocket();
            var https = new Regex("^https://", RegexOptions.Compiled);
            var http = new Regex("^http://", RegexOptions.Compiled);
            _wsEndpointUrl = https.Replace(_wsEndpointUrl, "wss://");
            _wsEndpointUrl = http.Replace(_wsEndpointUrl, "ws://");
        }

        public async Task StartPullAndMonitor()
        {
            if (ContextRepository == null)
            {
                throw new ArgumentNullException(nameof(ContextRepository), "Please add this remote to a repository.");
            }
            Console.WriteLine("Preparing websocket connection for: " + this.Name);
            await _ws.ConnectAsync(new Uri(_wsEndpointUrl + "?start=" + HEAD), CancellationToken.None);
            Console.WriteLine("[WebSocket Event] Websocket connected! " + this.Name);
            if (_ws.State == WebSocketState.Open)
            {
                var commits = await _ws.GetObject<List<Commit<T>>>();
                await ContextRepository.OnPulled(commits, this);
                await Task.Factory.StartNew(Monitor);
            }
            else
            {
                throw new InvalidOperationException("Websocket remote not correctly created!");
            }
        }

        private async Task Monitor()
        {
            while (_ws.State == WebSocketState.Open)
            {
                var commits = await _ws.GetObject<List<Commit<T>>>();
                if (_ws.State != WebSocketState.Open)
                {
                    return;
                }
                await ContextRepository.OnPulled(commits, this);
            }
            throw new InvalidOperationException("Websocket dropped!");
        }

        public Task Download()
        {
            throw new InvalidOperationException("You can't manually pull a websocket remote. Because all websocket remotes are updated automatically!");
        }

        public async Task Upload(List<Commit<T>> commitsToPush)
        {
            if (_ws.State != WebSocketState.Open)
            {
                throw new InvalidOperationException($"[{Name}] Websocket not connected! State: {_ws.State}");
            }
            var model = new PushModel<T> { Commits = commitsToPush, Start = PushPointer };
            await _ws.SendObject(model);
        }

        public async Task Unregister()
        {
            while (_ws.State == WebSocketState.Open)
            {
                await _ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
        }
    }

    public class PushModel<T>
    {
        public List<Commit<T>> Commits { get; set; }
        public string Start { get; set; }
    }
}
