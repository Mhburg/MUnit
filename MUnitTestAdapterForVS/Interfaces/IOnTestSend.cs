// <copyright file="IOnTestSend.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MUnit;
using MUnit.Transport;

namespace MUnitTestAdapter
{
    /// <summary>
    /// Action to take on tests being sent to <see cref="MUnit.Engine.ITestEngine"/>.
    /// </summary>
    public interface IOnTestSend
    {
        /// <summary>
        /// Action to take on tests being sent to <see cref="MUnit.Engine.ITestEngine"/>.
        /// </summary>
        /// <param name="client"> Used for communicating with remote test engine. </param>
        void OnTestSend(IMUnitClient client);
    }
}
