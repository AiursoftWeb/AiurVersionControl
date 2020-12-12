using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
using AiurEventSyncer.Tests.Models;
using AiurEventSyncer.Tests.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace AiurEventSyncer.Tests
{
    [TestClass]
    public class DbRepoTest
    {
        [TestMethod]
        public async Task PushToAndPullFrom()
        {
            var dbRepo = BookDbRepoFactory.BuildRepo<Book>();
            var localRepo = new Repository<Book>();
            localRepo.AddRemote(new ObjectRemote<Book>(dbRepo, true));

            var localRepo2 = new Repository<Book>();
            localRepo2.AddRemote(new ObjectRemote<Book>(dbRepo, false, true));

            await localRepo.CommitAsync(new Book { Name = "Love" });
            await Task.Delay(30);

            Assert.IsTrue(localRepo.Commits.FirstOrDefault().Item.Name == "Love");
            Assert.IsTrue(dbRepo.Commits.FirstOrDefault().Item.Name == "Love");
            Assert.IsTrue(localRepo2.Commits.FirstOrDefault().Item.Name == "Love");
        }
    }
}
