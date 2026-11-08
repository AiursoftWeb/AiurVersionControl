namespace Aiursoft.AiurEventSyncer.Tools
{
    public class TaskQueue
    {
        public Action<Exception> OnError;

        private readonly object _queueLock = new();
        private Task _engine = Task.CompletedTask;

        public void QueueNew(Func<Task> taskFactory)
        {
            lock (_queueLock)
            {
                var previous = _engine;
                // Publish the completion task before a notification can start.
                _engine = Task.Run(async () =>
                {
                    await previous;
                    try
                    {
                        await taskFactory();
                    }
                    catch (Exception e)
                    {
                        OnError?.Invoke(e);
                    }
                });
            }
        }

        public async Task WaitAsync()
        {
            while (true)
            {
                Task completion;
                lock (_queueLock)
                {
                    completion = _engine;
                }
                await completion;
                lock (_queueLock)
                {
                    if (completion == _engine)
                    {
                        return;
                    }
                }
            }
        }
    }
}
