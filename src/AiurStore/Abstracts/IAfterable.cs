using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiurStore.Abstracts
{
    public interface IAfterable<T> : IEnumerable<T>
    {
        IEnumerable<T> After(Func<T, bool> func);
    }
}
