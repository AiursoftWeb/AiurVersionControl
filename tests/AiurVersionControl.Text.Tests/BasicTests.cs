using AiurEventSyncer.Remotes;
using AiurVersionControl.Models;
using AiurVersionControl.Remotes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            repo.Update(new[] { "s", "t", "r", "i", "n", "g" });
            Assert.AreEqual("s t r i n g", string.Join(' ', repo.WorkSpace.Content));

            repo.Update(new[] { "s", "t", "r", "e", "n", "g", "t", "h2" });
            Assert.AreEqual("s t r e n g t h2", string.Join(' ', repo.WorkSpace.Content));

            var last = repo.Commits.ToList()[1];
            var lastJson = Newtonsoft.Json.JsonConvert.SerializeObject(last);
            Assert.IsTrue(lastJson.Length < 300);
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
            repo.Update(new[] { "a", "a", "a" });
            Assert.AreEqual("a a a", string.Join(' ', repo.WorkSpace.Content));

            var repoB = new TextRepository();
            repoB.Update(new[] { "b", "b", "b" });
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
