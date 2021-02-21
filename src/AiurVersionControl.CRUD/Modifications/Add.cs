using AiurVersionControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiurVersionControl.CRUD.Modifications
{
    public class Add<T> : IModification<CollectionWorkSpace<T>>
    {
        public T Item { get; set; }

        public Add(T item)
        {
            Item = item;
        }

        public void Apply(CollectionWorkSpace<T> workspace)
        {
            workspace.List.Add(Item);
        }
    }
}
