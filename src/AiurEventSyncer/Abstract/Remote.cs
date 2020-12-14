using AiurEventSyncer.Models;
using AiurEventSyncer.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AiurEventSyncer.Abstract
{
    public abstract class Remote<T>
    {
        public string Name { get; init; } = "remote name";
        public bool AutoPush { get; init; }
        public bool AutoPull { get; init; }
        public string PullPointer { get; set; }
        public string PushPointer { get; set; }
        protected SemaphoreSlim PushLock { get; } = new SemaphoreSlim(1);
        protected SemaphoreSlim PullLock { get; } = new SemaphoreSlim(1);
        public Repository<T> ContextRepository { get; set; }
#warning Avoid datetime!
        protected readonly DateTime _key = DateTime.UtcNow;


        public Remote(bool autoPush = false, bool autoPull = false)
        {
            AutoPush = autoPush;
            AutoPull = autoPull;
        }

        public async Task Push()
        {
            await PushLock.WaitAsync();
            var commitsToPush = ContextRepository.Commits.AfterCommitId(PushPointer).ToList();
            if (commitsToPush.Any())
            {
                Console.WriteLine($"[{Name}] Pushing remote: {Name}... Pushing content: {string.Join(',', commitsToPush.Select(t => t.Item.ToString()))}");
                await Upload(commitsToPush, PushPointer);
                Console.WriteLine($"[{Name}] Push remote '{Name}' completed.");
                PushPointer = commitsToPush.Last().Id;
            }
            PushLock.Release();
        }

        public async Task Pull()
        {
            if (ContextRepository == null)
            {
                throw new ArgumentNullException(nameof(ContextRepository), "Please add this remote to a repository.");
            }
            await PullLock.WaitAsync();
            Console.WriteLine($"[{Name}] Active Pulling!");
            var downloadResult = await Download(PullPointer);
            if (downloadResult.Any())
            {
                await ContextRepository.OnPulled(downloadResult, this);
            }
            PullLock.Release();
        }

        public async Task BeAdded()
        {
            if (AutoPush)
            {
                ContextRepository.OnNewCommitsSubscribers[DateTime.UtcNow] = async (c) => await Push();
            }
            if (AutoPull)
            {
                await PullAndMonitor();
            }
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
