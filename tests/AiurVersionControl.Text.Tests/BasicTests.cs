using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AiurVersionControl.Text.Tests
{
    [TestClass]
    public class BasicTests
    {
        [TestMethod]
        public void TextDiff()
        {
            var repo = new TextRepository();
            repo.Update("string");
            Assert.AreEqual("string", repo.WorkSpace.Content);

            repo.Update("strength");
            Assert.AreEqual("strength", repo.WorkSpace.Content);
        }
    }
}
