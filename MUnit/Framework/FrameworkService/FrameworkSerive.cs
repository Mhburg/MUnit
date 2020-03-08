// <copyright file="FrameworkSerive.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using MUnit.Framework.NET35;

namespace MUnit.Framework.FrameworkService
{
    /// <summary>
    /// Providing service based on language framework version.
    /// </summary>
    public static class FrameworkSerive
    {
        private static IFrameworkRelfection _reflectionWorker;

        /// <summary>
        /// Gets worker that facilitates reflection on object.
        /// </summary>
        public static IFrameworkRelfection RelfectionWoker
        {
            get
            {
                if (_reflectionWorker == null)
                {
#if NET35
                    _reflectionWorker = new N35ReflectionWorker(false);
#endif
                }

                return _reflectionWorker;
            }
        }
    }
}
