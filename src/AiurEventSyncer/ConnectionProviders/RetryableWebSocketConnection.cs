using AiurEventSyncer.Abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace AiurEventSyncer.ConnectionProviders
{
    public class RetryableWebSocketConnection<T> : WebSocketConnection<T>, IConnectionProvider<T>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsConnectionHealthy { get; set; }
        public bool StopRetrying { get; set; }
        public int AttemptCount { get; set; }


        public RetryableWebSocketConnection(string endpoint) : base(endpoint)
        {

        }

        public override async Task PullAndMonitor(Func<List<Commit<T>>, Task> onData, string startPosition, Func<Task> onConnected, bool monitorInCurrentThread)
        {
            var waitForQuitSignal = Task.Run(() => 
            {
                while (!StopRetrying);
            });
            var retryGapSeconds = 1;
            var connectedTime = DateTime.MinValue;
            while (!StopRetrying)
            {
                try
                {
                    connectedTime = DateTime.UtcNow;

                    AttemptCount++;
                    IsConnectionHealthy = true;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AttemptCount)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnectionHealthy)));

                    await base.PullAndMonitor(onData, startPosition, onConnected, monitorInCurrentThread);
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
                    await Task.WhenAny(Task.Delay(TimeSpan.FromSeconds(retryGapSeconds)), waitForQuitSignal);
                    if (retryGapSeconds < 128)
                    {
                        retryGapSeconds *= 2;
                    }
                }
            }
        }

        public override Task Disconnect()
        {
            StopRetrying = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StopRetrying)));
            return base.Disconnect();
        }
    }
}
