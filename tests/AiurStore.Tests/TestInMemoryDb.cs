using AiurStore.Tests.TestDbs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace AiurStore.Tests
{
    [TestClass]
    public class TestInMemoryDb
    {
        [TestMethod]
        public void BasicTest()
        {
            var fileStore = new MemoryTestDb();
            fileStore.Drop();
            fileStore.Insert("House");
            fileStore.Insert("Home");
            fileStore.Insert("Room");
            var result = fileStore.Query().Where(t => t.StartsWith("H")).ToList();

            Assert.AreEqual(result.Count, 2);
            Assert.IsTrue(result.Any(t => t == "Home"));
            Assert.IsTrue(result.Any(t => t == "House"));
        }
    }
}
