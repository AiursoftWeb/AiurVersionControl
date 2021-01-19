using AiurEventSyncer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiurEventSyncer.Abstract
{
    public interface IConnectionProvider<T>
    {
        Task Disconnect();

        Task PullAndMonitor(Func<List<Commit<T>>, Task> onData, string startPosition);

        Task Upload(List<Commit<T>> commits, string pointerId);

        Task<List<Commit<T>>> Download(string pointer);
    }
}
