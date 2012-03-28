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

namespace Hypertable.Test
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;

    using Hypertable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test dealing with multiple Hypertable instances.
    /// </summary>
    [TestClass]
    [Ignore] // TODO
    public class TestMultipleInstances : TestBase
    {
        #region Constants and Fields

        private const int CountA = 100000;

        private const int CountB = 10000;

        private const int CountC = 1000;

        private const string UriB = "localhost"; // TODO, check also with 2nd host different to localhost

        private static readonly UTF8Encoding Encoding = new UTF8Encoding();

        private static IClient clientB;

        private static IContext contextB;

        private static INamespace nsB;

        private static ITable tableA;

        private static ITable tableB;

        #endregion

        #region Public Methods

        [ClassCleanup]
        public static void ClassCleanup() {
            tableB.Dispose();
            nsB.Dispose();
            clientB.DropNamespace(NsName, DropDispositions.Complete);
            clientB.Dispose();
            contextB.Dispose();

            tableA.Dispose();
        }

        [ClassInitialize]
        public static void ClassInitialize(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext) {
            const string Schema =
                "<Schema><AccessGroup name=\"default\" blksz=\"1024\">" +
                "<ColumnFamily><Name>a</Name></ColumnFamily>" +
                "<ColumnFamily><Name>b</Name></ColumnFamily>" +
                "<ColumnFamily><Name>c</Name></ColumnFamily>" +
                "</AccessGroup></Schema>";

            tableA = EnsureTable(typeof(TestMultipleInstances), Schema);

            var properties = new Dictionary<string, object> { { "Uri", UriB } };

            contextB = Hypertable.Context.Create(ConnectionString, properties);
            clientB = contextB.CreateClient();
            nsB = clientB.OpenNamespace(NsName, OpenDispositions.OpenAlways);
            nsB.DropTable(typeof(TestMultipleInstances).Name, DropDispositions.IfExists);
            nsB.CreateTable(typeof(TestMultipleInstances).Name, Schema);
            tableB = nsB.OpenTable(typeof(TestMultipleInstances).Name);
        }

        [TestMethod]
        public void AsyncSetAccrossInstances() {
            this.AsyncSetAccrossInstances(null);
        }

        public void AsyncSetAccrossInstances(MutatorSpec mutatorSpec) {
            if (IsThrift) {
                return; // TODO, check what is wrong
            }

            var key = new Key { ColumnFamily = "a" };
            using (var asyncResult = new AsyncResult()) {
                using (var mutatorA = tableA.CreateAsyncMutator(asyncResult, mutatorSpec))
                using (var mutatorB = tableB.CreateAsyncMutator(asyncResult, mutatorSpec)) {
                    for (var n = 0; n < CountA; ++n) {
                        key.Row = Guid.NewGuid().ToString();
                        mutatorA.Set(key, Encoding.GetBytes(key.Row));
                        key.Row = Guid.NewGuid().ToString();
                        mutatorB.Set(key, Encoding.GetBytes(key.Row));
                    }
                }

                asyncResult.Join();
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : string.Empty);
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.AreEqual(CountA, GetCellCount(tableA));
                Assert.AreEqual(CountA, GetCellCount(tableB));
            }
        }

        [TestMethod]
        public void AsyncSetAccrossInstancesChunked() {
            this.AsyncSetAccrossInstances(new MutatorSpec(MutatorKind.Chunked) { FlushEachChunk = true, MaxCellCount = 100 });
        }

        [TestMethod]
        public void AsyncSetAccrossInstancesChunkedQueued() {
            this.AsyncSetAccrossInstances(new MutatorSpec(MutatorKind.Chunked) { Queued = true, Capacity = 100, FlushEachChunk = true, MaxCellCount = 100 });
        }

        [TestMethod]
        public void AsyncSetAccrossInstancesQueued() {
            this.AsyncSetAccrossInstances(MutatorSpec.CreateQueued());
        }

        [TestMethod]
        public void Copy() {
            var key = new Key { ColumnFamily = "a" };
            using (var mutator = tableA.CreateMutator()) {
                for (var n = 0; n < CountC; ++n) {
                    key.Row = "A" + Guid.NewGuid().ToString();
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                }
            }

            key = new Key { ColumnFamily = "b" };
            using (var mutator = tableB.CreateMutator()) {
                for (var n = 0; n < CountC; ++n) {
                    key.Row = "B" + Guid.NewGuid().ToString();
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                }
            }

            using (var scanner = tableA.CreateScanner(new ScanSpec().AddColumn("a")))
            using (var mutator = tableB.CreateMutator()) {
                var c = 0;
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    Assert.IsTrue(cell.Key.Row.StartsWith("A"));
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    mutator.Set(cell);
                    ++c;
                }

                Assert.AreEqual(CountC, c);
            }

            using (var scanner = tableB.CreateScanner(new ScanSpec().AddColumn("b")))
            using (var mutator = tableA.CreateMutator()) {
                var c = 0;
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    Assert.IsTrue(cell.Key.Row.StartsWith("B"));
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    mutator.Set(cell);
                    ++c;
                }

                Assert.AreEqual(CountC, c);
            }

            using (var scanner = tableA.CreateScanner(new ScanSpec().AddColumn("a"))) {
                var c = 0;
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    Assert.IsTrue(cell.Key.Row.StartsWith("A"));
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountC, c);
            }

            using (var scanner = tableA.CreateScanner(new ScanSpec().AddColumn("b"))) {
                var c = 0;
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    Assert.IsTrue(cell.Key.Row.StartsWith("B"));
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountC, c);
            }

            using (var scanner = tableB.CreateScanner(new ScanSpec().AddColumn("a"))) {
                var c = 0;
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    Assert.IsTrue(cell.Key.Row.StartsWith("A"));
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountC, c);
            }

            using (var scanner = tableB.CreateScanner(new ScanSpec().AddColumn("b"))) {
                var c = 0;
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    Assert.IsTrue(cell.Key.Row.StartsWith("B"));
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountC, c);
            }

            using (var scannerA = tableA.CreateScanner(new ScanSpec().AddColumn("a")))
            using (var scannerB = tableB.CreateScanner(new ScanSpec().AddColumn("a"))) {
                var c = 0;
                var cellA = new Cell();
                var cellB = new Cell();
                while (scannerA.Next(cellA)) {
                    Assert.IsTrue(scannerB.Next(cellB));
                    Assert.IsTrue(cellA.Key.Row.StartsWith("A"));
                    Assert.AreEqual(cellA.Key.Row, Encoding.GetString(cellA.Value));
                    Assert.IsTrue(cellB.Key.Row.StartsWith("A"));
                    Assert.AreEqual(cellB.Key.Row, Encoding.GetString(cellB.Value));
                    Assert.AreEqual(cellA.Key, cellB.Key);
                    ++c;
                }

                Assert.IsFalse(scannerB.Next(cellB));
                Assert.AreEqual(CountC, c);
            }

            using (var scannerB = tableA.CreateScanner(new ScanSpec().AddColumn("b")))
            using (var scannerA = tableB.CreateScanner(new ScanSpec().AddColumn("b"))) {
                var c = 0;
                var cellB = new Cell();
                var cellA = new Cell();
                while (scannerB.Next(cellB)) {
                    Assert.IsTrue(scannerA.Next(cellA));
                    Assert.IsTrue(cellA.Key.Row.StartsWith("B"));
                    Assert.AreEqual(cellA.Key.Row, Encoding.GetString(cellA.Value));
                    Assert.IsTrue(cellB.Key.Row.StartsWith("B"));
                    Assert.AreEqual(cellB.Key.Row, Encoding.GetString(cellB.Value));
                    Assert.AreEqual(cellA.Key, cellB.Key);
                    ++c;
                }

                Assert.IsFalse(scannerA.Next(cellB));
                Assert.AreEqual(CountC, c);
            }
        }

        [TestMethod]
        public void Delete() {
            var key = new Key { ColumnFamily = "a" };
            using (var mutator = tableA.CreateMutator()) {
                for (var n = 0; n < CountC; ++n) {
                    key.Row = "A" + Guid.NewGuid().ToString();
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                }
            }

            key = new Key { ColumnFamily = "b" };
            using (var mutator = tableB.CreateMutator()) {
                for (var n = 0; n < CountC; ++n) {
                    key.Row = "B" + Guid.NewGuid().ToString();
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                }
            }

            using (var scanner = tableA.CreateScanner(new ScanSpec { KeysOnly = true })) {
                using (var mutator = tableA.CreateMutator()) {
                    var cell = new Cell();
                    while (scanner.Next(cell)) {
                        mutator.Delete(cell.Key);
                    }
                }
            }

            using (var scanner = tableA.CreateScanner()) {
                var c = 0;
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    Assert.IsTrue(cell.Key.Row.StartsWith("A"));
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(0, c);
            }

            using (var scanner = tableB.CreateScanner(new ScanSpec { KeysOnly = true })) {
                using (var mutator = tableB.CreateMutator()) {
                    var cell = new Cell();
                    while (scanner.Next(cell)) {
                        mutator.Delete(cell.Key);
                    }
                }
            }

            using (var scanner = tableB.CreateScanner()) {
                var c = 0;
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    Assert.IsTrue(cell.Key.Row.StartsWith("B"));
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(0, c);
            }
        }

        [TestMethod]
        public void ScanTableAccrossInstancesAsync() {
            if (IsThrift) {
                return; // TODO, check what is wrong
            }

            InitializeTableData(tableA);
            InitializeTableData(tableB);

            var c = 0;
            using (var asyncResult = new AsyncResult(
                (ctx, _cells) =>
                    {
                        foreach (var _cell in _cells) {
                            Assert.IsFalse(string.IsNullOrEmpty(_cell.Key.Row));
                            Interlocked.Increment(ref c);
                        }

                        return AsyncCallbackResult.Continue;
                    })) {
                tableA.BeginScan(asyncResult, new ScanSpec().AddColumn("a"));
                tableB.BeginScan(asyncResult, new ScanSpec().AddColumn("b"));
                asyncResult.Join();
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : string.Empty);
                Assert.IsTrue(asyncResult.IsCompleted);
            }

            Assert.AreEqual(CountA + CountB, c);

            c = 0;
            using (var asyncResult = new AsyncResult(
                (ctx, _cells) =>
                    {
                        foreach (var _cell in _cells) {
                            Assert.IsFalse(string.IsNullOrEmpty(_cell.Key.Row));
                            Interlocked.Increment(ref c);
                        }

                        return AsyncCallbackResult.Continue;
                    })) {
                tableA.BeginScan(asyncResult, new ScanSpec().AddColumn("b"));
                tableB.BeginScan(asyncResult, new ScanSpec().AddColumn("c"));
                asyncResult.Join();
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : string.Empty);
                Assert.IsTrue(asyncResult.IsCompleted);
            }

            Assert.AreEqual(CountB + CountC, c);
        }

        [TestMethod]
        public void ScanTableAccrossInstancesBlockingAsync() {
            if (IsThrift) {
                return; // TODO, check what is wrong
            }

            InitializeTableData(tableA);
            InitializeTableData(tableB);

            var c = 0;
            using (var asyncResult = new BlockingAsyncResult()) {
                tableA.BeginScan(asyncResult, new ScanSpec().AddColumn("a"));
                tableB.BeginScan(asyncResult, new ScanSpec().AddColumn("b"));
                IList<Cell> cells;
                while (asyncResult.TryGetCells(out cells)) {
                    foreach (var cell in cells) {
                        Assert.IsFalse(string.IsNullOrEmpty(cell.Key.Row));
                        ++c;
                    }
                }

                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : string.Empty);
                Assert.IsTrue(asyncResult.IsCompleted);
            }

            Assert.AreEqual(CountA + CountB, c);

            c = 0;
            using (var asyncResult = new BlockingAsyncResult()) {
                tableA.BeginScan(asyncResult, new ScanSpec().AddColumn("b"));
                tableB.BeginScan(asyncResult, new ScanSpec().AddColumn("c"));
                IList<Cell> cells;
                while (asyncResult.TryGetCells(out cells)) {
                    foreach (var cell in cells) {
                        Assert.IsFalse(string.IsNullOrEmpty(cell.Key.Row));
                        ++c;
                    }
                }

                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : string.Empty);
                Assert.IsTrue(asyncResult.IsCompleted);
            }

            Assert.AreEqual(CountB + CountC, c);
        }

        [TestMethod]
        public void Set() {
            var key = new Key { ColumnFamily = "a" };
            using (var mutator = tableA.CreateMutator()) {
                for (var n = 0; n < CountC; ++n) {
                    key.Row = "A" + Guid.NewGuid().ToString();
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                }
            }

            key = new Key { ColumnFamily = "b" };
            using (var mutator = tableB.CreateMutator()) {
                for (var n = 0; n < CountC; ++n) {
                    key.Row = "B" + Guid.NewGuid().ToString();
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                }
            }

            using (var scanner = tableA.CreateScanner()) {
                var c = 0;
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    Assert.IsTrue(cell.Key.Row.StartsWith("A"));
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountC, c);
            }

            using (var scanner = tableB.CreateScanner()) {
                var c = 0;
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    Assert.IsTrue(cell.Key.Row.StartsWith("B"));
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountC, c);
            }
        }

        [TestInitialize]
        public void TestInitialize() {
            Delete(tableA);
            Delete(tableB);
        }

        #endregion

        #region Methods

        private static int GetCellCount(ITable _table) {
            var c = 0;
            using (var scanner = _table.CreateScanner()) {
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    ++c;
                }
            }

            return c;
        }

        private static void InitializeTableData(ITable _table) {
            var key = new Key();
            using (var mutator = _table.CreateMutator(MutatorSpec.CreateChunked())) {
                for (var n = 0; n < CountA; ++n) {
                    key.ColumnFamily = "a";
                    key.Row = Guid.NewGuid().ToString();
                    mutator.Set(key, Encoding.GetBytes(key.Row));

                    if (n < CountB) {
                        key.ColumnFamily = "b";
                        mutator.Set(key, Encoding.GetBytes(key.Row));
                    }

                    if (n < CountC) {
                        key.ColumnFamily = "c";
                        mutator.Set(key, Encoding.GetBytes(key.Row));
                    }
                }
            }
        }

        #endregion
    }
}