// <copyright file="WireMessage.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace MUnit.Transport
{
    /// <summary>
    /// Enum types of wired message.
    /// </summary>
    [Flags]
    public enum WireMessageTypes : int
    {
        /// <summary>
        /// Message is of request type.
        /// </summary>
        /// <remarks> Highest bit is reserved. 1 indicates message is for reply, 0 for request. </remarks>
        Reply = 0b_1,

        /// <summary>
        /// Message for requests.
        /// </summary>
        Request = 0b_10,

        /// <summary>
        /// Command in the message should be executed before running sent tests.
        /// </summary>
        OnSendTests = 0b_100,

        /// <summary>
        /// Suggests this <see cref="WireMessage"/> packet is the end of response.
        /// </summary>
        EndOfReply = 0b_1000,

        /// <summary>
        /// Status of MUnit.
        /// </summary>
        Telemetry = 0b_1_0000,
    }

    /// <summary>
    /// Type of commands used in message.
    /// </summary>
    public enum CommandType : int
    {
        /// <summary>
        /// Invoke MethodInfo that is transported along with the message.
        /// </summary>
        CallFunction,

        /// <summary>
        /// Discover tests.
        /// </summary>
        DiscoverTests,

        /// <summary>
        /// Run tests.
        /// </summary>
        RunTests,

        /// <summary>
        /// Send resutl to target host.
        /// </summary>
        TakeResults,

        /// <summary>
        /// Cancel test run.
        /// </summary>
        Cancel,

        /// <summary>
        /// Check if hash of assembly is matched.
        /// </summary>
        CheckAssemblyHash,

        /// <summary>
        /// Record the start of a test.
        /// </summary>
        RecordTestStart,

        /// <summary>
        /// Record the end of a test.
        /// </summary>
        RecordTestEnd,
    }

    /// <summary>
    /// Message class used for wire protocol.
    /// </summary>
    [DataContract]
    public class WireMessage
    {
        private const string _toStringRegex = "ID: {0}, SessionID: {1}, Message type: {2}, Command type: {3}, Entity: {4}, Argument: {5}";

        /// <summary>
        /// Initializes a new instance of the <see cref="WireMessage"/> class.
        /// </summary>
        /// <param name="type"> Type of this wire message. </param>
        /// <param name="command"> Command to be executed on remote host. </param>
        /// <param name="data"> Data to be passed in the message. </param>
        /// <param name="callingType"> Type used for creating instance to pass to <see cref="System.Reflection.MethodBase.Invoke(object, object[])"/>. </param>
        /// <param name="parameters"> Arguments used by <see cref="WireMessage.Entity"/>, if <paramref name="command"/> is <see cref="CommandType.CallFunction"/>. </param>
        /// <param name="ctorParams"> Auguments passed to <paramref name="callingType"/> constructor. </param>
        public WireMessage(WireMessageTypes type, CommandType command, object data = null, Type callingType = null, object[] parameters = null, object[] ctorParams = null)
        {
            this.Type        = type;
            this.Command     = command;
            this.Entity      = data;
            this.CallingType = callingType;
            this.Parameters  = parameters;
            this.CtorParams  = ctorParams;
        }

        /// <summary>
        /// Gets or sets ID that identities this message. Used with reply/request bit.
        /// </summary>
        [DataMember]
        public uint ID { get; set; }

        /// <summary>
        /// Gets or sets session ID.
        /// </summary>
        [DataMember]
        public int SessionID { get; set; }

        /// <summary>
        /// Gets or sets the type of this wire message.
        /// </summary>
        [DataMember]
        public WireMessageTypes Type { get; set; }

        /// <summary>
        /// Gets or sets command to be executed on remote host.
        /// </summary>
        [DataMember]
        public CommandType Command { get; set; }

        /// <summary>
        /// Gets or sets entity to be passed in the message. It can be <see cref="System.Reflection.MethodInfo"/>.
        /// </summary>
        [DataMember]
        public object Entity { get; set; }

        /// <summary>
        /// Gets or sets instance passed to <see cref="System.Reflection.MethodBase.Invoke(object, object[])"/>.
        /// </summary>
        [DataMember]
        public Type CallingType { get; set; }

        /// <summary>
        /// Gets or sets parameters for constructor.
        /// </summary>
        [DataMember]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Required for Method invocation.")]
        public object[] CtorParams { get; set; }

        /// <summary>
        /// Gets or sets parameters passed to <see cref="System.Reflection.MethodBase.Invoke(object, object[])"/>.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Required for Method invocation.")]
        [DataMember]
        public object[] Parameters { get; set; }

        /// <summary>
        /// Returns a string in a format of "ID : {0}, Message type: {1}, Command type: {2}, Data : {3}, Arg1 {4}, Arg2 {5}." .
        /// </summary>
        /// <returns> A string that represents this object. </returns>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                _toStringRegex,
                this.ID,
                this.SessionID,
                this.Type.ToString(),
                this.Command.ToString(),
                this.Entity?.ToString(),
                this.CallingType?.ToString());
        }
    }
}
