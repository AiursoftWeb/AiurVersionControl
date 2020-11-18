using AiurStore.Models;
using AiurStore.Providers.FileProvider;
using AiurStore.Providers.MemoryProvider;
using AiurStore.Tests.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AiurStore.Tests
{
    [TestClass]
    public class DbOperationsTest
    {
        [TestMethod]
        [DataRow(typeof(FileAiurStoreDb<string>))]
        [DataRow(typeof(MemoryAiurStoreDb<string>))]
        public void BasicTest(Type dbType)
        {
            InOutDatabase<string> store = Activator.CreateInstance(dbType) as InOutDatabase<string>;
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
