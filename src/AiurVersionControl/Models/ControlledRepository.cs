using AiurEventSyncer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using AiurEventSyncer.Abstract;

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

        protected override void OnInsertedCommits(List<Commit<IModification<T>>> insertedCommits)
        {
            // Back then forth
            var speedingCommits = Commits.GetAllAfter(insertedCommits.Last()).ToList();
            for (int i = speedingCommits.Count - 1; i >= 0; i--)
            {
                speedingCommits[i].Item.Rollback(WorkSpace);
            }
            insertedCommits.AddRange(speedingCommits);
            foreach (var commit in insertedCommits)
            {
                commit.Item.Apply(WorkSpace);
            }
        }
    }
}
