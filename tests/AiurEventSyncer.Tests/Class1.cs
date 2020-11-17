using AiurEventSyncer.Tests.TestDbs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace AiurEventSyncer.Tests
{
    [TestClass]
    public class Class1
    {
        [TestMethod]
        public void BasicPullTest()
        {
            var repo = new Repository<int>(new MemoryTestDb());
            var remoteRepo = new Repository<int>(new MemoryTestDb());
            remoteRepo.Commit(1);
            remoteRepo.Commit(2);
            remoteRepo.Commit(3);
            repo.Remotes.Add(new LocalRemote<int>(remoteRepo));
            repo.Pull(repo.Remotes.FirstOrDefault());
            AssertRepo(repo, 1, 2, 3);
            repo.Pull(repo.Remotes.FirstOrDefault());
            AssertRepo(repo, 1, 2, 3);
            AssertRepo(remoteRepo, 1, 2, 3);
        }

        private void AssertRepo<T>(Repository<T> repo, params T[] array)
        {
            for (int i = 0; i < repo.Commits.Query().Count(); i++)
            {
                Assert.AreEqual(repo.Commits.Query().ToArray()[i].Item, array[i]);
            }
            Assert.AreEqual(repo.Commits.Query().Count(), array.Length);
        }
    }
}
