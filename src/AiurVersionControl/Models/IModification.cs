using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AiurVersionControl.Models
{
    public interface IModification<T> where T: WorkSpace
    {
        void Apply(T workspace);
    }
}
