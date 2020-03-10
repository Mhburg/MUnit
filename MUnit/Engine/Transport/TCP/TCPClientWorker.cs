// <copyright file="TCPClientWorker.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Net;
using System.Threading;
using MUnit.Engine;
using MUnit.Engine.Service;

namespace MUnit.Transport
{
    /// <summary>
    /// Provides client functionality in TCP.
    /// </summary>
    internal class TCPClientWorker : TCPTransporter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TCPClientWorker"/> class.
        /// </summary>
        public TCPClientWorker()
            : base(GetSocket(TCPConstants.MachineAddressFamily))
        {
            _remoteEndPoint = TCPConstants.ServerEndPoint;
            _handler.SendTimeout = MUnitConfiguration.SendTimeout;
            _handler.ReceiveTimeout = MUnitConfiguration.ReceiveTimeout;
        }

        /// <inheritdoc/>
        public override void Start()
        {
            if (!_handler.Connected)
                this.Connect(new IPEndPoint(MUnitConfiguration.ServerIP, MUnitConfiguration.ServerPort));
        }
    }
}
