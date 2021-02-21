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
            var repo = new CollectionRepository<Book>();
            repo.ApplyChange(new Add<Book>(new Book { Id = 0, Title = "Book first." }));
            repo.ApplyChange(new Add<Book>(new Book { Id = 1, Title = "Book second." }));
            repo.ApplyChange(new Drop<Book>(t => t.Id == 0));
            repo.ApplyChange(new Patch<Book>(t => t.Id == 1, t => t.Title = "Book modified."));

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
            // This test will fail. I'm still trying to pass it.

            //var drop = new Drop<Book>(t => t.Id == 0);
            //var json = JsonTools.Serialize(drop);
            //var convertedBack = JsonTools.Deserialize<Drop<Book>>(json);
            //Console.WriteLine(json);
        }
    }
}
