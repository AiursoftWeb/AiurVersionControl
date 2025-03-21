﻿using Aiursoft.AiurEventSyncer.Abstract;
using Aiursoft.AiurEventSyncer.ConnectionProviders.Models;
using Aiursoft.AiurEventSyncer.Models;
using Aiursoft.AiurEventSyncer.Tools;
using Aiursoft.AiurObserver.WebSocket.Server;
using Aiursoft.AiurObserver;
using Aiursoft.AiurStore.Tools;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;

namespace Aiursoft.AiurEventSyncer.WebExtends
{
    public static class ActionBuilder
    {
        public static async Task RepositoryAsync<T>(this HttpContext context, Repository<T> repository, string startPosition)
        {
            var socket = await context.AcceptWebSocketClient();
            var firstPullResult = repository.Commits.GetCommitsAfterId<Commit<T>, T>(startPosition).ToList();
            await socket.Send(JsonTools.Serialize(firstPullResult));
            var repoSubscription = repository.AppendCommitsHappened.Subscribe(async newCommits =>
            {
                await socket.Send(JsonTools.Serialize(newCommits));
            });
            var clientSubscription = socket.Subscribe(async pushedCommits =>
            {
                var model = JsonTools.Deserialize<PushModel<T>>(pushedCommits);
                repository.OnPushed(model.Commits);
                await Task.CompletedTask;
            });
           
            try
            {
                await socket.Listen(context.RequestAborted);
            }
            catch (TaskCanceledException)
            {
                // Ignore. This happens when the client closes the connection.
            }
            catch (ConnectionAbortedException)
            {
                // Ignore. This happens when the client closes the connection.
            }
            finally
            {
                repoSubscription.Unsubscribe();
                clientSubscription.Unsubscribe();
                if (socket.Connected)
                {
                    await socket.Close(context.RequestAborted);
                }
            }
        }
    }
}
