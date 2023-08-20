using Aiursoft.AiurVersionControl.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Aiursoft.AiurVersionControl.CRUD
{
    /// <summary>
    /// A special workspace which contains a collection with item type is T.
    /// </summary>
    /// <typeparam name="T">The type of the item in the collection.</typeparam>
    public class CollectionWorkSpace<T> : WorkSpace, IEnumerable<T>
    {
        public List<T> List { get; init; } = new ();
        public T this[int index] => List[index];

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
