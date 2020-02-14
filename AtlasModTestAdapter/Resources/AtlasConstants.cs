// <copyright file="AtlasConstants.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Sockets;

namespace AtlasModTestAdapter.Resources
{
    /// <summary>
    /// Constants used across project.
    /// </summary>
    public static class AtlasConstants
    {
        /// <summary>
        /// EOF string for TCP connection.
        /// </summary>
        public const string EOF = "<AtlasEOF>";

        private static int localPort;
        private static int remotePort;
        private static IPEndPoint localEndPoint;
        private static IPEndPoint remoteEndPoint;

        /// <summary>
        /// Gets port for listening to incoming traffic.
        /// </summary>
        public static int LocalPort
        {
            get
            {
                if (localPort == default)
                {
                    localPort = Properties.Settings.Default.LocalPort;
                }

                return localPort;
            }
        }

        /// <summary>
        /// Gets port used for connecting to remote test engine.
        /// </summary>
        public static int RemotePort
        {
            get
            {
                if (remotePort == default)
                {
                    remotePort = Properties.Settings.Default.RemotePort;
                }

                return RemotePort;
            }
        }

        /// <summary>
        /// Gets endpoint for listening to incoming traffic.
        /// </summary>
        public static IPEndPoint LocalEndPoint
        {
            get
            {
                if (localEndPoint == null)
                {
                    localEndPoint =
                        new IPEndPoint(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], LocalPort);
                }

                return localEndPoint;
            }
        }

        /// <summary>
        /// Gets endpoint for sending traffic. It uses local host address and remote port set in app.config.
        /// </summary>
        public static IPEndPoint RemoteEndPoint
        {
            get
            {
                if (remoteEndPoint == null)
                {
                    remoteEndPoint =
                        new IPEndPoint(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], remotePort);
                }

                return remoteEndPoint;
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
