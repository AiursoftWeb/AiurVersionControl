using AiurEventSyncer.Abstract;
using AiurEventSyncer.Models;
using System.Collections.Generic;

namespace AiurVersionControl.Models
{
    public class ControlledRepository<T> : Repository<IModification<T>> where T : WorkSpace, new()
    {
        public T WorkSpace { get; set; }

        public ControlledRepository()
        {
            WorkSpace = new T();
        }

        protected override void OnAppendCommits(List<Commit<IModification<T>>> newCommits)
        {
            foreach (var newCommit in newCommits)
            {
                newCommit.Item.Apply(WorkSpace);
            }
            base.OnAppendCommits(newCommits);
        }

        public void ApplyChange(IModification<T> newModification)
        {
            Commit(newModification);
        }
    }
}
