using AiurStore.Tests.TestDbs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace AiurStore.Tests
{
    [TestClass]
    public class TestInFileDb
    {
        [TestMethod]
        public void BasicTest()
        {
            var fileStore = new FileTestDb();
            fileStore.Drop();
            fileStore.Insert("House");
            fileStore.Insert("Home");
            fileStore.Insert("Room");
            var result = fileStore.Query().Where(t => t.StartsWith("H")).ToList();

            Assert.AreEqual(result.Count, 2);
            Assert.AreEqual(result[0], "House");
            Assert.AreEqual(result[1], "Home");
        }
    }
}
