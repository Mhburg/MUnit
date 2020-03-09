// <copyright file="MUnitConfiguration.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Text;

namespace MUnit.Engine.Service
{
    using Settings = MUnit.Resources.Settings;

    /// <summary>
    /// Configuration used by <see cref="ITestEngine"/>.
    /// </summary>
    public static class MUnitConfiguration
    {
        static MUnitConfiguration()
        {
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap()
            {
                ExeConfigFilename = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Settings.SettingFile),
            };

            Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            ResourceManager resourceManager = new ResourceManager(typeof(Settings));
            foreach (DictionaryEntry setting in resourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true))
            {
                if (!configuration.AppSettings.Settings.AllKeys.Contains(setting.Key.ToString()))
                {
                    configuration.AppSettings.Settings.Add(setting.Key.ToString(), setting.Value.ToString());
                }
            }

            string byteString = string.Join(
                        ".",
                        Dns.GetHostAddresses(
                            Dns.GetHostName())[0]
                            .GetAddressBytes()
                            .Select(b => b.ToString(CultureInfo.InvariantCulture)).ToArray());

            if (!configuration.AppSettings.Settings.AllKeys.Contains(nameof(MUnitConfiguration.ServerIP)))
            {
                configuration.AppSettings.Settings.Add(
                    nameof(MUnitConfiguration.ServerIP), byteString);
            }

            configuration.Save(ConfigurationSaveMode.Minimal);

            MUnitConfiguration.ServerPort = int.Parse(configuration.AppSettings.Settings[nameof(Settings.ServerPort)].Value, CultureInfo.InvariantCulture);
            MUnitConfiguration.SendTimeout = int.Parse(configuration.AppSettings.Settings[nameof(Settings.SendTimeout)].Value, CultureInfo.InvariantCulture);
            MUnitConfiguration.ReceiveTimeout = int.Parse(configuration.AppSettings.Settings[nameof(Settings.ReceiveTimeout)].Value, CultureInfo.InvariantCulture);
            MUnitConfiguration.ServerIP = new IPAddress(
                configuration.AppSettings.Settings[nameof(MUnitConfiguration.ServerIP)]
                    .Value.Split('.').Select(s => byte.Parse(s, CultureInfo.InvariantCulture)).ToArray());
            MUnitConfiguration.TransporterAssembly = configuration.AppSettings.Settings[nameof(Settings.TransporterAssembly)].Value;
            MUnitConfiguration.TransporterType = configuration.AppSettings.Settings[nameof(Settings.TransporterType)].Value;
            MUnitConfiguration.LoggerAssembly = configuration.AppSettings.Settings[nameof(Settings.LoggerAssembly)].Value;
            MUnitConfiguration.LoggerType = configuration.AppSettings.Settings[nameof(Settings.LoggerType)].Value;
            MUnitConfiguration.LoggerLevel = configuration.AppSettings.Settings[nameof(Settings.LoggerLevel)].Value;
        }

        /// <summary>
        /// Gets or sets port the server listens to.
        /// </summary>
        public static int ServerPort { get; set; }

        /// <summary>
        /// Gets or sets server IP address.
        /// </summary>
        public static IPAddress ServerIP { get; set; }

        /// <summary>
        /// Gets or sets time-out for send operation.
        /// </summary>
        public static int SendTimeout { get; set; }

        /// <summary>
        /// Gets or sets time-out for receive operation.
        /// </summary>
        public static int ReceiveTimeout { get; set; }

        /// <summary>
        /// Gets or sets assembly that contains the transporter.
        /// </summary>
        public static string TransporterAssembly { get; set; }

        /// <summary>
        /// Gets or sets type of transporter.
        /// </summary>
        public static string TransporterType { get; set; }

        /// <summary>
        /// Gets or sets assembly that contains a logger type.
        /// </summary>
        public static string LoggerAssembly { get; set; }

        /// <summary>
        /// Gets or sets type of logger.
        /// </summary>
        public static string LoggerType { get; set; }

        /// <summary>
        /// Gets or sets severity level of message for which logger will records.
        /// </summary>
        public static string LoggerLevel { get; set; }
    }
}
