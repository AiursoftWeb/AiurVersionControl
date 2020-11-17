using AiurStore.Tests.TestDbs;
using AiurStore.Tests.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            fileStore.Add("House");
            fileStore.Add("Home");
            fileStore.Add("Room");

            TestExtends.AssertDb(fileStore, "House", "Home", "Room");
        }
    }
}
