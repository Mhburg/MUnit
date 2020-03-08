// <copyright file="TestCycleGraph.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using MUnit.Utilities;

namespace MUnit.Framework.Base
{
    /// <summary>
    /// A data structure used to add and run tests.
    /// </summary>
    public abstract class TestCycleGraph : ITestCycleGraph
    {
        // Running tests starts from this node.
        private readonly ITestCycle _root;
        private readonly Dictionary<Guid, ITestCycle> _testCycles = new Dictionary<Guid, ITestCycle>();
        private CancellationBit _cancellationBit;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestCycleGraph"/> class.
        /// </summary>
        /// <param name="root">Where test cycle stats.</param>
        /// <param name="logger"> Logs information. </param>
        public TestCycleGraph(ITestCycle root, IMUnitLogger logger)
        {
            ThrowUtilities.NullArgument(root, nameof(root));

            _root = root;
            _testCycles.Add(root.ID, root);
            this.Logger = logger;
        }

        /// <inheritdoc/>
        public Dictionary<Guid, ITestMethodContext> TestContextLookup { get; } = new Dictionary<Guid, ITestMethodContext>();

        /// <inheritdoc/>
        public int Count => _testCycles.Count;

        /// <inheritdoc/>
        public IMUnitLogger Logger { get; }

        /// <inheritdoc/>
        public ITestCycle this[Guid key] => _testCycles[key];

        /// <inheritdoc/>
        public IEnumerator<ITestCycle> GetEnumerator()
        {
            return _root.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc/>
        public virtual void Add(ITestCycle testCycle)
        {
            ThrowUtilities.NullArgument(testCycle, nameof(testCycle));

            _testCycles.Add(testCycle.ID, testCycle);
            if (_testCycles.TryGetValue(testCycle.ParentID, out ITestCycle parent))
            {
                ThrowUtilities.NullMember(parent.Children, nameof(ITestCycle), nameof(parent.Children));
                parent.Children.Add(testCycle);
            }
        }

        /// <inheritdoc/>
        public virtual void Run(int testRunID)
        {
            _cancellationBit = new CancellationBit();
            _root.Start(testRunID, this.Logger, _cancellationBit);
            _root.Reset();
        }

        /// <inheritdoc/>
        public bool TryGetValue(Guid key, out ITestCycle testCycle)
        {
            return _testCycles.TryGetValue(key, out testCycle);
        }

        /// <inheritdoc/>
        public void Cancel()
        {
            if (_cancellationBit != null)
                _cancellationBit.IsCancel = true;
        }
    }
}
