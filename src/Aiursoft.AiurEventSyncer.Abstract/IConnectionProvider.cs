﻿namespace Aiursoft.AiurEventSyncer.Abstract
{
    public interface IConnectionProvider<T>
    {
        event Action OnReconnecting;

        Task Disconnect();

        Task PullAndMonitor(
            Func<List<Commit<T>>, Task> onData, 
            Func<string> startPositionFactory,
            Func<Task> onConnected, 
            bool monitorInCurrentThread);

        /// <summary>
        /// Upload the commits to remote.
        /// </summary>
        /// <param name="commits">Commits</param>
        /// <returns>If successfully uploaded.</returns>
        Task<bool> Upload(List<Commit<T>> commits);

        Task<List<Commit<T>>> Download(string pointer);
    }
}
