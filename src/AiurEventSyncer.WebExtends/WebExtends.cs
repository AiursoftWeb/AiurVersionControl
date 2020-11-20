using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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
                var uploadResult = mockRemote.UploadFrom(startPosition, formObject);
                return controller.Ok(uploadResult);
            }
            else if (request.Method == "GET" && method == "syncer-pull")
            {
                string localPointerPosition = request.Query[nameof(localPointerPosition)];
                var pullResult = mockRemote.DownloadFrom(localPointerPosition).ToList();
                return controller.Ok(pullResult);
            }
            else if (context.WebSockets.IsWebSocketRequest)
            {
                return null;
            }
            else
            {
                return new BadRequestResult();
            }
        }
    }
}
