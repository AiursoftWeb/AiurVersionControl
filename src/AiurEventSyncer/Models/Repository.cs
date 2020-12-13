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
        public ConcurrentDictionary<DateTime, Func<List<Commit<T>>, Task>> OnNewCommitsSubscribers { get; set; } = new ConcurrentDictionary<DateTime, Func<List<Commit<T>>, Task>>();
        public Repository() : this(new MemoryAiurStoreDb<Commit<T>>()) { }

        private readonly InOutDatabase<Commit<T>> _commits;
        private readonly ConcurrentBag<IRemote<T>> _remotesStore = new ConcurrentBag<IRemote<T>>();
        private readonly SemaphoreSlim _insertCommitLock = new SemaphoreSlim(1);

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
            await TriggerOnNewCommits(new List<Commit<T>> { commitObject });
        }

        private async Task TriggerOnNewCommits(List<Commit<T>> newCommits)
        {
            Console.WriteLine($"[{Name}] New commits: {string.Join(',', newCommits.Select(t => t.Item.ToString()))} added locally!");
            Console.WriteLine($"[{Name}] Current db: {string.Join(',', Commits.Select(t => t.Item.ToString()))}");
            Console.WriteLine($"[{Name}] Broadcasting and auto pushing...");
            var notiyTasks = OnNewCommitsSubscribers.Select(t => t.Value(newCommits));
            var pushTasks = Remotes.Where(t => t.AutoPush).Select(t => PushAsync(t));
            await Task.Factory.StartNew(async () => await Task.WhenAll(notiyTasks));
            await Task.Factory.StartNew(async () => await Task.WhenAll(pushTasks));
        }

        public void AddRemote(IRemote<T> remote)
        {
            remote.ContextRepository = this;
            remote.StartPullAndMonitor().Wait();
#warning make async!
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
            await remoteRecord.Download();
        }

        public async Task OnPulled(IReadOnlyList<Commit<T>> subtraction, IRemote<T> remoteRecord)
        {
            await _insertCommitLock.WaitAsync();
            var newCommitsSaved = new List<Commit<T>>();
            var pushingPushPointer = false;
            foreach (var commit in subtraction)
            {
                var inserted = OnPulledCommit(commit, remoteRecord.HEAD);
                Console.WriteLine($"[{Name}] New commit {commit.Item} saved! Now local database: {string.Join(',', Commits.Select(t => t.Item.ToString()))}");
                if(remoteRecord.HEAD == remoteRecord.PushPointer)
                {
                    pushingPushPointer = true;
                }
                remoteRecord.HEAD = commit.Id;
                if(pushingPushPointer == true)
                {
                    remoteRecord.PushPointer = remoteRecord.HEAD;
                }
                if (inserted)
                {
                    newCommitsSaved.Add(commit);
                }
            }
            _insertCommitLock.Release();
            if (newCommitsSaved.Any())
            {
                Console.WriteLine($"[{Name}] Will trigger on new commit event. Because just pulled: {string.Join(',', newCommitsSaved.Select(t => t.Item.ToString()))}.");
                await TriggerOnNewCommits(newCommitsSaved);
            }
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
            var newCommitsSaved = new List<Commit<T>>();
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
                await TriggerOnNewCommits(newCommitsSaved);
            }
            _insertCommitLock.Release();
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
