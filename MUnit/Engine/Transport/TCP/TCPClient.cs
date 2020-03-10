// <copyright file="TCPClient.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.IO;
using System.Net;
using MUnit.Engine;
using MUnit.Engine.Service;
using MUnit.Framework;

namespace MUnit.Transport
{
    /// <summary>
    /// TCP implementation of MUnitClient.
    /// </summary>
    public class TCPClient : MUnitClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TCPClient"/> class.
        /// </summary>
        /// <param name="testEngine"> Test engine that uses this client. </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Null checked in base type.")]
        public TCPClient(ITestEngine testEngine)
            : base(testEngine, new TCPClientWorker())
        {
            new LogToFile().Initialize(
                testEngine.Logger,
                Path.Combine(
                    Path.GetDirectoryName(
                        PlatformService.GetServiceManager().ReflectionCache.GetExecAssemblyLocation()), "ClientLog.txt"));
        }
    }
}
