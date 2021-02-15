using System.Collections.Generic;
using System.Linq;

namespace AiurEventSyncer.Tools
{
    public class SafeQueue<T>
    {
        private readonly Queue<T> queue = new Queue<T>();

        public void Enqueue(T item)
        {
            lock (this)
            {
                queue.Enqueue(item);
            }
        }

        public T Dequeue()
        {
            T item = default;
            lock (this)
            {
                item = queue.Dequeue();
            }
            return item;
        }

        public bool Any()
        {
            bool any = false;
            lock (this)
            {
                any = queue.Any();
            }
            return any;
        }
    }
}