using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MUnit.Framework;

namespace MUnit.Test
{
    [TestClass]
    public class MockTestClass2
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
        public void InitializeOnMethod() { }

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
        public void CleanupOnMethod() { }

        [TestMethod]
        public void MockTestMethod1() { }

        [TestMethod]
        public void MockTestMethod2() { }

        [TestMethod]
        public void MockTestMethod3() { }
    }
}
