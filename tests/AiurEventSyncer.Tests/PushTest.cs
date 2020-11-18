using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
using AiurEventSyncer.Tests.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AiurEventSyncer.Tests
{
    [TestClass]
    public class PushTest
    {
        private Repository<int> _localRepo;

        [TestInitialize]
        public void GetBasicRepo()
        {
            _localRepo = new Repository<int>();
            _localRepo.Commit(1);
            _localRepo.Commit(2);
            _localRepo.Commit(3);
            TestExtends.AssertRepo(_localRepo, 1, 2, 3);
        }

        [TestMethod]
        public void PushSelfTest()
        {
            _localRepo.Remotes.Add(new ObjectRemote<int>(_localRepo));
            _localRepo.Push();
            TestExtends.AssertRepo(_localRepo, 1, 2, 3);

            _localRepo.Push();
            TestExtends.AssertRepo(_localRepo, 1, 2, 3);
        }

        [TestMethod]
        public void MeaninglessPushTest()
        {
            var remoteRepo = new Repository<int>();
            _localRepo.Remotes.Add(new ObjectRemote<int>(remoteRepo));

            _localRepo.Push();
            TestExtends.AssertRepo(remoteRepo, 1, 2, 3);

            _localRepo.Push();
            TestExtends.AssertRepo(_localRepo, 1, 2, 3);
            TestExtends.AssertRepo(remoteRepo, 1, 2, 3);
        }

        [TestMethod]
        public void PushWithResetRemoteTest()
        {
            var remoteRepo = new Repository<int>();
            _localRepo.Remotes.Add(new ObjectRemote<int>(remoteRepo));
            _localRepo.Push();
            TestExtends.AssertRepo(remoteRepo, 1, 2, 3);

            _localRepo.Remotes.Clear();
            _localRepo.Remotes.Add(new ObjectRemote<int>(remoteRepo));
            _localRepo.Push();

            TestExtends.AssertRepo(remoteRepo, 1, 2, 3);
        }

        [TestMethod]
        public void PushMultipleTimesTest()
        {
            var remoteRepo = new Repository<int>();
            _localRepo.Remotes.Add(new ObjectRemote<int>(remoteRepo));
            _localRepo.Push();
            TestExtends.AssertRepo(remoteRepo, 1, 2, 3);

            _localRepo.Commit(5);
            _localRepo.Commit(7);

            _localRepo.Push();

            TestExtends.AssertRepo(_localRepo, 1, 2, 3, 5, 7);
            TestExtends.AssertRepo(remoteRepo, 1, 2, 3, 5, 7);

            _localRepo.Commit(9);

            TestExtends.AssertRepo(_localRepo, 1, 2, 3, 5, 7, 9);
            TestExtends.AssertRepo(remoteRepo, 1, 2, 3, 5, 7);

            _localRepo.Push();

            TestExtends.AssertRepo(_localRepo, 1, 2, 3, 5, 7, 9);
            TestExtends.AssertRepo(remoteRepo, 1, 2, 3, 5, 7, 9);
        }

        [TestMethod]
        public void PushWithLocalCommitTest()
        {
            var remoteRepo = new Repository<int>();
            _localRepo.Remotes.Add(new ObjectRemote<int>(remoteRepo));
            _localRepo.Push();
            TestExtends.AssertRepo(remoteRepo, 1, 2, 3);

            remoteRepo.Commit(100);
            _localRepo.Push();

            TestExtends.AssertRepo(_localRepo, 1, 2, 3);
            TestExtends.AssertRepo(remoteRepo, 1, 2, 3, 100);

            _localRepo.Commit(20);
            _localRepo.Commit(30);

            TestExtends.AssertRepo(_localRepo, 1, 2, 3, 20, 30);
            TestExtends.AssertRepo(remoteRepo, 1, 2, 3, 100);

            _localRepo.Push();

            TestExtends.AssertRepo(_localRepo, 1, 2, 3, 20, 30);
            TestExtends.AssertRepo(remoteRepo, 1, 2, 3, 100, 20, 30);
        }

        [TestMethod]
        public void PushWithManualCommitTest()
        {
            var remoteRepo = new Repository<int>();
            _localRepo.Remotes.Add(new ObjectRemote<int>(remoteRepo));
            _localRepo.Push();
            TestExtends.AssertRepo(remoteRepo, 1, 2, 3);

            var manualSyncedCommit = new Commit<int>
            {
                Item = 10
            };
            remoteRepo.Commits.Add(manualSyncedCommit);
            _localRepo.Commits.Add(manualSyncedCommit);

            TestExtends.AssertRepo(_localRepo, 1, 2, 3, 10);
            TestExtends.AssertRepo(remoteRepo, 1, 2, 3, 10);

            _localRepo.Commit(20);

            TestExtends.AssertRepo(_localRepo, 1, 2, 3, 10, 20);
            TestExtends.AssertRepo(remoteRepo, 1, 2, 3, 10);

            _localRepo.Push();

            TestExtends.AssertRepo(_localRepo, 1, 2, 3, 10, 20);
            TestExtends.AssertRepo(remoteRepo, 1, 2, 3, 10, 20);
        }

        [TestMethod]
        public void PushWithDiffOrderCommitsTest()
        {
            var remoteRepo = new Repository<int>();
            _localRepo.Remotes.Add(new ObjectRemote<int>(remoteRepo));
            _localRepo.Push();
            TestExtends.AssertRepo(remoteRepo, 1, 2, 3);

            var manual10SyncedCommit = new Commit<int>
            {
                Item = 10
            };
            var manual20SyncedCommit = new Commit<int>
            {
                Item = 20
            };
            remoteRepo.Commits.Add(manual10SyncedCommit);
            remoteRepo.Commits.Add(manual20SyncedCommit);
            _localRepo.Commits.Add(manual20SyncedCommit);
            _localRepo.Commits.Add(manual10SyncedCommit);
            _localRepo.Commit(300);

            TestExtends.AssertRepo(_localRepo, 1, 2, 3, 20, 10, 300);
            TestExtends.AssertRepo(remoteRepo, 1, 2, 3, 10, 20);

            _localRepo.Push();

            TestExtends.AssertRepo(_localRepo, 1, 2, 3, 20, 10, 300);
            TestExtends.AssertRepo(remoteRepo, 1, 2, 3, 10, 20, 10, 300);
        }
    }
}
