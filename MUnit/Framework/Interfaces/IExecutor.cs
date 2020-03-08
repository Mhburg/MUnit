// <copyright file="IExecutor.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;

namespace MUnit.Framework
{
    /// <summary>
    /// Entity that runs test.
    /// </summary>
    public interface IExecutor
    {
        /// <summary>
        /// <para> Expose an entry point where user can customize the execution of test. </para>
        /// <para> See example in <see cref="TestMethodAttribute"/>. </para>
        /// </summary>
        /// <param name="testMethodContext"> Context in which test runs. </param>
        /// <returns> Results from running test. </returns>
        IEnumerable<TestResult> Execute(ITestMethodContext testMethodContext);
    }
}