using AiurEventSyncer.Abstract;
using AiurEventSyncer.Models;
using AiurEventSyncer.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AiurEventSyncer.Remotes
{
    public class ObjectRemote<T> : IRemote<T>
    {
        private readonly Repository<T> _fakeRemoteRepository;
        public string Name { get; set; } = "Object Origin Default Name";
        public bool AutoPush { get; set; }
        public bool AutoPull { get; set; }
        public string Position { get; set; }
        public Repository<T> ContextRepository { get; set; }

        public ObjectRemote(Repository<T> localRepository, bool autoPush = false, bool autoPull = false)
        {
            _fakeRemoteRepository = localRepository;
            AutoPush = autoPush;
            AutoPull = autoPull;
        }

        public Task Push(IReadOnlyList<Commit<T>> commitsToPush)
        {
            if (commitsToPush.Any())
            {
                return _fakeRemoteRepository.OnPushed(Position, commitsToPush);
            }
            return Task.CompletedTask;
        }

        public async Task Pull()
        {
            if (ContextRepository == null)
            {
                throw new ArgumentNullException(nameof(ContextRepository), "Please add this remote to a repository.");
            }
            var downloadResult = _fakeRemoteRepository.Commits.AfterCommitId(Position).ToList().AsReadOnly() as IReadOnlyList<Commit<T>>;
            await ContextRepository.OnPulled(downloadResult, this);
        }

        public async Task PullAndMonitor()
        {
            if (AutoPull)
            {
                await Pull();
                _fakeRemoteRepository.OnNewCommitSubscribers[DateTime.UtcNow] = async (c) =>
                {
                    await Pull();
                };
            }
            await Task.Delay(int.MaxValue);
        }
    }
}
