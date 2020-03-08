using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MUnit.Engine;
using MUnit.Utilities;

using UTF = MUnit.Framework;

namespace MUnit.Test.Framework
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestAssert
    {
        private AutoResetEvent _autoResetEvent;
        private static UTF.TestResult _result;

        [TestInitialize]
        public void Initialize()
        {
            AssemblyPrep.Logger.TestResultsEvent += Logger_TestResultEventHandler;
            _autoResetEvent = new AutoResetEvent(false);
        }

        [TestCleanup]
        public void Cleanup()
        {
            AssemblyPrep.Logger.TestResultsEvent -= Logger_TestResultEventHandler;
        }

        private void Logger_TestResultEventHandler(object sender, IEnumerable<UTF.TestResult> e)
        {
            _result = e.First();
            _autoResetEvent.Set();
        }

        [TestMethod]
        public void AssertExpectValueThrown()
        {
            AssemblyPrep.RunSingleTest(typeof(MockAssertClass), "AssertExpectValueThrown");
            _autoResetEvent.WaitOne();

            Assert.AreEqual(UTF.UnitTestOutcome.Failed, _result.Outcome);
        }

        [TestMethod]
        public void AssertExpectValue()
        {
            AssemblyPrep.RunSingleTest(typeof(MockAssertClass), "AssertExpectValue");
            _autoResetEvent.WaitOne();

            Assert.AreEqual(UTF.UnitTestOutcome.Passed, _result.Outcome);
        }

        [TestMethod]
        public void AssertAreEqualValueThrown()
        {
            AssemblyPrep.RunSingleTest(typeof(MockAssertClass), "AssertAreEqualValueThrown");
            _autoResetEvent.WaitOne();

            Assert.AreEqual(UTF.UnitTestOutcome.Failed, _result.Outcome);
        }

        [TestMethod]
        public void AssertAreEqualValue()
        {
            AssemblyPrep.RunSingleTest(typeof(MockAssertClass), "AssertAreEqualValue");
            //AssemblyPrep.TestEngine
            //    .RunTests(
            //        new[] { HashUtilities.GuidForTestCycleID(AssemblyPrep.Source, typeof(MockAssertClass).FullName + ".AssertAreEqualValue") },
            //        AssemblyPrep.Logger);
            _autoResetEvent.WaitOne();

            Assert.AreEqual(UTF.UnitTestOutcome.Passed, _result.Outcome);
        }
    }
}
