using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
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
        public static async Task<IActionResult> BuildWebActionResultAsync<T>(this ControllerBase controller, Repository<T> repository)
        {
            var context = controller.HttpContext;
            var request = context.Request;
            var method = request.Query["method"];
            //var mockRemote = new ObjectRemote<T>(repository) { Name = "Server-side-fake-repo" };

            //if (request.Method == "POST" && method == "syncer-push")
            //{
            //    string startPosition = request.Query[nameof(startPosition)];
            //    var jsonForm = await new StreamReader(request.Body).ReadToEndAsync();
            //    var formObject = JsonSerializer.Deserialize<List<Commit<T>>>(jsonForm, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            //    Console.WriteLine("[SERVER]: I was pushed!");
            //    await mockRemote.UploadFromAsync(startPosition, formObject);
            //    return controller.Ok();
            //}
            //else if (request.Method == "GET" && method == "syncer-pull")
            //{
            //    string localPointerPosition = request.Query[nameof(localPointerPosition)];
            //    Console.WriteLine("[SERVER]: I was pulled!");
            //    var pullResult = await mockRemote.DownloadFromAsync(localPointerPosition);
            //    return controller.Ok(pullResult);
            //}
            if (context.WebSockets.IsWebSocketRequest)
            {
                var ws = await context.WebSockets.AcceptWebSocketAsync();
                Console.WriteLine($"[SERVER]: New Websocket client online! Status: '{ws.State}'");
                repository.OnNewCommit += async (commit) =>
                {
                    // Broadcast new commits.
                    Console.WriteLine("[SERVER]: I was changed! Broadcasting to a remote...");
                    await SendMessage(ws, JsonSerializer.Serialize(commit, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
                };
                while (ws.State == WebSocketState.Open)
                {
                    // Waitting for pushed commits.
                    var rawJson = await GetMessage(ws);
                    Console.WriteLine("[SERVER]: I got a new push request.");
                    var pushedCommits = JsonSerializer.Deserialize<PushModel<T>>(rawJson, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    await repository.OnPushed(pushedCommits.Start, pushedCommits.Commits);
                }
                Console.WriteLine($"[SERVER]: Websocket dropped! Reason: '{ws.State}'");
                return controller.Ok();
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
            throw new InvalidOperationException();
        }
    }
}
