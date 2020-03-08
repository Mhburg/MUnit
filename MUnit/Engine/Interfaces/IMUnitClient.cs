// <copyright file="IMUnitClient.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using MUnit.Framework;

namespace MUnit.Transport
{
    /// <summary>
    /// A MUnit client that can communicate with <see cref="IMUnitServer"/>.
    /// </summary>
    public interface IMUnitClient
    {
        /// <summary>
        /// Start the client.
        /// </summary>
        void Start();

        /// <summary>
        /// Discover tests in <paramref name="sources"/>.
        /// </summary>
        /// <param name="sources"> Sources used for discovering tests. </param>
        /// <returns> A list of discovered test contexts. </returns>
        /// <exception cref="ArgumentNullException"> Throws if sources are null. </exception>
        ICollection<ITestMethodContext> DiscoverTests(IEnumerable<string> sources);

        /// <summary>
        /// Send tests to remote test engine.
        /// </summary>
        /// <param name="guids"> Guid of tests. </param>
        /// <param name="testRunID"> ID generated for this test run. </param>
        void RunTests(IEnumerable<Guid> guids, out int testRunID);

        /// <summary>
        /// Send test sources to remote test engine.
        /// </summary>
        /// <param name="sources"> Sources of tests. </param>
        /// <param name="testRunID"> ID generated for this test run. </param>
        /// <returns> A list of found tests in <paramref name="sources"/>. </returns>
        /// <exception cref="ArgumentNullException"> Throws if <paramref name="sources"/> is null. </exception>
        /// <exception cref="ArgumentException"> Throws if remote hash does not match local hash. </exception>
        ICollection<ITestMethodContext> RunTests(IEnumerable<string> sources, out int testRunID);

        /// <summary>
        /// Request cancellation on test run.
        /// </summary>
        void CancelTestRun();

        /// <summary>
        /// Check hash of <paramref name="source"/> is the same as that of <paramref name="source"/> in remote host.
        /// </summary>
        /// <param name="source"> Name of source. </param>
        /// <param name="localHash"> Hash of local assembly. </param>
        /// <param name="remoteHash"> Hash of remote assembly. </param>
        /// <returns> A value indicate if hash of local and that of remote is the same. </returns>
        /// <exception cref="ArgumentNullException"> Throws if <paramref name="source"/> is null. </exception>
        bool CheckAssemblyHash(string source, out byte[] localHash, out byte[] remoteHash);

        /// <summary>
        /// Invoke function in remote hotst.
        /// </summary>
        /// <param name="method"> Method to call in remote host. </param>
        /// <param name="type"> Type used to create instance to pass to <see cref="System.Reflection.MethodBase.Invoke(object, object[])"/>. </param>
        /// <param name="parameters"> Parameters to pass in <see cref="System.Reflection.MethodBase.Invoke(object, object[])"/>. </param>
        /// <param name="ctorParams"> Parameters passed to class constructor. </param>
        /// <returns> ID of the outgoing <see cref="WireMessage"/>. </returns>
        int RemoteProcedureCall(System.Reflection.MethodInfo method, Type type, object[] parameters, object[] ctorParams);
    }
}