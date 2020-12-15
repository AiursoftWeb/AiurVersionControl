using AiurEventSyncer.Abstract;
using AiurEventSyncer.Models;
using AiurEventSyncer.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiurEventSyncer.Remotes
{
    public class ObjectRemote<T> : Remote<T>
    {
        private Repository<T> _fakeRemoteRepository;

        public ObjectRemote(Repository<T> localRepository, bool autoPush = false, bool autoPull = false) : base(autoPush, autoPull)
        {
            _fakeRemoteRepository = localRepository;
        }

        protected override Task Upload(List<Commit<T>> commits, string pushPointer)
        {
            return _fakeRemoteRepository.OnPushed(commits, PushPointer);
        }

        protected override Task<List<Commit<T>>> Download(string pointer)
        {
            return Task.FromResult(_fakeRemoteRepository.Commits.AfterCommitId(PullPointer).ToList());
        }

        protected async override Task PullAndMonitor()
        {
            await Pull();
            _fakeRemoteRepository.OnNewCommitsSubscribers[_key] = async (c) =>
            {
#warning Might not necessary.
                await PullLock.WaitAsync();
                await ContextRepository.OnPulled(c.ToList(), this);
                PullLock.Release();
            };
        }

        protected override Task Disconnect()
        {
            _fakeRemoteRepository.OnNewCommitsSubscribers.TryRemove(_key, out _);
            return Task.CompletedTask;
        }
    }
}
