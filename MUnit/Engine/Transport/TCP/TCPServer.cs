// <copyright file="TCPServer.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.IO;
using MUnit.Engine;
using MUnit.Engine.Service;
using MUnit.Framework;

namespace MUnit.Transport
{
    /// <summary>
    /// TCP implementation of the Transporter.
    /// </summary>
    /// <remarks>
    /// <para> SynchronizationContext is captured using the IAsyncResult pattern. </para>
    /// <para> See https://referencesource.microsoft.com/#System/net/System/Net/_ContextAwareResult.cs,b5a5aefaa240ffa1,references .</para>
    /// </remarks>
    public class TCPServer : MUnitServer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TCPServer"/> class.
        /// </summary>
        /// <param name="testEngine"> Test engine that this transporter supports. </param>
        /// <param name="logger"> Logger used by the engine. </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Null checked in base type.")]
        public TCPServer(ITestEngine testEngine)
            : base(testEngine, new TCPServerWorker())
        {
            new LogToFile().Initialize(testEngine.Logger, Path.Combine(Path.GetDirectoryName(PlatformService.GetServiceManager().ReflectionCache.GetExecAssemblyLocation()), "ServerLog.txt"));
        }
    }
}
