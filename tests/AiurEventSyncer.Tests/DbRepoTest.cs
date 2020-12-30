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
            var dbRepo = BookDbRepoFactory.BuildBookRepo();
            var localRepo = new Repository<Book>();
            await new ObjectRemote<Book>(dbRepo, true).AttachAsync(localRepo);

            var localRepo2 = new Repository<Book>();
            await new ObjectRemote<Book>(dbRepo, false, true).AttachAsync(localRepo2);

            localRepo.Commit(new Book { Name = "Love" });
            while (!localRepo2.Commits.Any() || !dbRepo.Commits.Any())
            {
                await Task.Delay(10);
            }

            Assert.IsTrue(localRepo.Commits.FirstOrDefault().Item.Name == "Love");
            Assert.IsTrue(dbRepo.Commits.FirstOrDefault().Item.Name == "Love");
            Assert.IsTrue(localRepo2.Commits.FirstOrDefault().Item.Name == "Love");
        }
    }
}
