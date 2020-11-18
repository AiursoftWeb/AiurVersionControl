using System;
using System.Collections.Generic;

namespace AiurEventSyncer.Tools
{
    public static class ListExtends
    {
        public static IEnumerable<T> After<T>(this IEnumerable<T> source, Func<T, bool> func)
        {
            var yielding = false;
            foreach (var item in source)
            {
                if (yielding)
                {
                    yield return item;
                }
                if (func(item))
                {
                    yielding = true;
                }
            }
        }
    }
}
