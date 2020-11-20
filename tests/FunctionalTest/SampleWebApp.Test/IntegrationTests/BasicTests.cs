using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SampleWebApp.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SampleWebApp.Tests.IntegrationTests
{
    [TestClass]
    public class BasicTests
    {
        private IHost _server;

        [TestInitialize]
        public async Task CreateServer()
        {
            _server = Program.BuildHost(null);
            await _server.StartAsync();
        }

        [TestCleanup]
        public async Task CleanServer()
        {
            await _server.StopAsync();
            _server.Dispose();
        }

        [TestMethod]
        public void MyTest()
        {
            var repo = new Repository<LogItem>();
            repo.Remotes.Add(new WebSocketRemote<LogItem>("http://localhost:15000/repo.are"));

            repo.Commit(new LogItem { Message = "1" });
            repo.Commit(new LogItem { Message = "2" });
            repo.Commit(new LogItem { Message = "3" });

            repo.Push();
            repo.Push();

            var repo2 = new Repository<LogItem>();
            repo2.Remotes.Add(new WebSocketRemote<LogItem>("http://localhost:15000/repo.are"));
            repo2.Pull();
            repo2.Pull();

            Assert.AreEqual(repo2.Commits.Count(), 3);
            Assert.AreEqual(repo2.Commits.ToArray()[0].Item.Message, "1");
            Assert.AreEqual(repo2.Commits.ToArray()[1].Item.Message, "2");
            Assert.AreEqual(repo2.Commits.ToArray()[2].Item.Message, "3");
        }
    }
}
