// <copyright file="TestResult.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Copyright (c) 2020 Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using MUnit.Framework.Base;

namespace MUnit.Framework
{
    /// <summary>
    /// TestResult object to be returned to adapter.
    /// </summary>
    [Serializable]
    public class TestResult : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestResult"/> class.
        /// </summary>
        public TestResult()
        {
            this.DatarowIndex = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestResult"/> class.
        /// </summary>
        /// <param name="context"> Context in which test is executed. </param>
        internal TestResult(TestMethodContext context)
            : this()
        {
            this.Context = context;
        }

        /// <summary>
        /// Gets or sets source of test method that produces this result.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the display name of the result. Useful when returning multiple results.
        /// If null then Method name is used as DisplayName.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the fully qualified name for test method.
        /// </summary>
        public string FullyQualifiedName { get; set; }

        /// <summary>
        /// Gets or sets the outcome of the test execution.
        /// </summary>
        public UnitTestOutcome Outcome { get; set; }

        /// <summary>
        /// Gets or sets the exception thrown when test is failed.
        /// </summary>
        public Exception TestFailureException { get; set; }

        /// <summary>
        /// Gets or sets the output of the message logged by test code.
        /// </summary>
        public string LogOutput { get; set; }

        /// <summary>
        /// Gets or sets the output of the message logged by test code.
        /// </summary>
        public string LogError { get; set; }

        /// <summary>
        /// Gets or sets the debug traces by test code.
        /// </summary>
        public string DebugTrace { get; set; }

        /// <summary>
        /// Gets or sets the debug traces by test code.
        /// </summary>
        public string TestContextMessages { get; set; }

        /// <summary>
        /// Gets or sets the ID of test run.
        /// </summary>
        public int TestRunID { get; set; }

        /// <summary>
        /// Gets or sets the execution id of the result.
        /// </summary>
        public Guid DataEntryExecId { get; set; }

        /// <summary>
        /// Gets or sets the parent execution id of the result.
        /// </summary>
        public Guid ExecutionId { get; set; }

        /// <summary>
        /// Gets or sets the inner results count of the result.
        /// </summary>
        public int InnerResultsCount { get; set; }

        /// <summary>
        /// Gets or sets the duration of test execution.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets or sets the data row index in data source. Set only for results of individual
        /// run of data row of a data driven test.
        /// </summary>
        public int DatarowIndex { get; set; }

        /// <summary>
        /// Gets or sets the return value of the test method. (Currently null always).
        /// </summary>
        public object ReturnValue { get; set; }

        /// <summary>
        /// Gets the result files attached by the test.
        /// </summary>
        public IList<string> ResultFiles { get; } = new List<string>();

        /// <summary>
        /// Gets or sets time when test starts.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets time when test ends.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets context in which test is executed.
        /// </summary>
        [XmlIgnore]
        [field: NonSerialized]
        internal TestMethodContext Context { get; set; }
    }
}