using AiurEventSyncer.ConnectionProviders;
using AiurEventSyncer.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace AiurEventSyncer.Tests
{
    [TestClass]
    public class ConnectionRetryTests
    {
        [TestMethod]
        public async Task TestRetry()
        {
            var connection = new RetryableWebSocketConnection<Book>("wss://aaaa.bbbb.ccccccc/aaaaaa/dddddd/eeeee/fff.ares");
            var retryCounts = 0;
            var startTime = DateTime.UtcNow;
            connection.PropertyChanged += (o, e) =>
            {
                Console.WriteLine($"{(DateTime.UtcNow - startTime).TotalSeconds} seconds passed. Retried: {connection.AttemptCount} times.");
                retryCounts = connection.AttemptCount;
            };
            var pullTask = connection.PullAndMonitor(null, () => string.Empty, null, true);
            // 0 + 1 + 2 + 4     = 7
            // 0 + 1 + 2 + 4 + 8 = 15
            // When at 14, must tried 4 times.

            var waitTask = Task.Delay(14 * 1000);
            await Task.WhenAny(pullTask, waitTask);
            Assert.AreEqual(4, retryCounts);
        }
    }
}
