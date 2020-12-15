using AiurEventSyncer.Abstract;
using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes.Models;
using AiurEventSyncer.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AiurEventSyncer.Remotes
{
    public class WebSocketRemote<T> : Remote<T>
    {
        private readonly ClientWebSocket _ws;
        private readonly string _endPoint;

        public WebSocketRemote(string endPoint) : base(true, true)
        {
            _ws = new ClientWebSocket();
            _endPoint = endPoint;
        }

        protected override async Task Upload(List<Commit<T>> commits, string pushPointer)
        {
            if (_ws.State != WebSocketState.Open)
            {
                throw new InvalidOperationException($"[{Name}] Websocket not connected! State: {_ws.State}");
            }
            var model = new PushModel<T> { Commits = commits, Start = PushPointer };
            await _ws.SendObject(model);
        }

        protected override Task<List<Commit<T>>> Download(string pointer)
        {
            throw new InvalidOperationException("You can't manually pull a websocket remote. Because all websocket remotes are updated automatically!");
        }

        protected override async Task PullAndMonitor()
        {
            if (ContextRepository == null)
            {
                throw new ArgumentNullException(nameof(ContextRepository), "Please add this remote to a repository.");
            }
            Console.WriteLine("Preparing websocket connection for: " + this.Name);
            await _ws.ConnectAsync(new Uri(_endPoint + "?start=" + PullPointer), CancellationToken.None);
            Console.WriteLine("[WebSocket Event] Websocket connected! " + this.Name);
            if (_ws.State == WebSocketState.Open)
            {
                var commits = await _ws.GetObject<List<Commit<T>>>();
                if (commits.Any())
                {
                    await PullLock.WaitAsync();
                    await ContextRepository.OnPulled(commits, this);
                    PullLock.Release();
                }
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
                Console.WriteLine($"[{Name}] Now is in monitoring mode.");
                var commits = await _ws.GetObject<List<Commit<T>>>();
                Console.WriteLine($"[{Name}] Got some new websocket data. Telling repo to pull it...");
                if (_ws.State != WebSocketState.Open)
                {
                    Console.WriteLine($"[{Name}] WARNING! Websocket state: {_ws.State} is not connected! Will stop monitoring!");
                    return;
                }
                if (commits.Any())
                {
#warning Might not necessary.
                    await PullLock.WaitAsync();
                    await ContextRepository.OnPulled(commits, this);
                    PullLock.Release();
                }
            }
            throw new InvalidOperationException("Websocket dropped!");
        }

        protected async override Task Disconnect()
        {
            while (_ws.State == WebSocketState.Open)
            {
                await _ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
        }
    }
}
