// <copyright file="DiaSessionCache.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Navigation;

namespace MUnitTestAdapter
{
    /// <summary>
    /// Cache for <see cref="DiaSession"/>.
    /// </summary>
    public static class DiaSessionCache
    {
        private static Dictionary<string, DiaSession> _cache = new Dictionary<string, DiaSession>();

        /// <summary>
        /// Populate cache with <paramref name="source"/>.
        /// </summary>
        /// <param name="source"> Source to cache. </param>
        public static void PopulateCache(string source)
        {
            ValidateArg.NotNullOrEmpty(source, nameof(source));

            _cache[source] = new DiaSession(source);
        }

        /// <summary>
        /// Get <see cref="INavigationData"/> for <paramref name="methodName"/>.
        /// </summary>
        /// <param name="source"> Source of <paramref name="declaringTypeName"/>. </param>
        /// <param name="declaringTypeName"> Name of declaring type for <paramref name="methodName"/>. </param>
        /// <param name="methodName"> Name of the method. </param>
        /// <returns> Navigation data for <paramref name="methodName"/>. </returns>
        public static INavigationData GetNavDataForMethod(string source, string declaringTypeName, string methodName)
        {
            ValidateArg.NotNullOrEmpty(source, nameof(source));
            ValidateArg.NotNullOrEmpty(declaringTypeName, nameof(declaringTypeName));
            ValidateArg.NotNullOrEmpty(methodName, nameof(methodName));

            if (_cache.TryGetValue(source, out DiaSession diaSession))
            {
                return diaSession.GetNavigationDataForMethod(declaringTypeName, methodName);
            }
            else
            {
                return null;
            }
        }
    }
}
