// <copyright file="AssertException.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace MUnit
{
    /// <summary>
    /// Exception thrown by <see cref="Assert"/> class.
    /// </summary>
    [Serializable]
    public class AssertException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssertException"/> class.
        /// </summary>
        public AssertException()
        {
        }

        /// <inheritdoc/>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1648:inheritdoc should be used with inheriting class", Justification = "bug from StyleCop")]
        public AssertException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <inheritdoc/>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1648:inheritdoc should be used with inheriting class", Justification = "bug from StyleCop")]
        public AssertException(string message)
            : base(message)
        {
        }

        /// <inheritdoc/>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1648:inheritdoc should be used with inheriting class", Justification = "bug from StyleCop")]
        protected AssertException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}
