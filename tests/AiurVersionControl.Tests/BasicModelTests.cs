using AiurEventSyncer.ConnectionProviders;
using AiurEventSyncer.Remotes;
using AiurVersionControl.Models;
using AiurVersionControl.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace AiurVersionControl.Tests
{
    [TestClass]
    public class BasicModelTests
    {
        [TestMethod]
        public void ManualApplyModificationTest()
        {
            var workSpace = new NumberWorkSpace();
            var modification = new AddModification(5);
            var modification2 = new AddModification(50);
            modification.Apply(workSpace);
            modification2.Apply(workSpace);
            Assert.AreEqual(55, workSpace.NumberStore);
        }

        [TestMethod]
        public void ControlledRepositoryTest()
        {
            var repo = new ControlledRepository<NumberWorkSpace>();
            repo.ApplyChange(new AddModification(5));
            Assert.AreEqual(5, repo.WorkSpace.NumberStore);
            repo.ApplyChange(new AddModification(50));
            Assert.AreEqual(55, repo.WorkSpace.NumberStore);
        }

        [TestMethod]
        public async Task RemoteWithWorkSpaceTest()
        {
            var repo = new ControlledRepository<NumberWorkSpace>();
            repo.ApplyChange(new AddModification(5));
            repo.ApplyChange(new AddModification(50));

            var repo2 = new ControlledRepository<NumberWorkSpace>();
            var connection = new FakeConnection<IModification<NumberWorkSpace>>(repo2);
            var remote = new RemoteWithWorkSpace<NumberWorkSpace>(connection);
            await remote.AttachAsync(repo);

            await remote.PushAsync();
            Assert.AreEqual(55, repo.WorkSpace.NumberStore);
            Assert.AreEqual(55, repo2.WorkSpace.NumberStore);
            Assert.AreEqual(0, remote.RemoteWorkSpace.NumberStore);

            await remote.PullAsync();
            Assert.AreEqual(55, repo.WorkSpace.NumberStore);
            Assert.AreEqual(55, repo2.WorkSpace.NumberStore);
            Assert.AreEqual(55, remote.RemoteWorkSpace.NumberStore);
        }

        [TestMethod]
        public async Task RemoteWithWorkSpaceAutoTest()
        {
            var repo = new ControlledRepository<NumberWorkSpace>();
            var repo2 = new ControlledRepository<NumberWorkSpace>();
            var connection = new FakeConnection<IModification<NumberWorkSpace>>(repo2);
            var remote = new RemoteWithWorkSpace<NumberWorkSpace>(connection, true, true);
            var workspacePointer = repo.WorkSpace;
            await remote.AttachAsync(repo);

            Assert.AreEqual(0, remote.RemoteWorkSpace.NumberStore);
            Assert.AreEqual(0, repo.WorkSpace.NumberStore);
            Assert.AreEqual(0, repo2.WorkSpace.NumberStore);

            repo.ApplyChange(new AddModification(5));
            repo.ApplyChange(new AddModification(50));

            await Task.Delay(30);
            var workspacePointerNew = repo.WorkSpace;
            Assert.AreEqual(55, remote.RemoteWorkSpace.NumberStore);
            Assert.AreEqual(55, repo.WorkSpace.NumberStore);
            Assert.AreEqual(55, repo2.WorkSpace.NumberStore);
            Assert.AreEqual(workspacePointer, workspacePointerNew);
        }

        [TestMethod]
        public async Task RemoteWithMergeWorkSpaceTest()
        {
            var repo = new ControlledRepository<NumberWorkSpace>();
            var repo2 = new ControlledRepository<NumberWorkSpace>();
            var connection = new FakeConnection<IModification<NumberWorkSpace>>(repo2);
            var remote = new RemoteWithWorkSpace<NumberWorkSpace>(connection, false, false);
            await remote.AttachAsync(repo);

            Assert.AreEqual(0, remote.RemoteWorkSpace.NumberStore);
            Assert.AreEqual(0, repo.WorkSpace.NumberStore);
            Assert.AreEqual(0, repo2.WorkSpace.NumberStore);

            repo.ApplyChange(new AddModification(5));
            repo.ApplyChange(new AddModification(50));

            Assert.AreEqual(0, remote.RemoteWorkSpace.NumberStore);
            Assert.AreEqual(55, repo.WorkSpace.NumberStore); // 5, 50
            Assert.AreEqual(0, repo2.WorkSpace.NumberStore); // 0

            await remote.PushAsync();
            Assert.AreEqual(0, remote.RemoteWorkSpace.NumberStore);
            Assert.AreEqual(55, repo.WorkSpace.NumberStore); // 5, 50
            Assert.AreEqual(55, repo2.WorkSpace.NumberStore);// 5, 50

            await remote.PullAsync();
            Assert.AreEqual(55, remote.RemoteWorkSpace.NumberStore);
            Assert.AreEqual(55, repo.WorkSpace.NumberStore); // 5, 50
            Assert.AreEqual(55, repo2.WorkSpace.NumberStore);// 5, 50

            repo.ApplyChange(new AddModification(100));
            Assert.AreEqual(55, remote.RemoteWorkSpace.NumberStore);
            Assert.AreEqual(155, repo.WorkSpace.NumberStore);// 5, 50, 100
            Assert.AreEqual(55, repo2.WorkSpace.NumberStore);// 5, 50

            repo2.ApplyChange(new AddModification(1000));
            Assert.AreEqual(55, remote.RemoteWorkSpace.NumberStore);
            Assert.AreEqual(155, repo.WorkSpace.NumberStore);  // 5, 50, 100
            Assert.AreEqual(1055, repo2.WorkSpace.NumberStore);// 5, 50, 1000

            await remote.PushAsync();
            Assert.AreEqual(55, remote.RemoteWorkSpace.NumberStore);
            Assert.AreEqual(155, repo.WorkSpace.NumberStore);  // 5, 50, 100
            Assert.AreEqual(1155, repo2.WorkSpace.NumberStore);// 5, 50, 1000, 100

            await remote.PullAsync();
            Assert.AreEqual(1155, remote.RemoteWorkSpace.NumberStore);
            // Uncomment this line to enable this test case.
            Assert.AreEqual(1155, repo.WorkSpace.NumberStore);  // 5, 50, 1000, 100
            Assert.AreEqual(1155, repo2.WorkSpace.NumberStore);// 5, 50, 1000, 100
        }
    }
}
