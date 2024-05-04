using Aiursoft.AiurStore.Tools;
using Aiursoft.AiurVersionControl.Crud.Modifications;
using Aiursoft.AiurVersionControl.Crud.Tests.Models;
using Aiursoft.AiurVersionControl.Remotes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aiursoft.AiurVersionControl.Crud.Tests
{
    [TestClass]
    public class ModelTest
    {
        [TestMethod]
        public void CRUDTest()
        {
            var repo = new CollectionRepository<Book>
            {
                new() { Id = 0, Title = "Book first." },
                new() { Id = 1, Title = "Book second." }
            };
            repo.Drop(nameof(Book.Id), 0);
            repo.Patch(nameof(Book.Id), 1, nameof(Book.Title), "Book modified.");

            var only = repo.Single();
            Assert.AreEqual(1, only.Id);
            Assert.AreEqual("Book modified.", only.Title);
        }

        [TestMethod]
        public void ModelToJsonTests()
        {
            var add = new Add<Book>(new Book { Id = 0, Title = "Book first." });
            var json = JsonTools.Serialize(add);
            var convertedBack = JsonTools.Deserialize<Add<Book>>(json);
            Assert.AreEqual(add.Item.Title, convertedBack.Item.Title);
        }

        [TestMethod]
        public void DropModelToJsonTests()
        {
            var drop = new Drop<Book, int>(nameof(Book.Id), 1);
            var json = JsonTools.Serialize(drop);
            var convertedBack = JsonTools.Deserialize<Drop<Book, int>>(json);
            var repo = new CollectionRepository<Book>
            {
                new() { Id = 0, Title = "Book first." },
                new() { Id = 1, Title = "Book second." }
            };
            repo.ApplyChange(convertedBack);
            var only = repo.Single();
            Assert.AreEqual(0, only.Id);
        }

        [TestMethod]
        public void PatchModelToJsonTests()
        {
            var patch = new Patch<Book, int, string>(nameof(Book.Id), 1, nameof(Book.Title), "Patched");
            var json = JsonTools.Serialize(patch);
            var convertedBack = JsonTools.Deserialize<Patch<Book, int, string>>(json);
            var repo = new CollectionRepository<Book>
            {
                new() { Id = 1, Title = "Book second." }
            };
            repo.ApplyChange(convertedBack);
            var only = repo.Single();
            Assert.AreEqual("Patched", only.Title);
        }

        [TestMethod]
        public async Task TestMerge()
        {
            var repo = new CollectionRepository<Book>();
            repo.Add(new Book { Id = 0, Title = "Book first." });
            repo.Add(new Book { Id = 0, Title = "Book first." });
            repo.Add(new Book { Id = 0, Title = "Book first." });

            var repoB = new CollectionRepository<Book>();
            repoB.Add(new Book { Id = 0, Title = "Book second." });
            repoB.Add(new Book { Id = 0, Title = "Book second." });
            repoB.Add(new Book { Id = 0, Title = "Book second." });

            var remote = new ObjectRemoteWithWorkSpace<CollectionWorkSpace<Book>>(repo, true, true);
            await remote.AttachAsync(repoB);

            Assert.AreEqual(6, repo.WorkSpace.Count());
            Assert.AreEqual(6, repoB.WorkSpace.Count());
            Assert.AreEqual("Book first.", string.Join(' ', repo.WorkSpace[0].Title));
            Assert.AreEqual("Book second.", string.Join(' ', repoB.WorkSpace[3].Title));

            await Task.Delay(50);
            Assert.AreEqual(6, repo.WorkSpace.Count());
            Assert.AreEqual(6, repoB.WorkSpace.Count());
            Assert.AreEqual("Book first.", string.Join(' ', repo.WorkSpace[0].Title));
            Assert.AreEqual("Book second.", string.Join(' ', repoB.WorkSpace[3].Title));
        }
    }
}
