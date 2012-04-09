/** -*- C# -*-
 * Copyright (C) 2010-2012 Thalmann Software & Consulting, http://www.softdev.ch
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
        Begin, 

        Executing, 

        Completed
    }

    /// <summary>
    /// Connection state event argument.
    /// </summary>
    internal sealed class NotifyScannerStateChangedEventArgs : EventArgs
    {
        #region Constructors and Destructors

        public NotifyScannerStateChangedEventArgs() {
            this.Id = Guid.NewGuid();
            this.ScannerState = ScannerState.Begin;
        }

        #endregion

        #region Public Properties

        public long BytesScanned { get; private set; }

        public long CellsScanned { get; private set; }

        public Guid Id { get; private set; }

        public ScannerState ScannerState { get; private set; }

        #endregion

        #region Methods

        internal NotifyScannerStateChangedEventArgs Completed() {
            this.ScannerState = ScannerState.Completed;
            return this;
        }

        internal NotifyScannerStateChangedEventArgs Executing(IList<Cell> cells) {
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
        #region Constants and Fields

        private static readonly object syncRoot = new object();

        private static Guid currentScan = Guid.Empty;

        private static List<Task> tasks = new List<Task>();

        #endregion

        #region Public Events

        public static event NotifyScannerStateChangedEventHandler ScannerStateChanged;

        #endregion

        #region Public Methods

        public static void BeginScan(IClient client, string nsName, string tableName, Action<IList<Cell>> action) {
            lock (syncRoot) {
                tasks = tasks.Where(t => !t.IsCompleted).ToList();
            }

            var dispatcher = Dispatcher.CurrentDispatcher;
            var beginScan = Task.Factory.StartNew(
                () =>
                    {
                        using (var ns = client.OpenNamespace(nsName))
                        using (var table = ns.OpenTable(tableName)) {
                            var notify = new NotifyScannerStateChangedEventArgs();
                            lock (syncRoot) {
                                currentScan = notify.Id;
                            }

                            FireScannerStateChanged(dispatcher, notify);

                            if (client.Context.HasFeature(ContextFeature.AsyncTableScanner)) {
                                using (var asyncResult = new AsyncResult(
                                    (asyncScannerContext, cells) =>
                                        {
                                            if (!IsCancelled(notify)) {
                                                FireScannerStateChanged(dispatcher, notify.Executing(cells));
                                                action(cells);
                                                return AsyncCallbackResult.Continue;
                                            }

                                            return AsyncCallbackResult.Abort;
                                        })) {
                                    table.BeginScan(asyncResult);
                                    asyncResult.Join();
                                }
                            }
                            else {
                                using (var scanner = table.CreateScanner())
                                using (var bc = new BlockingCollection<List<Cell>>()) {
                                    var task = Task.Factory.StartNew(
                                        () =>
                                            {
                                                try {
                                                    while (!IsCancelled(notify)) {
                                                        var cells = bc.Take();
                                                        FireScannerStateChanged(dispatcher, notify.Executing(cells));
                                                        action(cells);
                                                    }
                                                }
                                                catch (InvalidOperationException) {
                                                }
                                            });

                                    const int ChunkSize = 2500;
                                    var chunk = new List<Cell>(ChunkSize);
                                    Cell cell;
                                    for (var c = 1; !IsCancelled(notify) && scanner.Next(out cell); ++c) {
                                        chunk.Add(cell);
                                        if ((c % chunk.Capacity) == 0) {
                                            bc.Add(chunk);
                                            chunk = new List<Cell>(ChunkSize);
                                        }
                                    }

                                    if (!IsCancelled(notify) && chunk.Count > 0) {
                                        bc.Add(chunk);
                                    }

                                    bc.CompleteAdding();
                                    task.Wait();
                                }
                            }

                            FireScannerStateChanged(dispatcher, notify.Completed());
                        }
                    });

            lock (syncRoot) {
                tasks.Add(beginScan);
            }
        }

        public static void Cancel() {
            int countAlive;
            lock (syncRoot) {
                currentScan = Guid.Empty;
                countAlive = tasks.Count(t => !t.IsCompleted);
            }

            var dispatcher = Dispatcher.CurrentDispatcher;
            var expire = DateTime.Now.AddSeconds(20).Ticks;
            while (countAlive > 0 && DateTime.Now.Ticks < expire) {
                dispatcher.Invoke(DispatcherPriority.Background, (DispatcherOperationCallback)(unused => null), null);
                lock (syncRoot) {
                    countAlive = tasks.Count(t => !t.IsCompleted);
                }
            }
        }

        public static Cell Find(IClient client, string nsName, string tableName, Key key) {
            using (var ns = client.OpenNamespace(nsName))
            using (var table = ns.OpenTable(tableName)) {
                using (var scanner = table.CreateScanner(new ScanSpec(key))) {
                    Cell cell;
                    return scanner.Next(out cell) ? cell : null;
                }
            }
        }

        #endregion

        #region Methods

        private static void FireScannerStateChanged(Dispatcher dispatcher, NotifyScannerStateChangedEventArgs e) {
            NotifyScannerStateChangedEventHandler scannerStateChanged;
            lock (syncRoot) {
                if (currentScan != e.Id) {
                    return;
                }

                scannerStateChanged = ScannerStateChanged;
            }

            if (scannerStateChanged != null) {
                if (dispatcher.CheckAccess()) {
                    scannerStateChanged(null, e);
                }
                else {
                    dispatcher.Invoke(new Action<Dispatcher, NotifyScannerStateChangedEventArgs>(FireScannerStateChanged), DispatcherPriority.Send, dispatcher, e);
                }
            }
        }

        private static bool IsCancelled(NotifyScannerStateChangedEventArgs e) {
            lock (syncRoot) {
                return currentScan != e.Id;
            }
        }

        #endregion
    }
}