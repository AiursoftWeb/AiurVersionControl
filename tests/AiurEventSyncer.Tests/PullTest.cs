using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
using AiurEventSyncer.Tests.TestDbs;
using AiurEventSyncer.Tests.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace AiurEventSyncer.Tests
{
    [TestClass]
    public class PullTest
    {
        private Repository<int> GetBasicRepo()
        {
            var repo = new Repository<int>(new MemoryTestDb());
            repo.Commit(1);
            repo.Commit(2);
            repo.Commit(3);
            TestExtends.AssertRepo(repo, 1, 2, 3);
            return repo;
        }

        [TestMethod]
        public void OncePullTest()
        {
            var remoteRepo = GetBasicRepo();

            var localRepo = new Repository<int>(new MemoryTestDb());
            localRepo.Remotes.Add(new ObjectRemote<int>(remoteRepo));
            localRepo.Pull();
            TestExtends.AssertRepo(localRepo, 1, 2, 3);

            localRepo.Pull();
            TestExtends.AssertRepo(localRepo, 1, 2, 3);
            TestExtends.AssertRepo(remoteRepo, 1, 2, 3);
        }

        [TestMethod]
        public void MultiplePullTest()
        {
            var remoteRepo = GetBasicRepo();

            var localRepo = new Repository<int>(new MemoryTestDb());
            localRepo.Remotes.Add(new ObjectRemote<int>(remoteRepo));
            localRepo.Pull();
            TestExtends.AssertRepo(localRepo, 1, 2, 3);

            remoteRepo.Commit(5);
            remoteRepo.Commit(7);
            localRepo.Pull();
            TestExtends.AssertRepo(localRepo, 1, 2, 3, 5, 7);

            TestExtends.AssertRepo(remoteRepo, 1, 2, 3, 5, 7);
        }

        [TestMethod]
        public void PullWithLocalCommitTest()
        {
            var remoteRepo = GetBasicRepo();

            var localRepo = new Repository<int>(new MemoryTestDb());
            localRepo.Remotes.Add(new ObjectRemote<int>(remoteRepo));
            localRepo.Pull();
            TestExtends.AssertRepo(localRepo, 1, 2, 3);

            localRepo.Commit(10);
            localRepo.Pull();

            TestExtends.AssertRepo(localRepo, 1, 2, 3, 10);
            TestExtends.AssertRepo(remoteRepo, 1, 2, 3);

            remoteRepo.Commit(20);
            localRepo.Pull();

            TestExtends.AssertRepo(localRepo, 1, 2, 3, 10, 20);
            TestExtends.AssertRepo(remoteRepo, 1, 2, 3, 20);
        }
    }
}
