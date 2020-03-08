// <copyright file="EventLockTuple.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MUnit.Engine
{
    /// <summary>
    /// A lock tuple consists of a <see cref="StateEvent"/> and a <see cref="AutoResetEvent"/>.
    /// </summary>
    public class EventLockTuple
    {
        /// <summary>
        /// Gets or sets an <see cref="StateEvent"/> for event handler.
        /// </summary>
        public StateEvent HandlerLock { get; set; } = new StateEvent(false, EventResetMode.AutoReset);

        /// <summary>
        /// Gets or sets an <see cref="AutoResetEvent"/>.
        /// </summary>
        public AutoResetEvent EventLock { get; set; } = new AutoResetEvent(false);
    }
}
