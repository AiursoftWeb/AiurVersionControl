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
        private const string commit3Id = "5e641147de8c4306b56d19c053122854";

        [TestInitialize]
        public async Task GetBasicRepo()
        {
            _demoRepo = new Repository<int>();
            await _demoRepo.CommitAsync(1);
            await _demoRepo.CommitAsync(2);
            await _demoRepo.CommitObjectAsync(new Commit<int>
            {
                Id = commit3Id,
                Item = 3
            });
            _demoRepo.Assert(1, 2, 3);
        }

        [TestMethod]
        public async Task TestPull()
        {
            var localRepo = new Repository<int>();
            var remote = new ObjectRemote<int>(_demoRepo);
            await localRepo.AddRemoteAsync(remote);

            Assert.AreEqual(remote.HEAD, null);
            Assert.AreEqual(remote.PushPointer, null);
            Assert.AreEqual(localRepo.Head?.Item, null);
            Assert.AreEqual(_demoRepo.Head.Item, 3);

            await localRepo.PullAsync();

            Assert.IsNotNull(remote.HEAD);
            Assert.IsNotNull(remote.PushPointer);
            Assert.AreEqual(localRepo.Head.Item, 3);
            Assert.AreEqual(_demoRepo.Head.Item, 3);
        }

        [TestMethod]
        public async Task TestPush()
        {
            var remoteRepo = new Repository<int>();
            var localRepo = _demoRepo;
            var remote = new ObjectRemote<int>(remoteRepo);
            await localRepo.AddRemoteAsync(remote);

            Assert.AreEqual(remote.HEAD, null);
            Assert.AreEqual(remote.PushPointer, null);
            Assert.AreEqual(localRepo.Head.Item, 3);
            Assert.AreEqual(remoteRepo.Head?.Item, null);

            await localRepo.PushAsync();

            Assert.AreEqual(remote.HEAD, null);
            Assert.IsNotNull(remote.PushPointer);
            Assert.AreEqual(localRepo.Head.Item, 3);
            Assert.AreEqual(remoteRepo.Head.Item, 3);

            await localRepo.PullAsync();
            Assert.IsNotNull(remote.HEAD);
            Assert.IsNotNull(remote.PushPointer);
            Assert.AreEqual(localRepo.Head.Item, 3);
            Assert.AreEqual(remoteRepo.Head.Item, 3);
        }

        [TestMethod]
        public async Task TestDrag()
        {
            var remoteRepo = new Repository<int>();
            var localRepo = _demoRepo;
            var remoteRecord = new ObjectRemote<int>(remoteRepo);
            await localRepo.AddRemoteAsync(remoteRecord);
            await localRepo.PushAsync();
            await localRepo.PullAsync();

            var square1 = new Commit<int> { Item = 111 };
            var square2 = new Commit<int> { Item = 222 };

            await localRepo.CommitObjectAsync(square1);
            await localRepo.PushAsync();

            Assert.AreEqual(remoteRecord.PushPointer, square1.Id);
            Assert.AreEqual(remoteRecord.HEAD, commit3Id);

            var tri1 = new Commit<int> { Item = 11111 };
            var tri2 = new Commit<int> { Item = 22222 };

            await localRepo.CommitObjectAsync(square2);
            await remoteRepo.CommitObjectAsync(tri1);
            await remoteRepo.CommitObjectAsync(tri2);

            localRepo.Assert(1, 2, 3, 111, 222);
            remoteRepo.Assert(1, 2, 3, 111, 11111,22222);

            await localRepo.PullAsync();

            localRepo.Assert(1, 2, 3, 111, 11111, 22222, 222);
            remoteRepo.Assert(1, 2, 3, 111, 11111, 22222);

            Assert.AreEqual(remoteRecord.PushPointer, tri2.Id);
            Assert.AreEqual(remoteRecord.HEAD, tri2.Id);
        }
    }
}
