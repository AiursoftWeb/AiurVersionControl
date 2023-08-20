using Aiursoft.AiurEventSyncer.Abstract;
using Aiursoft.AiurEventSyncer.ConnectionProviders.Models;
using Aiursoft.AiurEventSyncer.Models;
using Aiursoft.AiurEventSyncer.Tools;
using AiurObserver;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Aiursoft.AiurEventSyncer.WebExtends
{
    public static class ActionBuilder
    {
        public static async Task<IActionResult> RepositoryAsync<T>(this HttpContext context, Repository<T> repository, string startPosition)
        {
            var websocket = context.WebSockets;
            if (websocket.IsWebSocketRequest)
            {
                var ws = await websocket.AcceptWebSocketAsync();
                // Send pull result.
                var firstPullResult = repository.Commits.GetCommitsAfterId<Commit<T>, T>(startPosition).ToList();
                await ws.SendObject(firstPullResult);
                var subscription = repository.AppendCommitsHappened.Subscribe(async newCommits =>
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

            return new BadRequestResult();
        }
    }
}
