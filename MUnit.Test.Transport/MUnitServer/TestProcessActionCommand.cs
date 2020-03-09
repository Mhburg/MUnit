using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MUnit.Engine;
using MUnit.Transport;
using MUnit.Utilities;

namespace MUnit.Test.Transport.TestMUnitServer
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestProcessActionCommand
    {
        private static TCPServer _tcpServer;
        private static TCPClient _tcpClient;
        private static CancellationTokenSource _cancelSource = new CancellationTokenSource();
        private static CancellationToken _cancelToken;
        private static MethodInfo ProcessActionCommandForServer;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            MUnitEngine engine = new MUnitEngine(new MUnitLogger(Framework.MessageLevel.Debug));
            engine.DiscoverTests(new string[] { Path.GetFileName(Assembly.GetExecutingAssembly().Location) });
            _tcpServer = new TCPServer(engine, engine.Logger);

            engine = new MUnitEngine(new MUnitLogger(Framework.MessageLevel.Debug));
            engine.DiscoverTests(new string[] { Path.GetFileName(Assembly.GetExecutingAssembly().Location) });
            _tcpClient = new TCPClient(engine);

            ProcessActionCommandForServer = typeof(MUnitServer).GetMethod("ProcessActionCommand", BindingFlags.NonPublic | BindingFlags.Instance);
            _cancelToken = _cancelSource.Token;
            Task task = Task.Run(() =>
            {
                while (!_cancelToken.IsCancellationRequested)
                {
                    _tcpServer.Start();
                }
            }, _cancelToken);

            task.Wait(100);
            _tcpClient.Start();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            Task.Delay(500).Wait();
            _cancelSource.Cancel();
            _tcpClient.Dispose();
            _tcpServer.Dispose();
        }

        [TestMethod]
        public void TestCheckAssembly()
        {
            Assert.IsTrue(_tcpClient.CheckAssemblyHash(Assembly.GetExecutingAssembly().GetName().Name, out byte[] _, out byte[] _));
        }

        [DataTestMethod]
        [DynamicData("CallFunctionProvider", DynamicDataSourceType.Method)]
        public void TestCallFunction(WireMessage message, string expectedString)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                TextWriterTraceListener listener = new TextWriterTraceListener(stream);
                Trace.Listeners.Add(listener);

                ProcessActionCommandForServer.Invoke(_tcpServer, new object[] { message });
                Trace.Flush();

                stream.Position = 0;
                using (StreamReader reader = new StreamReader(stream))
                {
                    Assert.AreEqual(expectedString, reader.ReadLine());
                }
            }
        }

        [DataTestMethod]
        [DynamicData("RunTestsProvider", DynamicDataSourceType.Method)]
        public void TestRunTests(WireMessage message, IEnumerable<string> traceOutput)
        {
            VerifyTraceOutput(message, traceOutput);
        }

        [DataTestMethod]
        [DynamicData("CancelProvider", DynamicDataSourceType.Method)]
        public void TestCancel(Guid[] guids, int delay, IEnumerable<string> traceOutput)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                TextWriterTraceListener listener = new TextWriterTraceListener(stream);
                Trace.Listeners.Add(listener);

                _tcpClient.RunTests(guids, out int testRunID);
                Thread.Sleep(delay);
                _tcpClient.CancelTestRun();

                Thread.Sleep(2000);
                Trace.Flush();

                stream.Position = 0;
                using (StreamReader reader = new StreamReader(stream))
                {
                    foreach (string trace in traceOutput)
                    {
                        Assert.AreEqual(trace, reader.ReadLine());
                    }
                }
            }
        }

        public static IEnumerable<object[]> CancelProvider()
        {
            yield return new object[]
            {
                new Guid[] {
                    HashUtilities.GuidForTestCycleID(AssemblyPrep.Source, typeof(LongRunningTestClass).FullName + "." + "LongRunningTest1"),
                    HashUtilities.GuidForTestCycleID(AssemblyPrep.Source, typeof(LongRunningTestClass).FullName + "." + "LongRunningTest2"),
                    HashUtilities.GuidForTestCycleID(AssemblyPrep.Source, typeof(LongRunningTestClass).FullName + "." + "LongRunningTest3"),
                },
                1000,
                new string[] { "Entering LongRunningTest1...", "Leaving LongRunningTest1..." }
            };

            yield return new object[]
{
                new Guid[] {
                    HashUtilities.GuidForTestCycleID(AssemblyPrep.Source, typeof(LongRunningTestClass).FullName + "." + "LongRunningTest1"),
                    HashUtilities.GuidForTestCycleID(AssemblyPrep.Source, typeof(LongRunningTestClass).FullName + "." + "LongRunningTest2"),
                    HashUtilities.GuidForTestCycleID(AssemblyPrep.Source, typeof(LongRunningTestClass).FullName + "." + "LongRunningTest3"),
                },
                3000,
                new string[] {
                    "Entering LongRunningTest1...", "Leaving LongRunningTest1...",
                    "Entering LongRunningTest2...", "Leaving LongRunningTest2...",
                }
};
        }

        public static IEnumerable<object[]> RunTestsProvider()
        {
            yield return new object[]
            {
                new WireMessage(WireMessageTypes.Request, CommandType.RunTests, new Guid[] { HashUtilities.GuidForTestCycleID(AssemblyPrep.Source, typeof(SimpleTestMethodCycle).FullName + "." + "SimpleTestMethod1") }),
                new string[] { "InitializeOnSimpleMethod", "SimpleTestMethod1", "CleanupOnSimpleMethod" }
            };

            //yield return new object[]
            //{
            //    new WireMessage(
            //        WireMessageTypes.Request,
            //        CommandType.RunTests,
            //        new Guid[]
            //        {
            //            HashUtilities.GuidForTestCycleID(AssemblyPrep.Source, typeof(SimpleTestMethodCycle).FullName + "." + "SimpleTestMethod1"),
            //            HashUtilities.GuidForTestCycleID(AssemblyPrep.Source, typeof(SimpleTestMethodCycle).FullName + "." + "SimpleTestMethod2"),
            //        }),
            //    new string[]
            //    {
            //        "InitializeOnSimpleMethod", "SimpleTestMethod1", "CleanupOnSimpleMethod",
            //        "InitializeOnSimpleMethod", "SimpleTestMethod2", "CleanupOnSimpleMethod",
            //    }
            //};

            //yield return new object[]
            //{
            //    new WireMessage(
            //        WireMessageTypes.Request,
            //        CommandType.RunTests,
            //        new Guid[]
            //        {
            //            HashUtilities.GuidForTestCycleID(AssemblyPrep.Source, typeof(LongRunningTestClass).FullName + "." + "LongRunningTest1"),
            //            HashUtilities.GuidForTestCycleID(AssemblyPrep.Source, typeof(LongRunningTestClass).FullName + "." + "LongRunningTest2"),
            //            HashUtilities.GuidForTestCycleID(AssemblyPrep.Source, typeof(LongRunningTestClass).FullName + "." + "LongRunningTest3"),
            //        }),
            //    new string[]
            //    { 
            //        "Entering LongRunningTest1...", "Leaving LongRunningTest1...",
            //        "Entering LongRunningTest2...", "Leaving LongRunningTest2...",
            //        "Entering LongRunningTest3...", "Leaving LongRunningTest3...",
            //    }
            //};

            yield return new object[]
            {
                new WireMessage(
                    WireMessageTypes.Request,
                    CommandType.RunTests,
                    new string[] { Path.GetFileName(Assembly.GetExecutingAssembly().Location) }
                    ),
                new string[]
                {
                    "Entering LongRunningTest1...", "Leaving LongRunningTest1...",
                    "Entering LongRunningTest2...", "Leaving LongRunningTest2...",
                    "Entering LongRunningTest3...", "Leaving LongRunningTest3...",
                    "InitializeOnSimpleMethod", "SimpleTestMethod1", "CleanupOnSimpleMethod",
                    "InitializeOnSimpleMethod", "SimpleTestMethod2", "CleanupOnSimpleMethod",
                }
            };
        }

        public static IEnumerable<object[]> CallFunctionProvider()
        {
            // Invoke a member function of a class.
            yield return new object[] {
                new WireMessage(WireMessageTypes.Request,
                                CommandType.CallFunction,
                                typeof(SimpleTestMethodCycle).GetMethod("SimpleTestMethod1"),
                                typeof(SimpleTestMethodCycle)),
                "SimpleTestMethod1",
            };

            // Invoke a static function.
            yield return new object[] {
                new WireMessage(WireMessageTypes.Request,
                                CommandType.CallFunction,
                                typeof(MUnitServer).GetMethod("StaticSimpleTraceOutput"),
                                null,
                                new object[]{ "Test ProcessActionCommand function"} ),
                "Trace output from MUnitServere. Printing: Test ProcessActionCommand function",
            };
        }

        private void VerifyTraceOutput(WireMessage message, IEnumerable<string> traceOutput)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                TextWriterTraceListener listener = new TextWriterTraceListener(stream);
                Trace.Listeners.Add(listener);

                ProcessActionCommandForServer.Invoke(_tcpServer, new object[] { message });
                Trace.Flush();

                stream.Position = 0;
                using (StreamReader reader = new StreamReader(stream))
                {
                    foreach (string trace in traceOutput)
                    {
                        Assert.AreEqual(trace, reader.ReadLine());
                    }
                }
            }
        }
    }
}
