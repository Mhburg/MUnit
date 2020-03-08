// <copyright file="TestCycleCollection.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using MUnit.Framework;
using MUnit.Framework.Base;

namespace MUnit.Engine
{
    /// <inheritdoc/>
    public class TestCycleCollection : TestCycleGraph
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestCycleCollection"/> class.
        /// </summary>
        /// <param name="root">From which test starts.</param>
        /// <param name="logger"> Logs information. </param>
        public TestCycleCollection(ITestCycle root, IMUnitLogger logger)
            : base(root, logger)
        {
        }
    }
}
