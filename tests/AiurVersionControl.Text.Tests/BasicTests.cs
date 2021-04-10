using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AiurVersionControl.Text.Tests
{
    [TestClass]
    public class BasicTests
    {
        [TestMethod]
        public void TextDiff()
        {
            var repo = new TextRepository();
            repo.Update(new[] { "s", "t", "r", "i", "n", "g" });
            Assert.AreEqual("s t r i n g", string.Join(' ', repo.WorkSpace.Content));

            repo.Update(new[] { "s", "t", "r", "e", "n", "g", "t", "h2" });
            Assert.AreEqual("s t r e n g t h2", string.Join(' ', repo.WorkSpace.Content));
        }
    }
}
