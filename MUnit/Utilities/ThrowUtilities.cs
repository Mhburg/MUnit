// <copyright file="ThrowUtilities.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MUnit.Utilities
{
    /// <summary>
    /// Utitlies that help facilitate throwing exceptions.
    /// </summary>
    public static class ThrowUtilities
    {
        /// <summary>
        /// Throw if argument is null.
        /// </summary>
        /// <param name="arg">Argument to check.</param>
        /// <param name="argName">Name of <paramref name="arg"/>.</param>
        /// <exception cref="ArgumentNullException">Argument is null.</exception>
        public static void NullArgument(object arg, string argName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argName);
            }
        }

        /// <summary>
        /// Throw exception if <paramref name="arg"/> is null or empty.
        /// </summary>
        /// <param name="arg">Argument to check.</param>
        /// <param name="argName">Name of <paramref name="arg"/>.</param>
        /// <exception cref="Exception">Argument is null or empty.</exception>
        public static void NullOrEmpty(string arg, string argName)
        {
            if (string.IsNullOrEmpty(arg))
            {
                throw new Exception(string.Format(CultureInfo.CurrentCulture, Resources.Errors.Arg_IsNullOrEmpty, argName));
            }
        }

        /// <summary>
        /// Throw exception if <paramref name="arg"/> is null or empty.
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="arg">Argument to check.</param>
        /// <param name="argName">Name of <paramref name="arg"/>.</param>
        /// <exception cref="Exception">Argument is null or empty.</exception>
        public static void NullOrEmpty<T>(ICollection<T> arg, string argName)
        {
            if (arg == null || !arg.Any())
            {
                throw new Exception(string.Format(CultureInfo.CurrentCulture, Resources.Errors.Arg_IsNullOrEmpty, argName));
            }
        }

        /// <summary>
        /// Throw <see cref="Exception"/> if <paramref name="member"/> is null.
        /// </summary>
        /// <param name="member">Member to check.</param>
        /// <param name="className">Name of the class.</param>
        /// <param name="memberName">Name of the member.</param>
        public static void NullMember(object member, string className, string memberName)
        {
            if (member == null)
            {
                throw new Exception(string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.Errors.Arg_IsNullOrEmpty,
                    className + "." + memberName));
            }
        }
    }
}
