using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MUnit.Utilities;

using UTF = MUnit.Framework;

namespace MUnit.Test.Framework
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestTestCyclesStructure
    {
        private static UTF.ITestCycleGraph MockTests = AssemblyPrep.MockTests;

        [TestMethod]
        public void TestCycleLayer()
        {
            // Total cycles in the graph.
            Assert.AreEqual(24, MockTests.Count);

            // Test AppDomain Cycle
            Assert.IsTrue(MockTests.TryGetValue(
                HashUtilities.GuidForTestCycleID(null, AppDomain.CurrentDomain.FriendlyName),
                out UTF.ITestCycle appDomainCycle));

            // Test Assembly Cycle
            Assert.IsTrue(MockTests.TryGetValue(
                HashUtilities.GuidForTestCycleID(AssemblyPrep.Source, typeof(MockTestClass1).Assembly.FullName),
                out UTF.ITestCycle assemblyCycle));

            // Test 2 Namespace Cycle
            Assert.IsTrue(MockTests.TryGetValue(
                HashUtilities.GuidForTestCycleID(AssemblyPrep.Source, typeof(MockTestClass1).Namespace),
                out UTF.ITestCycle namespace1Cycle));

            Assert.IsTrue(MockTests.TryGetValue(
                HashUtilities.GuidForTestCycleID(AssemblyPrep.Source, typeof(OneMoreScope.MockTestClass3).Namespace),
                out UTF.ITestCycle namespace2Cycle));

            // Test Class cycle
            Assert.IsTrue(MockTests.TryGetValue(
                HashUtilities.GuidForTestCycleID(AssemblyPrep.Source, typeof(MockTestClass1).FullName),
                out UTF.ITestCycle class1Cycle));

            Assert.IsTrue(MockTests.TryGetValue(
                HashUtilities.GuidForTestCycleID(AssemblyPrep.Source, typeof(MockTestClass2).FullName),
                out UTF.ITestCycle class2Cycle));

            Assert.IsTrue(MockTests.TryGetValue(
                HashUtilities.GuidForTestCycleID(AssemblyPrep.Source, typeof(OneMoreScope.MockTestClass3).FullName),
                out UTF.ITestCycle class3Cycle));

            Assert.IsTrue(MockTests.TryGetValue(
                HashUtilities.GuidForTestCycleID(AssemblyPrep.Source, typeof(OneMoreScope.MockTestClass4).FullName),
                out UTF.ITestCycle class4Cycle));

            // Test Method cycle
            Assert.IsTrue(MockTests.TryGetValue(
                HashUtilities.GuidForTestCycleID(
                    AssemblyPrep.Source, AssemblyPrep.ReflectionHelper.GetTestCycleNameForMethodScope(typeof(MockTestClass1))),
                out UTF.ITestCycle method1Cycle));

            Assert.IsTrue(MockTests.TryGetValue(
                HashUtilities.GuidForTestCycleID(
                    AssemblyPrep.Source, AssemblyPrep.ReflectionHelper.GetTestCycleNameForMethodScope(typeof(MockTestClass2))),
                out UTF.ITestCycle method2Cycle));

            Assert.IsTrue(MockTests.TryGetValue(
                HashUtilities.GuidForTestCycleID(
                    AssemblyPrep.Source, AssemblyPrep.ReflectionHelper.GetTestCycleNameForMethodScope(typeof(OneMoreScope.MockTestClass3))),
                out UTF.ITestCycle method3Cycle));

            Assert.IsTrue(MockTests.TryGetValue(
                HashUtilities.GuidForTestCycleID(
                    AssemblyPrep.Source, AssemblyPrep.ReflectionHelper.GetTestCycleNameForMethodScope(typeof(OneMoreScope.MockTestClass4))),
                out UTF.ITestCycle method4Cycle));

            // Test integrity betweeen parent and children test cycles.
            Assert.IsTrue(appDomainCycle.Children.Count == 1 && appDomainCycle.Children.First().ID == assemblyCycle.ID);
            Assert.IsTrue(assemblyCycle.Children.Count == 2 && assemblyCycle.Children[0].ID == namespace1Cycle.ID && assemblyCycle.Children[1].ID == namespace2Cycle.ID);
            Assert.AreEqual(7, namespace1Cycle.Children.Count);
            Assert.AreEqual(3, namespace2Cycle.Children.Count);
            Assert.IsTrue(namespace1Cycle.Any(cycle => cycle.FullName == class1Cycle.FullName));
            Assert.IsTrue(namespace1Cycle.Any(cycle => cycle.FullName == class2Cycle.FullName));
            Assert.IsTrue(class1Cycle.Children.Count == 1 && class1Cycle.Children.First().ID == method1Cycle.ID);
            Assert.IsTrue(class2Cycle.Children.Count == 1 && class2Cycle.Children.First().ID == method2Cycle.ID);

            Assert.AreEqual(appDomainCycle.ID, assemblyCycle.ParentID);
            Assert.AreEqual(assemblyCycle.ID, namespace1Cycle.ParentID);
            Assert.AreEqual(namespace1Cycle.ID, class1Cycle.ParentID);
            Assert.AreEqual(namespace2Cycle.ID, class3Cycle.ParentID);
            Assert.AreEqual(class1Cycle.ID, method1Cycle.ParentID);
        }

        [TestMethod]
        public void TestIfSupportingAttributeRegistered()
        {
            MockTests.TryGetValue(
                HashUtilities.GuidForTestCycleID(
                    AssemblyPrep.Source, AssemblyPrep.ReflectionHelper.GetTestCycleNameForMethodScope(typeof(MockTestClass1))),
                out UTF.ITestCycle method1Cycle);

            Assert.AreEqual(1, method1Cycle.SupportMethodsGroups.Count);
            Assert.IsNotNull(method1Cycle.SupportMethodsGroups.First().InitializeMethod);
            Assert.IsNotNull(method1Cycle.SupportMethodsGroups.First().CleanupMethod);

            MockTests.TryGetValue(
                HashUtilities.GuidForTestCycleID(AssemblyPrep.Source, typeof(MockTestClass1).FullName),
                out UTF.ITestCycle class1Cycle);

            Assert.AreEqual(1, class1Cycle.SupportMethodsGroups.Count);
            Assert.IsNotNull(class1Cycle.SupportMethodsGroups.First().InitializeMethod);
            Assert.IsNotNull(class1Cycle.SupportMethodsGroups.First().CleanupMethod);

            MockTests.TryGetValue(
                HashUtilities.GuidForTestCycleID(AssemblyPrep.Source, typeof(MockTestClass1).Namespace),
                out UTF.ITestCycle namespaceCycle);

            Assert.AreEqual(2, namespaceCycle.SupportMethodsGroups.Count);
            Assert.IsNotNull(namespaceCycle.SupportMethodsGroups.ElementAt(0).InitializeMethod);
            Assert.IsNotNull(namespaceCycle.SupportMethodsGroups.ElementAt(0).CleanupMethod);
            Assert.IsNotNull(namespaceCycle.SupportMethodsGroups.ElementAt(1).InitializeMethod);
            Assert.IsNotNull(namespaceCycle.SupportMethodsGroups.ElementAt(1).CleanupMethod);

            MockTests.TryGetValue(
                HashUtilities.GuidForTestCycleID(AssemblyPrep.Source, typeof(MockTestClass1).Assembly.FullName),
                out UTF.ITestCycle assemblyCycle);

            Assert.AreEqual(4, assemblyCycle.SupportMethodsGroups.Count);
        }

        [TestMethod]
        public void TestSupportingAttrOnDerivedClass()
        {
            Assert.IsTrue(MockTests.TryGetValue(
                HashUtilities.GuidForTestCycleID(AssemblyPrep.Source, typeof(OneMoreScope.MockTestClass4).FullName),
                out UTF.ITestCycle class4Cycle));

            Assert.AreEqual(2, class4Cycle.SupportMethodsGroups.Count);

            MockTests.TryGetValue(
                HashUtilities.GuidForTestCycleID(
                    AssemblyPrep.Source, AssemblyPrep.ReflectionHelper.GetTestCycleNameForMethodScope(typeof(OneMoreScope.MockTestClass4))),
                out UTF.ITestCycle method4Cycle);

            Assert.AreEqual(2, method4Cycle.SupportMethodsGroups.Count);
        }

        [TestMethod]
        public void TestExecutorNumbers()
        {
            MockTests.TryGetValue(
                HashUtilities.GuidForTestCycleID(
                    AssemblyPrep.Source, AssemblyPrep.ReflectionHelper.GetTestCycleNameForMethodScope(typeof(MockTestClass1))),
                out UTF.ITestCycle methodCycle);

            Assert.AreEqual(2, methodCycle.TestMethodContexts.Count);
        }

        [TestMethod]
        public void TestPreparationOrder()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                TextWriterTraceListener listener = new TextWriterTraceListener(stream);
                Trace.Listeners.Add(listener);
                AssemblyPrep.TestEngine.RunTests(MockTests.TestContextLookup.Keys, 0, AssemblyPrep.Logger);
                Trace.Flush();

                stream.Position = 0;
                using (StreamReader reader = new StreamReader(stream))
                {
                    Assert.AreEqual("InitializeOnMethod6", reader.ReadLine());
                    Assert.AreEqual("InitializeOnMethod7", reader.ReadLine());
                    Assert.AreEqual("CleanupOnMethod7", reader.ReadLine());
                    Assert.AreEqual("CleanupOnMethod6", reader.ReadLine());
                    Assert.AreEqual("InitializeOnMethod3", reader.ReadLine());
                    Assert.AreEqual("InitializeOnMethod4", reader.ReadLine());
                    Assert.AreEqual("InitializeOnMethod5", reader.ReadLine());
                    Assert.AreEqual("CleanupOnMethod5", reader.ReadLine());
                    Assert.AreEqual("CleanupOnMethod4", reader.ReadLine());
                    Assert.AreEqual("CleanupOnMethod3", reader.ReadLine());
                }
            }
        }
    }
}
