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
    public class MergeTest
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
        public void TestMerge()
        {
            var remoteRepo = new Repository<int>();
            remoteRepo.Commit(20);
            remoteRepo.Assert(20);

            _demoRepo.Remotes.Add(new ObjectRemote<int>(remoteRepo));
            _demoRepo.Pull();

            _demoRepo.Assert(20, 1, 2, 3);
            remoteRepo.Assert(20);

            _demoRepo.Push();

            _demoRepo.Assert(20, 1, 2, 3);
            remoteRepo.Assert(20, 1, 2, 3);

            _demoRepo.Pull();
            _demoRepo.Assert(20, 1, 2, 3);
            remoteRepo.Assert(20, 1, 2, 3);
        }
    }
}
