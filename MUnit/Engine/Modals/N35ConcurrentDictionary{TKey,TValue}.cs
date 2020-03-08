// <copyright file="N35ConcurrentDictionary{TKey,TValue}.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

#if NET35
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace MUnit.Engine
{
    /// <summary>
    /// A naive implementation for a concurrent dictionary.
    /// </summary>
    /// <typeparam name="TKey"> Key for hash table. </typeparam>
    /// <typeparam name="TValue"> Value to store. </typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Not necessary.")]
    internal class N35ConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private Dictionary<TKey, TValue> _internalDic = new Dictionary<TKey, TValue>();

        /// <inheritdoc/>
        ICollection<TKey> IDictionary<TKey, TValue>.Keys => this.SyncRead(() => _internalDic.Keys);

        /// <inheritdoc/>
        ICollection<TValue> IDictionary<TKey, TValue>.Values => this.SyncRead(() => _internalDic.Values);

        /// <inheritdoc/>
        public int Count => this.SyncRead(() => _internalDic.Count);

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <inheritdoc/>
        public TValue this[TKey key]
        {
            get => this.SyncRead(() => _internalDic[key]);
            set => this.SyncWrite(() => _internalDic[key] = value);
        }

        /// <inheritdoc/>
        public void Add(TKey key, TValue value)
        {
            this.SyncWrite(() => _internalDic.Add(key, value));
        }

        /// <inheritdoc/>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.SyncWrite(() => _internalDic.Add(item.Key, item.Value));
        }

        /// <inheritdoc/>
        public void Clear()
        {
            this.SyncWrite(() => _internalDic.Clear());
        }

        /// <inheritdoc/>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return this.SyncRead(() => _internalDic.ContainsKey(item.Key));
        }

        /// <inheritdoc/>
        public bool ContainsKey(TKey key)
        {
            return this.SyncRead(() => _internalDic.ContainsKey(key));
        }

        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            this.SyncRead(() =>
            {
                if (_internalDic.Count + arrayIndex > array.Length)
                    throw new ArgumentException("Number of available elements is more than available space.");

                if (arrayIndex < 0)
                    throw new ArgumentException(nameof(arrayIndex) + " is less than 0");

                if (arrayIndex > array.Length - 1)
                    throw new ArgumentException(nameof(arrayIndex) + " is out of range.");

                foreach (var pair in _internalDic)
                {
                    array[arrayIndex++] = pair;
                }
            });
        }

        /// <inheritdoc/>
        public bool Remove(TKey key)
        {
            return this.SyncWrite(() => _internalDic.Remove(key));
        }

        /// <inheritdoc/>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return this.SyncWrite(() => _internalDic.Remove(item.Key));
        }

        /// <inheritdoc/>
        public bool TryGetValue(TKey key, out TValue value)
        {
            _lock.EnterReadLock();
            var success = _internalDic.TryGetValue(key, out value);
            _lock.ExitReadLock();
            return success;
        }

        /// <summary>
        /// Enumerate logs in a thread-safe context.
        /// </summary>
        /// <param name="action"> Action to take on each log. </param>
        public void EnumerateRead(Action<KeyValuePair<TKey, TValue>> action)
        {
            this.SyncRead(() =>
            {
                foreach (var pair in _internalDic)
                {
                    action(pair);
                }
            });
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.SyncRead(() => _internalDic.GetEnumerator());
        }

        /// <inheritdoc/>
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return this.SyncRead(() => _internalDic.GetEnumerator());
        }

        private T SyncRead<T>(Func<T> func)
        {
            _lock.EnterReadLock();
            T value = func();
            _lock.ExitReadLock();
            return value;
        }

        private void SyncRead(Action action)
        {
            _lock.EnterReadLock();
            action();
            _lock.ExitReadLock();
        }

        private void SyncWrite(Action action)
        {
            _lock.EnterWriteLock();
            action();
            _lock.ExitWriteLock();
        }

        private T SyncWrite<T>(Func<T> func)
        {
            _lock.EnterWriteLock();
            T value = func();
            _lock.ExitWriteLock();
            return value;
        }
    }
}
#endif