using System.Collections.Concurrent;
using Aiursoft.AiurEventSyncer.Abstract;
using Aiursoft.AiurEventSyncer.ConnectionProviders.Models;
using Aiursoft.AiurEventSyncer.Models;
using Aiursoft.AiurEventSyncer.Remotes;
using Aiursoft.AiurObserver;
using Aiursoft.AiurObserver.Extensions;
using Aiursoft.AiurObserver.WebSocket.Server;
using Aiursoft.AiurStore.Tools;
using Aiursoft.ArrayDb.ObjectBucket;
using Aiursoft.ArrayDb.Partitions;
using Aiursoft.CSTools.Tools;
using Aiursoft.WebTools;
using Aiursoft.WebTools.Abstractions.Models;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.EventSyncerWithArrayDbServer.Tests;

[TestClass]
public class ArrayDbAsWebServerTests
{
    private readonly int _port = Network.GetAvailablePort();
    
    private async Task<WebApplication> BuildServer()
    {
        if (Directory.Exists("my-db"))
        {
            Directory.Delete("my-db", true);
        }
        Directory.CreateDirectory("my-db");
        
        return await Extends.AppAsync<ServerStartUp>(
            args: Array.Empty<string>(),
            port: _port);
    }

    [TestMethod]
    public async Task TestPush()
    {
        // Prepare server.
        var server = await BuildServer();
        await server.StartAsync();
        
        // Prepare client 1.
        var endPoint = $"ws://localhost:{_port}/Channel/1/";
        var repo = new Repository<ChatMessage>();
        await new WebSocketRemote<ChatMessage>(endPoint + $"{Guid.NewGuid()}")
            .AttachAsync(repo);

        // Prepare client 2.
        var repo2 = new Repository<ChatMessage>();
        var ws2 = await new WebSocketRemote<ChatMessage>(endPoint + $"{Guid.NewGuid()}")
            .AttachAsync(repo2);
        
        // Client 1 takes action.
        repo.Commit(new ChatMessage
        {
            ThreadId = 1,
            Content = "Hello, world!",
            SenderId = Guid.NewGuid()
        });

        // Reflect to client 2.
        await Task.Delay(1000);
        Assert.AreEqual(1, repo2.Commits.Count);
        Assert.AreEqual("Hello, world!", repo2.Head.Item.Content);
        
        // Prepare client 3.
        var repo3 = new Repository<ChatMessage>();
        await new WebSocketRemote<ChatMessage>(endPoint + $"{Guid.NewGuid()}")
            .AttachAsync(repo3);
        
        // Client 3 gets the message.
        await Task.Delay(1000);
        Assert.AreEqual(1, repo3.Commits.Count);
        Assert.AreEqual("Hello, world!", repo3.Head.Item.Content);
        
        // Client 2 drop.
        await ws2.DetachAsync();
        
        // Client 3 takes action.
        repo3.Commit(new ChatMessage
        {
            ThreadId = 1,
            Content = "Hello, world! 2",
            SenderId = Guid.NewGuid()
        });
        
        // Reflect to client 1.
        await Task.Delay(1000);
        Assert.AreEqual(2, repo.Commits.Count);
        Assert.AreEqual("Hello, world! 2", repo.Head.Item.Content);
        
        // Not reflect to client 2.
        Assert.AreEqual(1, repo2.Commits.Count);
        
        // Client 2 commit locally
        repo2.Commit(new ChatMessage
        {
            ThreadId = 1,
            Content = "Hello, world! 3",
            SenderId = Guid.NewGuid()
        });
        
        // Not reflect to client 1.
        await Task.Delay(1000);
        Assert.AreEqual(2, repo.Commits.Count);
        
        // Client 2 reconnect.
        await ws2.AttachAsync(repo2);
        
        // All has 3 messages: Hw, Hw2, Hw3
        await Task.Delay(1000);
        Assert.AreEqual(3, repo.Commits.Count);
        Assert.AreEqual(3, repo2.Commits.Count);
        Assert.AreEqual(3, repo3.Commits.Count);
        Assert.AreEqual("Hello, world! 3", repo.Head.Item.Content);
        Assert.AreEqual("Hello, world! 3", repo2.Head.Item.Content);
        Assert.AreEqual("Hello, world! 3", repo3.Head.Item.Content);
    }
}

public class ChatMessage
{
    /// <summary>
    /// Don't trust this on server side!!!
    /// </summary>
    public int ThreadId { get; set; } 
    
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Don't trust this on server side!!!
    /// </summary>
    public Guid SenderId { get; set; } = Guid.Empty;
}

public class ChatMessageInDb : PartitionedBucketEntity<int>
{
    [PartitionKey]
    public int ThreadId { get; set; } 
    
    [PartitionKey] 
    public override int PartitionId
    {
        get => ThreadId;
        set => ThreadId = value;
    }
    
    public string Content { get; set; } = string.Empty;
    
    public Guid SenderId { get; set; } = Guid.Empty;
    
    public string Id { get; set; } = Guid.NewGuid().ToString("D");

    public ChatMessage ToClientView()
    {
        return new ChatMessage
        {
            ThreadId = ThreadId,
            Content = Content,
            SenderId = SenderId
        };
    }
}

public class ServerStartUp : IWebStartup
{
    public void ConfigureServices(IConfiguration configuration, IWebHostEnvironment environment, IServiceCollection services)
    {
        services.AddSingleton<PartitionedObjectBucket<ChatMessageInDb, int>>(_ =>
        {
            var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "my-db");
            return new PartitionedObjectBucket<ChatMessageInDb, int>("my-db", dbPath);
        });

        services.AddSingleton<ThreadsInMemoryDb>();
        
        services
            .AddControllersWithViews()
            .AddApplicationPart(typeof(ServerController).Assembly);
    }

    public void Configure(WebApplication app)
    {
        app.UseWebSockets();
        app.UseRouting();
        app.MapDefaultControllerRoute();
    }
}


public class ThreadsInMemoryDb
{
    private ConcurrentDictionary<int, AsyncObservable<ChatMessageInDb[]>> ThreadsListenChannels { get; } = new();
    
    public AsyncObservable<ChatMessageInDb[]> GetMyChannel(int threadId)
    {
        lock (ThreadsListenChannels)
        {
            return ThreadsListenChannels.GetOrAdd(threadId, _ => new AsyncObservable<ChatMessageInDb[]>());
        }
    }
}

public class ServerController(
    ThreadsInMemoryDb memoryDb,
    PartitionedObjectBucket<ChatMessageInDb, int> messages) : ControllerBase
{
    [Route("Channel/{threadId}/{userId}")]
    [EnforceWebSocket]
    public async Task Channel([FromRoute]int threadId, [FromRoute]string userId, string start)
    {
        var userIdGuid = Guid.Parse(userId);
        var messagesDb = messages.GetPartitionById(threadId);

        // Convert start (id) to startLocation (int offset).
        var startLocation = 0;
        if (!string.IsNullOrWhiteSpace(start))
        {
            foreach (var message in messagesDb.AsEnumerable())
            {
                startLocation++;
                if (message.Id == start)
                {
                    break;
                }
            }
        }
        var readLength = messagesDb.Count - startLocation;

        var threadChannel = memoryDb.GetMyChannel(threadId);
        var socket = await HttpContext.AcceptWebSocketClient();
        if (readLength > 0)
        {
            var firstPullRequest = messagesDb.ReadBulk(startLocation, readLength);
            await socket.Send(JsonTools.Serialize(firstPullRequest.Select(t => new Commit<ChatMessage>
            {
                Item = t.ToClientView(),
                Id = t.Id,
                CommitTime = t.CreationTime
            })));
        }
        var repoSubscription = threadChannel.Subscribe(async newCommits =>
        {
            await socket.Send(JsonTools.Serialize(newCommits.Select(t => new Commit<ChatMessage>
            {
                Item = t.ToClientView(),
                Id = t.Id,
                CommitTime = t.CreationTime
            })));
        });
        var clientSubscription = socket.Subscribe(async pushedCommits =>
        {
            // Deserialize the incoming message and fill the properties.
            var model = JsonTools.Deserialize<PushModel<ChatMessage>>(pushedCommits);
            var serverTime = DateTime.UtcNow;
            var messagesToAddToDb = model.Commits
                .Select(messageIncoming => new ChatMessageInDb
                {
                    Content = messageIncoming.Item.Content,
                    Id = messageIncoming.Id,
                    PartitionId = threadId,
                    ThreadId = threadId,
                    CreationTime = serverTime,
                    SenderId = userIdGuid,
                })
                .ToArray();
            
            // Reflect to other clients.
            await threadChannel.BroadcastAsync(messagesToAddToDb);

            // Save to database.
            messagesDb.Add(messagesToAddToDb);
            await Task.CompletedTask;
        });
        
        try
        {
            await socket.Listen(HttpContext.RequestAborted);
        }
        catch (TaskCanceledException)
        {
            // Ignore. This happens when the client closes the connection.
        }
        finally
        {
            await socket.Close(HttpContext.RequestAborted);
            repoSubscription.Unsubscribe();
            clientSubscription.Unsubscribe();
        }
    }
}
