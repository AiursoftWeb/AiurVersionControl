using AiurEventSyncer.Abstract;
using AiurEventSyncer.Tools;
using AiurStore.Models;
using AiurStore.Providers.MemoryProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AiurEventSyncer.Models
{
    public class Repository<T>
    {
        public InOutDatabase<Commit<T>> Commits { get; }
        public IEnumerable<IRemote<T>> Remotes => RemotesStore.AsReadOnly();
        public Commit<T> Head => Commits.LastOrDefault();
        public Func<Task> OnNewCommit { get; set; }
        public Repository() : this(new MemoryAiurStoreDb<Commit<T>>()) { }

        private List<IRemote<T>> RemotesStore = new List<IRemote<T>>();

        public Repository(InOutDatabase<Commit<T>> dbProvider)
        {
            Commits = dbProvider;
        }

        /// <summary>
        /// Add a new commit to this repository.
        /// Also will push to all remotes which are auto push.
        /// Also for all other repositories which auto pull this one, will notify them.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task CommitAsync(T content)
        {
            Commits.Add(new Commit<T>
            {
                Item = content
            });
            await TriggerOnNewCommit();
        }

        private async Task TriggerOnNewCommit()
        {
            if (OnNewCommit != null)
            {
                await OnNewCommit();
            }
            await Task.WhenAll(Remotes.Where(t => t.AutoPush).Select(t => PushAsync(t)));
        }

        /// <summary>
        /// Add a new remote repository to this.
        /// If the remote requires auto pull, it will pull it immediatly, and also register the remote change event.
        /// </summary>
        /// <param name="remote"></param>
        /// <returns></returns>
        public async Task AddRemoteAsync(IRemote<T> remote)
        {
            this.RemotesStore.Add(remote);
            if (remote.AutoPull)
            {
                await this.PullAsync(remote);
                remote.OnRemoteChanged += async () =>
                {
                    await this.PullAsync(remote);
                };
            }
        }

        public Task PullAsync()
        {
            return PullAsync(Remotes.First());
        }

        public async Task PullAsync(IRemote<T> remoteRecord)
        {
            var triggerOnNewCommit = false;
            var subtraction = await remoteRecord.DownloadFromAsync(remoteRecord.LocalPointer?.Id);
            foreach (var subtract in subtraction)
            {
                var localAfter = Commits.AfterCommitId(remoteRecord.LocalPointer?.Id).FirstOrDefault();
                if (localAfter is not null)
                {
                    if (localAfter.Id != subtract.Id)
                    {
                        Commits.InsertAfterCommitId(remoteRecord.LocalPointer?.Id, subtract);
                        triggerOnNewCommit = true;
                    }
                }
                else
                {
                    Commits.Add(subtract);
                    triggerOnNewCommit = true;
                }
                remoteRecord.LocalPointer = subtract;
            }
            if (triggerOnNewCommit)
            {
                await TriggerOnNewCommit();
            }
        }

        public Task PushAsync()
        {
            return PushAsync(Remotes.First());
        }

        public async Task PushAsync(IRemote<T> remoteRecord)
        {
            var commitsToPush = Commits.AfterCommitId(remoteRecord.LocalPointer?.Id);
            var remotePointer = await remoteRecord.UploadFromAsync(remoteRecord.LocalPointer?.Id, commitsToPush.ToList());
            remoteRecord.LocalPointer = Commits.FirstOrDefault(t => t.Id == remotePointer);
        }

        public async Task<string> OnPushed(string startPosition, IEnumerable<Commit<T>> commitsToPush)
        {
            string firstDiffPoint = null;
            var triggerOnNewCommit = false;
            foreach (var commit in commitsToPush)
            {
                var localAfter = Commits.AfterCommitId(startPosition).FirstOrDefault();
                if (localAfter is not null)
                {
                    if (commit.Id != localAfter.Id && Commits.Last().Id != commit.Id)
                    {
                        firstDiffPoint ??= startPosition;
                        Commits.Add(commit);
                        triggerOnNewCommit = true;
                    }
                }
                else
                {
                    Commits.Add(commit);
                    triggerOnNewCommit = true;
                }
                startPosition = commit.Id;
            }

            if (triggerOnNewCommit)
            {
                await TriggerOnNewCommit();
            }
            return firstDiffPoint ?? startPosition;
        }
    }
}
