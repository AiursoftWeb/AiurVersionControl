using Aiursoft.AiurEventSyncer.Abstract;
using Aiursoft.AiurEventSyncer.Tools;
using Aiursoft.AiurObserver;

namespace Aiursoft.AiurEventSyncer.ConnectionProviders
{
    public class FakeConnection<T> : IConnectionProvider<T>
    {
        private readonly IRepository<T> _fakeRemoteRepository;
        private ISubscription _subscription;
        public event Action OnReconnecting;

        public FakeConnection(IRepository<T> localRepository)
        {
            _fakeRemoteRepository = localRepository;
        }

        public Task<bool> Upload(List<Commit<T>> commits)
        {
            _fakeRemoteRepository.OnPushed(commits);
            return Task.FromResult(true);
        }

        public Task<List<Commit<T>>> Download(string pointer)
        {
            return Task.FromResult(_fakeRemoteRepository.Commits.GetCommitsAfterId<Commit<T>, T>(pointer).ToList());
        }

        public async Task PullAndMonitor(Func<List<Commit<T>>, Task> onData, Func<string> startPositionFactory, Func<Task> onConnected, bool monitorInCurrentThread)
        {
            _subscription = _fakeRemoteRepository.AppendCommitsHappened.Subscribe(onData);
            var pulledData = await Download(startPositionFactory());
            if (pulledData.Any())
            {
                await onData(pulledData);
            }
            await onConnected();
            if (monitorInCurrentThread)
            {
                await Task.Delay(int.MaxValue);
            }
        }

        public Task Disconnect()
        {
            OnReconnecting?.Invoke();
            _subscription?.Unsubscribe();
            return Task.CompletedTask;
        }
    }
}
