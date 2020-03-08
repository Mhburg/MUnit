// <copyright file="PlatformService.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

#if NET35
using MUnit.Engine.Net35;
#endif

namespace MUnit.Engine
{
    /// <summary>
    /// Return service mangaer based on the source.
    /// </summary>
    public static class PlatformService
    {
        private static IServiceManager net35ServiceManager;

        /// <summary>
        /// <para>Get service manager based on the framework/standard/core in use.</para>
        /// <para>Rule of thumb is use as high the version as the underlying runtime allows.</para>
        /// </summary>
        /// <returns>Service manager that provides many fundamental services.</returns>
        public static IServiceManager GetServiceManager()
        {
#if NET35
            return net35ServiceManager ?? (net35ServiceManager = new Net35ServiceManager());
#endif
        }
    }
}
