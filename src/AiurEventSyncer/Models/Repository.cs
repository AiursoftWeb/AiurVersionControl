using AiurEventSyncer.Abstract;
using AiurEventSyncer.Remotes;
using AiurStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AiurEventSyncer.Models
{
    public class Repository<T>
    {
        public InOutDatabase<Commit<T>> Commits { get; }
        public List<IRemote<T>> Remotes { get; } = new List<IRemote<T>>();

        public Repository(InOutDatabase<Commit<T>> dbProvider)
        {
            Commits = dbProvider;
        }

        public void Commit(T content)
        {
            Commits.Add(new Commit<T>
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
                var localAfter = Commits.AfterCommitId(remoteRecord.LocalPointerPosition?.Id).FirstOrDefault();
                if (localAfter != null)
                {
                    if (localAfter.Id == subtract.Id)
                    {
                        // Patch
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }
                else
                {
                    Commits.Add(subtract);
                }
                remoteRecord.LocalPointerPosition = subtract;
            }
        }

        public void Push(IRemote<T> remote)
        {

        }
    }
}
