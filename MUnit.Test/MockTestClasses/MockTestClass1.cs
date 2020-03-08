using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MUnit.Framework;

namespace MUnit.Test
{
    [TestClass]
    public class MockTestClass1
    {
        [TestMethod]
        public void MockTestMethod1() { }

        [TestMethod]
        public void MockTestMethod2() { }

        [TestMethod]
        public string IncorrectMockTestMethod() { return string.Empty; }

        [TestInitialize]
        public void InitializeOnMethod() { }

        [TestInitialize]
        internal void IncorrectInitializeOnMethod() { }

        [TestInitialize]
        public void IncorrectInitializeOnMethod(string arg) { }

        [TestCleanup]
        public void CleanupOnMethod() { }

        [ClassInitialize]
        public static void InitializeOnClass() { }

        [ClassInitialize]
        public void IncorrectInitializeOnClass() { }

        [ClassCleanup]
        public static void CleanupOnClass() { }

        [NamespaceInitialize]
        public static void InitializeOnNamespace() { }

        [NamespaceCleanup]
        public static void CleanupOnNamespace() { }

        [AssemblyInitialize]
        public static void InitializeOnAssembly() { }

        [AssemblyCleanup]
        public static void CleanupOnAssembly() { }
    }
}
