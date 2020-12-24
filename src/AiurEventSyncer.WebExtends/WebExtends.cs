using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes.Models;
using AiurEventSyncer.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace AiurEventSyncer.WebExtends
{
    public class ActionBuilder
    {
        public async Task<IActionResult> BuildWebActionResultAsync<T>(WebSocketManager websocket, Repository<T> repository, string startPosition)
        {
            if (websocket.IsWebSocketRequest)
            {
                var ws = await websocket.AcceptWebSocketAsync();
                // Send pull result.
                var firstPullResult = repository.Commits.AfterCommitId(startPosition).ToList();
                await ws.SendObject(firstPullResult);
                async Task pushEvent(List<Commit<T>> newCommits)
                {
                    // Broadcast new commits.
                    await ws.SendObject(newCommits.Where(t => !firstPullResult.Any(p => p.Id == t.Id)));
                }
                var connectionId = Guid.NewGuid();
                repository.OnNewCommitsSubscribers[connectionId]= pushEvent;
                while (ws.State == WebSocketState.Open)
                {
                    try
                    {
                        // Waitting for pushed commits.
                        var pushedCommits = await ws.GetObject<PushModel<T>>();
                        await repository.OnPushed(pushedCommits.Commits, pushedCommits.Start);
                    }
                    catch (WebSocketException)
                    {
                        break;
                    }
                }
                repository.OnNewCommitsSubscribers.TryRemove(connectionId, out _);
                return new EmptyResult();
            }
            else
            {
                return new BadRequestResult();
            }
        }
    }
}
