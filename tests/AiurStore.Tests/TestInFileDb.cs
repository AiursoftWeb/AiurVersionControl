using AiurStore.Tests.TestDbs;
using AiurStore.Tests.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

            TestExtends.AssertDb(fileStore, "House", "Home", "Room");
        }
    }
}
