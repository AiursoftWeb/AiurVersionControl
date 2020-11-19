using AiurStore.Models;
using AiurStore.Providers.DbQueryProvider;
using AiurStore.Providers.FileProvider;
using AiurStore.Providers.MemoryProvider;
using AiurStore.Tests.TestDbs;
using AiurStore.Tests.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace AiurStore.Tests
{
    [TestClass]
    public class DbOperationsTest
    {
        private SqliteDbContext dbContext;
        [TestInitialize]
        public void Init()
        {
            dbContext = new SqliteDbContext();
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
        }

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

        [TestMethod]
        public void QueryDbTest()
        {
            var store = DbQueryProviderTools.BuildFromDbSet<string>(
                queryFactory: () => dbContext.Records.Where(t => t.Filter == "Demo Filter!").Select(t => t.Content),
                addAction: (newItem) =>
                {
                    dbContext.Records.Add(new InDbEntity
                    {
                        Content = newItem,
                        Filter = "Demo Filter!"
                    });
                    dbContext.Records.Add(new InDbEntity
                    {
                        Content = "Dirty item",
                        Filter = "Can't pass filter!"
                    });
                    dbContext.SaveChanges();
                });
            store.Add("House");
            store.Add("Home");
            store.Add("Room");
            TestExtends.AssertDb(store, "House", "Home", "Room");
        }
    }
}
