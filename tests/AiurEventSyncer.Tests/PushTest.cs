using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
using AiurEventSyncer.Tests.Tools;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AiurEventSyncer.Tests
{
    [TestClass]
    public class PushTest
    {
        private Repository<int> _localRepo;

        [TestInitialize]
        public async Task GetBasicRepo()
        {
            _localRepo = new Repository<int>();
            await _localRepo.CommitAsync(1);
            await _localRepo.CommitAsync(2);
            await _localRepo.CommitAsync(3);
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
            await _localRepo.PushAsync(secondRemoteRecord);

            remoteRepo.Assert(1, 2, 3);
        }

        [TestMethod]
        public async Task PushMultipleTimesTest()
        {
            var remoteRepo = new Repository<int>();
            await _localRepo.AddRemoteAsync(new ObjectRemote<int>(remoteRepo));
            await _localRepo.PushAsync();
            remoteRepo.Assert(1, 2, 3);

            await _localRepo.CommitAsync(5);
            await _localRepo.CommitAsync(7);

            await _localRepo.PushAsync();

            _localRepo.Assert(1, 2, 3, 5, 7);
            remoteRepo.Assert(1, 2, 3, 5, 7);

            await _localRepo.CommitAsync(9);

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

            await remoteRepo.CommitAsync(100);
            await _localRepo.PushAsync();

            _localRepo.Assert(1, 2, 3);
            remoteRepo.Assert(1, 2, 3, 100);

            await _localRepo.CommitAsync(20);
            await _localRepo.CommitAsync(30);

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
            await remoteRepo.CommitObjectAsync(manualSyncedCommit);
            await _localRepo.CommitObjectAsync(manualSyncedCommit);

            _localRepo.Assert(1, 2, 3, 10);
            remoteRepo.Assert(1, 2, 3, 10);

            await _localRepo.CommitAsync(20);

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

            await remoteRepo.CommitAsync(1);

            var commit4 = new Commit<int>
            {
                Item = 4
            };
            await remoteRepo.CommitObjectAsync(commit4);
            await localRepo.CommitObjectAsync(commit4);

            var commit5 = new Commit<int>
            {
                Item = 5
            };
            await remoteRepo.CommitObjectAsync(commit5);
            await localRepo.CommitObjectAsync(commit5);

            var commit6 = new Commit<int>
            {
                Item = 6
            };
            await localRepo.CommitObjectAsync(commit6);

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
            await remoteRepo.CommitObjectAsync(manual10SyncedCommit);
            await remoteRepo.CommitObjectAsync(manual20SyncedCommit);
            await _localRepo.CommitObjectAsync(manual20SyncedCommit);
            await _localRepo.CommitObjectAsync(manual10SyncedCommit);
            await _localRepo.CommitAsync(300);

            _localRepo.Assert(1, 2, 3, 20, 10, 300);
            remoteRepo.Assert(1, 2, 3, 10, 20);

            await _localRepo.PushAsync();

            _localRepo.Assert(1, 2, 3, 20, 10, 300);
            remoteRepo.Assert(1, 2, 3, 10, 20, 20, 10, 300);
        }
    }
}
