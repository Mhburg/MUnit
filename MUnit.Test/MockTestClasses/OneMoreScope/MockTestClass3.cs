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
    public class MockTestClass3
    {
        [AssemblyInitialize]
        public static void InitializeOnAssembly() { }

        [NamespaceInitialize]
        public static void InitializeOnNamespace() { }

        [ClassInitialize]
        public static void InitializeOnClass() { }

        [ClassInitialize]
        public void IncorrectInitializeOnClass() { }

        [TestInitialize]
        public void InitializeOnMethod()
        {
            if (this.GetType() != typeof(MockTestClass5))
                return;

            Trace.WriteLine("InitializeOnMethod3");
        }

        [TestInitialize]
        internal void IncorrectInitializeOnMethod() { }

        [TestInitialize]
        public void IncorrectInitializeOnMethod(string arg) { }

        [AssemblyCleanup]
        public static void CleanupOnAssembly() { }

        [NamespaceCleanup]
        public static void CleanupOnNamespace() { }

        [ClassCleanup]
        public static void CleanupOnClass() { }

        [TestCleanup]
        public void CleanupOnMethod3()
        {
            if (this.GetType() != typeof(MockTestClass5))
                return;

            Trace.WriteLine("CleanupOnMethod3");
        }

        [TestMethod]
        public void MockTestMethod1() { }

        [TestMethod]
        public void MockTestMethod2() { }
    }
}
