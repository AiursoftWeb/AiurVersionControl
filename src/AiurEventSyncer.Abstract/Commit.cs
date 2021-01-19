using System;

namespace AiurEventSyncer.Abstract
{
    public class Commit<T>
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("D");
        public T Item { get; set; }
        public DateTime CommitTime { get; set; } = DateTime.UtcNow;

        public override string ToString()
        {
            return $"{Item}";
        }
    }
}
