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
    public class MockTestClass4 : MockTestClass3
    {
        [AssemblyInitialize]
        public static void Initialize2OnAssembly() { }

        [ClassInitialize]
        public static void InitializeOnClass() { }

        [ClassInitialize]
        public static void InitializeOnClass2() { }

        [TestInitialize]
        public void InitializeOnMethod4()
        {
            if (this.GetType() != typeof(MockTestClass5))
                return;

            Trace.WriteLine("InitializeOnMethod4");
        }

        [AssemblyCleanup]
        public static void CleanupOnAssembly() { }

        [ClassCleanup]
        public static void CleanupOnClass() { }

        [ClassCleanup]
        public static void CleanupOnClass2() { }

        [TestCleanup]
        public new void CleanupOnMethod4()
        {
            if (this.GetType() != typeof(MockTestClass5))
                return;

            Trace.WriteLine("CleanupOnMethod4");
        }

        [TestMethod]
        public void MockTestMethod1() { }

        [TestMethod]
        public void MockTestMethod2() { }
    }
}
