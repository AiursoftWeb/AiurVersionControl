using AiurEventSyncer.Models;
using AiurStore.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AiurEventSyncer.Abstract
{
    public interface IRepository<T>
    {
        void RegisterAsyncTask(Guid key, Func<List<Commit<T>>, Task> action);
        void Register(Guid key, Func<List<Commit<T>>, Task> action);
        void UnRegister(Guid key);
        InOutDatabase<Commit<T>> Commits { get; }
        Task OnPulled(List<Commit<T>> subtraction, IRemote<T> remoteRecord);
    }
}
