using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace AiurStore.Tests
{
    public class MyTestDb : InOutDatabase<string>
    {
        protected override void OnConfiguring(InOutDbOptions options)
            => options.UseFileStore("test.txt");
    }

    [TestClass]
    public class Class1
    {
        [TestMethod]
        public void Test()
        {
            var fileStore = new MyTestDb();
            fileStore.Drop();
            fileStore.Insert("House");
            fileStore.Insert("Home");
            fileStore.Insert("Room");
            var result = fileStore.Query().Where(t => t.StartsWith("H")).ToList();

            Assert.AreEqual(result.Count, 2);
            Assert.AreEqual(result[0], "House");
            Assert.AreEqual(result[1], "Home");
        }
    }
}
