using AiurEventSyncer.Abstract;
using AiurEventSyncer.Tools;
using System;
using System.Collections.Generic;
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
        public string PullPointer { get; set; }
        public string PushPointer { get; set; }
        protected SemaphoreSlim PushLock { get; } = new SemaphoreSlim(1);
        protected SemaphoreSlim PullLock { get; } = new SemaphoreSlim(1);
        protected IRepository<T> ContextRepository { get; set; }

        public Remote(bool autoPush = false, bool autoPull = false)
        {
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
                ContextRepository.Register(Guid.NewGuid(), async (c) => await PushAsync());
            }
            if (AutoPull)
            {
                await PullAndMonitor();
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
            var commitsToPush = ContextRepository.Commits.AfterCommitId(PushPointer).ToList();
            if (commitsToPush.Any())
            {
                await Upload(commitsToPush, PushPointer);
                PushPointer = commitsToPush.Last().Id;
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
            var downloadResult = await Download(PullPointer);
            if (downloadResult.Any())
            {
                await ContextRepository.OnPulled(downloadResult, this);
            }
            PullLock.Release();
        }

        public async Task StopMonitoring()
        {
            await PushLock.WaitAsync();
            await Disconnect();
            PushLock.Release();
        }

        protected abstract Task Disconnect();

        protected abstract Task PullAndMonitor();

        protected abstract Task Upload(List<Commit<T>> commits, string pushPointer);

        protected abstract Task<List<Commit<T>>> Download(string pointer);
    }
}
