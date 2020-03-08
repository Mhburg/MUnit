// <copyright file="Interface1.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUnit.Framework
{
    /// <summary>
    /// Data source that is in loaded assemblies.
    /// </summary>
    public interface IDataProvidingMethod : IDataSource
    {
        /// <summary>
        /// Gets or sets type in which the data providing method is declared.
        /// </summary>
        Type DeclaringType { get; set; }

        /// <summary>
        /// Gets the name of a data providing method.
        /// </summary>
        string MethodName { get; }
    }
}
