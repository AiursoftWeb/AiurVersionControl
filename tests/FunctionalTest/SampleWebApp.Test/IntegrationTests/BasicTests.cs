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
            var repo = new Repository<LogItem>();
            repo.AddRemote(new WebSocketRemote<LogItem>(_endpointUrl));

            await repo.CommitAsync(new LogItem { Message = "1" });
            await repo.CommitAsync(new LogItem { Message = "2" });
            await repo.CommitAsync(new LogItem { Message = "3" });
            await repo.PushAsync();
            await Task.Delay(30);

            HomeController._repo.Assert(
                new LogItem { Message = "1" },
                new LogItem { Message = "2" },
                new LogItem { Message = "3" });

            var repo2 = new Repository<LogItem>();
            repo2.AddRemote(new WebSocketRemote<LogItem>(_endpointUrl));
            await Task.Delay(30);

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
            repo.AddRemote(remote);
            await Task.Delay(30);
            await repo.CommitAsync(new LogItem { Message = "1" });
            await repo.CommitAsync(new LogItem { Message = "2" });
            await Task.Delay(30);
            Assert.IsNotNull(remote.Position);

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
            repo.AddRemote(new WebSocketRemote<LogItem>(_endpointUrl));

            var repo2 = new Repository<LogItem>();
            repo2.AddRemote(new WebSocketRemote<LogItem>(_endpointUrl));

            await Task.Delay(30);
            await repo.CommitAsync(new LogItem { Message = "1" });
            await repo.CommitAsync(new LogItem { Message = "2" });
            await repo.CommitAsync(new LogItem { Message = "3" });
            await Task.Delay(300);

            Assert.AreNotEqual(null, repo.Remotes.First().Position);
            Assert.AreNotEqual(null, repo2.Remotes.First().Position);
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
            repoA.AddRemote(new WebSocketRemote<LogItem>(_endpointUrl) { Name = "A to server" });

            var repoB = new Repository<LogItem>() { Name = "Repo B" };
            repoB.AddRemote(new WebSocketRemote<LogItem>(_endpointUrl) { Name = "B to server" });

            await Task.Delay(30);
            await repoA.CommitAsync(new LogItem { Message = "1" });
            await Task.Delay(30);

            HomeController._repo.Assert(
                new LogItem { Message = "1" });
            repoA.Assert(
                new LogItem { Message = "1" });
            repoB.Assert(
                new LogItem { Message = "1" });
        }

        [TestMethod]
        public async Task DoubleWayDataBinding()
        {
            var repoA = new Repository<LogItem>() { Name = "Repo A" };
            repoA.AddRemote(new WebSocketRemote<LogItem>(_endpointUrl) { Name = "A to server" });

            var repoB = new Repository<LogItem>() { Name = "Repo B" };
            repoB.AddRemote(new WebSocketRemote<LogItem>(_endpointUrl) { Name = "B to server" });

            await Task.Delay(30);
            await repoA.CommitAsync(new LogItem { Message = "G" });
            await Task.Delay(30);
            await repoA.CommitAsync(new LogItem { Message = "H" });
            await Task.Delay(30);
            await repoB.CommitAsync(new LogItem { Message = "X" });
            await Task.Delay(30);
            await repoB.CommitAsync(new LogItem { Message = "Z" });
            await Task.Delay(30);

            HomeController._repo.Assert(
                new LogItem { Message = "G" },
                new LogItem { Message = "H" },
                new LogItem { Message = "X" },
                new LogItem { Message = "Z" });

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
