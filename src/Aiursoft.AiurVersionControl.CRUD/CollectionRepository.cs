using Aiursoft.AiurVersionControl.CRUD.Modifications;
using Aiursoft.AiurVersionControl.Models;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Aiursoft.AiurVersionControl.CRUD
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

        public void Drop<TProperty>(string searchProperty, TProperty value)
        {
            ApplyChange(new Drop<T, TProperty>(searchProperty, value));
        }

        public void Patch<TPropertySearch, TPropertyPatch>(string searchPropertyName,
            TPropertySearch expectValue,
            string patchPropertyName,
            TPropertyPatch newValue)
        {
            ApplyChange(new Patch<T, TPropertySearch, TPropertyPatch>(searchPropertyName, expectValue, patchPropertyName, newValue));
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
