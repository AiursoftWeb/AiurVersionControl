using AiurVersionControl.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace AiurVersionControl.Tests
{
    [TestClass]
    public class BasicModelTest
    {
        [TestMethod]
        public void TestNumberWorkSpace()
        {
            var workSpace = new NumberWorkSpace();
            var modification = new AddModification(5);
            var modification2 = new AddModification(50);
            modification.Apply(workSpace);
            modification2.Apply(workSpace);
            Assert.AreEqual(55, workSpace.NumberStore);
        }
    }

    public class NumberWorkSpace : WorkSpace
    {
        public int NumberStore { get; set; }
    }

    public class AddModification : IModification<NumberWorkSpace>
    {
        private readonly int _amount;

        public AddModification(int amount)
        {
            _amount = amount;
        }

        public void Apply(NumberWorkSpace workspace)
        {
            workspace.NumberStore += _amount;
        }
    }
}
