using Aiursoft.AiurEventSyncer.Abstract;
using Aiursoft.AiurObserver;
using Aiursoft.AiurObserver.Extensions;

namespace Aiursoft.AiurEventSyncer.Models
{
    public abstract class Remote<T> : IRemote<T>
    {
        public bool AutoPush { get; init; }
        public bool AutoPull { get; init; }
        public Commit<T> PullPointer { get; set; }
        public Commit<T> PushPointer { get; set; }
        protected SemaphoreSlim PushLock { get; } = new(1);
        protected SemaphoreSlim PullLock { get; } = new(1);
        protected IRepository<T> ContextRepository { get; set; }
        protected ISubscription AutoPushsubscription { get; set; }
        public IConnectionProvider<T> ConnectionProvider { get; set; }

        public Remote(
            IConnectionProvider<T> provider,
            bool autoPush = false,
            bool autoPull = false)
        {
            ConnectionProvider = provider;
            ConnectionProvider.OnReconnecting += () => this.PushPointer = this.PullPointer;
            AutoPush = autoPush;
            AutoPull = autoPull;
        }

        public async Task<Remote<T>> AttachAsync(IRepository<T> target, bool monitorInCurrentThread = false)
        {
            if (ContextRepository != null)
            {
                throw new InvalidOperationException("You can't attach a remote to more than one repository. Consider creating a new remote!");
            }
            ContextRepository = target;
            if (AutoPush)
            {
                AutoPushsubscription = ContextRepository.AppendCommitsHappened.Subscribe(_ => PushAsync());
            }
            if (AutoPull)
            {
                await ConnectionProvider.PullAndMonitor(onData: async data =>
                {
                    await PullLock.WaitAsync();
                    ContextRepository.OnPulled(data.ToList(), this);
                    PullLock.Release();
                }, () => PullPointer?.Id, onConnected: () => AutoPush ? PushAsync() : Task.CompletedTask, monitorInCurrentThread);
            }
            return this;
        }

        public async Task DetachAsync()
        {
            if (ContextRepository == null)
            {
                throw new InvalidOperationException("You can't drop the remote because it has no repository attached!");
            }
            await StopMonitoring();
            if (AutoPush)
            {
                AutoPushsubscription.Unsubscribe();
            }
            ContextRepository = null;
        }

        public async Task PushAsync()
        {
            await PushLock.WaitAsync();
            var commitsToPush = ContextRepository.Commits.GetAllAfter(PushPointer).ToList();
            if (commitsToPush.Any())
            {
                var uploaded = await ConnectionProvider.Upload(commitsToPush, PushPointer?.Id);
                if(uploaded)
                {
                    PushPointer = commitsToPush.Last();
                }
            }
            PushLock.Release();
        }

        public async Task PullAsync()
        {
            if (ContextRepository == null)
            {
                throw new ArgumentNullException(nameof(ContextRepository), "Please add this remote to a repository.");
            }
            await PullLock.WaitAsync();
            var downloadResult = await ConnectionProvider.Download(PullPointer?.Id);
            if (downloadResult.Any())
            {
                ContextRepository.OnPulled(downloadResult, this);
            }
            PullLock.Release();
        }

        public async Task StopMonitoring()
        {
            await PushLock.WaitAsync();
            await ConnectionProvider.Disconnect();
            PushLock.Release();
        }

        public virtual void OnPullPointerMovedForwardOnce(Commit<T> pointer)
        {
        }

        public virtual void OnPullInsert()
        {
        }
    }
}
