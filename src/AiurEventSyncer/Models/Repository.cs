using AiurEventSyncer.Abstract;
using AiurEventSyncer.Tools;
using AiurStore.Abstracts;
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
        public IAfterable<Commit<T>> Commits => _commits;
        public IEnumerable<IRemote<T>> Remotes => _remotesStore.AsReadOnly();
        public Commit<T> Head => Commits.LastOrDefault();
        public Func<string, Task> OnNewCommit { get; set; }
        public Repository() : this(new MemoryAiurStoreDb<Commit<T>>()) { }

        private readonly InOutDatabase<Commit<T>> _commits;
        private readonly List<string> _localEvents = new List<string>();
        private readonly List<IRemote<T>> _remotesStore = new List<IRemote<T>>();
        private readonly SemaphoreSlim _commitAccessLock = new SemaphoreSlim(1, 1);

        public Repository(InOutDatabase<Commit<T>> dbProvider)
        {
            _commits = dbProvider;
        }

        /// <summary>
        /// Add a new commit to this repository.
        /// Also will push to all remotes which are auto push.
        /// Also for all other repositories which auto pull this one, will notify them.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public Task CommitAsync(T content)
        {
            return CommitObjectAsync(new Commit<T> { Item = content });
        }

        /// <summary>
        /// Add a new commit to this repository.
        /// Also will push to all remotes which are auto push.
        /// Also for all other repositories which auto pull this one, will notify them.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task CommitObjectAsync(Commit<T> commitObject)
        {
            await _commitAccessLock.WaitAsync();
            try
            {
                _commits.Add(commitObject);
            }
            finally
            {
                _commitAccessLock.Release();
            }
            Console.WriteLine($"[LOCAL] New commit: {commitObject.Item} added locally!");
            await TriggerOnNewCommit();
        }

        private async Task TriggerOnNewCommit(IRemote<T> except = null, string state = null)
        {
            if (OnNewCommit != null)
            {
                Console.WriteLine("[LOCAL] Some service subscribed this repo change event. Broadcasting...");
                await OnNewCommit(state);
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
            this._remotesStore.Add(remote);
            if (remote.AutoPull)
            {
                await this.PullAsync(remote);
                remote.OnRemoteChanged += async (str) =>
                {
                    if (!_localEvents.Any(t => t == str))
                    {
                        Console.WriteLine("[MONITORING]: remote changed! I will pull now!");
                        await this.PullAsync(remote);
                    }
                    else
                    {
                        Console.WriteLine("[MONITORING]: remote changed by my event. Just skip!");
                    }
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
            Console.WriteLine($"Pulling remote: {remoteRecord.Name}...");
            var triggerOnNewCommit = false;
            var subtraction = await remoteRecord.DownloadFromAsync(remoteRecord.Position);
            await _commitAccessLock.WaitAsync();
            try
            {
                foreach (var subtract in subtraction)
                {
                    Console.WriteLine($"[LOCAL] Pulled a new commit: '{subtract.Item}' from remote: {remoteRecord.Name}. Will load.");
                    var localAfter = _commits.AfterCommitId(remoteRecord.Position).FirstOrDefault();
                    if (localAfter is not null)
                    {
                        if (localAfter.Id != subtract.Id)
                        {
                            _commits.InsertAfterCommitId(remoteRecord.Position, subtract);
                            triggerOnNewCommit = true;
                        }
                    }
                    else
                    {
                        _commits.Add(subtract);
                        triggerOnNewCommit = true;
                    }
                    remoteRecord.Position = subtract.Id;
                }
            }
            finally
            {
                _commitAccessLock.Release();
            }
            if (triggerOnNewCommit)
            {
                await TriggerOnNewCommit(except: remoteRecord);
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
            List<Commit<T>> commitsToPush = null;
            await _commitAccessLock.WaitAsync();
            try
            {
                commitsToPush = _commits.AfterCommitId(remoteRecord.Position).ToList();
            }
            finally
            {
                _commitAccessLock.Release();
            }
            var eventState = Guid.NewGuid().ToString("D");
            _localEvents.Add(eventState);
            var remotePointer = await remoteRecord.UploadFromAsync(remoteRecord.Position, commitsToPush, eventState);
            remoteRecord.Position = remotePointer;
            Console.WriteLine($"Push remote '{remoteRecord.Name}' completed. Pointer updated to: {remoteRecord.Position}");
        }

        public async Task<string> OnPushed(IRemote<T> pusher, string startPosition, IEnumerable<Commit<T>> commitsToPush, string state)
        {
            string firstDiffPoint = null;
            var triggerOnNewCommit = false;
            await _commitAccessLock.WaitAsync();
            try
            {
                foreach (var commit in commitsToPush)
                {
                    Console.WriteLine($"New commit: {commit.Item} (pushed by other remote) is loaded. Adding to local commits.");
                    var localAfter = _commits.AfterCommitId(startPosition).FirstOrDefault();
                    if (localAfter is not null)
                    {
                        if (commit.Id != localAfter.Id && _commits.Last().Id != commit.Id)
                        {
                            firstDiffPoint ??= startPosition;
                            _commits.Add(commit);
                            triggerOnNewCommit = true;
                        }
                    }
                    else
                    {
                        _commits.Add(commit);
                        triggerOnNewCommit = true;
                    }
                    startPosition = commit.Id;
                }
            }
            finally
            {
                _commitAccessLock.Release();
            }
            if (triggerOnNewCommit)
            {
                await TriggerOnNewCommit(pusher, state);
            }
            return firstDiffPoint ?? startPosition;
        }
    }
}
