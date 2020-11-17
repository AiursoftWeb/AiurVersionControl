using AiurEventSyncer.Abstract;
using AiurStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiurEventSyncer.Models
{
    public class Repository<T>
    {
        public IEnumerable<Commit<T>> Commits => _commits.Query();
        private InOutDatabase<Commit<T>> _commits { get; }
        public List<IRemote<T>> Remotes { get; } = new List<IRemote<T>>();

        public Repository(InOutDatabase<Commit<T>> dbProvider)
        {
            _commits = dbProvider;
        }

        public void Commit(T content)
        {
            _commits.Insert(new Commit<T>
            {
                Item = content
            });
        }

        public void Pull()
        {
            Pull(Remotes.First());
        }

        public void Pull(IRemote<T> remoteRecord)
        {
            var remotePointerPositionId = remoteRecord.GetRemotePointerPositionId();
            if (remotePointerPositionId == remoteRecord.LocalPointerPosition?.Id)
            {
                // Remote repo unchanged.
                return;
            }
            var subtraction = remoteRecord.DownloadFrom(remoteRecord.LocalPointerPosition?.Id);
            foreach (var subtract in subtraction)
            {
                _commits.Insert(subtract);
                remoteRecord.LocalPointerPosition = subtract;
            }
        }

        public void Push(IRemote<T> remote)
        {

        }
    }
}
