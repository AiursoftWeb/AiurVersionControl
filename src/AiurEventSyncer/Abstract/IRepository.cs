using AiurEventSyncer.Models;
using AiurStore.Abstracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiurEventSyncer.Abstract
{
    public interface IRepository<T>
    {
        ConcurrentDictionary<Guid, Func<List<Commit<T>>, Task>> OnNewCommitsSubscribers { get; }
        IAfterable<Commit<T>> Commits { get; }
        Task OnPulled(List<Commit<T>> subtraction, Remote<T> remoteRecord);
    }
}
