using AiurEventSyncer.Models;
using System.Linq;
using System.Threading.Tasks;

namespace AiurEventSyncer.Tests.Tools
{
    public static class TestExtends
    {
        public static void Assert<T>(this Repository<T> repo, params T[] array)
        {
            repo.WaitTill(array.Length, 9).Wait();
            var commits = repo.Commits.ToArray();
            for (int i = 0; i < commits.Length; i++)
            {
                if (!commits[i].Item.Equals(array[i]))
                {
                    Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail($"The repo don't match! Expected: {string.Join(',', array.Select(t => t.ToString()))}; Actual: {string.Join(',', repo.Commits.Select(t => t.ToString()))}");
                }
            }
        }

        private static async Task WaitTill<T>(this Repository<T> repo, int count, int maxWaitSeconds = 5)
        {
            int waitedTimes = 0;
            while (repo.Commits.Count() < count)
            {
                await Task.Delay(5);
                waitedTimes++;
                if (waitedTimes / 100 >= maxWaitSeconds)
                {
                    Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail($"Two repo don't match! Expected: {count} commits; Actual: {string.Join(',', repo.Commits.Select(t => t.ToString()))}");
                }
            }
        }
    }
}
