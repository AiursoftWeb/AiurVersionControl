using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AiurEventSyncer.Tools
{
    public class TaskQueue 
    {
        private readonly SafeQueue<Func<Task>> _pendingTaskFactories = new SafeQueue<Func<Task>>();
        private readonly object loc = new object();
        private readonly int _depth;
        private Task _engine = Task.CompletedTask;

        public TaskQueue(int depth)
        {
            _depth = depth;
        }

        public void QueueNew(Func<Task> taskFactory)
        {
            _pendingTaskFactories.Enqueue(taskFactory);
            Task.Factory.StartNew(() =>
            {
                lock (loc)
                {
                    if (_engine.IsCompleted)
                    {
                        _engine = RunTasksInQueue();
                    }
                }
            });
        }

        private async Task RunTasksInQueue()
        {
            var tasksInFlight = new List<Task>(_depth);
            while (_pendingTaskFactories.Any())
            {
                while (tasksInFlight.Count < _depth && _pendingTaskFactories.Any())
                {
                    Func<Task> taskFactory = _pendingTaskFactories.Dequeue();
                    tasksInFlight.Add(taskFactory());
                }
                var completedTask = await Task.WhenAny(tasksInFlight).ConfigureAwait(false);
                await completedTask.ConfigureAwait(false);
                tasksInFlight.Remove(completedTask);
            }
        }
    }

    public class SafeQueue<T>
    {
        private readonly Queue<T> queue = new Queue<T>();
        private readonly object loc = new object();

        public void Enqueue(T item)
        {
            lock (loc)
            {
                queue.Enqueue(item);
            }
        }

        public T Dequeue()
        {
            T item = default;
            lock (loc)
            {
                item = queue.Dequeue();
            }
            return item;
        }

        public bool Any()
        {
            bool any = false;
            lock (loc)
            {
                any = queue.Any();
            }
            return any;
        }
    }
}
