// <copyright file="MUnitAttributes.cs" company="Zizhen Li">
// Copyright (c) 2020 Microsoft Corporation. All rights reserved.
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// Licensed under the MIT license. See LICENSE.Microsoft.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using MUnit.Framework;
using MUnit.Utilities;

namespace MUnit
{

#pragma warning disable SA1402 // FileMayOnlyContainASingleType
#pragma warning disable SA1649 // SA1649FileNameMustMatchTypeName

    /// <summary>
    /// Enumeration for timeouts, that can be used with the <see cref="TimeoutAttribute"/> class.
    /// The type of the enumeration must match.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "Compat reasons")]
    public enum TestTimeout
    {
        /// <summary>
        /// The infinite.
        /// </summary>
        Infinite = int.MaxValue,
    }

    /// <summary>
    /// Enumeration for inheritance behavior, that can be used with both the <see cref="ClassInitializeAttribute"/> class
    /// and <see cref="ClassCleanupAttribute"/> class.
    /// Defines the behavior of the ClassInitialize and ClassCleanup methods of base classes.
    /// The type of the enumeration must match.
    /// </summary>
    public enum InheritanceBehavior
    {
        /// <summary>
        /// None.
        /// </summary>
        None,

        /// <summary>
        /// Before each derived class.
        /// </summary>
        BeforeEachDerivedClass,
    }

    /// <summary>
    /// Type of a method in a test cycle.
    /// </summary>
    public enum TestCycleMethodType
    {
        /// <summary>
        /// Initialize test conditions.
        /// </summary>
        Initialize,

        /// <summary>
        /// Clean up lasting effects from testing.
        /// </summary>
        Cleanup,

        /// <summary>
        /// Method for test.
        /// </summary>
        TestMethod,
    }

    /// <summary>
    /// The test class attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TestClassAttribute : Attribute
    {
        /// <summary>
        /// Gets a test method attribute that enables running this test.
        /// </summary>
        /// <param name="testMethodAttribute">The test method attribute instance defined on this method.</param>
        /// <returns>The <see cref="TestMethodAttribute"/> to be used to run this test.</returns>
        /// <remarks>Extensions can override this method to customize how all methods in a class are run.</remarks>
        public virtual TestMethodAttribute GetTestMethodAttribute(TestMethodAttribute testMethodAttribute)
        {
            // If TestMethod is not extended by derived class then return back the original TestMethodAttribute
            return testMethodAttribute;
        }
    }

    /// <summary>
    /// The test method attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TestMethodAttribute : Attribute, IExecutor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestMethodAttribute"/> class.
        /// </summary>
        public TestMethodAttribute()
        : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestMethodAttribute"/> class.
        /// </summary>
        /// <param name="displayName">
        /// Message specifies reason for ignoring.
        /// </param>
        public TestMethodAttribute(string displayName)
        {
            this.DisplayName = displayName;
        }

        public TestCycleScope Scope { get; set; } = TestCycleScope.Method;

        public string DisplayName { get; private set; }

        public IDataSource DataSource { get; set; }

        public ITestMethodContext ExecuteContext { get; set; }

        /// <summary>
        /// Executes a test method.
        /// </summary>
        /// <param name="testMethodContext">The test method to execute.</param>
        /// <returns>An array of TestResult objects that represent the outcome(s) of the test.</returns>
        /// <remarks>Extensions can override this method to customize running a TestMethod.</remarks>
        public virtual IEnumerable<TestResult> Execute(ITestMethodContext testMethodContext)
        {
            ThrowUtilities.NullArgument(testMethodContext, nameof(testMethodContext));

            return testMethodContext.Invoke();
        }
    }

    /// <summary>
    /// The test initialize attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class TestInitializeAttribute : SupportingAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestInitializeAttribute"/> class.
        /// </summary>
        public TestInitializeAttribute()
        {
            this.Scope = TestCycleScope.Method;
        }

        /// <inheritdoc/>
        public override TestCycleMethodType PreparationType => TestCycleMethodType.Initialize;

        /// <inheritdoc/>
        public override void Register(ITestCycle testCycle, MethodInfo method)
        {
            Register(testCycle, TestCycleMethodType.Initialize, method);
        }
    }

    /// <summary>
    /// The test cleanup attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class TestCleanupAttribute : SupportingAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestCleanupAttribute"/> class.
        /// </summary>
        public TestCleanupAttribute()
        {
            this.Scope = TestCycleScope.Method;
        }

        /// <inheritdoc/>
        public override TestCycleMethodType PreparationType => TestCycleMethodType.Cleanup;

        /// <inheritdoc/>
        public override void Register(ITestCycle testCycle, MethodInfo method)
        {
            Register(testCycle, TestCycleMethodType.Cleanup, method);
        }
    }

    /// <summary>
    /// The ignore attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class IgnoreAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IgnoreAttribute"/> class.
        /// </summary>
        public IgnoreAttribute()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IgnoreAttribute"/> class.
        /// </summary>
        /// <param name="message">
        /// Message specifies reason for ignoring.
        /// </param>
        public IgnoreAttribute(string message)
        {
            this.IgnoreMessage = message;
        }

        /// <summary>
        /// Gets the owner.
        /// </summary>
        public string IgnoreMessage { get; private set; }
    }

    /// <summary>
    /// The test property attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TestPropertyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestPropertyAttribute"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        public TestPropertyAttribute(string name, string value)
        {
            // NOTE : DONT THROW EXCEPTIONS FROM HERE IT WILL CRASH GetCustomAttributes() call
            this.Name = name;
            this.Value = value;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public string Value { get; }
    }

    /// <summary>
    /// The class initialize attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ClassInitializeAttribute : SupportingAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassInitializeAttribute"/> class.
        /// ClassInitializeAttribute.
        /// </summary>
        public ClassInitializeAttribute()
            : this(InheritanceBehavior.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassInitializeAttribute"/> class.
        /// ClassInitializeAttribute.
        /// </summary>
        /// <param name="inheritanceBehavior">
        /// Specifies the ClassInitialize Inheritance Behavior.
        /// </param>
        public ClassInitializeAttribute(InheritanceBehavior inheritanceBehavior)
        {
            this.InheritanceBehavior = inheritanceBehavior;
            this.Scope = TestCycleScope.Class;
        }

        /// <summary>
        /// Gets the Inheritance Behavior.
        /// </summary>
        public InheritanceBehavior InheritanceBehavior { get; private set; }

        /// <inheritdoc/>
        public override TestCycleMethodType PreparationType => TestCycleMethodType.Initialize;

        /// <inheritdoc/>
        public override void Register(ITestCycle testCycle, MethodInfo method)
        {
            Register(testCycle, TestCycleMethodType.Initialize, method);
        }
    }

    /// <summary>
    /// The class cleanup attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ClassCleanupAttribute : SupportingAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassCleanupAttribute"/> class.
        /// ClassCleanupAttribute.
        /// </summary>
        public ClassCleanupAttribute()
            : this(InheritanceBehavior.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassCleanupAttribute"/> class.
        /// ClassCleanupAttribute.
        /// </summary>
        /// <param name="inheritanceBehavior">
        /// Specifies the ClassCleanup Inheritance Behavior.
        /// </param>
        public ClassCleanupAttribute(InheritanceBehavior inheritanceBehavior)
        {
            this.InheritanceBehavior = inheritanceBehavior;
            this.Scope = TestCycleScope.Class;
        }

        /// <summary>
        /// Gets the Inheritance Behavior.
        /// </summary>
        public InheritanceBehavior InheritanceBehavior { get; private set; }

        /// <inheritdoc/>
        public override TestCycleMethodType PreparationType => TestCycleMethodType.Cleanup;

        /// <inheritdoc/>
        public override void Register(ITestCycle testCycle, MethodInfo method)
        {
            Register(testCycle, TestCycleMethodType.Cleanup, method);
        }
    }

    /// <summary>
    /// Base for derived supporting attributes, e.g., AssemblyInitializeAttribute and AssmeblyCleanupAttribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public abstract class SupportingAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets scope for this preparation attribute.
        /// </summary>
        public TestCycleScope Scope { get; set; }

        /// <summary>
        /// Gets preparation type of this attribute.
        /// </summary>
        public abstract TestCycleMethodType PreparationType { get; }

        /// <summary>
        /// Register the method adorned with this attribute to test cycle.
        /// </summary>
        /// <param name="testCycle">In which the underlying method belongs.</param>
        /// <param name="method">Method to register.</param>
        public abstract void Register(ITestCycle testCycle, MethodInfo method);

        /// <summary>
        /// Register the method adorned with this attribute to test cycle.
        /// </summary>
        /// <param name="testCycle">In which the underlying method belongs.</param>
        /// <param name="pType">Preparation type of a method.</param>
        /// <param name="method">Method to register.</param>
        protected static void Register(ITestCycle testCycle, TestCycleMethodType pType, MethodInfo method)
        {
            ThrowUtilities.NullArgument(testCycle, nameof(testCycle));
            ThrowUtilities.NullArgument(method, nameof(method));

            SupportMethodsGroup group = testCycle.SupportMethodsGroups
                .FirstOrDefault(g => g.DeclaringType == method.DeclaringType);

            if (group == default)
            {
                group = new SupportMethodsGroup(method.DeclaringType);
                testCycle.SupportMethodsGroups.Add(group);
            }

            if (pType == TestCycleMethodType.Initialize)
            {
                // It will overwrite previously found methods in the same type, if there is any.
                group.InitializeMethod = method;
            }
            else if (pType == TestCycleMethodType.Cleanup)
            {
                group.CleanupMethod = method;
            }
        }
    }

    /// <summary>
    /// Assembly initialize attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class AssemblyInitializeAttribute : SupportingAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyInitializeAttribute"/> class.
        /// </summary>
        public AssemblyInitializeAttribute()
        {
            this.Scope = TestCycleScope.Assembly;
        }

        public override TestCycleMethodType PreparationType => TestCycleMethodType.Initialize;

        /// <inheritdoc/>
        public override void Register(ITestCycle testCycle, MethodInfo method)
        {
            Register(testCycle, TestCycleMethodType.Initialize, method);
        }
    }

    /// <summary>
    /// Assembly cleanup attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class AssemblyCleanupAttribute : SupportingAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyCleanupAttribute"/> class.
        /// </summary>
        public AssemblyCleanupAttribute()
        {
            this.Scope = TestCycleScope.Assembly;
        }

        /// <inheritdoc/>
        public override TestCycleMethodType PreparationType => TestCycleMethodType.Cleanup;

        /// <inheritdoc/>
        public override void Register(ITestCycle testCycle, MethodInfo method)
        {
            Register(testCycle, TestCycleMethodType.Cleanup, method);
        }
    }

    /// <summary>
    /// Namespace initialize attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class NamespaceInitializeAttribute : SupportingAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NamespaceInitializeAttribute"/> class.
        /// </summary>
        public NamespaceInitializeAttribute()
        {
            this.Scope = TestCycleScope.Namespace;
        }

        /// <inheritdoc/>
        public override TestCycleMethodType PreparationType => TestCycleMethodType.Initialize;

        /// <inheritdoc/>
        public override void Register(ITestCycle testCycle, MethodInfo method)
        {
            Register(testCycle, TestCycleMethodType.Initialize, method);
        }
    }

    /// <summary>
    /// Namespace cleanup attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class NamespaceCleanupAttribute : SupportingAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NamespaceCleanupAttribute"/> class.
        /// </summary>
        public NamespaceCleanupAttribute()
        {
            this.Scope = TestCycleScope.Namespace;
        }

        /// <inheritdoc/>
        public override TestCycleMethodType PreparationType => TestCycleMethodType.Cleanup;

        /// <inheritdoc/>
        public override void Register(ITestCycle testCycle, MethodInfo method)
        {
            Register(testCycle, TestCycleMethodType.Cleanup, method);
        }
    }

    /// <summary>
    /// Test Owner.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class OwnerAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OwnerAttribute"/> class.
        /// </summary>
        /// <param name="owner">
        /// The owner.
        /// </param>
        public OwnerAttribute(string owner)
        {
            this.Owner = owner;
        }

        /// <summary>
        /// Gets the owner.
        /// </summary>
        public string Owner { get; }
    }

    /// <summary>
    /// Priority attribute; used to specify the priority of a unit test.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class PriorityAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PriorityAttribute"/> class.
        /// </summary>
        /// <param name="priority">
        /// The priority.
        /// </param>
        public PriorityAttribute(int priority)
        {
            this.Priority = priority;
        }

        /// <summary>
        /// Gets the priority.
        /// </summary>
        public int Priority { get; }
    }

    /// <summary>
    /// Description of the test.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class DescriptionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DescriptionAttribute"/> class to describe a test.
        /// </summary>
        /// <param name="description">The description.</param>
        public DescriptionAttribute(string description)
        {
            this.Description = description;
        }

        /// <summary>
        /// Gets the description of a test.
        /// </summary>
        public string Description { get; private set; }
    }

    /// <summary>
    /// CSS Project Structure URI.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class CssProjectStructureAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CssProjectStructureAttribute"/> class for CSS Project Structure URI.
        /// </summary>
        /// <param name="cssProjectStructure">The CSS Project Structure URI.</param>
        public CssProjectStructureAttribute(string cssProjectStructure)
        {
            this.CssProjectStructure = cssProjectStructure;
        }

        /// <summary>
        /// Gets the CSS Project Structure URI.
        /// </summary>
        public string CssProjectStructure { get; private set; }
    }

    /// <summary>
    /// CSS Iteration URI.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class CssIterationAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CssIterationAttribute"/> class for CSS Iteration URI.
        /// </summary>
        /// <param name="cssIteration">The CSS Iteration URI.</param>
        public CssIterationAttribute(string cssIteration)
        {
            this.CssIteration = cssIteration;
        }

        /// <summary>
        /// Gets the CSS Iteration URI.
        /// </summary>
        public string CssIteration { get; private set; }
    }

    /// <summary>
    /// WorkItem attribute; used to specify a work item associated with this test.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class WorkItemAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkItemAttribute"/> class for the WorkItem Attribute.
        /// </summary>
        /// <param name="id">The Id to a work item.</param>
        public WorkItemAttribute(int id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets the Id to a workitem associated.
        /// </summary>
        public int Id { get; private set; }
    }

    /// <summary>
    /// Timeout attribute; used to specify the timeout of a unit test.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class TimeoutAttribute : Attribute
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeoutAttribute"/> class.
        /// </summary>
        /// <param name="timeout">
        /// The timeout in milliseconds.
        /// </param>
        public TimeoutAttribute(int timeout)
        {
            this.Timeout = timeout;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeoutAttribute"/> class with a preset timeout.
        /// </summary>
        /// <param name="timeout">
        /// The timeout.
        /// </param>
        public TimeoutAttribute(TestTimeout timeout)
        {
            this.Timeout = (int)timeout;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the timeout in milliseconds.
        /// </summary>
        public int Timeout { get; }

        #endregion
    }

    /// <summary>
    /// Method that provides data to drive tests.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class DataMethodAttribute : Attribute, IDataProvidingMethod
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataMethodAttribute"/> class.
        /// </summary>
        /// <param name="declaringType"> Where <paramref name="methodName"/> is defined. </param>
        /// <param name="methodName"> Name of the method. </param>
        public DataMethodAttribute(Type declaringType, string methodName)
        {
            this.DeclaringType = declaringType;
            this.MethodName = methodName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataMethodAttribute"/> class.
        /// </summary>
        /// <param name="methodName"> Name of the method. </param>
        public DataMethodAttribute(string methodName)
        {
            this.MethodName = methodName;
        }

        /// <summary>
        /// Gets or sets type in which the data providing method is declared.
        /// </summary>
        public Type DeclaringType { get; set; }

        /// <summary>
        /// Gets or sets name of the method.
        /// </summary>
        public string MethodName { get; set; }

        /// <inheritdoc/>
        public IEnumerable<object[]> GetData()
        {
            object data = this.DeclaringType.GetMethod(this.MethodName, BindingFlags.Public | BindingFlags.Static).Invoke(null, null);

            if (data == null)
            {
                throw new ArgumentNullException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.Errors.UTF_DataMethodReturnsNull,
                        this.DeclaringType.FullName,
                        this.MethodName));
            }

            if (!(data is IEnumerable<object[]> enumerable))
            {
                throw new ArgumentNullException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.Errors.UTF_DataMethodNullIEnumberable,
                        this.DeclaringType.FullName,
                        this.MethodName));
            }
            else if (!enumerable.Any())
            {
                throw new ArgumentNullException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.Errors.UTF_DataMethodEmptyIEnumberable,
                        this.DeclaringType.FullName,
                        this.MethodName));
            }

            return enumerable;
        }
    }
#pragma warning restore SA1402 // FileMayOnlyContainASingleType
#pragma warning restore SA1649 // SA1649FileNameMustMatchTypeName
}
