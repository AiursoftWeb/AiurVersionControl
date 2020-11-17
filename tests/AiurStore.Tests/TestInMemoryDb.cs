using AiurStore.Tests.TestDbs;
using AiurStore.Tests.Tools;
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

            TestExtends.AssertDb(fileStore, "House", "Home", "Room");
        }
    }
}
