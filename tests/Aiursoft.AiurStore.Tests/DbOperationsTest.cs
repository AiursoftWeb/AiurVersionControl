using Aiursoft.AiurStore.Models;
using Aiursoft.AiurStore.Providers;
using Aiursoft.AiurStore.Tests.Tools;

namespace Aiursoft.AiurStore.Tests
{
    [TestClass]
    public class DbOperationsTest
    {
        [TestMethod]
        public void BasicTest()
        {
            var memoryDb = new MemoryAiurStoreDb<string>();
            TestDb(memoryDb);
        }

        private static void TestDb(InOutDatabase<string> store)
        {
            store.Add("House");
            store.Add("Home");
            store.Add("Room");
            TestExtends.AssertDb(store, "House", "Home", "Room");

            var afterHouse = store.GetAllAfter("House").ToArray();
            Assert.AreEqual("Home", afterHouse[0]);
            Assert.AreEqual("Room", afterHouse[1]);

            var afternull = store.GetAllAfter(afterWhich: null).ToArray();
            Assert.AreEqual("House", afternull[0]);
            Assert.AreEqual("Home", afternull[1]);
            Assert.AreEqual("Room", afternull[2]);
        }
    }
}
