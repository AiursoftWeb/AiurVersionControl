//using AiurEventSyncer.Models;
//using AiurEventSyncer.Tools;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace AiurEventSyncer.Abstract
//{
//    public abstract class Remote<T>
//    {
//        public string Name { get; init; } = "remote name";
//        public bool AutoPush { get; init; }
//        public bool AutoPull { get; init; }
//        public string PullPointer { get; set; }
//        public string PushPointer { get; set; }
//        public SemaphoreSlim PushLock { get; } = new SemaphoreSlim(1);
//        public SemaphoreSlim PullLock { get; } = new SemaphoreSlim(1);
//        public Repository<T> ContextRepository { get; set; }
//        private readonly DateTime _key = DateTime.UtcNow;

//        public Remote(bool autoPush = false, bool autoPull = false)
//        {
//            AutoPush = autoPush;
//            AutoPull = autoPull;
//        }

//        public async Task Push()
//        {
//            await PushLock.WaitAsync();
//            var commitsToPush = ContextRepository.Commits.AfterCommitId(PushPointer).ToList();
//            if (commitsToPush.Any())
//            {
//                Console.WriteLine($"[{Name}] Pushing remote: {Name}... Pushing content: {string.Join(',', commitsToPush.Select(t => t.Item.ToString()))}");
//                await Upload(commitsToPush, PushPointer);
//                Console.WriteLine($"[{Name}] Push remote '{Name}' completed.");
//                PushPointer = commitsToPush.Last().Id;
//            }
//            PushLock.Release();
//        }

//        public async Task Pull()
//        {
//            await PullLock.WaitAsync();
//            if (ContextRepository == null)
//            {
//                throw new ArgumentNullException(nameof(ContextRepository), "Please add this remote to a repository.");
//            }
//            var downloadResult = await Download(PullPointer);
//            await ContextRepository.OnPulled(downloadResult, this);
//            PullLock.Release();
//        }

//        public async Task BeAdded()
//        {
//            if (AutoPull)
//            {
//                await Pull();
//                await RegisterOnCommingCommit();
//            }
//        }
//        public Task Unregister()
//        {
//            return Task.CompletedTask;
//        }

//        public abstract Task RegisterOnCommingCommit();
//        //_fakeRemoteRepository.OnNewCommitsSubscribers[_key] = async (c) =>
//        //{
//        //    await ContextRepository.OnPulled(c.ToList(), this);
//        //};

//        public abstract Task Upload(List<Commit<T>> commits, string pushPointer);
//        //_fakeRemoteRepository.OnPushed(commitsToPush, PushPointer?.ToString());

//        public abstract Task<List<Commit<T>>> Download(string pointer);
//        //_fakeRemoteRepository.Commits.AfterCommitId(PullPointer).ToList();

//        public abstract Task Disconnect();
//        //_fakeRemoteRepository.OnNewCommitsSubscribers.TryRemove(_key, out _);
//    }
//}
