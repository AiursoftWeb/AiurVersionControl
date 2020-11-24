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
        private SqlDbContext dbContext;
        [TestInitialize]
        public void Init()
        {
            dbContext = new SqlDbContext();
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
        }

        [TestMethod]
        [DataRow(typeof(FileAiurStoreDb<string>))]
        [DataRow(typeof(MemoryAiurStoreDb<string>))]
        public void BasicTest(Type dbType)
        {
            var store = Activator.CreateInstance(dbType) as InOutDatabase<string>;
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
            var store = DbQueryProviderTools.BuildFromDbSet<string, SqlDbContext>(
                contextFactory: () => dbContext,
                queryFactory: (context) => context.Records.Where(t => t.Filter == "Demo Filter!").Select(t => t.Content),
                addAction: (newItem, context) =>
                {
                    context.Records.Add(new InDbEntity
                    {
                        Content = newItem,
                        Filter = "Demo Filter!"
                    });
                    context.Records.Add(new InDbEntity
                    {
                        Content = "Dirty item",
                        Filter = "Can't pass filter!"
                    });
                    context.SaveChanges();
                });
            store.Add("House");
            store.Add("Home");
            store.Add("Room");
            TestExtends.AssertDb(store, "House", "Home", "Room");
        }
    }
}
