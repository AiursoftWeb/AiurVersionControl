using AiurStore.Tests.TestDbs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace AiurStore.Tests
{
    [TestClass]
    public class CommonTests
    {
        [TestMethod]
        public void TestException()
        {
            try
            {
                var db = new BadDb();
                db.ToList();
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e.Message.StartsWith("Object reference not "));
            }
        }
    }
}
