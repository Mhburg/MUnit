// <copyright file="N35ConcurrentQueue{TValue}.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MUnit.Engine
{
    /// <summary>
    /// A naive implementation for a concurrent queue.
    /// </summary>
    /// <typeparam name="TValue"> Type of stored objects. </typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Not necessary for a lock.")]
    internal class N35ConcurrentQueue<TValue> : ConcurrentCollection, IConcurrentQueue<TValue>
    {
        private Queue<TValue> _queue = new Queue<TValue>();

        /// <inheritdoc/>
        public int Count => this.SyncRead(() => _queue.Count);

        /// <inheritdoc/>
        public bool Any => this.SyncRead(() => _queue.Any());

        /// <inheritdoc/>
        public void Enqueue(TValue value)
        {
            this.SyncWrite(() => _queue.Enqueue(value));
        }

        /// <inheritdoc/>
        public bool TryDequeue(out TValue value)
        {
            return this.SyncWrite(
                (out TValue v) =>
                {
                    if (_queue.Any())
                    {
                        v = _queue.Dequeue();
                        return true;
                    }

                    v = default;
                    return false;
                }, out value);
        }

        /// <inheritdoc/>
        public bool TryPeek(out TValue value)
        {
            return this.SyncRead(
                (out TValue v) =>
                {
                    if (_queue.Any())
                    {
                        v = _queue.Peek();
                        return true;
                    }

                    v = default;
                    return false;
                }, out value);
        }

        /// <inheritdoc/>
        public void EnumerateRead(Action<TValue> action)
        {
            _lock.EnterReadLock();
            foreach (TValue v in _queue)
            {
                action(v);
            }

            _lock.ExitReadLock();
        }

        /// <inheritdoc/>
        public void EnumerateWrite(Action<TValue> action)
        {
            _lock.EnterWriteLock();
            foreach (TValue v in _queue)
            {
                action(v);
            }

            _lock.ExitWriteLock();
        }
    }
}
