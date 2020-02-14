// <copyright file="TestTransporter.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using AtlasModTestAdapter.Resources;
using AtlasModTestAdapter.Utilities;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace AtlasModTestAdapter
{
    /// <summary>
    /// Implementation of ITestTransport.
    /// </summary>
    public class TestTransporter : ITestTransport
    {
        private Socket sender;
        private Socket listener;
        private int canceled = 0;
        private int backlog = 10;

        #region ITestTransport Implementation

        /// <inheritdoc/>
        public bool SendTests(IEnumerable<TestCase> tests, IFrameworkHandle frameworkHandle)
        {
            this.sender = this.GetSocket(AtlasConstants.LocalEndPoint.AddressFamily);
            if (!this.Connect(this.sender, tests, frameworkHandle))
            {
                this.sender.Dispose();
                return false;
            }

            this.StartListening();

            byte[] bytes = SerializeUtilites.BinarySerialize(tests);
            int size = bytes.Length;
            int byteSent = 0;

            if (this.canceled == 1)
            {
                this.sender.Dispose();
                this.listener.Dispose();
                return false;
            }

            try
            {
                while (byteSent != size)
                {
                    size -= byteSent;
                    byteSent = this.sender.Send(bytes, byteSent, size, SocketFlags.None, out SocketError error);
                }

                return true;
            }
            catch (SocketException e)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine(string.Format(CultureInfo.InvariantCulture, StringResources.SocketError, e.SocketErrorCode));
                builder.AppendLine(string.Format(CultureInfo.InvariantCulture, StringResources.TimeoutFormattableString, "Send"));

                ReportUtilities.BatchErrorReport(
                    tests,
                    frameworkHandle,
                    builder.ToString());

                return false;
            }
            finally
            {
                this.sender.Dispose();
                this.listener.Dispose();
            }
        }


        /// <inheritdoc/>
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Unlikely be null")]
        public bool GetTestResults(IEnumerable<TestCase> tests, IFrameworkHandle frameworkHandle)
        {
            Debug.Assert(frameworkHandle != null, "frameworkHandle is null");

            byte[] buffer = new byte[10240];
            Socket handler = this.listener.Accept();
            MemoryStream stream = new MemoryStream();

            try
            {
                while (handler.Available > 0)
                {
                    if (this.canceled == 1)
                    {
                        stream.Dispose();
                        handler.Dispose();
                        this.listener.Dispose();
                        return false;
                    }

                    int byteRead = handler.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                    stream.Write(buffer, 0, byteRead);
                }

                stream.Position = 0;
                using (XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(stream, new XmlDictionaryReaderQuotas()))
                {
                    DataContractSerializer ser = new DataContractSerializer(typeof(IEnumerable<TestResult>));

                    IEnumerable<TestResult> results = (IEnumerable<TestResult>)ser.ReadObject(reader);

                    foreach (TestResult result in results)
                    {
                        frameworkHandle.RecordResult(result);
                    }

                    return true;
                }
            }
            catch (SocketException e)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine(string.Format(CultureInfo.InvariantCulture, StringResources.SocketError, e.SocketErrorCode));
                builder.AppendLine(string.Format(CultureInfo.InvariantCulture, StringResources.TimeoutFormattableString, "Receive"));

                ReportUtilities.BatchErrorReport(
                    tests,
                    frameworkHandle,
                    builder.ToString());

                return false;
            }
            finally
            {
                stream.Dispose();
                handler.Dispose();
                this.listener.Dispose();
            }
        }

        /// <inheritdoc/>
        public void Cancel()
        {
            Interlocked.Exchange(ref this.canceled, 1);
        }

        #endregion

        private bool Connect(Socket sender, IEnumerable<TestCase> tests, IFrameworkHandle frameworkHandle)
        {
            if (!sender.BeginConnect(AtlasConstants.RemoteEndPoint, this.ConnectCallback, sender)
                    .AsyncWaitHandle.WaitOne(Properties.Settings.Default.ConnectionTimeout))
            {
                ReportUtilities.BatchErrorReport(
                    tests,
                    frameworkHandle,
                    string.Format(CultureInfo.InvariantCulture, StringResources.TimeoutFormattableString, "Connect"));

                return false;
            }

            return true;
        }

        private void StartListening()
        {
            this.listener = this.GetListeningSocket(AtlasConstants.LocalEndPoint);
            this.listener.Listen(this.backlog);
        }

        private Socket GetSocket(AddressFamily addressFamily)
        {
            return new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                SendTimeout = Properties.Settings.Default.SendReceiveTimeout,
                ReceiveTimeout = Properties.Settings.Default.SendReceiveTimeout,
            };
        }

        private Socket GetListeningSocket(IPEndPoint endPoint)
        {
            Socket socket = this.GetSocket(endPoint.AddressFamily);
            socket.Bind(endPoint);
            return socket;
        }

        private void ConnectCallback(IAsyncResult result)
        {
            var sender = (Socket)result.AsyncState;
            sender?.EndConnect(result);
        }
    }
}
