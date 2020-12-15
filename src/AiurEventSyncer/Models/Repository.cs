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
        [Obsolete]
        public IEnumerable<Remote<T>> Remotes => _remotesStore.ToList();
        public Commit<T> Head => Commits.LastOrDefault();
#warning find a better solution to register.
        public ConcurrentDictionary<DateTime, Func<ConcurrentBag<Commit<T>>, Task>> OnNewCommitsSubscribers { get; set; } = new ConcurrentDictionary<DateTime, Func<ConcurrentBag<Commit<T>>, Task>>();

        private readonly InOutDatabase<Commit<T>> _commits;
        private readonly List<Remote<T>> _remotesStore = new List<Remote<T>>();
        private readonly SemaphoreSlim _pullingLock = new SemaphoreSlim(1);
        private readonly TaskQueue _notifyingQueue = new TaskQueue(1);

        public Repository(InOutDatabase<Commit<T>> dbProvider) { _commits = dbProvider; }
        public Repository() : this(new MemoryAiurStoreDb<Commit<T>>()) { }

        public Task CommitAsync(T content)
        {
            return CommitObjectAsync(new Commit<T> { Item = content });
        }

        public async Task CommitObjectAsync(Commit<T> commitObject)
        {
            _commits.Add(commitObject);
            TriggerOnNewCommits(new ConcurrentBag<Commit<T>> { commitObject });
        }

#warning Use List is ok.
        private void TriggerOnNewCommits(ConcurrentBag<Commit<T>> newCommits)
        {
            Console.WriteLine($"[{Name}] New commits: {string.Join(',', newCommits.Select(t => t.Item.ToString()))} added locally!");
            Console.WriteLine($"[{Name}] Current db: {string.Join(',', Commits.Select(t => t.Item.ToString()))}");
            var notiyTasks = OnNewCommitsSubscribers.Select(t => t.Value(newCommits)).ToList();
            Console.WriteLine($"[{Name}] Broadcasting and auto pushing... Totally: {notiyTasks.Count} listeners.");
            _notifyingQueue.QueueNew(async () =>
            {
                await Task.WhenAll(notiyTasks);
            });
        }

        public async Task AddRemoteAsync(Remote<T> remote)
        {
            remote.ContextRepository = this;
            await remote.BeAdded();
            _remotesStore.Add(remote);
        }

        public async Task DropRemoteAsync(Remote<T> remote)
        {
            if (!_remotesStore.Contains(remote))
            {
                throw new InvalidOperationException("Our remotes record doesn't contains the remote you want to drop.");
            }
            if (remote.ContextRepository != this)
            {
                throw new InvalidOperationException("Our remotes record you want to drop do not have a context for current repository.");
            }
            await remote.StopMonitoring();
            _remotesStore.Remove(remote);
        }

        public Task PullAsync()
        {
            return Remotes.First().Pull();
        }

        public Task PushAsync()
        {
            return Remotes.First().Push();
        }

        public async Task OnPulled(List<Commit<T>> subtraction, Remote<T> remoteRecord)
        {
            await _pullingLock.WaitAsync();
            Console.WriteLine($"[{Name}] Loading on pulled commits {string.Join(',', subtraction.Select(t => t.Item.ToString()))} from remote: {remoteRecord.Name}");
            var newCommitsSaved = new ConcurrentBag<Commit<T>>();
            var pushingPushPointer = false;
            foreach (var commit in subtraction)
            {
                Console.WriteLine($"[{Name}] Trying to save pulled commit : {commit}...");
                var inserted = OnPulledCommit(commit, remoteRecord.PullPointer);
                Console.WriteLine($"[{Name}] New commit {commit.Item} saved! Now local database: {string.Join(',', Commits.Select(t => t.Item.ToString()))}");
                if (remoteRecord.PullPointer == remoteRecord.PushPointer)
                {
                    pushingPushPointer = true;
                }
                remoteRecord.PullPointer = commit.Id;
                if (pushingPushPointer == true)
                {
                    remoteRecord.PushPointer = remoteRecord.PullPointer;
                }
                if (inserted)
                {
                    newCommitsSaved.Add(commit);
                }
            }
            _pullingLock.Release();
            if (newCommitsSaved.Any())
            {
                Console.WriteLine($"[{Name}] Will trigger on new commit event. Because just pulled: {string.Join(',', newCommitsSaved.Select(t => t.Item.ToString()))}.");
                TriggerOnNewCommits(newCommitsSaved);
            }
        }

        private bool OnPulledCommit(Commit<T> subtract, string position)
        {
            Console.WriteLine($"[{Name}] Pull process is loading commit: '{subtract.Item}' to local database: {string.Join(',', Commits.Select(t => t.Item.ToString()))}");
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

        public async Task OnPushed(IEnumerable<Commit<T>> commitsToPush, string startPosition)
        {
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
            if (newCommitsSaved.Any())
            {
                Console.WriteLine($"[{Name}] Will trigger on new commit event. Because just by pushed with: {string.Join(',', newCommitsSaved.Select(t => t.Item.ToString()))}.");
                TriggerOnNewCommits(newCommitsSaved);
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
