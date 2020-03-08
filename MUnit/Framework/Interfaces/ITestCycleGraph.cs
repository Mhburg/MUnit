// <copyright file="ITestCycleGraph.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;

namespace MUnit.Framework
{
    /// <summary>
    /// A data structure used to add and run tests.
    /// </summary>
    public interface ITestCycleGraph : IEnumerable<ITestCycle>
    {
        /// <summary>
        /// Gets the number of test cycles.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets a logger that handles information and error recording.
        /// </summary>
        IMUnitLogger Logger { get; }

        /// <summary>
        /// Gets a lookup table with which one can query test cycle with test ID.
        /// </summary>
        Dictionary<Guid, ITestMethodContext> TestContextLookup { get; }

        /// <summary>
        /// Retrieve ITestCycle.
        /// </summary>
        /// <param name="key">Key to query.</param>
        /// <returns>Requested ITestCycle.</returns>
        ITestCycle this[Guid key] { get; }

        /// <summary>
        /// Add new test cycle.
        /// </summary>
        /// <param name="testCycle">Test cycle to add.</param>
        /// <exception cref="Exception">An element with the same key already exists.</exception>
        void Add(ITestCycle testCycle);

        /// <summary>
        /// Try get test cycle based on the <paramref name="key"/> input.
        /// </summary>
        /// <param name="key"> The key used for query. </param>
        /// <param name="testCycle"> Test cycle that has <paramref name="key"/>.</param>
        /// <returns> Returns true if test cycle is found, otherwise false. </returns>
        bool TryGetValue(Guid key, out ITestCycle testCycle);

        /// <summary>
        /// Run tests in this graph.
        /// </summary>
        /// <param name="testRunID"> ID of this test run. </param>
        void Run(int testRunID);

        /// <summary>
        /// Cancel test run.
        /// </summary>
        void Cancel();
    }
}