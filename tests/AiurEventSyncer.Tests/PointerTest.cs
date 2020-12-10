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
        public async Task TestPull()
        {
            var localRepo = new Repository<int>();
            var remote = new ObjectRemote<int>(_demoRepo);
            await localRepo.AddRemoteAsync(remote);

            Assert.IsNull(remote.Position);
            Assert.AreEqual(_demoRepo.Head.Item, 3);

            await localRepo.PullAsync();

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

            Assert.IsNull(remote.Position);
            Assert.AreEqual(_demoRepo.Head.Item, 3);

            await localRepo.PushAsync();

            Assert.IsNull(remote.Position);
            Assert.AreEqual(localRepo.Head.Item, 3);
            Assert.AreEqual(_demoRepo.Head.Item, 3);

            await localRepo.PullAsync();
            Assert.IsNotNull(remote.Position);
            Assert.AreEqual(localRepo.Head.Item, 3);
            Assert.AreEqual(_demoRepo.Head.Item, 3);
        }
    }
}
