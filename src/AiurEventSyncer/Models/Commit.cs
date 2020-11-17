using System;

namespace AiurEventSyncer.Models
{
    public class Commit<T>
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("D");
        public T Item { get; set; }
    }
}
