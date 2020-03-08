// <copyright file="MUnitWire.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using MUnit.Engine;
using MUnit.Framework;
using MUnit.Resources;
using MUnit.Utilities;

namespace MUnit.Transport
{
    /// <summary>
    /// Abstract class that provides blueprint for handling wire protocol in MUnit.
    /// </summary>
    public abstract class MUnitWire : IDisposable
    {
        /// <summary>
        /// Session ID for Request/Reply messages.
        /// </summary>
        protected int _sessionID;

        /// <summary>
        /// Messages sent to remote host. Used for matching ID to replied messages.
        /// </summary>
        protected ITestEngine _testEngine;

        /// <summary>
        /// Logger used by the engine.
        /// </summary>
        protected IMUnitLogger _logger;

        /// <summary>
        /// Transporter used for IPC.
        /// </summary>
        private ITransporter _transporter;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="MUnitWire"/> class.
        /// </summary>
        /// <param name="testEngine"> Test engine supported by this transporter. </param>
        /// <param name="transporter"> Transporter used for IPC. </param>
        /// <param name="logger"> Logger used by the engine. </param>
        public MUnitWire(ITestEngine testEngine, ITransporter transporter, IMUnitLogger logger)
        {
            ThrowUtilities.NullArgument(testEngine, nameof(testEngine));
            ThrowUtilities.NullArgument(transporter, nameof(transporter));
            ThrowUtilities.NullArgument(logger, nameof(logger));

            _logger = logger;
            _transporter = transporter;
            _testEngine = testEngine;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="MUnitWire"/> class.
        /// </summary>
        ~MUnitWire()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets message queue from transporter.
        /// </summary>
        protected IConcurrentQueue<WireMessage> MessageQueue { get => _transporter.MessageQueue; }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Start the wire agent.
        /// </summary>
        public virtual void Start()
        {
            _transporter.Start();
        }

        /// <summary>
        /// Dequeue and process message if there is any available.
        /// </summary>
        protected virtual void ProcessMessageIfAny()
        {
            if (_transporter.MessageQueue.TryDequeue(out WireMessage wireMessage))
            {
                this.ProcessMessagePacket(wireMessage);
            }
        }

        /// <summary>
        /// Dispose managed resources. If <paramref name="disposing"/> is true, native resources as well.
        /// </summary>
        /// <param name="disposing"> True if <see cref="MUnitWire.Dispose"/> is called by user. </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // No op.
                }

                _transporter.Dispose();
                _disposed = true;
            }
        }

        /// <summary>
        /// Send <paramref name="wireMessage"/> to remote host.
        /// </summary>
        /// <param name="wireMessage"> Message to send. </param>
        /// <returns> Session ID for sent message. </returns>
        protected virtual int Send(WireMessage wireMessage)
        {
            this.Send(wireMessage, ++_sessionID);
            return _sessionID;
        }

        /// <summary>
        /// Send <paramref name="wireMessage"/> to remote host.
        /// </summary>
        /// <param name="wireMessage"> Message to send. </param>
        /// <param name="sessionID"> Session ID for <paramref name="wireMessage"/>. </param>
        protected virtual void Send(WireMessage wireMessage, int sessionID)
        {
            ThrowUtilities.NullArgument(wireMessage, nameof(wireMessage));

            wireMessage.SessionID = sessionID;
            wireMessage.ID = _transporter.Send(wireMessage);
            _logger.RecordMessage(MessageLevel.Trace, wireMessage.ToString());
        }

        /// <summary>
        /// Receive message transmissin.
        /// </summary>
        /// <returns> Received message from remote host. </returns>
        protected virtual WireMessage Receive()
        {
            try
            {
                return _transporter.Receive();
            }
            catch (Exception e)
            {
                _logger.RecordMessage(MessageLevel.Error, e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Process incoming messages.
        /// </summary>
        /// <param name="message"> Message to be processed. </param>
        /// <returns> Returned object from processing message. </returns>
        /// <exception cref="ArgumentNullException"> Throws if <paramref name="message"/> is null. </exception>
        protected virtual object ProcessMessagePacket(WireMessage message)
        {
            ThrowUtilities.NullArgument(message, nameof(message));

            _logger.RecordMessage(MessageLevel.Trace, message.ToString());
            return ProcessActionCommand(message);
        }

        /// <summary>
        /// Process command in <paramref name="message"/>.
        /// </summary>
        /// <param name="message"> Message to be processed. </param>
        /// <returns> Returned object from processing message. </returns>
        protected abstract object ProcessActionCommand(WireMessage message);
    }
}
