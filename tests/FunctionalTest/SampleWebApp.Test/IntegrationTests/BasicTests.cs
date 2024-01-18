using Aiursoft.AiurEventSyncer.Models;
using Aiursoft.AiurEventSyncer.Remotes;
using Aiursoft.CSTools.Tools;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SampleWebApp.Models;
using SampleWebApp.Services;
using static Aiursoft.WebTools.Extends;

namespace SampleWebApp.Test.IntegrationTests
{
    [TestClass]
    public class BasicTests
    {
        private readonly int _port;
        private readonly string _endpointUrl;
        private static IHost _server;
        public BasicTests()
        {
            _port = Network.GetAvailablePort();
            _endpointUrl = $"ws://localhost:{_port}/repo.ares";
        }
        
        [TestInitialize]
        public async Task CreateServer()
        {
            _server = App<Startup>(Array.Empty<string>(), port: _port);
            await _server.StartAsync();
        }

        [TestCleanup]
        public void CleanServer()
        {
            RepositoryContainer.ResetRepositoryForTest();
        }

        [TestMethod]
        public async Task SimpleCommitWithRemote()
        {
            var repo = new Repository<LogItem>();
            var remote = await new WebSocketRemote<LogItem>(_endpointUrl)
                .AttachAsync(repo);

            repo.Commit(new LogItem { Message = "1" });
            repo.Commit(new LogItem { Message = "2" });

            await Task.Delay(250); // Wait for the pull to update pointer.
            Assert.IsNotNull(remote.PullPointer);

            var remoteRepo = RepositoryContainer.GetRepositoryForTest();
            remoteRepo.Assert(
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
            await Task.Delay(500); // Wait for the pull to update pointer.
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
            var repoA = new Repository<LogItem>();
            await new WebSocketRemote<LogItem>(_endpointUrl)
                .AttachAsync(repoA);

            var repoB = new Repository<LogItem>();
            await new WebSocketRemote<LogItem>(_endpointUrl)
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

            var senderserver = new Repository<LogItem>();
            var subscriber1 = new Repository<LogItem>();
            var subscriber2 = new Repository<LogItem>();
            var subscriber3 = new Repository<LogItem>();

            await new WebSocketRemote<LogItem>(_endpointUrl).AttachAsync(senderserver);
            var remote1 = await new WebSocketRemote<LogItem>(_endpointUrl)
                .AttachAsync(subscriber1);
            var remote2 = await new WebSocketRemote<LogItem>(_endpointUrl)
                .AttachAsync(subscriber2);

            senderserver.Commit(new LogItem { Message = "G" });
            senderserver.Commit(new LogItem { Message = "H" });
            var remote3 = await new WebSocketRemote<LogItem>(_endpointUrl)
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
            Assert.AreEqual(subscriber1.Head, remote1.PushPointer);
            Assert.AreEqual(subscriber2.Head, remote2.PushPointer);
            Assert.AreEqual(subscriber3.Head, remote3.PushPointer);
            Assert.AreEqual(subscriber1.Head, remote1.PullPointer);
            Assert.AreEqual(subscriber2.Head, remote2.PullPointer);
            Assert.AreEqual(subscriber3.Head, remote3.PullPointer);
        }


        [TestMethod]
        public async Task DoubleWayDataBinding()
        {
            var repoA = new Repository<LogItem>();
            await new WebSocketRemote<LogItem>(_endpointUrl)
                .AttachAsync(repoA);

            var repoB = new Repository<LogItem>();
            await new WebSocketRemote<LogItem>(_endpointUrl)
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

            var subscriberRemote = await new WebSocketRemote<LogItem>(_endpointUrl).AttachAsync(subscriber);

            sender.Commit(new LogItem { Message = "G" });
            sender.Commit(new LogItem { Message = "H" });

            sender.Assert(
                new LogItem { Message = "G" },
                new LogItem { Message = "H" });
            subscriber.Assert(
                new LogItem { Message = "G" },
                new LogItem { Message = "H" });

            await subscriberRemote.DetachAsync();

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
        public async Task ReattachTest()
        {
            var sender = new Repository<LogItem>();
            var subscriber = new Repository<LogItem>();
            await new WebSocketRemote<LogItem>(_endpointUrl).AttachAsync(sender);

            var subscriberRemote = await new WebSocketRemote<LogItem>(_endpointUrl).AttachAsync(subscriber);

            sender.Commit(new LogItem { Message = "G" });
            sender.Commit(new LogItem { Message = "H" });

            sender.Assert(
                new LogItem { Message = "G" },
                new LogItem { Message = "H" });
            subscriber.Assert(
                new LogItem { Message = "G" },
                new LogItem { Message = "H" });

            await subscriberRemote.DetachAsync();

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

            await subscriberRemote.AttachAsync(subscriber);
            subscriber.Assert(
                new LogItem { Message = "G" },
                new LogItem { Message = "H" },
                new LogItem { Message = "X" },
                new LogItem { Message = "Z" });
        }

        [TestMethod]
        public async Task ReconnectTest()
        {
            var sender = new Repository<LogItem>();
            var subscriber = new Repository<LogItem>();
            await new WebSocketRemote<LogItem>(_endpointUrl, true).AttachAsync(sender);

            var subscriberRemote = await new WebSocketRemote<LogItem>(_endpointUrl).AttachAsync(subscriber);

            sender.Commit(new LogItem { Message = "G" });
            sender.Commit(new LogItem { Message = "H" });

            sender.Assert(
                new LogItem { Message = "G" },
                new LogItem { Message = "H" });
            subscriber.Assert(
                new LogItem { Message = "G" },
                new LogItem { Message = "H" });

            await subscriberRemote.DetachAsync();

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

            await subscriberRemote.AttachAsync(subscriber);
            subscriber.Assert(
                new LogItem { Message = "G" },
                new LogItem { Message = "H" },
                new LogItem { Message = "X" },
                new LogItem { Message = "Z" });
        }

        [TestMethod]
        public async Task PerformanceTest()
        {
            var repo = new Repository<LogItem>();
            await new WebSocketRemote<LogItem>(_endpointUrl).AttachAsync(repo);

            var repo2 = new Repository<LogItem>();
            await new WebSocketRemote<LogItem>(_endpointUrl).AttachAsync(repo2);

            var beginTime = DateTime.Now;
            for (var i = 0; i < 1000; i++)
            {
                repo.Commit(new LogItem { Message = "1" });
                repo2.Commit(new LogItem { Message = "2" });
            }

            var endTime = DateTime.Now;

            Assert.IsTrue((endTime - beginTime) < TimeSpan.FromSeconds(0.5));
        }

        [TestMethod]
        public async Task PreviousPushedTest()
        {
            var repo = new Repository<LogItem>();

            repo.Commit(new LogItem { Message = "1" });
            repo.Commit(new LogItem { Message = "2" });

            var remote = await new WebSocketRemote<LogItem>(_endpointUrl)
                .AttachAsync(repo);

            await Task.Delay(50);
            Assert.IsNotNull(remote.PullPointer);

            var remoteRepo = RepositoryContainer.GetRepositoryForTest();
            remoteRepo.Assert(
                new LogItem { Message = "1" },
                new LogItem { Message = "2" });

            repo.Assert(
                new LogItem { Message = "1" },
                new LogItem { Message = "2" });
        }
    }

    public static class TestExtends
    {
        public static void AssertEqual<T>(this Repository<T> repo, Repository<T> repo2, int expectedCount)
        {
            repo.WaitTill(expectedCount, 9).Wait();
            repo2.WaitTill(expectedCount, 9).Wait();
            var commits = repo.Commits.ToArray();
            var commits2 = repo2.Commits.ToArray();
            if (commits.Length != commits2.Length || commits.Length != expectedCount)
            {
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail($"The repo  don't match! Expected: {string.Join(',', commits2.Select(t => t.ToString()))}; Actual: {string.Join(',', repo.Commits.Select(t => t.ToString()))}");
            }
            for (var i = 0; i < commits.Length; i++)
            {
                if (!commits[i].Id.Equals(commits2[i].Id))
                {
                    Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail($"The repo don't match! Expected: {string.Join(',', commits2.Select(t => t.ToString()))}; Actual: {string.Join(',', repo.Commits.Select(t => t.ToString()))}");
                }
            }
        }
        public static void Assert<T>(this Repository<T> repo, params T[] array)
        {
            repo.WaitTill(array.Length, 9).Wait();
            var commits = repo.Commits.ToArray();
            for (var i = 0; i < commits.Length; i++)
            {
                if (!commits[i].Item.Equals(array[i]))
                {
                    Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail($"The repo don't match! Expected: {string.Join(',', array.Select(t => t.ToString()))}; Actual: {string.Join(',', repo.Commits.Select(t => t.ToString()))}");
                }
            }
        }

        private static async Task WaitTill<T>(this Repository<T> repo, int count, int maxWaitSeconds = 5)
        {
            var waitedTimes = 0;
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
