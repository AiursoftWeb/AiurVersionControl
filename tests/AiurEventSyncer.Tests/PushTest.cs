using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
using AiurEventSyncer.Tests.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

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
            _localRepo.Assert(1, 2, 3);
        }

        [TestMethod]
        public async Task PushSelfTest()
        {
            await _localRepo.AddRemoteAsync(new ObjectRemote<int>(_localRepo));
            _localRepo.Assert(1, 2, 3);

            await _localRepo.PushAsync();
            _localRepo.Assert(1, 2, 3);

            await _localRepo.PushAsync();
            _localRepo.Assert(1, 2, 3);
        }

        [TestMethod]
        public async Task MeaninglessPushTest()
        {
            var remoteRepo = new Repository<int>();
            await _localRepo.AddRemoteAsync(new ObjectRemote<int>(remoteRepo));

            await _localRepo.PushAsync();
            remoteRepo.Assert(1, 2, 3);

            await _localRepo.PushAsync();
            _localRepo.Assert(1, 2, 3);
            remoteRepo.Assert(1, 2, 3);
        }

        [TestMethod]
        public async Task PushWithResetRemoteTest()
        {
            var remoteRepo = new Repository<int>();
            await _localRepo.AddRemoteAsync(new ObjectRemote<int>(remoteRepo));
            await _localRepo.PushAsync();
            remoteRepo.Assert(1, 2, 3);

            var secondRemoteRecord = new ObjectRemote<int>(remoteRepo);
            await _localRepo.AddRemoteAsync(secondRemoteRecord);
            await secondRemoteRecord.Push();

            remoteRepo.Assert(1, 2, 3);
        }

        [TestMethod]
        public async Task PushMultipleTimesTest()
        {
            var remoteRepo = new Repository<int>();
            await _localRepo.AddRemoteAsync(new ObjectRemote<int>(remoteRepo));
            await _localRepo.PushAsync();
            remoteRepo.Assert(1, 2, 3);

            _localRepo.Commit(5);
            _localRepo.Commit(7);

            await _localRepo.PushAsync();

            _localRepo.Assert(1, 2, 3, 5, 7);
            remoteRepo.Assert(1, 2, 3, 5, 7);

            _localRepo.Commit(9);

            _localRepo.Assert(1, 2, 3, 5, 7, 9);
            remoteRepo.Assert(1, 2, 3, 5, 7);

            await _localRepo.PushAsync();

            _localRepo.Assert(1, 2, 3, 5, 7, 9);
            remoteRepo.Assert(1, 2, 3, 5, 7, 9);
        }

        [TestMethod]
        public async Task PushWithLocalCommitTest()
        {
            var remoteRepo = new Repository<int>();
            await _localRepo.AddRemoteAsync(new ObjectRemote<int>(remoteRepo));
            await _localRepo.PushAsync();
            remoteRepo.Assert(1, 2, 3);

            remoteRepo.Commit(100);
            await _localRepo.PushAsync();

            _localRepo.Assert(1, 2, 3);
            remoteRepo.Assert(1, 2, 3, 100);

            _localRepo.Commit(20);
            _localRepo.Commit(30);

            _localRepo.Assert(1, 2, 3, 20, 30);
            remoteRepo.Assert(1, 2, 3, 100);

            await _localRepo.PushAsync();

            _localRepo.Assert(1, 2, 3, 20, 30);
            remoteRepo.Assert(1, 2, 3, 100, 20, 30);
        }

        [TestMethod]
        public async Task PushWithManualCommitTest()
        {
            var remoteRepo = new Repository<int>();
            await _localRepo.AddRemoteAsync(new ObjectRemote<int>(remoteRepo));
            await _localRepo.PushAsync();
            remoteRepo.Assert(1, 2, 3);

            var manualSyncedCommit = new Commit<int>
            {
                Item = 10
            };
            remoteRepo.CommitObject(manualSyncedCommit);
            _localRepo.CommitObject(manualSyncedCommit);

            _localRepo.Assert(1, 2, 3, 10);
            remoteRepo.Assert(1, 2, 3, 10);

            _localRepo.Commit(20);

            _localRepo.Assert(1, 2, 3, 10, 20);
            remoteRepo.Assert(1, 2, 3, 10);

            await _localRepo.PushAsync();

            _localRepo.Assert(1, 2, 3, 10, 20);
            remoteRepo.Assert(1, 2, 3, 10, 20);
        }

        [TestMethod]
        public async Task PushWithManualSharedRangeTest()
        {
            var localRepo = new Repository<int>();
            var remoteRepo = new Repository<int>();
            await localRepo.AddRemoteAsync(new ObjectRemote<int>(remoteRepo));

            remoteRepo.Commit(1);

            var commit4 = new Commit<int>
            {
                Item = 4
            };
            remoteRepo.CommitObject(commit4);
            localRepo.CommitObject(commit4);

            var commit5 = new Commit<int>
            {
                Item = 5
            };
            remoteRepo.CommitObject(commit5);
            localRepo.CommitObject(commit5);

            var commit6 = new Commit<int>
            {
                Item = 6
            };
            localRepo.CommitObject(commit6);

            localRepo.Assert(4, 5, 6);
            remoteRepo.Assert(1, 4, 5);

            await localRepo.PushAsync();

            localRepo.Assert(4, 5, 6);
            remoteRepo.Assert(1, 4, 5, 4, 6);
        }

        [TestMethod]
        public async Task PushWithDiffOrderCommitsTest()
        {
            var remoteRepo = new Repository<int>();
            await _localRepo.AddRemoteAsync(new ObjectRemote<int>(remoteRepo));
            await _localRepo.PushAsync();
            remoteRepo.Assert(1, 2, 3);

            var manual10SyncedCommit = new Commit<int>
            {
                Item = 10
            };
            var manual20SyncedCommit = new Commit<int>
            {
                Item = 20
            };
            remoteRepo.CommitObject(manual10SyncedCommit);
            remoteRepo.CommitObject(manual20SyncedCommit);
            _localRepo.CommitObject(manual20SyncedCommit);
            _localRepo.CommitObject(manual10SyncedCommit);
            _localRepo.Commit(300);

            _localRepo.Assert(1, 2, 3, 20, 10, 300);
            remoteRepo.Assert(1, 2, 3, 10, 20);

            await _localRepo.PushAsync();

            _localRepo.Assert(1, 2, 3, 20, 10, 300);
            remoteRepo.Assert(1, 2, 3, 10, 20, 20, 10, 300);
        }
    }
}
