using AiurEventSyncer.Abstract;
using AiurEventSyncer.Models;
using AiurEventSyncer.Tools;
using System.Collections.Generic;

namespace AiurEventSyncer.Remotes
{
    public class ObjectRemote<T> : IRemote<T>
    {
        private readonly Repository<T> _localRepository;

        public ObjectRemote(Repository<T> localRepository)
        {
            _localRepository = localRepository;
        }

        public Commit<T> LocalPointer { get; set; }

        public IEnumerable<Commit<T>> DownloadFrom(string sourcePointerPosition)
        {
            return _localRepository.Commits.AfterCommitId(sourcePointerPosition);
        }

        public string UploadFrom(string startPosition, IEnumerable<Commit<T>> commitsToPush)
        {
            return _localRepository.OnPushing(startPosition, commitsToPush);
        }
    }
}
