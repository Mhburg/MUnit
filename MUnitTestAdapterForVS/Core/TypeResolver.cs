// <copyright file="TypeResolver.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MUnit.Engine;
using MUnit.Engine.Service;
using MUnit.Framework;
using MUnit.Transport;

namespace MUnitTestAdapter
{
    /// <summary>
    /// Resolve type dependencies for MUnit.
    /// </summary>
    public static class TypeResolver
    {
        private static ITestEngine _engine = null;
        private static Assembly _clientAssembly = null;
        private static MUnitClient _mUnitClient = null;
        private static IMUnitLogger _mLogger = null;

        /// <summary>
        /// Gets an instance of <see cref="MUnitClient"/> that is specified in MUnit.config .
        /// </summary>
        public static MUnitClient MUnitClient
        {
            get
            {
                if (_mUnitClient == null)
                {
                    if (_clientAssembly == null)
                        _clientAssembly = Assembly.LoadFrom(MUnitConfiguration.ClientAssembly);

                    _mUnitClient = (MUnitClient)Activator.CreateInstance(_clientAssembly.GetType(MUnitConfiguration.ClientType), Engine);
                }

                return _mUnitClient;
            }
        }

        /// <summary>
        /// Gets instance of engine.
        /// </summary>
        public static ITestEngine Engine
        {
            get
            {
                if (_engine == null)
                {
                    _engine = new MUnitEngine(GetLogger());
                }

                return _engine;
            }
        }

        /// <summary>
        /// Gets an instance of <see cref="IMUnitLogger"/> that is specified in MUnit.config .
        /// </summary>
        /// <returns> Returns an implementation of <see cref="IMUnitLogger"/>. </returns>
        public static IMUnitLogger GetLogger()
        {
            if (_mLogger == null)
            {
                Assembly assembly = Assembly.LoadFrom(MUnitConfiguration.LoggerAssembly);
                if (Enum.TryParse<MessageLevel>(MUnitConfiguration.LoggerLevel, out MessageLevel messageLevel))
                {
                    _mLogger = (IMUnitLogger)Activator.CreateInstance(assembly.GetType(MUnitConfiguration.LoggerType), messageLevel);
                }
                else
                {
                    throw new InvalidDataException(
                        string.Format(
                            System.Globalization.CultureInfo.InvariantCulture,
                            Resources.Errors.UTA_CantParseLoggerLevel,
                            MessageLevel.Information,
                            MessageLevel.Warning,
                            MessageLevel.Error,
                            MessageLevel.Trace,
                            MessageLevel.Debug));
                }
            }

            return _mLogger;
        }

        /// <summary>
        /// Replace client instance.
        /// </summary>
        /// <returns> The latest client instance. </returns>
        public static MUnitClient ReplaceClient()
        {
            _mUnitClient = (MUnitClient)Activator.CreateInstance(_clientAssembly.GetType(MUnitConfiguration.ClientType), Engine);
            return _mUnitClient;
        }
    }
}
