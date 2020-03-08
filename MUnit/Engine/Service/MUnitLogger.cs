// <copyright file="MUnitLogger.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using MUnit.Engine;
using MUnit.Engine.Resources;
using MUnit.Framework;

namespace MUnit
{
    /// <summary>
    /// Default logger used by the test engine.
    /// </summary>
    public class MUnitLogger : IMUnitLogger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MUnitLogger"/> class.
        /// </summary>
        public MUnitLogger()
            : this(MessageLevel.Error)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MUnitLogger"/> class.
        /// </summary>
        /// <param name="messageLevel"> Set message at or below <paramref name="messageLevel"/> the logger should record. </param>
        public MUnitLogger(MessageLevel messageLevel)
        {
            this.LoggerLevel = messageLevel;
            StringBuilder message = new StringBuilder("Logger will record messages of level: ");

            string[] levels = ((MessageLevel[])Enum.GetValues(typeof(MessageLevel)))
                                    .Where(level => (!(level > this.LoggerLevel)))
                                    .Select(level => level.ToString())
                                    .ToArray();

            message.Append(string.Join(", ", levels));
            MessageContext messageContext = new MessageContext(
                message.ToString(), MessageLevel.Information, DateTime.Now);

            this.Log.Add(MessageLevel.Information, new List<MessageContext>() { messageContext });
        }

        /// <inheritdoc/>
        public event EventHandler<MessageContext> MessageEvent;

        /// <inheritdoc/>
        public event Action<object, IEnumerable<TestResult>> TestResultsEvent;

        /// <inheritdoc/>
        public event Action<object, TestResult> TestStartEvent;

        /// <inheritdoc/>
        public event Action<object, TestResult> TestEndEvent;

        /// <summary>
        /// Gets a dictionary instance of log, where the key is MessageLevel, and the value is a list of MessageContext.
        /// </summary>
        public IDictionary<MessageLevel, List<MessageContext>> Log { get; } = PlatformService.GetServiceManager().GetConcurrentLog();

        #region IMUnitLogger Implementation

        /// <inheritdoc/>
        public MessageLevel LoggerLevel { get; set; }

        /// <inheritdoc/>
        public string OutputLog(LogOrder logOrder)
        {
            return FormattedLog(logOrder);
        }

        /// <inheritdoc/>
        public void WriteToFile(string path = null)
        {
            if (path == null)
                path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestLog.txt");

            File.WriteAllText(path, this.OutputLog(LogOrder.ByDateTime));
        }

        /// <summary>
        /// Record messages. Captures <see cref="Environment.StackTrace"/>, if <see cref="MessageLevel.Error"/> is used.
        /// </summary>
        /// <param name="level">Severity of the message.</param>
        /// <param name="message">Messge to be logged.</param>
        public void RecordMessage(MessageLevel level, string message)
        {
            if (!(level > LoggerLevel))
            {
                MessageContext messageContext;
                if (level == MessageLevel.Error)
                {
                    messageContext = new MessageContext(message, level, DateTime.Now, Environment.StackTrace);
                }
                else
                {
                    messageContext = new MessageContext(message, level, DateTime.Now);
                }

                if (this.Log.TryGetValue(level, out List<MessageContext> contexts))
                {
                    contexts.Add(messageContext);
                }
                else
                {
                    this.Log.Add(level, new List<MessageContext>() { messageContext });
                }

                this.MessageEvent?.Invoke(this, messageContext);
            }
        }

        /// <inheritdoc/>
        public void ReportTestResults(IEnumerable<TestResult> results)
        {
            this.TestResultsEvent?.Invoke(this, results);
        }

        /// <inheritdoc/>
        public void ReportTestStarted(TestResult testResult)
        {
            this.TestStartEvent?.Invoke(this, testResult);
        }

        /// <inheritdoc/>
        public void ReportTestEnded(TestResult testResult)
        {
            this.TestEndEvent?.Invoke(this, testResult);
        }

        /// <inheritdoc/>
        public MessageContext LastLog()
        {
            if (this.Log.Any())
            {
                return this.Log.Values.SelectMany(m => m).OrderByDescending(m => m.Timestamp).First();
            }

            return null;
        }

        #endregion

        private string FormattedLog(LogOrder logOrder)
        {
            StringBuilder builder = new StringBuilder();
            foreach (MessageContext context in SortMessages(logOrder))
            {
                builder.AppendFormat(Strings.FormattedLogMessage, context.Timestamp.ToLongTimeString(), context.Level, context.Message);
                builder.AppendLine();
            }

            return builder.ToString();
        }

        private IOrderedEnumerable<MessageContext> SortMessages(LogOrder logOrder)
        {
            switch (logOrder)
            {
                case LogOrder.ByLevel:
                    return this.Log.SelectMany(p => p.Value).OrderByDescending(c => c.Level);

                default:
                    return this.Log.SelectMany(p => p.Value).OrderBy(c => c.Timestamp);
            }
        }
    }
}
