using AiurEventSyncer.Abstract;
using AiurEventSyncer.Tools;
using AiurStore.Abstracts;
using AiurStore.Models;
using AiurStore.Providers.MemoryProvider;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        public ConcurrentDictionary<DateTime, Func<Commit<T>, Task>> OnNewCommitSubscribers { get; set; } = new ConcurrentDictionary<DateTime, Func<Commit<T>, Task>>();
        public Repository() : this(new MemoryAiurStoreDb<Commit<T>>()) { }

        private readonly InOutDatabase<Commit<T>> _commits;
        private readonly ConcurrentBag<IRemote<T>> _remotesStore = new ConcurrentBag<IRemote<T>>();
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(3);

        public Repository(InOutDatabase<Commit<T>> dbProvider)
        {
            _commits = dbProvider;
        }

        public Task CommitAsync(T content)
        {
            return CommitObjectAsync(new Commit<T> { Item = content });
        }

        public async Task CommitObjectAsync(Commit<T> commitObject)
        {
            _commits.Add(commitObject);
            await TriggerOnNewCommit(commitObject);
        }

        private async Task TriggerOnNewCommit(Commit<T> newCommit)
        {
            Console.WriteLine($"[{Name}] New commit: {newCommit.Item} added locally!");
            Console.WriteLine($"[{Name}] Current db: {string.Join(',', Commits.Select(t => t.Item.ToString()))}");
            Console.WriteLine($"[{Name}] Broadcasting...");
            var notiyTasks = OnNewCommitSubscribers.Select(t => t.Value(newCommit));
            await Task.WhenAll(notiyTasks);
#warning Consider do the same time.
            Console.WriteLine($"[{Name}] Auto pushing...");
            var pushTasks = Remotes.Where(t => t.AutoPush).Select(t => PushAsync(t));
            await Task.WhenAll(pushTasks);
        }

        public void AddRemote(IRemote<T> remote)
        {
            remote.ContextRepository = this;
            Task.Factory.StartNew(remote.PullAndMonitor).Wait();
            this._remotesStore.Add(remote);
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
            await remoteRecord.Pull();
        }

        public async Task OnPulled(IReadOnlyList<Commit<T>> subtraction, IRemote<T> remoteRecord)
        {
            await _semaphoreSlim.WaitAsync();
            foreach (var commit in subtraction)
            {
                var inserted = OnPulledCommit(commit, remoteRecord.Position);
                Console.WriteLine($"[{Name}] New commit {commit.Item} saved! Now local database: {string.Join(',', Commits.Select(t => t.Item.ToString()))}");
                remoteRecord.Position = commit.Id;
                if (inserted)
                {
                    Console.WriteLine($"[{Name}] Will trigger on new commit event. Because just inserted: {commit.Item}.");
                    await TriggerOnNewCommit(commit);
                }
            }
            _semaphoreSlim.Release();
        }

        private bool OnPulledCommit(Commit<T> subtract, string position)
        {
            Console.WriteLine($"[{Name}] Pulled a new commit: '{subtract.Item}'. Will load to local database: {string.Join(',', Commits.Select(t => t.Item.ToString()))}");
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

        public async Task PushAsync(IRemote<T> remoteRecord)
        {
            Console.WriteLine($"[{Name}] Pushing remote: {remoteRecord.Name}...");
            List<Commit<T>> commitsToPush = _commits.AfterCommitId(remoteRecord.Position).ToList();
            await remoteRecord.Push(commitsToPush);
            Console.WriteLine($"[{Name}] Push remote '{remoteRecord.Name}' completed.");
        }

        public async Task OnPushed(string startPosition, IEnumerable<Commit<T>> commitsToPush)
        {
            await _semaphoreSlim.WaitAsync();
            Console.WriteLine($"[{Name}] New {commitsToPush.Count()} commits (pushed by other remote) are loaded. Adding to local commits.");
            foreach (var commit in commitsToPush) // 4,5,6
            {
                var inserted = OnPushedCommit(commit, startPosition);
                startPosition = commit.Id;
                if (inserted)
                {
                    await TriggerOnNewCommit(commit);
                }
            }
            _semaphoreSlim.Release();
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
