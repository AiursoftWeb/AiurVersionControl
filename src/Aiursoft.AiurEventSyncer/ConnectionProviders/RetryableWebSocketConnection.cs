using Aiursoft.AiurEventSyncer.Abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Aiursoft.AiurEventSyncer.ConnectionProviders
{
    public class RetryableWebSocketConnection<T> : WebSocketConnection<T>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public override event Action OnReconnecting;

        public bool IsConnectionHealthy { get; set; }
        public int AttemptCount { get; set; }

        private readonly ManualResetEvent _exitEvent = new(false);

        public RetryableWebSocketConnection(
            string endpoint) : base(endpoint)
        {
        }

        public override async Task PullAndMonitor(Func<List<Commit<T>>, Task> onData, Func<string> startPositionFactory, Func<Task> onConnected, bool monitorInCurrentThread)
        {
            var pullAndMonitorTask = PullAndMonitorInThisThread(onData, startPositionFactory, onConnected);
            if (monitorInCurrentThread)
            {
                await pullAndMonitorTask;
            }
        }

        private async Task PullAndMonitorInThisThread(Func<List<Commit<T>>, Task> onData, Func<string> startPositionFactory, Func<Task> onConnected)
        {
            var exitTask = Task.Run(() => _exitEvent.WaitOne());
            var retryGapSeconds = 1;
            var connectedTime = DateTime.MinValue;
            while (!exitTask.IsCompleted)
            {
                try
                {
                    connectedTime = DateTime.UtcNow;

                    AttemptCount++;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AttemptCount)));

                    await base.PullAndMonitor(onData, startPositionFactory, onConnected: () => 
                    {
                        IsConnectionHealthy = true;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnectionHealthy)));
                        return onConnected();
                    }, true);
                }
                catch (WebSocketException)
                {
                    IsConnectionHealthy = false;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnectionHealthy)));
                    OnReconnecting?.Invoke();
                    if (DateTime.UtcNow - connectedTime > TimeSpan.FromMinutes(1))
                    {
                        // Connection held for one minute. Seems to be a healthy server. Retry soon.
                        retryGapSeconds = 1;
                    }

                    // When retry time finish, or asked to finished.
                    await Task.WhenAny(Task.Delay(TimeSpan.FromSeconds(retryGapSeconds)), exitTask);
                    if (retryGapSeconds < 128)
                    {
                        retryGapSeconds *= 2;
                    }
                }
            }
        }

        public override Task Disconnect()
        {
            _exitEvent.Set();
            return base.Disconnect();
        }
    }
}
