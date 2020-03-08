// <copyright file="IMUnitLogger.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;

namespace MUnit.Framework
{
    /// <summary>
    /// Message level that indicates severity of the message.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1602:Enumeration items should be documented",
        Justification = "Self explanatory.")]
    public enum MessageLevel
    {
        Information = 0,
        Warning = 1,
        Error = 2,
        Trace = 3,
        Debug = 4,
    }

    /// <summary>
    /// Ways to sort log.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1602:Enumeration items should be documented",
        Justification = "Self explanatory.")]
    public enum LogOrder
    {
        ByDateTime = 0,
        ByLevel = 1,
    }

    /// <summary>
    /// Logger to record information.
    /// </summary>
    public interface IMUnitLogger
    {
        /// <summary>
        /// Raised when any message is received.
        /// </summary>
        event EventHandler<MessageContext> MessageEvent;

        /// <summary>
        /// Raised when test result is ready to report.
        /// </summary>
        event Action<object, IEnumerable<TestResult>> TestResultsEvent;

        /// <summary>
        /// Raised when a test has started.
        /// </summary>
        event Action<object, TestResult> TestStartEvent;

        /// <summary>
        /// Raised when a test ends.
        /// </summary>
        event Action<object, TestResult> TestEndEvent;

        /// <summary>
        /// Gets or sets the logger level at or below which messages will be logged.
        /// </summary>
        MessageLevel LoggerLevel { get; set; }

        /// <summary>
        /// Record messages.
        /// </summary>
        /// <param name="level">Severity of the message.</param>
        /// <param name="message">Messge to be logged.</param>
        void RecordMessage(MessageLevel level, string message);

        /// <summary>
        /// Report test results.
        /// </summary>
        /// <param name="results"> Results to report. </param>
        void ReportTestResults(IEnumerable<TestResult> results);

        /// <summary>
        /// Report a test has started.
        /// </summary>
        /// <param name="testResult"> Test to report. </param>
        void ReportTestStarted(TestResult testResult);

        /// <summary>
        /// Report a test ends.
        /// </summary>
        /// <param name="testResult"> Test to report. </param>
        void ReportTestEnded(TestResult testResult);

        /// <summary>
        /// Retrieve recorded messages in the order of <paramref name="logOrder"/>.
        /// </summary>
        /// <param name="logOrder">How to sort log.</param>
        /// <returns>A string of recored messages.</returns>
        string OutputLog(LogOrder logOrder);

        /// <summary>
        /// Return the message logged. Null, if log is empty.
        /// </summary>
        /// <returns> Returns <see cref="MessageContext"/> that contains a timestamp, a message and more. </returns>
        MessageContext LastLog();

        /// <summary>
        /// Writes log to file.
        /// </summary>
        /// <param name="path">
        ///     <para> Writes log to a newly created file. If file exists, overwrites it. </para>
        ///     <para> If path is null, a "TestLog.txt" will be created at the location of the executing assembly. </para>
        /// </param>
        void WriteToFile(string path = null);
    }
}