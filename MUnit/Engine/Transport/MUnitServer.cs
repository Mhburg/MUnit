// <copyright file="MUnitServer.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Policy;
using System.Threading;
using MUnit.Engine;
using MUnit.Framework;
using MUnit.Resources;
using MUnit.Utilities;

#if UnitTest
using System.Diagnostics;
#endif

namespace MUnit.Transport
{
    /// <summary>
    /// MUnit abstract server class to process test run requests.
    /// </summary>
    public abstract class MUnitServer : MUnitWire
    {
        private volatile bool _isServerStarted;
        private Exception _serverException;

        /// <summary>
        /// Initializes a new instance of the <see cref="MUnitServer"/> class.
        /// </summary>
        /// <param name="testEngine"> Test engine supported by this server. </param>
        /// <param name="serverWorker"> Transporter used for IPC. </param>
        /// <exception cref="ArgumentNullException">Throws if any argument is null. </exception>
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Null chekc is performed in base class.")]
        public MUnitServer(ITestEngine testEngine, ITransporter serverWorker)
            : base(testEngine, serverWorker)
        {
            _logger.TestResultsEvent += TestResultEventHandlerAsync;
            _logger.TestStartEvent += this.TestStartEventHandler;
            _logger.TestEndEvent += this.TestEndEventHandler;
        }

        /// <summary>
        /// Start the server instance. It will process incoming message on the caller thread
        /// and start a background thread to handle incoming data transmission. Safe for re-entry.
        /// </summary>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Should not crash host.")]
        public override void Start()
        {
            this.ProcessMessageIfAny();
            if (!_isServerStarted)
            {
                _isServerStarted = true;
                ThreadPool.QueueUserWorkItem(
                    (state) =>
                    {
                        while (_isServerStarted)
                        {
                            try
                            {
                                base.Start();
                                if (this.MessageQueue.TryPeek(out WireMessage wireMessage1))
                                {
                                    if (wireMessage1.Command == CommandType.Cancel)
                                    {
                                        _testEngine.Cancel();
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                _isServerStarted = false;
                                _serverException = e;
                                _logger.RecordMessage(MessageLevel.Error, e.Message);
                            }
                        }
                    });
            }
            else if (_serverException != null)
            {
                throw new Exception(Errors.UTE_FatalServerError, _serverException);
            }
        }

        /// <summary>
        /// Send test results to remote host.
        /// </summary>
        /// <param name="testResults"> Test result to be sent. </param>
        /// <exception cref="ArgumentNullException"> Throws if <paramref name="testResults"/> is null. </exception>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Should not crash host.")]
        public virtual void SendTestResult(IEnumerable<TestResult> testResults)
        {
            ThrowUtilities.NullArgument(testResults, nameof(testResults));

            try
            {
                this.Send(
                    new WireMessage(
                        WireMessageTypes.Reply, CommandType.TakeResults, testResults),
                    testResults.First().TestRunID);
            }
            catch (Exception e)
            {
                _logger.RecordMessage(MessageLevel.Error, e.ToString());
            }
        }

        /// <summary>
        /// Send test stats to remote host.
        /// </summary>
        /// <param name="result"> result to send. </param>
        /// <param name="commandType"> <see cref="CommandType"/> of <see cref="WireMessage"/>. </param>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Should not crash host.")]
        public virtual void SendTestStats(TestResult result, CommandType commandType)
        {
            ThrowUtilities.NullArgument(result, nameof(result));
            Trace.Assert(commandType == CommandType.RecordTestStart || commandType == CommandType.RecordTestEnd, "Input command type is not supported.");

            try
            {
                this.Send(
                    new WireMessage(
                        WireMessageTypes.Telemetry, commandType, result),
                    result.TestRunID);
            }
            catch (Exception e)
            {
                _logger.RecordMessage(MessageLevel.Error, e.ToString());
            }
        }

        /// <summary>
        /// Disposes managed resources and shuts down server. If <paramref name="disposing"/> is true, native resources as well.
        /// </summary>
        /// <param name="disposing"> True if <see cref="MUnitWire.Dispose"/> is called by user. </param>
        protected override void Dispose(bool disposing)
        {
            _isServerStarted = false;
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Required to handle all exception except SocketException.")]
        protected override object ProcessActionCommand(WireMessage message)
        {
            ThrowUtilities.NullArgument(message, nameof(message));

            try
            {
                switch (message.Command)
                {
                    case CommandType.CallFunction:
                        if (message.CallingType != null)
                        {
                            object instance = message.CtorParams == null
                            ? Activator.CreateInstance(message.CallingType)
                            : Activator.CreateInstance(message.CallingType, message.CtorParams);
                            ((MethodInfo)message.Entity).Invoke(instance, message.Parameters);
                        }
                        else
                        {
                            ((MethodInfo)message.Entity).Invoke(null, message.Parameters);
                        }

                        break;

                    case CommandType.DiscoverTests:
                        this.DiscoverTests(message);
                        break;

                    case CommandType.RunTests:
                        switch (message.Entity)
                        {
                            case IEnumerable<string> sources:
                                _testEngine.RunTests(sources, message.SessionID, _logger);
                                break;
                            case IEnumerable<Guid> guids:
                                _testEngine.RunTests(guids, message.SessionID, _logger);
                                break;
                            default:
                                throw new ArgumentException(
                                    string.Format(
                                        CultureInfo.CurrentCulture,
                                        Errors.UTE_UnSupportedTestSources,
                                        message.Entity.GetType()));
                        }

                        break;

                    case CommandType.Cancel:
                        _testEngine.Cancel();
                        break;

                    case CommandType.CheckAssemblyHash:
                        this.Send(
                            new WireMessage(
                                WireMessageTypes.Reply,
                                CommandType.CheckAssemblyHash,
                                new Hash(Assembly.Load((string)message.Entity)).SHA1),
                            message.SessionID);
                        break;

                    default:
                        _logger.RecordMessage(MessageLevel.Warning, string.Format(
                            CultureInfo.CurrentCulture,
                            Errors.UTE_UnsupportedCommandType,
                            message.Command.ToString()));
                        break;
                }

                return null;
            }
            catch (SocketException)
            {
                throw;
            }
            catch (Exception e)
            {
                _logger.RecordMessage(MessageLevel.Error, e.ToString());
                return null;
            }
        }

        /// <summary>
        /// Send test results asynchronously using <see cref="ThreadPool.QueueUserWorkItem(WaitCallback)"/>.
        /// </summary>
        /// <param name="sender"> Event handler which calls this method. </param>
        /// <param name="testResults"> Test result to be sent. </param>
        protected virtual void TestResultEventHandlerAsync(object sender, IEnumerable<TestResult> testResults)
        {
            ThreadPool.QueueUserWorkItem((result) => this.SendTestResult(result as IEnumerable<TestResult>), testResults);
        }

        protected virtual void TestStartEventHandler(object sender, TestResult result)
        {
            this.SendTestStats(result, CommandType.RecordTestStart);
        }

        protected virtual void TestEndEventHandler(object sender, TestResult result)
        {
            this.SendTestStats(result, CommandType.RecordTestEnd);
        }

        private void DiscoverTests(WireMessage message)
        {
            ITestCycleGraph tests = _testEngine.DiscoverTests(message.Entity as IEnumerable<string>);
            this.Send(new WireMessage(WireMessageTypes.Telemetry, CommandType.DiscoverTests, tests.TestContextLookup.Values), message.SessionID);
        }
#if UnitTest
        /// <summary>
        /// Produce simple trace output.
        /// </summary>
        /// <param name="str"> string to print. </param>
        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:Elements should be ordered by access", Justification = "Method is unit test only.")]
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Needed for test as class member.")]
        public void SimpleTraceOutput(string str)
        {
            Trace.WriteLine("Trace output from MUnitServere. Printing: " + str);
        }

        /// <summary>
        /// Static version of <see cref="SimpleTraceOutput"/>.
        /// </summary>
        /// <param name="str"> string to print. </param>
        [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1204:Static elements should appear before instance elements", Justification = "Method is unit test only.")]
        public static void StaticSimpleTraceOutput(string str)
        {
            Trace.WriteLine("Trace output from MUnitServere. Printing: " + str);
        }
#endif
    }
}
