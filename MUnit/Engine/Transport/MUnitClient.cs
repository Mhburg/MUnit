// <copyright file="MUnitClient.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Threading;
using MUnit.Engine;
using MUnit.Framework;
using MUnit.Resources;
using MUnit.Utilities;

namespace MUnit.Transport
{
    /// <summary>
    /// Sends tests to <see cref="MUnitServer"/> and retrieves results.
    /// </summary>
    public abstract class MUnitClient : MUnitWire, IMUnitClient
    {
        private volatile bool _isReceiving = false;
        private EventLockTuple _discoverTestsLocks = new EventLockTuple();
        private EventLockTuple _checkAssemblyHashLocks = new EventLockTuple();

        /// <summary>
        /// Initializes a new instance of the <see cref="MUnitClient"/> class.
        /// </summary>
        /// <param name="testEngine"> Test engine supported by this transporter. </param>
        /// <param name="transporter"> Transporter used for IPC. </param>
        /// <param name="logger"> Logger used by the engine. </param>
        public MUnitClient(ITestEngine testEngine, ITransporter transporter, IMUnitLogger logger)
            : base(testEngine, transporter, logger)
        {
        }

        /// <summary>
        /// Raised when a test starts.
        /// </summary>
        protected event Action<TestResult> RecordTestStartEvent;

        /// <summary>
        /// Raised when a test is ended.
        /// </summary>
        protected event Action<TestResult> RecordTestEndEvent;

        /// <summary>
        /// Rasied when a test resutl is reported.
        /// </summary>
        protected event Action<TestResult> RecordTestResultEvent;

        /// <summary>
        /// Raised when server sends back discovered tests.
        /// </summary>
        protected event Action<int, ICollection<ITestMethodContext>> DiscoverTestsEvent;

        /// <summary>
        /// Check assmbly hash with server.
        /// </summary>
        protected event Action<int, byte[]> CheckAssemblyHashEvent;

        /// <summary>
        /// Start a background thread to receive and process incoming messages.
        /// </summary>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Exceptions in worker thread can't propagate.")]
        public override void Start()
        {
            base.Start();

            _isReceiving = true;
            ThreadPool.UnsafeQueueUserWorkItem(
                (object state) =>
                {
                    try
                    {
                        while (_isReceiving)
                        {
                            this.ProcessMessagePacket(this.Receive());
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.RecordMessage(MessageLevel.Error, e.ToString());
                    }
                }, null);
        }

        /// <inheritdoc/>
        public virtual ICollection<ITestMethodContext> DiscoverTests(IEnumerable<string> sources)
        {
            ThrowUtilities.NullArgument(sources, nameof(sources));

            int sessionID = this.Send(new WireMessage(WireMessageTypes.Request, CommandType.DiscoverTests, sources));
            void DiscoverTestsEventHandler(int id, ICollection<ITestMethodContext> tests)
            {
                if (id == sessionID)
                {
                    _discoverTestsLocks.HandlerLock.State = tests;
                    _discoverTestsLocks.EventLock.Set();
                }
            }

            this.DiscoverTestsEvent += DiscoverTestsEventHandler;
            _discoverTestsLocks.HandlerLock.Set();
            _discoverTestsLocks.EventLock.WaitOne();
            this.DiscoverTestsEvent -= DiscoverTestsEventHandler;

            return (ICollection<ITestMethodContext>)_discoverTestsLocks.HandlerLock.State;
        }

        /// <inheritdoc/>
        public bool CheckAssemblyHash(string source, out byte[] localHash, out byte[] remoteHash)
        {
            try
            {
                int sessionID = this.Send(new WireMessage(WireMessageTypes.Request, CommandType.CheckAssemblyHash, source));
                void CheckAssemblyHashEventHandler(int id, byte[] obj)
                {
                    if (sessionID == id)
                    {
                        _checkAssemblyHashLocks.HandlerLock.State = obj;
                        _checkAssemblyHashLocks.EventLock.Set();
                    }
                }

                this.CheckAssemblyHashEvent += CheckAssemblyHashEventHandler;
                _checkAssemblyHashLocks.HandlerLock.Set();
                _checkAssemblyHashLocks.EventLock.WaitOne();
                this.CheckAssemblyHashEvent -= CheckAssemblyHashEventHandler;

                remoteHash = _checkAssemblyHashLocks.HandlerLock.State as byte[];
                if (remoteHash == null)
                {
                    throw new NullReferenceException(Errors.UTT_RemoteHahsNull);
                }

                bool isMatched = false;
                localHash = new Hash(Assembly.Load(source)).SHA1;
                if (localHash.Length == remoteHash.Length)
                {
                    for (int i = 0; i < localHash.Length; i++)
                    {
                        if (localHash[i] != remoteHash[i])
                            break;
                    }

                    isMatched = true;
                }

                if (!isMatched)
                {
                    _logger.RecordMessage(
                        MessageLevel.Warning,
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Errors.UTT_AssemblyHashNotMatch,
                            localHash,
                            remoteHash));
                }

                return isMatched;
            }
            catch (Exception e)
            {
                _logger.RecordMessage(MessageLevel.Error, e.ToString());
                throw;
            }
        }

        /// <inheritdoc/>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Exception is re-thrown.")]
        public virtual void RunTests(IEnumerable<Guid> guids, out int testRunID)
        {
            try
            {
                testRunID = this.Send(new WireMessage(
                                WireMessageTypes.Request, CommandType.RunTests, guids));
            }
            catch (Exception e)
            {
                _logger.RecordMessage(MessageLevel.Error, e.ToString());
                throw;
            }
        }

        /// <inheritdoc/>
        public virtual ICollection<ITestMethodContext> RunTests(IEnumerable<string> sources, out int testRunID)
        {
            ThrowUtilities.NullArgument(sources, nameof(sources));

            try
            {
                testRunID = this.Send(new WireMessage(
                                WireMessageTypes.Request, CommandType.RunTests, sources));
                return this.DiscoverTests(sources);
            }
            catch (Exception e)
            {
                _logger.RecordMessage(MessageLevel.Error, e.ToString());
                throw;
            }
        }

        /// <inheritdoc/>
        public int RemoteProcedureCall(MethodInfo method, Type type, object[] parameters, object[] ctorParams)
        {
            return this.Send(new WireMessage(
                                        WireMessageTypes.Request,
                                        CommandType.CallFunction,
                                        method,
                                        type,
                                        parameters,
                                        ctorParams));
        }

        /// <inheritdoc/>
        public virtual void CancelTestRun()
        {
            this.Send(new WireMessage(WireMessageTypes.Request, CommandType.Cancel, null, null));
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            _isReceiving = false;
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override object ProcessActionCommand(WireMessage message)
        {
            ThrowUtilities.NullArgument(message, nameof(message));

            switch (message.Type)
            {
                case WireMessageTypes.Telemetry:
                    switch (message.Command)
                    {
                        case CommandType.RecordTestStart:
                            this.RecordTestStartEvent?.Invoke(message.Entity as TestResult);
                            break;

                        case CommandType.RecordTestEnd:
                            this.RecordTestEndEvent?.Invoke(message.Entity as TestResult);
                            break;

                        case CommandType.DiscoverTests:
                            _discoverTestsLocks.HandlerLock.WaitOne();
                            this.DiscoverTestsEvent?.Invoke(message.SessionID, message.Entity as ICollection<ITestMethodContext>);
                            break;

                        default:
                            _logger.RecordMessage(MessageLevel.Warning, string.Format(
                                CultureInfo.CurrentCulture,
                                Errors.UTE_UnsupportedCommandType,
                                message.Command.ToString()));
                            break;
                    }

                    break;

                default:
                    switch (message.Command)
                    {
                        case CommandType.TakeResults:
                            this.RecordTestResultEvent?.Invoke(message.Entity as TestResult);
                            break;

                        case CommandType.CheckAssemblyHash:
                            this.CheckAssemblyHashEvent?.Invoke(message.SessionID, message.Entity as byte[]);
                            break;

                        default:
                            _logger.RecordMessage(MessageLevel.Warning, string.Format(
                                CultureInfo.CurrentCulture,
                                Errors.UTE_UnsupportedCommandType,
                                message.Command.ToString()));
                            break;
                    }

                    break;
            }

            return null;
        }
    }
}
