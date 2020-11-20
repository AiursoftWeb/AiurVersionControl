using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
using AiurEventSyncer.Tests.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace AiurEventSyncer.Tests
{
    [TestClass]
    public class AutoTest
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
        public async Task TestAutoPush()
        {
            var remoteRepo = new Repository<int>();
            var remoteRecord = new ObjectRemote<int>(remoteRepo, true);
            _localRepo.Remotes.Add(remoteRecord);

            await _localRepo.CommitAsync(50);
            remoteRepo.Assert(1, 2, 3, 50);

            await _localRepo.CommitAsync(200);
            await _localRepo.CommitAsync(300);

            _localRepo.Assert(1, 2, 3, 50, 200, 300);
            remoteRepo.Assert(1, 2, 3, 50, 200, 300);
        }

        [TestMethod]
        public async Task TestAutoPull()
        {
            var remoteRepo = _localRepo;
            var localRepo = new Repository<int>();
            await localRepo.AddAutoPullRemoteAsync(new ObjectRemote<int>(remoteRepo));

            localRepo.Assert(1, 2, 3);

            await remoteRepo.CommitAsync(50);
            localRepo.Assert(1, 2, 3, 50);

            await remoteRepo.CommitAsync(200);
            await remoteRepo.CommitAsync(300);

            localRepo.Assert(1, 2, 3, 50, 200, 300);
            remoteRepo.Assert(1, 2, 3, 50, 200, 300);
        }

        [TestMethod]
        public async Task DoubleWaySync()
        {
            var a = new Repository<int>();
            var b = new Repository<int>();
            await a.AddAutoPullRemoteAsync(new ObjectRemote<int>(b, true));

            await a.CommitAsync(5);
            a.Assert(5);
            b.Assert(5);

            await b.CommitAsync(10);
            a.Assert(5, 10);
            b.Assert(5, 10);

            await a.CommitAsync(100);
            await b.CommitAsync(200);
            a.Assert(5, 10, 100, 200);
            b.Assert(5, 10, 100, 200);
        }

        [TestMethod]
        public async Task ComplicatedAutoTest()
        {
            //     B   D
            //    / \ / \
            //   A   C   E

            var a = new Repository<int>();
            var b = BookDbRepoFactory.BuildRepo<int>();
            var c = new Repository<int>();
            var d = new Repository<int>();
            var e = new Repository<int>();

            a.Remotes.Add(new ObjectRemote<int>(b, true));
            await c.AddAutoPullRemoteAsync(new ObjectRemote<int>(b));
            c.Remotes.Add(new ObjectRemote<int>(d, true));
            await e.AddAutoPullRemoteAsync(new ObjectRemote<int>(d));

            await a.CommitAsync(5);
            b.Assert(5);
            c.Assert(5);
            d.Assert(5);
            e.Assert(5);
        }
    }
}
