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
        public async Task GetBasicRepo()
        {
            _demoRepo = new Repository<int>();
            await _demoRepo.CommitAsync(1);
            await _demoRepo.CommitAsync(2);
            await _demoRepo.CommitAsync(3);
            _demoRepo.Assert(1, 2, 3);
        }

        [TestMethod]
        public async Task PullSelfTest()
        {
            _demoRepo.Remotes.Add(new ObjectRemote<int>(_demoRepo));
            await _demoRepo.PullAsync();
            _demoRepo.Assert(1, 2, 3);

            await _demoRepo.PullAsync();
            _demoRepo.Assert(1, 2, 3);
        }

        [TestMethod]
        public async Task MeaninglessPullTest()
        {
            var localRepo = new Repository<int>();
            localRepo.Remotes.Add(new ObjectRemote<int>(_demoRepo));

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
            localRepo.Remotes.Add(new ObjectRemote<int>(_demoRepo));
            await localRepo.PullAsync();
            localRepo.Assert(1, 2, 3);

            localRepo.Remotes.Clear();
            localRepo.Remotes.Add(new ObjectRemote<int>(_demoRepo));
            await localRepo.PullAsync();

            localRepo.Assert(1, 2, 3);
        }

        [TestMethod]
        public async Task PullMultipleTimesTest()
        {
            var localRepo = new Repository<int>();
            localRepo.Remotes.Add(new ObjectRemote<int>(_demoRepo));
            await localRepo.PullAsync();
            localRepo.Assert(1, 2, 3);

            await _demoRepo.CommitAsync(5);
            await _demoRepo.CommitAsync(7);

            await localRepo.PullAsync();

            localRepo.Assert(1, 2, 3, 5, 7);
            _demoRepo.Assert(1, 2, 3, 5, 7);

            await _demoRepo.CommitAsync(9);

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
            localRepo.Remotes.Add(new ObjectRemote<int>(_demoRepo));
            await localRepo.PullAsync();
            localRepo.Assert(1, 2, 3);

            await localRepo.CommitAsync(100);
            await localRepo.PullAsync();

            localRepo.Assert(1, 2, 3, 100);
            _demoRepo.Assert(1, 2, 3);

            await _demoRepo.CommitAsync(20);
            await _demoRepo.CommitAsync(30);

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
            localRepo.Remotes.Add(new ObjectRemote<int>(_demoRepo));
            await localRepo.PullAsync();
            localRepo.Assert(1, 2, 3);

            var manualSyncedCommit = new Commit<int>
            {
                Item = 10
            };
            localRepo.Commits.Add(manualSyncedCommit);
            _demoRepo.Commits.Add(manualSyncedCommit);

            localRepo.Assert(1, 2, 3, 10);
            _demoRepo.Assert(1, 2, 3, 10);

            await _demoRepo.CommitAsync(20);

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
            localRepo.Remotes.Add(new ObjectRemote<int>(_demoRepo));
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
            localRepo.Commits.Add(manual10SyncedCommit);
            localRepo.Commits.Add(manual20SyncedCommit);
            _demoRepo.Commits.Add(manual20SyncedCommit);
            _demoRepo.Commits.Add(manual10SyncedCommit);
            await _demoRepo.CommitAsync(300);

            localRepo.Assert(1, 2, 3, 10, 20);
            _demoRepo.Assert(1, 2, 3, 20, 10, 300);

            await localRepo.PullAsync();

            localRepo.Assert(1, 2, 3, 20, 10, 300, 20);
            _demoRepo.Assert(1, 2, 3, 20, 10, 300);
        }
    }
}
