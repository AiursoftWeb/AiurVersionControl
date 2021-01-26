using AiurEventSyncer.Models;
using System;

namespace AiurVersionControl.Models
{
    public class ControlledRepository<T> : Repository<IModification<T>> where T : WorkSpace, new()
    {
        public T WorkSpace { get; private set; }

        public ControlledRepository()
        {
            WorkSpace = new T();
            Register(Guid.NewGuid(), (commits) =>
            {
                foreach (var newCommit in commits)
                {
                    newCommit.Item.Apply(WorkSpace);
                }
            });
        }

        public void ApplyChange(IModification<T> newModification)
        {
            Commit(newModification);
        }
    }
}
