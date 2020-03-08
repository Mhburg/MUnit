using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MUnit.Utilities;

using UTF = MUnit.Framework;

namespace MUnit.Test.Framework
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestAttributeOnMethod
    {
        public static UTF.ITestCycleGraph Mocktests = AssemblyPrep.MockTests;

        [TestMethod]
        public void IsValidtTestMethod()
        {
            Assert.IsTrue(Mocktests.TryGetValue(
                HashUtilities.GuidForTestCycleID(
                    AssemblyPrep.Source, AssemblyPrep.ReflectionHelper.GetTestCycleNameForMethodScope(typeof(MockTestClass1))),
                out UTF.ITestCycle method1Cycle));

            Assert.AreEqual(2, method1Cycle.TestMethodContexts.Count);
        }
    }
}
