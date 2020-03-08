// <copyright file="Assert.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using MUnit.Resources;

namespace MUnit
{
    /// <summary>
    /// Helper class that produces test results.
    /// </summary>
    public static class Assert
    {
        /// <summary>
        /// Throw <see cref="AssertException"/> if <paramref name="actual"/> is not equal to <paramref name="expected"/>.
        /// </summary>
        /// <typeparam name="T"> Can be any generic type. </typeparam>
        /// <param name="actual"> Value produced by test. </param>
        /// <param name="expected"> Expected value from test. </param>
        /// <param name="nameOfActual"> Display name for <paramref name="actual"/>. </param>
        /// <remarks> It calls <see cref="EqualityComparer{T}.Default"/> for comparison. </remarks>
        public static void Expect<T>(T actual, T expected, string nameOfActual)
        {
            bool result;
            try
            {
                result = EqualityComparer<T>.Default.Equals(actual, expected);
            }
            catch (Exception)
            {
                throw;
            }

            if (result)
            {
                return;
            }
            else
            {
                throw new AssertException(string.Format(
                    CultureInfo.CurrentCulture,
                    AssertString.ExpectedString,
                    nameOfActual,
                    expected.ToString(),
                    actual.ToString()));
            }
        }

        /// <summary>
        /// Throw <see cref="AssertException"/> if <paramref name="objectA"/> is not equal to <paramref name="objectB"/>.
        /// </summary>
        /// <typeparam name="T"> Can be any generic type. </typeparam>
        /// <param name="objectA"> Any object. </param>
        /// <param name="objectB"> Another object. </param>
        /// <param name="nameA"> Display name for <paramref name="objectA"/>. </param>
        /// <param name="nameB"> Display name for <paramref name="objectB"/>. </param>
        public static void AreEqual<T>(T objectA, T objectB, string nameA, string nameB)
        {
            bool result;
            try
            {
                result = EqualityComparer<T>.Default.Equals(objectA, objectB);
            }
            catch (Exception)
            {
                throw;
            }

            if (result)
            {
                return;
            }
            else
            {
                throw new AssertException(string.Format(
                    CultureInfo.CurrentCulture,
                    AssertString.ObjectsAreNotEqual,
                    nameA,
                    objectA.ToString(),
                    nameB,
                    objectB.ToString()));
            }
        }
    }
}
