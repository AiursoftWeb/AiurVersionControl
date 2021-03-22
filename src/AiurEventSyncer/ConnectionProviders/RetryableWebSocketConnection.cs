using AiurEventSyncer.Abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace AiurEventSyncer.ConnectionProviders
{
    public class RetryableWebSocketConnection<T> : WebSocketConnection<T>, IConnectionProvider<T>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsConnectionHealthy { get; set; }
        public int AttemptCount { get; set; }
        private ManualResetEvent exitEvent = new ManualResetEvent(false);

        public RetryableWebSocketConnection(string endpoint) : base(endpoint)
        {

        }

        public override async Task Upload(List<Commit<T>> commits, string pointerId)
        {
            if (_ws?.State != WebSocketState.Open)
            {
                // Surpress error when uploading. Because the remote might not be connected. Retry will help.
                return;
            }
            await base.Upload(commits, pointerId);
        }

        public override async Task PullAndMonitor(Func<List<Commit<T>>, Task> onData, string startPosition, Func<Task> onConnected, bool monitorInCurrentThread)
        {
            var monitorTask = PullAndMonitorInThisThread(onData, startPosition, onConnected);
            if (monitorInCurrentThread)
            {
                await monitorTask;
            }
        }

        private async Task PullAndMonitorInThisThread(Func<List<Commit<T>>, Task> onData, string startPosition, Func<Task> onConnected)
        {
            var exitTask = Task.Run(() => exitEvent.WaitOne());
            var retryGapSeconds = 1;
            var connectedTime = DateTime.MinValue;
            while (!exitTask.IsCompleted)
            {
                try
                {
                    connectedTime = DateTime.UtcNow;

                    AttemptCount++;
                    IsConnectionHealthy = true;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AttemptCount)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnectionHealthy)));

                    await base.PullAndMonitor(onData, startPosition, onConnected, true);
                }
                catch (WebSocketException)
                {
                    IsConnectionHealthy = false;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnectionHealthy)));

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
            exitEvent.Set();
            return base.Disconnect();
        }
    }
}
