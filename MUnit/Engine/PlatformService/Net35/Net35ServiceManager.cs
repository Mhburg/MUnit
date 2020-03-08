// <copyright file="Net35ServiceManager.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using MUnit.Framework;

namespace MUnit.Engine.Net35
{
    /// <inheritdoc />
    public class Net35ServiceManager : IServiceManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Net35ServiceManager"/> class.
        /// </summary>
        public Net35ServiceManager()
        {
            this.TestBuilder = new TestCycleBuilder(this.ReflectionCache);
        }

        /// <inheritdoc />
        public IReflectionWorker ReflectionCache { get; } = new N35ReflectionWorker(false);

        /// <inheritdoc />
        public IReflectionWorker ReflectionOnlyCache { get; } = new N35ReflectionWorker(true);

        /// <inheritdoc />
        public ITestSource TestSource { get; } = new Net35TestSource();

        /// <inheritdoc />
        public ITestCycleBuilder TestBuilder { get; }

        /// <inheritdoc />
        public IDictionary<MessageLevel, List<MessageContext>> GetConcurrentLog()
        {
            return new N35ConcurrentDictionary<MessageLevel, List<MessageContext>>();
        }

        /// <inheritdoc />
        public IConcurrentQueue<T> GetConcurrentQueue<T>()
        {
            return new N35ConcurrentQueue<T>();
        }
    }
}