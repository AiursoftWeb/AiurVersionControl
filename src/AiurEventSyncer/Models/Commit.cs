using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiurEventSyncer.Models
{
    public class Commit<T>
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("D");
        public T Item { get; set; }
    }
}
