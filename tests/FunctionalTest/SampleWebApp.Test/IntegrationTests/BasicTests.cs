using System;
using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SampleWebApp.Data;
using SampleWebApp.Services;
using System.Linq;
using System.Threading.Tasks;

namespace SampleWebApp.Tests.IntegrationTests
{
    [TestClass]
    public class BasicTests
    {
        private const int _port = 15151;
        private readonly string _endpointUrl = $"ws://localhost:{_port}/repo.ares";
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
        }

        [TestMethod]
        public async Task SimpleCommitWithRemote()
        {
            var repo = new Repository<LogItem>() { Name = "Test local repo" };
            var remote = await new WebSocketRemote<LogItem>(_endpointUrl) { Name = "Demo remote" }
                .AttachAsync(repo);

            repo.Commit(new LogItem { Message = "1" });
            repo.Commit(new LogItem { Message = "2" });

            await Task.Delay(500); // Wait for the pull to update pointer.
            Assert.IsNotNull(remote.PullPointer);

            RepositoryContainer.GetRepositoryForTest().Assert(
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
            await new WebSocketRemote<LogItem>(_endpointUrl).AttachAsync(repo);

            var repo2 = new Repository<LogItem>();
            await new WebSocketRemote<LogItem>(_endpointUrl).AttachAsync(repo2);

            repo.Commit(new LogItem { Message = "1" });
            repo.Commit(new LogItem { Message = "2" });
            repo.Commit(new LogItem { Message = "3" });

            repo.Assert(
                new LogItem { Message = "1" },
                new LogItem { Message = "2" },
                new LogItem { Message = "3" });
            RepositoryContainer.GetRepositoryForTest().Assert(
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
            await new WebSocketRemote<LogItem>(_endpointUrl) { Name = "A to server" }
                .AttachAsync(repoA);

            var repoB = new Repository<LogItem>() { Name = "Repo B" };
            await new WebSocketRemote<LogItem>(_endpointUrl) { Name = "B to server" }
                .AttachAsync(repoB);

            repoA.Commit(new LogItem { Message = "1" });

            RepositoryContainer.GetRepositoryForTest().Assert(
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

            var senderserver = new Repository<LogItem>() { Name = "Sender Server" };
            var subscriber1 = new Repository<LogItem>() { Name = "Subscriber1" };
            var subscriber2 = new Repository<LogItem>() { Name = "Subscriber2" };
            var subscriber3 = new Repository<LogItem>() { Name = "Subscriber3" };

            await new WebSocketRemote<LogItem>(_endpointUrl) { Name = "Remote of sender" }.AttachAsync(senderserver);
            var remote1 = await new WebSocketRemote<LogItem>(_endpointUrl) { Name = "Remote of subscriber1" }
                .AttachAsync(subscriber1);
            var remote2 = await new WebSocketRemote<LogItem>(_endpointUrl) { Name = "Remote of subscriber2" }
                .AttachAsync(subscriber2);

            senderserver.Commit(new LogItem { Message = "G" });
            senderserver.Commit(new LogItem { Message = "H" });
            var remote3 = await new WebSocketRemote<LogItem>(_endpointUrl) { Name = "Remote of subscriber3" }
                .AttachAsync(subscriber3);
            senderserver.Commit(new LogItem { Message = "X" });
            senderserver.Commit(new LogItem { Message = "Z" });

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
            Assert.AreEqual(subscriber1.Head.Id, remote1.PushPointer, remote1.PullPointer);
            Assert.AreEqual(subscriber2.Head.Id, remote2.PushPointer, remote2.PullPointer);
            Assert.AreEqual(subscriber3.Head.Id, remote3.PushPointer, remote3.PullPointer);
        }


        [TestMethod]
        public async Task DoubleWayDataBinding()
        {
            var repoA = new Repository<LogItem>() { Name = "Repo A" };
            await new WebSocketRemote<LogItem>(_endpointUrl) { Name = "Connction to server for Repo A" }
                .AttachAsync(repoA);

            var repoB = new Repository<LogItem>() { Name = "Repo B" };
            await new WebSocketRemote<LogItem>(_endpointUrl) { Name = "Connection to server for Repo B" }
                .AttachAsync(repoB);

            repoA.Commit(new LogItem { Message = "G" });
            repoA.Commit(new LogItem { Message = "H" });
            repoA.Commit(new LogItem { Message = "X" });
            repoB.Commit(new LogItem { Message = "Z" });

            repoA.AssertEqual(repoB, 4);
            repoA.AssertEqual(RepositoryContainer.GetRepositoryForTest(), 4);
        }

        [TestMethod]
        public async Task DropTest()
        {
            var sender = new Repository<LogItem>();
            var subscriber = new Repository<LogItem>();
            await new WebSocketRemote<LogItem>(_endpointUrl).AttachAsync(sender);

            var subscriberRemote = await new WebSocketRemote<LogItem>(_endpointUrl).AttachAsync(subscriber); ;

            sender.Commit(new LogItem { Message = "G" });
            sender.Commit(new LogItem { Message = "H" });

            sender.Assert(
                new LogItem { Message = "G" },
                new LogItem { Message = "H" });
            subscriber.Assert(
                new LogItem { Message = "G" },
                new LogItem { Message = "H" });

            await subscriberRemote.DropAsync();

            sender.Commit(new LogItem { Message = "X" });
            sender.Commit(new LogItem { Message = "Z" });

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

        [TestMethod]
        public async Task TimeTest()
        {
            var repo = new Repository<LogItem>() { Name = "Test local repo" };
            
            DateTime beginTime = DateTime.Now;
            for (int i = 0; i < 100; i++)
            {
                repo.Commit(new LogItem { Message = "1" });
                Console.WriteLine(DateTime.Now);
            }
            
            DateTime beginTime2 = DateTime.Now;
            for (int i = 0; i < 100; i++)
            {
                repo.Commit(new LogItem { Message = "2" });
                Console.WriteLine(DateTime.Now);
            }
            
            DateTime beginTime3 = DateTime.Now;
            for (int i = 0; i < 100; i++)
            {
                repo.Commit(new LogItem { Message = "3" });
                Console.WriteLine(DateTime.Now);
            }
            
            DateTime endTime = DateTime.Now;

            Assert.IsTrue(2 * (beginTime2 - beginTime) > (endTime - beginTime3));
        }
    }

    public static class TestExtends
    {
        public static void AssertEqual<T>(this Repository<T> repo, Repository<T> repo2, int expectedCount)
        {
            repo.WaitTill(expectedCount, 9).Wait();
            repo2.WaitTill(expectedCount, 9).Wait();
            var commits = repo.Commits.ToArray();
            var commit2s = repo2.Commits.ToArray();
            if (commits.Count() != commit2s.Count() || commits.Count() != expectedCount)
            {
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail($"The repo '{repo.Name}' don't match! Expected: {string.Join(',', commit2s.Select(t => t.ToString()))}; Actual: {string.Join(',', repo.Commits.Select(t => t.ToString()))}");
            }
            for (int i = 0; i < commits.Count(); i++)
            {
                if (!commits[i].Id.Equals(commit2s[i].Id))
                {
                    Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail($"The repo '{repo.Name}' don't match! Expected: {string.Join(',', commit2s.Select(t => t.ToString()))}; Actual: {string.Join(',', repo.Commits.Select(t => t.ToString()))}");
                }
            }
        }
        public static void Assert<T>(this Repository<T> repo, params T[] array)
        {
            repo.WaitTill(array.Length, 9).Wait();
            var commits = repo.Commits.ToArray();
            for (int i = 0; i < commits.Count(); i++)
            {
                if (!commits[i].Item.Equals(array[i]))
                {
                    Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail($"The repo '{repo.Name}' don't match! Expected: {string.Join(',', array.Select(t => t.ToString()))}; Actual: {string.Join(',', repo.Commits.Select(t => t.ToString()))}");
                }
            }
        }

        private static async Task WaitTill<T>(this Repository<T> repo, int count, int maxWaitSeconds = 5)
        {
            int waitedTimes = 0;
            while (repo.Commits.Count() < count)
            {
                await Task.Delay(10);
                waitedTimes++;
                if (waitedTimes / 100 >= maxWaitSeconds)
                {
                    Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail($"Two repo don't match! Expected: {count} commits; Actual: {string.Join(',', repo.Commits.Select(t => t.ToString()))}");
                }
            }
        }
    }
}
