using Aiursoft.AiurEventSyncer.Abstract;
using Aiursoft.AiurEventSyncer.ConnectionProviders.Models;
using Aiursoft.AiurEventSyncer.Models;
using Aiursoft.AiurEventSyncer.Tools;
using Aiursoft.AiurObserver;
using Aiursoft.AiurStore.Tools;
using Microsoft.AspNetCore.Http;
using Aiursoft.WebTools.Services;

namespace Aiursoft.AiurEventSyncer.WebExtends
{
    public static class ActionBuilder
    {
        public static async Task RepositoryAsync<T>(this HttpContext context, Repository<T> repository, string startPosition)
        {
            var socket = await context.AcceptWebSocketClient();
            var firstPullResult = repository.Commits.GetCommitsAfterId<Commit<T>, T>(startPosition).ToList();
            await socket.Send(JsonTools.Serialize(firstPullResult));
            var repoSubscription = repository.AppendCommitsHappened.Subscribe(async newCommits =>
            {
                await socket.Send(JsonTools.Serialize(newCommits));
            });
            var clientSubscription = socket.Subscribe(async pushedCommits =>
            {
                var model = JsonTools.Deserialize<PushModel<T>>(pushedCommits);
                repository.OnPushed(model.Commits, model.Start);
                await Task.CompletedTask;
            });
           
            await socket.Listen(context.RequestAborted);
            repoSubscription.Unsubscribe();
            clientSubscription.Unsubscribe();
            
            // var websocket = context.WebSockets;
            // if (websocket.IsWebSocketRequest)
            // {
            //     var ws = await websocket.AcceptWebSocketAsync();
            //     // Send pull result.
            //     await ws.SendObject(firstPullResult);
            //     var subscription = repository.AppendCommitsHappened.Subscribe(async newCommits =>
            //     {
            //         await ws.SendObject(newCommits);
            //     });
            //     await ws.Monitor<PushModel<T>>(onNewObject: pushedCommits => 
            //     {
            //         repository.OnPushed(pushedCommits.Commits, pushedCommits.Start);
            //         return Task.CompletedTask;
            //     });
            //     subscription.UnRegister();
            // }
        }
    }
}
