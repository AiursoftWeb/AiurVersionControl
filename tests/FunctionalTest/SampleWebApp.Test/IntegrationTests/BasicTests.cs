using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
using SampleWebApp;
using SampleWebApp.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SampleWebApp.Tests.IntegrationTests
{
    public class BasicTests : IClassFixture<SampleWebAppFactory<Startup>>
    {
        private readonly SampleWebAppFactory<Startup> _factory;

        public BasicTests(SampleWebAppFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public void MyTest()
        {
            var repo = new Repository<LogItem>();
            repo.Remotes.Add(new WebSocketRemote<LogItem>("http://localhost:5000/repo.are"));

            repo.Commit(new LogItem { Message = "1" });
            repo.Commit(new LogItem { Message = "2" });
            repo.Commit(new LogItem { Message = "3" });

            repo.Push();

            var repo2 = new Repository<LogItem>();
            repo2.Remotes.Add(new WebSocketRemote<LogItem>("http://localhost:5000/repo.are"));
            repo2.Pull();


            Assert.True(repo2.Commits.Count() == 3);
        }
    }
}
