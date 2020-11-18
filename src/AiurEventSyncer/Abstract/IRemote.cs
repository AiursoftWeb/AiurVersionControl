using AiurEventSyncer.Models;
using System.Collections.Generic;

namespace AiurEventSyncer.Abstract
{
    public interface IRemote<T>
    {
        public Commit<T> LocalPointer { get; set; }
        IEnumerable<Commit<T>> DownloadFrom(string localPointerPosition);
        void UploadFrom(string startPosition, IEnumerable<Commit<T>> commitsToPush);
    }
}
