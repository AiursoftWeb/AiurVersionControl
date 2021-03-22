using AiurEventSyncer.Abstract;
using AiurEventSyncer.Tools;
using AiurObserver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AiurEventSyncer.ConnectionProviders
{
    public class FakeConnection<T> : IConnectionProvider<T>
    {
        private readonly IRepository<T> _fakeRemoteRepository;
        private IDisposable _subscription;

        public FakeConnection(IRepository<T> localRepository)
        {
            _fakeRemoteRepository = localRepository;
        }

        public Task<bool> Upload(List<Commit<T>> commits, string pointerId)
        {
            _fakeRemoteRepository.OnPushed(commits, pointerId);
            return Task.FromResult(true);
        }

        public Task<List<Commit<T>>> Download(string pointer)
        {
            return Task.FromResult(_fakeRemoteRepository.Commits.GetCommitsAfterId<Commit<T>, T>(pointer).ToList());
        }

        public async Task PullAndMonitor(Func<List<Commit<T>>, Task> onData, Func<string> startPositionFactory, Func<Task> onConnected, bool monitorInCurrentThread)
        {
            var pulledData = await Download(startPositionFactory());
            if (pulledData.Any())
            {
                await onData(pulledData);
            }
            await onConnected();
            _subscription = _fakeRemoteRepository.AppendCommitsHappened.Subscribe(onData);
            if (monitorInCurrentThread)
            {
                await Task.Delay(int.MaxValue);
            }
        }

        public Task Disconnect()
        {
            if (_subscription != null)
            {
                _subscription.Dispose();
            }
            return Task.CompletedTask;
        }
    }
}
