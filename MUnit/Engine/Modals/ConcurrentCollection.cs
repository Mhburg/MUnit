// <copyright file="ConcurrentCollection.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using MUnit.Utilities;

namespace MUnit.Engine
{
    /// <summary>
    /// Collection for concurrent access.
    /// </summary>
    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Not required.")]
    public abstract class ConcurrentCollection
    {
        /// <summary>
        /// Used for synchronizing access to underlying data structure.
        /// </summary>
        protected ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        /// <summary>
        /// Finalizes an instance of the <see cref="ConcurrentCollection"/> class.
        /// </summary>
        ~ConcurrentCollection()
        {
            _lock.Dispose();
        }

        /// <summary>
        /// Correspond to Try*Operation* in concurrent data structure in .net 4.0 or later.
        /// </summary>
        /// <typeparam name="T"> Generic type. </typeparam>
        /// <param name="value"> Value returned from try operation. </param>
        /// <returns> Indicates whether the operation is a success. </returns>
        protected delegate bool TryFunction<T>(out T value);

        /// <summary>
        /// Synchronize read.
        /// </summary>
        /// <typeparam name="T"> Generic type. </typeparam>
        /// <param name="func"> Function used to read. </param>
        /// <returns> Result returned from <paramref name="func"/>. </returns>
        protected T SyncRead<T>(Func<T> func)
        {
            ThrowUtilities.NullArgument(func, nameof(func));

            _lock.EnterReadLock();
            T value = func();
            _lock.ExitReadLock();
            return value;
        }

        /// <summary>
        /// Synchronize read.
        /// </summary>
        /// <typeparam name="T"> Generic type. </typeparam>
        /// <param name="tryFunc"> Delegate used for reading. </param>
        /// <param name="value"> Parameter passed to <paramref name="tryFunc"/>. </param>
        /// <returns> Returns true if <paramref name="tryFunc"/> is a success. </returns>
        protected bool SyncRead<T>(TryFunction<T> tryFunc, out T value)
        {
            ThrowUtilities.NullArgument(tryFunc, nameof(tryFunc));

            _lock.EnterReadLock();
            var success = tryFunc(out value);
            _lock.ExitReadLock();
            return success;
        }

        /// <summary>
        /// Synchronize read.
        /// </summary>
        /// <param name="action"> Action to perform on underlying data structure. </param>
        protected void SyncRead(Action action)
        {
            ThrowUtilities.NullArgument(action, nameof(action));

            _lock.EnterReadLock();
            action();
            _lock.ExitReadLock();
        }

        /// <summary>
        /// Synchronize write.
        /// </summary>
        /// <typeparam name="T"> Generic type. </typeparam>
        /// <param name="tryFunc"> Functions used for writing. </param>
        /// <param name="value"> An out parameter passed to <paramref name="tryFunc"/>. </param>
        /// <returns> Returns true if <paramref name="tryFunc"/> is a success. </returns>
        protected bool SyncWrite<T>(TryFunction<T> tryFunc, out T value)
        {
            ThrowUtilities.NullArgument(tryFunc, nameof(tryFunc));

            _lock.EnterWriteLock();
            var success = tryFunc(out value);
            _lock.ExitWriteLock();
            return success;
        }

        /// <summary>
        /// Synchronize write.
        /// </summary>
        /// <param name="action"> Action to perform on underlying data structure. </param>
        protected void SyncWrite(Action action)
        {
            ThrowUtilities.NullArgument(action, nameof(action));

            _lock.EnterWriteLock();
            action();
            _lock.ExitWriteLock();
        }

        /// <summary>
        /// Synchronize write.
        /// </summary>
        /// <typeparam name="T"> Generic type. </typeparam>
        /// <param name="func"> Functions used for writing. </param>
        /// <returns> Result returned by <paramref name="func"/>. </returns>
        protected T SyncWrite<T>(Func<T> func)
        {
            ThrowUtilities.NullArgument(func, nameof(func));

            _lock.EnterWriteLock();
            T value = func();
            _lock.ExitWriteLock();
            return value;
        }
    }
}
