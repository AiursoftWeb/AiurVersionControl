using AiurVersionControl.Models;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AiurVersionControl.CRUD
{
    public class CollectionRepository<T> : ControlledRepository<CollectionWorkSpace<T>>, IEnumerable<T>
    {
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
