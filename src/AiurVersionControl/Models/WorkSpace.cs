using System;

namespace AiurVersionControl.Models
{
    public abstract class WorkSpace : ICloneable
    {
        public abstract object Clone();
    }
}
