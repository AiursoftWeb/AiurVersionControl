using Aiursoft.AiurEventSyncer.Models;
using Aiursoft.AiurEventSyncer.Remotes;
using Aiursoft.AiurEventSyncer.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.AiurEventSyncer.Tests
{
    [TestClass]
    public class DbRepoTest
    {
        [TestMethod]
        public async Task PushToAndPullFrom()
        {
            var dbRepo = new Repository<Book>();
            var localRepo = new Repository<Book>();
            await new ObjectRemote<Book>(dbRepo, true).AttachAsync(localRepo);

            var localRepo2 = new Repository<Book>();
            await new ObjectRemote<Book>(dbRepo, false, true).AttachAsync(localRepo2);

            localRepo.Commit(new Book { Name = "Love" });
            while (!localRepo2.Commits.Any() || !dbRepo.Commits.Any())
            {
                await Task.Delay(10);
            }

            Assert.IsTrue(localRepo.Commits.First().Item.Name == "Love");
            Assert.IsTrue(dbRepo.Commits.First().Item.Name == "Love");
            Assert.IsTrue(localRepo2.Commits.First().Item.Name == "Love");
        }
    }
}
