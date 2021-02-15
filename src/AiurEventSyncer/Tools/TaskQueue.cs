using System;
using System.Threading.Tasks;

namespace AiurEventSyncer.Tools
{
    public class TaskQueue
    {
        private readonly SafeQueue<Func<Task>> _pendingTaskFactories = new();
        private Task _engine = Task.CompletedTask;

        public void QueueNew(Func<Task> taskFactory)
        {
            _pendingTaskFactories.Enqueue(taskFactory);
            Task.Factory.StartNew(() =>
            {
                lock (this)
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
            var tasksInFlight = Task.CompletedTask;
            while (_pendingTaskFactories.Any())
            {
                while (tasksInFlight.IsCompleted && _pendingTaskFactories.Any())
                {
                    var taskFactory = _pendingTaskFactories.Dequeue();
                    tasksInFlight = taskFactory();
                }
                await tasksInFlight;
            }
        }
    }
}
