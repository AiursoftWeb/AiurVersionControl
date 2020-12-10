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
        public string Position { get; set; }

        public ObjectRemote(Repository<T> localRepository, bool autoPush = false)
        {
            _fakeRemoteRepository = localRepository;
            AutoPush = autoPush;
        }

        public Task UploadFromAsync(IReadOnlyList<Commit<T>> commitsToPush)
        {
            if (commitsToPush.Any())
            {
                return _fakeRemoteRepository.OnPushed(Position, commitsToPush);
            }
            return Task.CompletedTask;
        }

        public async Task DownloadAndSaveTo( bool keepAlive, Repository<T> repository)
        {
            var downloadResult = _fakeRemoteRepository.Commits.AfterCommitId(Position).ToList().AsReadOnly() as IReadOnlyList<Commit<T>>;
            await repository.OnPulled(downloadResult, this);
            if (keepAlive)
            {
                _fakeRemoteRepository.OnNewCommit += async (c) =>
                {
                    await repository.OnPulled(new List<Commit<T>>() { c }, this);
                };
                while (true)
                {
                    await Task.Delay(int.MaxValue);
                }
            }
        }
    }
}
