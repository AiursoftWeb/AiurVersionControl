using AiurStore.Tools;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace AiurVersionControl.Models
{
    public abstract class WorkSpace : ICloneable
    {
        public abstract object Clone();
    }
}
