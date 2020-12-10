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
        public Func<Commit<T>, Task> OnNewCommit { get; set; }
        public Repository() : this(new MemoryAiurStoreDb<Commit<T>>()) { }

        private readonly InOutDatabase<Commit<T>> _commits;
        private readonly List<IRemote<T>> _remotesStore = new List<IRemote<T>>();
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
            Console.WriteLine($"[LOCAL] New commit: {newCommit.Item} added locally!");
            if (OnNewCommit != null)
            {
                Console.WriteLine("[LOCAL] Some service subscribed this repo change event. Broadcasting...");
                await OnNewCommit(newCommit);
            }
            var pushTasks = Remotes.Where(t => t.AutoPush).Select(t => PushAsync(t));
            await Task.WhenAll(pushTasks);
        }

        public void AddRemote(IRemote<T> remote)
        {
            this._remotesStore.Add(remote);
        }

        public Task PullAsync(bool keepAlive = false)
        {
            return PullAsync(Remotes.First(), keepAlive);
        }

        public Task PushAsync()
        {
            return PushAsync(Remotes.First());
        }

        public async Task PullAsync(IRemote<T> remoteRecord,bool keepAlive = false)
        {
            Console.WriteLine($"Pulling remote: {remoteRecord.Name}...");
            await remoteRecord.DownloadAndSaveTo(keepAlive, this);
        }

        public async Task OnPulled(IReadOnlyList<Commit<T>> subtraction, IRemote<T> remoteRecord)
        {
            await _semaphoreSlim.WaitAsync();
            Console.WriteLine($"DEBUG!: POINTER: {remoteRecord.Position}");
            Console.WriteLine($"DEBUG!: On Pulled: {string.Join(',', subtraction.Select(t => t.Item.ToString()))}");
            Console.WriteLine($"DEBUG!: Current db: {string.Join(',', Commits.Select(t => t.Item.ToString()))}");
            foreach (var commit in subtraction)
            {
                var inserted = OnPulledCommit(commit, remoteRecord.Position);
                remoteRecord.Position = commit.Id;
                if (inserted)
                {
                    await TriggerOnNewCommit(commit);
                }
            }
            Console.WriteLine($"DEBUG!: Finished pull. db: {string.Join(',', Commits.Select(t => t.Item.ToString()))}");
            _semaphoreSlim.Release();
        }

        private bool OnPulledCommit(Commit<T> subtract, string position)
        {
            Console.WriteLine($"[LOCAL] Pulled a new commit: '{subtract.Item}'. Will load.");
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
            Console.WriteLine($"Pushing remote: {remoteRecord.Name}...");
            List<Commit<T>> commitsToPush = _commits.AfterCommitId(remoteRecord.Position).ToList();
            await remoteRecord.UploadFromAsync(commitsToPush);
            Console.WriteLine($"Push remote '{remoteRecord.Name}' completed.");
        }

        public async Task OnPushed(string startPosition, IEnumerable<Commit<T>> commitsToPush)
        {
            await _semaphoreSlim.WaitAsync();
            Console.WriteLine($"[REMOTE] New {commitsToPush.Count()} commits (pushed by other remote) are loaded. Adding to local commits.");
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
            Console.WriteLine($"[REMOTE] New commit: {subtract.Item} (pushed by other remote) is loaded. Adding to local commits.");
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
            }
            return true;
        }
    }
}
