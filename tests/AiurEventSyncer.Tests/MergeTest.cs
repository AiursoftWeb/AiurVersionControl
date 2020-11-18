using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
using AiurEventSyncer.Tests.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AiurEventSyncer.Tests
{
    [TestClass]
    public class MergeTest
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
        public void TestPushWithDifferentTree()
        {
            var remoteRepo = new Repository<int>();
            remoteRepo.Commit(20);
            remoteRepo.Assert(20);

            _localRepo.Remotes.Add(new ObjectRemote<int>(remoteRepo));

            _localRepo.Pull();
            remoteRepo.Commit(50);

            _localRepo.Assert(20, 1, 2, 3);
            remoteRepo.Assert(20, 50);

            _localRepo.Push();
            _localRepo.Assert(20, 1, 2, 3);
            remoteRepo.Assert(20, 50, 1, 2, 3);

            _localRepo.Pull();
            _localRepo.Assert(20, 50, 1, 2, 3);
            remoteRepo.Assert(20, 50, 1, 2, 3);
        }

        [TestMethod]
        public void TestPullWithDifferentTree()
        {
            var remoteRepo = new Repository<int>();
            remoteRepo.Commit(20);
            remoteRepo.Assert(20);

            _localRepo.Remotes.Add(new ObjectRemote<int>(remoteRepo));

            _localRepo.Pull();
            remoteRepo.Commit(50);

            _localRepo.Assert(20, 1, 2, 3);
            remoteRepo.Assert(20, 50);

            _localRepo.Pull();
            _localRepo.Assert(20, 50, 1, 2, 3);
            remoteRepo.Assert(20, 50);

            _localRepo.Push();
            _localRepo.Assert(20, 50, 1, 2, 3);
            remoteRepo.Assert(20, 50, 1, 2, 3);
        }
    }
}
