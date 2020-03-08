using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUnit.Test.Transport
{
    [TestClass]
    public class SimpleTestMethodCycle
    {
        private static string _staticString;
        private string _initialString;

        public SimpleTestMethodCycle() { }

        public SimpleTestMethodCycle(string initialString)
        {
            _initialString = initialString;
        }

        static SimpleTestMethodCycle()
        {
            _staticString = "Static string";
        }

        [TestInitialize]
        public void InitializeOnSimpleMethod()
        {
            if (this.GetType() != typeof(SimpleTestMethodCycle))
                return;

            Trace.WriteLine("InitializeOnSimpleMethod");
        }

        [TestCleanup]
        public void CleanupOnSimpleMethod()
        {
            if (this.GetType() != typeof(SimpleTestMethodCycle))
                return;

            Trace.WriteLine("CleanupOnSimpleMethod");
        }

        [TestMethod]
        public void SimpleTestMethod1()
        {
            Trace.WriteLine("SimpleTestMethod1");
        }

        [TestMethod]
        public void SimpleTestMethod2()
        {
            Trace.WriteLine("SimpleTestMethod2");
        }

        public void SimpleInstanceMethod()
        {
            Trace.WriteLine(_initialString);
        }

        public static void SimpleStaticMethod()
        {
            Trace.WriteLine(_staticString);
        }
    }
}
