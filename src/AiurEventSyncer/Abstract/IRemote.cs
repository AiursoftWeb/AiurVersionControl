using AiurEventSyncer.Models;
using System.Collections.Generic;

namespace AiurEventSyncer.Abstract
{
    public interface IRemote<T>
    {
        public string Name { get; set; }
        public Commit<T> LocalPointer { get; set; }
        IEnumerable<Commit<T>> DownloadFrom(string localPointerPosition);
        string UploadFrom(string startPosition, IEnumerable<Commit<T>> commitsToPush);
    }
}
