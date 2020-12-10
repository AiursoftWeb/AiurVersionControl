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

        public Func<string, Task> OnRemoteChanged { get; set; }
        public string Position { get; set; }

        public ObjectRemote(Repository<T> localRepository, bool autoPush = false, bool autoPull = false)
        {
            _fakeRemoteRepository = localRepository;
            _fakeRemoteRepository.OnNewCommit += async (state) =>
            {
                if (OnRemoteChanged != null)
                {
                    await OnRemoteChanged(state);
                }
            };
            AutoPush = autoPush;
            AutoPull = autoPull;
        }

        public Task<IReadOnlyList<Commit<T>>> DownloadFromAsync(string localPointerPosition)
        {
            var downloadResult = _fakeRemoteRepository.Commits.AfterCommitId(localPointerPosition).ToList().AsReadOnly() as IReadOnlyList<Commit<T>>;
            return Task.FromResult(downloadResult);
        }

        public async Task UploadFromAsync(string startPosition, IReadOnlyList<Commit<T>> commitsToPush, string state)
        {
            await _fakeRemoteRepository.OnPushed(this, startPosition, commitsToPush, state);
        }
    }
}
