// <copyright file="RunSettingsProvider.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        /// Load settings from xml file.
        /// </summary>
        /// <param name="reader"> XmlReader with loaded xml file ready for reading. </param>
        public void Load(XmlReader reader)
        {
            ValidateArg.NotNull(reader, nameof(reader));

            while (reader.MoveToContent() != XmlNodeType.None)
            {
                switch (reader.Name)
                {
                    case nameof(MUnitConfiguration.ServerIP):
                        MUnitConfiguration.ServerIP = new IPAddress(
                            reader.ReadContentAsString()
                                  .Split('.')
                                  .Select(s => byte.Parse(s, System.Globalization.CultureInfo.InvariantCulture))
                                  .ToArray());
                        break;
                    case nameof(MUnitConfiguration.ServerPort):
                        MUnitConfiguration.ServerPort = reader.ReadContentAsInt();
                        break;
                    case nameof(MUnitConfiguration.SendTimeout):
                        MUnitConfiguration.SendTimeout = reader.ReadContentAsInt();
                        break;
                    case nameof(MUnitConfiguration.ReceiveTimeout):
                        MUnitConfiguration.ReceiveTimeout = reader.ReadContentAsInt();
                        break;
                }
            }
        }
    }
}
