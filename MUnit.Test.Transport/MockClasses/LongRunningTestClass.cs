using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUnit.Test.Transport
{
    [TestClass]
    public class LongRunningTestClass
    {
        [TestMethod]
        public void LongRunningTest1()
        {
            Trace.WriteLine("Entering LongRunningTest1...");
            Task.Delay(2000).Wait();
            Trace.WriteLine("Leaving LongRunningTest1...");
        }

        [TestMethod]
        public void LongRunningTest2()
        {
            Trace.WriteLine("Entering LongRunningTest2...");
            Task.Delay(2000).Wait();
            Trace.WriteLine("Leaving LongRunningTest2...");
        }

        [TestMethod]
        public void LongRunningTest3()
        {
            Trace.WriteLine("Entering LongRunningTest3...");
            Task.Delay(2000).Wait();
            Trace.WriteLine("Leaving LongRunningTest3...");
        }
    }
}
