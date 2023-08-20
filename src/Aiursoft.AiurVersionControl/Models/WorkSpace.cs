using System;

namespace Aiursoft.AiurVersionControl.Models
{
    /// <summary>
    /// A class which must inert to be version controlled in a controlled repository.
    /// </summary>
    public abstract class WorkSpace : ICloneable
    {
        public abstract object Clone();
    }
}
