using AiurObserver;
using AiurEventSyncer.Abstract;
using AiurEventSyncer.Tools;
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

        public Task Upload(List<Commit<T>> commits, string pointerId)
        {
            _fakeRemoteRepository.OnPushed(commits, pointerId);
            return Task.CompletedTask;
        }

        public Task<List<Commit<T>>> Download(string pointer)
        {
            return Task.FromResult(_fakeRemoteRepository.Commits.GetCommitsAfterId<Commit<T>, T>(pointer).ToList());
        }

        public async Task PullAndMonitor(Func<List<Commit<T>>, Task> onData, string startPosition)
        {
            var pulledData = await Download(startPosition);
            await onData(pulledData);
            _subscription = _fakeRemoteRepository.AppendCommitsHappened.Subscribe(onHappen: (commits) => onData(commits));
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
