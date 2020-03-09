// <copyright file="MUnitTestExecutor.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using MUnit.Engine.Service;
using MUnit.Transport;
using MUnitTestAdapter.Resources;
using MUnitTestAdapter.Utilities;
using MUF = MUnit.Framework;

namespace MUnitTestAdapter
{
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

    /// <summary>
    /// <para>ITestExecutor implementation which is called by vs test framework or the IDE.</para>
    /// <para>See https://github.com/Microsoft/vstest-docs/blob/master/RFCs/0004-Adapter-Extensibility.md .</para>
    /// </summary>
    [ExtensionUri(MUnitTAConstants.ExecutorUri)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Dispose() is not called by caller.")]
    public class MUnitTestExecutor : ITestExecutor
    {
        private readonly IMUnitClient _client;
        private IFrameworkHandle _frameworkHandle;
        private SynchronizedCollection<Guid> _testEndTrackList;
        private SynchronizedCollection<Guid> _testResultTrackList;
        private AutoResetEvent _testResultReceivedEvent = new AutoResetEvent(false);

        /// <summary>
        /// Initializes a new instance of the <see cref="MUnitTestExecutor"/> class.
        /// </summary>
        public MUnitTestExecutor()
        {
            _client = TypeResolver.MUnitClient;
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

            ICollection<Guid> runningTests = tests.Select(t => t.Id).ToList();
            this.RunTestsInternal(runningTests, frameworkHandle);
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

            ICollection<Guid> runningTests = _client.DiscoverTests(sources).Select(t => t.TestID).ToList();
            this.RunTestsInternal(runningTests, frameworkHandle);
        }

        #endregion

        private void RunTestsInternal(ICollection<Guid> tests, IFrameworkHandle frameworkHandle)
        {
            _frameworkHandle = frameworkHandle;
            _testEndTrackList = new SynchronizedCollection<Guid>(tests);
            _testResultTrackList = new SynchronizedCollection<Guid>(tests);

            _client.RecordTestStartEvent += this.RecordTestStartEventHandler;
            _client.RecordTestEndEvent += this.RecordTestEndEventHandler;
            _client.RecordTestResultEvent += this.RecordTestResultEventHandler;

            _client.RunTests(tests, out int testRunID);
            _testResultReceivedEvent.WaitOne();
        }

        private void RecordTestResultEventHandler(MUF.TestResult result)
        {
            _frameworkHandle.RecordResult(AdpaterUtilites.ConvertToTestResult(result));
            this.UpdateTestResultTrackList(result);
        }

        private void RecordTestEndEventHandler(MUF.TestResult result)
        {
            TestOutcome outcome = AdpaterUtilites.ConvertToTestOutcome(result.Outcome);
            _frameworkHandle.RecordEnd(AdpaterUtilites.ConvertToTestCase(result), outcome);
            this.UpdateTestEndTrackList(result);
        }

        private void RecordTestStartEventHandler(MUF.TestResult result)
        {
            _frameworkHandle.RecordStart(AdpaterUtilites.ConvertToTestCase(result));
        }

        private void UpdateTestEndTrackList(MUF.TestResult result)
        {
            Guid guid = result.ExecutionId;
            _testEndTrackList.Remove(guid);
            this.SetTestResultsReceivedEvent();
        }

        private void UpdateTestResultTrackList(MUF.TestResult result)
        {
            Guid guid = result.ExecutionId;
            _testResultTrackList.Remove(guid);
            this.SetTestResultsReceivedEvent();
        }

        private void SetTestResultsReceivedEvent()
        {
            if (!_testEndTrackList.Any() && !_testResultTrackList.Any())
            {
                _client.RecordTestStartEvent -= this.RecordTestStartEventHandler;
                _client.RecordTestEndEvent -= this.RecordTestEndEventHandler;
                _client.RecordTestResultEvent -= this.RecordTestResultEventHandler;

                _testResultReceivedEvent.Set();
            }
        }
    }
}
