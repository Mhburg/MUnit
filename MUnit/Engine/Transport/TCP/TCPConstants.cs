// <copyright file="TCPConstants.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System.Net;
using System.Net.Sockets;
using MUnit.Engine.Service;

namespace MUnit.Transport
{
    /// <summary>
    /// Constants used across project.
    /// </summary>
    public static class TCPConstants
    {
        /// <summary>
        /// EOF mark for <see cref="WireMessage"/>.
        /// </summary>
        public const string MUnitEOF = "<MUnitEOF>";

        private static IPEndPoint _serverEndPoint;

        /// <summary>
        /// Gets endpoint for listening to incoming traffic.
        /// </summary>
        public static IPEndPoint ServerEndPoint
        {
            get
            {
                if (_serverEndPoint == null)
                {
                    _serverEndPoint =
                        new IPEndPoint(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], MUnitConfiguration.ServerPort);
                }

                return _serverEndPoint;
            }
        }

        /// <summary>
        /// Gets address family used by local host.
        /// </summary>
        public static AddressFamily MachineAddressFamily
        {
            get => Dns.GetHostAddresses(Dns.GetHostName())[0].AddressFamily;
        }
    }
}
