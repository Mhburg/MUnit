using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using MUnit.Engine;
using MUnit.Transport;
using MUnit.Utilities;

using UTF = MUnit.Framework;

namespace MUnit.Test.Transport
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AssemblyPrep
    {
        private static FileStream _fileStream;

        public static MUnitLogger Logger = new MUnitLogger(UTF.MessageLevel.Debug);
        public static ITestEngine TestEngine = new MUnitEngine(Logger);
        public static string Source = Assembly.GetExecutingAssembly().Location;
        public static byte[] EOFBinary = SerializeHelper.BinarySerialize(TCPConstants.MUnitEOF);

        public static UTF.ITestCycleGraph MockTests;

        internal static ReflectionHelper ReflectionHelper = new ReflectionHelper(PlatformService.GetServiceManager().ReflectionCache);

        [AssemblyInitialize]
        public static void Initialize(TestContext testContext)
        {
            MockTests = AssemblyPrep.TestEngine.DiscoverTests(new List<string>() { AssemblyPrep.Source });
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestLog.txt");
            Logger.WriteToFile(path);
            _fileStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None);
            Logger.MessageEvent += Logger_MessageEvent;
        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
            Logger.MessageEvent -= Logger_MessageEvent;
            _fileStream.Close();
        }

        public static void RunSingleTest(Type testClass, string testMethod)
        {
            AssemblyPrep.TestEngine
                .RunTests(
                    new[] { HashUtilities.GuidForTestCycleID(AssemblyPrep.Source, testClass.FullName + "." + testMethod) },
                    0,
                    AssemblyPrep.Logger);
        }

        private static void Logger_MessageEvent(object sender, UTF.MessageContext e)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat(
                Engine.Resources.Strings.FormattedLogMessage,
                e.Timestamp.ToLongTimeString(),
                e.Level,
                e.Message)
                .AppendLine();

            byte[] bytes = UTF8Encoding.UTF8.GetBytes(stringBuilder.ToString());
            _fileStream.Write(bytes, 0, bytes.Length);
            _fileStream.Flush();
        }
    }
}
