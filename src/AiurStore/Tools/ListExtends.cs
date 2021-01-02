using System.Collections.Generic;

namespace AiurStore.Tools
{
    public static class ListExtends
    {
        public static IEnumerable<T> YieldAfter<T>(this LinkedListNode<T> node)
        {
            while (node != null)
            {
                yield return node.Value;
                node = node.Next;
            }
        }
    }
}
