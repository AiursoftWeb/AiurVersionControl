using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiurEventSyncer.Tests.Models
{
    public class Book
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("D");
        public string Name { get; set; }
    }
}
