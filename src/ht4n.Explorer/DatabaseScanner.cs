/** -*- C# -*-
 * Copyright (C) 2010-2013 Thalmann Software & Consulting, http://www.softdev.ch
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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Threading;

    /// <summary>
    /// Notify scanner state changed delegate.
    /// </summary>
    internal delegate void NotifyScannerStateChangedEventHandler(object sender, NotifyScannerStateChangedEventArgs e);

    /// <summary>
    /// Scanner state.
    /// </summary>
    internal enum ScannerState
    {
        /// <summary>
        /// The begin.
        /// </summary>
        Begin, 

        /// <summary>
        /// The executing.
        /// </summary>
        Executing, 

        /// <summary>
        /// The completed.
        /// </summary>
        Completed
    }

    /// <summary>
    /// Connection state event argument.
    /// </summary>
    internal sealed class NotifyScannerStateChangedEventArgs : EventArgs
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyScannerStateChangedEventArgs"/> class.
        /// </summary>
        public NotifyScannerStateChangedEventArgs()
        {
            this.Id = Guid.NewGuid();
            this.ScannerState = ScannerState.Begin;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the bytes scanned.
        /// </summary>
        public long BytesScanned { get; private set; }

        /// <summary>
        /// Gets the cells scanned.
        /// </summary>
        public long CellsScanned { get; private set; }

        /// <summary>
        /// Gets the id.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the scanner state.
        /// </summary>
        public ScannerState ScannerState { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// The completed.
        /// </summary>
        /// <returns>
        /// </returns>
        internal NotifyScannerStateChangedEventArgs Completed()
        {
            this.ScannerState = ScannerState.Completed;
            return this;
        }

        /// <summary>
        /// The executing.
        /// </summary>
        /// <param name="cells">
        /// The cells.
        /// </param>
        /// <returns>
        /// </returns>
        internal NotifyScannerStateChangedEventArgs Executing(IList<Cell> cells)
        {
            this.CellsScanned += cells.Count;
            this.BytesScanned += cells.Sum(cell => cell.Value != null ? cell.Value.Length : 0);
            this.ScannerState = ScannerState.Executing;
            return this;
        }

        #endregion
    }

    /// <summary>
    /// The database session.
    /// </summary>
    internal static class DatabaseScanner
    {
        #region Static Fields

        /// <summary>
        /// The sync root.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// The current scan.
        /// </summary>
        private static Guid currentScan = Guid.Empty;

        /// <summary>
        /// The tasks.
        /// </summary>
        private static List<Task> tasks = new List<Task>();

        #endregion

        #region Public Events

        /// <summary>
        /// The scanner state changed.
        /// </summary>
        public static event NotifyScannerStateChangedEventHandler ScannerStateChanged;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The begin scan.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="namespaceName">
        /// The namespace name.
        /// </param>
        /// <param name="tableName">
        /// The table name.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        public static void BeginScan(IClient client, string namespaceName, string tableName, Action<IList<Cell>> action)
        {
            lock (SyncRoot)
            {
                tasks = tasks.Where(t => !t.IsCompleted).ToList();
            }

            var dispatcher = Dispatcher.CurrentDispatcher;
            var beginScan = Task.Factory.StartNew(
                () =>
                    {
                        using (var ns = client.OpenNamespace(namespaceName))
                        using (var table = ns.OpenTable(tableName))
                        {
                            var notify = new NotifyScannerStateChangedEventArgs();
                            lock (SyncRoot)
                            {
                                currentScan = notify.Id;
                            }

                            FireScannerStateChanged(dispatcher, notify);

                            if (client.Context.HasFeature(ContextFeature.AsyncTableScanner))
                            {
                                using (var asyncResult = new AsyncResult(
                                    (asyncScannerContext, cells) =>
                                        {
                                            if (!IsCancelled(notify))
                                            {
                                                FireScannerStateChanged(dispatcher, notify.Executing(cells));
                                                action(cells);
                                                return AsyncCallbackResult.Continue;
                                            }

                                            return AsyncCallbackResult.Abort;
                                        }))
                                {
                                    table.BeginScan(asyncResult);
                                    asyncResult.Join();
                                }
                            }
                            else
                            {
                                using (var scanner = table.CreateScanner())
                                using (var bc = new BlockingCollection<List<Cell>>())
                                {
                                    var task = Task.Factory.StartNew(
                                        () =>
                                            {
                                                try
                                                {
                                                    while (!IsCancelled(notify))
                                                    {
                                                        var cells = bc.Take();
                                                        FireScannerStateChanged(dispatcher, notify.Executing(cells));
                                                        action(cells);
                                                    }
                                                }
                                                catch (InvalidOperationException)
                                                {
                                                }
                                            });

                                    const int ChunkSize = 2500;
                                    var chunk = new List<Cell>(ChunkSize);
                                    Cell cell;
                                    for (var c = 1; !IsCancelled(notify) && scanner.Next(out cell); ++c)
                                    {
                                        chunk.Add(cell);
                                        if ((c % chunk.Capacity) == 0)
                                        {
                                            bc.Add(chunk);
                                            chunk = new List<Cell>(ChunkSize);
                                        }
                                    }

                                    if (!IsCancelled(notify) && chunk.Count > 0)
                                    {
                                        bc.Add(chunk);
                                    }

                                    bc.CompleteAdding();
                                    task.Wait();
                                }
                            }

                            FireScannerStateChanged(dispatcher, notify.Completed());
                        }
                    });

            lock (SyncRoot)
            {
                tasks.Add(beginScan);
            }
        }

        /// <summary>
        /// The cancel.
        /// </summary>
        public static void Cancel()
        {
            int countAlive;
            lock (SyncRoot)
            {
                currentScan = Guid.Empty;
                countAlive = tasks.Count(t => !t.IsCompleted);
            }

            var dispatcher = Dispatcher.CurrentDispatcher;
            var expire = DateTime.Now.AddSeconds(20).Ticks;
            while (countAlive > 0 && DateTime.Now.Ticks < expire)
            {
                dispatcher.Invoke(DispatcherPriority.Background, (DispatcherOperationCallback)(unused => null), null);
                lock (SyncRoot)
                {
                    countAlive = tasks.Count(t => !t.IsCompleted);
                }
            }
        }

        /// <summary>
        /// The find.
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="namespaceName">
        /// The namespace name.
        /// </param>
        /// <param name="tableName">
        /// The table name.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// </returns>
        public static Cell Find(IClient client, string namespaceName, string tableName, Key key)
        {
            using (var ns = client.OpenNamespace(namespaceName))
            using (var table = ns.OpenTable(tableName))
            {
                using (var scanner = table.CreateScanner(new ScanSpec(key)))
                {
                    Cell cell;
                    return scanner.Next(out cell) ? cell : null;
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The fire scanner state changed.
        /// </summary>
        /// <param name="dispatcher">
        /// The dispatcher.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void FireScannerStateChanged(Dispatcher dispatcher, NotifyScannerStateChangedEventArgs e)
        {
            NotifyScannerStateChangedEventHandler scannerStateChanged;
            lock (SyncRoot)
            {
                if (currentScan != e.Id)
                {
                    return;
                }

                scannerStateChanged = ScannerStateChanged;
            }

            if (scannerStateChanged != null)
            {
                if (dispatcher.CheckAccess())
                {
                    scannerStateChanged(null, e);
                }
                else
                {
                    dispatcher.Invoke(new Action<Dispatcher, NotifyScannerStateChangedEventArgs>(FireScannerStateChanged), DispatcherPriority.Send, dispatcher, e);
                }
            }
        }

        /// <summary>
        /// The is cancelled.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        /// <returns>
        /// </returns>
        private static bool IsCancelled(NotifyScannerStateChangedEventArgs e)
        {
            lock (SyncRoot)
            {
                return currentScan != e.Id;
            }
        }

        #endregion
    }
}