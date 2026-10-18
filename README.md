# AiurVersionControl

[![MIT licensed](https://img.shields.io/badge/license-MIT-blue.svg)](https://gitlab.aiursoft.com/aiursoft/aiurversioncontrol/-/blob/master/LICENSE)
[![Pipeline stat](https://gitlab.aiursoft.com/aiursoft/aiurversioncontrol/badges/master/pipeline.svg)](https://gitlab.aiursoft.com/aiursoft/aiurversioncontrol/-/pipelines)
[![Test Coverage](https://gitlab.aiursoft.com/aiursoft/aiurversioncontrol/badges/master/coverage.svg)](https://gitlab.aiursoft.com/aiursoft/aiurversioncontrol/-/pipelines)
[![NuGet version](https://img.shields.io/nuget/v/Aiursoft.AiurVersionControl.svg?style=flat-square)](https://www.nuget.org/packages/Aiursoft.AiurVersionControl/)
[![Man hours](https://manhours.aiursoft.com/r/github.com/aiursoftweb/aiurversioncontrol.svg)](https://manhours.aiursoft.com/r/github.com/aiursoftweb/aiurversioncontrol.html)

**A powerful, elegant distributed synchronization framework** that brings Git-like version control to any .NET application. Powers [Kahla](https://kahla.app) real-time messaging with **eventual consistency**, **automatic conflict resolution**, and **offline-first capabilities**.

## 🌟 Why AiurVersionControl?

Imagine building a **multiplayer Snake game** where all players see the exact same state in real-time, or a **collaborative chat app** that works perfectly even when offline. AiurVersionControl makes this trivially easy by treating all data changes as **immutable commits** that can be synced, merged, and replayed.

**Key advantages:**
- ✅ **Offline Operations** - Commit changes and query your repository without any network connection
- ✅ **Automatic Sync** - When connection is available, changes sync automatically
- ✅ **Local-First** - All operations work on local data first, perfect for building offline-capable CRUD apps

### Key Features

- 🔄 **Distributed Event Sourcing** - Every change is an immutable commit with automatic conflict resolution
- 🌐 **Offline-First Architecture** - Work offline, sync later with guaranteed eventual consistency
- ⚡ **Real-time Sync** - Bi-directional automatic push/pull between repositories
- 🎯 **Type-Safe** - Fully generic API works with any data type
- 🧩 **CRUD Operations** - High-level abstractions for collection manipulation
- 🔌 **Multiple Transports** - In-memory, WebSocket, HTTP, or custom implementations
- 🎮 **Production Ready** - Battle-tested in Kahla real-time messaging platform

## 📦 Packages

| Package | Description | Use Case |
|---------|-------------|----------|
| **Aiursoft.AiurEventSyncer** | Core event synchronization engine | Building distributed apps with auto-sync |
| **Aiursoft.AiurVersionControl** | Event replay & workspace reconstruction | State management with commit history |
| **Aiursoft.AiurVersionControl.Crud** | High-level CRUD operations | Managing collections with Add/Update/Delete |
| **Aiursoft.AiurEventSyncer.WebExtends** | WebSocket transport support | Real-time web applications |
| **Aiursoft.AiurStore** | Immutable storage abstraction | Custom database backends |

## 🚀 Quick Start

### Installation

```bash
dotnet add package Aiursoft.AiurEventSyncer
```

### Basic Event Syncing

Create two repositories that automatically stay in sync:

```csharp
using Aiursoft.AiurEventSyncer.Models;
using Aiursoft.AiurEventSyncer.Remotes;

// Create two repositories
var clientRepo = new Repository<string>();
var serverRepo = new Repository<string>();

// Subscribe to observe commits
clientRepo.AppendCommitsHappened.Subscribe(commits =>
{
    Console.WriteLine($"[Client] Received: {commits.Last().Item}");
    return Task.CompletedTask;
});

serverRepo.AppendCommitsHappened.Subscribe(commits =>
{
    Console.WriteLine($"[Server] Received: {commits.Last().Item}");
    return Task.CompletedTask;
});

// Attach client to server with bi-directional auto-sync
await new ObjectRemote<string>(serverRepo, autoPush: true, autoPull: true)
    .AttachAsync(clientRepo);

// Now they sync automatically!
clientRepo.Commit("Hello from client");
// Output: [Client] Received: Hello from client
//         [Server] Received: Hello from client

serverRepo.Commit("Hello from server");
// Output: [Server] Received: Hello from server
//         [Client] Received: Hello from server

// Both repositories now have identical commit history
Console.WriteLine($"Client commits: {clientRepo.Commits.Count()}"); // 2
Console.WriteLine($"Server commits: {serverRepo.Commits.Count()}"); // 2
```

### CRUD Operations on Collections

Work with collections using familiar operations:

```csharp
using Aiursoft.AiurVersionControl.Crud;
using Aiursoft.AiurVersionControl.Remotes;

// Define your model
public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
}

// Create a collection repository
var library = new CollectionRepository<Book>
{
    new() { Id = 1, Title = "1984" },
    new() { Id = 2, Title = "Brave New World" }
};

// CRUD operations
library.Add(new Book { Id = 3, Title = "Fahrenheit 451" });
library.Patch(nameof(Book.Id), 2, nameof(Book.Title), "Updated Title");
library.Drop(nameof(Book.Id), 1);

// Sync with remote
var remoteLibrary = new CollectionRepository<Book>();
var remote = new ObjectRemoteWithWorkSpace<CollectionWorkSpace<Book>>(
    library, autoPush: true, autoPull: true);
await remote.AttachAsync(remoteLibrary);

// All changes automatically sync to remoteLibrary!
```

### Advanced: Version-Controlled State

Build stateful applications with full commit history:

```csharp
using Aiursoft.AiurVersionControl.Models;
using Aiursoft.AiurVersionControl.Remotes;

// Your state model
public class GameState : WorkSpace
{
    public int Score { get; set; }
    public override object Clone() => new GameState { Score = Score };
}

// Modification actions
public class UpdateScore : IModification<GameState>
{
    public int Points { get; set; }
    public void Apply(GameState workspace) => workspace.Score += Points;
}

// Create controlled repository
var game = new ControlledRepository<GameState>();

// Apply changes
game.ApplyChange(new UpdateScore { Points = 10 });
game.ApplyChange(new UpdateScore { Points = 5 });

Console.WriteLine(game.WorkSpace.Score); // 15

// Sync with other players
var player2 = new ControlledRepository<GameState>();
await new ObjectRemoteWithWorkSpace<GameState>(player2, true, true)
    .AttachAsync(game);
// player2.WorkSpace.Score is now 15!
```

## 🎮 Real-World Examples

Check out the `/demos` folder for complete working examples:

### [Multiplayer Snake Game](./demos/Aiursoft.SnakeGame)

A fully functional **multiplayer Snake game** demonstrating:
- Real-time game state synchronization across multiple clients
- Deterministic conflict resolution
- Immediate visual feedback with no server round-trip delay
- Game state replay and debugging

```bash
cd demos/Aiursoft.SnakeGame
dotnet run
```

### [WPF Sample Application](./demos/SampleWPF)

Windows desktop app showcasing offline-first data binding.

## 🏗️ Architecture

AiurVersionControl implements a **layered architecture** inspired by distributed version control systems:

```
┌─────────────────────────────────────────┐
│  Your Application (CRUD, WorkSpace)    │  ← High-level business logic
├─────────────────────────────────────────┤
│  AiurVersionControl (Event Replay)      │  ← State reconstruction
├─────────────────────────────────────────┤
│  AiurEventSyncer (Sync Engine)          │  ← Distributed syncing
├─────────────────────────────────────────┤
│  AiurStore (Immutable Storage)          │  ← Persistence layer
└─────────────────────────────────────────┘
```

### Core Concepts

1. **Repository** - Immutable commit log, similar to Git
2. **Commit** - Atomic change event with unique ID
3. **Remote** - Connection to another repository for syncing
4. **WorkSpace** - Current state reconstructed from commit history
5. **Modification** - Operation that transforms workspace state

### Sync Patterns

```
Publish-Subscribe:          Hub-and-Spoke:           Peer-to-Peer:
                                                     
sender → server           client₁ ↔ server          repo₁ ↔ repo₂
         ↓                client₂ ↔ server              ↓     ↓
    subscriber₁           client₃ ↔ server          repo₃ ↔ repo₄
    subscriber₂
```

All patterns supported with `autoPush` and `autoPull` configurations!

## 🔬 How It Works

### Event Sourcing

Instead of storing current state, AiurVersionControl stores **all changes as immutable commits**:

```csharp
// Traditional approach
var user = dbContext.Users.Find(123);
user.Name = "Alice";  // Lost: what was the old name? when did it change?
dbContext.SaveChanges();

// AiurVersionControl approach  
userRepo.Commit(new UpdateName { UserId = 123, NewName = "Alice" });
// Full history preserved! Can replay, debug, and sync.
```

### Automatic Conflict Resolution

When two repositories diverge and then sync, commits are **automatically merged in deterministic order**:

```csharp
// Repo A and B start synchronized
await new ObjectRemote<int>(repoB, autoPush: true, autoPull: true)
    .AttachAsync(repoA);

// Both add commits while temporarily disconnected
repoA.Commit(100);  // A's timeline: [100]
repoB.Commit(200);  // B's timeline: [200]

// When reconnected, both converge to same order
await Task.Delay(100);  // Allow sync
// Both now have: [100, 200] or [200, 100] - deterministic based on commit IDs
```

### Offline-First Experience

**All operations work without network connection** - commits, queries, and CRUD operations are purely local:

```csharp
// At home with WiFi
var todoApp = new CollectionRepository<Todo>();
todoApp.Add(new Todo { Id = 1, Task = "Buy milk" });

var remote = await ConnectToServer(todoApp);

// Airplane mode! Disconnect from server
await remote.DetachAsync();

// Everything still works - completely offline!
todoApp.Add(new Todo { Id = 2, Task = "Read book" });
todoApp.Add(new Todo { Id = 3, Task = "Exercise" });
todoApp.Patch(nameof(Todo.Id), 2, nameof(Todo.Task), "Read 50 pages");
todoApp.Drop(nameof(Todo.Id), 1);

// Query your local repository - no network needed
var allTodos = todoApp.WorkSpace.ToList();
Console.WriteLine($"Offline todos count: {allTodos.Count}"); // 2

// Back online - reconnect to server
await remote.AttachAsync(todoApp);
// All offline commits automatically sync to server!
// Server now has the same state: todos 2 and 3
```

**This makes AiurVersionControl perfect for building offline-capable CRUD applications** - notes apps, todo lists, inventory managers, or any app that needs to work without internet!

## 📊 Comparison with Alternatives

| Feature | AiurVersionControl | SignalR | Firebase | Event Store |
|---------|-------------------|---------|----------|-------------|
| Offline Support | ✅ Full | ❌ | ⚠️ Limited | ❌ |
| Automatic Sync | ✅ Bi-directional | ⚠️ Manual | ✅ | ❌ |
| Conflict Resolution | ✅ Automatic | ❌ | ⚠️ Manual | ❌ |
| Type Safety | ✅ Generic | ⚠️ Partial | ❌ | ⚠️ Partial |
| Self-Hosted | ✅ | ✅ | ❌ | ✅ |
| CRUD Abstraction | ✅ | ❌ | ✅ | ❌ |

## 🧪 Testing

The framework includes **comprehensive test coverage** demonstrating all features:

```bash
# Run all tests
dotnet test

# Run specific test suite
dotnet test --filter "FullyQualifiedName~AiurEventSyncer.Tests"
```

Key test scenarios:
- ✅ Bi-directional syncing with auto push/pull
- ✅ Hub-and-spoke distribution patterns  
- ✅ Attach/detach/reattach functionality
- ✅ CRUD operations with conflict resolution
- ✅ Merge scenarios with concurrent modifications

## 📚 Advanced Usage

### Custom Storage Backend

```csharp
// Implement your own storage (SQL, Redis, etc.)
public class SqlCommitStore : InOutDatabase<Commit<MyData>>
{
    public override void Add(Commit<MyData> item) => /* SQL INSERT */;
    public override IEnumerable<Commit<MyData>> GetAll() => /* SQL SELECT */;
    // ... implement other methods
}

var repo = new Repository<MyData>(new SqlCommitStore());
```

### WebSocket Real-Time Sync

```csharp
// Server side
services.AddAiurEventSyncer();
app.MapWebSocketSyncer<ChatMessage>("/sync");

// Client side
var wsRemote = new WebSocketRemote<ChatMessage>(
    "wss://myapp.com/sync", autoPush: true, autoPull: true);
await wsRemote.AttachAsync(localRepo);
```

### Observable State Changes

```csharp
var game = new ControlledRepository<GameState>();

// React to state changes
game.PropertyChanged += (sender, e) =>
{
    if (e.PropertyName == nameof(game.WorkSpace))
    {
        UI.UpdateDisplay(game.WorkSpace);
    }
};

game.ApplyChange(new UpdateScore { Points = 10 });
// UI automatically updates!
```

## 🛠️ Development

### Requirements

- [.NET 10 SDK](https://dot.net/)
- Any IDE (Visual Studio, VS Code, Rider)

### Building

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Pack NuGet packages
dotnet pack
```

### Project Structure

```
AiurVersionControl/
├── src/
│   ├── Aiursoft.AiurEventSyncer/          # Core sync engine
│   ├── Aiursoft.AiurVersionControl/       # Version control layer
│   ├── Aiursoft.AiurVersionControl.Crud/  # CRUD operations
│   ├── Aiursoft.AiurEventSyncer.WebExtends/ # WebSocket support
│   └── Aiursoft.AiurStore/                # Storage abstraction
├── demos/
│   ├── Aiursoft.SnakeGame/                # Multiplayer game demo
│   └── SampleWPF/                         # Desktop app demo
└── tests/                                  # Comprehensive tests
```

## 🤝 Contributing

We welcome contributions! This project follows a **fork-and-pull-request** workflow:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Commit** your changes (`git commit -m 'Add amazing feature'`)
4. **Push** to your branch (`git push origin feature/amazing-feature`)
5. **Open** a Pull Request

### Contribution Ideas

- 🔌 Additional transport implementations (gRPC, MQTT, etc.)
- 📝 More CRUD operation types
- 🎨 Sample applications
- 📚 Documentation improvements
- 🐛 Bug reports with reproduction cases

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Inspired by **Git**, **Event Sourcing**, and **CRDT** research
- Powers **[Kahla](https://kahla.app)** - Production-grade real-time messaging
- Built with ❤️ by the Aiursoft team

## 📖 Learn More

- **Live Demo**: Try the [Snake Game](./demos/Aiursoft.SnakeGame) to see real-time sync in action
- **NuGet Packages**: [Aiursoft.AiurVersionControl](https://www.nuget.org/packages/Aiursoft.AiurVersionControl/)
- **GitLab Repository**: [gitlab.aiursoft.com/aiursoft/aiurversioncontrol](https://gitlab.aiursoft.com/aiursoft/aiurversioncontrol)
- **Kahla Application**: [kahla.app](https://kahla.app) - See it in production

---

**Made with 🎯 precision and ✨ elegance by Aiursoft**
