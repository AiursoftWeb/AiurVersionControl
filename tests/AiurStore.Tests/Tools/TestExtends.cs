using AiurStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace AiurStore.Tests.Tools
{
    public class TestExtends
    {
        public static void AssertDb<T>(InOutDatabase<T> db, params T[] array) where T : class
        {
            for (var i = 0; i < db.Count(); i++)
            {
                Assert.AreEqual(db.ToArray()[i], array[i]);
            }
            Assert.AreEqual(db.Count(), array.Length);
        }
    }
}
