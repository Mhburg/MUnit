using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MUnit.Transport;
using MUnit.Utilities;

using UTF = MUnit.Framework;

namespace MUnit.Test.Transport.TestMUnitClient
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestFunctinality
    {
        private static TCPServer _tcpServer = new TCPServer(AssemblyPrep.TestEngine);
        private static TCPClient _tcpClient = new TCPClient(AssemblyPrep.TestEngine);
        private static CancellationTokenSource _cancelSource = new CancellationTokenSource();
        private static CancellationToken _cancelToken;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
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
            _cancelSource.Cancel();
            _tcpClient.Dispose();
            _tcpServer.Dispose();
        }

        [TestMethod]
        public void TestDiscoverTests()
        {
            IEnumerable<string> sources = new List<string>() { AssemblyPrep.Source };
            IEnumerable<Guid> actualTests = _tcpClient.DiscoverTests(sources).Select(t => t.TestID);
            IEnumerable<Guid> expectedTests = AssemblyPrep.TestEngine.DiscoverTests(sources).TestContextLookup.Keys;

            Assert.AreEqual(expectedTests.Count(), actualTests.Count());
            Assert.IsTrue(!expectedTests.Except(actualTests).Any());
        }

        [DataTestMethod]
        [DynamicData("CallFunctionProvider", DynamicDataSourceType.Method)]
        public void TestCallFunction(MethodInfo method, Type type, object[] parameters, object[] ctorParams, string expectedString)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                TextWriterTraceListener listener = new TextWriterTraceListener(stream);
                Trace.Listeners.Add(listener);

                _tcpClient.RemoteProcedureCall(method, type, parameters, ctorParams);

                Task.Delay(1000).Wait();
                Trace.Flush();

                stream.Position = 0;
                using (StreamReader reader = new StreamReader(stream))
                {
                    Assert.AreEqual(expectedString, reader.ReadLine());
                }
            }
        }

        public static IEnumerable<object[]> CallFunctionProvider()
        {
            // Invoke a member function of a class.
            yield return new object[] {
                                typeof(SimpleTestMethodCycle).GetMethod("SimpleTestMethod1"),
                                typeof(SimpleTestMethodCycle),
                                null,
                                null,
                                "SimpleTestMethod1"
            };

            // Call a class constructor with one parameter, then invoke a member function.
            yield return new object[] {
                                typeof(SimpleTestMethodCycle).GetMethod("SimpleInstanceMethod"),
                                typeof(SimpleTestMethodCycle),
                                null,
                                new object[] { "Simple instance string" },
                                "Simple instance string",
            };

            // Invoke a static function.
            yield return new object[] {
                                typeof(SimpleTestMethodCycle).GetMethod("SimpleStaticMethod"),
                                null,
                                null,
                                null,
                                "Static string"
            };
        }
    }
}
