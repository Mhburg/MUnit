// <copyright file="AdpaterUtilites.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using MUnitTestAdapter.Resources;
using UTF = MUnit.Framework;

namespace MUnitTestAdapter.Utilities
{
    public static class AdpaterUtilites
    {
        /// <summary>
        /// Convert <see cref="UTF.TestResult"/> to <see cref="TestCase"/>.
        /// </summary>
        /// <param name="result"> Result to convert. </param>
        /// <returns> <see cref="TestCase"/> constructed with <paramref name="result"/>. </returns>
        public static TestCase ConvertToTestCase(UTF.TestResult result)
        {
            ValidateArg.NotNull(result, nameof(result));

            return new TestCase(result.FullyQualifiedName, new Uri(MUnitTAConstants.ExecutorUri), result.Source);
        }

        public static TestCase ConvertToTestCase(UTF.ITestMethodContext context)
        {
            ValidateArg.NotNull(context, nameof(context));

            return new TestCase(context.FullyQualifiedName, new Uri(MUnitTAConstants.ExecutorUri), context.Source);
        }

        /// <summary>
        /// Convert MUnit test result to VS test framework test result.
        /// </summary>
        /// <param name="result"> Result to convert. </param>
        /// <returns> Converted test result for VS test framework. </returns>
        public static TestResult ConvertToTestResult(UTF.TestResult result)
        {
            ValidateArg.NotNull(result, nameof(result));

            return new TestResult(AdpaterUtilites.ConvertToTestCase(result))
            {
                StartTime = result.StartTime,
                EndTime = result.EndTime,
                DisplayName = result.DisplayName,
                Duration = result.Duration,
                ErrorMessage = result.LogError,
                ErrorStackTrace = result.DebugTrace,
                Outcome = ConvertToTestOutcome(result.Outcome),
            };
        }

        public static TestOutcome ConvertToTestOutcome(UTF.UnitTestOutcome mOutcome)
        {
            TestOutcome outcome;
            switch (mOutcome)
            {
                case UTF.UnitTestOutcome.Passed:
                    outcome = TestOutcome.Passed;
                    break;
                case UTF.UnitTestOutcome.Failed:
                    outcome = TestOutcome.Failed;
                    break;
                case UTF.UnitTestOutcome.Error:
                    outcome = TestOutcome.Failed;
                    break;
                case UTF.UnitTestOutcome.Aborted:
                    outcome = TestOutcome.Skipped;
                    break;
                case UTF.UnitTestOutcome.Timeout:
                    outcome = TestOutcome.Failed;
                    break;
                default:
                    outcome = TestOutcome.None;
                    break;
            }

            return outcome;
        }
    }
}
