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
    /// Test asynchronous table scanners.
    /// </summary>
    [TestClass]
    public class TestAsyncTableScanner : TestBase
    {
        #region Constants and Fields

        private const int CountA = 100000;

        private const int CountB = 10000;

        private const int CountC = 1000;

        private const string Schema =
            "<Schema>" + "<AccessGroup name=\"default\" blksz=\"1024\">" + "<ColumnFamily>" + "<Name>a</Name>" + "<deleted>false</deleted>" + "</ColumnFamily>" + "<ColumnFamily>" +
            "<Name>b</Name>" + "<deleted>false</deleted>" + "</ColumnFamily>" + "<ColumnFamily>" + "<Name>c</Name>" + "<deleted>false</deleted>" + "</ColumnFamily>" +
            "<ColumnFamily>" + "<Name>d</Name>" + "<deleted>false</deleted>" + "</ColumnFamily>" + "<ColumnFamily>" + "<Name>e</Name>" + "<deleted>false</deleted>" +
            "</ColumnFamily>" + "<ColumnFamily>" + "<Name>f</Name>" + "<deleted>false</deleted>" + "</ColumnFamily>" + "<ColumnFamily>" + "<Name>g</Name>" +
            "<deleted>false</deleted>" + "</ColumnFamily>" + "</AccessGroup>" + "</Schema>";

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
            table = EnsureTable(typeof(TestAsyncTableScanner), Schema);
            InitializeTableData(table);
        }

        [TestMethod]
        public void ScanMultipleTableAsync() {
            const int CountTables = 10;
            var tables = new List<Table>();
            try {
                for (int t = 0; t < CountTables; ++t) {
                    var testTable = EnsureTable(String.Format("ScanMultipleTableBlockingAsync-{0}", t), Schema);
                    InitializeTableData(testTable);
                    tables.Add(testTable);
                }

                int c = 0;
                using (var asyncResult = new AsyncResult(
                    (ctx, cells) => {
                        foreach (var cell in cells) {
                            Assert.IsFalse(String.IsNullOrEmpty(cell.Key.Row));
                            Interlocked.Increment(ref c);
                        }
                        return AsyncCallbackResult.Continue;
                    })) {
                    tables.ForEach(t => t.BeginScan(asyncResult, new ScanSpec().AddColumn("a")));
                    tables.ForEach(t => t.BeginScan(asyncResult, new ScanSpec().AddColumn("b")));
                    asyncResult.Join();
                    Assert.IsTrue(asyncResult.IsCompleted);
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                }
                Assert.AreEqual(CountTables * (CountA + CountB), c);

                c = 0;
                using (var asyncResult = new AsyncResult()) {
                    tables.ForEach(
                        t => t.BeginScan(
                            asyncResult,
                            new ScanSpec().AddColumn("b"),
                            (ctx, cells) => {
                                foreach (var cell in cells) {
                                    Assert.IsFalse(String.IsNullOrEmpty(cell.Key.Row));
                                    Interlocked.Increment(ref c);
                                }
                                return AsyncCallbackResult.Continue;
                            }));
                    tables.ForEach(
                        t => t.BeginScan(
                            asyncResult,
                            new ScanSpec().AddColumn("c"),
                            (ctx, cells) => {
                                foreach (var cell in cells) {
                                    Assert.IsFalse(String.IsNullOrEmpty(cell.Key.Row));
                                    Interlocked.Increment(ref c);
                                }
                                return AsyncCallbackResult.Continue;
                            }));
                    asyncResult.Join();
                    Assert.IsTrue(asyncResult.IsCompleted);
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                }
                Assert.AreEqual(CountTables * (CountB + CountC), c);
            }
            finally {
                tables.ForEach(t => t.Dispose());
                for (int t = 0; t < CountTables; ++t) {
                    Ns.DropTable(String.Format("ScanMultipleTableBlockingAsync-{0}", t), DropDispositions.IfExists);
                }
            }
        }

        [TestMethod]
        public void ScanMultipleTableBlockingAsync() {
            const int CountTables = 10;
            var tables = new List<Table>();
            try {
                for (int t = 0; t < CountTables; ++t) {
                    var testTable = EnsureTable(String.Format("ScanMultipleTableBlockingAsync-{0}", t), Schema);
                    InitializeTableData(testTable);
                    tables.Add(testTable);
                }

                int c = 0;
                using (var asyncResult = new BlockingAsyncResult()) {
                    tables.ForEach(t => t.BeginScan(asyncResult, new ScanSpec().AddColumn("a")));
                    tables.ForEach(t => t.BeginScan(asyncResult, new ScanSpec().AddColumn("b")));
                    IList<Cell> cells;
                    while (asyncResult.TryGetCells(out cells)) {
                        foreach (var cell in cells) {
                            Assert.IsFalse(String.IsNullOrEmpty(cell.Key.Row));
                            ++c;
                        }
                    }
                    Assert.IsTrue(asyncResult.IsCompleted);
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                }
                Assert.AreEqual(CountTables * (CountA + CountB), c);

                c = 0;
                using (var asyncResult = new BlockingAsyncResult()) {
                    tables.ForEach(t => t.BeginScan(asyncResult, new ScanSpec().AddColumn("b")));
                    tables.ForEach(t => t.BeginScan(asyncResult, new ScanSpec().AddColumn("c")));
                    IList<Cell> cells;
                    while (asyncResult.TryGetCells(out cells)) {
                        foreach (var cell in cells) {
                            Assert.IsFalse(String.IsNullOrEmpty(cell.Key.Row));
                            ++c;
                        }
                    }
                    Assert.IsTrue(asyncResult.IsCompleted);
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                }
                Assert.AreEqual(CountTables * (CountB + CountC), c);
            }
            finally {
                tables.ForEach(t => t.Dispose());
                for (int t = 0; t < CountTables; ++t) {
                    Ns.DropTable(String.Format("ScanMultipleTableBlockingAsync-{0}", t), DropDispositions.IfExists);
                }
            }
        }

        [TestMethod]
        public void ScanMultipleTableColumnFamilyAsync() {
            using (var table2 = EnsureTable("ScanMultipleTableColumnFamilyAsync", Schema)) {
                InitializeTableData(table2);

                int c = 0;
                using (var asyncResult = new AsyncResult(
                    (ctx, cells) => {
                        foreach (var cell in cells) {
                            Assert.IsFalse(String.IsNullOrEmpty(cell.Key.Row));
                            Interlocked.Increment(ref c);
                        }
                        return AsyncCallbackResult.Continue;
                    })) {
                    table.BeginScan(asyncResult, new ScanSpec().AddColumn("a"));
                    table2.BeginScan(asyncResult, new ScanSpec().AddColumn("b"));
                    asyncResult.Join();
                    Assert.IsTrue(asyncResult.IsCompleted);
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                }
                Assert.AreEqual(CountA + CountB, c);

                c = 0;
                using (var asyncResult = new AsyncResult(
                    (ctx, cells) => {
                        foreach (var cell in cells) {
                            Assert.IsFalse(String.IsNullOrEmpty(cell.Key.Row));
                            Interlocked.Increment(ref c);
                        }
                        return AsyncCallbackResult.Continue;
                    })) {
                    table2.BeginScan(asyncResult, new ScanSpec().AddColumn("b"));
                    table.BeginScan(asyncResult, new ScanSpec().AddColumn("c"));
                    asyncResult.Join();
                    Assert.IsTrue(asyncResult.IsCompleted);
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                }
                Assert.AreEqual(CountB + CountC, c);
            }
        }

        [TestMethod]
        public void ScanMultipleTableColumnFamilyBlockingAsync() {
            using (var table2 = EnsureTable("ScanMultipleTableColumnFamilyBlockingAsync", Schema)) {
                InitializeTableData(table2);

                int c = 0;
                using (var asyncResult = new BlockingAsyncResult()) {
                    table.BeginScan(asyncResult, new ScanSpec().AddColumn("a"));
                    table2.BeginScan(asyncResult, new ScanSpec().AddColumn("b"));
                    IList<Cell> cells;
                    while (asyncResult.TryGetCells(out cells)) {
                        foreach (var cell in cells) {
                            Assert.IsFalse(String.IsNullOrEmpty(cell.Key.Row));
                            ++c;
                        }
                    }
                    Assert.IsTrue(asyncResult.IsCompleted);
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                }
                Assert.AreEqual(CountA + CountB, c);

                c = 0;
                using (var asyncResult = new BlockingAsyncResult()) {
                    table2.BeginScan(asyncResult, new ScanSpec().AddColumn("b"));
                    table.BeginScan(asyncResult, new ScanSpec().AddColumn("c"));
                    IList<Cell> cells;
                    while (asyncResult.TryGetCells(out cells)) {
                        foreach (var cell in cells) {
                            Assert.IsFalse(String.IsNullOrEmpty(cell.Key.Row));
                            ++c;
                        }
                    }
                    Assert.IsTrue(asyncResult.IsCompleted);
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                }
                Assert.AreEqual(CountB + CountC, c);
            }
        }

        [TestMethod]
        public void ScanTableAsync() {
            var param = new Object();
            int c = 0;
            int d = 0;
            AsyncScannerCallback cb = (ctx, cells) => {
                Assert.AreSame(table, ctx.Table);
                Assert.IsNull(ctx.ScanSpec);
                Assert.AreSame(param, ctx.Param);

                foreach (var cell in cells) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }
                return AsyncCallbackResult.Continue;
            };
            using (var asyncResult = new AsyncResult(cb)) {
                table.BeginScan(asyncResult, null, param);
                Assert.IsFalse(asyncResult.IsCompleted);

                long id = 0;
                id = table.BeginScan(
                    asyncResult,
                    (ctx, cells) => {
                        Assert.IsNull(ctx.Param);
                        Assert.AreEqual(id, ctx.Id);
                        d += cells.Count;
                        return AsyncCallbackResult.Continue;
                    });

                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                Assert.AreEqual(CountA + CountB + CountC, c);
                Assert.AreEqual(CountA + CountB + CountC, d);
                Thread.Sleep(1000);

                c = 0;
                table.BeginScan(asyncResult, null, param);
                Assert.IsFalse(asyncResult.IsCompleted);
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                Assert.AreEqual(CountA + CountB + CountC, c);
            }
        }

        [TestMethod]
        public void ScanTableBlockingAsync() {
            int c = 0;
            using (var asyncResult = new BlockingAsyncResult(4 * 1024)) {
                AsyncScannerContext asyncScannerContext;
                IList<Cell> cells;

                table.BeginScan(asyncResult);
                while (asyncResult.TryGetCells(out asyncScannerContext, out cells)) {
                    Assert.AreSame(table, asyncScannerContext.Table);
                    foreach (var cell in cells) {
                        Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                        ++c;
                    }
                }
                Assert.AreEqual(CountA + CountB + CountC, c);

                c = 0;
                table.BeginScan(asyncResult);
                while (asyncResult.TryGetCells(out asyncScannerContext, out cells)) {
                    Assert.AreSame(table, asyncScannerContext.Table);
                    foreach (var cell in cells) {
                        Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                        ++c;
                    }
                }
                Assert.AreEqual(CountA + CountB + CountC, c);
            }
        }

        [TestMethod]
        public void ScanTableCancelAsync() {
            for (int r = 0; r < 5; ++r) {
                int c = 0;
                using (var asyncResult = new AsyncResult(
                    (ctx, cells) => {
                        foreach (var cell in cells) {
                            if (c == CountA) {
                                return AsyncCallbackResult.Abort;
                            }
                            ++c;
                        }
                        return AsyncCallbackResult.Continue;
                    })) {
                    table.BeginScan(asyncResult);
                    asyncResult.Join();
                    Assert.IsTrue(asyncResult.IsCompleted);
                    Assert.IsTrue(asyncResult.IsCancelled);
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                    Assert.AreEqual(CountA, c);

                    c = 0;
                    table.BeginScan(
                        asyncResult,
                        (ctx, cells) => {
                            c += cells.Count;
                            return AsyncCallbackResult.Continue;
                        });
                    Assert.IsFalse(asyncResult.IsCompleted);
                    Assert.IsFalse(asyncResult.IsCancelled);
                    asyncResult.Join();
                    Assert.IsTrue(asyncResult.IsCompleted);
                    Assert.IsFalse(asyncResult.IsCancelled);
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                    Assert.AreEqual(CountA + CountB + CountC, c);
                }

                using (var asyncResult = new AsyncResult(delegate { return AsyncCallbackResult.Continue; })) {
                    table.BeginScan(asyncResult);
                    Thread.Sleep(500);
                    asyncResult.Cancel();
                    Assert.IsTrue(asyncResult.IsCancelled);

                    c = 0;
                    table.BeginScan(
                        asyncResult,
                        (ctx, cells) => {
                            c += cells.Count;
                            return AsyncCallbackResult.Continue;
                        });
                    Assert.IsFalse(asyncResult.IsCancelled);
                    asyncResult.Join();
                    Assert.IsTrue(asyncResult.IsCompleted);
                    Assert.IsFalse(asyncResult.IsCancelled);
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                    Assert.AreEqual(CountA + CountB + CountC, c);
                }
            }
        }

        [TestMethod]
        public void ScanTableCancelAsyncScanner() {
            var rng = new Random();
            for (int r = 0; r < 5; ++r) {
                int ca = 0;
                int caLimit = rng.Next(CountA / 2);
                int caTotal = 0;
                int cb = 0;
                int cbLimit = rng.Next(CountB / 2);
                int cbTotal = 0;
                int cc = 0;

                using (var asyncResult = new AsyncResult()) {
                    table.BeginScan(
                        asyncResult,
                        new ScanSpec().AddColumn("a"),
                        (ctx, cells) => {
                            ca += cells.Count;
                            if (caTotal == 0 && ca > caLimit) {
                                caTotal = ca;
                                return AsyncCallbackResult.Cancel;
                            }
                            return AsyncCallbackResult.Continue;
                        });

                    table.BeginScan(
                        asyncResult,
                        new ScanSpec().AddColumn("a").AddColumn("b").AddColumn("c"),
                        (ctx, cells) => {
                            cb += cells.Count;
                            if (cbTotal == 0 && cb > cbLimit) {
                                cbTotal = cb;
                                return AsyncCallbackResult.Cancel;
                            }
                            return AsyncCallbackResult.Continue;
                        });

                    table.BeginScan(
                        asyncResult,
                        new ScanSpec().AddColumn("b").AddColumn("c"),
                        (ctx, cells) => {
                            cc += cells.Count;
                            return AsyncCallbackResult.Continue;
                        });

                    asyncResult.Join();
                    Assert.IsTrue(asyncResult.IsCompleted);
                    Assert.IsFalse(asyncResult.IsCancelled);
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                    Assert.AreEqual(caTotal, ca);
                    Assert.AreEqual(cbTotal, cb);
                    Assert.AreEqual(CountB + CountC, cc);

                    ca = 0;
                    caLimit = rng.Next(CountA / 2);
                    caTotal = 0;
                    cb = 0;
                    cbLimit = rng.Next(CountB / 2);
                    cbTotal = 0;
                    cc = 0;

                    table.BeginScan(
                        asyncResult,
                        new ScanSpec().AddColumn("a"),
                        (ctx, cells) => {
                            ca += cells.Count;
                            if (caTotal == 0 && ca > caLimit) {
                                caTotal = ca;
                                asyncResult.CancelAsyncScanner(ctx);
                            }
                            return AsyncCallbackResult.Continue;
                        });

                    table.BeginScan(
                        asyncResult,
                        new ScanSpec().AddColumn("a").AddColumn("b").AddColumn("c"),
                        (ctx, cells) => {
                            cb += cells.Count;
                            if (cbTotal == 0 && cb > cbLimit) {
                                cbTotal = cb;
                                asyncResult.CancelAsyncScanner(ctx);
                            }
                            return AsyncCallbackResult.Continue;
                        });

                    table.BeginScan(
                        asyncResult,
                        new ScanSpec().AddColumn("b").AddColumn("c"),
                        (ctx, cells) => {
                            cc += cells.Count;
                            return AsyncCallbackResult.Continue;
                        });

                    asyncResult.Join();
                    Assert.IsTrue(asyncResult.IsCompleted);
                    Assert.IsFalse(asyncResult.IsCancelled);
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                    Assert.AreEqual(caTotal, ca);
                    Assert.AreEqual(cbTotal, cb);
                    Assert.AreEqual(CountB + CountC, cc);
                }
            }
        }

        [TestMethod]
        public void ScanTableCancelBlockingAsync() {
            for (int r = 0; r < 5; ++r) {
                int c = 0;
                using (var asyncResult = new BlockingAsyncResult()) {
                    table.BeginScan(asyncResult);
                    IList<Cell> cells;
                    while (asyncResult.TryGetCells(out cells)) {
                        foreach (var cell in cells) {
                            if (c == CountC) {
                                asyncResult.Cancel();
                                Assert.IsTrue(asyncResult.IsCancelled);
                                break;
                            }
                            c++;
                        }
                    }
                    Assert.IsTrue(asyncResult.IsCancelled);
                    asyncResult.Join();
                    Assert.IsTrue(asyncResult.IsCompleted);
                    Assert.IsTrue(asyncResult.IsCancelled);
                    Assert.AreEqual(CountC, c);

                    c = 0;
                    table.BeginScan(asyncResult);
                    Assert.IsFalse(asyncResult.IsCancelled);
                    while (asyncResult.TryGetCells(out cells)) {
                        c += cells.Count;
                    }
                    asyncResult.Join();
                    Assert.IsTrue(asyncResult.IsCompleted);
                    Assert.IsFalse(asyncResult.IsCancelled);
                    Assert.AreEqual(CountA + CountB + CountC, c);
                }
            }
        }

        [TestMethod]
        public void ScanTableCancelBlockingAsyncScanner() {
            var rng = new Random();
            var scanSpecA = new ScanSpec().AddColumn("a");
            var scanSpecB = new ScanSpec().AddColumn("a").AddColumn("b").AddColumn("c");
            var scanSpecC = new ScanSpec().AddColumn("b").AddColumn("c");

            for (int r = 0; r < 5; ++r) {
                var c = new Dictionary<ScanSpec, int>();
                var limit = new Dictionary<ScanSpec, int>();
                var total = new Dictionary<ScanSpec, int>();

                c[scanSpecA] = 0;
                c[scanSpecB] = 0;
                c[scanSpecC] = 0;

                limit[scanSpecA] = rng.Next(CountA / 2);
                limit[scanSpecB] = rng.Next(CountB / 2);
                limit[scanSpecC] = int.MaxValue;

                using (var asyncResult = new BlockingAsyncResult()) {
                    table.BeginScan(asyncResult, scanSpecA);
                    table.BeginScan(asyncResult, scanSpecB);
                    table.BeginScan(asyncResult, scanSpecC);

                    AsyncScannerContext ctx;
                    IList<Cell> cells;
                    while (asyncResult.TryGetCells(out ctx, out cells)) {
                        c[ctx.ScanSpec] += cells.Count;
                        if (!total.ContainsKey(ctx.ScanSpec) && c[ctx.ScanSpec] > limit[ctx.ScanSpec]) {
                            total.Add(ctx.ScanSpec, c[ctx.ScanSpec]);
                            asyncResult.CancelAsyncScanner(ctx);
                        }
                    }
                    asyncResult.Join();
                    Assert.IsTrue(asyncResult.IsCompleted);
                    Assert.IsFalse(asyncResult.IsCancelled);
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                    Assert.AreEqual(total[scanSpecA], c[scanSpecA]);
                    Assert.AreEqual(total[scanSpecB], c[scanSpecB]);
                    Assert.AreEqual(CountB + CountC, c[scanSpecC]);

                    total = new Dictionary<ScanSpec, int>();

                    c[scanSpecA] = 0;
                    c[scanSpecB] = 0;
                    c[scanSpecC] = 0;

                    limit[scanSpecA] = rng.Next(CountA / 2);
                    limit[scanSpecB] = rng.Next(CountB / 2);
                    limit[scanSpecC] = int.MaxValue;

                    table.BeginScan(asyncResult, scanSpecC);
                    table.BeginScan(asyncResult, scanSpecB);
                    table.BeginScan(asyncResult, scanSpecA);

                    while (asyncResult.TryGetCells(out ctx, out cells)) {
                        c[ctx.ScanSpec] += cells.Count;
                        if (!total.ContainsKey(ctx.ScanSpec) && c[ctx.ScanSpec] > limit[ctx.ScanSpec]) {
                            total.Add(ctx.ScanSpec, c[ctx.ScanSpec]);
                            asyncResult.CancelAsyncScanner(ctx);
                        }
                    }
                    asyncResult.Join();
                    Assert.IsTrue(asyncResult.IsCompleted);
                    Assert.IsFalse(asyncResult.IsCancelled);
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                    Assert.AreEqual(total[scanSpecA], c[scanSpecA]);
                    Assert.AreEqual(total[scanSpecB], c[scanSpecB]);
                    Assert.AreEqual(CountB + CountC, c[scanSpecC]);
                }
            }
        }

        [TestMethod]
        public void ScanTableDifferentContextAsync() {
            using (var ctx = Context.Create(CtxKind == ContextKind.Hyper ? ContextKind.Thrift : ContextKind.Hyper, Host))
            using (var client = ctx.CreateClient()) {
                string nsNameOther = NsName + "/other";
                try {
                    using (var nsOther = client.OpenNamespace(nsNameOther, OpenDispositions.OpenAlways))
                    using (var tableOther = nsOther.OpenTable("ScanTableDifferentContextAsync", Schema, OpenDispositions.CreateAlways)) {
                        InitializeTableData(tableOther);

                        int c = 0;
                        using (var asyncResult = new AsyncResult(
                            (_ctx, cells) => {
                                foreach (var cell in cells) {
                                    Assert.IsFalse(String.IsNullOrEmpty(cell.Key.Row));
                                    Interlocked.Increment(ref c);
                                }
                                return AsyncCallbackResult.Continue;
                            })) {
                            table.BeginScan(asyncResult, new ScanSpec().AddColumn("a"));
                            tableOther.BeginScan(asyncResult, new ScanSpec().AddColumn("b"));
                            asyncResult.Join();
                            Assert.IsTrue(asyncResult.IsCompleted);
                            Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                        }
                        Assert.AreEqual(CountA + CountB, c);

                        c = 0;
                        using (var asyncResult = new AsyncResult(
                            (_ctx, cells) => {
                                foreach (var cell in cells) {
                                    Assert.IsFalse(String.IsNullOrEmpty(cell.Key.Row));
                                    Interlocked.Increment(ref c);
                                }
                                return AsyncCallbackResult.Continue;
                            })) {
                            table.BeginScan(asyncResult, new ScanSpec().AddColumn("b"));
                            tableOther.BeginScan(asyncResult, new ScanSpec().AddColumn("a"));
                            asyncResult.Join();
                            Assert.IsTrue(asyncResult.IsCompleted);
                            Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                        }
                        Assert.AreEqual(CountB + CountA, c);
                    }
                }
                finally {
                    client.DropNamespace(nsNameOther, DropDispositions.Complete);
                }
            }
        }

        [TestMethod]
        public void ScanTableDifferentContextBlockingAsync() {
            using (var ctx = Context.Create(CtxKind == ContextKind.Hyper ? ContextKind.Thrift : ContextKind.Hyper, Host))
            using (var client = ctx.CreateClient()) {
                string nsNameOther = NsName + "/other";
                try {
                    using (var nsOther = client.OpenNamespace(nsNameOther, OpenDispositions.OpenAlways))
                    using (var tableOther = nsOther.OpenTable("ScanTableDifferentContextBlockingAsync", Schema, OpenDispositions.CreateAlways)) {
                        InitializeTableData(tableOther);

                        int c = 0;
                        using (var asyncResult = new BlockingAsyncResult()) {
                            table.BeginScan(asyncResult, new ScanSpec().AddColumn("a"));
                            tableOther.BeginScan(asyncResult, new ScanSpec().AddColumn("b"));
                            IList<Cell> cells;
                            while (asyncResult.TryGetCells(out cells)) {
                                foreach (var cell in cells) {
                                    Assert.IsFalse(String.IsNullOrEmpty(cell.Key.Row));
                                    ++c;
                                }
                            }
                            Assert.IsTrue(asyncResult.IsCompleted);
                            Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                        }
                        Assert.AreEqual(CountA + CountB, c);

                        c = 0;
                        using (var asyncResult = new BlockingAsyncResult()) {
                            table.BeginScan(asyncResult, new ScanSpec().AddColumn("b"));
                            tableOther.BeginScan(asyncResult, new ScanSpec().AddColumn("a"));
                            IList<Cell> cells;
                            while (asyncResult.TryGetCells(out cells)) {
                                foreach (var cell in cells) {
                                    Assert.IsFalse(String.IsNullOrEmpty(cell.Key.Row));
                                    ++c;
                                }
                            }
                            Assert.IsTrue(asyncResult.IsCompleted);
                            Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
                        }
                        Assert.AreEqual(CountB + CountA, c);
                    }
                }
                finally {
                    client.DropNamespace(nsNameOther, DropDispositions.Complete);
                }
            }
        }

        [TestMethod]
        public void ScanTableKeyOnlyAsync() {
            int c = 0;
            using (var asyncResult = new AsyncResult(
                (ctx, cells) => {
                    foreach (var cell in cells) {
                        Assert.IsFalse(String.IsNullOrEmpty(cell.Key.Row));
                        Assert.IsNull(cell.Value);
                        ++c;
                    }
                    return AsyncCallbackResult.Continue;
                })) {
                table.BeginScan(asyncResult, new ScanSpec { KeysOnly = true });
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(CountA + CountB + CountC, c);
        }

        [TestMethod]
        public void ScanTableMaxRowsAsync() {
            int c = 0;
            using (var asyncResult = new AsyncResult(
                (ctx, cells) => {
                    foreach (var cell in cells) {
                        Assert.IsFalse(String.IsNullOrEmpty(cell.Key.Row));
                        ++c;
                    }
                    return AsyncCallbackResult.Continue;
                })) {
                table.BeginScan(asyncResult, new ScanSpec { MaxRows = CountC }.AddColumn("a"));
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(CountC, c);

            c = 0;
            using (var asyncResult = new AsyncResult(
                (ctx, cells) => {
                    foreach (var cell in cells) {
                        Assert.IsFalse(String.IsNullOrEmpty(cell.Key.Row));
                        ++c;
                    }
                    return AsyncCallbackResult.Continue;
                })) {
                table.BeginScan(asyncResult, new ScanSpec { MaxRows = CountB }.AddColumn("a"));
                asyncResult.Join();
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : "");
            }
            Assert.AreEqual(CountB, c);
        }

        [TestInitialize]
        public void TestInitialize() {
            DeleteColumnFamily(table, "d");
            DeleteColumnFamily(table, "e");
            DeleteColumnFamily(table, "f");
            DeleteColumnFamily(table, "g");
        }

        #endregion

        #region Methods

        private static void InitializeTableData(Table _table) {
            var key = new Key();
            using (var mutator = _table.CreateMutator(MutatorSpec.CreateChunked())) {
                for (int n = 0; n < CountA; ++n) {
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
