using System;
using System.Collections.Generic;

namespace AiurStore.Abstracts
{
    public interface IAfterable<T> : IEnumerable<T>
    {
        IEnumerable<T> After(Func<T, bool> func);
    }
}
