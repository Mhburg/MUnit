// <copyright file="IServiceManager.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using MUnit.Framework;

namespace MUnit.Engine
{
    /// <summary>
    /// Service manager provides many vital services to the engine.
    /// </summary>
    public interface IServiceManager
    {
        /// <summary>
        /// Gets reflection cache for Default Load Context.
        /// Cache is populated with a reflection helper specific to a certian version of CLR.
        /// </summary>
        IReflectionWorker ReflectionCache { get; }

        /// <summary>
        /// Gets reflection cache for Reflection-only Context.
        /// Cache is populated with a reflection helper specific to a certian version of CLR.
        /// </summary>
        IReflectionWorker ReflectionOnlyCache { get; }

        /// <summary>
        /// Gets a uniform test source that reads from multiple sources.
        /// </summary>
        ITestSource TestSource { get; }

        /// <summary>
        /// Gets a buidler that build tests from Type and handle test data input.
        /// </summary>
        ITestCycleBuilder TestBuilder { get; }

        /// <summary>
        /// Gets a log container that is suitable for concurrnet access.
        /// </summary>
        /// <returns> A log container for concurrent access. </returns>
        IDictionary<MessageLevel, List<MessageContext>> GetConcurrentLog();

        /// <summary>
        /// Gets a queue for concurrent access.
        /// </summary>
        /// <typeparam name="T"> Type of stored item. </typeparam>
        /// <returns> Queue for concurrent access. </returns>
        IConcurrentQueue<T> GetConcurrentQueue<T>();

    }
}
