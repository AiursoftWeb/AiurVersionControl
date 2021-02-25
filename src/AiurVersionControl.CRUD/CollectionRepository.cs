using AiurVersionControl.CRUD.Modifications;
using AiurVersionControl.Models;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AiurVersionControl.CRUD
{
    public class CollectionRepository<T> : ControlledRepository<CollectionWorkSpace<T>>, IEnumerable<T>
    {
        public void Add(T newItem)
        {
            ApplyChange(new Add<T>(newItem));
        }

        public void Drop<D>(string searchProperty, D value)
        {
            ApplyChange(new Drop<T, D>(searchProperty, value));
        }

        public void Patch(string searchPropertyName,
            object expectValue,
            string patchPropertyName,
            object newValue)
        {
            ApplyChange(new Patch<T>(searchPropertyName, expectValue, patchPropertyName, newValue));
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
