using AiurEventSyncer.Models;
using System;
using System.Threading.Tasks;

namespace AiurVersionControl.Models
{
    public class ControlledRepository<T> : Repository<IModification<T>> where T : WorkSpace, new()
    {
        public T WorkSpace { get; private set; }

        public ControlledRepository()
        {
            Register(Guid.NewGuid(), (commits) =>
            {
                if (WorkSpace == null)
                {
                    WorkSpace = new T();
                }
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
