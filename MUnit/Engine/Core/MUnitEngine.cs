// <copyright file="MUnitEngine.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using MUnit;
using MUnit.Framework;
using MUnit.Transport;
using MUnit.Utilities;

namespace MUnit.Engine
{
    /// <summary>
    /// MUnit Test engine.
    /// </summary>
    public class MUnitEngine : ITestEngine
    {
        private readonly IServiceManager _services = PlatformService.GetServiceManager();
        private TestCycleCollection _testCycles;

        /// <summary>
        /// Initializes a new instance of the <see cref="MUnitEngine"/> class.
        /// </summary>
        /// <param name="logger"> Logs events reported by engine. </param>
        public MUnitEngine(IMUnitLogger logger)
        {
            Logger = logger;
        }

        /// <inheritdoc/>
        public ITestEngineSetting Settings { get; set; }

        /// <inheritdoc/>
        public IMUnitLogger Logger { get; set; }

        /// <inheritdoc/>
        public void Cancel()
        {
            _testCycles.Cancel();
        }

        /// <summary>
        /// Discovers tests available from the provided source.
        /// </summary>
        /// <param name="sources">Collection of test containers.</param>
        /// <returns> A collection of test cycles. </returns>
        public TestCycleCollection DiscoverTests(IEnumerable<string> sources)
        {
            ThrowUtilities.NullArgument(sources, nameof(sources));

            IList<SourcePackage> packages = _services.TestSource.GetTypes(sources, this.Logger);
            _testCycles = _services.TestBuilder.BuildTestCycles(packages, this.Logger);

            this.Logger.RecordMessage(MessageLevel.Information, "Total tests found: " + _testCycles.TestContextLookup.Count);
            return _testCycles;
        }

        /// <inheritdoc/>
        public void RunTests(IEnumerable<Guid> guids, int testRunID, IMUnitLogger logger)
        {
            ThrowUtilities.NullArgument(guids, nameof(guids));
            ThrowUtilities.NullArgument(logger, nameof(logger));

            foreach (Guid guid in guids)
            {
                _testCycles.TestContextLookup[guid].SetActive(_testCycles);
            }

            _testCycles.Run(testRunID);
        }

        /// <inheritdoc/>
        public void RunTests(IEnumerable<string> sources, int testRunID, IMUnitLogger logger)
        {
            _testCycles = DiscoverTests(sources);
            foreach (ITestMethodContext context in _testCycles.TestContextLookup.Values)
            {
                context.SetActive(_testCycles);
            }

            _testCycles.Run(testRunID);
        }
    }
}
