// <copyright file="AtlasModTestExecutor.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using AtlasModTestAdapter.Resources;
using AtlasModTestAdapter.Utilities;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace AtlasModTestAdapter
{
    /// <summary>
    /// <para>ITestExecutor implementation that is called by vs test framework or the IDE.</para>
    /// <para>See https://github.com/Microsoft/vstest-docs/blob/master/RFCs/0004-Adapter-Extensibility.md .</para>
    /// </summary>
    public class AtlasModTestExecutor : ITestExecutor
    {
        private readonly ITestTransport testTransport;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtlasModTestExecutor"/> class.
        /// </summary>
        public AtlasModTestExecutor()
        {
            this.testTransport = (ITestTransport)Activator.CreateInstance(
                                    Properties.Settings.Default.AssemblyName,
                                    Properties.Settings.Default.TestTransporterClass);
        }

        #region ITestExecutor Implementation

        /// <summary>
        /// Cancel the execution of the tests.
        /// </summary>
        public void Cancel()
        {
            this.testTransport.Cancel();
        }

        /// <summary>
        /// Runs only the tests specified by parameter 'tests'. 
        /// </summary>
        /// <param name="tests">Tests to be run.</param>
        /// <param name="runContext">Context to use when executing the tests.</param>
        /// <param name="frameworkHandle">Handle to the framework to record results and to do framework operations.</param>
        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            ValidateArg.NotNull(tests, nameof(tests));

            if (!this.testTransport.SendTests(tests, frameworkHandle))
            {
                return;
            }

            if (!this.testTransport.GetTestResults(tests, frameworkHandle))
            {
                return;
            }
        }

        /// <summary>
        /// Runs 'all' the tests present in the specified 'sources'. 
        /// </summary>
        /// <param name="sources">Path to test container files to look for tests in.</param>
        /// <param name="runContext">Context to use when executing the tests.</param>
        /// <param name="frameworkHandle">Handle to the framework to record results and to do framework operations.</param>
        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
