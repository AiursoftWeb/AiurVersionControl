using AiurStore.Models;
using AiurStore.Providers.MemoryProvider;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AiurEventSyncer
{
    public interface IRemote<T>
    {
        public Commit<T> LocalPointerPosition { get; set; }

        public string GetRemotePointerPositionId();
        IEnumerable<Commit<T>> DownloadFrom(string localPointerPosition);
    }

    public class LocalRemote<T> : IRemote<T>
    {
        private readonly Repository<T> _localRepository;

        public LocalRemote(Repository<T> localRepository)
        {
            _localRepository = localRepository;
        }

        public Commit<T> LocalPointerPosition { get; set; }

        public IEnumerable<Commit<T>> DownloadFrom(string localPointerPosition)
        {
            var yielding = false;
            foreach (var commit in _localRepository.Commits.Query())
            {
                if (yielding)
                {
                    yield return commit;
                }
                if (commit.Id == localPointerPosition)
                {
                    yielding = true;
                }
            }
        }

        public string GetRemotePointerPositionId()
        {
            return _localRepository.Commits.Query().Last().Id;
        }
    }

    public class WebSocketRemote<T> : IRemote<T>
    {
        private readonly string _endpointUrl;

        public WebSocketRemote(string endpointUrl)
        {
            _endpointUrl = endpointUrl;
        }

        public Commit<T> LocalPointerPosition { get; set; }

        public IEnumerable<Commit<T>> DownloadFrom(string localPointerPosition)
        {
            throw new NotImplementedException();
        }

        public string GetRemotePointerPositionId()
        {
            throw new NotImplementedException();
        }
    }

    public class Commit<T>
    {
        public string Id { get; set; }
        public T Item { get; set; }
    }

    public class Repository<T>
    {
        public InOutDatabase<Commit<T>> Commits { get; set; }
        public List<IRemote<T>> Remotes { get; set; }

        public Repository(InOutDatabase<Commit<T>> dbProvider)
        {
            Commits = dbProvider;
        }

        public void Pull(IRemote<T> remote)
        {
            var remotePointerPositionId = remote.GetRemotePointerPositionId();
            if (remotePointerPositionId == remote.LocalPointerPosition.Id)
            {
                return;
            }
            if (Commits.Query().Any(c => c.Id == remotePointerPositionId))
            {
                return;
            }

            var subtraction = remote.DownloadFrom(remote.LocalPointerPosition.Id);
            foreach (var subtract in subtraction)
            {
                Commits.Insert(subtract);
                remote.LocalPointerPosition = subtract;
            }
        }

        public void Push(IRemote<T> remote)
        {

        }
    }
}
