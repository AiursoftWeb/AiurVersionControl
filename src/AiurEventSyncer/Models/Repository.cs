using AiurEventSyncer.Abstract;
using AiurEventSyncer.Tools;
using AiurStore.Abstracts;
using AiurStore.Models;
using AiurStore.Providers.MemoryProvider;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AiurEventSyncer.Models
{
    public class Repository<T>
    {
        public string Name { get; init; } = string.Empty;
        public IAfterable<Commit<T>> Commits => _commits;
        public IEnumerable<IRemote<T>> Remotes => _remotesStore.ToList();
        public Commit<T> Head => Commits.LastOrDefault();
        public ConcurrentDictionary<DateTime, Func<ConcurrentBag<Commit<T>>, Task>> OnNewCommitsSubscribers { get; set; } = new ConcurrentDictionary<DateTime, Func<ConcurrentBag<Commit<T>>, Task>>();

        private readonly InOutDatabase<Commit<T>> _commits;
        private readonly List<IRemote<T>> _remotesStore = new List<IRemote<T>>();
        private readonly SemaphoreSlim _insertCommitLock = new SemaphoreSlim(1);

        public Repository(InOutDatabase<Commit<T>> dbProvider) { _commits = dbProvider; }
        public Repository() : this(new MemoryAiurStoreDb<Commit<T>>()) { }

        public Task CommitAsync(T content)
        {
            return CommitObjectAsync(new Commit<T> { Item = content });
        }

        public async Task CommitObjectAsync(Commit<T> commitObject)
        {
            await _insertCommitLock.WaitAsync();
            _commits.Add(commitObject);
            _insertCommitLock.Release();
            await TriggerOnNewCommits(new ConcurrentBag<Commit<T>> { commitObject });
        }

        private async Task TriggerOnNewCommits(ConcurrentBag<Commit<T>> newCommits)
        {
            Console.WriteLine($"[{Name}] New commits: {string.Join(',', newCommits.Select(t => t.Item.ToString()))} added locally!");
            Console.WriteLine($"[{Name}] Current db: {string.Join(',', Commits.Select(t => t.Item.ToString()))}");
            Console.WriteLine($"[{Name}] Broadcasting and auto pushing...");
            var notiyTasks = OnNewCommitsSubscribers.Select(t => t.Value(newCommits));
            var pushTasks = Remotes.Where(t => t.AutoPush).Select(t => PushAsync(t));
            await Task.Factory.StartNew(async () => await Task.WhenAll(notiyTasks));
            await Task.Factory.StartNew(async () => await Task.WhenAll(pushTasks));
        }

        public async Task AddRemoteAsync(IRemote<T> remote)
        {
            remote.ContextRepository = this;
            await remote.StartPullAndMonitor();
            _remotesStore.Add(remote);
        }

        public async Task DropRemoteAsync(IRemote<T> remote)
        {
            if (!_remotesStore.Contains(remote))
            {
                throw new InvalidOperationException("Our remotes record doesn't contains the remote you want to drop.");
            }
            if (remote.ContextRepository != this)
            {
                throw new InvalidOperationException("Our remotes record you want to drop do not have a context for current repository.");
            }
            await remote.Unregister();
            _remotesStore.Remove(remote);
        }

        public Task PullAsync()
        {
            return PullAsync(Remotes.First());
        }

        public Task PushAsync()
        {
            return PushAsync(Remotes.First());
        }

        public async Task PullAsync(IRemote<T> remoteRecord)
        {
            Console.WriteLine($"[{Name}] Pulling remote: {remoteRecord.Name}...");
            await remoteRecord.Download();
        }

        public async Task OnPulled(List<Commit<T>> subtraction, IRemote<T> remoteRecord)
        {
            Console.WriteLine($"[{Name}] Loading on pulled commits {string.Join(',', subtraction.Select(t => t.Item.ToString()))} from remote: {remoteRecord.Name}");
            await remoteRecord.PullLock.WaitAsync();
            Console.WriteLine($"[{Name}] pull unlocked!");
            var newCommitsSaved = new ConcurrentBag<Commit<T>>();
            var pushingPushPointer = false;
            foreach (var commit in subtraction)
            {
                Console.WriteLine($"[{Name}] Trying to save pulled  commit : {commit}...");
                var inserted = await OnPulledCommit(commit, remoteRecord.HEAD);
                Console.WriteLine($"[{Name}] New commit {commit.Item} saved! Now local database: {string.Join(',', Commits.Select(t => t.Item.ToString()))}");
                if (remoteRecord.HEAD == remoteRecord.PushPointer)
                {
                    pushingPushPointer = true;
                }
                remoteRecord.HEAD = commit.Id;
                if (pushingPushPointer == true)
                {
                    remoteRecord.PushPointer = remoteRecord.HEAD;
                }
                if (inserted)
                {
                    newCommitsSaved.Add(commit);
                }
            }
            remoteRecord.PullLock.Release();
            if (newCommitsSaved.Any())
            {
                Console.WriteLine($"[{Name}] Will trigger on new commit event. Because just pulled: {string.Join(',', newCommitsSaved.Select(t => t.Item.ToString()))}.");
                await TriggerOnNewCommits(newCommitsSaved);
            }
        }

        private async Task<bool> OnPulledCommit(Commit<T> subtract, string position)
        {
            Console.WriteLine($"[{Name}] Pulled a new commit: '{subtract.Item}'. Will load to local database: {string.Join(',', Commits.Select(t => t.Item.ToString()))}");
            await _insertCommitLock.WaitAsync();
            Console.WriteLine($"[{Name}] Insert commit unlocked!");
            try
            {
                var localAfter = _commits.AfterCommitId(position).FirstOrDefault();
                if (localAfter is not null)
                {
                    if (localAfter.Id != subtract.Id)
                    {
                        _commits.InsertAfterCommitId(position, subtract);
                        return true;
                    }
                }
                else
                {
                    _commits.Add(subtract);
                    return true;
                }
                return false;
            }
            finally
            {
                _insertCommitLock.Release();
            }
        }

        public async Task PushAsync(IRemote<T> remoteRecord)
        {
            await remoteRecord.PushLock.WaitAsync();
            var commitsToPush = _commits.AfterCommitId(remoteRecord.PushPointer).ToList();
            if (commitsToPush.Any())
            {
                Console.WriteLine($"[{Name}] Pushing remote: {remoteRecord.Name}... Pushing content: {string.Join(',', commitsToPush.Select(t => t.Item.ToString()))}");
                await remoteRecord.Upload(commitsToPush);
                Console.WriteLine($"[{Name}] Push remote '{remoteRecord.Name}' completed.");
                remoteRecord.PushPointer = commitsToPush.Last().Id;
            }
            remoteRecord.PushLock.Release();
        }

        public async Task OnPushed(IEnumerable<Commit<T>> commitsToPush, string startPosition)
        {
            await _insertCommitLock.WaitAsync();
            var newCommitsSaved = new ConcurrentBag<Commit<T>>();
            Console.WriteLine($"[{Name}] New {commitsToPush.Count()} commits: {string.Join(',', commitsToPush.Select(t => t.Item.ToString()))} (pushed by other remote) are loaded. Adding to local commits.");
            foreach (var commit in commitsToPush) // 4,5,6
            {
                var inserted = OnPushedCommit(commit, startPosition);
                startPosition = commit.Id;
                if (inserted)
                {
                    newCommitsSaved.Add(commit);
                }
            }
            _insertCommitLock.Release();
            if (newCommitsSaved.Any())
            {
                Console.WriteLine($"[{Name}] Will trigger on new commit event. Because just by pushed with: {string.Join(',', newCommitsSaved.Select(t => t.Item.ToString()))}.");
                await TriggerOnNewCommits(newCommitsSaved);
            }
        }

        private bool OnPushedCommit(Commit<T> subtract, string position)
        {
            var localAfter = _commits.AfterCommitId(position).FirstOrDefault();
            if (localAfter is not null)
            {
                if (subtract.Id != localAfter.Id)
                {
                    _commits.Add(subtract);
                    return true;
                }
            }
            else
            {
                _commits.Add(subtract);
                return true;
            }
            return false;
        }
    }
}
