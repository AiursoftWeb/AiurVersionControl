using AiurEventSyncer.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace AiurEventSyncer.Tests.Tools
{
    public class TestExtends
    {
        public static void AssertRepo<T>(Repository<T> repo, params T[] array)
        {
            var commits = repo.Commits.ToArray();
            if (commits.Count() != array.Length)
            {
                Assert.Fail($"Two repo don't match! Expected: {string.Join(',', array.Select(t => t.ToString()))}; Actual: {string.Join(',', repo.Commits.Select(t => t.ToString()))}");
            }
            for (int i = 0; i < commits.Count(); i++)
            {
                if (!commits[i].Item.Equals(array[i]))
                {
                    Assert.Fail($"Two repo don't match! Expected: {string.Join(',', array.Select(t => t.ToString()))}; Actual: {string.Join(',', repo.Commits.Select(t => t.ToString()))}");
                }
            }
        }
    }
}
