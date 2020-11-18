using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
using AiurEventSyncer.Tests.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiurEventSyncer.Tests
{
    [TestClass]
    public class PointerTest
    {
        private Repository<int> _demoRepo;

        [TestInitialize]
        public void GetBasicRepo()
        {
            _demoRepo = new Repository<int>();
            _demoRepo.Commit(1);
            _demoRepo.Commit(2);
            _demoRepo.Commit(3);
            _demoRepo.Assert(1, 2, 3);
        }

        [TestMethod]
        public void TestPull()
        {
            var localRepo = new Repository<int>();
            var remote = new ObjectRemote<int>(_demoRepo);
            localRepo.Remotes.Add(remote);

            Assert.IsNull(remote.LocalPointer);
            Assert.AreEqual(_demoRepo.Head.Item, 3);

            localRepo.Pull();

            Assert.AreEqual(localRepo.Head.Item, 3);
            Assert.AreEqual(_demoRepo.Head.Item, 3);
        }

        [TestMethod]
        public void TestPush()
        {
            var remoteRepo = new Repository<int>();
            var localRepo = _demoRepo;
            var remote = new ObjectRemote<int>(remoteRepo);
            localRepo.Remotes.Add(remote);

            Assert.IsNull(remote.LocalPointer);
            Assert.AreEqual(_demoRepo.Head.Item, 3);

            localRepo.Push();

            Assert.AreEqual(localRepo.Head.Item, 3);
            Assert.AreEqual(_demoRepo.Head.Item, 3);
        }
    }
}
