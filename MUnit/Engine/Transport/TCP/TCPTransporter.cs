// <copyright file="TCPTransporter.cs" company="Zizhen Li">
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
using System.Threading;
using MUnit.Engine;
using MUnit.Resources;
using MUnit.Utilities;

namespace MUnit.Transport
{
    /// <summary>
    /// TCP implementation of <see cref="ITransporter"/>.
    /// </summary>
    /// <remarks>
    /// <para> Don't block thread that calls Begin"Operation" and call set on AsyncCallback. </para>
    /// <para> It will cause deadlock if Begin"Operation" is called on UI thread. </para>
    /// <para> See https://referencesource.microsoft.com/#System/net/System/Net/_ContextAwareResult.cs,eaca96f5871f3ea9,references .</para>
    /// </remarks>
    internal abstract class TCPTransporter : ITransporter
    {
        protected uint _messageID = 0;
        protected Socket _handler = null;
        protected IPEndPoint _remoteEndPoint = null;

        private readonly byte[] _eof;
        private StateObject _stateObject;
        private volatile bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="TCPTransporter"/> class.
        /// </summary>
        /// <param name="logger"> Logger used by test engine. </param>
        public TCPTransporter()
        {
            _eof = SerializeHelper.BinarySerialize(TCPConstants.MUnitEOF);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TCPTransporter"/> class.
        /// </summary>
        /// <param name="handler"> Socket used for handling traffic for this instance. </param>
        public TCPTransporter(Socket handler)
            : this()
        {
            _handler = handler;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="TCPTransporter"/> class.
        /// </summary>
        ~TCPTransporter()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets or sets received message.
        /// </summary>
        public IConcurrentQueue<WireMessage> MessageQueue { get; set; } = PlatformService.GetServiceManager().GetConcurrentQueue<WireMessage>();

        /// <inheritdoc/>
        public bool Connected => _handler.Connected;

        /// <summary>
        /// Connect to <paramref name="endPoint"/>.
        /// </summary>
        /// <param name="endPoint"> Endpoint to connect to. </param>
        public virtual void Connect(IPEndPoint endPoint)
        {
            _handler.Connect(endPoint);
        }

        /// <inheritdoc/>
        /// <exception cref="TimeoutException"> Throws if operation takes longer than <see cref="Service.MUnitConfiguration.SendTimeout"/>. </exception>
        /// <exception cref="SocketException"> Re-throws exception thrown by underlying TCP socket. </exception>
        public virtual uint Send(WireMessage data)
        {
            ThrowUtilities.NullArgument(data, nameof(data));
            Debug.Assert(_handler != null, "_handler should not be null." + Environment.NewLine + Environment.StackTrace);

            data.ID = _messageID++;
            byte[] bytes = SerializeHelper.BinarySerialize(data, _eof);

            _handler.Send(bytes, 0, bytes.Length, SocketFlags.None, out SocketError socketError);

            if (socketError != SocketError.Success)
            {
                if (socketError == SocketError.TimedOut)
                {
                    throw new TimeoutException(string.Format(
                        CultureInfo.CurrentCulture,
                        Errors.UTE_TimeoutFormattableString,
                        "Send"));
                }
                else
                {
                    throw new SocketException((int)socketError);
                }
            }

            return data.ID;
        }

        /// <inheritdoc/>
        /// <exception cref="TimeoutException"> Throws if operation takes longer than <see cref="Service.MUnitConfiguration.SendTimeout"/>. </exception>
        /// <exception cref="SocketException"> Re-throws exception thrown by underlying TCP socket. </exception>
        public void SendAsync(WireMessage data)
        {
            ThrowUtilities.NullArgument(data, nameof(data));
            Debug.Assert(_handler != null, "_handler should not be null." + Environment.NewLine + Environment.StackTrace);

            if (!_handler.Connected)
                _handler.Connect(_remoteEndPoint ?? _handler.RemoteEndPoint);

            if (data.Type == WireMessageTypes.Request)
                data.ID = _messageID++;

            byte[] bytes = SerializeHelper.BinarySerialize(data, _eof);

            _handler.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, SendAsyncCallback, _handler);
        }

        /// <inheritdoc/>
        public WireMessage Receive()
        {
            while (!this.MessageQueue.Any)
            {
                this.DeserializeMessage(_handler);
            }

            this.MessageQueue.TryDequeue(out WireMessage wireMessage);
            return wireMessage;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public abstract void Start();

        /// <summary>
        /// Returns a TCP socket.
        /// </summary>
        /// <param name="addressFamily"> <see cref="AddressFamily"/> of the socket to be created. </param>
        /// <returns> A TCP socket. </returns>
        protected static Socket GetSocket(AddressFamily addressFamily)
        {
            return new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                SendTimeout = Engine.Service.MUnitConfiguration.SendTimeout,
                ReceiveTimeout = Engine.Service.MUnitConfiguration.ReceiveTimeout,
            };
        }

        /// <summary>
        /// Dispose resources used by this transporter.
        /// </summary>
        /// <param name="disposing"> True if <see cref="TCPServer.Dispose"/> is called by user. </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                if (disposing)
                {
                    // No op.
                }

                _handler?.Close();
            }
        }

        /// <summary>
        /// Receive byte transmission synchronously and deserialize them to <see cref="WireMessage"/>.
        /// </summary>
        /// <param name="handler"> Socket that handles data transmission. </param>
        /// <exception cref="SocketException"> Re-throw from <paramref name="handler"/>. </exception>
        protected void DeserializeMessage(Socket handler)
        {
            if (_stateObject == null)
                _stateObject = new StateObject();

            if (_stateObject.Stream.Length > _eof.Length)
            {
                byte[] buffer = _stateObject.Stream.ToArray();
                if (this.IsEOF(buffer, _stateObject.StartReadPos, buffer.Length, out int index))
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        stream.Write(buffer, 0, index + 1);
                        stream.Position = 0;
                        WireMessage wireMessage = SerializeHelper.BinaryRead<WireMessage>(stream);
                        this.MessageQueue.Enqueue(wireMessage);
                    }

                    _stateObject.ResetStream(buffer, index + _eof.Length + 1);
                    return;
                }
                else
                {
                    _stateObject.StartReadPos = buffer.Length;
                }
            }

            if (!_disposed && handler.Available > 0)
            {
                byte[] buffer = new byte[TCPConstants.TCPBufferSize];
                int byteRead = handler.Receive(
                    buffer,
                    0,
                    buffer.Length,
                    SocketFlags.None,
                    out SocketError socketError);
                _stateObject.Stream.Write(buffer, 0, byteRead);

                if (socketError != SocketError.Success)
                    throw new SocketException((int)socketError);
            }

            if (!handler.Connected)
                throw new SocketException((int)SocketError.NotConnected);
        }

        /// <summary>
        /// Search EOF marking.
        /// </summary>
        /// <param name="buffer"> The byte array used for search. </param>
        /// <param name="offset"> Position in <paramref name="buffer"/> where the function starts to search. </param>
        /// <param name="length"> Number of bytes will be examined. </param>
        /// <param name="index"> Position in <paramref name="buffer"/> which is just before EOF marking. </param>
        /// <returns> Returns true if EOF marking is found. </returns>
        protected bool IsEOF(byte[] buffer, int offset, int length, out int index)
        {
            Debug.Assert(buffer != null, "Buffer is null");
            Debug.Assert(!(offset < 0 || offset > buffer.Length), "offset is out of range.");
            Debug.Assert(!(length > buffer.Length - offset), "Number of bytes requested to read is out of range");

            index = -1;
            for (int i = offset, j = 0; i < offset + length; i++)
            {
                if (buffer[i] == _eof[j])
                {
                    if (j == _eof.Length - 1)
                    {
                        index = i - j - 1;
                        return true;
                    }

                    j++;
                    continue;
                }
                else
                {
                    j = 0;
                    if (buffer[i] == _eof[j])
                        ++j;
                }
            }

            return false;
        }

        private void SendAsyncCallback(IAsyncResult ar)
        {
            Socket handler = (Socket)ar.AsyncState;

            // Allows exceptions to bubble up in the captured context.
            handler.EndSend(ar);
        }

        /// <summary>
        /// State object used in <see cref="System.Net.Sockets.Socket"/>'s async methods, e.g., <see cref="Socket.BeginAccept(AsyncCallback, object)"/>.
        /// </summary>
        protected class StateObject : IDisposable
        {
            /// <summary>
            /// Gets or sets starting position when <see cref="StateObject.Stream"/> is read.
            /// </summary>
            /// <remarks> It will be moved X index forward when X number of bytes are processed. </remarks>
            public int StartReadPos { get; set; } = 0;

            public MemoryStream Stream { get; set; } = new MemoryStream();

            public ManualResetEvent ResetEvent { get; set; } = new ManualResetEvent(false);

            public void Dispose()
            {
                this.ResetEvent.Close();
                this.Stream.Dispose();
            }

            internal void ResetStream(byte[] buffer, int offset)
            {
                this.Stream.Dispose();
                this.Stream = new MemoryStream();
                this.Stream.Write(buffer, offset, buffer.Length - offset);
                this.StartReadPos = 0;
            }
        }
    }
}
