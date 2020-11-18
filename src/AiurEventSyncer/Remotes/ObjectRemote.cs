using AiurEventSyncer.Abstract;
using AiurEventSyncer.Models;
using AiurEventSyncer.Tools;
using System;
using System.Collections.Generic;

namespace AiurEventSyncer.Remotes
{
    public class ObjectRemote<T> : IRemote<T>
    {
        private readonly Repository<T> _localRepository;
        public string Name { get; set; } = "Object Origin Default Name";
        public bool AutoPushToIt { get; set; }

        public Action OnRemoteChanged { get; set; }
        public Commit<T> LocalPointer { get; set; }

        public ObjectRemote(Repository<T> localRepository)
        {
            _localRepository = localRepository;
            _localRepository.OnNewCommit += () =>
            {
                OnRemoteChanged?.Invoke();
            };
        }

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
