using AiurVersionControl.Remotes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AiurVersionControl.Text.Tests
{
    [TestClass]
    public class BasicTests
    {
        [TestMethod]
        public void TextDiff()
        {
            var repo = new TextRepository();
            repo.UpdateText(new[] { "s", "t", "r", "i", "n", "g" });
            Assert.AreEqual("s t r i n g", string.Join(' ', repo.WorkSpace.Content));

            repo.UpdateText(new[] { "s", "t", "r", "e", "n", "g", "t", "h2" });
            Assert.AreEqual("s t r e n g t h2", string.Join(' ', repo.WorkSpace.Content));

            var last = repo.Commits.ToList()[1];
            var lastJson = JsonConvert.SerializeObject(last);
            Assert.IsTrue(lastJson.Length < 300);
            Console.WriteLine(lastJson.Length);
            Console.WriteLine(lastJson);
        }

        /// <summary>
        /// Test 5 times. Because it's possible that thread safety issues may be suppressed in a single test.
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(5)]
        public async Task TestMerge(int _)
        {
            var repo = new TextRepository();
            repo.UpdateText(new[] { "a", "a", "a" });
            Assert.AreEqual("a a a", string.Join(' ', repo.WorkSpace.Content));

            var repoB = new TextRepository();
            repoB.UpdateText(new[] { "b", "b", "b" });
            Assert.AreEqual("b b b", string.Join(' ', repoB.WorkSpace.Content));

            var remote = new ObjectRemoteWithWorkSpace<TextWorkSpace>(repo, true, true);
            await remote.AttachAsync(repoB);

            Assert.AreEqual("b b b a a a", string.Join(' ', repo.WorkSpace.Content));
            Assert.AreEqual("b b b a a a", string.Join(' ', repoB.WorkSpace.Content));

            await Task.Delay(50);
            Assert.AreEqual("b b b a a a", string.Join(' ', repo.WorkSpace.Content));
            Assert.AreEqual("b b b a a a", string.Join(' ', repoB.WorkSpace.Content));
        }
    }
}
