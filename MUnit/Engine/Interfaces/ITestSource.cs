// <copyright file="ITestSource.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using MUnit.Framework;
using System.Collections.Generic;

namespace MUnit.Engine
{
    /// <summary>
    /// Prepare types from variable sources for reading.
    /// </summary>
    public interface ITestSource
    {
        /// <summary>
        /// Get Types from <paramref name="sources"/>.
        /// </summary>
        /// <param name="sources">Full path or file name of the source.</param>
        /// <param name="logger">Logger used to record message.</param>
        /// <returns> return a <see cref="SourcePackage"/>. </returns>
        IList<SourcePackage> GetTypes(IEnumerable<string> sources, IMUnitLogger logger);
    }
}