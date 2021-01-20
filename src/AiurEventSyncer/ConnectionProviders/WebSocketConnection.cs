using AiurEventSyncer.Abstract;
using AiurEventSyncer.ConnectionProviders.Models;
using AiurEventSyncer.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace AiurEventSyncer.ConnectionProviders
{
    public class WebSocketConnection<T> : IConnectionProvider<T>
    {
        private readonly string _endPoint;
        private ClientWebSocket _ws;

        public WebSocketConnection(string endPoint)
        {
            _endPoint = endPoint;
            _ws = new ClientWebSocket();
        }

        public async Task Upload(List<Commit<T>> commits, string pointerId)
        {
            if (_ws.State != WebSocketState.Open)
            {
                throw new InvalidOperationException($"Websocket not connected! State: {_ws.State}");
            }
            var model = new PushModel<T> { Commits = commits, Start = pointerId };
            await _ws.SendObject(model);
        }

        public Task<List<Commit<T>>> Download(string pointer)
        {
            throw new InvalidOperationException($"You can't manually pull a websocket remote. Because all websocket remotes are updated automatically!");
        }

        public async Task PullAndMonitor(Func<List<Commit<T>>, Task> onData, string startPosition)
        {
            await _ws.ConnectAsync(new Uri(_endPoint + "?start=" + startPosition), CancellationToken.None);
            if (_ws.State == WebSocketState.Open)
            {
                var commits = await _ws.GetObject<List<Commit<T>>>();
                if (commits.Any())
                {
                    await onData(commits);
                }
                await Task.Factory.StartNew(() => Monitor(onData));
            }
            else
            {
                throw new InvalidOperationException("Websocket remote not correctly created!");
            }
        }

        private async Task Monitor(Func<List<Commit<T>>, Task> onData)
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
                    await onData(commits);
                }
            }
            throw new InvalidOperationException($"Websocket dropped!");
        }

        public async Task Disconnect()
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
