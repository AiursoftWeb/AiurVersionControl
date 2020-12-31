using AiurVersionControl.Models;
using AiurVersionControl.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
