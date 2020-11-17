using AiurEventSyncer.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiurEventSyncer.Tests.Tools
{
    public class TestExtends
    {
        public static void AssertRepo<T>(Repository<T> repo, params T[] array)
        {
            var commits = repo.Commits.ToArray();
            for (int i = 0; i < commits.Count(); i++)
            {
                Assert.AreEqual(commits[i].Item, array[i]);
            }
            Assert.AreEqual(commits.Count(), array.Length);
        }
    }
}
