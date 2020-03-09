// <copyright file="RunSettingsProvider.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using System.Linq;
using System.Net;

using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using MUnit.Engine.Service;

namespace MUnitTestAdapter
{
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

    /// <summary>
    /// Read settings from *.runsettings file.
    /// </summary>
    [SettingsName("MUnitAdpater")]
    public class RunSettingsProvider : ISettingsProvider
    {
        /// <summary>
        /// Gets or sets action to take before sending tests to remote host.
        /// </summary>
        public static IOnTestSend OnTestSendAction { get; set; }

        private static string OnTestSendAssembly { get; set; }

        /// <summary>
        /// Load settings from xml file.
        /// </summary>
        /// <param name="reader"> XmlReader with loaded xml file ready for reading. </param>
        public void Load(XmlReader reader)
        {
            ValidateArg.NotNull(reader, nameof(reader));

            while (reader.Read())
            {
                Debug.WriteLine("Name: {0}, Value : {1}", reader.Name, reader.Value);
                switch (reader.Name)
                {
                    case nameof(MUnitConfiguration.ServerIP):
                        reader.Read();
                        MUnitConfiguration.ServerIP = new IPAddress(
                            reader.ReadContentAsString()
                                  .Split('.')
                                  .Select(s => byte.Parse(s, System.Globalization.CultureInfo.InvariantCulture))
                                  .ToArray());
                        break;
                    case nameof(MUnitConfiguration.ServerPort):
                        reader.Read();
                        MUnitConfiguration.ServerPort = reader.ReadContentAsInt();
                        break;
                    case nameof(MUnitConfiguration.SendTimeout):
                        reader.Read();
                        MUnitConfiguration.SendTimeout = reader.ReadContentAsInt();
                        break;
                    case nameof(MUnitConfiguration.ReceiveTimeout):
                        reader.Read();
                        MUnitConfiguration.ReceiveTimeout = reader.ReadContentAsInt();
                        break;
                    case nameof(OnTestSendAssembly):
                        reader.Read();
                        OnTestSendAssembly = reader.ReadContentAsString();
                        OnTestSendAction = this.LoadOnTestSendAction(OnTestSendAssembly);
                        break;
                }
            }
        }

        private IOnTestSend LoadOnTestSendAction(string onTestSendAssembly)
        {
            Assembly assembly = Assembly.LoadFrom(onTestSendAssembly);
            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(IOnTestSend).IsAssignableFrom(type))
                {
                    return (IOnTestSend)Activator.CreateInstance(type);
                }
            }

            return null;
        }
    }
}
