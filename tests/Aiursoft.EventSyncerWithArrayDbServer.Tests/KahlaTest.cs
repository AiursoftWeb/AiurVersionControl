using System.Collections.Concurrent;
using Aiursoft.AiurEventSyncer.Abstract;
using Aiursoft.AiurEventSyncer.ConnectionProviders.Models;
using Aiursoft.AiurEventSyncer.Models;
using Aiursoft.AiurEventSyncer.Remotes;
using Aiursoft.AiurObserver;
using Aiursoft.AiurObserver.WebSocket;
using Aiursoft.AiurObserver.WebSocket.Server;
using Aiursoft.AiurStore.Tools;
using Aiursoft.ArrayDb.ObjectBucket;
using Aiursoft.ArrayDb.Partitions;
using Aiursoft.CSTools.Tools;
using Aiursoft.InMemoryKvDb;
using Aiursoft.InMemoryKvDb.AutoCreate;
using Aiursoft.WebTools;
using Aiursoft.WebTools.Abstractions.Models;
using Aiursoft.WebTools.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        var endPoint = $"ws://localhost:{_port}/Channel/114514/";
        var repo1 = new Repository<ChatMessage>();
        var client1UserId = Guid.NewGuid();
        // ReSharper disable once UnusedVariable
        var ws1 = await new WebSocketRemote<ChatMessage>(endPoint + $"{client1UserId}")
            .AttachAsync(repo1);

        // Prepare client 2.
        var repo2 = new Repository<ChatMessage>();
        var client2UserId = Guid.NewGuid();
        var ws2 = await new WebSocketRemote<ChatMessage>(endPoint + $"{client2UserId}")
            .AttachAsync(repo2);

        // Client 1 takes action.
        repo1.Commit(new ChatMessage
        {
            Content = "Hello, world!",
            SenderId = client1UserId
        });

        // Reflect to client 2.
        await Task.Delay(1500);
        Assert.AreEqual(1, repo2.Commits.Count);
        Assert.AreEqual("Hello, world!", repo2.Head.Item.Content);

        // Prepare client 3.
        var repo3 = new Repository<ChatMessage>();
        var client3UserId = Guid.NewGuid();
        await new WebSocketRemote<ChatMessage>(endPoint + $"{client3UserId}")
            .AttachAsync(repo3);

        // Client 3 gets the message.
        await Task.Delay(1500);
        Assert.AreEqual(1, repo3.Commits.Count);
        Assert.AreEqual("Hello, world!", repo3.Head.Item.Content);

        // Client 2 drop.
        await ws2.DetachAsync();

        // Client 3 takes action.
        repo3.Commit(new ChatMessage
        {
            Content = "Hello, world! 2",
            SenderId = client3UserId
        });

        // Reflect to client 1.
        await Task.Delay(1500);
        Assert.AreEqual(2, repo1.Commits.Count);
        Assert.AreEqual("Hello, world! 2", repo1.Head.Item.Content);

        // Not reflect to client 2.
        Assert.AreEqual(1, repo2.Commits.Count);

        // Client 2 commit locally (Not sync to server).
        repo2.Commit(new ChatMessage
        {
            Content = "Hello, world! 3",
            SenderId = client2UserId
        });

        // Not reflect to client 1 and client 3.
        await Task.Delay(1500);
        Assert.AreEqual(2, repo1.Commits.Count);
        Assert.AreEqual(2, repo2.Commits.Count);
        Assert.AreEqual(2, repo3.Commits.Count);
        Assert.AreEqual("Hello, world! 2", repo1.Head.Item.Content);
        Assert.AreEqual("Hello, world! 3", repo2.Head.Item.Content);
        Assert.AreEqual("Hello, world! 2", repo3.Head.Item.Content);
        
        // Client 2 reconnect.
        await ws2.AttachAsync(repo2);

        // All has 3 messages: Hw, Hw2, Hw3
        await Task.Delay(1500);
        Assert.AreEqual(3, repo1.Commits.Count);
        Assert.AreEqual(3, repo2.Commits.Count);
        Assert.AreEqual(3, repo3.Commits.Count);
        Assert.AreEqual("Hello, world! 3", repo1.Head.Item.Content);
        Assert.AreEqual("Hello, world! 3", repo2.Head.Item.Content);
        Assert.AreEqual("Hello, world! 3", repo3.Head.Item.Content);
        Assert.AreEqual(client2UserId, repo1.Head.Item.SenderId);
        Assert.AreEqual(client2UserId, repo2.Head.Item.SenderId);
        Assert.AreEqual(client2UserId, repo3.Head.Item.SenderId);
    }
}

public class ChatMessage
{
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Don't trust this on server side!!!
    /// </summary>
    public Guid SenderId { get; set; } = Guid.Empty;
}

public class ChatMessageInDb : PartitionedBucketEntity<int>
{
    [PartitionKey] public int ThreadId { get; set; }

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
            Content = Content,
            SenderId = SenderId
        };
    }
}

public class ServerStartUp : IWebStartup
{
    public void ConfigureServices(IConfiguration configuration, IWebHostEnvironment environment,
        IServiceCollection services)
    {
        services.AddSingleton<PartitionedObjectBucket<ChatMessageInDb, int>>(_ =>
        {
            var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "my-db");
            return new PartitionedObjectBucket<ChatMessageInDb, int>("my-db", dbPath);
        });

        services.AddSingleton<ThreadsInMemoryDb>();
        services.AddSingleton<LocksDb>();
        services.AddNamedLruMemoryStore<ReaderWriterLockSlim, int>(
            onNotFound: _ => new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion),
            maxCachedItemsCount: 32768);

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

    public AsyncObservable<ChatMessageInDb[]> GetThreadNewMessagesChannel(int threadId)
    {
        lock (ThreadsListenChannels)
        {
            return ThreadsListenChannels.GetOrAdd(threadId, _ => new AsyncObservable<ChatMessageInDb[]>());
        }
    }
}

public class LocksDb(NamedLruMemoryStoreProvider<ReaderWriterLockSlim, int> memoryStoreProvider)
{
    public ReaderWriterLockSlim GetThreadMessagesLock(int threadId)
    {
        return memoryStoreProvider.GetStore("ThreadMessagesLock").GetOrAdd(threadId);
    }
}

public class ServerController(
    ILogger<ServerController> logger,
    LocksDb locks,
    ThreadsInMemoryDb memoryDb,
    PartitionedObjectBucket<ChatMessageInDb, int> messages) : ControllerBase
{
    [Route("Channel/{threadId}/{userId}")]
    [EnforceWebSocket]
    public async Task Channel([FromRoute] int threadId, [FromRoute] string userId, string start)
    {
        var messagesDb = messages.GetPartitionById(threadId);
        var threadReflector = memoryDb.GetThreadNewMessagesChannel(threadId);
        var lockObject = locks.GetThreadMessagesLock(threadId);
        
        logger.LogInformation("User with ID: {UserId} is trying to connect to thread {ThreadId}.", userId, threadId);
        var socket = await HttpContext.AcceptWebSocketClient();

        ISubscription? reflectorSubscription;
        ISubscription? clientSubscription;
        
        var clientPushConsumer = new ClientPushConsumer(
            logger,
            lockObject,
            Guid.Parse(userId),
            threadReflector,
            messagesDb);
        var reflectorConsumer = new ThreadReflectConsumer(
            logger,
            socket);
        
        lockObject.EnterReadLock();
        try
        {
            var (startLocation, readLength) = GetInitialReadLocation(messagesDb, start);
            if (readLength > 0)
            {
                logger.LogInformation("User with ID: {UserId} is trying to pull {ReadLength} messages from thread {ThreadId}. Start Index is {StartIndex}", userId, readLength, threadId, startLocation);
                var firstPullRequest = messagesDb.ReadBulk(startLocation, readLength);
                await socket.Send(JsonTools.Serialize(firstPullRequest.Select(t => new Commit<ChatMessage>
                {
                    Item = t.ToClientView(),
                    Id = t.Id,
                    CommitTime = t.CreationTime
                })));
            }

            reflectorSubscription = threadReflector.Subscribe(reflectorConsumer);
            clientSubscription = socket.Subscribe(clientPushConsumer);
        }
        finally
        {
            lockObject.ExitReadLock();
        }

        try
        {
            logger.LogInformation("User with ID: {UserId} connected to thread {ThreadId} and listening for new events.", userId, threadId);
            await socket.Listen(HttpContext.RequestAborted);
        }
        catch (TaskCanceledException)
        {
            // Ignore. This happens when the client closes the connection.
        }
        finally
        {
            reflectorSubscription.Unsubscribe();
            clientSubscription.Unsubscribe();
            await socket.Close(HttpContext.RequestAborted);
        }
    }
    
    private static (int startOffset, int readLength) GetInitialReadLocation(
        IObjectBucket<ChatMessageInDb> messagesDb,
        string start)
    {
        var startLocation = 0;
        var found = false;

        if (!string.IsNullOrWhiteSpace(start))
        {
            startLocation = messagesDb.Count;
            
            // TODO: Really really bad performance. O(n) search.
            // Refactor required. Replace this with a hash table with LRU.
            foreach (var message in messagesDb.AsReverseEnumerable())
            {
                if (message.Id == start)
                {
                    found = true;
                    break;
                }
                startLocation--;
            }
        }

        if (!found)
        {
            startLocation = 0;
        }

        var readLength = messagesDb.Count - startLocation;
        return (startLocation, readLength);
    }
}

public class ClientPushConsumer(
    ILogger<ServerController> logger,
    ReaderWriterLockSlim lockObject,
    Guid userIdGuid,
    AsyncObservable<ChatMessageInDb[]> threadReflector,
    IObjectBucket<ChatMessageInDb> messagesDb)
    : IConsumer<string>
{
    public async Task Consume(string clientPushed)
    {
        logger.LogInformation("User with ID: {UserId} is trying to push a message.", userIdGuid);
        lockObject.EnterWriteLock();
        try
        {
            // TODO: The thread may be muted that not allowing anyone to send new messages. In this case, don't allow him to do this.
            // Deserialize the incoming messages and fill the properties.
            var model = JsonTools.Deserialize<PushModel<ChatMessage>>(clientPushed);
            var serverTime = DateTime.UtcNow;
            var messagesToAddToDb = model.Commits
                .Select(messageIncoming => new ChatMessageInDb
                {
                    Content = messageIncoming.Item.Content,
                    Id = messageIncoming.Id,
                    CreationTime = serverTime,
                    SenderId = userIdGuid,
                })
                .ToArray();

            // Reflect to other clients.
            await threadReflector.BroadcastAsync(messagesToAddToDb);

            // Save to database.
            messagesDb.Add(messagesToAddToDb);
            logger.LogInformation("User with ID: {UserId} pushed {Count} messages. Successfully broadcast to other clients and saved to database.", userIdGuid, messagesToAddToDb.Length);
        }
        finally
        {
            lockObject.ExitWriteLock();
        }
    }
}

public class ThreadReflectConsumer(
    ILogger<ServerController> logger,
    ObservableWebSocket socket)
    : IConsumer<ChatMessageInDb[]>
{
    public async Task Consume(ChatMessageInDb[] newCommits)
    {
        logger.LogInformation("Reflecting {Count} new messages to the client.", newCommits.Length);
        await socket.Send(JsonTools.Serialize(newCommits.Select(t => new Commit<ChatMessage>
        {
            Item = t.ToClientView(),
            Id = t.Id,
            CommitTime = t.CreationTime
        })));
    }
}