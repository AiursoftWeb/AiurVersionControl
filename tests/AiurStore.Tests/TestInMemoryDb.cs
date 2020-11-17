using AiurStore.Providers.MemoryProvider;
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
            var fileStore = new MemoryAiurStoreDb<string>();
            fileStore.Clear();
            fileStore.Add("House");
            fileStore.Add("Home");
            fileStore.Add("Room");
            fileStore.InsertAfter(t => t.StartsWith("Hom"), "Home2");
            fileStore.InsertAfter(t => false, "Trash");
            TestExtends.AssertDb(fileStore, "House", "Home", "Home2", "Room");
        }
    }
}
