using AiurEventSyncer.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AiurEventSyncer.Abstract
{
    public interface IRemote<T>
    {
        public string Name { get; }
        public bool AutoPush { get; }
        public string PushPointer { get; set; }
        public string PullPointer { get; set; }
        public SemaphoreSlim PushLock { get; }
        public SemaphoreSlim PullLock { get; }

        /// <summary>
        /// Start a new thread to monitor remote.
        /// 
        /// When remote is changed, remote may:
        ///     Tell the repository to pull.
        ///     Or: Load the pulled data to repository directly.
        ///     
        /// When implementing: 
        ///     Only allow this method to execute once.
        ///     Only work when using auto pull.
        /// </summary>
        /// <returns></returns>
        public Task PullAndStartMonitoring();
        public Repository<T> ContextRepository { get; set; }
        /// <summary>
        /// When implementing: 
        ///     This method needs to be locked.
        /// </summary>
        Task DownloadAndPull();
        Task Upload(List<Commit<T>> commitsToPush);
        Task Unregister();
    }
}
