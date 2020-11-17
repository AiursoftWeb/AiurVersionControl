using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
using AiurEventSyncer.Tests.TestDbs;
using AiurEventSyncer.Tests.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AiurEventSyncer.Tests
{
    [TestClass]
    public class PullTest
    {
        private Repository<int> _demoRepo;

        [TestInitialize]
        public void GetBasicRepo()
        {
            _demoRepo = new Repository<int>(new MemoryTestDb());
            _demoRepo.Commit(1);
            _demoRepo.Commit(2);
            _demoRepo.Commit(3);
            TestExtends.AssertRepo(_demoRepo, 1, 2, 3);
        }

        [TestMethod]
        public void MeaninglessPullTest()
        {
            var localRepo = new Repository<int>(new MemoryTestDb());
            localRepo.Remotes.Add(new ObjectRemote<int>(_demoRepo));

            localRepo.Pull();
            TestExtends.AssertRepo(localRepo, 1, 2, 3);

            localRepo.Pull();
            TestExtends.AssertRepo(localRepo, 1, 2, 3);
            TestExtends.AssertRepo(_demoRepo, 1, 2, 3);
        }

        [TestMethod]
        public void MultiplePullTest()
        {
            var localRepo = new Repository<int>(new MemoryTestDb());
            localRepo.Remotes.Add(new ObjectRemote<int>(_demoRepo));
            localRepo.Pull();
            TestExtends.AssertRepo(localRepo, 1, 2, 3);

            _demoRepo.Commit(5);
            _demoRepo.Commit(7);

            localRepo.Pull();

            TestExtends.AssertRepo(localRepo, 1, 2, 3, 5, 7);
            TestExtends.AssertRepo(_demoRepo, 1, 2, 3, 5, 7);

            _demoRepo.Commit(9);

            localRepo.Pull();

            TestExtends.AssertRepo(localRepo, 1, 2, 3, 5, 7, 9);
            TestExtends.AssertRepo(_demoRepo, 1, 2, 3, 5, 7, 9);
        }

        [TestMethod]
        public void PullWithLocalCommitTest()
        {
            var localRepo = new Repository<int>(new MemoryTestDb());
            localRepo.Remotes.Add(new ObjectRemote<int>(_demoRepo));
            localRepo.Pull();
            TestExtends.AssertRepo(localRepo, 1, 2, 3);

            localRepo.Commit(10);
            localRepo.Pull();

            TestExtends.AssertRepo(localRepo, 1, 2, 3, 10);
            TestExtends.AssertRepo(_demoRepo, 1, 2, 3);

            _demoRepo.Commit(20);

            TestExtends.AssertRepo(localRepo, 1, 2, 3, 10);
            TestExtends.AssertRepo(_demoRepo, 1, 2, 3, 20);

            localRepo.Pull();

            TestExtends.AssertRepo(localRepo, 1, 2, 3, 10, 20);
            TestExtends.AssertRepo(_demoRepo, 1, 2, 3, 20);
        }

        [TestMethod]
        public void PullWithManualCommitTest()
        {
            var localRepo = new Repository<int>(new MemoryTestDb());
            localRepo.Remotes.Add(new ObjectRemote<int>(_demoRepo));
            localRepo.Pull();
            TestExtends.AssertRepo(localRepo, 1, 2, 3);

            var manualSyncedCommit = new Commit<int>
            {
                Item = 10
            };
            localRepo.Commits.Add(manualSyncedCommit);
            _demoRepo.Commits.Add(manualSyncedCommit);

            TestExtends.AssertRepo(localRepo, 1, 2, 3, 10);
            TestExtends.AssertRepo(_demoRepo, 1, 2, 3, 10);

            _demoRepo.Commit(20);

            TestExtends.AssertRepo(localRepo, 1, 2, 3, 10);
            TestExtends.AssertRepo(_demoRepo, 1, 2, 3, 10, 20);

            localRepo.Pull();

            TestExtends.AssertRepo(localRepo, 1, 2, 3, 10, 20);
            TestExtends.AssertRepo(_demoRepo, 1, 2, 3, 10, 20);
        }

        [TestMethod]
        public void PullWithResetRemoteTest()
        {
            var localRepo = new Repository<int>(new MemoryTestDb());
            localRepo.Remotes.Add(new ObjectRemote<int>(_demoRepo));
            localRepo.Pull();
            TestExtends.AssertRepo(localRepo, 1, 2, 3);

            localRepo.Remotes.Clear();
            localRepo.Remotes.Add(new ObjectRemote<int>(_demoRepo));
            localRepo.Pull();

            TestExtends.AssertRepo(localRepo, 1, 2, 3);
        }
    }
}
