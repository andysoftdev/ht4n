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

namespace Hypertable.Test
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;

    using Hypertable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test table mutators.
    /// </summary>
    [TestClass]
    public class TestTableMutator : TestBase
    {
        #region Constants and Fields

        private const int Count = 1000;

        private static readonly MutatorSpec ChunkedMutatorSpec = MutatorSpec.CreateChunked();

        private static readonly MutatorSpec ChunkedQueuedMutatorSpec = new MutatorSpec(MutatorKind.Chunked) { Queued = true, Capacity = 200 };

        private static readonly UTF8Encoding Encoding = new UTF8Encoding();

        private static ITable table;

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
            const string Schema =
                "<Schema><AccessGroup name=\"default\" blksz=\"1024\">" +
                "<ColumnFamily><Name>a</Name></ColumnFamily>" +
                "<ColumnFamily><Name>b</Name></ColumnFamily>" +
                "<ColumnFamily><Name>c</Name></ColumnFamily>" +
                "</AccessGroup></Schema>";

            table = EnsureTable(typeof(TestTableMutator), Schema);

            if (!HasAsyncTableMutator) {
                Assert.IsFalse(IsHyper);
                Assert.IsFalse(IsThrift);
            }
        }

        [TestMethod]
        public void Delete() {
            this.Delete(null);
        }

        public void Delete(MutatorSpec mutatorSpec) {
            var key = new Key { ColumnFamily = "a" };

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                key.Row = "Row does not exist";
                mutator.Set(key, Encoding.GetBytes(key.Row));
            }

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                for (var n = 0; n < Count; ++n) {
                    key.Row = Guid.NewGuid().ToString();
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                }
            }

            var rowKeys = new SortedSet<string>();
            using (var scanner = table.CreateScanner()) {
                var cell = new Cell();
                while( scanner.Move(cell) ) {
                    rowKeys.Add(cell.Key.Row);
                }
            }

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                foreach (var rowKey in rowKeys) {
                    mutator.Delete(rowKey);
                }
            }

            Assert.AreEqual(0, this.GetCellCount());

            key = new Key { ColumnFamily = "a" };
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                for (var n = 0; n < Count; ++n) {
                    key.Row = Guid.NewGuid().ToString();
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                }
            }

            rowKeys = new SortedSet<string>();
            using (var scanner = table.CreateScanner()) {
                var cell = new Cell();
                while( scanner.Move(cell) ) {
                    rowKeys.Add(cell.Key.Row);
                }
            }

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                key = new Key();
                foreach (var rowKey in rowKeys) {
                    key.Row = rowKey;
                    mutator.Delete(key);
                }
            }

            Assert.AreEqual(0, this.GetCellCount());

            key = new Key { ColumnFamily = "a" };
            var keys = new List<Key>();
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                for (var n = 0; n < Count; ++n) {
                    key.Row = Guid.NewGuid().ToString();
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                    keys.Add((Key)key.Clone());
                }
            }

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Delete(keys);
            }

            Assert.AreEqual(0, this.GetCellCount());

            key = new Key { ColumnFamily = "a", ColumnQualifier = null };
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                for (var n = 0; n < Count; ++n) {
                    key.Row = Guid.NewGuid().ToString();
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                }
            }

            var cells = new List<Cell>();
            using (var scanner = table.CreateScanner()) {
                Cell cell;
                while (scanner.Next(out cell)) {
                    cells.Add(cell);
                }
            }

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Delete(cells);
            }

            Assert.AreEqual(0, this.GetCellCount());

            key = new Key { ColumnFamily = "a", ColumnQualifier = string.Empty };
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                for (var n = 0; n < Count; ++n) {
                    key.Row = Guid.NewGuid().ToString();
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                }
            }

            cells = new List<Cell>();
            using (var scanner = table.CreateScanner()) {
                Cell cell;
                while (scanner.Next(out cell)) {
                    cells.Add(cell);
                }
            }

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Delete(cells);
            }

            Assert.AreEqual(0, this.GetCellCount());

            key = new Key();
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                key.Row = Guid.NewGuid().ToString();
                key.ColumnFamily = "a";
                mutator.Set(key, Encoding.GetBytes(key.Row));
                key.ColumnFamily = "b";
                mutator.Set(key, Encoding.GetBytes(key.Row));
            }

            Assert.AreEqual(2, this.GetCellCount());

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Delete(key);
            }

            using (var scanner = table.CreateScanner()) {
                var cell = new Cell();
                while( scanner.Move(cell) ) {
                    Assert.AreEqual("a", cell.Key.ColumnFamily);
                }
            }

            Assert.AreEqual(1, this.GetCellCount());

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                key.ColumnFamily = "b";
                key.ColumnQualifier = "1";
                mutator.Set(key, Encoding.GetBytes(key.Row));
                key.ColumnQualifier = "2";
                mutator.Set(key, Encoding.GetBytes(key.Row));
                key.ColumnQualifier = "3";
                mutator.Set(key, Encoding.GetBytes(key.Row));
                key.ColumnQualifier = string.Empty;
                mutator.Set(key, Encoding.GetBytes(key.Row));
                key.ColumnQualifier = "4";
                mutator.Set(key, Encoding.GetBytes(key.Row));
            }

            Assert.AreEqual(6, this.GetCellCount());

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Delete(key);
            }

            Assert.AreEqual(5, this.GetCellCount());

            key.ColumnQualifier = "1";
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Delete(key);
            }

            Assert.AreEqual(4, this.GetCellCount());

            key.ColumnQualifier = string.Empty;
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Delete(key);
            }

            Assert.AreEqual(3, this.GetCellCount());

            key.ColumnQualifier = null;
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Delete(key);
            }

            using (var scanner = table.CreateScanner()) {
                var cell = new Cell();
                while( scanner.Move(cell) ) {
                    Assert.AreEqual("a", cell.Key.ColumnFamily);
                }
            }

            Assert.AreEqual(1, this.GetCellCount());

            key.ColumnFamily = string.Empty; // usually use null
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Delete(key);
            }

            Assert.AreEqual(0, this.GetCellCount());

            key = new Key(); // FIXME(remove this line), issue 'delete with unspecified timestamp applying to all future inserts'

            var dateTimes = new List<DateTime>();
            using (var mutator = table.CreateMutator(mutatorSpec)) {
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

            Assert.AreEqual(6, dateTimes.Count);
            Assert.AreEqual(6, this.GetCellCount());

            key.DateTime = dateTimes[1];
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Delete(key);
            }

            Assert.AreEqual(4, this.GetCellCount());

            key.ColumnQualifier = null;
            key.DateTime = dateTimes[3];
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Delete(key);
            }

            Assert.AreEqual(2, this.GetCellCount());

            key.ColumnFamily = null;
            key.ColumnQualifier = null;
            key.DateTime = dateTimes[4];
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Delete(key);
            }

            Assert.AreEqual(1, this.GetCellCount());

            key.ColumnFamily = null;
            key.ColumnQualifier = null;
            key.DateTime = dateTimes[5];
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Delete(key);
            }

            Assert.AreEqual(0, this.GetCellCount());
        }

        [TestMethod]
        public void DeleteChunked() {
            this.Delete(ChunkedMutatorSpec);
        }

        [TestMethod]
        public void DeleteChunkedQueued() {
            this.Delete(ChunkedQueuedMutatorSpec);
        }

        [TestMethod]
        public void DeleteQueued() {
            this.Delete(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void DeleteSet() {
            this.DeleteSet(null);
        }

        public void DeleteSet(MutatorSpec mutatorSpec) {
            Cell cell;
            var key = new Key();
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                for (var n = 0; n < Count; ++n) {
                    key.Row = Guid.NewGuid().ToString();
                    key.ColumnFamily = "a";
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                    key.ColumnFamily = "b";
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                }
            }

            var cells = new List<Cell>();
            using (var scanner = table.CreateScanner()) {
                while (scanner.Next(out cell)) {
                    cells.Add(new Cell(new Key(cell.Key.Row), CellFlag.DeleteRow));
                }
            }

            Assert.AreEqual(2 * Count, cells.Count);

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Set(cells);
            }

            Assert.AreEqual(0, this.GetCellCount());

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                for (var n = 0; n < Count; ++n) {
                    key.Row = Guid.NewGuid().ToString();
                    key.ColumnFamily = "a";
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                    key.ColumnFamily = "b";
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                }
            }

            cells.Clear();
            using (var scanner = table.CreateScanner()) {
                while (scanner.Next(out cell)) {
                    cells.Add(new Cell(cell.Key, CellFlag.DeleteRow));
                }
            }

            Assert.AreEqual(2 * Count, cells.Count);

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Set(cells);
            }

            Assert.AreEqual(0, this.GetCellCount());

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                for (var n = 0; n < Count; ++n) {
                    key.Row = Guid.NewGuid().ToString();
                    key.ColumnFamily = "a";
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                    key.ColumnFamily = "b";
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                }
            }

            cells.Clear();
            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("b"))) {
                while (scanner.Next(out cell)) {
                    cells.Add(new Cell(cell.Key, CellFlag.DeleteColumnFamily));
                }
            }

            Assert.AreEqual(Count, cells.Count);

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Set(cells);
            }

            Assert.AreEqual(Count, this.GetCellCount());

            cells.Clear();
            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("a"))) {
                while (scanner.Next(out cell)) {
                    cells.Add(new Cell(cell.Key, CellFlag.DeleteCell));
                }
            }

            Assert.AreEqual(Count, this.GetCellCount());

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Set(cells);
            }

            Assert.AreEqual(0, this.GetCellCount());

            Cell cellDelete;
            key = new Key();
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                key.Row = Guid.NewGuid().ToString();
                key.ColumnFamily = "a";
                mutator.Set(key, Encoding.GetBytes(key.Row));
                key.ColumnFamily = "b";
                mutator.Set(key, Encoding.GetBytes(key.Row));
                cellDelete = new Cell(key, CellFlag.DeleteCell, true);
            }

            Assert.AreEqual(2, this.GetCellCount());

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Set(cellDelete);
            }

            using (var scanner = table.CreateScanner()) {
                while (scanner.Next(out cell)) {
                    Assert.AreEqual("a", cell.Key.ColumnFamily);
                }
            }

            Assert.AreEqual(1, this.GetCellCount());

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                key.ColumnFamily = "b";
                key.ColumnQualifier = "1";
                mutator.Set(key, Encoding.GetBytes(key.Row));
                key.ColumnQualifier = "2";
                mutator.Set(key, Encoding.GetBytes(key.Row));
                key.ColumnQualifier = "3";
                mutator.Set(key, Encoding.GetBytes(key.Row));
                key.ColumnQualifier = string.Empty;
                mutator.Set(key, Encoding.GetBytes(key.Row));
                key.ColumnQualifier = "4";
                mutator.Set(key, Encoding.GetBytes(key.Row));
                cellDelete = new Cell(key, CellFlag.DeleteCell, true);
            }

            Assert.AreEqual(6, this.GetCellCount());

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Set(cellDelete);
            }

            Assert.AreEqual(5, this.GetCellCount());

            cellDelete.Key.ColumnQualifier = "1";
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Set(cellDelete);
            }

            Assert.AreEqual(4, this.GetCellCount());

            cellDelete.Key.ColumnQualifier = string.Empty;
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Set(cellDelete);
            }

            Assert.AreEqual(3, this.GetCellCount());

            cellDelete.Key.ColumnQualifier = null;
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Set(cellDelete);
            }

            using (var scanner = table.CreateScanner()) {
                while (scanner.Next(out cell)) {
                    Assert.AreEqual("a", cell.Key.ColumnFamily);
                }
            }

            Assert.AreEqual(1, this.GetCellCount());

            cellDelete.Key.ColumnFamily = string.Empty; // usually use null
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Set(cellDelete);
            }

            Assert.AreEqual(0, this.GetCellCount());

            key = new Key(); // FIXME(remove this line), issue 'delete with unspecified timestamp applying to all future inserts'

            var dateTimes = new List<DateTime>();
            using (var mutator = table.CreateMutator(mutatorSpec)) {
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

            Assert.AreEqual(8, this.GetCellCount());
            Assert.AreEqual(8, dateTimes.Count);

            cells.Clear();
            using (var scanner = table.CreateScanner()) {
                while (scanner.Next(out cell)) {
                    cells.Add(cell);
                }
            }

            Assert.AreEqual(8, cells.Count);

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Set(new Cell(cells[7].Key, CellFlag.DeleteCellVersion, true));
            }

            Assert.AreEqual(7, this.GetCellCount());

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Set(new Cell(cells[1].Key, CellFlag.DeleteCellVersion, true));
            }

            Assert.AreEqual(6, this.GetCellCount());

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Set(new Cell(cells[0].Key, CellFlag.DeleteCellVersion, true));
            }

            Assert.AreEqual(5, this.GetCellCount());

            cellDelete.Key.DateTime = dateTimes[1];
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Set(cellDelete);
            }

            Assert.AreEqual(4, this.GetCellCount());

            cellDelete.Key.ColumnQualifier = null;
            cellDelete.Key.DateTime = dateTimes[3];
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Set(cellDelete);
            }

            Assert.AreEqual(2, this.GetCellCount());

            cellDelete.Key.ColumnFamily = null;
            cellDelete.Key.ColumnQualifier = null;
            cellDelete.Key.DateTime = dateTimes[4];
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Set(cellDelete);
            }

            Assert.AreEqual(1, this.GetCellCount());

            cellDelete.Key.ColumnFamily = null;
            cellDelete.Key.ColumnQualifier = null;
            cellDelete.Key.DateTime = dateTimes[5];
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Set(cellDelete);
            }

            Assert.AreEqual(0, this.GetCellCount());
        }

        [TestMethod]
        public void DeleteSetChunked() {
            this.DeleteSet(ChunkedMutatorSpec);
        }

        [TestMethod]
        public void DeleteSetChunkedQueued() {
            this.DeleteSet(ChunkedQueuedMutatorSpec);
        }

        [TestMethod]
        public void DeleteSetQueued() {
            this.DeleteSet(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void Revisions() {
            this.Revisions(null);
        }

        public void Revisions(MutatorSpec mutatorSpec) {
            Assert.AreEqual(0, this.GetCellCount());

            var key = new Key(Guid.NewGuid().ToString()) { ColumnFamily = "a", ColumnQualifier = "tag" };

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Set(key, Encoding.GetBytes(key.Row));
                mutator.Flush();

                Cell cell1;
                Cell cellNone;
                using (var scanner = table.CreateScanner(new ScanSpec(key.Row).AddColumn("a"))) {
                    Assert.IsTrue(scanner.Next(out cell1));
                    Assert.IsFalse(scanner.Next(out cellNone));
                }

                Assert.AreEqual(cell1.Key.Row, Encoding.GetString(cell1.Value));

                mutator.Set(key, Encoding.GetBytes("abc"));
                mutator.Flush();

                Cell cell2;
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
                mutator.Flush();

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
        }

        [TestMethod]
        public void RevisionsChunked() {
            this.Revisions(ChunkedMutatorSpec);
        }

        [TestMethod]
        public void RevisionsChunkedQueued() {
            this.Revisions(ChunkedQueuedMutatorSpec);
        }

        [TestMethod]
        public void RevisionsQueued() {
            this.Revisions(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void Set() {
            this.Set(null);
        }

        public void Set(MutatorSpec mutatorSpec) {
            var key = new Key { ColumnFamily = "a" };
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                for (var n = 0; n < Count; ++n) {
                    key.Row = Guid.NewGuid().ToString();
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                }
            }

            Assert.AreEqual(Count, this.GetCellCount());
        }

        [TestMethod]
        public void SetChunked() {
            this.Set(new MutatorSpec(MutatorKind.Chunked) { FlushEachChunk = true, MaxCellCount = 100 });
        }

        [TestMethod]
        public void SetChunkedQueued() {
            this.Set(new MutatorSpec(MutatorKind.Chunked) { Queued = true, Capacity = 100, FlushEachChunk = true, MaxCellCount = 100 });
        }

        [TestMethod]
        public void SetCollection() {
            this.SetCollection(null);
        }

        public void SetCollection(MutatorSpec mutatorSpec) {
            var key = new Key { ColumnFamily = "a" };
            var cells = new List<Cell>();
            for (var n = 0; n < Count; ++n) {
                key.Row = Guid.NewGuid().ToString();
                cells.Add(new Cell(key, Encoding.GetBytes(key.Row), true));
            }

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Set(cells);
            }

            Assert.AreEqual(Count, this.GetCellCount());
        }

        [TestMethod]
        public void SetCollectionChunked() {
            this.SetCollection(ChunkedMutatorSpec);
        }

        [TestMethod]
        public void SetCollectionChunkedQueued() {
            this.SetCollection(ChunkedQueuedMutatorSpec);
        }

        [TestMethod]
        public void SetCollectionCreateKey() {
            this.SetCollectionCreateKey(null);
        }

        public void SetCollectionCreateKey(MutatorSpec mutatorSpec) {
            var key = new Key { ColumnFamily = "a" };
            var cells = new List<Cell>();
            for (var n = 0; n < Count; ++n) {
                cells.Add(new Cell(key.Clone() as Key, Encoding.GetBytes(Guid.NewGuid().ToString())));
            }

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Set(cells, true);
            }

            foreach (var cell in cells) {
                Assert.IsFalse(string.IsNullOrEmpty(cell.Key.Row));
            }

            Assert.AreEqual(Count, this.GetCellCount());
        }

        [TestMethod]
        public void SetCollectionCreateKeyChunked() {
            this.SetCollectionCreateKey(ChunkedMutatorSpec);
        }

        [TestMethod]
        public void SetCollectionCreateKeyChunkedQueued() {
            this.SetCollectionCreateKey(ChunkedQueuedMutatorSpec);
        }

        [TestMethod]
        public void SetCollectionCreateKeyLazy() {
            this.SetCollectionCreateKeyLazy(null);
        }

        public void SetCollectionCreateKeyLazy(MutatorSpec mutatorSpec) {
            var key = new Key { ColumnFamily = "a" };
            var cells = new List<Cell>();
            for (var n = 0; n < Count; ++n) {
                key.Row = (n % 3) == 0 ? Guid.NewGuid().ToString() : null;
                cells.Add(new Cell(key.Clone() as Key, Encoding.GetBytes(Guid.NewGuid().ToString())));
            }

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Set(cells);
            }

            foreach (var cell in cells) {
                Assert.IsFalse(string.IsNullOrEmpty(cell.Key.Row));
            }

            Assert.AreEqual(Count, this.GetCellCount());
        }

        [TestMethod]
        public void SetCollectionCreateKeyLazyChunked() {
            this.SetCollectionCreateKeyLazy(ChunkedMutatorSpec);
        }

        [TestMethod]
        public void SetCollectionCreateKeyLazyChunkedQueued() {
            this.SetCollectionCreateKeyLazy(ChunkedQueuedMutatorSpec);
        }

        [TestMethod]
        public void SetCollectionCreateKeyLazyKeyQueued() {
            this.SetCollectionCreateKeyLazy(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void SetCollectionCreateKeyQueued() {
            this.SetCollectionCreateKey(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void SetCollectionDifferentSizedValues() {
            this.SetCollectionDifferentSizedValues(null);
        }

        public void SetCollectionDifferentSizedValues(MutatorSpec mutatorSpec) {
            var sb = new StringBuilder();
            for (var n = 0; n < 0x40; ++n) {
                sb.Append(Guid.NewGuid().ToString());
            }

            var largeValue = Encoding.GetBytes(sb.ToString());
            for (var n = 0; n < 0x4000; ++n) {
                sb.Append(Guid.NewGuid().ToString());
            }

            var hugeValue = Encoding.GetBytes(sb.ToString());
            var smallValue = Encoding.GetBytes(Guid.NewGuid().ToString());

            var key = new Key { ColumnFamily = "a" };
            var cells = new List<Cell> {
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

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                mutator.Set(cells, true);
            }

            foreach (var cell in cells) {
                Assert.IsFalse(string.IsNullOrEmpty(cell.Key.Row));
            }
        }

        [TestMethod]
        public void SetCollectionDifferentSizedValuesChunked() {
            this.SetCollectionDifferentSizedValues(ChunkedMutatorSpec);
        }

        [TestMethod]
        public void SetCollectionDifferentSizedValuesChunkedQueued() {
            this.SetCollectionDifferentSizedValues(ChunkedQueuedMutatorSpec);
        }

        [TestMethod]
        public void SetCollectionDifferentSizedValuesQueued() {
            this.SetCollectionDifferentSizedValues(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void SetCollectionHugeValues() {
            this.SetCollectionHugeValues(null);
        }

        public void SetCollectionHugeValues(MutatorSpec mutatorSpec) {
            const int K = 1024;
            const int M = K * K;
            var hugeValue = new byte[M];
            new Random().NextBytes(hugeValue);

            var key = new Key { ColumnFamily = "a" };
            var cells = new List<Cell> {
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

            using (var mutator = table.CreateMutator()) {
                mutator.Set(cells, true);
            }

            foreach (var cell in cells) {
                Assert.IsFalse(string.IsNullOrEmpty(cell.Key.Row));
            }
        }

        [TestMethod]
        public void SetCollectionHugeValuesChunked() {
            this.SetCollectionHugeValues(ChunkedMutatorSpec);
        }

        [TestMethod]
        public void SetCollectionHugeValuesChunkedQueued() {
            this.SetCollectionHugeValues(ChunkedQueuedMutatorSpec);
        }

        [TestMethod]
        public void SetCollectionHugeValuesQueued() {
            this.SetCollectionHugeValues(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void SetCollectionQueued() {
            this.SetCollection(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void SetCollectionThreaded() {
            this.SetCollectionThreaded(null);
        }

        public void SetCollectionThreaded(MutatorSpec mutatorSpec) {
            var key = new Key { ColumnFamily = "a" };
            var cells1 = new List<Cell>();
            for (var n = 0; n < 16; ++n) {
                cells1.Add(new Cell(key.Clone() as Key, Encoding.GetBytes(Guid.NewGuid().ToString())));
            }

            var cells2 = new List<Cell>();
            for (var n = 0; n < 16; ++n) {
                cells2.Add(new Cell(key.Clone() as Key, Encoding.GetBytes(Guid.NewGuid().ToString())));
            }

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                var c1 = 0;
                var c2 = 0;

                var t1 = new Thread(
                    () =>
                        {
                            for (var n = 0; n < Count; ++n, ++c1) {
                                mutator.Set(cells1, true);
                                if (n == Count / 2) {
                                    mutator.Flush();
                                }
                            }
                        });

                var t2 = new Thread(
                    () =>
                        {
                            for (var n = 0; n < Count; ++n, ++c2) {
                                mutator.Set(cells2, true);
                                if (n == Count / 2) {
                                    mutator.Flush();
                                }
                            }
                        });

                t1.Start();
                t2.Start();
                t1.Join();
                t2.Join();
                Assert.IsTrue(c1 > 0);
                Assert.IsTrue(c2 > 0);
            }
        }

        [TestMethod]
        public void SetCollectionThreadedChunked() {
            this.SetCollectionThreaded(ChunkedMutatorSpec);
        }

        [TestMethod]
        public void SetCollectionThreadedChunkedQueued() {
            this.SetCollectionThreaded(ChunkedQueuedMutatorSpec);
        }

        [TestMethod]
        public void SetCollectionThreadedQueued() {
            this.SetCollectionThreaded(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void SetCreateKey() {
            this.SetCreateKey(null);
        }

        public void SetCreateKey(MutatorSpec mutatorSpec) {
            var key = new Key { ColumnFamily = "b" };
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                for (var n = 0; n < Count; ++n) {
                    mutator.Set(key, Encoding.GetBytes(Guid.NewGuid().ToString()), true);
                    Assert.IsFalse(string.IsNullOrEmpty(key.Row));
                }
            }

            Assert.AreEqual(Count, this.GetCellCount());
        }

        [TestMethod]
        public void SetCreateKeyChunked() {
            this.SetCreateKey(ChunkedMutatorSpec);
        }

        [TestMethod]
        public void SetCreateKeyChunkedQueued() {
            this.SetCreateKey(ChunkedQueuedMutatorSpec);
        }

        [TestMethod]
        public void SetCreateKeyLazy() {
            this.SetCreateKeyLazy(null);
        }

        public void SetCreateKeyLazy(MutatorSpec mutatorSpec) {
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                for (var n = 0; n < Count; ++n) {
                    var key = new Key { ColumnFamily = "b" };
                    mutator.Set(key, Encoding.GetBytes(Guid.NewGuid().ToString()));
                    Assert.IsFalse(string.IsNullOrEmpty(key.Row));
                }
            }

            Assert.AreEqual(Count, this.GetCellCount());
        }

        [TestMethod]
        public void SetCreateKeyLazyChunked() {
            this.SetCreateKeyLazy(ChunkedMutatorSpec);
        }

        [TestMethod]
        public void SetCreateKeyLazyChunkedQueued() {
            this.SetCreateKeyLazy(ChunkedQueuedMutatorSpec);
        }

        [TestMethod]
        public void SetCreateKeyLazyQueued() {
            this.SetCreateKeyLazy(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void SetCreateKeyQueued() {
            this.SetCreateKey(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void SetPeriodicFlush() {
            this.SetPeriodicFlush(new MutatorSpec(MutatorKind.Default) { FlushInterval = TimeSpan.FromSeconds(1) });
        }

        public void SetPeriodicFlush(MutatorSpec mutatorSpec) {
            if (!HasPeriodicFlushTableMutator) {
                return;
            }

            using (var mutator = table.CreateMutator(mutatorSpec)) {
                for (var i = 0; i < 5; ++i) {
                    var row = string.Format("periodicFlush-{0}", i);
                    var scanSpec = new ScanSpec(row).AddColumn("a");
                    var key = new Key(row, "a");
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                    using (var scanner = table.CreateScanner(scanSpec)) {
                        Cell cell;
                        Assert.IsFalse(scanner.Next(out cell), string.Format("iteration {0}", i));
                    }

                    Thread.Sleep(3000); // wait enough

                    using (var scanner = table.CreateScanner(scanSpec)) {
                        Cell cell;
                        Assert.IsTrue(scanner.Next(out cell), string.Format("iteration {0}", i));
                        Assert.AreEqual(row, cell.Key.Row);
                    }
                }
            }
        }

        [TestMethod]
        public void SetPeriodicFlushChunked() {
            this.SetPeriodicFlush(new MutatorSpec(MutatorKind.Chunked) { FlushInterval = TimeSpan.FromSeconds(1), FlushEachChunk = false, MaxCellCount = 1 });
        }

        [TestMethod]
        public void SetPeriodicFlushChunkedQueued() {
            this.SetPeriodicFlush(new MutatorSpec(MutatorKind.Chunked) { FlushInterval = TimeSpan.FromSeconds(1), FlushEachChunk = false, MaxCellCount = 1, Queued = true });
        }

        [TestMethod]
        public void SetPeriodicFlushQueued() {
            this.SetPeriodicFlush(new MutatorSpec(MutatorKind.Default) { FlushInterval = TimeSpan.FromSeconds(1), Queued = true });
        }

        [TestMethod]
        public void SetQueued() {
            this.Set(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void SetThreaded() {
            this.SetThreaded(null);
        }

        public void SetThreaded(MutatorSpec mutatorSpec) {
            using (var mutator = table.CreateMutator(mutatorSpec)) {
                var c1 = 0;
                var c2 = 0;

                var t1 = new Thread(
                    () =>
                        {
                            var key = new Key { ColumnFamily = "a" };
                            for (var n = 0; n < Count; ++n, ++c1) {
                                key.Row = Guid.NewGuid().ToString();
                                mutator.Set(key, Encoding.GetBytes(key.Row));
                            }
                        });

                var t2 = new Thread(
                    () =>
                        {
                            var key = new Key { ColumnFamily = "a" };
                            for (var n = 0; n < Count; ++n, ++c2) {
                                key.Row = Guid.NewGuid().ToString();
                                mutator.Set(key, Encoding.GetBytes(key.Row));
                            }
                        });

                t1.Start();
                t2.Start();
                t1.Join();
                t2.Join();
                Assert.IsTrue(c1 > 0);
                Assert.IsTrue(c2 > 0);
            }
        }

        [TestMethod]
        public void SetThreadedChunked() {
            this.SetThreaded(ChunkedMutatorSpec);
        }

        [TestMethod]
        public void SetThreadedChunkedQueued() {
            this.SetThreaded(ChunkedQueuedMutatorSpec);
        }

        [TestMethod]
        public void SetThreadedQueued() {
            this.SetThreaded(MutatorSpec.CreateQueued());
        }

        [TestInitialize]
        public void TestInitialize() {
            TestBase.ContinueExecution();
            Delete(table);
        }

        #endregion

        #region Methods

        private static int GetCellCount(ITable _table) {
            var c = 0;
            using (var scanner = _table.CreateScanner()) {
                var cell = new Cell();
                while( scanner.Move(cell) ) {
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