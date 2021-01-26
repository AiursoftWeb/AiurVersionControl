using AiurEventSyncer.Abstract;
using AiurEventSyncer.Models;

namespace AiurVersionControl.Models
{
    public class RemoteWithWorkSpace<T> : Remote<IModification<T>> where T : WorkSpace, new()
    {
        public T RemoteWorkSpace { get; set; } = new T();

        public RemoteWithWorkSpace(
            IConnectionProvider<IModification<T>> provider,
            bool autoPush = false,
            bool autoPull = false) : base(provider, autoPush, autoPull)
        {
        }


    }
}
