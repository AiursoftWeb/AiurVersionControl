using AiurEventSyncer.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AiurEventSyncer.Abstract
{
    public interface IRemote<T>
    {
        public string Name { get; set; }
        public bool AutoPush { get; set; }
        public string Position { get; set; }
        Task DownloadAndSaveTo(bool keepAlive, Repository<T> repository);
        Task UploadFromAsync(IReadOnlyList<Commit<T>> commitsToPush);
    }
}
