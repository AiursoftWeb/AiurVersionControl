using AiurEventSyncer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiurEventSyncer.Abstract
{
    public interface IRemote<T>
    {
        public Commit<T> LocalPointerPosition { get; set; }
        public string GetRemotePointerPositionId();
        IEnumerable<Commit<T>> DownloadFrom(string localPointerPosition);
    }
}
