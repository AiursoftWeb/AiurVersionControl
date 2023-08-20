using System;
using System.Threading.Tasks;

namespace Aiursoft.AiurEventSyncer.Tools
{
    public class TaskQueue
    {
        public Action<Exception> OnError;

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
                try
                {
                    await tasksInFlight;
                }
                catch (Exception e)
                {
                    OnError?.Invoke(e);
                }
            }
        }
    }
}
