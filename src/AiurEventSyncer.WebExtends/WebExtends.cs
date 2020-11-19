using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
                var formObject = JsonConvert.DeserializeObject<List<Commit<T>>>(jsonForm);
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

            }
            else
            {
                return new BadRequestResult();
            }
            return null;
        }
    }
}
