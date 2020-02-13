using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AtlasModTestAdapter.Resources;
using AtlasModTestAdapter.Utilities;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace AtlasModTestAdapter
{
    /// <summary>
    /// <para>ITestExecutor implementation that is called by vs test framework or the IDE.</para>
    /// <para>See https://github.com/Microsoft/vstest-docs/blob/master/RFCs/0004-Adapter-Extensibility.md .</para>
    /// </summary>
    public class AtlasModTestExecutor : ITestExecutor
    {
        internal const int Timeout = 5000;
        internal const string ConnectTimeout = "Connect operation to App is time-out";
        internal const string SendTimeout = "Send operation to App is  time-out";

        private readonly int backlog = 10;

        private Socket GetSocket(IPEndPoint endPoint)
        {
            Debug.Assert(endPoint != null, "endPoint should not be null");

            return new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                SendTimeout = Timeout,
                ReceiveTimeout = Timeout,
            };
        }

        private Socket GetListeningSocket(IPEndPoint endPoint)
        {
            Socket socket = this.GetSocket(endPoint);
            socket.Bind(endPoint);
            return socket;
        }

        private byte[] GetSerializedMethodInfo<T>(T testCase)
        {
            DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(T));
            using (MemoryStream memoryStream = new MemoryStream())
            {
                dataContractSerializer.WriteObject(memoryStream, testCase);
                return memoryStream.ToArray();
            }
        }

        private void ConnectCallback(IAsyncResult result)
        {
            var sender = (Socket)result.AsyncState;
            sender?.EndConnect(result);
        }

        private void SendCallback(IAsyncResult result)
        {
            var sender = (Socket)result.AsyncState;
            sender?.EndSend(result);
        }

        private void AcceptCallback(IAsyncResult ar)
        {

        }

        internal class StateObject
        {
            /// <summary>
            /// Buffer size.
            /// </summary>
            public const int BufferSize = 10240;

            /// <summary>
            /// Buffer used for receiving test result.
            /// </summary>
            public byte[] Buffer = new byte[BufferSize];

            /// <summary>
            /// Initializes a new instance of the <see cref="StateObject"/> class.
            /// </summary>
            /// <param name="socket">socket in use.</param>
            public StateObject(Socket socket)
            {
                this.Socket = socket;
            }

            /// <summary>
            /// Gets or sets underlying socket.
            /// </summary>
            public Socket Socket { get; set; }
        }

        #region ITestExecutor Implementation
        public void Cancel()
        {
            throw new NotImplementedException();
        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            ValidateArg.NotNull(tests, nameof(tests));

            Socket sender = this.GetSocket(AtlasConstants.LocalEndPoint);
            Socket listener = this.GetListeningSocket(AtlasConstants.LocalEndPoint);

            try
            {
                if (!this.Connect(sender, tests, frameworkHandle))
                {
                    return;
                }

                // Put listen before send in case of instant completion of send.
                listener.Listen(this.backlog);

                if (!this.SendTests(sender, tests, frameworkHandle))
                {
                    return;
                }

                byte[] bytes = this.GetSerializedMethodInfo(tests.First());
                if (!sender.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, this.SendCallback, sender)
                    .AsyncWaitHandle.WaitOne(Timeout))
                {
                    return new TestResult[] { new TestResult() { Outcome = UnitTestOutcome.Error, LogError = SendTimeout } };
                }

                StateObject state = new StateObject();
                listener.BeginAccept(this.AcceptCallback, state);

                listener.Accept();
            }
            catch (SocketException e)
            {
                return new TestResult[]
                {
                    new TestResult()
                    {
                        Outcome = UnitTestOutcome.Error,
                        LogError = string.Concat(e.SocketErrorCode.ToString(), Environment.NewLine, e.Message, Environment.NewLine, e.StackTrace),
                    },
                };
            }
            finally
            {
                sender.Dispose();
                listener.Dispose();
            }
        }



        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            throw new NotImplementedException();
        }
        #endregion

        private bool Connect(Socket sender, IEnumerable<TestCase> tests, IFrameworkHandle frameworkHandle)
        {
            if (!sender.BeginConnect(AtlasConstants.RemoteEndPoint, this.ConnectCallback, sender)
                    .AsyncWaitHandle.WaitOne(Timeout))
            {
                ReportUtilities.BatchErrorReport(
                    tests,
                    frameworkHandle,
                    string.Format(CultureInfo.InvariantCulture, StringResources.TimeoutFormattableString, "Connect"));

                return false;
            }

            return true;
        }

        private bool SendTests(Socket sender, IEnumerable<TestCase> tests, IFrameworkHandle frameworkHandle)
        {
            byte[] bytes = this.GetSerializedMethodInfo(tests);
            int size = bytes.Length;
            int byteSent = 0;

            for (int i = 0; i < Properties.Settings.Default.TimesToRetry; i++)
            {
                byteSent = sender.Send(bytes, byteSent, size, SocketFlags.None, out SocketError error);
                if (byteSent != size)
                {
                    if (error == SocketError.NoBufferSpaceAvailable || error == SocketError.IOPending)
                    {
                        size -= byteSent;
                        continue;
                    }
                    else
                    {
                        ReportUtilities.BatchErrorReport(
                            tests,
                            frameworkHandle,
                            string.Format(CultureInfo.InvariantCulture, StringResources.TimeoutFormattableString, "Send"));
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        private bool GetTestResult(Socket listener, TestCase test, IFrameworkHandle frameworkHandle)
        {
            StateObject state = new StateObject(listener);
            IAsyncResult asyncResult = listener.BeginAccept(this.AcceptCallback, state);
            if (asyncResult.AsyncWaitHandle.WaitOne(Timeout))
            {
                Socket worker = listener.EndAccept(asyncResult);

                return true;
            }

            frameworkHandle.RecordResult(
                new TestResult(test)
                {
                    Outcome = TestOutcome.Failed,
                    ErrorMessage = string.Format(CultureInfo.InvariantCulture, StringResources.TimeoutFormattableString, "Accept"),
                });
            return false;
        }
    }
}
