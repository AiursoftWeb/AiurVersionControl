namespace Aiursoft.AiurEventSyncer.Tools
{
    public class SafeQueue<T>
    {
        private readonly Queue<T> _queue = new();

        public void Enqueue(T item)
        {
            lock (this)
            {
                _queue.Enqueue(item);
            }
        }

        public T Dequeue()
        {
            T item;
            lock (this)
            {
                item = _queue.Dequeue();
            }
            return item;
        }

        public bool Any()
        {
            bool any;
            lock (this)
            {
                any = _queue.Any();
            }
            return any;
        }
    }
}