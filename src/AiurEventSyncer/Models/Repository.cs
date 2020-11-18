using AiurEventSyncer.Abstract;
using AiurEventSyncer.Tools;
using AiurStore.Models;
using AiurStore.Providers.MemoryProvider;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AiurEventSyncer.Models
{
    public class Repository<T>
    {
        public InOutDatabase<Commit<T>> Commits { get; }
        public List<IRemote<T>> Remotes { get; } = new List<IRemote<T>>();
        public Commit<T> Head => Commits.LastOrDefault();
        public Action OnNewCommit { get; set; }
        public Repository() : this(new MemoryAiurStoreDb<Commit<T>>()) { }

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
            OnNewCommit?.Invoke();
            foreach (var remote in Remotes.Where(t => t.AutoPush))
            {
                Push(remote);
            }
        }

        public void RegisterAutoPull(IRemote<T> remote)
        {
            this.Pull(remote);
            remote.OnRemoteChanged += () =>
            {
                this.Pull(remote);
            };
        }

        public void Pull()
        {
            Pull(Remotes.First());
        }

        public void Pull(IRemote<T> remoteRecord)
        {
            var subtraction = remoteRecord.DownloadFrom(remoteRecord.LocalPointer?.Id);
            foreach (var subtract in subtraction)
            {
                var localAfter = Commits.AfterCommitId(remoteRecord.LocalPointer?.Id).FirstOrDefault();
                if (localAfter is not null)
                {
                    if (localAfter.Id != subtract.Id)
                    {
                        Commits.InsertAfterCommitId(remoteRecord.LocalPointer?.Id, subtract);
                    }
                }
                else
                {
                    Commits.Add(subtract);
                }
                remoteRecord.LocalPointer = subtract;
            }
        }

        public void Push()
        {
            Push(Remotes.First());
        }

        public void Push(IRemote<T> remoteRecord)
        {
            var commitsToPush = Commits.AfterCommitId(remoteRecord.LocalPointer?.Id);
            var remotePointer = remoteRecord.UploadFrom(remoteRecord.LocalPointer?.Id, commitsToPush);
            remoteRecord.LocalPointer = Commits.FirstOrDefault(t => t.Id == remotePointer);
        }

        public string OnPushing(string startPosition, IEnumerable<Commit<T>> commitsToPush)
        {
            string firstDiffPoint = null;
            foreach (var commit in commitsToPush)
            {
                var localAfter = Commits.AfterCommitId(startPosition).FirstOrDefault();
                if (localAfter is not null)
                {
                    if (commit.Id != localAfter.Id && Commits.Last().Id != commit.Id)
                    {
                        firstDiffPoint ??= startPosition;
                        Commits.Add(commit);
                    }
                }
                else
                {
                    Commits.Add(commit);
                }
                startPosition = commit.Id;
            }
            return firstDiffPoint ?? startPosition;
        }
    }
}
