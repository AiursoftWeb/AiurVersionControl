using AiurStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace AiurStore.Tests.Tools
{
    public class TestExtends
    {
        public static void AssertDb<T>(InOutDatabase<T> db, params T[] array)
        {
            for (int i = 0; i < db.Query().Count(); i++)
            {
                Assert.AreEqual(db.Query().ToArray()[i], array[i]);
            }
            Assert.AreEqual(db.Query().Count(), array.Length);
        }
    }
}
