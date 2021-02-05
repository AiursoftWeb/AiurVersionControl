using AiurEventSyncer.ConnectionProviders;
using AiurEventSyncer.Remotes;
using AiurVersionControl.Models;
using AiurVersionControl.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using AiurEventSyncer.Models;

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
            await remote.AttachAsync(repo);

            Assert.AreEqual(0, remote.RemoteWorkSpace.NumberStore);
            Assert.AreEqual(0, repo.WorkSpace.NumberStore);
            Assert.AreEqual(0, repo2.WorkSpace.NumberStore);

            repo.ApplyChange(new AddModification(5));
            repo.ApplyChange(new AddModification(50));

            await Task.Delay(30);

            Assert.AreEqual(55, remote.RemoteWorkSpace.NumberStore);
            Assert.AreEqual(55, repo.WorkSpace.NumberStore);
            Assert.AreEqual(55, repo2.WorkSpace.NumberStore);
        }

        [TestMethod]
        public async Task MultiRepoTestWithWorkSpace()
        {
            // Repository<NumberWorkSpace> repo = new ControlledRepository<NumberWorkSpace>();
            var repoPool = new RepositoryPool<ControlledRepository<NumberWorkSpace>>();
            repoPool.CreatePool();
            repoPool.AddRepository("user1", new ControlledRepository<NumberWorkSpace>());
            repoPool.AddRepository("user2", new ControlledRepository<NumberWorkSpace>());
            
            var repo1 = new ControlledRepository<NumberWorkSpace>();
            var repo2 = new ControlledRepository<NumberWorkSpace>();
            var connection1 = new FakeConnection<IModification<NumberWorkSpace>>(repoPool.GetRepository("user1"));
            var connection2 = new FakeConnection<IModification<NumberWorkSpace>>(repoPool.GetRepository("user2"));
            var remote1 = new RemoteWithWorkSpace<NumberWorkSpace>(connection1, true, true);
            await remote1.AttachAsync(repo1);
            var remote2 = new RemoteWithWorkSpace<NumberWorkSpace>(connection2, true, true);
            await remote2.AttachAsync(repo2);
            
            
            Assert.AreEqual(0, repo1.WorkSpace.NumberStore);
            Assert.AreEqual(0, repoPool.GetRepository("user1").WorkSpace.NumberStore);
            Assert.AreEqual(0, repo2.WorkSpace.NumberStore);
            Assert.AreEqual(0, repoPool.GetRepository("user2").WorkSpace.NumberStore);
            
            repo1.ApplyChange(new AddModification(5));
            repo1.ApplyChange(new AddModification(50));
            repo2.ApplyChange(new AddModification(6));
            repo2.ApplyChange(new AddModification(60));
            
            await Task.Delay(60);

            Assert.AreEqual(55, repoPool.GetRepository("user1").WorkSpace.NumberStore);
            Assert.AreEqual(repo1.WorkSpace.NumberStore, repoPool.GetRepository("user1").WorkSpace.NumberStore);
            Assert.AreEqual(66, repoPool.GetRepository("user2").WorkSpace.NumberStore);
            Assert.AreEqual(repo2.WorkSpace.NumberStore, repoPool.GetRepository("user2").WorkSpace.NumberStore);

        }
    }
}
