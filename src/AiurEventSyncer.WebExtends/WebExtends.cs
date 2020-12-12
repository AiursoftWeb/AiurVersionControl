using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
using AiurEventSyncer.Tools;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AiurEventSyncer.WebExtends
{
    public static class WebExtends
    {
        public static async Task<IActionResult> BuildWebActionResultAsync<T>(this ControllerBase controller, Repository<T> repository, string startPosition)
        {
            if (controller.HttpContext.WebSockets.IsWebSocketRequest)
            {
                var ws = await controller.HttpContext.WebSockets.AcceptWebSocketAsync();
                Console.WriteLine($"[SERVER]: New Websocket client online! Status: '{ws.State}'");
                // Send pull result.
                var pullResult = repository.Commits.AfterCommitId(startPosition).ToList();
                await SendMessage(ws, JsonSerializer.Serialize(pullResult, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
                Func<List<Commit<T>>, Task> pushEvent = async (List<Commit<T>> newCommits) =>
                {
                    // Broadcast new commits.
                    Console.WriteLine($"[SERVER]: I was changed with: {string.Join(',', newCommits.Select(t => t.Item.ToString()))}! Broadcasting to a remote...");
                    await SendMessage(ws, JsonSerializer.Serialize(newCommits, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
                };
                var key = DateTime.UtcNow;
                repository.OnNewCommitsSubscribers[key]= pushEvent;
                Console.WriteLine($"[SERVER] New Websocket subscriber registered! Current registers: {repository.OnNewCommitsSubscribers.Count}.");
                while (ws.State == WebSocketState.Open)
                {
                    // Waitting for pushed commits.
                    var rawJson = await GetMessage(ws);
                    var pushedCommits = JsonSerializer.Deserialize<PushModel<T>>(rawJson, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    Console.WriteLine($"[SERVER]: I got a new push request with commits: {string.Join(',', pushedCommits.Commits.Select(t => t.Item.ToString()))}.");
                    await repository.OnPushed(pushedCommits.Commits, pushedCommits.Start);
                }
                Console.WriteLine($"[SERVER]: Websocket dropped! Reason: '{ws.State}'");
                repository.OnNewCommitsSubscribers.TryRemove(key, out _);
                return new EmptyResult();
            }
            else
            {
                return new BadRequestResult();
            }
        }

        public static async Task SendMessage(WebSocket ws, string message)
        {
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            await ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public static async Task<string> GetMessage(WebSocket ws)
        {
            var buffer = new ArraySegment<byte>(new byte[2048]);
            var wsResult = await ws.ReceiveAsync(buffer, CancellationToken.None);
            if (wsResult.MessageType == WebSocketMessageType.Text)
            {
                var rawJson = Encoding.UTF8.GetString(buffer.Skip(buffer.Offset).Take(buffer.Count).ToArray()).Trim('\0').Trim();
                return rawJson;

            }
            else if (wsResult.MessageType == WebSocketMessageType.Close)
            {
                return "[]";
            }
            else
            {
                throw new InvalidOperationException($"{wsResult.MessageType} is an invalid stage!");
            }
        }
    }
}
