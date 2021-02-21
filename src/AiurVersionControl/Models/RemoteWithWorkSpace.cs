using AiurEventSyncer.Abstract;
using AiurEventSyncer.Models;

namespace AiurVersionControl.Models
{
    public class RemoteWithWorkSpace<T> : Remote<IModification<T>> where T : WorkSpace, new()
    {
        public T RemoteWorkSpace { get; } = new T();

        public RemoteWithWorkSpace(
            IConnectionProvider<IModification<T>> provider,
            bool autoPush = false,
            bool autoPull = false) : base(provider, autoPush, autoPull)
        {
        }

        public override void OnPullPointerMoved(Commit<IModification<T>> pointer)
        {
            pointer.Item.Apply(RemoteWorkSpace);
        }

        public override void OnPullInsert()
        {
            if (ContextRepository is ControlledRepository<T> controlled)
            {
                var fork = (T)RemoteWorkSpace.Clone();
                var localNewCommits = controlled.Commits.GetAllAfter(PullPointer);
                foreach (var localNewCommit in localNewCommits)
                {
                    localNewCommit.Item.Apply(fork);
                }
                controlled.WorkSpace = fork;
            }
        }
    }
}
