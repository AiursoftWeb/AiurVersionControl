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
                Console.WriteLine($"[SERVER]: New Websocket client online! Status: '{ws.State}'");
                // Send pull result.
                var firstPullResult = repository.Commits.AfterCommitId(startPosition).ToList();
                await ws.SendObject(firstPullResult);
                async Task pushEvent(List<Commit<T>> newCommits)
                {
                    // Broadcast new commits.
                    Console.WriteLine($"[SERVER]: I was changed with: {string.Join(',', newCommits.Select(t => t.Item.ToString()))}! Broadcasting to a remote...");
                    await ws.SendObject(newCommits.Where(t => !firstPullResult.Any(p => p.Id == t.Id)));
                }
                var connectionId = Guid.NewGuid();
                repository.OnNewCommitsSubscribers[connectionId]= pushEvent;
                Console.WriteLine($"[SERVER] New Websocket subscriber registered! Current registers: {repository.OnNewCommitsSubscribers.Count}.");
                while (ws.State == WebSocketState.Open)
                {
                    try
                    {
                        // Waitting for pushed commits.
                        var pushedCommits = await ws.GetObject<PushModel<T>>();
                        Console.WriteLine($"[SERVER]: I got a new push request with commits: {string.Join(',', pushedCommits.Commits.Select(t => t.Item.ToString()))}.");
                        await repository.OnPushed(pushedCommits.Commits, pushedCommits.Start);
                    }
                    catch (WebSocketException)
                    {
                        break;
                    }
                }
                Console.WriteLine($"[SERVER]: Websocket dropped! Reason: '{ws.State}'");
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
