using AiurStore.Tools;
using AiurVersionControl.CRUD.Modifications;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace AiurVersionControl.CRUD.Tests
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }


    [TestClass]
    public class ModelTest
    {
        [TestMethod]
        public void CRUDTest()
        {
            var repo = new CollectionRepository<Book>
            {
                new Book { Id = 0, Title = "Book first." },
                new Book { Id = 1, Title = "Book second." }
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
            Console.WriteLine(json);
        }

        [TestMethod]
        public void LamdbaModelToJsonTests()
        {
            var drop = new Drop<Book>(nameof(Book.Id), 5);
            var json = JsonTools.Serialize(drop);
            var convertedBack = JsonTools.Deserialize<Drop<Book>>(json);
            Console.WriteLine(json);
        }
    }
}
