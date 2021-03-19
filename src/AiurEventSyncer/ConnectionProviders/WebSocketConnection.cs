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
        private Task monitorTask;
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
            throw new InvalidOperationException("You can't manually pull a websocket remote. Because all websocket remotes are updated automatically!");
        }

        public async Task PullAndMonitor(Func<List<Commit<T>>, Task> onData, string startPosition, Func<Task> onConnected, bool monitorInCurrentThread)
        {
            await _ws.ConnectAsync(new Uri(_endPoint + "?start=" + startPosition), CancellationToken.None);
            await onConnected();
            monitorTask = _ws.Monitor<List<Commit<T>>>(onNewObject: commits =>
            {
                if (_ws.State != WebSocketState.Open)
                {
                    return Task.CompletedTask;
                }
                if (commits.Any())
                {
                    return onData(commits);
                }
                return Task.CompletedTask;
            });
            if (monitorInCurrentThread)
            {
                await monitorTask;
            }
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
