// <copyright file="RestartRW.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Reflection;
using MUnit.Transport;
using MUnitTestAdapter;
using Verse;

namespace MUnitVSExtensionForRW
{
    /// <summary>
    /// Restart RimWorld before running tests.
    /// </summary>
    public class RestartRW : IOnTestSend
    {
        /// <inheritdoc/>
        public int ReconnectDelay { get; set; } = 30000;

        /// <inheritdoc/>
        public void OnTestSend(IMUnitClient client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            client.RemoteProcedureCall(typeof(GenCommandLine).GetMethod("Restart", BindingFlags.Public | BindingFlags.Static), null, null, null);
        }
    }
}
