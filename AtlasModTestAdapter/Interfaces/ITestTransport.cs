// <copyright file="ITestTransport.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace AtlasModTestAdapter
{
    /// <summary>
    /// To ferry tests between test adapter and test engine.
    /// </summary>
    public interface ITestTransport
    {
        /// <summary>
        /// Send tests to test engine/app, and will bloack the current thread.
        /// </summary>
        /// <param name="tests">Tests to be sent.</param>
        /// <param name="frameworkHandle">Object to record test status, including results.</param>
        /// <returns>Indicate if all data is sent.</returns>
        bool SendTests(IEnumerable<TestCase> tests, IFrameworkHandle frameworkHandle);

        /// <summary>
        /// Get Test results from remote host.
        /// </summary>
        /// <param name="tests">Tests that are sent.</param>
        /// <param name="frameworkHandle">Object to record test status, including results.</param>
        /// <returns>Fail if not all results are received.</returns>
        bool GetTestResults(IEnumerable<TestCase> tests, IFrameworkHandle frameworkHandle);

        /// <summary>
        /// Cancel operation
        /// </summary>
        void Cancel();
    }
}
