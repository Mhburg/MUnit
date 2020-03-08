// <copyright file="MessageContext.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;

namespace MUnit.Framework
{
    /// <summary>
    /// Context in which a message is recorded.
    /// </summary>
    public class MessageContext : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageContext"/> class.
        /// </summary>
        /// <param name="message">Message to be logged.</param>
        /// <param name="level">Severity of the message.</param>
        /// <param name="timestamp">The time when the messaeg is logged.</param>
        /// <param name="stackTrace"> Stack trace of this message. </param>
        public MessageContext(string message, MessageLevel level, DateTime timestamp, string stackTrace = null)
        {
            this.Message = message;
            this.Level = level;
            this.Timestamp = timestamp;
            this.StackTrace = stackTrace;
        }

        /// <summary>
        /// Gets or sets message to be recorded.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets severity of the message.
        /// </summary>
        public MessageLevel Level { get; set; }

        /// <summary>
        /// Gets or sets the time when the message is logged.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets stack trace of where this message is created.
        /// </summary>
        public string StackTrace { get; set; }
    }
}
