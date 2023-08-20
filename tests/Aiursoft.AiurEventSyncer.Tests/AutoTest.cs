using Aiursoft.AiurEventSyncer.Models;
using Aiursoft.AiurEventSyncer.Remotes;
using Aiursoft.AiurEventSyncer.Tests.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.AiurEventSyncer.Tests
{
    [TestClass]
    public class AutoTest
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
        public async Task TestAutoPush()
        {
            var remoteRepo = new Repository<int>();
            await new ObjectRemote<int>(remoteRepo, autoPush: true).AttachAsync(_localRepo);

             _localRepo.Commit(50);
            remoteRepo.Assert(1, 2, 3, 50);

            _localRepo.Commit(200);
            _localRepo.Commit(300);

            _localRepo.Assert(1, 2, 3, 50, 200, 300);
            remoteRepo.Assert(1, 2, 3, 50, 200, 300);
        }

        [TestMethod]
        public async Task TestAutoPull()
        {
            var remoteRepo = _localRepo;
            var localRepo = new Repository<int>();
            await new ObjectRemote<int>(remoteRepo, autoPush: false, autoPull: true).AttachAsync(localRepo);
            localRepo.Assert(1, 2, 3);

            remoteRepo.Commit(50);
            localRepo.Assert(1, 2, 3, 50);

            remoteRepo.Commit(200);
            remoteRepo.Commit(300);

            localRepo.Assert(1, 2, 3, 50, 200, 300);
            remoteRepo.Assert(1, 2, 3, 50, 200, 300);
        }

        [TestMethod]
        public async Task DoubleWaySync()
        {
            var a = new Repository<int>();
            var b = new Repository<int>();
            await new ObjectRemote<int>(b, autoPush: true, autoPull: true).AttachAsync(a);

            a.Commit(5);
            a.Assert(5);
            b.Assert(5);

            b.Commit(10);
            a.Assert(5, 10);
            b.Assert(5, 10);

            a.Commit(100);
            b.Commit(200); // This is faster. Because while B is saving 200, A hasn't push 100.
            a.Assert(5, 10, 200, 100);
            b.Assert(5, 10, 200, 100);
        }

        [TestMethod]
        public async Task ComplicatedAutoTest()
        {
            //     B   D
            //    / \ / \
            //   A   C   E

            var a = new Repository<int>();
            var b = new Repository<int>();
            var c = new Repository<int>();
            var d = new Repository<int>();
            var e = new Repository<int>();

            await new ObjectRemote<int>(b, true).AttachAsync(a);
            await new ObjectRemote<int>(b, false, true).AttachAsync(c);
            await new ObjectRemote<int>(d, true).AttachAsync(c);
            await new ObjectRemote<int>(d, false, true).AttachAsync(e);

            a.Commit(5);

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
            var server = new Repository<int>();
            var subscriber1 = new Repository<int>();
            var subscriber2 = new Repository<int>();
            var subscriber3 = new Repository<int>();

            await new ObjectRemote<int>(server, true).AttachAsync(senderserver);
            await new ObjectRemote<int>(server, false, true).AttachAsync(subscriber1);
            await new ObjectRemote<int>(server, false, true).AttachAsync(subscriber2);
            senderserver.Commit(5);
            await new ObjectRemote<int>(server, false, true).AttachAsync(subscriber3);

            senderserver.Assert(5);
            server.Assert(5);
            subscriber1.Assert(5);
            subscriber2.Assert(5);
            subscriber3.Assert(5);
        }

        [TestMethod]
        public async Task DropTest()
        {
            //     server
            //    /       \
            //   sender    subscriber
            var sender = new Repository<int>();
            var server = new Repository<int>();
            var subscriber = new Repository<int>();
            await new ObjectRemote<int>(server, true).AttachAsync(sender);

            var subscription = await new ObjectRemote<int>(server, false, true)
                .AttachAsync(subscriber);

            sender.Commit(5);
            sender.Commit(10);
            server.Assert(5, 10);
            subscriber.Assert(5, 10);

            await subscription.DetachAsync();
            sender.Commit(100);
            sender.Commit(200);
            server.Assert(5, 10, 100, 200);
            subscriber.Assert(5, 10);
        }

        [TestMethod]
        public async Task ReattachTest()
        {
            //     server
            //    /       \
            //   sender    subscriber
            var sender = new Repository<int>();
            var server = new Repository<int>();
            var subscriber = new Repository<int>();
            await new ObjectRemote<int>(server, true).AttachAsync(sender);

            var subscription = await new ObjectRemote<int>(server, false, true)
                .AttachAsync(subscriber);

            sender.Commit(5);
            sender.Commit(10);
            server.Assert(5, 10);
            subscriber.Assert(5, 10);

            await subscription.DetachAsync();
            sender.Commit(100);
            sender.Commit(200);
            server.Assert(5, 10, 100, 200);
            subscriber.Assert(5, 10);

            await subscription.AttachAsync(subscriber);
            subscriber.Assert(5, 10, 100, 200);
        }
    }
}
