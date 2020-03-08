// <copyright file="TestCycle.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using MUnit.Framework;
using MUnit.Framework.Base;

namespace MUnit.Engine
{
    /// <summary>
    /// A test cycle that sets up, cleans up and run tests for a specific scope in code.
    /// </summary>
    public class TestCycle : TestCycleBase
    {
        private ReflectionHelper _reflectionHelper = new ReflectionHelper(PlatformService.GetServiceManager().ReflectionCache);

        /// <inheritdoc/>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1648:inheritdoc should be used with inheriting class", Justification = "Bug of style cop.")]
        public TestCycle(string source, Type type, TestCycleScope scope)
            : base(source, type, scope)
        {
        }

        /// <inheritdoc/>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1648:inheritdoc should be used with inheriting class", Justification = "Bug of style cop.")]
        public TestCycle(string source, Type type, Guid parentID, TestCycleScope scope)
            : base(source, type, parentID, scope)
        {
        }

        /// <inheritdoc/>
        public override string ResolveTestCycleFullName(Type type, TestCycleScope scope)
        {
            return _reflectionHelper.ResolveTestCycleFullName(type, scope);
        }

        /// <inheritdoc/>
        public override Guid ResovleParentID(Type type, TestCycleScope scope)
        {
            return _reflectionHelper.ResolveParentID(type, scope);
        }
    }
}
