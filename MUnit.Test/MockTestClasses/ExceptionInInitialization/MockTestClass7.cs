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
    public class MockTestClass7 : MockTestClass6
    {
        [TestInitialize]
        public void InitializeOnMethod7()
        {
            if (this.GetType() != typeof(MockTestClass8))
                return;

            Trace.WriteLine("InitializeOnMethod7");
            throw new Exception("Initialization exception.");
        }

        [TestCleanup]
        public void CleanupOnMethod7()
        {
            if (this.GetType() != typeof(MockTestClass8))
                return;

            Trace.WriteLine("CleanupOnMethod7");
        }

        [TestMethod]
        public void MockestMethod7() { }
    }
}
