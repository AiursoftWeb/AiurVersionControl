using AiurStore.Tools;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace AiurVersionControl.Models
{
    public abstract class WorkSpace
    {
        public WorkSpace() { }

        public virtual WorkSpace Clone()
        {
            var stream = JsonTools.Serialize(this);
            return JsonTools.Deserialize<WorkSpace>(stream);
        }
    }
}
