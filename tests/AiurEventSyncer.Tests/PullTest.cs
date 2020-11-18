using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
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
            _demoRepo = new Repository<int>();
            _demoRepo.Commit(1);
            _demoRepo.Commit(2);
            _demoRepo.Commit(3);
            TestExtends.AssertRepo(_demoRepo, 1, 2, 3);
        }

        [TestMethod]
        public void PullSelfTest()
        {
            _demoRepo.Remotes.Add(new ObjectRemote<int>(_demoRepo));
            _demoRepo.Pull();
            TestExtends.AssertRepo(_demoRepo, 1, 2, 3);

            _demoRepo.Pull();
            TestExtends.AssertRepo(_demoRepo, 1, 2, 3);
        }

        [TestMethod]
        public void MeaninglessPullTest()
        {
            var localRepo = new Repository<int>();
            localRepo.Remotes.Add(new ObjectRemote<int>(_demoRepo));

            localRepo.Pull();
            TestExtends.AssertRepo(localRepo, 1, 2, 3);

            localRepo.Pull();
            TestExtends.AssertRepo(localRepo, 1, 2, 3);
            TestExtends.AssertRepo(_demoRepo, 1, 2, 3);
        }

        [TestMethod]
        public void PullWithResetRemoteTest()
        {
            var localRepo = new Repository<int>();
            localRepo.Remotes.Add(new ObjectRemote<int>(_demoRepo));
            localRepo.Pull();
            TestExtends.AssertRepo(localRepo, 1, 2, 3);

            localRepo.Remotes.Clear();
            localRepo.Remotes.Add(new ObjectRemote<int>(_demoRepo));
            localRepo.Pull();

            TestExtends.AssertRepo(localRepo, 1, 2, 3);
        }

        [TestMethod]
        public void PullMultipleTimesTest()
        {
            var localRepo = new Repository<int>();
            localRepo.Remotes.Add(new ObjectRemote<int>(_demoRepo));
            localRepo.Pull();
            TestExtends.AssertRepo(localRepo, 1, 2, 3);

            _demoRepo.Commit(5);
            _demoRepo.Commit(7);

            localRepo.Pull();

            TestExtends.AssertRepo(localRepo, 1, 2, 3, 5, 7);
            TestExtends.AssertRepo(_demoRepo, 1, 2, 3, 5, 7);

            _demoRepo.Commit(9);

            TestExtends.AssertRepo(localRepo, 1, 2, 3, 5, 7);
            TestExtends.AssertRepo(_demoRepo, 1, 2, 3, 5, 7, 9);

            localRepo.Pull();

            TestExtends.AssertRepo(localRepo, 1, 2, 3, 5, 7, 9);
            TestExtends.AssertRepo(_demoRepo, 1, 2, 3, 5, 7, 9);
        }

        [TestMethod]
        public void PullWithLocalCommitTest()
        {
            var localRepo = new Repository<int>();
            localRepo.Remotes.Add(new ObjectRemote<int>(_demoRepo));
            localRepo.Pull();
            TestExtends.AssertRepo(localRepo, 1, 2, 3);

            localRepo.Commit(100);
            localRepo.Pull();

            TestExtends.AssertRepo(localRepo, 1, 2, 3, 100);
            TestExtends.AssertRepo(_demoRepo, 1, 2, 3);

            _demoRepo.Commit(20);
            _demoRepo.Commit(30);

            TestExtends.AssertRepo(localRepo, 1, 2, 3, 100);
            TestExtends.AssertRepo(_demoRepo, 1, 2, 3, 20, 30);

            localRepo.Pull();

            TestExtends.AssertRepo(localRepo, 1, 2, 3, 20, 30, 100);
            TestExtends.AssertRepo(_demoRepo, 1, 2, 3, 20, 30);
        }

        [TestMethod]
        public void PullWithManualCommitTest()
        {
            var localRepo = new Repository<int>();
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
        public void PullWithDiffOrderCommitsTest()
        {
            var localRepo = new Repository<int>();
            localRepo.Remotes.Add(new ObjectRemote<int>(_demoRepo));
            localRepo.Pull();
            TestExtends.AssertRepo(localRepo, 1, 2, 3);

            var manual10SyncedCommit = new Commit<int>
            {
                Item = 10
            };
            var manual20SyncedCommit = new Commit<int>
            {
                Item = 20
            };
            localRepo.Commits.Add(manual10SyncedCommit);
            localRepo.Commits.Add(manual20SyncedCommit);
            _demoRepo.Commits.Add(manual20SyncedCommit);
            _demoRepo.Commits.Add(manual10SyncedCommit);
            _demoRepo.Commit(300);

            TestExtends.AssertRepo(localRepo, 1, 2, 3, 10, 20);
            TestExtends.AssertRepo(_demoRepo, 1, 2, 3, 20, 10, 300);

            localRepo.Pull();

            TestExtends.AssertRepo(localRepo, 1, 2, 3, 20, 10, 300, 20);
            TestExtends.AssertRepo(_demoRepo, 1, 2, 3, 20, 10, 300);
        }
    }
}
