using AiurStore.Tests.TestDbs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
            }
            catch (InvalidOperationException e)
            {
                Assert.AreEqual(e.Message, "No store service configured!");
            }
        }
    }
}
