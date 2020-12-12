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
        public string PushPointer { get; set; }
        public string HEAD { get; set; }
        public Task StartPullAndMonitor();
        public Repository<T> ContextRepository { get; set; }
        Task Download();
        Task Upload(List<Commit<T>> commitsToPush);
    }
}
