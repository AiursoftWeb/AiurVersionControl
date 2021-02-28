using AiurVersionControl.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AiurVersionControl.CRUD
{
    public class CollectionWorkSpace<T> : WorkSpace, IEnumerable<T>
    {
        public List<T> List { get; private set; } = new ();

        public CollectionWorkSpace()
        {

        }

        public CollectionWorkSpace(List<T> list)
        {
            List = list;
        }

        public override object Clone()
        {
            return new CollectionWorkSpace<T>(List.ToList());
        }

        public IEnumerator<T> GetEnumerator()
        {
            return List.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
