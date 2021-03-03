using AiurEventSyncer.Abstract;
using AiurEventSyncer.Models;
using AiurObserver.Models;
using System;
using System.Collections.Generic;

namespace AiurVersionControl.Models
{
    /// <summary>
    /// A special repository which contains a workspace and applies modifications to it automatically.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ControlledRepository<T> : Repository<IModification<T>> where T : WorkSpace, new()
    {
        private readonly Observable<object> _subscribersManager = new();

        public T WorkSpace { get; set; }

        public IObservable<object> WorkSpaceChangedHappened => _subscribersManager;

        /// <summary>
        /// Call this method to manually broadcast a new event to all workspace changed subscribers.
        /// </summary>
        public virtual void BroadcastWorkSpaceChanged() => _subscribersManager.Boradcast(null);

        public ControlledRepository()
        {
            WorkSpace = new T();
        }

        protected override void OnAppendCommits(List<Commit<IModification<T>>> newCommits)
        {
            foreach (var newCommit in newCommits)
            {
                newCommit.Item.Apply(WorkSpace);
                BroadcastWorkSpaceChanged();
            }
            base.OnAppendCommits(newCommits);
        }

        public void ApplyChange(IModification<T> newModification)
        {
            Commit(newModification);
        }
    }
}
