// <copyright file="Net35TestSource.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using MUnit.Framework;
using MUnit.Resources;
using MUnit.Utilities;

namespace MUnit.Engine.Net35
{
    /// <summary>
    /// Prepare test source under .Net framework 3.5.
    /// </summary>
    public class Net35TestSource : ITestSource
    {
        /// <inheritdoc/>
        public IList<SourcePackage> GetTypes(IEnumerable<string> sources, IMUnitLogger logger)
        {
            ThrowUtilities.NullArgument(sources, nameof(sources));
            ThrowUtilities.NullArgument(logger, nameof(logger));

            List<SourcePackage> packages = new List<SourcePackage>();
            foreach (string source in sources)
            {
                logger.RecordMessage(MessageLevel.Trace, "Loading tests from: " + Path.GetFullPath(source));

                if (File.Exists(source))
                {
                    string fileName = Path.GetFileName(source);
                    if (!ValidateExtension(fileName))
                    {
                        logger.RecordMessage(MessageLevel.Error, Errors.InvalidExtension);
                        continue;
                    }
                    else if (!IsLoaded(source, out Assembly assembly))
                    {
                        logger.RecordMessage(MessageLevel.Error, Errors.FileNotLoaded);
                        continue;
                    }
                    else
                    {
                        if (packages.Find(package => package.FullName != assembly.FullName) == null)
                            packages.Add(new SourcePackage(Path.GetFullPath(source), assembly.FullName, assembly.GetTypes()));
                        else
                            logger.RecordMessage(MessageLevel.Warning, Errors.UTE_DuplicateAssembly);
                    }
                }
                else
                {
                    logger.RecordMessage(MessageLevel.Error, "File not exist.");
                }
            }

            return packages;
        }

        private bool ValidateExtension(string filename)
        {
            return filename.EndsWith(".dll", false, CultureInfo.CurrentCulture)
                   || filename.EndsWith(".exe.", false, CultureInfo.CurrentCulture);
        }

        private bool IsLoaded(string source, out Assembly target)
        {
            Assembly assembly = Assembly.Load(new AssemblyName(Path.GetFileNameWithoutExtension(source)));

            foreach (Assembly assembly1 in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly1.FullName == assembly.FullName)
                {
                    target = assembly1;
                    return true;
                }
            }

            target = null;
            return false;
        }
    }
}