using AiurEventSyncer.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AiurEventSyncer.Abstract
{
    public interface IRemote<T>
    {
        public string Name { get; }
        public bool AutoPush { get; }
        public string Position { get; set; }
        public Task PullAndMonitor();
        public Repository<T> ContextRepository { get; set; }
        Task Pull();
        Task Push(IReadOnlyList<Commit<T>> commitsToPush);
    }
}
