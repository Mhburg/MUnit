using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MUnit.Engine;
using MUnit.Framework;
using MUnit.Utilities;

namespace MUnit.Test
{
    [TestClass]
    public class MockAssertClass
    {
        [TestMethod]
        public void AssertExpectValueThrown()
        {
            Assert.Expect(1, 2, "Assert.Expect exception.");
        }

        [TestMethod]
        public void AssertExpectValue()
        {
            Assert.Expect(1, 1, "Assert.Expect passed.");
        }

        [TestMethod]
        public void AssertAreEqualValue()
        {
            Assert.AreEqual(1, 1, "Integer 1", "Integer 1");
        }

        [TestMethod]
        public void AssertAreEqualValueThrown()
        {
            Assert.AreEqual(1, 2, "Integer 1", "Integer 2");
        }
    }
}
