// <copyright file="TestCycleBuilder.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using MUnit.Framework;
using MUnit.Framework.Base;
using MUnit.Resources;
using MUnit.Utilities;

namespace MUnit.Engine
{
    /// <summary>
    /// Provide test builder functionality for .Net framework 3.5.
    /// </summary>
    public class TestCycleBuilder : ITestCycleBuilder
    {
        private readonly IReflectionWorker _reflectionWorker;
        private readonly ReflectionHelper _reflectionHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestCycleBuilder"/> class.
        /// </summary>
        /// <param name="reflectionWorker"> Worker for reflection operation. </param>
        public TestCycleBuilder(IReflectionWorker reflectionWorker)
        {
            _reflectionWorker = reflectionWorker;
            _reflectionHelper = new ReflectionHelper(reflectionWorker);
        }

        /// <inheritdoc/>
        public virtual TestCycleCollection BuildTestCycles(IList<SourcePackage> packages, IMUnitLogger logger)
        {
            ThrowUtilities.NullArgument(packages, nameof(packages));

            TestCycle root = new TestCycle(null, GetType(), TestCycleScope.AppDomain);
            TestCycleCollection testCycles = new TestCycleCollection(root, logger);

            logger?.RecordMessage(MessageLevel.Trace, string.Format(
                CultureInfo.InvariantCulture,
                "Create root test cycle with full name: {0} and parent ID: {1}",
                root.FullName,
                root.ParentID));

            foreach (SourcePackage package in packages)
            {
                BuildCycleFromAssembly(package.Source, root, package.Types, testCycles, logger);
            }

            return testCycles;
        }

        protected virtual void BuildCycleFromAssembly(string source, ITestCycle root, IEnumerable<Type> types, TestCycleCollection testCycles, IMUnitLogger logger)
        {
            ThrowUtilities.NullArgument(root, nameof(root));
            ThrowUtilities.NullArgument(testCycles, nameof(testCycles));
            ThrowUtilities.NullArgument(types, nameof(types));

            TestCycle assemblyCycle = new TestCycle(source, types.First(), root.ID, TestCycleScope.Assembly);

            testCycles.Add(assemblyCycle);
            foreach (Type type in types)
            {
                BuildCycleFromType(source, type, testCycles, logger);
            }
        }

        protected virtual void BuildCycleFromType(string source, Type type, TestCycleCollection testCycles, IMUnitLogger logger)
        {
            ThrowUtilities.NullArgument(type, nameof(type));
            ThrowUtilities.NullArgument(testCycles, nameof(testCycles));

            if (_reflectionHelper.IsValidTestClass(type, logger))
            {
                if (!testCycles.TryGetValue(HashUtilities.GuidForTestCycleID(source, type.Namespace), out _))
                {
                    TestCycle namespaceCycle = new TestCycle(source, type, TestCycleScope.Namespace);
                    testCycles.Add(namespaceCycle);
                }

                TestCycle classCycle = new TestCycle(source, type, TestCycleScope.Class);
                testCycles.Add(classCycle);

                DiscoverTests(source, type, testCycles, logger);
                DiscoverPreparationMethod(source, type, type, testCycles, logger);
            }
        }

        /// <summary>
        /// Discover preparation methods in type.
        /// </summary>
        /// <param name="source"> Full path to the assembly that contains <paramref name="type"/>.</param>
        /// <param name="reference"> The type used for reference when retrieve test cycles. </param>
        /// <param name="type"> In which preparation methods are discovered. </param>
        /// <param name="testCycles"> Test cycles for query. </param>
        /// <param name="logger"> Log information. </param>
        protected virtual void DiscoverPreparationMethod(string source, Type reference, Type type, TestCycleCollection testCycles, IMUnitLogger logger)
        {
            if (type == null)
                return;

            ThrowUtilities.NullArgument(testCycles, nameof(testCycles));

            DiscoverPreparationMethod(source, reference, type.BaseType, testCycles, logger);

            foreach (MethodInfo method in _reflectionWorker.GetDeclaredMethods(type))
            {
                if (_reflectionHelper.IsValidPrepMethod(method, logger))
                {
                    IEnumerable<SupportingAttribute> preparations =
                        _reflectionWorker.GetDerivedAttributes(method, typeof(SupportingAttribute), false)
                            .OfType<SupportingAttribute>();

                    foreach (SupportingAttribute prep in preparations)
                    {
                        Guid testCycleID = HashUtilities.GuidForTestCycleID(source, _reflectionHelper.ResolveTestCycleFullName(reference, prep.Scope));
                        if (testCycles.TryGetValue(testCycleID, out ITestCycle cycle))
                        {
                            prep.Register(cycle, method);

                            logger?.RecordMessage(MessageLevel.Trace, string.Format(
                                CultureInfo.InvariantCulture,
                                "{0} prep method is registered to test cycle {1}",
                                prep.PreparationType,
                                cycle.FullName));
                        }
                        else
                        {
                            logger?.RecordMessage(MessageLevel.Error, string.Format(
                                CultureInfo.CurrentCulture,
                                Errors.UTE_TestCycleNotFoundForPrep,
                                method.Name));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Discover tests from a type.
        /// </summary>
        /// <param name="source"> Full path to the assembly that contains <paramref name="type"/>.</param>
        /// <param name="type"> Discover tests in this type. </param>
        /// <param name="testCycles"> Test cycles for query. </param>
        /// <param name="logger"> Log information. </param>
        protected virtual void DiscoverTests(string source, Type type, TestCycleCollection testCycles, IMUnitLogger logger)
        {
            ThrowUtilities.NullArgument(type, nameof(type));
            ThrowUtilities.NullArgument(testCycles, nameof(testCycles));

            foreach (MethodInfo method in _reflectionWorker.GetDeclaredMethods(type))
            {
                if (_reflectionHelper.IsValidTestMethod(method, logger))
                {
                    TestMethodAttribute methodAttribute = _reflectionWorker.GetAttributesHaveBase(method, typeof(TestMethodAttribute), false).First() as TestMethodAttribute;
                    Guid testCycleID = HashUtilities.GuidForTestCycleID(source, _reflectionHelper.ResolveTestCycleFullName(type, methodAttribute.Scope));

                    if (!testCycles.TryGetValue(testCycleID, out ITestCycle testCycle))
                    {
                        testCycle = new TestCycle(source, type, TestCycleScope.Method)
                        {
                            DeclaringClass = type,
                        };
                        testCycles.Add(testCycle);
                    }

                    TestMethodContext context = new TestMethodContext(source, testCycle, method, type, logger);
                    if (_reflectionWorker.TryGetAttributeAssignableTo(method, typeof(IDataSource), false, out Attribute dataAttribute))
                    {
                        if (dataAttribute is IDataProvidingMethod dataProvidingMethod)
                        {
                            if (dataProvidingMethod.DeclaringType == null)
                                dataProvidingMethod.DeclaringType = type;
                        }

                        // TODO Report data method that has wrong signature.
                        context.DataSource = dataAttribute as IDataSource;
                    }

                    IExecutor executor = _reflectionWorker.GetAttributeAssignableTo(method, typeof(IExecutor), false) as IExecutor;
                    context.Executor = executor;

                    testCycle.TestMethodContexts.Add(context);
                    testCycles.TestContextLookup.Add(context.TestID, context);

                    logger?.RecordMessage(MessageLevel.Trace, string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.Strings.FoundTestMethod,
                        type.FullName,
                        method.Name));
                }
            }
        }
    }
}