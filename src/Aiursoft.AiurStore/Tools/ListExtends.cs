namespace Aiursoft.AiurStore.Tools
{
    public static class ListExtends
    {
        public static IEnumerable<T> YieldAfter<T>(LinkedListNode<T> node)
        {
            return YieldFrom(node.Next);
        }

        private static IEnumerable<T> YieldFrom<T>(LinkedListNode<T> node)
        {
            while (node != null)
            {
                yield return node.Value;
                node = node.Next;
            }
        }
    }
}
