// <copyright file="MUnitTestExecutor.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using MUnit.Transport;
using MUnitTestAdapter.Resources;
using MUnitTestAdapter.Utilities;

namespace MUnitTestAdapter
{
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

    /// <summary>
    /// <para>ITestExecutor implementation which is called by vs test framework or the IDE.</para>
    /// <para>See https://github.com/Microsoft/vstest-docs/blob/master/RFCs/0004-Adapter-Extensibility.md .</para>
    /// </summary>
    [ExtensionUri(MUnitTAConstants.ExecutorUri)]
    public class MUnitTestExecutor : ITestExecutor
    {
        private readonly MUnitClient _client;
        private IFrameworkHandle _frameworkHandle;
        private ICollection<Guid> _runningTests;

        /// <summary>
        /// Initializes a new instance of the <see cref="MUnitTestExecutor"/> class.
        /// </summary>
        public MUnitTestExecutor()
        {
            this._client = (IMUnitClient)Activator.CreateInstance(
                                    Properties.Settings.Default.AssemblyName,
                                    Properties.Settings.Default.TestTransporterClass);
        }

        #region ITestExecutor Implementation

        /// <summary>
        /// Cancel the execution of the tests.
        /// </summary>
        public void Cancel()
        {
            this._client.CancelTestRun();
        }

        /// <summary>
        /// Runs only the tests specified by parameter 'tests'.
        /// </summary>
        /// <param name="tests">Tests to be run.</param>
        /// <param name="runContext">Context to use when executing the tests.</param>
        /// <param name="frameworkHandle">Handle to the framework to record results and to do framework operations.</param>
        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            ValidateArg.NotNull(tests, nameof(tests));
            ValidateArg.NotNull(frameworkHandle, nameof(frameworkHandle));

            _runningTests = tests.Select(t => t.Id).ToList();
            this.RunTestsInternal(_runningTests, frameworkHandle);
        }

        /// <summary>
        /// Runs 'all' the tests present in the specified 'sources'.
        /// </summary>
        /// <param name="sources">Path to test container files to look for tests in.</param>
        /// <param name="runContext">Context to use when executing the tests.</param>
        /// <param name="frameworkHandle">Handle to the framework to record results and to do framework operations.</param>
        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            ValidateArg.NotNullOrEmpty(sources, nameof(sources));
            ValidateArg.NotNull(runContext, nameof(runContext));
            ValidateArg.NotNull(frameworkHandle, nameof(frameworkHandle));

            _runningTests = _client.DiscoverTests(sources).Select(t => t.TestID).ToList();
            this.RunTestsInternal(_runningTests, frameworkHandle);
        }

        #endregion

        private void RunTestsInternal(ICollection<Guid> tests, IFrameworkHandle frameworkHandle)
        {
            _frameworkHandle = frameworkHandle;
            _client.RunTests(_runningTests, out int testRunID);

            _client.RecordTestStartEvent += this.RecordTestStartEventHandler;
            _client.RecordTestEndEvent += this.RecordTestEndEventHandler;
            _client.RecordTestResultEvent += this.RecordTestResultEvent;

            _client.StartReceiving();

            _client.RecordTestStartEvent -= this.RecordTestStartEventHandler;
            _client.RecordTestEndEvent -= this.RecordTestEndEventHandler;
            _client.RecordTestResultEvent -= this.RecordTestResultEvent;
        }

        private void RecordTestResultEvent(MUnit.Framework.TestResult result)
        {
            _frameworkHandle.RecordResult(AdpaterUtilites.ConvertToTestResult(result));
        }

        private void RecordTestEndEventHandler(MUnit.Framework.TestResult result)
        {
            TestOutcome outcome = AdpaterUtilites.ConvertToTestOutcome(result.Outcome);
            _frameworkHandle.RecordEnd(AdpaterUtilites.ConvertToTestCase(result), outcome);
            _runningTests.Remove(result.ExecutionId);
            if (!_runningTests.Any())
            {
                _client.StopReceiving();
            }
        }

        private void RecordTestStartEventHandler(MUnit.Framework.TestResult result)
        {
            _frameworkHandle.RecordStart(AdpaterUtilites.ConvertToTestCase(result));
        }
    }
}
