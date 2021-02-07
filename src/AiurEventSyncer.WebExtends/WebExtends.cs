using AiurEventSyncer.Abstract;
using AiurEventSyncer.ConnectionProviders.Models;
using AiurEventSyncer.Models;
using AiurEventSyncer.Tools;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
                async Task pushEvent(List<Commit<T>> newCommits)
                {
                    // Broadcast new commits.
                    await ws.SendObject(newCommits);
                }
                var connectionId = Guid.NewGuid();
                repository.RegisterAsyncTask(connectionId, pushEvent);
                while (ws.State == WebSocketState.Open)
                {
                    try
                    {
                        // Waitting for pushed commits.
                        var pushedCommits = await ws.GetObject<PushModel<T>>();
                        repository.OnPushed(pushedCommits.Commits, pushedCommits.Start);
                    }
                    catch
                    {
                        break;
                    }
                }
                repository.UnRegister(connectionId);
                return new EmptyResult();
            }
            else
            {
                return new BadRequestResult();
            }
        }
    }
}
