using AiurEventSyncer.Abstract;
using AiurEventSyncer.Models;
using System.Collections.Generic;
using System.ComponentModel;

namespace AiurVersionControl.Models
{
    /// <summary>
    /// A special repository which contains a workspace and applies modifications to it automatically.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ControlledRepository<T> : Repository<IModification<T>>, INotifyPropertyChanged where T : WorkSpace, new()
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public T WorkSpace { get; internal set; }

        /// <summary>
        /// Call this method to manually broadcast a new event to all workspace changed subscribers.
        /// </summary>
        public virtual void BroadcastWorkSpaceChanged() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WorkSpace)));

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
