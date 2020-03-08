// <copyright file="ITestMethodContext.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Reflection;

namespace MUnit.Framework
{
    /// <summary>
    /// The context in which a test is run.
    /// </summary>
    public interface ITestMethodContext
    {
        /// <summary>
        /// Gets the ID of test.
        /// </summary>
        Guid TestID { get; }

        /// <summary>
        /// Gets or sets ID of test run.
        /// </summary>
        int TestRunID { get; set; }

        /// <summary>
        /// Gets or sets instance passed to function call <see cref="System.Reflection.MethodBase.Invoke(object, object[])"/>.
        /// </summary>
        object Instance { get; set; }

        /// <summary>
        /// Gets executor of test.
        /// </summary>
        IExecutor Executor { get; }

        /// <summary>
        /// Gets or sets a value indicating whether context is active.
        /// </summary>
        bool IsActive { get; set; }

        /// <summary>
        /// Gets fully qualified name for test method.
        /// </summary>
        string FullyQualifiedName { get; }

        /// <summary>
        /// Gets or sets source of the test.
        /// </summary>
        string Source { get; set; }

        /// <summary>
        /// Gets or sets declaring type of the test method.
        /// </summary>
        Type DeclaringType { get; set; }

        /// <summary>
        /// Gets or sets method info of the test method.
        /// </summary>
        MethodInfo MethodInfo { get; set; }

        /// <summary>
        /// Invoke test method.
        /// </summary>
        /// <returns> Test results from invoking test method. </returns>
        IEnumerable<TestResult> Invoke();

        /// <summary>
        /// Set this context and its parent test cycles to active.
        /// </summary>
        /// <param name="testCycles"> Test cycle collection that has this context. </param>
        void SetActive(ITestCycleGraph testCycles);
    }
}