// <copyright file="N35ConcurrentList.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUnit.Engine
{
    /// <summary>
    /// A naive implementation of a concurrent list for .net 3.5.
    /// </summary>
    /// <typeparam name="TValue"> Generic type. </typeparam>
    internal class N35ConcurrentList<TValue> : ConcurrentCollection, IConcurrentList<TValue>
    {
        private List<TValue> _list = new List<TValue>();

        /// <inheritdoc/>
        public int Count => this.SyncRead(() => _list.Count);

        /// <inheritdoc/>
        public bool Any => this.SyncRead(() => _list.Any());

        /// <summary>
        /// Try to remove the first element that meets the <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate"> To filter elements. </param>
        /// <param name="value"> Element that fits the <paramref name="predicate"/>. Returns default value, if not found. </param>
        /// <returns> Returns true if element is found. </returns>
        public bool TryRemove(Predicate<TValue> predicate, out TValue value)
        {
            return this.SyncWrite(
                (out TValue result) =>
                {
                    int index = _list.FindIndex(predicate);
                    if (index == -1)
                    {
                        result = default;
                        return false;
                    }
                    else
                    {
                        result = _list[index];
                        _list.RemoveAt(index);
                        return true;
                    }
                }, out value);
        }

        /// <summary>
        /// Add item to list.
        /// </summary>
        /// <param name="value"> Item to add. </param>
        public void Add(TValue value)
        {
            this.SyncWrite(
                () =>
                {
                    _list.Add(value);
                });
        }

        /// <inheritdoc/>
        public void EnumerateRead(Action<TValue> action)
        {
            _lock.EnterReadLock();
            foreach (TValue v in _list)
            {
                action(v);
            }

            _lock.ExitReadLock();
        }

        /// <inheritdoc/>
        public void EnumerateWrite(Action<TValue> action)
        {
            _lock.EnterWriteLock();
            foreach (TValue v in _list)
            {
                action(v);
            }

            _lock.ExitWriteLock();
        }
    }
}
