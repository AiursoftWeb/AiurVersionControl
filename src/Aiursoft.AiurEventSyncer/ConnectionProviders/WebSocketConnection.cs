using Aiursoft.AiurEventSyncer.Abstract;
using Aiursoft.AiurEventSyncer.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Aiursoft.AiurEventSyncer.ConnectionProviders.Models;

namespace Aiursoft.AiurEventSyncer.ConnectionProviders
{
    public class WebSocketConnection<T> : IConnectionProvider<T>
    {
        private readonly string _endPoint;
        private ClientWebSocket _ws;
        public virtual event Action OnReconnecting;

        public WebSocketConnection(string endPoint)
        {
            _endPoint = endPoint;
        }

        public async Task<bool> Upload(List<Commit<T>> commits, string pointerId)
        {
            if (_ws?.State != WebSocketState.Open)
            {
                return false;
            }
            var model = new PushModel<T> { Commits = commits, Start = pointerId };
            await _ws.SendObject(model);
            return true;
        }

        public Task<List<Commit<T>>> Download(string pointer)
        {
            throw new InvalidOperationException("You can't manually pull a websocket remote. Because all websocket remotes are updated automatically!");
        }

        public virtual async Task PullAndMonitor(Func<List<Commit<T>>, Task> onData, Func<string> startPositionFactory, Func<Task> onConnected, bool monitorInCurrentThread)
        {
            _ws = new ClientWebSocket();
            await _ws.ConnectAsync(new Uri(_endPoint + "?start=" + startPositionFactory()), CancellationToken.None);
            await (onConnected?.Invoke() ?? Task.CompletedTask);
            var monitorTask = _ws.Monitor<List<Commit<T>>>(onNewObject: commits =>
            {
                if (_ws.State != WebSocketState.Open)
                {
                    return Task.CompletedTask;
                }
                if (commits.Any())
                {
                    return onData?.Invoke(commits);
                }
                return Task.CompletedTask;
            });
            if (monitorInCurrentThread)
            {
                await monitorTask;
            }
        }

        public virtual async Task Disconnect()
        {
            while (_ws?.State == WebSocketState.Open)
            {
                await _ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
            OnReconnecting?.Invoke();
            _ws?.Dispose();
        }
    }
}
