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
            var remoteRecord = new ObjectRemote<int>(remoteRepo, true);
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
            localRepo.AddAutoPullRemote(new ObjectRemote<int>(remoteRepo));

            localRepo.Assert(1, 2, 3);

            remoteRepo.Commit(50);
            localRepo.Assert(1, 2, 3, 50);

            remoteRepo.Commit(200);
            remoteRepo.Commit(300);

            localRepo.Assert(1, 2, 3, 50, 200, 300);
            remoteRepo.Assert(1, 2, 3, 50, 200, 300);
        }

        [TestMethod]
        public void DoubleWaySync()
        {
            var a = new Repository<int>();
            var b = new Repository<int>();
            a.AddAutoPullRemote(new ObjectRemote<int>(b, true));

            a.Commit(5);
            a.Assert(5);
            b.Assert(5);

            b.Commit(10);
            a.Assert(5, 10);
            b.Assert(5, 10);

            a.Commit(100);
            b.Commit(200);
            a.Assert(5, 10, 100, 200);
            b.Assert(5, 10, 100, 200);
        }

        [TestMethod]
        public void ComplicatedAutoTest()
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
            c.AddAutoPullRemote(new ObjectRemote<int>(b));
            c.Remotes.Add(new ObjectRemote<int>(d, true));
            e.AddAutoPullRemote(new ObjectRemote<int>(d));

            a.Commit(5);
            b.Assert(5);
            c.Assert(5);
            d.Assert(5);
            e.Assert(5);
        }
    }
}
