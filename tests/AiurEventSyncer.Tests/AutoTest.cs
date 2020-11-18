using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
using AiurEventSyncer.Tests.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AiurEventSyncer.Tests
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
        public void TestAutoPush()
        {
            var remoteRepo = new Repository<int>();
            var remoteRecord = new ObjectRemote<int>(remoteRepo)
            {
                AutoPushToIt = true
            };
            _localRepo.Remotes.Add(remoteRecord);

            _localRepo.Commit(50);
            remoteRepo.Assert(1, 2, 3, 50);

            _localRepo.Commit(200);
            _localRepo.Commit(300);

            _localRepo.Assert(1, 2, 3, 50, 200, 300);
            remoteRepo.Assert(1, 2, 3, 50, 200, 300);
        }

        [TestMethod]
        public void TestAutoPull()
        {
            var remoteRepo = _localRepo;
            var localRepo = new Repository<int>();
            var remoteRecord = new ObjectRemote<int>(remoteRepo);
            localRepo.Remotes.Add(remoteRecord);
            localRepo.RegisterAutoPull(remoteRecord);

            localRepo.Assert(1, 2, 3);

            remoteRepo.Commit(50);
            localRepo.Assert(1, 2, 3, 50);

            remoteRepo.Commit(200);
            remoteRepo.Commit(300);

            localRepo.Assert(1, 2, 3, 50, 200, 300);
            remoteRepo.Assert(1, 2, 3, 50, 200, 300);
        }
    }
}
