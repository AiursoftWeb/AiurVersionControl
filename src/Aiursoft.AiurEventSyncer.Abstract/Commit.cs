using System;

namespace Aiursoft.AiurEventSyncer.Abstract
{
    /// <summary>
    /// An object that can be synced between repositories and contains an inner object.
    /// </summary>
    /// <typeparam name="T">The type of the inner object.</typeparam>
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
