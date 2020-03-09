// <copyright file="MUnitTestDiscoverer.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MUnit;
using MUnit.Engine;
using MUnitTestAdapter.Utilities;

using MUF = MUnit.Framework;

namespace MUnitTestAdapter
{
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Navigation;

    /// <summary>
    /// Discover tests developed with MUnit.
    /// </summary>
    [DefaultExecutorUri(Resources.MUnitTAConstants.ExecutorUri)]
    [FileExtension(".dll")]
    [FileExtension(".exe")]
    [Category("managed")]
    public class MUnitTestDiscoverer : ITestDiscoverer
    {
        private IMessageLogger _vsLogger;
        private ITestEngine _engine = TypeResolver.Engine;

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Required to report all exceptions.")]
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            try
            {
                ValidateArg.NotNull(sources, nameof(sources));
                ValidateArg.NotNull(discoveryContext, nameof(discoveryContext));
                ValidateArg.NotNull(logger, nameof(logger));
                ValidateArg.NotNull(discoverySink, nameof(discoverySink));

                _vsLogger = logger;
                _engine.Logger.MessageEvent += MessageEventHandler;
                TestCycleCollection tests = _engine.DiscoverTests(sources);
                _engine.Logger.MessageEvent -= MessageEventHandler;

                foreach (string source in sources)
                {
                    DiaSessionCache.PopulateCache(source);
                }

                foreach (MUF.ITestMethodContext context in tests.TestContextLookup.Values)
                {
                    TestCase testCase = AdpaterUtilites.ConvertToTestCase(context);
                    INavigationData nav = DiaSessionCache.GetNavDataForMethod(testCase.Source, context.DeclaringType.FullName, context.MethodInfo.Name);
                    testCase.CodeFilePath = nav.FileName ?? string.Empty;
                    testCase.LineNumber = nav.MinLineNumber;

                    discoverySink.SendTestCase(testCase);
                }
            }
            catch (Exception e)
            {
                _engine.Logger.RecordMessage(MUF.MessageLevel.Error, e.ToString());
            }
        }

        private void MessageEventHandler(object sender, MUF.MessageContext e)
        {
            TestMessageLevel messageLevel;
            switch (e.Level)
            {
                case MUF.MessageLevel.Debug:
                    messageLevel = TestMessageLevel.Error;
                    break;
                case MUF.MessageLevel.Trace:
                    messageLevel = TestMessageLevel.Error;
                    break;
                case MUF.MessageLevel.Error:
                    messageLevel = TestMessageLevel.Error;
                    break;
                case MUF.MessageLevel.Warning:
                    messageLevel = TestMessageLevel.Warning;
                    break;
                case MUF.MessageLevel.Information:
                    messageLevel = TestMessageLevel.Informational;
                    break;
                default:
                    messageLevel = default;
                    break;
            }

            _vsLogger.SendMessage(messageLevel, e.Message);
        }
    }
}
