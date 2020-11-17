using AiurStore.Providers.FileProvider;
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
            var fileStore = new FileAiurStoreDb<string>();
            fileStore.Clear();
            fileStore.Add("House");
            fileStore.Add("Home");
            fileStore.Add("Room");
            fileStore.InsertAfter(t => t.StartsWith("Hom"), "Home2");
            TestExtends.AssertDb(fileStore, "House", "Home", "Home2", "Room");
        }
    }
}
