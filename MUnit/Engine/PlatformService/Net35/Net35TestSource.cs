// <copyright file="Net35TestSource.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Reflection;
using MUnit.Engine.Service;
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
                    else if (!TryLoaded(source, out Assembly assembly, logger))
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Required to catch all exception.")]
        private bool TryLoaded(string source, out Assembly target, IMUnitLogger logger)
        {
            target = Assembly.LoadFrom(source);
            List<AssemblyName> loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().Select(asm => asm.GetName()).ToList();
            foreach (AssemblyName assemblyName in target.GetReferencedAssemblies())
            {
                if (!loadedAssemblies.Contains(assemblyName))
                {
                    try
                    {
                        foreach (string directory in MUnitConfiguration.WorkingDirectories)
                        {
                            string path = Path.Combine(directory, assemblyName.Name) + ".dll";
                            Assembly.LoadFrom(path);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.RecordMessage(MessageLevel.Information, e.ToString());
                    }
                }
            }

            if (target != null)
                return true;
            else
                return false;
        }
    }
}