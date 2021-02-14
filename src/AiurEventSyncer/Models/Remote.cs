using AiurEventSyncer.Abstract;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AiurEventSyncer.Models
{
    public abstract class Remote<T> : IRemote<T>
    {
        public string Name { get; init; } = "remote name";
        public bool AutoPush { get; init; }
        public bool AutoPull { get; init; }
        public Commit<T> PullPointer { get; set; }
        public Commit<T> PushPointer { get; set; }
        protected SemaphoreSlim PushLock { get; } = new SemaphoreSlim(1);
        protected SemaphoreSlim PullLock { get; } = new SemaphoreSlim(1);
        protected IRepository<T> ContextRepository { get; set; }
        private IConnectionProvider<T> ConnectionProvider { get; set; }

        public Remote(
            IConnectionProvider<T> provider,
            bool autoPush = false, 
            bool autoPull = false)
        {
            ConnectionProvider = provider;
            AutoPush = autoPush;
            AutoPull = autoPull;
        }

        public async Task<Remote<T>> AttachAsync(IRepository<T> target)
        {
            if (ContextRepository != null)
            {
                throw new InvalidOperationException("You can't attach a remote to more than one repository. Consider creating a new remote!");
            }
            ContextRepository = target;
            if (AutoPush)
            {
                ContextRepository.RegisterAsyncTask(Guid.NewGuid(), async (c) => await PushAsync());
            }
            if (AutoPull)
            {
                await ConnectionProvider.PullAndMonitor(onData: async data => 
                {
                    await PullLock.WaitAsync();
                    var inserted = ContextRepository.OnPulled(data.ToList(), this);
                    PullComplete(inserted);
                    PullLock.Release();
                }, PullPointer?.Id);
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
            ContextRepository = null;
        }

        public async Task PushAsync()
        {
            await PushLock.WaitAsync();
            var commitsToPush = ContextRepository.Commits.GetAllAfter(PushPointer).ToList();
            if (commitsToPush.Any())
            {
                await ConnectionProvider.Upload(commitsToPush, PushPointer?.Id);
                PushPointer = commitsToPush.Last();
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
                var inserted = ContextRepository.OnPulled(downloadResult, this);
                PullComplete(inserted);
            }
            PullLock.Release();
        }

        public async Task StopMonitoring()
        {
            await PushLock.WaitAsync();
            await ConnectionProvider.Disconnect();
            PushLock.Release();
        }

        public virtual void OnPullPointerMoved(Commit<T> pointer)
        {
        }

        public virtual void PullComplete(bool inserted)
        {
        }
    }
}
