// <copyright file="ReportUtilities.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace MUnitTestAdapter.Utilities
{
    /// <summary>
    /// Utility class that aids in reporting.
    /// </summary>
    public static class ReportUtilities
    {
        /// <summary>
        ///     Report error to tests in batch.
        /// </summary>
        /// <param name="tests"> Failed tests. </param>
        /// <param name="frameworkHandle"> Framework handle for recording results. </param>
        /// <param name="errorMessage"> Message to report. </param>
        public static void BatchErrorReport(IEnumerable<TestCase> tests, IFrameworkHandle frameworkHandle, string errorMessage)
        {
            Debug.Assert(tests != null, "tests should not be null");
            Debug.Assert(frameworkHandle != null, "frameworkHandle should not be null");
            ValidateArg.NotNullOrEmpty(errorMessage, nameof(errorMessage));

            foreach (TestCase test in tests)
            {
                frameworkHandle.RecordResult(new TestResult(test)
                {
                    Outcome = TestOutcome.Failed,
                    ErrorMessage = errorMessage,
                });
            }
        }
    }
}
