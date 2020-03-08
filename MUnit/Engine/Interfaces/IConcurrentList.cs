// <copyright file="IConcurrentList.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;

namespace MUnit.Engine
{
    /// <summary>
    /// List for concurrent access.
    /// </summary>
    /// <typeparam name="TValue"> Generic type parameter. </typeparam>
    internal interface IConcurrentList<TValue>
    {
        /// <summary>
        /// Gets a value indicating whether there is any element in the list.
        /// </summary>
        bool Any { get; }

        /// <summary>
        /// Gets count of items in the list.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Add <paramref name="value"/> to list.
        /// </summary>
        /// <param name="value"> Item to add. </param>
        void Add(TValue value);

        /// <summary>
        /// Perform a read <paramref name="action"/> on all list items.
        /// </summary>
        /// <param name="action"> Function to act on items in list. </param>
        void EnumerateRead(Action<TValue> action);

        /// <summary>
        /// Perform a write <paramref name="action"/> on all list items.
        /// </summary>
        /// <param name="action"> Function to act on items in list. </param>
        void EnumerateWrite(Action<TValue> action);

        /// <summary>
        /// Try to remove the first item that meets <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate"> Function acts as predication. </param>
        /// <param name="value"> Item removed from list. Returns default value of <typeparamref name="TValue"/> if no item is found. </param>
        /// <returns> Returns true if any item meets <paramref name="predicate"/>. </returns>
        bool TryRemove(Predicate<TValue> predicate, out TValue value);
    }
}