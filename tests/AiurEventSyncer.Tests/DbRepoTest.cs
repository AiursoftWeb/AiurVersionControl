using AiurEventSyncer.Models;
using AiurEventSyncer.Remotes;
using AiurEventSyncer.Tests.Models;
using AiurStore.Models;
using AiurStore.Providers.DbQueryProvider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiurEventSyncer.Tests
{
    [TestClass]
    public class DbRepoTest
    {
        private SqlDbContext dbContext;
        private InOutDatabase<Commit<Book>> store;
        private Repository<Book> dbRepo;

        [TestInitialize]
        public void Init()
        {
            dbContext = new SqlDbContext();
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
            store = DbQueryProviderTools.BuildFromDbSet<Commit<Book>>(
                queryFactory: () => dbContext.Records.Select(t => t.Content),
                addAction: (newItem) =>
                {
                    dbContext.Records.Add(new InDbEntity
                    {
                        Content = newItem
                    });
                    dbContext.SaveChanges();
                });
            dbRepo = new Repository<Book>(store);
        }

        [TestMethod]
        public void PushToAndPullFrom()
        {
            var localRepo = new Repository<Book>();
            localRepo.Remotes.Add(new ObjectRemote<Book>(dbRepo, true));

            var localRepo2 = new Repository<Book>();
            localRepo2.AddAutoPullRemote(new ObjectRemote<Book>(dbRepo));

            localRepo.Commit(new Book { Name = "Love" });

            Assert.IsTrue(localRepo.Commits.FirstOrDefault().Item.Name == "Love");
            Assert.IsTrue(dbRepo.Commits.FirstOrDefault().Item.Name == "Love");
            Assert.IsTrue(localRepo2.Commits.FirstOrDefault().Item.Name == "Love");
        }
    }
}
