using AiurEventSyncer.Abstract;
using AiurEventSyncer.Tools;
using AiurStore.Models;
using AiurStore.Providers.MemoryProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AiurEventSyncer.Models
{
    public class Repository<T>
    {
        public InOutDatabase<Commit<T>> Commits { get; }
        public IEnumerable<IRemote<T>> Remotes => remotesStore.AsReadOnly();
        public Commit<T> Head => Commits.LastOrDefault();
        public Func<Task> OnNewCommit { get; set; }
        public Repository() : this(new MemoryAiurStoreDb<Commit<T>>()) { }

        private readonly List<IRemote<T>> remotesStore = new List<IRemote<T>>();
        private readonly SemaphoreSlim readLock = new SemaphoreSlim(1, 1);

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
            Console.WriteLine($"New commit: {content} added locally!");
            await TriggerOnNewCommit();
        }

        private async Task TriggerOnNewCommit(IRemote<T> except = null)
        {
            if (OnNewCommit != null)
            {
                Console.WriteLine("Some service subscribed this repo change event. Broadcasting.");
                await OnNewCommit();
            }
            IEnumerable<Task> pushTasks = null;
            if (except == null)
            {
                var remotes = Remotes.Where(t => t.AutoPush);
                if (remotes.Any())
                {
                    Console.WriteLine("Will auto push to all remote repos!");
                }
                pushTasks = remotes.Select(t => PushAsync(t));
            }
            else
            {
                var remotes = Remotes.Where(t => t != except).Where(t => t.AutoPush);
                if (remotes.Any())
                {
                    Console.WriteLine("Will auto push to all repo except one!");
                }
                pushTasks = remotes.Select(t => PushAsync(t));
            }
            await Task.WhenAll(pushTasks);
        }

        /// <summary>
        /// Add a new remote repository to this.
        /// If the remote requires auto pull, it will pull it immediatly, and also register the remote change event.
        /// </summary>
        /// <param name="remote"></param>
        /// <returns></returns>
        public async Task AddRemoteAsync(IRemote<T> remote)
        {
            this.remotesStore.Add(remote);
            if (remote.AutoPull)
            {
                await this.PullAsync(remote);
                remote.OnRemoteChanged += async () =>
                {
                    Console.WriteLine("[MONITORING]: remote changed! I will pull now!");
                    await this.PullAsync(remote);
                };
            }
        }

        /// <summary>
        /// This will pull from the first remote. Not suggested if you have multiple remotes.
        /// As for more details, please check the document for `PullAsync(IRemote<T> remoteRecord)`
        /// </summary>
        /// <returns></returns>
        public Task PullAsync()
        {
            return PullAsync(Remotes.First());
        }

        public async Task PullAsync(IRemote<T> remoteRecord)
        {
            await readLock.WaitAsync();
            Console.WriteLine($"Pulling remote: {remoteRecord.Name}...");
            try
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
                    await TriggerOnNewCommit(except: remoteRecord);
                }
            }
            finally
            {
                readLock.Release();
            }
        }

        /// <summary>
        /// This will push to the first remote. Not suggested if you have multiple remotes.
        /// As for more details, please check the document for `PushAsync(IRemote<T> remoteRecord)`
        /// </summary>
        /// <returns></returns>
        public Task PushAsync()
        {
            return PushAsync(Remotes.First());
        }

        public async Task PushAsync(IRemote<T> remoteRecord)
        {
            Console.WriteLine($"Pushing remote: {remoteRecord.Name}...");
            var commitsToPush = Commits.AfterCommitId(remoteRecord.LocalPointer?.Id);
            var remotePointer = await remoteRecord.UploadFromAsync(remoteRecord.LocalPointer?.Id, commitsToPush.ToList());
            remoteRecord.LocalPointer = Commits.FirstOrDefault(t => t.Id == remotePointer);
            Console.WriteLine($"Push remote '{remoteRecord.Name}' completed. Pointer updated to: {remoteRecord.LocalPointer?.Item?.ToString()}");
        }

        public async Task<string> OnPushed(string startPosition, IEnumerable<Commit<T>> commitsToPush)
        {
            string firstDiffPoint = null;
            var triggerOnNewCommit = false;
            foreach (var commit in commitsToPush)
            {
                Console.WriteLine($"New commit: {commit.Item} (pushed by other remote) is loaded. Adding to local commits.");
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
