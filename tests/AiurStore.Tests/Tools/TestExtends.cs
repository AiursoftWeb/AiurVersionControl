using AiurStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
