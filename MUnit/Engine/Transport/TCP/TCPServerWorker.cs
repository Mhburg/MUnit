// <copyright file="TCPServerWorker.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using MUnit.Engine;

namespace MUnit.Transport
{
    /// <summary>
    /// Provides server functionality in TCP.
    /// </summary>
    internal class TCPServerWorker : TCPTransporter, ITransporter
    {
        private Status _status;
        private Socket _listener;
        private int _backlog = 10;
        private IAsyncResult _asyncResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="TCPServerWorker"/> class.
        /// </summary>
        public TCPServerWorker()
            : base()
        {
            _status = Status.Idle;
        }

        internal enum Status
        {
            Idle,
            Listening,
            Accepting,
            Receiving,
            Sending,
        }

        /// <inheritdoc/>
        public override void Start()
        {
            switch (_status)
            {
                case Status.Idle:
                    this.StartListening(ref _listener);
                    break;

                case Status.Listening:
                    _asyncResult = this.StartAccepting(_listener);
                    break;

                case Status.Accepting:
                    if (_asyncResult.IsCompleted)
                    {
                        _handler = _listener.EndAccept(_asyncResult);
                        _handler.ReceiveTimeout = Engine.Service.MUnitConfiguration.ReceiveTimeout;
                        _handler.SendTimeout = Engine.Service.MUnitConfiguration.SendTimeout;
                        _status = Status.Receiving;
                        this.DeserializeMessage(_handler);
                    }

                    break;
                case Status.Receiving:
                    this.DeserializeMessage(_handler);
                    break;

                case Status.Sending:

                    break;
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                // No op.
            }

            _listener?.Close();
        }

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Async call")]
        private void StartListening(ref Socket listener)
        {
            if (listener == null)
                listener = this.GetListeningSocket(TCPConstants.ServerEndPoint);

            listener.Listen(_backlog);
            _status = Status.Listening;
        }

        private IAsyncResult StartAccepting(Socket listener)
        {
            IAsyncResult asyncResult = listener.BeginAccept(null, null);
            _status = Status.Accepting;

            return asyncResult;
        }

        private Socket GetListeningSocket(IPEndPoint endPoint)
        {
            Socket socket = GetSocket(endPoint.AddressFamily);
            socket.Bind(endPoint);
            return socket;
        }

#if DEBUG
        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "Property is for test only")]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "Property is for test only")]
        internal Status ServerStatus { get => _status; }
#endif
    }
}
