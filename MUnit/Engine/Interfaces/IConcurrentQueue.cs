// <copyright file="IConcurrentQueue.cs" company="Zizhen Li">
// Copyright (c) Zizhen Li. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
// </copyright>

using System;

namespace MUnit.Engine
{
    /// <summary>
    /// Queue data structure for concurrent access.
    /// </summary>
    /// <typeparam name="TValue"> Type of stored item. </typeparam>
    public interface IConcurrentQueue<TValue>
    {
        /// <summary>
        /// Gets number of items in queue.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets a value indicating whether there is any item in queue.
        /// </summary>
        bool Any { get; }

        /// <summary>
        /// Remove an object from the begining of the queue.
        /// </summary>
        /// <param name="value"> Retrieved value if operation is a success. </param>
        /// <returns> Returns true if operation is a success. </returns>
        bool TryDequeue(out TValue value);

        /// <summary>
        /// Adds an object to the end of queue.
        /// </summary>
        /// <param name="value"> Object to add to the queue. </param>
        void Enqueue(TValue value);

        /// <summary>
        ///  Returns the object at the beginning of the queue
        ///  without removing it.
        /// </summary>
        /// <param name="value"> Retrieved value if operation is a success. </param>
        /// <returns> Returns true if operation is a success. </returns>
        bool TryPeek(out TValue value);

        /// <summary>
        /// Enumerate items for reading.
        /// </summary>
        /// <param name="action"> Action to perform on item. </param>
        void EnumerateRead(Action<TValue> action);

        /// <summary>
        /// Enumerate items for writing.
        /// </summary>
        /// <param name="action"> Action to perform on item. </param>
        void EnumerateWrite(Action<TValue> action);
    }
}