using Aiursoft.AiurEventSyncer.Abstract;
using Aiursoft.AiurObserver;
using Aiursoft.AiurEventSyncer.ConnectionProviders.Models;
using Aiursoft.AiurObserver.WebSocket;
using Aiursoft.AiurStore.Tools;

namespace Aiursoft.AiurEventSyncer.ConnectionProviders
{
    public class WebSocketConnection<T>(string endPoint) : IConnectionProvider<T>
    {
        private ObservableWebSocket _ws;
        public virtual event Action OnReconnecting;

        public async Task<bool> Upload(List<Commit<T>> commits)
        {
            if (!_ws.Connected)
            {
                return false;
            }
            var model = new PushModel<T> { Commits = commits };
            await _ws.Send(JsonTools.Serialize(model));
            return true;
        }

        public Task<List<Commit<T>>> Download(string pointer)
        {
            throw new InvalidOperationException("You can't manually pull a websocket remote. Because all websocket remotes are updated automatically!");
        }

        public virtual async Task PullAndMonitor(Func<List<Commit<T>>, Task> onData, Func<string> startPositionFactory, Func<Task> onConnected, bool monitorInCurrentThread)
        {
            _ws = await (endPoint + "?start=" + startPositionFactory()).ConnectAsWebSocketServer();
            await (onConnected?.Invoke() ?? Task.CompletedTask);
            _ws.Subscribe(data =>
            {
                var commits = JsonTools.Deserialize<List<Commit<T>>>(data);
                if (!_ws.Connected)
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
                await _ws.Listen();
            }
            else
            {
                _ = Task.Run(async () => await _ws.Listen());
            }
        }

        public virtual async Task Disconnect()
        {
            // while (_ws?.State == WebSocketState.Open)
            // {
            //     await _ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            // }
            await _ws.Close();
            OnReconnecting?.Invoke();
            //_ws?.Dispose();
        }
    }
}
