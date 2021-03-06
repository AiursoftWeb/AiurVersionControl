using AiurEventSyncer.Abstract;
using AiurEventSyncer.ConnectionProviders.Models;
using AiurEventSyncer.Models;
using AiurEventSyncer.Tools;
using AiurObserver;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace AiurEventSyncer.WebExtends
{
    public static class ActionBuilder
    {
        public static async Task<IActionResult> RepositoryAsync<T>(this ControllerBase controller, Repository<T> repository, string startPosition)
        {
            var websocket = controller.HttpContext.WebSockets;
            if (websocket.IsWebSocketRequest)
            {
                var ws = await websocket.AcceptWebSocketAsync();
                // Send pull result.
                var firstPullResult = repository.Commits.GetCommitsAfterId<Commit<T>, T>(startPosition).ToList();
                await ws.SendObject(firstPullResult);
                var connectionId = Guid.NewGuid();
                var subscription = repository.AppendCommitsHappened.Subscribe(async (newCommits) =>
                {
                    await ws.SendObject(newCommits);
                });
                await ws.Monitor<PushModel<T>>(onNewObject: pushedCommits => 
                {
                    repository.OnPushed(pushedCommits.Commits, pushedCommits.Start);
                    return Task.CompletedTask;
                });
                subscription.Dispose();
                return new EmptyResult();
            }
            else
            {
                return new BadRequestResult();
            }
        }
    }
}
