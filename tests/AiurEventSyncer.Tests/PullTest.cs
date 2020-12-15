using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
using AiurEventSyncer.Tests.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

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
            _demoRepo.Assert(1, 2, 3);
        }

        [TestMethod]
        public async Task PullSelfTest()
        {
            await _demoRepo.AddRemoteAsync(new ObjectRemote<int>(_demoRepo));
            await _demoRepo.PullAsync();
            _demoRepo.Assert(1, 2, 3);

            await _demoRepo.PullAsync();
            _demoRepo.Assert(1, 2, 3);
        }

        [TestMethod]
        public async Task MeaninglessPullTest()
        {
            var localRepo = new Repository<int>();
            await localRepo.AddRemoteAsync(new ObjectRemote<int>(_demoRepo));

            await localRepo.PullAsync();
            localRepo.Assert(1, 2, 3);

            await localRepo.PullAsync();
            localRepo.Assert(1, 2, 3);
            _demoRepo.Assert(1, 2, 3);
        }

        [TestMethod]
        public async Task PullWithResetRemoteTest()
        {
            var localRepo = new Repository<int>();
            await localRepo.AddRemoteAsync(new ObjectRemote<int>(_demoRepo));
            await localRepo.PullAsync();
            localRepo.Assert(1, 2, 3);

            var record = new ObjectRemote<int>(_demoRepo);
            await localRepo.AddRemoteAsync(record);
            await localRepo.PullAsync();

            localRepo.Assert(1, 2, 3);
        }

        [TestMethod]
        public async Task PullMultipleTimesTest()
        {
            var localRepo = new Repository<int>();
            await localRepo.AddRemoteAsync(new ObjectRemote<int>(_demoRepo));
            await localRepo.PullAsync();
            localRepo.Assert(1, 2, 3);

            _demoRepo.Commit(5);
            _demoRepo.Commit(7);

            await localRepo.PullAsync();

            localRepo.Assert(1, 2, 3, 5, 7);
            _demoRepo.Assert(1, 2, 3, 5, 7);

            _demoRepo.Commit(9);

            localRepo.Assert(1, 2, 3, 5, 7);
            _demoRepo.Assert(1, 2, 3, 5, 7, 9);

            await localRepo.PullAsync();

            localRepo.Assert(1, 2, 3, 5, 7, 9);
            _demoRepo.Assert(1, 2, 3, 5, 7, 9);
        }

        [TestMethod]
        public async Task PullWithLocalCommitTest()
        {
            var localRepo = new Repository<int>();
            await localRepo.AddRemoteAsync(new ObjectRemote<int>(_demoRepo));
            await localRepo.PullAsync();
            localRepo.Assert(1, 2, 3);

            localRepo.Commit(100);
            await localRepo.PullAsync();

            localRepo.Assert(1, 2, 3, 100);
            _demoRepo.Assert(1, 2, 3);

            _demoRepo.Commit(20);
            _demoRepo.Commit(30);

            localRepo.Assert(1, 2, 3, 100);
            _demoRepo.Assert(1, 2, 3, 20, 30);

            await localRepo.PullAsync();

            localRepo.Assert(1, 2, 3, 20, 30, 100);
            _demoRepo.Assert(1, 2, 3, 20, 30);
        }

        [TestMethod]
        public async Task PullWithManualCommitTest()
        {
            var localRepo = new Repository<int>();
            await localRepo.AddRemoteAsync(new ObjectRemote<int>(_demoRepo));
            await localRepo.PullAsync();
            localRepo.Assert(1, 2, 3);

            var manualSyncedCommit = new Commit<int>
            {
                Item = 10
            };
            localRepo.CommitObject(manualSyncedCommit);
            _demoRepo.CommitObject(manualSyncedCommit);

            localRepo.Assert(1, 2, 3, 10);
            _demoRepo.Assert(1, 2, 3, 10);

            _demoRepo.Commit(20);

            localRepo.Assert(1, 2, 3, 10);
            _demoRepo.Assert(1, 2, 3, 10, 20);

            await localRepo.PullAsync();

            localRepo.Assert(1, 2, 3, 10, 20);
            _demoRepo.Assert(1, 2, 3, 10, 20);
        }

        [TestMethod]
        public async Task PullWithDiffOrderCommitsTest()
        {
            var localRepo = new Repository<int>();
            await localRepo.AddRemoteAsync(new ObjectRemote<int>(_demoRepo));
            await localRepo.PullAsync();
            localRepo.Assert(1, 2, 3);

            var manual10SyncedCommit = new Commit<int>
            {
                Item = 10
            };
            var manual20SyncedCommit = new Commit<int>
            {
                Item = 20
            };
            localRepo.CommitObject(manual10SyncedCommit);
            localRepo.CommitObject(manual20SyncedCommit);
            _demoRepo.CommitObject(manual20SyncedCommit);
            _demoRepo.CommitObject(manual10SyncedCommit);
            _demoRepo.Commit(300);

            localRepo.Assert(1, 2, 3, 10, 20);
            _demoRepo.Assert(1, 2, 3, 20, 10, 300);

            await localRepo.PullAsync();

            localRepo.Assert(1, 2, 3, 20, 10, 300, 20);
            _demoRepo.Assert(1, 2, 3, 20, 10, 300);
        }
    }
}
