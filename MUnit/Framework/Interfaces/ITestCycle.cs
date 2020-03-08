// <copyright file="ITestCycle.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Reflection;

namespace MUnit.Framework
{
    /// <summary>
    /// Scope of test cycle.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented", Justification = "Self explanatory.")]
    public enum TestCycleScope
    {
        AppDomain = 0,
        Assembly = 1,
        Namespace = 2,
        Class = 3,
        Method = 4,
    }

    /// <summary>
    /// Basic cycle of a test.
    /// </summary>
    public interface ITestCycle : IEnumerable<ITestCycle>
    {
        /// <summary>
        /// Gets the scope in which test cycle runs.
        /// </summary>
        TestCycleScope Scope { get; }

        /// <summary>
        /// Gets full qualified name of the underlying type representing the test cycle.
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Gets name for displayed.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets the type in which this test cycle is delcared. Only used for scope <see cref="TestCycleScope.Method"/>.
        /// </summary>
        Type DeclaringClass { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the test cycle is active.
        /// </summary>
        bool IsActive { get; set; }

        /// <summary>
        /// Gets children test cycles.
        /// </summary>
        IList<ITestCycle> Children { get; }

        /// <summary>
        /// Gets full path of the source that contains tests.
        /// </summary>
        string Source { get; }

        /// <summary>
        /// Gets test ID.
        /// </summary>
        Guid ID { get; }

        /// <summary>
        /// Gets parent of this test cycle. Should not be null.
        /// </summary>
        Guid ParentID { get; }

        /// <summary>
        /// Gets a collectioni of <see cref="SupportMethodsGroup"/> that has Initialize method and Cleanup method for this test cycle.
        /// </summary>
        ICollection<SupportMethodsGroup> SupportMethodsGroups { get; }

        /// <summary>
        /// Gets executors for test.
        /// </summary>
        ICollection<ITestMethodContext> TestMethodContexts { get; }

        /// <summary>
        /// Start the test cycle.
        /// </summary>
        /// <param name="testRunID"> ID of test run. </param>
        /// <param name="logger"> Logs information. </param>
        /// <param name="cancellationBit"> Indicate if cancellation is signalled. </param>
        /// <exception cref="ArgumentNullException"> Throws if <paramref name="logger"/> is null. </exception>
        /// <remarks>
        /// <para> The requirement for cancellation is quite relaxed for this application. </para>
        /// <para> For a more robust implementation, See https://docs.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads?view=netframework-4.8 .</para>
        /// </remarks>
        void Start(int testRunID, IMUnitLogger logger, CancellationBit cancellationBit);

        /// <summary>
        /// Run Initialize methods starting from base class at the start.
        /// </summary>
        /// <param name="instance"> Instance passed to <see cref="MethodBase.Invoke(object, object[])"/>. </param>
        /// <param name="logger"> Logs information. </param>
        /// <returns> Returns true if set up is successful. </returns>
        bool Setup(object instance, IMUnitLogger logger);

        /// <summary>
        /// Run Cleanup methods starting from the most derived class.
        /// </summary>
        /// <param name="instance"> Instance passed to <see cref="MethodBase.Invoke(object, object[])"/>. </param>
        /// <param name="logger"> Logs information. </param>
        /// <returns> Returns true if clean up is successful. </returns>
        bool Cleanup(object instance, IMUnitLogger logger);

        /// <summary>
        /// Set this test cycle and all test cycles above its scope to active.
        /// </summary>
        /// <param name="tests">Collection that holds this test.</param>
        void SetTestCycleActive(ITestCycleGraph tests);

        /// <summary>
        /// Reset test cycle and its contexts to inactive.
        /// </summary>
        void Reset();

        /// <summary>
        /// Resolve parent ID for this test cycle.
        /// </summary>
        /// <param name="type"> From which this test cycle is constructed. </param>
        /// <param name="scope"> Scope of this test cycle. </param>
        /// <returns> Parent's Guid for this test cycle. </returns>
        /// <remarks> It is called in constructor. Make sure it does not depend on any class members. </remarks>
        Guid ResovleParentID(Type type, TestCycleScope scope);

        /// <summary>
        /// Resolve full name for this test cycle.
        /// </summary>
        /// <param name="type"> From which this test cycle is constructed. </param>
        /// <param name="scope"> Scope of this test cycle. </param>
        /// <returns> Full name of this test cycle. </returns>
        string ResolveTestCycleFullName(Type type, TestCycleScope scope);

        /// <summary>
        /// Check if cancellation bit is set.
        /// </summary>
        /// <param name="logger"> Logger used by engine. </param>
        /// <param name="cancellationBit"> Bit to check. </param>
        /// <returns> Returns true is cancellation is signalled. </returns>
        bool CheckIfCancellationSet(IMUnitLogger logger, CancellationBit cancellationBit);
    }
}
