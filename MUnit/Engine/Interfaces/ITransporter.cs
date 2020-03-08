// <copyright file="ITransporter.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using MUnit.Engine;

namespace MUnit.Transport
{
    /// <summary>
    /// Interface for transporter used for IPC.
    /// </summary>
    public interface ITransporter : IDisposable
    {
        /// <summary>
        /// Gets message queue.
        /// </summary>
        IConcurrentQueue<WireMessage> MessageQueue { get; }

        /// <summary>
        /// Initialize the transporter.
        /// </summary>
        void Start();

        /// <summary>
        /// Send <paramref name="data"/> to remote host. Blocks current method.
        /// </summary>
        /// <param name="data"> Data to be sent. </param>
        /// <returns> ID for the <paramref name="data"/>. </returns>
        uint Send(WireMessage data);

        /// <summary>
        /// Receive <see cref="WireMessage"/> from remote host. Blocks current thread.
        /// </summary>
        /// <returns> A <see cref="WireMessage"/> from remote host. </returns>
        WireMessage Receive();

        /// <summary>
        /// Send data asynchronously.
        /// </summary>
        /// <param name="data"> Data to be sent. </param>
        void SendAsync(WireMessage data);
    }
}