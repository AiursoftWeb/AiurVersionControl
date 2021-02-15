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
            if (ContextRepository is not ControlledRepository<T>)
            {
                // In this case, the user is trying to use a RemoteWithWorkSpace attached to a typical Repository.
                return;
            }

            var fork = (T)RemoteWorkSpace.Clone();
            var localNewCommits = ContextRepository.Commits.GetAllAfter(PullPointer);
            foreach (var localNewCommit in localNewCommits)
            {
                localNewCommit.Item.Apply(fork);
            }
            (ContextRepository as ControlledRepository<T>).WorkSpace = fork;
        }
    }
}
