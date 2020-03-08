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
    public class MockTestClass8 : MockTestClass7
    {
        [TestInitialize]
        public void InitializeOnMethod8()
        {
            if (this.GetType() != typeof(MockTestClass8))
                return;

            Trace.WriteLine("InitializeOnMethod8");
        }

        [TestCleanup]
        public void CleanupOnMethod8()
        {
            if (this.GetType() != typeof(MockTestClass8))
                return;

            Trace.WriteLine("CleanupOnMethod8");
        }

        [TestMethod]
        public void MockestMethod8() { }
    }
}
