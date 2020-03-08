using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MUnit.Framework;

namespace MUnit.Test.OneMoreScope
{
    [TestClass]
    public class MockTestClass5 : MockTestClass4
    {
        [TestInitialize]
        public void InitializeOnMethod5()
        {
            if (this.GetType() != typeof(MockTestClass5))
                return;

            Trace.WriteLine("InitializeOnMethod5");
        }

        [TestCleanup]
        public void CleanupOnMethod5()
        {
            if (this.GetType() != typeof(MockTestClass5))
                return;

            Trace.WriteLine("CleanupOnMethod5");
        }

        [TestMethod]
        public void MockestMethod5() { }
    }
}
