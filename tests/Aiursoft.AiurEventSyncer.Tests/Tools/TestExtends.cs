using Aiursoft.AiurEventSyncer.Models;

namespace Aiursoft.AiurEventSyncer.Tests.Tools
{
    public static class TestExtends
    {
        public static void Assert<T>(this Repository<T> repo, params T[] array)
        {
            repo.WaitTill(array.Length, 2).Wait();
            repo.WaitForNotificationsAsync(200).Wait();
            var commits = repo.Commits.ToArray();
            for (var i = 0; i < commits.Length; i++)
            {
                if (!commits[i].Item.Equals(array[i]))
                {
                    Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail($"The repo don't match! Expected: {string.Join(',', array.Select(t => t.ToString()))}; Actual: {string.Join(',', repo.Commits.Select(t => t.ToString()))}");
                }
            }
        }

        public static async Task WaitForNotificationsAsync<T>(this Repository<T> repo, int maxWaitMs = 1000)
        {
            // Wait for the notification queue to flush
            await Task.Delay(50);
            var waited = 50;
            while (waited < maxWaitMs)
            {
                // Check if commit count is stable
                var initialCount = repo.Commits.Count;
                await Task.Delay(50);
                if (repo.Commits.Count == initialCount)
                {
                    break;
                }
                waited += 50;
            }
        }

        private static async Task WaitTill<T>(this Repository<T> repo, int count, int maxWaitSeconds = 5)
        {
            var waitedTime = TimeSpan.Zero;
            while (repo.Commits.Count < count)
            {
                var thisWait = TimeSpan.FromSeconds(1);
                await Task.Delay(thisWait);
                waitedTime += thisWait;
                if (waitedTime.TotalSeconds >= maxWaitSeconds)
                {
                    Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail($"Two repo don't match! Expected: {count} commits; Actual: {string.Join(',', repo.Commits.Select(t => t.ToString()))}");
                }
            }
        }
    }
}
