// <copyright file="TestMethodContext.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;
using MUnit.Framework.FrameworkService;
using MUnit.Utilities;

namespace MUnit.Framework.Base
{
    /// <summary>
    /// Context in which a test method is run.
    /// </summary>
    [DataContract]
    internal class TestMethodContext : ITestMethodContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestMethodContext"/> class.
        /// </summary>
        /// <param name="source"> Source where the test is discovered. </param>
        /// <param name="parentCycle"> Test cycle that owns this context. </param>
        /// <param name="method"> Test method to run. </param>
        /// <param name="parentClass"> The class in which test method is declared. </param>
        /// <param name="logger"> Logs information. </param>
        internal TestMethodContext(string source, ITestCycle parentCycle, MethodInfo method, Type parentClass, IMUnitLogger logger)
        {
            Debug.Assert(method != null, "Test method should not be null.");
            Debug.Assert(parentClass != null, "Parent class should not be null.");

            this.Source = source;
            this.MethodInfo = method;
            this.DeclaringType = parentClass;
            this.ParentCycle = parentCycle;
            this.Logger = logger;
            this.FullyQualifiedName = FrameworkSerive.RelfectionWoker.GetMethodFullName(method);
            this.TestID = HashUtilities.GuidForTestCycleID(source, this.FullyQualifiedName);
        }

        /// <inheritdoc/>
        [DataMember]
        public Guid TestID { get; protected set; }

        /// <inheritdoc/>
        [DataMember]
        public int TestRunID { get; set; }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public IExecutor Executor { get; set; }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public bool IsActive { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public string FullyQualifiedName { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public string Source { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public MethodInfo MethodInfo { get; set; }

        /// <inheritdoc/>
        [DataMember]
        public Type DeclaringType { get; set; }

        [IgnoreDataMember]
        internal ITestCycle ParentCycle { get; set; }

        [IgnoreDataMember]
        internal IDataSource DataSource { get; set; }

        [IgnoreDataMember]
        internal IMUnitLogger Logger { get; set; }

        /// <inheritdoc/>
        [IgnoreDataMember]
        public object Instance { get; set; }

        /// <inheritdoc/>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Handling all exception is required.")]
        public virtual IEnumerable<TestResult> Invoke()
        {
            if (this.DataSource == null)
            {
                return new[] { this.InvokeOnce(null) };
            }
            else
            {
                List<TestResult> testResults = new List<TestResult>();

                foreach (object[] arguments in this.DataSource.GetData())
                {
                    testResults.Add(this.InvokeOnce(arguments));
                }

                return testResults;
            }
        }

        /// <inheritdoc/>
        public virtual void SetActive(ITestCycleGraph testCycles)
        {
            this.IsActive = true;
            this.ParentCycle.SetTestCycleActive(testCycles);
        }

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Handling all exception is required.")]
        private TestResult InvokeOnce(object[] arguments)
        {
            TestResult result = new TestResult(this)
            {
                Source = this.Source,
                FullyQualifiedName = this.FullyQualifiedName,
                DisplayName = this.MethodInfo.Name,
                ExecutionId = this.TestID,
                TestRunID = this.TestRunID,
            };

            try
            {
                result.StartTime = DateTime.Now;
                this.Logger.ReportTestStarted(result);

                this.MethodInfo.Invoke(this.Instance, arguments);

                result.EndTime = DateTime.Now;
                this.Logger.ReportTestEnded(result);
            }
            catch (Exception e)
            {
                if (e.InnerException is AssertException assertException)
                {
                    result.Outcome = UnitTestOutcome.Failed;
                    result.LogOutput = assertException.Message;
                    return result;
                }

                result.Outcome = UnitTestOutcome.Error;
                result.LogError = e.ToString();

                this.Logger.RecordMessage(MessageLevel.Error, e.ToString());
                return result;
            }

            result.Outcome = UnitTestOutcome.Passed;
            return result;
        }
    }
}