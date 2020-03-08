// <copyright file="SourcePackage.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;

namespace MUnit.Engine
{
    /// <summary>
    /// An information package of source that contains its full path, full name and the types in it.
    /// </summary>
    public sealed class SourcePackage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourcePackage"/> class.
        /// </summary>
        /// <param name="source"> Full path of th source. </param>
        /// <param name="fullName"> Full name of the souce including extension. </param>
        /// <param name="types"> Types in the souce. </param>
        public SourcePackage(string source, string fullName, IEnumerable<Type> types)
        {
            Source = source;
            FullName = fullName;
            Types = types;
        }

        /// <summary>
        /// Gets or sets full path of th source.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets full name of the souce including extension.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets types in the souce.
        /// </summary>
        public IEnumerable<Type> Types { get; set; }
    }
}
