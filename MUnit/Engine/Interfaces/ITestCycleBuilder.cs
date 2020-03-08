// <copyright file="ITestCycleBuilder.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using MUnit.Framework;
using System.Collections.Generic;

namespace MUnit.Engine
{
    /// <summary>
    /// Build tests and read test data from Type.
    /// </summary>
    public interface ITestCycleBuilder
    {
        /// <summary>
        /// Discover tests from <paramref name="typesInAssembly"/>.
        /// </summary>
        /// <param name="packages"> Packages that are used for building test cycles. </param>
        /// <param name="logger">Used for logging information.</param>
        /// <returns>A collection of top level cycles.</returns>
        TestCycleCollection BuildTestCycles(IList<SourcePackage> packages, IMUnitLogger logger);
    }
}