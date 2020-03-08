// <copyright file="SupportMethodsGroup.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace MUnit.Framework
{
    /// <summary>
    /// Enclose Initialize and Cleanup methods discoverd in a <see cref="Type"/>.
    /// </summary>
    public class SupportMethodsGroup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SupportMethodsGroup"/> class.
        /// </summary>
        /// <param name="declaringType"> <see cref="Type"/> in which support methods are discovered. </param>
        public SupportMethodsGroup(Type declaringType)
        {
            this.DeclaringType = declaringType;
        }

        /// <summary>
        /// Gets or sets <see cref="Type"/> in which <see cref="SupportMethodsGroup.CleanupMethod"/> and <see cref="SupportMethodsGroup.InitializeMethod"/> are found.
        /// </summary>
        public Type DeclaringType { get; set; }

        /// <summary>
        /// Gets or sets initialize method used by <see cref="ITestCycle"/>.
        /// </summary>
        public MethodInfo InitializeMethod { get; set; }

        /// <summary>
        /// Gets or sets Cleanup method used by <see cref="ITestCycle"/>.
        /// </summary>
        public MethodInfo CleanupMethod { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            return stringBuilder.AppendFormat(
                CultureInfo.CurrentCulture,
                "{0}, Declaring Type:{1}, Initialize Method:{2}, Cleanup Method:{3}",
                base.ToString(),
                this.DeclaringType.ToString(),
                this.InitializeMethod.Name,
                this.CleanupMethod)
                .ToString();
        }
    }
}
