// <copyright file="IDataSource.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;

namespace MUnit.Framework
{
    /// <summary>
    /// Data source for test method.
    /// </summary>
    public interface IDataSource
    {
        /// <summary>
        /// Retrieve data from data source.
        /// </summary>
        /// <returns> A collection of data entries for testing. </returns>
        IEnumerable<object[]> GetData();
    }
}
