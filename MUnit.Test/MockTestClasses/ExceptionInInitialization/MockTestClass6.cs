using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MUnit.Framework;

namespace MUnit.Test
{
    [TestClass]
    public class MockTestClass6
    {
        [TestInitialize]
        public void InitializeOnMethod6()
        {
            if (this.GetType() != typeof(MockTestClass8))
                return;

            Trace.WriteLine("InitializeOnMethod6");
        }

        [TestCleanup]
        public void CleanupOnMethod6()
        {
            if (this.GetType() != typeof(MockTestClass8))
                return;

            Trace.WriteLine("CleanupOnMethod6");
        }

        [TestMethod]
        public void MockTestMethod6() { }
    }
}
