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

        public IEnumerable<Commit<T>> DownloadFrom(string sourcePointerPosition)
        {
            var yielding = string.IsNullOrWhiteSpace(sourcePointerPosition);
            foreach (var commit in _localRepository.Commits.Query())
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
        public string Id { get; set; } = Guid.NewGuid().ToString("D");
        public T Item { get; set; }
    }

    public class Repository<T>
    {
        public InOutDatabase<Commit<T>> Commits { get; set; }
        public List<IRemote<T>> Remotes { get; set; } = new List<IRemote<T>>();

        public Repository(InOutDatabase<Commit<T>> dbProvider)
        {
            Commits = dbProvider;
        }

        public void Commit(T content)
        {
            Commits.Insert(new Commit<T>
            {
                Item = content
            });
        }

        public void Pull(IRemote<T> remoteRecord)
        {
            var remotePointerPositionId = remoteRecord.GetRemotePointerPositionId();
            if (remotePointerPositionId == remoteRecord.LocalPointerPosition?.Id)
            {
                return;
            }
            if (Commits.Query().Any(c => c.Id == remotePointerPositionId))
            {
                return;
            }

            var subtraction = remoteRecord.DownloadFrom(remoteRecord.LocalPointerPosition?.Id);
            foreach (var subtract in subtraction)
            {
                Commits.Insert(subtract);
                remoteRecord.LocalPointerPosition = subtract;
            }
        }

        public void Push(IRemote<T> remote)
        {

        }
    }
}
