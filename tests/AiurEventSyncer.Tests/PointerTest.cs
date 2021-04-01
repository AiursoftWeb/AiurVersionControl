using AiurEventSyncer.Abstract;
using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
using AiurEventSyncer.Tests.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace AiurEventSyncer.Tests
{
    [TestClass]
    public class PointerTest
    {
        private Repository<int> _demoRepo;
        private const string CommitId3 = "5e641147de8c4306b56d19c053122854";

        [TestInitialize]
        public void GetBasicRepo()
        {
            _demoRepo = new Repository<int>();
            _demoRepo.Commit(1);
            _demoRepo.Commit(2);
            _demoRepo.CommitObject(new Commit<int>
            {
                Id = CommitId3,
                Item = 3
            });
            _demoRepo.Assert(1, 2, 3);
        }

        [TestMethod]
        public async Task TestPull()
        {
            var localRepo = new Repository<int>();
            var origin = await new ObjectRemote<int>(_demoRepo).AttachAsync(localRepo);

            Assert.AreEqual(origin.PullPointer, null);
            Assert.AreEqual(origin.PushPointer, null);
            Assert.AreEqual(localRepo.Head?.Item, null);
            Assert.AreEqual(_demoRepo.Head.Item, 3);

            await origin.PullAsync();

            Assert.IsNotNull(origin.PullPointer);
            Assert.IsNotNull(origin.PushPointer);
            Assert.AreEqual(localRepo.Head?.Item, 3);
            Assert.AreEqual(_demoRepo.Head.Item, 3);
        }

        [TestMethod]
        public async Task TestPush()
        {
            var remoteRepo = new Repository<int>();
            var localRepo = _demoRepo;
            var origin = await new ObjectRemote<int>(remoteRepo).AttachAsync(localRepo);

            Assert.AreEqual(origin.PullPointer, null);
            Assert.AreEqual(origin.PushPointer, null);
            Assert.AreEqual(localRepo.Head.Item, 3);
            Assert.AreEqual(remoteRepo.Head?.Item, null);

            await origin.PushAsync();

            Assert.AreEqual(origin.PullPointer, null);
            Assert.IsNotNull(origin.PushPointer);
            Assert.AreEqual(localRepo.Head.Item, 3);
            Assert.AreEqual(remoteRepo.Head?.Item, 3);

            await origin.PullAsync();
            Assert.IsNotNull(origin.PullPointer);
            Assert.IsNotNull(origin.PushPointer);
            Assert.AreEqual(localRepo.Head.Item, 3);
            Assert.AreEqual(remoteRepo.Head?.Item, 3);
        }

        [TestMethod]
        public async Task TestDrag()
        {
            var remoteRepo = new Repository<int>();
            var localRepo = _demoRepo;
            var origin = await new ObjectRemote<int>(remoteRepo).AttachAsync(localRepo);
            await origin.PushAsync();
            await origin.PullAsync();

            var square1 = new Commit<int> { Item = 111 };
            var square2 = new Commit<int> { Item = 222 };

            localRepo.CommitObject(square1);
            await origin.PushAsync();

            Assert.AreEqual(origin.PushPointer.Id, square1.Id);
            Assert.AreEqual(origin.PullPointer.Id, CommitId3);

            var tri1 = new Commit<int> { Item = 11111 };
            var tri2 = new Commit<int> { Item = 22222 };

            localRepo.CommitObject(square2);
            remoteRepo.CommitObject(tri1);
            remoteRepo.CommitObject(tri2);

            localRepo.Assert(1, 2, 3, 111, 222);
            remoteRepo.Assert(1, 2, 3, 111, 11111, 22222);

            await origin.PullAsync();

            localRepo.Assert(1, 2, 3, 111, 11111, 22222, 222);
            remoteRepo.Assert(1, 2, 3, 111, 11111, 22222);

            Assert.AreEqual(origin.PushPointer.Id, tri2.Id);
            Assert.AreEqual(origin.PullPointer.Id, tri2.Id);
        }

        [TestMethod]
        public async Task TestNotDrag()
        {
            var remoteRepo = new Repository<int>();
            var localRepo = _demoRepo;
            var origin = await new ObjectRemote<int>(remoteRepo).AttachAsync(localRepo);
            await origin.PushAsync();
            await origin.PullAsync();

            var square1 = new Commit<int> { Item = 111 };

            localRepo.CommitObject(square1);
            await origin.PushAsync();

            Assert.AreEqual(origin.PushPointer.Id, square1.Id);
            Assert.AreEqual(origin.PullPointer.Id, CommitId3);
            localRepo.Assert(1, 2, 3, 111);
            remoteRepo.Assert(1, 2, 3, 111);

            var square2 = new Commit<int> { Item = 222 };
            localRepo.CommitObject(square2);

            Assert.AreEqual(origin.PushPointer.Id, square1.Id);
            Assert.AreEqual(origin.PullPointer.Id, CommitId3);
            localRepo.Assert(1, 2, 3, 111, 222);
            remoteRepo.Assert(1, 2, 3, 111);

            await origin.PullAsync();

            Assert.AreEqual(origin.PushPointer.Id, square1.Id);
            Assert.AreEqual(origin.PullPointer.Id, square1.Id);
            localRepo.Assert(1, 2, 3, 111, 222);
            remoteRepo.Assert(1, 2, 3, 111);
        }
    }
}
