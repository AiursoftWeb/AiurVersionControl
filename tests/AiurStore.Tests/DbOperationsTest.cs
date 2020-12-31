using AiurStore.Models;
using AiurStore.Providers;
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
        private SqlDbContext dbContext;

        [TestInitialize]
        public void Init()
        {
            dbContext = new SqlDbContext();
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
        }

        [TestMethod]
        public void BasicTest()
        {
            var memoryDb = new MemoryAiurStoreDb<string>();
            var fileDb = new FileAiurStoreDb<string>("aiur-store.txt");
            TestDb(memoryDb);
            TestDb(fileDb);
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

        [TestMethod]
        public void QueryDbTest()
        {
            var store = new DbSetDb<string, InDbEntity, SqlDbContext>(
                contextFactory: () => dbContext,
                dbSetFactory: (context) => context.Records,
                queryFactory: (dbSet) => dbSet.Where(t => t.Filter == "Demo Filter!").Select(t => t.Content),
                addAction: (newItem, dbSet) =>
                {
                    dbSet.Add(new InDbEntity
                    {
                        Content = newItem,
                        Filter = "Demo Filter!"
                    });
                    dbSet.Add(new InDbEntity
                    {
                        Content = "Dirty item",
                        Filter = "Can't pass filter!"
                    });
                })
            {
                "House",
                "Home",
                "Room"
            };
            TestExtends.AssertDb(store, "House", "Home", "Room");
        }
    }
}
