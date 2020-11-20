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
            await localRepo.AddRemoteAsync(new ObjectRemote<Book>(dbRepo, true, false));

            var localRepo2 = new Repository<Book>();
            await localRepo2.AddRemoteAsync(new ObjectRemote<Book>(dbRepo, false, true));

            await localRepo.CommitAsync(new Book { Name = "Love" });

            Assert.IsTrue(localRepo.Commits.FirstOrDefault().Item.Name == "Love");
            Assert.IsTrue(dbRepo.Commits.FirstOrDefault().Item.Name == "Love");
            Assert.IsTrue(localRepo2.Commits.FirstOrDefault().Item.Name == "Love");
        }
    }
}
