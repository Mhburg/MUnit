// <copyright file="ITestEngine.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using MUnit.Framework;
using MUnit.Transport;

namespace MUnit.Engine
{
    /// <summary>
    /// Interface of an AtlasMode test engine.
    /// </summary>
    public interface ITestEngine
    {
        /// <summary>
        /// Gets or sets settings for the test engine.
        /// </summary>
        ITestEngineSetting Settings { get; set; }

        /// <summary>
        /// Gets or sets a logger to record information.
        /// </summary>
        IMUnitLogger Logger { get; set; }

        /// <summary>
        /// Discover tests from source.
        /// </summary>
        /// <param name="sources">Full path or file name of the sources.</param>
        /// <returns> Collection of discovered tests. </returns>
        /// <exception cref="ArgumentNullException"> Throws when <paramref name="sources"/> is null. </exception>
        TestCycleCollection DiscoverTests(IEnumerable<string> sources);

        /// <summary>
        /// Run selected tests.
        /// </summary>
        /// <param name="guids"> Guids of tests that are requested to run. </param>
        /// <param name="testrunID"> ID for test run. </param>
        /// <param name="logger">To record information.</param>
        void RunTests(IEnumerable<Guid> guids, int testrunID, IMUnitLogger logger);

        /// <summary>
        /// Run tests from sources.
        /// </summary>
        /// <param name="sources"> Sources used for discovering tests. </param>
        /// <param name="testrunID"> ID for test run. </param>
        /// <param name="logger"> Logs informatin. </param>
        void RunTests(IEnumerable<string> sources, int testrunID, IMUnitLogger logger);

        /// <summary>
        /// Cancel test run.
        /// </summary>
        void Cancel();
    }
}
