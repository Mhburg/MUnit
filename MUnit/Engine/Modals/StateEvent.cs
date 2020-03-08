// <copyright file="StateEvent.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Threading;

namespace MUnit.Engine
{
    /// <summary>
    /// Event with state object.
    /// </summary>
    public class StateEvent : EventWaitHandle
    {
        private volatile object _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateEvent"/> class.
        /// </summary>
        /// <param name="initialState"> Initial state for the event. </param>
        /// <param name="mode"> Event reset mode. </param>
        public StateEvent(bool initialState, EventResetMode mode)
            : base(initialState, mode)
        {
        }

        /// <summary>
        /// Gets or sets state object of the event.
        /// </summary>
        public object State
        {
            get => _state;
            set => _state = value;
        }
    }
}
