// <copyright file="ReflectionHelper.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// Copyright (c) 2020 Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using MUnit.Framework;
using MUnit.Resources;
using MUnit.Utilities;

namespace MUnit.Engine
{
    /// <summary>
    /// Determines whether a type is a valid test class for this adapter.
    /// </summary>
    internal class ReflectionHelper
    {
        // Setting this to a string representation instead of a typeof(TestContext).FullName
        // since the later would require a load of the Test Framework extension assembly at this point.
        private const string TestContextFullName = "Microsoft.VisualStudio.TestTools.UnitTesting.TestContext";
        private readonly IReflectionWorker _reflectionWorker;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionHelper"/> class.
        /// </summary>
        /// <param name="reflectHelper">An instance to reflection helper for type information.</param>
        internal ReflectionHelper(IReflectionWorker reflectHelper)
        {
            this._reflectionWorker = reflectHelper;
        }

        /// <summary>
        /// Determines if a type is a valid test class for this adapter.
        /// </summary>
        /// <param name="type">The reflected type.</param>
        /// <param name="logger">Used to record information.</param>
        /// <returns>Return true if it is a valid test class.</returns>
        internal virtual bool IsValidTestClass(Type type, IMUnitLogger logger)
        {
            if (type.IsClass && _reflectionWorker.GetAttributesHaveBase(type, typeof(TestClassAttribute), false) != null)
            {
                var isPublic = type.IsPublic || (type.IsNested && type.IsNestedPublic);

                // non-public class
                if (!isPublic)
                {
                    var warning = string.Format(CultureInfo.CurrentCulture, Errors.UTA_ErrorNonPublicTestClass, type.FullName);
                    logger?.RecordMessage(MessageLevel.Warning, warning);
                    return false;
                }

                // Generic class
                if (type.IsGenericTypeDefinition && !type.IsAbstract)
                {
                    // In IDE generic classes that are not abstract are treated as not runnable. Keep consistence.
                    var warning = string.Format(CultureInfo.CurrentCulture, Errors.UTA_ErrorNonPublicTestClass, type.FullName);
                    logger?.RecordMessage(MessageLevel.Warning, warning);
                    return false;
                }

                // Class is not valid if the testContext property is incorrect
                if (!this.HasCorrectTestContextSignature(type))
                {
                    var warning = string.Format(CultureInfo.CurrentCulture, Errors.UTA_ErrorInValidTestContextSignature, type.FullName);
                    logger?.RecordMessage(MessageLevel.Warning, warning);
                    return false;
                }

                // Abstract test classes can be base classes for derived test classes.
                //   There is no way to see if there are derived test classes.
                //   Thus if a test class is abstract, just ignore all test methods from it
                //   (they will be visible in derived classes). No warnings (such as test method, deployment item,
                //   etc attribute is defined on the class) will be generated for this class:
                // What we do is:
                //   - report the class as "not valid" test class. This will cause to skip enumerating tests from it.
                //   - Do not generate warnings/do not create NOT RUNNABLE tests.
                if (type.IsAbstract)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if a method is a valid test method.
        /// </summary>
        /// <param name="testMethodInfo"> The reflected method. </param>
        /// <param name="logger"> Logs message for test engine. </param>
        /// <returns> Return true if a method is a valid test method. </returns>
        internal virtual bool IsValidTestMethod(MethodInfo testMethodInfo, IMUnitLogger logger)
        {
            if (!_reflectionWorker.HasAttributeIsOrDerivedFrom(testMethodInfo, typeof(TestMethodAttribute), false))
            {
                return false;
            }

            // Generic method Definitions are not valid.
            if (testMethodInfo.IsGenericMethodDefinition)
            {
                var message = string.Format(CultureInfo.CurrentCulture, Errors.UTA_ErrorGenericTestMethod, testMethodInfo.DeclaringType.FullName, testMethodInfo.Name);
                logger?.RecordMessage(MessageLevel.Warning, message);
                return false;
            }

            // Todo: Decide wheter parameter count matters.
            // The isGenericMethod check below id to verify that there are no closed generic methods slipping through.
            // Closed generic methods being GenericMethod<int> and open being GenericMethod<T>.
            var isValidTestMethod = testMethodInfo.IsPublic && !testMethodInfo.IsAbstract && !testMethodInfo.IsStatic
                                    && !testMethodInfo.IsGenericMethod
                                    && testMethodInfo.ReturnType == typeof(void);

            if (!isValidTestMethod)
            {
                var message = string.Format(CultureInfo.CurrentCulture, Errors.UTF_ErrorIncorrectTestMethodSignature, testMethodInfo.DeclaringType.FullName, testMethodInfo.Name);
                logger?.RecordMessage(MessageLevel.Error, message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if the type has a valid TestContext property definition.
        /// </summary>
        /// <param name="type">The reflected type.</param>
        /// <returns>Returns true if type has a valid TestContext property definition.</returns>
        internal bool HasCorrectTestContextSignature(Type type)
        {
            Debug.Assert(type != null, "HasCorrectTestContextSignature type is null");

            var propertyInfoEnumerable = type.GetProperties();
            var propertyInfo = new List<PropertyInfo>();

            foreach (var pinfo in propertyInfoEnumerable)
            {
                // PropertyType.FullName can be null if the property is a generic type.
                if (TestContextFullName.Equals(pinfo.PropertyType.FullName, StringComparison.Ordinal))
                {
                    propertyInfo.Add(pinfo);
                }
            }

            if (propertyInfo.Count == 0)
            {
                return true;
            }

            foreach (var pinfo in propertyInfo)
            {
                var setInfo = pinfo.GetSetMethod();
                if (setInfo == null)
                {
                    // we have a getter, but not a setter.
                    return false;
                }

                if (setInfo.IsPrivate || setInfo.IsStatic || setInfo.IsAbstract)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check if the method is a valid preparation method.
        /// </summary>
        /// <param name="prepMethodInfo"> Method to be checked. </param>
        /// <param name="logger"> Logs information. </param>
        /// <returns> Return true if the method is a valid preparation method. </returns>
        internal bool IsValidPrepMethod(MethodInfo prepMethodInfo, IMUnitLogger logger)
        {
            if (!_reflectionWorker.HasAttributeDerivedFrom(prepMethodInfo, typeof(SupportingAttribute), false))
            {
                return false;
            }

            bool isTestPrep = _reflectionWorker.HasAttributeIsOrDerivedFrom(prepMethodInfo, typeof(TestCleanupAttribute), false)
                           || _reflectionWorker.HasAttributeIsOrDerivedFrom(prepMethodInfo, typeof(TestInitializeAttribute), false);

            var isValidTestMethod = prepMethodInfo.IsPublic
                                    && !prepMethodInfo.IsAbstract
                                    && (prepMethodInfo.IsStatic || isTestPrep)
                                    && prepMethodInfo.GetParameters().Length == 0
                                    && prepMethodInfo.ReturnType == typeof(void);

            if (!isValidTestMethod)
            {
                string message = string.Format(
                             CultureInfo.CurrentCulture,
                             Errors.UTF_IncorrectPrepMethodSignature,
                             prepMethodInfo.DeclaringType.FullName,
                             prepMethodInfo.Name,
                             _reflectionWorker.GetAttributeAssignableTo(prepMethodInfo, typeof(SupportingAttribute), false).GetType().Name,
                             isTestPrep ? string.Empty : "static, ");

                logger?.RecordMessage(MessageLevel.Error, message);
                return false;
            }

            return isValidTestMethod;
        }

        /// <summary>
        /// Resolve parent ID for this test cycle.
        /// </summary>
        /// <param name="type"> From which this test cycle is constructed. </param>
        /// <param name="scope"> Scope of this test cycle. </param>
        /// <returns> Parent's Guid for this test cycle. </returns>
        /// <remarks> It is called in constructor. Make sure it does not depend on any class members. </remarks>
        internal Guid ResolveParentID(Type type, TestCycleScope scope)
        {
            string fullName = string.Empty;
            string fullPath = _reflectionWorker.GetAssemblyFullPath(type);

            switch (scope)
            {
                case TestCycleScope.AppDomain:
                    break;
                case TestCycleScope.Assembly:
                    fullName = AppDomain.CurrentDomain.FriendlyName;
                    break;
                case TestCycleScope.Namespace:
                    fullName = type.Assembly.FullName;
                    break;
                case TestCycleScope.Class:
                    // Namespace can be null if a type is defined outside any namespace keyword.
                    fullName = _reflectionWorker.GetNamespace(type) ?? Resources.Strings.NoNamespace;
                    break;
                case TestCycleScope.Method:
                    fullName = _reflectionWorker.GetTypeFullName(type);
                    break;
            }

            return HashUtilities.GuidForTestCycleID(fullPath, fullName);
        }

        /// <summary>
        /// Resolve full name for this test cycle.
        /// </summary>
        /// <param name="type"> From which this test cycle is constructed. </param>
        /// <param name="scope"> Scope of this test cycle. </param>
        /// <returns> Full name of this test cycle. </returns>
        internal string ResolveTestCycleFullName(Type type, TestCycleScope scope)
        {
            string fullName = string.Empty;

            switch (scope)
            {
                case TestCycleScope.AppDomain:
                    fullName = Thread.GetDomain().FriendlyName;
                    break;
                case TestCycleScope.Assembly:
                    fullName = _reflectionWorker.GetAssembly(type).FullName;
                    break;
                case TestCycleScope.Namespace:
                    // Namespace can be null if a type is defined outside any namespace keyword.
                    fullName = _reflectionWorker.GetNamespace(type) ?? Resources.Strings.NoNamespace;
                    break;
                case TestCycleScope.Class:
                    fullName = _reflectionWorker.GetTypeFullName(type);
                    break;
                case TestCycleScope.Method:
                    fullName = this.GetTestCycleNameForMethodScope(type);
                    break;
            }

            return fullName;
        }

        /// <summary>
        /// Returns a name that can be assigned to ITestCycle.FullName.
        /// </summary>
        /// <param name="type"> Type that contains this method. </param>
        /// <returns> A full name for a test cycle. </returns>
        internal string GetTestCycleNameForMethodScope(Type type)
        {
            return string.Concat(_reflectionWorker.GetTypeFullName(type), ".Methods");
        }
    }
}
