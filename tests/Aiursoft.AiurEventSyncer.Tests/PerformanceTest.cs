using Aiursoft.AiurEventSyncer.Models;

namespace Aiursoft.AiurEventSyncer.Tests
{
    [TestClass]
    public class PerformanceTest
    {
        [TestMethod]
        public void TestCommits()
        {
            var repo = new Repository<int>();

            var beginTime = DateTime.Now;
            for (var i = 0; i < 1000; i++)
            {
                repo.Commit(1);
            }

            var endTime = DateTime.Now;

            Assert.IsTrue((endTime - beginTime) < TimeSpan.FromSeconds(0.1));
        }
    }
}
