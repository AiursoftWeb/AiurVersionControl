using AiurEventSyncer.Abstract;
using AiurEventSyncer.Models;
using AiurObserver;
using AiurObserver.Models;
using System;
using System.Collections.Generic;

namespace AiurVersionControl.Models
{
    public class ControlledRepository<T> : Repository<IModification<T>> where T : WorkSpace, new()
    {
        private readonly Observable<object> _subscribersManager = new();

        public T WorkSpace { get; set; }

        public IObservable<object> WorkSpaceChangedHappened => _subscribersManager;

        internal void ForceBroadcastWorkSpaceChanged() => _subscribersManager.Boradcast(null);

        public ControlledRepository()
        {
            WorkSpace = new T();
        }

        protected override void OnAppendCommits(List<Commit<IModification<T>>> newCommits)
        {
            foreach (var newCommit in newCommits)
            {
                newCommit.Item.Apply(WorkSpace);
                ForceBroadcastWorkSpaceChanged();
            }
            base.OnAppendCommits(newCommits);
        }

        public void ApplyChange(IModification<T> newModification)
        {
            Commit(newModification);
        }
    }
}
