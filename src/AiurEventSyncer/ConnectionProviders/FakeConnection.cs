using AiurEventSyncer.Abstract;
using AiurEventSyncer.Models;
using AiurEventSyncer.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AiurEventSyncer.ConnectionProviders
{
    public class FakeConnection<T> : IConnectionProvider<T>
    {
        private readonly Repository<T> _fakeRemoteRepository;
        private readonly Guid _id = Guid.NewGuid();

        public FakeConnection(Repository<T> localRepository)
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
            _fakeRemoteRepository.RegisterAsyncTask(_id, onData);
        }

        public Task Disconnect()
        {
            _fakeRemoteRepository.UnRegister(_id);
            return Task.CompletedTask;
        }
    }
}
