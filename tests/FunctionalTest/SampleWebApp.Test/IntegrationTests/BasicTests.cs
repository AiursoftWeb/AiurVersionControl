using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SampleWebApp.Controllers;
using SampleWebApp.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SampleWebApp.Tests.IntegrationTests
{
    [TestClass]
    public class BasicTests
    {
        private const int _port = 15151;
        private readonly string _endpointUrl = $"http://localhost:{_port}/repo.ares";
        private IHost _server;

        [TestInitialize]
        public async Task CreateServer()
        {
            _server = Program.BuildHost(null, _port);
            await _server.StartAsync();
        }

        [TestCleanup]
        public async Task CleanServer()
        {
            await _server.StopAsync();
            _server.Dispose();
            HomeController._repo = null;
        }

        [TestMethod]
        public async Task ManualPushPull()
        {
            var repo = new Repository<LogItem>() { Name = "Local Repo A" };
            await repo.AddRemoteAsync(new WebSocketRemote<LogItem>(_endpointUrl) { Name = "A websocket to server" });

            await repo.CommitAsync(new LogItem { Message = "1" });
            await repo.CommitAsync(new LogItem { Message = "2" });
            await repo.CommitAsync(new LogItem { Message = "3" });
            Assert.IsNotNull(repo.Remotes.First().PushPointer);
            await repo.PushAsync(); // Shall do nothing.
            await Task.Delay(50);

            HomeController._repo.Assert(
                new LogItem { Message = "1" },
                new LogItem { Message = "2" },
                new LogItem { Message = "3" });

            var repo2 = new Repository<LogItem>();
            await repo2.AddRemoteAsync(new WebSocketRemote<LogItem>(_endpointUrl));

            repo2.Assert(
                new LogItem { Message = "1" },
                new LogItem { Message = "2" },
                new LogItem { Message = "3" });
        }

        [TestMethod]
        public async Task SimpleCommitWhilingPulling()
        {
            var repo = new Repository<LogItem>() { Name = "Test local repo" };
            var remote = new WebSocketRemote<LogItem>(_endpointUrl) { Name = "Demo remote" };
            await repo.AddRemoteAsync(remote);

            await repo.CommitAsync(new LogItem { Message = "1" });
            await repo.CommitAsync(new LogItem { Message = "2" });
            await Task.Delay(50);
            Assert.IsNotNull(remote.HEAD);

            HomeController._repo.Assert(
                new LogItem { Message = "1" },
                new LogItem { Message = "2" });

            repo.Assert(
                new LogItem { Message = "1" },
                new LogItem { Message = "2" });
        }

        [TestMethod]
        public async Task OnewayAutoPull()
        {
            var repo = new Repository<LogItem>();
            await repo.AddRemoteAsync(new WebSocketRemote<LogItem>(_endpointUrl));

            var repo2 = new Repository<LogItem>();
            await repo2.AddRemoteAsync(new WebSocketRemote<LogItem>(_endpointUrl));

            await repo.CommitAsync(new LogItem { Message = "1" });
            await repo.CommitAsync(new LogItem { Message = "2" });
            await repo.CommitAsync(new LogItem { Message = "3" });
            await Task.Delay(50);

            repo.Assert(
                new LogItem { Message = "1" },
                new LogItem { Message = "2" },
                new LogItem { Message = "3" });
            HomeController._repo.Assert(
                new LogItem { Message = "1" },
                new LogItem { Message = "2" },
                new LogItem { Message = "3" });
            repo2.Assert(
                new LogItem { Message = "1" },
                new LogItem { Message = "2" },
                new LogItem { Message = "3" });
        }

        [TestMethod]
        public async Task OneCommitSync()
        {
            var repoA = new Repository<LogItem>() { Name = "Repo A" };
            await repoA.AddRemoteAsync(new WebSocketRemote<LogItem>(_endpointUrl) { Name = "A to server" });

            var repoB = new Repository<LogItem>() { Name = "Repo B" };
            await repoB.AddRemoteAsync(new WebSocketRemote<LogItem>(_endpointUrl) { Name = "B to server" });

            await repoA.CommitAsync(new LogItem { Message = "1" });
            await Task.Delay(50);

            HomeController._repo.Assert(
                new LogItem { Message = "1" });
            repoA.Assert(
                new LogItem { Message = "1" });
            repoB.Assert(
                new LogItem { Message = "1" });
        }

        [TestMethod]
        public async Task DistributeTest()
        {
            //     server
            //    /       \
            //   sender    subscriber1,2,3

            var senderserver = new Repository<LogItem>();
            var subscriber1 = new Repository<LogItem>();
            var subscriber2 = new Repository<LogItem>();
            var subscriber3 = new Repository<LogItem>();

            await senderserver.AddRemoteAsync(new WebSocketRemote<LogItem>(_endpointUrl));
            await subscriber1.AddRemoteAsync(new WebSocketRemote<LogItem>(_endpointUrl));
            await subscriber2.AddRemoteAsync(new WebSocketRemote<LogItem>(_endpointUrl));
            await subscriber3.AddRemoteAsync(new WebSocketRemote<LogItem>(_endpointUrl));

            await senderserver.CommitAsync(new LogItem { Message = "G" });
            await senderserver.CommitAsync(new LogItem { Message = "H" });
            await senderserver.CommitAsync(new LogItem { Message = "X" });
            await senderserver.CommitAsync(new LogItem { Message = "Z" });
            await Task.Delay(200);

            subscriber1.Assert(
                new LogItem { Message = "G" },
                new LogItem { Message = "H" },
                new LogItem { Message = "X" },
                new LogItem { Message = "Z" });
            subscriber2.Assert(
                new LogItem { Message = "G" },
                new LogItem { Message = "H" },
                new LogItem { Message = "X" },
                new LogItem { Message = "Z" });
            subscriber3.Assert(
                new LogItem { Message = "G" },
                new LogItem { Message = "H" },
                new LogItem { Message = "X" },
                new LogItem { Message = "Z" });
            Assert.AreEqual(subscriber1.Head.Id, subscriber1.Remotes.First().PushPointer, subscriber1.Remotes.First().HEAD);
            Assert.AreEqual(subscriber2.Head.Id, subscriber2.Remotes.First().PushPointer, subscriber2.Remotes.First().HEAD);
            Assert.AreEqual(subscriber3.Head.Id, subscriber3.Remotes.First().PushPointer, subscriber3.Remotes.First().HEAD);
        }

        [TestMethod]
        public async Task DoubleWayDataBindingWithMultipleRemote()
        {
            var repoA = new Repository<LogItem>() { Name = "Repo A" };
            await repoA.AddRemoteAsync(new WebSocketRemote<LogItem>(_endpointUrl) { Name = "A to server" });

            var repoB = new Repository<LogItem>() { Name = "Repo B" };
            await repoB.AddRemoteAsync(new WebSocketRemote<LogItem>(_endpointUrl) { Name = "B to server" });

            await repoA.CommitAsync(new LogItem { Message = "G" });
            await repoA.CommitAsync(new LogItem { Message = "H" });
            await repoA.CommitAsync(new LogItem { Message = "X" });
            await repoB.CommitAsync(new LogItem { Message = "Z" });
            await Task.Delay(200);

            repoA.Assert(
                new LogItem { Message = "G" },
                new LogItem { Message = "H" },
                new LogItem { Message = "X" },
                new LogItem { Message = "Z" });
            repoB.Assert(
                new LogItem { Message = "G" },
                new LogItem { Message = "H" },
                new LogItem { Message = "X" },
                new LogItem { Message = "Z" });

            HomeController._repo.Assert(
                new LogItem { Message = "G" },
                new LogItem { Message = "H" },
                new LogItem { Message = "X" },
                new LogItem { Message = "Z" });
        }

        [TestMethod]
        public async Task DropTest()
        {
            var sender = new Repository<LogItem>();
            var subscriber = new Repository<LogItem>();
            await sender.AddRemoteAsync(new WebSocketRemote<LogItem>(_endpointUrl));

            var remote = new WebSocketRemote<LogItem>(_endpointUrl);
            await subscriber.AddRemoteAsync(remote);

            await sender.CommitAsync(new LogItem { Message = "G" });
            await sender.CommitAsync(new LogItem { Message = "H" });
            await Task.Delay(30);
            sender.Assert(
                new LogItem { Message = "G" },
                new LogItem { Message = "H" });
            subscriber.Assert(
                new LogItem { Message = "G" },
                new LogItem { Message = "H" });
            await subscriber.DropRemoteAsync(remote);
            await sender.CommitAsync(new LogItem { Message = "X" });
            await sender.CommitAsync(new LogItem { Message = "Z" });
            await Task.Delay(30);
            sender.Assert(
                new LogItem { Message = "G" },
                new LogItem { Message = "H" },
                new LogItem { Message = "X" },
                new LogItem { Message = "Z" });
            subscriber.Assert(
                new LogItem { Message = "G" },
                new LogItem { Message = "H" });
        }
    }

    public static class TestExtends
    {
        public static void Assert<T>(this Repository<T> repo, params T[] array)
        {
            var commits = repo.Commits.ToArray();
            if (commits.Length != array.Length)
            {
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail($"Two repo don't match! Expected: {string.Join(',', array.Select(t => t.ToString()))}; Actual: {string.Join(',', repo.Commits.Select(t => t.ToString()))}");
            }
            for (int i = 0; i < commits.Count(); i++)
            {
                if (!commits[i].Item.Equals(array[i]))
                {
                    Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail($"Two repo don't match! Expected: {string.Join(',', array.Select(t => t.ToString()))}; Actual: {string.Join(',', repo.Commits.Select(t => t.ToString()))}");
                }
            }
        }
    }
}
