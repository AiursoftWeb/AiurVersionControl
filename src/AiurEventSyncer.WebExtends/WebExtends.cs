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
            var mockRemote = new ObjectRemote<T>(repository);

            if (request.Method == "POST" && method == "syncer-push")
            {
                string startPosition = request.Query[nameof(startPosition)];
                var jsonForm = await new StreamReader(request.Body).ReadToEndAsync();
                var formObject = JsonSerializer.Deserialize<List<Commit<T>>>(jsonForm, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var uploadResult = await mockRemote.UploadFromAsync(startPosition, formObject);
                return controller.Ok(uploadResult);
            }
            else if (request.Method == "GET" && method == "syncer-pull")
            {
                string localPointerPosition = request.Query[nameof(localPointerPosition)];
                var pullResult = await mockRemote.DownloadFromAsync(localPointerPosition);
                return controller.Ok(pullResult);
            }
            else if (context.WebSockets.IsWebSocketRequest)
            {
                var ws = await context.WebSockets.AcceptWebSocketAsync();
                async Task pushTask()
                {
                    await SendMessage(ws, "updated");
                }
                mockRemote.OnRemoteChanged += pushTask;
                while (ws.State == WebSocketState.Open)
                {
                    await Task.Delay(5000);
                }
                mockRemote.OnRemoteChanged -= pushTask;
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
    }
}
