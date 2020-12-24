using AiurEventSyncer.Abstract;
using AiurEventSyncer.Tools;
using AiurStore.Abstracts;
using AiurStore.Models;
using AiurStore.Providers.MemoryProvider;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
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
        public Commit<T> Head => Commits.LastOrDefault();
        public ConcurrentDictionary<Guid, Func<List<Commit<T>>, Task>> OnNewCommitsSubscribers { get; set; } = new ConcurrentDictionary<Guid, Func<List<Commit<T>>, Task>>();

        private readonly ILogger<Repository<T>> _logger;
        private readonly InOutDatabase<Commit<T>> _commits;
        private readonly SemaphoreSlim _pullingLock = new SemaphoreSlim(1);
        private readonly TaskQueue _notifyingQueue = new TaskQueue(1);

        public Repository(
            InOutDatabase<Commit<T>> dbProvider,
            ILogger<Repository<T>> logger)
        { 
            _commits = dbProvider;
            _logger = logger;
        }
        public Repository() : this(new MemoryAiurStoreDb<Commit<T>>(), null) 
        {
        }

        public void Commit(T content)
        {
            CommitObject(new Commit<T> { Item = content });
        }

        public void CommitObject(Commit<T> commitObject)
        {
            _commits.Add(commitObject);
            OnNewCommits(new List<Commit<T>>{ commitObject });
        }

        private void OnNewCommits(List<Commit<T>> newCommits)
        {
            _logger?.LogInformation($"[{Name}] New commits: {string.Join(',', newCommits.Select(t => t.Item.ToString()))} added locally!");
            Console.WriteLine($"[{Name}] Current db: {string.Join(',', Commits.Select(t => t.Item.ToString()))}");
            var notiyTasks = OnNewCommitsSubscribers.Select(t => t.Value(newCommits)).ToList();
            Console.WriteLine($"[{Name}] Broadcasting and auto pushing... Totally: {notiyTasks.Count} listeners.");
            _notifyingQueue.QueueNew(async () =>
            {
                await Task.WhenAll(notiyTasks);
            });
        }

        public async Task OnPulled(List<Commit<T>> subtraction, Remote<T> remoteRecord)
        {
            var newCommitsSaved = new List<Commit<T>>();
            var pushingPushPointer = false;

            await _pullingLock.WaitAsync();
            Console.WriteLine($"[{Name}] Loading on pulled commits {string.Join(',', subtraction.Select(t => t.Item.ToString()))} from remote: {remoteRecord.Name}");
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
                OnNewCommits(newCommitsSaved);
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
            var newCommitsSaved = new List<Commit<T>>();
            await _pullingLock.WaitAsync();
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
            _pullingLock.Release();
            if (newCommitsSaved.Any())
            {
                Console.WriteLine($"[{Name}] Will trigger on new commit event. Because just by pushed with: {string.Join(',', newCommitsSaved.Select(t => t.Item.ToString()))}.");
                OnNewCommits(newCommitsSaved);
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
