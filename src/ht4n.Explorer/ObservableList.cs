/** -*- C# -*-
 * Copyright (C) 2010-2014 Thalmann Software & Consulting, http://www.softdev.ch
 *
 * This file is part of ht4n.
 *
 * ht4n is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 3
 * of the License, or any later version.
 *
 * Hypertable is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA
 * 02110-1301, USA.
 */
namespace Hypertable.Explorer
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Windows.Threading;

    /// <summary>
    /// A thread-safe observable list.
    /// </summary>
    internal sealed class ObservableList<T> : IList<T>, INotifyCollectionChanged
    {
        #region Fields

        /// <summary>
        /// The dispatcher.
        /// </summary>
        private readonly Dispatcher dispatcher;

        /// <summary>
        /// The list.
        /// </summary>
        private readonly List<T> list = new List<T>();

        /// <summary>
        /// The sync root.
        /// </summary>
        private readonly object syncRoot = new object();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableList{T}"/> class.
        /// </summary>
        public ObservableList()
        {
            this.dispatcher = Dispatcher.CurrentDispatcher;
        }

        #endregion

        #region Public Events

        /// <summary>
        /// The collection changed.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the capacity.
        /// </summary>
        public int Capacity
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.list.Capacity;
                }
            }

            set
            {
                lock (this.syncRoot)
                {
                    this.list.Capacity = value;
                }
            }
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        public int Count
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.list.Count;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether is read only.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region Public Indexers

        /// <summary>
        /// The this.
        /// </summary>
        /// <param name="index">
        /// The index.
        /// </param>
        public T this[int index]
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.list[index];
                }
            }

            set
            {
                lock (this.syncRoot)
                {
                    this.list[index] = value;
                }
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        public void Add(T item)
        {
            if (this.dispatcher.CheckAccess())
            {
                lock (this.syncRoot)
                {
                    this.list.Add(item);
                    this.FireCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
                }
            }
            else
            {
                this.dispatcher.Invoke(new Action<T>(this.Add), DispatcherPriority.Send, item);
            }
        }

        /// <summary>
        /// The add range.
        /// </summary>
        /// <param name="items">
        /// The items.
        /// </param>
        public void AddRange(IEnumerable<T> items)
        {
            if (this.dispatcher.CheckAccess())
            {
                lock (this.syncRoot)
                {
                    this.list.AddRange(items);
                    this.FireCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items.ToList()));
                }
            }
            else
            {
                this.dispatcher.Invoke(new Action<IEnumerable<T>>(this.AddRange), DispatcherPriority.Send, items);
            }
        }

        /// <summary>
        /// The clear.
        /// </summary>
        public void Clear()
        {
            if (this.dispatcher.CheckAccess())
            {
                lock (this.syncRoot)
                {
                    this.list.Clear();
                    this.FireCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
            }
            else
            {
                this.dispatcher.Invoke(new Action(this.Clear), DispatcherPriority.Send);
            }
        }

        /// <summary>
        /// The contains.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <returns>
        /// </returns>
        public bool Contains(T item)
        {
            lock (this.syncRoot)
            {
                return this.list.Contains(item);
            }
        }

        /// <summary>
        /// The copy to.
        /// </summary>
        /// <param name="array">
        /// The array.
        /// </param>
        /// <param name="arrayIndex">
        /// The array index.
        /// </param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (this.syncRoot)
            {
                this.list.CopyTo(array, arrayIndex);
            }
        }

        /// <summary>
        /// The get enumerator.
        /// </summary>
        /// <returns>
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            lock (this.syncRoot)
            {
                return this.list.GetEnumerator();
            }
        }

        /// <summary>
        /// The index of.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <returns>
        /// </returns>
        public int IndexOf(T item)
        {
            lock (this.syncRoot)
            {
                return this.list.IndexOf(item);
            }
        }

        /// <summary>
        /// The insert.
        /// </summary>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <param name="item">
        /// The item.
        /// </param>
        public void Insert(int index, T item)
        {
            if (this.dispatcher.CheckAccess())
            {
                lock (this.syncRoot)
                {
                    this.list.Insert(index, item);
                    this.FireCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
                }
            }
            else
            {
                this.dispatcher.Invoke(new Action<int, T>(this.Insert), DispatcherPriority.Send, index, item);
            }
        }

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <returns>
        /// </returns>
        public bool Remove(T item)
        {
            if (this.dispatcher.CheckAccess())
            {
                lock (this.syncRoot)
                {
                    var index = this.list.IndexOf(item);
                    var result = this.list.Remove(item);
                    this.FireCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
                    return result;
                }
            }

            return (bool)this.dispatcher.Invoke(new Func<T, bool>(this.Remove), DispatcherPriority.Send, item);
        }

        /// <summary>
        /// The remove at.
        /// </summary>
        /// <param name="index">
        /// The index.
        /// </param>
        public void RemoveAt(int index)
        {
            if (this.dispatcher.CheckAccess())
            {
                lock (this.syncRoot)
                {
                    var item = this.list[index];
                    this.list.RemoveAt(index);
                    this.FireCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
                }
            }
            else
            {
                this.dispatcher.Invoke(new Action<int>(this.RemoveAt), DispatcherPriority.Send, index);
            }
        }

        #endregion

        #region Explicit Interface Methods

        /// <summary>
        /// The get enumerator.
        /// </summary>
        /// <returns>
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The fire collection changed.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        private void FireCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (this.CollectionChanged != null)
            {
                this.CollectionChanged(this, e);
            }
        }

        #endregion
    }
}