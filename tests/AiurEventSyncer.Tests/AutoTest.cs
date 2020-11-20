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
            var remoteRecord = new ObjectRemote<int>(remoteRepo, autoPush: true);
            await _localRepo.AddRemoteAsync(remoteRecord);

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
            await localRepo.AddRemoteAsync(new ObjectRemote<int>(remoteRepo, autoPull: true));

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
            await a.AddRemoteAsync(new ObjectRemote<int>(b, autoPush: true, autoPull: true));

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

            await a.AddRemoteAsync(new ObjectRemote<int>(b, true, false) { Name ="a autopush b"});
            await c.AddRemoteAsync(new ObjectRemote<int>(b, false, true) { Name = "c autopull b" });
            await c.AddRemoteAsync(new ObjectRemote<int>(d, true, false) { Name = "c autopush d" });
            await e.AddRemoteAsync(new ObjectRemote<int>(d, false, true) { Name = "e autopull d" });

            await a.CommitAsync(5);
            b.Assert(5);
            c.Assert(5);
            d.Assert(5);
            e.Assert(5);
        }

        [TestMethod]
        public async Task DistributeTest()
        {
            //     server
            //    /       \
            //   sender    subscriber1,2,3

            var senderserver = new Repository<int>();
            var server = BookDbRepoFactory.BuildRepo<int>();
            var subscriber1 = new Repository<int>();
            var subscriber2 = new Repository<int>();
            var subscriber3 = new Repository<int>();

            await senderserver.AddRemoteAsync(new ObjectRemote<int>(server, true));
            await subscriber1.AddRemoteAsync(new ObjectRemote<int>(server, false, true));
            await subscriber2.AddRemoteAsync(new ObjectRemote<int>(server, false, true));
            await subscriber3.AddRemoteAsync(new ObjectRemote<int>(server, false, true));

            await senderserver.CommitAsync(5);
            server.Assert(5);
            subscriber1.Assert(5);
            subscriber2.Assert(5);
            subscriber3.Assert(5);
        }
    }
}
