/** -*- C# -*-
 * Copyright (C) 2011 Andy Thalmann
 *
 * This file is part of ht4n.
 *
 * ht4n is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
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

namespace Hypertable.Test
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;

    using Hypertable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test asynchronous table mutators.
    /// </summary>
    [TestClass]
    public class TestAsyncTableMutator : TestBase
    {
        #region Constants and Fields

        private const int Count = 1000;

        private const string Schema =
            "<Schema>" + "<AccessGroup name=\"default\" blksz=\"1024\">" + "<ColumnFamily>" + "<Name>a</Name>" + "<deleted>false</deleted>" + "</ColumnFamily>" + "<ColumnFamily>" +
            "<Name>b</Name>" + "<deleted>false</deleted>" + "</ColumnFamily>" + "<ColumnFamily>" + "<Name>c</Name>" + "<deleted>false</deleted>" + "</ColumnFamily>" +
            "</AccessGroup>" + "</Schema>";

        private static readonly MutatorSpec ChunkedMutatorSpec = MutatorSpec.CreateChunked();

        private static readonly MutatorSpec ChunkedQueuedMutatorSpec = new MutatorSpec(MutatorKind.Chunked) { Queued = true, Capacity = 200 };

        private static readonly UTF8Encoding Encoding = new UTF8Encoding();

        private static Table table;

        #endregion

        #region Public Methods

        [ClassCleanup]
        public static void ClassCleanup() {
            table.Dispose();
            Ns.DropNamespaces(DropDispositions.Complete);
            Ns.DropTables();
        }

        [ClassInitialize]
        public static void ClassInitialize(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext) {
            table = EnsureTable(typeof(TestAsyncTableMutator), Schema);
        }

        [TestMethod]
        public void AsyncDelete() {
            this.AsyncDelete(null);
        }

        public void AsyncDelete(MutatorSpec mutatorSpec) {
            var key = new Key { ColumnFamily = "a" };
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    for (int n = 0; n < Count; ++n) {
                        key.Row = Guid.NewGuid().ToString();
                        mutator.Set(key, Encoding.GetBytes(key.Row));
                    }
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }

            var rowKeys = new SortedSet<String>();
            using (var scanner = table.CreateScanner()) {
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    rowKeys.Add(cell.Key.Row);
                }
            }

            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    foreach (String rowKey in rowKeys) {
                        mutator.Delete(rowKey);
                    }
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(0, this.GetCellCount());

            key = new Key { ColumnFamily = "a" };
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    for (int n = 0; n < Count; ++n) {
                        key.Row = Guid.NewGuid().ToString();
                        mutator.Set(key, Encoding.GetBytes(key.Row));
                    }
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }

            rowKeys = new SortedSet<String>();
            using (var scanner = table.CreateScanner()) {
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    rowKeys.Add(cell.Key.Row);
                }
            }

            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    key = new Key();
                    foreach (String rowKey in rowKeys) {
                        key.Row = rowKey;
                        mutator.Delete(key);
                    }
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(0, this.GetCellCount());

            key = new Key { ColumnFamily = "a" };
            var keys = new List<Key>();
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    for (int n = 0; n < Count; ++n) {
                        key.Row = Guid.NewGuid().ToString();
                        mutator.Set(key, Encoding.GetBytes(key.Row));
                        keys.Add((Key)key.Clone());
                    }
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }

            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Delete(keys);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(0, this.GetCellCount());

            key = new Key { ColumnFamily = "a" };
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    for (int n = 0; n < Count; ++n) {
                        key.Row = Guid.NewGuid().ToString();
                        mutator.Set(key, Encoding.GetBytes(key.Row));
                    }
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }

            var cells = new List<Cell>();
            using (var scanner = table.CreateScanner()) {
                Cell cell;
                while (scanner.Next(out cell)) {
                    cells.Add(cell);
                }
            }

            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Delete(cells);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(0, this.GetCellCount());

            key = new Key();
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    key.Row = Guid.NewGuid().ToString();
                    key.ColumnFamily = "a";
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                    key.ColumnFamily = "b";
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(2, this.GetCellCount());

            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Delete(key);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }

            using (var scanner = table.CreateScanner()) {
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    Assert.AreEqual("a", cell.Key.ColumnFamily);
                }
            }
            Assert.AreEqual(1, this.GetCellCount());

            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    key.ColumnFamily = "b";
                    key.ColumnQualifier = "1";
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                    key.ColumnQualifier = "2";
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                    key.ColumnQualifier = "3";
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                    key.ColumnQualifier = String.Empty;
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                    key.ColumnQualifier = "4";
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(6, this.GetCellCount());

            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Delete(key);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(5, this.GetCellCount());

            key.ColumnQualifier = "1";
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Delete(key);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(4, this.GetCellCount());

            key.ColumnQualifier = "";
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Delete(key);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(3, this.GetCellCount());

            key.ColumnQualifier = null;
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Delete(key);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }

            using (var scanner = table.CreateScanner()) {
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    Assert.AreEqual("a", cell.Key.ColumnFamily);
                }
            }
            Assert.AreEqual(1, this.GetCellCount());

            key.ColumnFamily = String.Empty; // usually use null
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Delete(key);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(0, this.GetCellCount());

            key = new Key();
            //FIXME(remove this line), issue 'delete with unspecified timestamp applying to all future inserts'

            var dateTimes = new List<DateTime>();
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    key.ColumnFamily = "b";
                    key.ColumnQualifier = "1";
                    key.DateTime = DateTime.UtcNow;
                    mutator.Set(key, Encoding.GetBytes(key.DateTime.ToString()));
                    dateTimes.Add(key.DateTime);
                    Thread.Sleep(200);
                    key.DateTime = DateTime.UtcNow;
                    Assert.IsTrue(dateTimes[dateTimes.Count - 1] < key.DateTime);
                    mutator.Set(key, Encoding.GetBytes(key.DateTime.ToString()));
                    dateTimes.Add(key.DateTime);
                    Thread.Sleep(200);
                    key.DateTime = DateTime.UtcNow;
                    Assert.IsTrue(dateTimes[dateTimes.Count - 1] < key.DateTime);
                    mutator.Set(key, Encoding.GetBytes(key.DateTime.ToString()));
                    dateTimes.Add(key.DateTime);
                    Thread.Sleep(200);
                    key.DateTime = DateTime.UtcNow;
                    Assert.IsTrue(dateTimes[dateTimes.Count - 1] < key.DateTime);
                    mutator.Set(key, Encoding.GetBytes(key.DateTime.ToString()));
                    dateTimes.Add(key.DateTime);
                    Thread.Sleep(200);
                    key.DateTime = DateTime.UtcNow;
                    Assert.IsTrue(dateTimes[dateTimes.Count - 1] < key.DateTime);
                    mutator.Set(key, Encoding.GetBytes(key.DateTime.ToString()));
                    dateTimes.Add(key.DateTime);
                    Thread.Sleep(200);
                    key.DateTime = DateTime.UtcNow;
                    Assert.IsTrue(dateTimes[dateTimes.Count - 1] < key.DateTime);
                    mutator.Set(key, Encoding.GetBytes(key.DateTime.ToString()));
                    dateTimes.Add(key.DateTime);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(6, dateTimes.Count);
            Assert.AreEqual(6, this.GetCellCount());

            key.DateTime = dateTimes[1];
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Delete(key);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(4, this.GetCellCount());

            key.ColumnQualifier = null;
            key.DateTime = dateTimes[3];
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Delete(key);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(2, this.GetCellCount());

            key.ColumnFamily = null;
            key.ColumnQualifier = null;
            key.DateTime = dateTimes[4];
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Delete(key);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(1, this.GetCellCount());

            key.ColumnFamily = null;
            key.ColumnQualifier = null;
            key.DateTime = dateTimes[5];
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Delete(key);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(0, this.GetCellCount());
        }

        [TestMethod]
        public void AsyncDeleteChunked() {
            this.AsyncDelete(ChunkedMutatorSpec);
        }

        [TestMethod]
        public void AsyncDeleteChunkedQueued() {
            this.AsyncDelete(ChunkedQueuedMutatorSpec);
        }

        [TestMethod]
        public void AsyncDeleteQueued() {
            this.AsyncDelete(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void AsyncDeleteSet() {
            this.AsyncDeleteSet(null);
        }

        public void AsyncDeleteSet(MutatorSpec mutatorSpec) {
            Cell cell;
            var key = new Key();
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    for (int n = 0; n < Count; ++n) {
                        key.Row = Guid.NewGuid().ToString();
                        key.ColumnFamily = "a";
                        mutator.Set(key, Encoding.GetBytes(key.Row));
                        key.ColumnFamily = "b";
                        mutator.Set(key, Encoding.GetBytes(key.Row));
                    }
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }

            var cells = new List<Cell>();
            using (var scanner = table.CreateScanner()) {
                while (scanner.Next(out cell)) {
                    cells.Add(new Cell(new Key(cell.Key.Row), CellFlag.DeleteRow));
                }
            }
            Assert.AreEqual(2 * Count, cells.Count);

            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Set(cells);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(0, this.GetCellCount());

            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    for (int n = 0; n < Count; ++n) {
                        key.Row = Guid.NewGuid().ToString();
                        key.ColumnFamily = "a";
                        mutator.Set(key, Encoding.GetBytes(key.Row));
                        key.ColumnFamily = "b";
                        mutator.Set(key, Encoding.GetBytes(key.Row));
                    }
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }

            cells.Clear();
            using (var scanner = table.CreateScanner()) {
                while (scanner.Next(out cell)) {
                    cells.Add(new Cell(cell.Key, CellFlag.DeleteRow));
                }
            }
            Assert.AreEqual(2 * Count, cells.Count);

            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Set(cells);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(0, this.GetCellCount());

            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    for (int n = 0; n < Count; ++n) {
                        key.Row = Guid.NewGuid().ToString();
                        key.ColumnFamily = "a";
                        mutator.Set(key, Encoding.GetBytes(key.Row));
                        key.ColumnFamily = "b";
                        mutator.Set(key, Encoding.GetBytes(key.Row));
                    }
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }

            cells.Clear();
            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("b"))) {
                while (scanner.Next(out cell)) {
                    cells.Add(new Cell(cell.Key, CellFlag.DeleteColumnFamily));
                }
            }
            Assert.AreEqual(Count, cells.Count);

            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Set(cells);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(Count, this.GetCellCount());

            cells.Clear();
            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("a"))) {
                while (scanner.Next(out cell)) {
                    cells.Add(new Cell(cell.Key, CellFlag.DeleteCell));
                }
            }
            Assert.AreEqual(Count, this.GetCellCount());

            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Set(cells);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(0, this.GetCellCount());

            Cell cellDelete;
            key = new Key();
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    key.Row = Guid.NewGuid().ToString();
                    key.ColumnFamily = "a";
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                    key.ColumnFamily = "b";
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                    cellDelete = new Cell(key, CellFlag.DeleteCell, true);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(2, this.GetCellCount());

            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Set(cellDelete);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }

            using (var scanner = table.CreateScanner()) {
                while (scanner.Next(out cell)) {
                    Assert.AreEqual("a", cell.Key.ColumnFamily);
                }
            }
            Assert.AreEqual(1, this.GetCellCount());

            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    key.ColumnFamily = "b";
                    key.ColumnQualifier = "1";
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                    key.ColumnQualifier = "2";
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                    key.ColumnQualifier = "3";
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                    key.ColumnQualifier = String.Empty;
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                    key.ColumnQualifier = "4";
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                    cellDelete = new Cell(key, CellFlag.DeleteCell, true);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(6, this.GetCellCount());

            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Set(cellDelete);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(5, this.GetCellCount());

            cellDelete.Key.ColumnQualifier = "1";
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Set(cellDelete);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(4, this.GetCellCount());

            cellDelete.Key.ColumnQualifier = "";
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Set(cellDelete);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(3, this.GetCellCount());

            cellDelete.Key.ColumnQualifier = null;
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Set(cellDelete);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }

            using (var scanner = table.CreateScanner()) {
                while (scanner.Next(out cell)) {
                    Assert.AreEqual("a", cell.Key.ColumnFamily);
                }
            }
            Assert.AreEqual(1, this.GetCellCount());

            cellDelete.Key.ColumnFamily = String.Empty; // usually use null
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Set(cellDelete);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(0, this.GetCellCount());

            key = new Key();
            //FIXME(remove this line), issue 'delete with unspecified timestamp applying to all future inserts'

            var dateTimes = new List<DateTime>();
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    key.ColumnFamily = "b";
                    key.ColumnQualifier = "1";
                    key.DateTime = DateTime.UtcNow;
                    mutator.Set(key, Encoding.GetBytes(key.DateTime.ToString()));
                    dateTimes.Add(key.DateTime);
                    Thread.Sleep(200);
                    key.DateTime = DateTime.UtcNow;
                    mutator.Set(key, Encoding.GetBytes(key.DateTime.ToString()));
                    dateTimes.Add(key.DateTime);
                    Thread.Sleep(200);
                    key.DateTime = DateTime.UtcNow;
                    mutator.Set(key, Encoding.GetBytes(key.DateTime.ToString()));
                    dateTimes.Add(key.DateTime);
                    Thread.Sleep(200);
                    key.DateTime = DateTime.UtcNow;
                    mutator.Set(key, Encoding.GetBytes(key.DateTime.ToString()));
                    dateTimes.Add(key.DateTime);
                    Thread.Sleep(200);
                    key.DateTime = DateTime.UtcNow;
                    mutator.Set(key, Encoding.GetBytes(key.DateTime.ToString()));
                    dateTimes.Add(key.DateTime);
                    Thread.Sleep(200);
                    key.DateTime = DateTime.UtcNow;
                    mutator.Set(key, Encoding.GetBytes(key.DateTime.ToString()));
                    dateTimes.Add(key.DateTime);
                    Thread.Sleep(200);
                    key.DateTime = DateTime.UtcNow;
                    mutator.Set(key, Encoding.GetBytes(key.DateTime.ToString()));
                    dateTimes.Add(key.DateTime);
                    Thread.Sleep(200);
                    key.DateTime = DateTime.UtcNow;
                    mutator.Set(key, Encoding.GetBytes(key.DateTime.ToString()));
                    dateTimes.Add(key.DateTime);
                    cellDelete = new Cell(key, CellFlag.DeleteCell, true);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(8, this.GetCellCount());
            Assert.AreEqual(8, dateTimes.Count);

            cells.Clear();
            using (var scanner = table.CreateScanner()) {
                while (scanner.Next(out cell)) {
                    cells.Add(cell);
                }
            }
            Assert.AreEqual(8, cells.Count);

            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Set(new Cell(cells[7].Key, CellFlag.DeleteCellVersion, true));
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(7, this.GetCellCount());

            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Set(new Cell(cells[1].Key, CellFlag.DeleteCellVersion, true));
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(6, this.GetCellCount());

            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Set(new Cell(cells[0].Key, CellFlag.DeleteCellVersion, true));
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(5, this.GetCellCount());

            cellDelete.Key.DateTime = dateTimes[1];
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Set(cellDelete);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(4, this.GetCellCount());

            cellDelete.Key.ColumnQualifier = null;
            cellDelete.Key.DateTime = dateTimes[3];
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Set(cellDelete);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(2, this.GetCellCount());

            cellDelete.Key.ColumnFamily = null;
            cellDelete.Key.ColumnQualifier = null;
            cellDelete.Key.DateTime = dateTimes[4];
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Set(cellDelete);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(1, this.GetCellCount());

            cellDelete.Key.ColumnFamily = null;
            cellDelete.Key.ColumnQualifier = null;
            cellDelete.Key.DateTime = dateTimes[5];
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Set(cellDelete);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(0, this.GetCellCount());
        }

        [TestMethod]
        public void AsyncDeleteSetChunked() {
            this.AsyncDeleteSet(ChunkedMutatorSpec);
        }

        [TestMethod]
        public void AsyncDeleteSetChunkedQueued() {
            this.AsyncDeleteSet(ChunkedQueuedMutatorSpec);
        }

        [TestMethod]
        public void AsyncDeleteSetQueued() {
            this.AsyncDeleteSet(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void AsyncRevisions() {
            this.AsyncRevisions(null);
        }

        public void AsyncRevisions(MutatorSpec mutatorSpec) {
            Assert.AreEqual(0, this.GetCellCount());

            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    var key = new Key(Guid.NewGuid().ToString()) { ColumnFamily = "a" };
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                    asyncResult.Join();
                    Assert.IsTrue(asyncResult.IsCompleted);
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");

                    Cell cell1, cell2, cellNone;

                    using (var scanner = table.CreateScanner(new ScanSpec(key.Row).AddColumn("a"))) {
                        Assert.IsTrue(scanner.Next(out cell1));
                        Assert.IsFalse(scanner.Next(out cellNone));
                    }
                    Assert.AreEqual(cell1.Key.Row, Encoding.GetString(cell1.Value));

                    mutator.Set(key, Encoding.GetBytes("abc"));
                    asyncResult.Join();
                    Assert.IsTrue(asyncResult.IsCompleted);
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");

                    using (var scanner = table.CreateScanner(new ScanSpec(key.Row).AddColumn("a"))) {
                        Assert.IsTrue(scanner.Next(out cell1));
                        Assert.IsTrue(scanner.Next(out cell2));
                        Assert.IsFalse(scanner.Next(out cellNone));
                    }

                    Assert.IsTrue(cell1.Key.Timestamp > cell2.Key.Timestamp);
                    Assert.AreEqual("abc", Encoding.GetString(cell1.Value));
                    Assert.AreEqual(cell2.Key.Row, Encoding.GetString(cell2.Value));

                    cell1.Value = Encoding.GetBytes("def");
                    mutator.Set(cell1); // does NOT create a new revision
                    asyncResult.Join();
                    Assert.IsTrue(asyncResult.IsCompleted);
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");

                    Cell cell1_1;
                    using (var scanner = table.CreateScanner(new ScanSpec(key.Row).AddColumn("a"))) {
                        Assert.IsTrue(scanner.Next(out cell1_1));
                        Assert.IsTrue(scanner.Next(out cell2));
                        Assert.IsFalse(scanner.Next(out cellNone));
                    }

                    Assert.IsTrue(cell1.Key == cell1_1.Key);
                    Assert.AreEqual(cell1.Key, cell1_1.Key);
                    Assert.AreEqual(0, cell1.Key.CompareTo(cell1_1.Key));
                    Assert.AreEqual(0, Key.Compare(cell1.Key, cell1_1.Key));
                    Assert.AreEqual(cell1.Key.GetHashCode(), cell1_1.Key.GetHashCode());
                    Assert.AreEqual("def", Encoding.GetString(cell1_1.Value));
                    Assert.AreEqual(cell2.Key.Row, Encoding.GetString(cell2.Value));
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
        }

        [TestMethod]
        public void AsyncRevisionsChunked() {
            this.AsyncRevisions(ChunkedMutatorSpec);
        }

        [TestMethod]
        public void AsyncRevisionsChunkedQueued() {
            this.AsyncRevisions(ChunkedQueuedMutatorSpec);
        }

        [TestMethod]
        public void AsyncRevisionsQueued() {
            this.AsyncRevisions(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void AsyncSet() {
            this.AsyncSet(null);
        }

        public void AsyncSet(MutatorSpec mutatorSpec) {
            var key = new Key { ColumnFamily = "a" };
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    for (int n = 0; n < Count; ++n) {
                        key.Row = Guid.NewGuid().ToString();
                        mutator.Set(key, Encoding.GetBytes(key.Row));
                    }
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                Assert.AreEqual(Count, this.GetCellCount());
            }

            using (AsyncResult asyncResult = new BlockingAsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    for (int n = 0; n < Count; ++n) {
                        key.Row = Guid.NewGuid().ToString();
                        mutator.Set(key, Encoding.GetBytes(key.Row));
                    }
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                Assert.AreEqual(2 * Count, this.GetCellCount());
            }
        }

        [TestMethod]
        public void AsyncSetChunked() {
            this.AsyncSet(new MutatorSpec(MutatorKind.Chunked) { FlushEachChunk = true, MaxCellCount = 100 });
        }

        [TestMethod]
        public void AsyncSetChunkedQueued() {
            this.AsyncSet(new MutatorSpec(MutatorKind.Chunked) { Queued = true, Capacity = 100, FlushEachChunk = true, MaxCellCount = 100 });
        }

        [TestMethod]
        public void AsyncSetCollection() {
            this.AsyncSetCollection(null);
        }

        public void AsyncSetCollection(MutatorSpec mutatorSpec) {
            var key = new Key { ColumnFamily = "a" };
            var cells = new List<Cell>();
            for (int n = 0; n < Count; ++n) {
                key.Row = Guid.NewGuid().ToString();
                cells.Add(new Cell(key, Encoding.GetBytes(key.Row), true));
            }
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Set(cells);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                Assert.AreEqual(Count, this.GetCellCount());
            }
        }

        [TestMethod]
        public void AsyncSetCollectionChunked() {
            this.AsyncSetCollection(ChunkedMutatorSpec);
        }

        [TestMethod]
        public void AsyncSetCollectionChunkedQueued() {
            this.AsyncSetCollection(ChunkedQueuedMutatorSpec);
        }

        [TestMethod]
        public void AsyncSetCollectionCreateKey() {
            this.AsyncSetCollectionCreateKey(null);
        }

        public void AsyncSetCollectionCreateKey(MutatorSpec mutatorSpec) {
            var key = new Key { ColumnFamily = "a" };
            var cells = new List<Cell>();
            for (int n = 0; n < Count; ++n) {
                cells.Add(new Cell(key.Clone() as Key, Encoding.GetBytes(Guid.NewGuid().ToString())));
            }
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Set(cells, true);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                foreach (var cell in cells) {
                    Assert.IsFalse(String.IsNullOrEmpty(cell.Key.Row));
                }
                Assert.AreEqual(Count, this.GetCellCount());
            }
        }

        [TestMethod]
        public void AsyncSetCollectionCreateKeyChunked() {
            this.AsyncSetCollectionCreateKey(ChunkedMutatorSpec);
        }

        [TestMethod]
        public void AsyncSetCollectionCreateKeyChunkedQueued() {
            this.AsyncSetCollectionCreateKey(ChunkedQueuedMutatorSpec);
        }

        [TestMethod]
        public void AsyncSetCollectionCreateKeyLazy() {
            this.AsyncSetCollectionCreateKeyLazy(null);
        }

        public void AsyncSetCollectionCreateKeyLazy(MutatorSpec mutatorSpec) {
            var key = new Key { ColumnFamily = "a" };
            var cells = new List<Cell>();
            for (int n = 0; n < Count; ++n) {
                key.Row = (n % 3) == 0 ? Guid.NewGuid().ToString() : null;
                cells.Add(new Cell(key.Clone() as Key, Encoding.GetBytes(Guid.NewGuid().ToString())));
            }
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Set(cells);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                foreach (var cell in cells) {
                    Assert.IsFalse(String.IsNullOrEmpty(cell.Key.Row));
                }
                Assert.AreEqual(Count, this.GetCellCount());
            }
        }

        [TestMethod]
        public void AsyncSetCollectionCreateKeyLazyChunked() {
            this.AsyncSetCollectionCreateKeyLazy(ChunkedMutatorSpec);
        }

        [TestMethod]
        public void AsyncSetCollectionCreateKeyLazyChunkedQueued() {
            this.AsyncSetCollectionCreateKeyLazy(ChunkedQueuedMutatorSpec);
        }

        [TestMethod]
        public void AsyncSetCollectionCreateKeyLazyKeyQueued() {
            this.AsyncSetCollectionCreateKeyLazy(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void AsyncSetCollectionCreateKeyQueued() {
            this.AsyncSetCollectionCreateKey(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void AsyncSetCollectionDifferentSizedValues() {
            this.AsyncSetCollectionDifferentSizedValues(null);
        }

        public void AsyncSetCollectionDifferentSizedValues(MutatorSpec mutatorSpec) {
            var sb = new StringBuilder();
            for (int n = 0; n < 0x40; ++n) {
                sb.Append(Guid.NewGuid().ToString());
            }
            var largeValue = Encoding.GetBytes(sb.ToString());
            for (int n = 0; n < 0x4000; ++n) {
                sb.Append(Guid.NewGuid().ToString());
            }
            var hugeValue = Encoding.GetBytes(sb.ToString());
            var smallValue = Encoding.GetBytes(Guid.NewGuid().ToString());

            var key = new Key { ColumnFamily = "a" };
            var cells = new List<Cell>
                {
                    new Cell(key.Clone() as Key, smallValue),
                    new Cell(key.Clone() as Key, null),
                    new Cell(key.Clone() as Key, smallValue),
                    new Cell(key.Clone() as Key, largeValue),
                    new Cell(key.Clone() as Key, null),
                    new Cell(key.Clone() as Key, smallValue),
                    new Cell(key.Clone() as Key, largeValue),
                    new Cell(key.Clone() as Key, hugeValue),
                    new Cell(key.Clone() as Key, null),
                    new Cell(key.Clone() as Key, smallValue),
                    new Cell(key.Clone() as Key, largeValue)
                };

            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Set(cells, true);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                foreach (var cell in cells) {
                    Assert.IsFalse(String.IsNullOrEmpty(cell.Key.Row));
                }
            }
        }

        [TestMethod]
        public void AsyncSetCollectionDifferentSizedValuesChunked() {
            this.AsyncSetCollectionDifferentSizedValues(ChunkedMutatorSpec);
        }

        [TestMethod]
        public void AsyncSetCollectionDifferentSizedValuesChunkedQueued() {
            this.AsyncSetCollectionDifferentSizedValues(ChunkedQueuedMutatorSpec);
        }

        [TestMethod]
        public void AsyncSetCollectionDifferentSizedValuesQueued() {
            this.AsyncSetCollectionDifferentSizedValues(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void AsyncSetCollectionHugeValues() {
            this.AsyncSetCollectionHugeValues(null);
        }

        public void AsyncSetCollectionHugeValues(MutatorSpec mutatorSpec) {
            const int K = 1024;
            const int M = K * K;
            var hugeValue = new byte[M];
            new Random().NextBytes(hugeValue);

            var key = new Key { ColumnFamily = "a" };
            var cells = new List<Cell>
                {
                    new Cell(key.Clone() as Key, hugeValue),
                    new Cell(key.Clone() as Key, hugeValue),
                    new Cell(key.Clone() as Key, hugeValue),
                    new Cell(key.Clone() as Key, hugeValue),
                    new Cell(key.Clone() as Key, hugeValue),
                    new Cell(key.Clone() as Key, hugeValue),
                    new Cell(key.Clone() as Key, hugeValue),
                    new Cell(key.Clone() as Key, hugeValue),
                    new Cell(key.Clone() as Key, hugeValue),
                    new Cell(key.Clone() as Key, hugeValue),
                    new Cell(key.Clone() as Key, hugeValue)
                };

            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    mutator.Set(cells, true);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                foreach (var cell in cells) {
                    Assert.IsFalse(String.IsNullOrEmpty(cell.Key.Row));
                }
            }
        }

        [TestMethod]
        public void AsyncSetCollectionHugeValuesChunked() {
            this.AsyncSetCollectionHugeValues(ChunkedMutatorSpec);
        }

        [TestMethod]
        public void AsyncSetCollectionHugeValuesChunkedQueued() {
            this.AsyncSetCollectionHugeValues(ChunkedQueuedMutatorSpec);
        }

        [TestMethod]
        public void AsyncSetCollectionHugeValuesQueued() {
            this.AsyncSetCollectionHugeValues(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void AsyncSetCollectionQueued() {
            this.AsyncSetCollection(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void AsyncSetCollectionThreaded() {
            this.AsyncSetCollectionThreaded(null);
        }

        public void AsyncSetCollectionThreaded(MutatorSpec mutatorSpec) {
            var key = new Key { ColumnFamily = "a" };
            var cells1 = new List<Cell>();
            for (int n = 0; n < 16; ++n) {
                cells1.Add(new Cell(key.Clone() as Key, Encoding.GetBytes(Guid.NewGuid().ToString())));
            }
            var cells2 = new List<Cell>();
            for (int n = 0; n < 16; ++n) {
                cells2.Add(new Cell(key.Clone() as Key, Encoding.GetBytes(Guid.NewGuid().ToString())));
            }
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    int c1 = 0;
                    int c2 = 0;

                    var t1 = new Thread(
                        () => {
                            for (int n = 0; n < Count; ++n, ++c1) {
                                mutator.Set(cells1, true);
                                if (n == Count / 2) {
                                    mutator.Flush();
                                }
                            }
                        }) { IsBackground = true };

                    var t2 = new Thread(
                        () => {
                            for (int n = 0; n < Count; ++n, ++c2) {
                                mutator.Set(cells2, true);
                                if (n == Count / 2) {
                                    mutator.Flush();
                                }
                            }
                        }) { IsBackground = true };

                    t1.Start();
                    t2.Start();
                    t1.Join();
                    t2.Join();
                    Assert.IsTrue(c1 > 0);
                    Assert.IsTrue(c2 > 0);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
        }

        [TestMethod]
        public void AsyncSetCollectionThreadedChunked() {
            this.AsyncSetCollectionThreaded(ChunkedMutatorSpec);
        }

        [TestMethod]
        public void AsyncSetCollectionThreadedChunkedQueued() {
            this.AsyncSetCollectionThreaded(ChunkedQueuedMutatorSpec);
        }

        [TestMethod]
        public void AsyncSetCollectionThreadedQueued() {
            this.AsyncSetCollectionThreaded(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void AsyncSetCreateKey() {
            this.AsyncSetCreateKey(null);
        }

        public void AsyncSetCreateKey(MutatorSpec mutatorSpec) {
            var key = new Key { ColumnFamily = "b" };
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    for (int n = 0; n < Count; ++n) {
                        mutator.Set(key, Encoding.GetBytes(Guid.NewGuid().ToString()), true);
                        Assert.IsFalse(String.IsNullOrEmpty(key.Row));
                    }
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                Assert.AreEqual(Count, this.GetCellCount());
            }
        }

        [TestMethod]
        public void AsyncSetCreateKeyChunked() {
            this.AsyncSetCreateKey(ChunkedMutatorSpec);
        }

        [TestMethod]
        public void AsyncSetCreateKeyChunkedQueued() {
            this.AsyncSetCreateKey(ChunkedQueuedMutatorSpec);
        }

        [TestMethod]
        public void AsyncSetCreateKeyLazy() {
            this.AsyncSetCreateKeyLazy(null);
        }

        public void AsyncSetCreateKeyLazy(MutatorSpec mutatorSpec) {
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    for (int n = 0; n < Count; ++n) {
                        var key = new Key { ColumnFamily = "b" };
                        mutator.Set(key, Encoding.GetBytes(Guid.NewGuid().ToString()));
                        Assert.IsFalse(String.IsNullOrEmpty(key.Row));
                    }
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                Assert.AreEqual(Count, this.GetCellCount());
            }
        }

        [TestMethod]
        public void AsyncSetCreateKeyLazyChunked() {
            this.AsyncSetCreateKeyLazy(ChunkedMutatorSpec);
        }

        [TestMethod]
        public void AsyncSetCreateKeyLazyChunkedQueued() {
            this.AsyncSetCreateKeyLazy(ChunkedQueuedMutatorSpec);
        }

        [TestMethod]
        public void AsyncSetCreateKeyLazyQueued() {
            this.AsyncSetCreateKeyLazy(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void AsyncSetCreateKeyQueued() {
            this.AsyncSetCreateKey(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void AsyncSetDifferentContext() {
            this.AsyncSetDifferentContext(null);
        }

        public void AsyncSetDifferentContext(MutatorSpec mutatorSpec) {
            using (var ctx = Context.Create(CtxKind == ContextKind.Hyper ? ContextKind.Thrift : ContextKind.Hyper, Host))
            using (var client = ctx.CreateClient()) {
                string nsNameOther = NsName + "/other";
                try {
                    using (var nsOther = client.OpenNamespace(nsNameOther, OpenDispositions.OpenAlways))
                    using (var tableOther = nsOther.OpenTable("AsyncSetDifferentContext", Schema, OpenDispositions.CreateAlways)) {
                        var key = new Key { ColumnFamily = "a" };
                        using (var asyncResult = new AsyncResult()) {
                            using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec))
                            using (var mutatorOther = tableOther.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                                for (int n = 0; n < Count; ++n) {
                                    key.Row = Guid.NewGuid().ToString();
                                    mutator.Set(key, Encoding.GetBytes(key.Row));
                                    key.Row = Guid.NewGuid().ToString();
                                    mutatorOther.Set(key, Encoding.GetBytes(key.Row));
                                }
                            }
                            asyncResult.Join();
                            Assert.IsTrue(asyncResult.IsCompleted);
                            Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                            Assert.AreEqual(Count, this.GetCellCount());
                            Assert.AreEqual(Count, GetCellCount(tableOther));
                        }
                    }
                }
                finally {
                    client.DropNamespace(nsNameOther, DropDispositions.Complete);
                }
            }
        }

        [TestMethod]
        public void AsyncSetDifferentContextChunked() {
            this.AsyncSetDifferentContext(new MutatorSpec(MutatorKind.Chunked) { FlushEachChunk = true, MaxCellCount = 100 });
        }

        [TestMethod]
        public void AsyncSetDifferentContextChunkedQueued() {
            this.AsyncSetDifferentContext(new MutatorSpec(MutatorKind.Chunked) { Queued = true, Capacity = 100, FlushEachChunk = true, MaxCellCount = 100 });
        }

        [TestMethod]
        public void AsyncSetDifferentContextQueued() {
            this.AsyncSetDifferentContext(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void AsyncSetMultipleTables() {
            this.AsyncSetMultipleTables(null);
        }

        public void AsyncSetMultipleTables(MutatorSpec mutatorSpec) {
            const int CountTables = 10;
            var tables = new List<Table>();
            try {
                for (int t = 0; t < CountTables; ++t) {
                    var table2 = EnsureTable(String.Format("AsyncSetMultipleTables-{0}", t), Schema);
                    tables.Add(table2);
                }

                var key = new Key { ColumnFamily = "a" };
                using (var asyncResult = new AsyncResult()) {
                    var mutators = new List<ITableMutator>();
                    try {
                        tables.ForEach(t => mutators.Add(t.CreateAsyncMutator(asyncResult, mutatorSpec)));
                        for (int n = 0; n < Count; ++n) {
                            mutators.ForEach(m => m.Set(key, Encoding.GetBytes(key.Row = Guid.NewGuid().ToString())));
                        }
                        asyncResult.Join();
                        Assert.IsTrue(asyncResult.IsCompleted);
                        Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                        tables.ForEach(t => Assert.AreEqual(Count, GetCellCount(t)));
                    }
                    finally {
                        mutators.ForEach(m => m.Dispose());
                    }
                }
            }
            finally {
                tables.ForEach(t => t.Dispose());
                for (int t = 0; t < CountTables; ++t) {
                    Ns.DropTable(String.Format("AsyncSetMultipleTables-{0}", t), DropDispositions.IfExists);
                }
            }
        }

        [TestMethod]
        public void AsyncSetMultipleTablesChunked() {
            this.AsyncSetMultipleTables(new MutatorSpec(MutatorKind.Chunked) { FlushEachChunk = true, MaxCellCount = 100 });
        }

        [TestMethod]
        public void AsyncSetMultipleTablesChunkedQueued() {
            this.AsyncSetMultipleTables(new MutatorSpec(MutatorKind.Chunked) { Queued = true, Capacity = 100, FlushEachChunk = true, MaxCellCount = 100 });
        }

        [TestMethod]
        public void AsyncSetMultipleTablesQueued() {
            this.AsyncSetMultipleTables(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void AsyncSetQueued() {
            this.AsyncSet(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void AsyncSetThreaded() {
            this.AsyncSetThreaded(null);
        }

        public void AsyncSetThreaded(MutatorSpec mutatorSpec) {
            var key = new Key { ColumnFamily = "a" };
            using (var asyncResult = new AsyncResult()) {
                using (var mutator = table.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    int c1 = 0;
                    int c2 = 0;

                    var t1 = new Thread(
                        () => {
                            for (int n = 0; n < Count; ++n, ++c1) {
                                key.Row = Guid.NewGuid().ToString();
                                mutator.Set(key, Encoding.GetBytes(key.Row));
                            }
                        }) { IsBackground = true };

                    var t2 = new Thread(
                        () => {
                            for (int n = 0; n < Count; ++n, ++c2) {
                                key.Row = Guid.NewGuid().ToString();
                                mutator.Set(key, Encoding.GetBytes(key.Row));
                            }
                        }) { IsBackground = true };

                    t1.Start();
                    t2.Start();
                    t1.Join();
                    t2.Join();
                    Assert.IsTrue(c1 > 0);
                    Assert.IsTrue(c2 > 0);
                }
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
        }

        [TestMethod]
        public void AsyncSetThreadedChunked() {
            this.AsyncSetThreaded(ChunkedMutatorSpec);
        }

        [TestMethod]
        public void AsyncSetThreadedChunkedQueued() {
            this.AsyncSetThreaded(ChunkedQueuedMutatorSpec);
        }

        [TestMethod]
        public void AsyncSetThreadedQueued() {
            this.AsyncSetThreaded(MutatorSpec.CreateQueued());
        }

        [TestInitialize]
        public void TestInitialize() {
            Delete(table);
        }

        #endregion

        #region Methods

        private static int GetCellCount(Table t) {
            if (t == null) {
                throw new ArgumentNullException("t");
            }
            int c = 0;
            using (var scanner = t.CreateScanner()) {
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    ++c;
                }
            }
            return c;
        }

        private int GetCellCount() {
            return GetCellCount(table);
        }

        #endregion
    }
}
