// <copyright file="IOnTestSend.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using MUnit.Transport;

namespace MUnitTestAdapter
{
    /// <summary>
    /// Action to take on tests being sent to <see cref="MUnit.Engine.ITestEngine"/>.
    /// </summary>
    public interface IOnTestSend
    {
        /// <summary>
        /// Gets or sets milliseconed elasped after taking OnTestSend action but before sending tests to <see cref="MUnit.Engine.ITestEngine"/>.
        /// -1 to wait indefinitely.
        /// </summary>
        int ReconnectDelay { get; set; }

        /// <summary>
        /// Action to take before tests are sent to <see cref="MUnit.Engine.ITestEngine"/>.
        /// </summary>
        /// <param name="client"> Used for communicating with remote test engine. </param>
        void OnTestSend(IMUnitClient client);
    }
}
