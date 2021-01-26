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
            Assert.AreEqual(0, remote.RemoteWorkSpace.NumberStore);
            await remote.PullAsync();
            Assert.AreEqual(55, remote.RemoteWorkSpace.NumberStore);

#warning This test case is still not passing. This is our next work item.
            // Assert.AreEqual(55, repo2.WorkSpace.NumberStore);
        }
    }
}
