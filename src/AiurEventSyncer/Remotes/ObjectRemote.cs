using AiurEventSyncer.Abstract;
using AiurEventSyncer.Models;
using AiurEventSyncer.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AiurEventSyncer.Remotes
{
    public class ObjectRemote<T> : IRemote<T>
    {
        private readonly Repository<T> _fakeRemoteRepository;
        public string Name { get; init; } = "Object Origin Default Name";
        public bool AutoPush { get; init; }
        public bool AutoPull { get; init; }
        public string PullPointer { get; set; }
        public string PushPointer { get; set; }
        public SemaphoreSlim PushLock { get; } = new SemaphoreSlim(1);
        public SemaphoreSlim PullLock { get; } = new SemaphoreSlim(1);
        public Repository<T> ContextRepository { get; set; }
        private readonly DateTime _key = DateTime.UtcNow;

        public ObjectRemote(Repository<T> localRepository, bool autoPush = false, bool autoPull = false)
        {
            _fakeRemoteRepository = localRepository;
            AutoPush = autoPush;
            AutoPull = autoPull;
        }

        public Task Upload(List<Commit<T>> commitsToPush)
        {
            if (commitsToPush.Any())
            {
                return _fakeRemoteRepository.OnPushed(commitsToPush, PushPointer?.ToString());
            }
            return Task.CompletedTask;
        }

        public async Task DownloadAndPull()
        {
            await PullLock.WaitAsync();
            if (ContextRepository == null)
            {
                throw new ArgumentNullException(nameof(ContextRepository), "Please add this remote to a repository.");
            }
            var downloadResult = _fakeRemoteRepository.Commits.AfterCommitId(PullPointer).ToList();
            await ContextRepository.OnPulled(downloadResult, this);
            PullLock.Release();
        }

        public async Task PullAndStartMonitoring()
        {
            if (AutoPull)
            {
                await ContextRepository.PullAsync(this);
                _fakeRemoteRepository.OnNewCommitsSubscribers[_key] = async (c) =>
                {
                    await ContextRepository.PullAsync(this);
                };
            }
        }

        public Task Unregister()
        {
            _fakeRemoteRepository.OnNewCommitsSubscribers.TryRemove(_key, out _);
            return Task.CompletedTask;
        }
    }
}
