using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
using AiurEventSyncer.Tests.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace AiurEventSyncer.Tests
{
    [TestClass]
    public class MergeTest
    {
        private Repository<int> _localRepo;

        [TestInitialize]
        public async Task BuildBasicRepo()
        {
            _localRepo = new Repository<int>();
            await _localRepo.CommitAsync(1);
            await _localRepo.CommitAsync(2);
            await _localRepo.CommitAsync(3);
            _localRepo.Assert(1, 2, 3);
        }

        [TestMethod]
        public async Task TestPushWithDifferentTree()
        {
            var remoteRepo = new Repository<int>();
            await remoteRepo.CommitAsync(20);
            remoteRepo.Assert(20);

            await _localRepo.AddRemoteAsync(new ObjectRemote<int>(remoteRepo));

            await _localRepo.PullAsync();
            await remoteRepo.CommitAsync(50);

            _localRepo.Assert(20, 1, 2, 3);
            remoteRepo.Assert(20, 50);

            await _localRepo.PushAsync();
            _localRepo.Assert(20, 1, 2, 3);
            remoteRepo.Assert(20, 50, 1, 2, 3);

            await _localRepo.PullAsync();
            _localRepo.Assert(20, 50, 1, 2, 3);
            remoteRepo.Assert(20, 50, 1, 2, 3);
        }

        [TestMethod]
        public async Task TestPullWithDifferentTree()
        {
            var remoteRepo = new Repository<int>();
            await remoteRepo.CommitAsync(20);
            remoteRepo.Assert(20);

            await _localRepo.AddRemoteAsync(new ObjectRemote<int>(remoteRepo));

            await _localRepo.PullAsync();
            await remoteRepo.CommitAsync(50);

            _localRepo.Assert(20, 1, 2, 3);
            remoteRepo.Assert(20, 50);

            await _localRepo.PullAsync();
            _localRepo.Assert(20, 50, 1, 2, 3);
            remoteRepo.Assert(20, 50);

            await _localRepo.PushAsync();
            _localRepo.Assert(20, 50, 1, 2, 3);
            remoteRepo.Assert(20, 50, 1, 2, 3);
        }
    }
}
