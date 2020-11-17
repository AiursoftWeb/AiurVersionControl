using AiurEventSyncer.Abstract;
using AiurEventSyncer.Models;
using System.Collections.Generic;
using System.Linq;

namespace AiurEventSyncer.Remotes
{
    public class ObjectRemote<T> : IRemote<T>
    {
        private readonly Repository<T> _localRepository;

        public ObjectRemote(Repository<T> localRepository)
        {
            _localRepository = localRepository;
        }

        public Commit<T> LocalPointerPosition { get; set; }

        public IEnumerable<Commit<T>> DownloadFrom(string sourcePointerPosition)
        {
            var yielding = string.IsNullOrWhiteSpace(sourcePointerPosition);
            foreach (var commit in _localRepository.Commits)
            {
                if (yielding)
                {
                    yield return commit;
                }
                if (commit.Id == sourcePointerPosition)
                {
                    yielding = true;
                }
            }
        }

        public string GetRemotePointerPositionId()
        {
            return _localRepository.Commits.Last().Id;
        }
    }
}
