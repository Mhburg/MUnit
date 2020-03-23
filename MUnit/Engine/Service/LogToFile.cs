// <copyright file="LogToFile.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using MUnit.Framework;
using MUnit.Utilities;

namespace MUnit.Engine.Service
{
    /// <summary>
    /// Writes log to file whenever <see cref="MUnitLogger"/> raises <see cref="MUnitLogger.MessageEvent"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Log file handle should live as long as the engine.")]
    public class LogToFile
    {
        private FileStream _fileStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogToFile"/> class.
        /// </summary>
        /// <param name="logger"> Logger to tapped into. </param>
        /// <param name="path"> Path to log file, includes file name. If null, a TestLog.txt will be generated at the location of executing assembly. </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Required to handle all exceptions.")]
        public void Initialize(IMUnitLogger logger, string path)
        {
            ThrowUtilities.NullArgument(logger, nameof(logger));

            try
            {
                if (path == null)
                    path = Path.Combine(Path.GetDirectoryName(MUnitConfiguration.ConfigPath), "TestLog.txt");

                logger.WriteToFile(path);
                _fileStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None);
                logger.MessageEvent += Logger_MessageEvent;
            }
            catch (Exception e)
            {
                logger.RecordMessage(MessageLevel.Error, e.ToString());
            }
        }

        private void Logger_MessageEvent(object sender, MessageContext e)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat(
                Resources.Strings.FormattedLogMessage,
                e.Timestamp.ToLongTimeString(),
                e.Level,
                e.Message)
                .AppendLine();

            byte[] bytes = UTF8Encoding.UTF8.GetBytes(stringBuilder.ToString());
            _fileStream.Write(bytes, 0, bytes.Length);
            _fileStream.Flush();
        }
    }
}
