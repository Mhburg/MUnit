// <copyright file="TestCycleBase.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using MUnit.Resources;
using MUnit.Utilities;

namespace MUnit.Framework.Base
{
    /// <inheritdoc/>
    public abstract class TestCycleBase : ITestCycle
    {
        /// <summary>
        /// A set of cleanup methods that should be called when exiting test cycle.
        /// </summary>
        protected Stack<SupportMethodsGroup> _activeCleanupMethods = new Stack<SupportMethodsGroup>();

        private string _displayName;
        private Guid _id;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestCycleBase"/> class.
        /// </summary>
        /// <param name="source">Source name of the test.</param>
        /// <param name="type"> The type from which this test cycle is constructed. </param>
        /// <param name="parentID">ID of parent test cycle.</param>
        /// <param name="scope">Scope of this test cycle.</param>
        /// <param name="displayName">Name to display in UI.</param>
        public TestCycleBase(string source, Type type, Guid parentID, TestCycleScope scope, string displayName)
            : this(source, type, parentID, scope)
        {
            _displayName = displayName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestCycleBase"/> class.
        /// </summary>
        /// <param name="source">Source name of the test.</param>
        /// <param name="type"> The type from which this test cycle is constructed. </param>
        /// <param name="parentID">ID of parent test cycle.</param>
        /// <param name="scope">Scope of this test cycle.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2214:Do not call overridable methods in constructors", Justification = "It does not depend on any class members.")]
        public TestCycleBase(string source, Type type, Guid parentID, TestCycleScope scope)
        {
            this.FullName = ResolveTestCycleFullName(type, scope);
            this.ParentID = parentID;
            this.Source = source;
            this.Scope = scope;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestCycleBase"/> class.
        /// </summary>
        /// <param name="source">Source name of the test.</param>
        /// <param name="type"> The type where this test cycle is created.</param>
        /// <param name="scope">Scope of this test cycle.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2214:Do not call overridable methods in constructors", Justification = "It does not depend on any class members.")]
        public TestCycleBase(string source, Type type, TestCycleScope scope)
        {
            this.ParentID = scope == TestCycleScope.AppDomain ? Guid.Empty : ResovleParentID(type, scope);
            this.FullName = ResolveTestCycleFullName(type, scope);
            this.Source = source;
            this.Scope = scope;
        }

        /// <inheritdoc/>
        public string FullName { get; private set; }

        /// <inheritdoc/>
        public string DisplayName
        {
            get => _displayName ?? this.FullName;
            set => _displayName = value;
        }

        /// <summary>
        /// Gets or sets the type in which this test cycle is delcared. Only used for scope <see cref="TestCycleScope.Method"/>.
        /// </summary>
        public Type DeclaringClass { get; set; }

        /// <inheritdoc/>
        public bool IsActive { get; set; }

        /// <inheritdoc/>
        public Guid ParentID { get; private set; }

        /// <inheritdoc/>
        public string Source { get; private set; }

        /// <inheritdoc/>
        public Guid ID
        {
            get
            {
                if (_id == default)
                {
                    _id = HashUtilities.GuidForTestCycleID(this.Source, this.FullName);
                }

                return _id;
            }
        }

        /// <inheritdoc/>
        public IList<ITestCycle> Children { get; } = new List<ITestCycle>();

        /// <summary>
        /// Gets or sets the scope in which test cycle runs.
        /// </summary>
        public TestCycleScope Scope { get; set; }

        /// <inheritdoc/>
        public ICollection<ITestMethodContext> TestMethodContexts { get; } = new List<ITestMethodContext>();

        /// <inheritdoc/>
        public ICollection<SupportMethodsGroup> SupportMethodsGroups { get; } = new List<SupportMethodsGroup>();

        /// <summary>
        /// Get enumerator that goes through children cycles.
        /// </summary>
        /// <returns> Returns <see cref="IEnumerator"/> that enumerates <see cref="ITestCycle"/>s. </returns>
        public IEnumerator<ITestCycle> GetEnumerator()
        {
            return this.Children.GetEnumerator();
        }

        /// <inheritdoc/>
        public void SetTestCycleActive(ITestCycleGraph tests)
        {
            ThrowUtilities.NullArgument(tests, nameof(tests));

            this.IsActive = true;
            if (this.ParentID != Guid.Empty)
            {
                tests[this.ParentID].SetTestCycleActive(tests);
            }
        }

        /// <summary>
        /// Get enumerator that goes through children cycles.
        /// </summary>
        /// <returns> Returns <see cref="IEnumerator"/> that enumerates <see cref="ITestCycle"/>s. </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc/>
        public void Start(int testRunID, IMUnitLogger logger, CancellationBit cancellationBit)
        {
            ThrowUtilities.NullArgument(logger, nameof(logger));

            foreach (ITestMethodContext context in this.TestMethodContexts.Where(t => t.IsActive))
            {
                if (this.CheckIfCancellationSet(logger, cancellationBit))
                    return;

                context.TestRunID = testRunID;
                context.Instance = this.DeclaringClass == null ? null : Activator.CreateInstance(this.DeclaringClass);
                if (this.Setup(context.Instance, logger))
                {
                    IEnumerable<TestResult> results = context.Executor.Execute(context);
                    logger.ReportTestResults(results);
                    this.Cleanup(context.Instance, logger);
                    continue;
                }

                this.Cleanup(context.Instance, logger);
                break;
            }

            if (this.CheckIfCancellationSet(logger, cancellationBit))
                return;

            if (this.Children.Any())
            {
                if (this.Setup(null, logger))
                {
                    foreach (ITestCycle testCycle in this.Children.Where(t => t.IsActive))
                    {
                        testCycle.Start(testRunID, logger, cancellationBit);
                    }
                }

                this.Cleanup(null, logger);
            }
        }

        /// <inheritdoc/>
        public abstract Guid ResovleParentID(Type type, TestCycleScope scope);

        /// <inheritdoc/>
        public abstract string ResolveTestCycleFullName(Type type, TestCycleScope scope);

        /// <inheritdoc/>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Need to handle all exceptions.")]
        public bool Setup(object instance, IMUnitLogger logger)
        {
            ThrowUtilities.NullArgument(logger, nameof(logger));

            var initMethods = new Queue<SupportMethodsGroup>(this.SupportMethodsGroups);

            try
            {
                while (initMethods.Any())
                {
                    SupportMethodsGroup group = initMethods.Dequeue();
                    _activeCleanupMethods.Push(group);

                    logger.RecordMessage(MessageLevel.Debug, group.ToString());
                    group.InitializeMethod.Invoke(instance, null);
                }
            }
            catch (Exception e)
            {
                logger.RecordMessage(MessageLevel.Error, e.ToString());
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Need to handle all exceptions.")]
        public bool Cleanup(object instance, IMUnitLogger logger)
        {
            ThrowUtilities.NullArgument(logger, nameof(logger));

            try
            {
                while (_activeCleanupMethods.Any())
                {
                    _activeCleanupMethods.Pop().CleanupMethod.Invoke(instance, null);
                }
            }
            catch (Exception e)
            {
                logger.RecordMessage(MessageLevel.Error, e.ToString());
                return false;
            }
            finally
            {
                _activeCleanupMethods.Clear();
            }

            return true;
        }

        /// <inheritdoc/>
        public void Reset()
        {
            this.IsActive = false;
            foreach (ITestMethodContext context in this.TestMethodContexts)
            {
                context.IsActive = false;
            }

            foreach (ITestCycle testCycle in this.Children)
            {
                testCycle.Reset();
            }
        }

        /// <inheritdoc/>
        public bool CheckIfCancellationSet(IMUnitLogger logger, CancellationBit cancellationBit)
        {
            ThrowUtilities.NullArgument(logger, nameof(logger));
            ThrowUtilities.NullArgument(cancellationBit, nameof(cancellationBit));

            if (cancellationBit.IsCancel)
            {
                logger.RecordMessage(MessageLevel.Information, Errors.UTE_TestIsCancelled);
                return true;
            }

            return false;
        }
    }
}
