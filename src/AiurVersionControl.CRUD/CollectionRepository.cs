using AiurVersionControl.CRUD.Modifications;
using AiurVersionControl.Models;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace AiurVersionControl.CRUD
{
    /// <summary>
    /// A special controlled repository that contains a collection workspace which you can do CRUD to.
    /// </summary>
    /// <typeparam name="T">The item type of the collection.</typeparam>
    public class CollectionRepository<T> : ControlledRepository<CollectionWorkSpace<T>>, IEnumerable<T>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public override void BroadcastWorkSpaceChanged()
        {
            base.BroadcastWorkSpaceChanged();
            CollectionChanged?.Invoke(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void Add(T newItem)
        {
            ApplyChange(new Add<T>(newItem));
        }

        public void Drop<D>(string searchProperty, D value)
        {
            ApplyChange(new Drop<T, D>(searchProperty, value));
        }

        public void Patch<D1, D2>(string searchPropertyName,
            D1 expectValue,
            string patchPropertyName,
            D2 newValue)
        {
            ApplyChange(new Patch<T, D1, D2>(searchPropertyName, expectValue, patchPropertyName, newValue));
        }

        public IEnumerator<T> GetEnumerator()
        {
            return WorkSpace.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
