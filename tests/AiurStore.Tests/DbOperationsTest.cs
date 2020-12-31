using AiurStore.Models;
using AiurStore.Providers;
using AiurStore.Tests.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AiurStore.Tests
{
    [TestClass]
    public class DbOperationsTest
    {
        [TestMethod]
        public void BasicTest()
        {
            var memoryDb = new MemoryAiurStoreDb<string>();
            //var fileDb = new FileAiurStoreDb<string>("aiur-store.txt");
            TestDb(memoryDb);
            //TestDb(fileDb);
        }

        private static void TestDb(InOutDatabase<string> store)
        {
            store.Clear();
            store.Add("House");
            store.Add("Home");
            store.Add("Room");
            store.InsertAfter(t => t.StartsWith("Hom"), "Home2");
            store.InsertAfter(t => false, "Trash");
            TestExtends.AssertDb(store, "House", "Home", "Home2", "Room");
        }
    }
}
