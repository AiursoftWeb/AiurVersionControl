using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes.Models;
using AiurEventSyncer.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace AiurEventSyncer.Remotes
{
    public class WebSocketRemote<T> : Remote<T>
    {
        private readonly string _endPoint;
        private ClientWebSocket _ws;

        public WebSocketRemote(string endPoint) : base(true, true)
        {
            _endPoint = endPoint;
            _ws = new ClientWebSocket();
        }

        protected override async Task Upload(List<Commit<T>> commits)
        {
            if (_ws.State != WebSocketState.Open)
            {
                throw new InvalidOperationException($"[{Name}] Websocket not connected! State: {_ws.State}");
            }
            var model = new PushModel<T> { Commits = commits, Start = PushPointer?.Id };
            await _ws.SendObject(model);
        }

        protected override Task<List<Commit<T>>> Download(string pointer)
        {
            throw new InvalidOperationException($"You can't manually pull a websocket remote: '{Name}'. Because all websocket remotes are updated automatically!");
        }

        protected override async Task PullAndMonitor()
        {
            if (ContextRepository == null)
            {
                throw new ArgumentNullException(nameof(ContextRepository), "Please add this remote to a repository.");
            }
            await _ws.ConnectAsync(new Uri(_endPoint + "?start=" + PullPointer?.Id), CancellationToken.None);
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
                var commits = await _ws.GetObject<List<Commit<T>>>();
                if (_ws.State != WebSocketState.Open)
                {
                    return;
                }
                if (commits.Any())
                {
                    // Don't need to lock the pull lock here. Because websocket remote is never pulled.
                    await ContextRepository.OnPulled(commits, this);
                }
            }
            throw new InvalidOperationException($"[{Name}] Websocket dropped!");
        }

        protected async override Task Disconnect()
        {
            while (_ws.State == WebSocketState.Open)
            {
                await _ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
            _ws.Dispose();
            _ws = new ClientWebSocket();
        }
    }
}
