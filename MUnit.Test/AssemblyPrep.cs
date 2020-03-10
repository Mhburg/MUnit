using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MUnit.Engine;
using MUnit.Utilities;

using UTF = MUnit.Framework;

namespace MUnit.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AssemblyPrep
    {
        public static MUnitLogger Logger = new MUnitLogger(UTF.MessageLevel.Debug);
        public static ITestEngine TestEngine = new MUnitEngine(Logger);
        public static string Source = Assembly.GetExecutingAssembly().Location;
        internal static ReflectionHelper ReflectionHelper = new ReflectionHelper(PlatformService.GetServiceManager().ReflectionCache);
        public static UTF.ITestCycleGraph MockTests = AssemblyPrep.TestEngine.DiscoverTests(new List<string>() { AssemblyPrep.Source });

        [AssemblyInitialize]
        public static void Initialize(TestContext testContext)
        {
        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
            Logger.WriteToFile();
        }

        public static void RunSingleTest(Type testClass, string testMethod)
        {
            AssemblyPrep.TestEngine
                .RunTests(
                    new[] { HashUtilities.GuidForTestCycleID(AssemblyPrep.Source, testClass.FullName + "." + testMethod) },
                    0,
                    AssemblyPrep.Logger);
        }
    }
}
