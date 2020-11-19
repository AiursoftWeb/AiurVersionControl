using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
using AiurEventSyncer.Tests.Models;
using AiurEventSyncer.Tests.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace AiurEventSyncer.Tests
{
    [TestClass]
    public class DbRepoTest
    {
        [TestMethod]
        public void PushToAndPullFrom()
        {
            var dbRepo = BookDbRepoFactory.BuildRepo<Book>();
            var localRepo = new Repository<Book>();
            localRepo.Remotes.Add(new ObjectRemote<Book>(dbRepo, true));

            var localRepo2 = new Repository<Book>();
            localRepo2.AddAutoPullRemote(new ObjectRemote<Book>(dbRepo));

            localRepo.Commit(new Book { Name = "Love" });

            Assert.IsTrue(localRepo.Commits.FirstOrDefault().Item.Name == "Love");
            Assert.IsTrue(dbRepo.Commits.FirstOrDefault().Item.Name == "Love");
            Assert.IsTrue(localRepo2.Commits.FirstOrDefault().Item.Name == "Love");
        }
    }
}
