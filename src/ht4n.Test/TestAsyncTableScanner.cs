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
            "<Schema><AccessGroup name=\"default\" blksz=\"1024\">" +
            "<ColumnFamily><Name>a</Name></ColumnFamily>" +
            "<ColumnFamily><Name>b</Name></ColumnFamily>" +
            "<ColumnFamily><Name>c</Name></ColumnFamily>" +
            "<ColumnFamily><Name>d</Name></ColumnFamily>" +
            "<ColumnFamily><Name>e</Name></ColumnFamily>" +
            "<ColumnFamily><Name>f</Name></ColumnFamily>" +
            "<ColumnFamily><Name>g</Name></ColumnFamily>" +
            "</AccessGroup></Schema>";

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
            table = EnsureTable(typeof(TestAsyncTableScanner), Schema);
            InitializeTableData(table);

            if (!HasPeriodicFlushTableMutator) {
                Assert.IsFalse(IsHyper);
                Assert.IsFalse(IsThrift);
            }
        }

        [TestMethod]
        public void ScanMultipleTableAsync() {
            if (!HasAsyncTableScanner) {
                return;
            }

            const int CountTables = 10;
            var tables = new List<ITable>();
            try {
                for (var t = 0; t < CountTables; ++t) {
                    var testTable = EnsureTable(string.Format("ScanMultipleTableAsync-{0}", t), Schema);
                    InitializeTableData(testTable);
                    tables.Add(testTable);
                }

                var c = 0;
                using (var asyncResult = new AsyncResult(
                    (ctx, cells) =>
                        {
                            foreach (var cell in cells) {
                                Assert.IsFalse(string.IsNullOrEmpty(cell.Key.Row));
                                Interlocked.Increment(ref c);
                            }

                            return AsyncCallbackResult.Continue;
                        })) {
                    tables.ForEach(t => t.BeginScan(asyncResult, new ScanSpec().AddColumn("a")));
                    tables.ForEach(t => t.BeginScan(asyncResult, new ScanSpec().AddColumn("b")));
                    asyncResult.Join();
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : string.Empty);
                    Assert.IsTrue(asyncResult.IsCompleted);
                }

                Assert.AreEqual(CountTables * (CountA + CountB), c);

                c = 0;
                using (var asyncResult = new AsyncResult()) {
                    tables.ForEach(
                        t => t.BeginScan(
                            asyncResult,
                            new ScanSpec().AddColumn("b"),
                            (ctx, cells) =>
                                {
                                    foreach (var cell in cells) {
                                        Assert.IsFalse(string.IsNullOrEmpty(cell.Key.Row));
                                        Interlocked.Increment(ref c);
                                    }

                                    return AsyncCallbackResult.Continue;
                                }));
                    tables.ForEach(
                        t => t.BeginScan(
                            asyncResult,
                            new ScanSpec().AddColumn("c"),
                            (ctx, cells) =>
                                {
                                    foreach (var cell in cells) {
                                        Assert.IsFalse(string.IsNullOrEmpty(cell.Key.Row));
                                        Interlocked.Increment(ref c);
                                    }

                                    return AsyncCallbackResult.Continue;
                                }));
                    asyncResult.Join();
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : string.Empty);
                    Assert.IsTrue(asyncResult.IsCompleted);
                }

                Assert.AreEqual(CountTables * (CountB + CountC), c);
            }
            finally {
                tables.ForEach(t => t.Dispose());
                for (var t = 0; t < CountTables; ++t) {
                    Ns.DropTable(string.Format("ScanMultipleTableAsync-{0}", t), DropDispositions.IfExists);
                }
            }
        }

        [TestMethod]
        public void ScanMultipleTableBlockingAsync() {
            if (!HasAsyncTableScanner) {
                return;
            }

            const int CountTables = 10;
            var tables = new List<ITable>();
            try {
                for (var t = 0; t < CountTables; ++t) {
                    var testTable = EnsureTable(string.Format("ScanMultipleTableBlockingAsync-{0}", t), Schema);
                    InitializeTableData(testTable);
                    tables.Add(testTable);
                }

                var c = 0;
                using (var asyncResult = new BlockingAsyncResult()) {
                    tables.ForEach(t => t.BeginScan(asyncResult, new ScanSpec().AddColumn("a")));
                    tables.ForEach(t => t.BeginScan(asyncResult, new ScanSpec().AddColumn("b")));
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

                Assert.AreEqual(CountTables * (CountA + CountB), c);

                c = 0;
                using (var asyncResult = new BlockingAsyncResult()) {
                    tables.ForEach(t => t.BeginScan(asyncResult, new ScanSpec().AddColumn("b")));
                    tables.ForEach(t => t.BeginScan(asyncResult, new ScanSpec().AddColumn("c")));
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

                Assert.AreEqual(CountTables * (CountB + CountC), c);
            }
            finally {
                tables.ForEach(t => t.Dispose());
                for (var t = 0; t < CountTables; ++t) {
                    Ns.DropTable(string.Format("ScanMultipleTableBlockingAsync-{0}", t), DropDispositions.IfExists);
                }
            }
        }

        [TestMethod]
        public void ScanMultipleTableColumnFamilyAsync() {
            if (!HasAsyncTableScanner) {
                return;
            }

            using (var table2 = EnsureTable("ScanMultipleTableColumnFamilyAsync", Schema)) {
                InitializeTableData(table2);

                var c = 0;
                using (var asyncResult = new AsyncResult(
                    (ctx, cells) =>
                        {
                            foreach (var cell in cells) {
                                Assert.IsFalse(string.IsNullOrEmpty(cell.Key.Row));
                                Interlocked.Increment(ref c);
                            }

                            return AsyncCallbackResult.Continue;
                        })) {
                    table.BeginScan(asyncResult, new ScanSpec().AddColumn("a"));
                    table2.BeginScan(asyncResult, new ScanSpec().AddColumn("b"));
                    asyncResult.Join();
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : string.Empty);
                    Assert.IsTrue(asyncResult.IsCompleted);
                }

                Assert.AreEqual(CountA + CountB, c);

                c = 0;
                using (var asyncResult = new AsyncResult(
                    (ctx, cells) =>
                        {
                            foreach (var cell in cells) {
                                Assert.IsFalse(string.IsNullOrEmpty(cell.Key.Row));
                                Interlocked.Increment(ref c);
                            }

                            return AsyncCallbackResult.Continue;
                        })) {
                    table2.BeginScan(asyncResult, new ScanSpec().AddColumn("b"));
                    table.BeginScan(asyncResult, new ScanSpec().AddColumn("c"));
                    asyncResult.Join();
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : string.Empty);
                    Assert.IsTrue(asyncResult.IsCompleted);
                }

                Assert.AreEqual(CountB + CountC, c);
            }
        }

        [TestMethod]
        public void ScanMultipleTableColumnFamilyBlockingAsync() {
            if (!HasAsyncTableScanner) {
                return;
            }

            using (var table2 = EnsureTable("ScanMultipleTableColumnFamilyBlockingAsync", Schema)) {
                InitializeTableData(table2);

                var c = 0;
                using (var asyncResult = new BlockingAsyncResult()) {
                    table.BeginScan(asyncResult, new ScanSpec().AddColumn("a"));
                    table2.BeginScan(asyncResult, new ScanSpec().AddColumn("b"));
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
                    table2.BeginScan(asyncResult, new ScanSpec().AddColumn("b"));
                    table.BeginScan(asyncResult, new ScanSpec().AddColumn("c"));
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
        }

        [TestMethod]
        public void ScanTableAsync() {
            if (!HasAsyncTableScanner) {
                return;
            }

            var param = new object();
            var c = 0;
            var d = 0;
            AsyncScannerCallback cb = (ctx, cells) =>
                {
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
                    (ctx, cells) =>
                        {
                            Assert.IsNull(ctx.Param);
                            Assert.AreEqual(id, ctx.Id);
                            d += cells.Count;
                            return AsyncCallbackResult.Continue;
                        });

                asyncResult.Join();
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : string.Empty);
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.AreEqual(CountA + CountB + CountC, c);
                Assert.AreEqual(CountA + CountB + CountC, d);
                Thread.Sleep(1000);

                c = 0;
                table.BeginScan(asyncResult, null, param);
                Assert.IsFalse(asyncResult.IsCompleted);
                asyncResult.Join();
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : string.Empty);
                Assert.IsTrue(asyncResult.IsCompleted);
                Assert.AreEqual(CountA + CountB + CountC, c);
            }
        }

        [TestMethod]
        public void ScanTableBlockingAsync() {
            if (!HasAsyncTableScanner) {
                return;
            }

            var c = 0;
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
            if (!HasAsyncTableScanner) {
                return;
            }

            for (var r = 0; r < 5; ++r) {
                var c = 0;
                using (var asyncResult = new AsyncResult(
                    (ctx, cells) =>
                        {
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
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : string.Empty);
                    Assert.IsTrue(asyncResult.IsCompleted);
                    Assert.IsTrue(asyncResult.IsCancelled);
                    Assert.AreEqual(CountA, c);

                    // The official Hypertable version does not support re-using of the future object using the thrift API
                    if (!IsThrift) {
                        c = 0;
                        table.BeginScan(
                            asyncResult, 
                            (ctx, cells) =>
                                {
                                    c += cells.Count;
                                    return AsyncCallbackResult.Continue;
                                });
                        Assert.IsFalse(asyncResult.IsCompleted);
                        Assert.IsFalse(asyncResult.IsCancelled);
                        asyncResult.Join();
                        Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : string.Empty);
                        Assert.IsTrue(asyncResult.IsCompleted);
                        Assert.IsFalse(asyncResult.IsCancelled);
                        Assert.AreEqual(CountA + CountB + CountC, c);
                    }
                }

                using (var asyncResult = new AsyncResult(delegate { return AsyncCallbackResult.Continue; })) {
                    table.BeginScan(asyncResult);
                    Thread.Sleep(500);
                    asyncResult.Cancel();
                    Assert.IsTrue(asyncResult.IsCancelled);

                    // The official Hypertable version does not support re-using of the future object using the thrift API
                    if (!IsThrift) {
                        c = 0;
                        table.BeginScan(
                            asyncResult, 
                            (ctx, cells) =>
                                {
                                    c += cells.Count;
                                    return AsyncCallbackResult.Continue;
                                });
                        Assert.IsFalse(asyncResult.IsCancelled);
                        asyncResult.Join();
                        Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : string.Empty);
                        Assert.IsTrue(asyncResult.IsCompleted);
                        Assert.IsFalse(asyncResult.IsCancelled);
                        Assert.AreEqual(CountA + CountB + CountC, c);
                    }
                }
            }
        }

        [TestMethod]
        public void ScanTableCancelAsyncScanner() {
            if (!HasAsyncTableScanner) {
                return;
            }

            var rng = new Random();
            for (var r = 0; r < 5; ++r) {
                var ca = 0;
                var caLimit = rng.Next(CountA / 2);
                var caTotal = 0;
                var cb = 0;
                var cbLimit = rng.Next(CountB / 2);
                var cbTotal = 0;
                var cc = 0;

                using (var asyncResult = new AsyncResult()) {
                    table.BeginScan(
                        asyncResult, 
                        new ScanSpec().AddColumn("a"), 
                        (ctx, cells) =>
                            {
                                ca += cells.Count;
                                if (caTotal == 0 && ca > caLimit) {
                                    caTotal = ca;
                                    return AsyncCallbackResult.Cancel;
                                }

                                return AsyncCallbackResult.Continue;
                            });

                    table.BeginScan(
                        asyncResult, 
                        new ScanSpec().AddColumn("a", "b", "c"), 
                        (ctx, cells) =>
                            {
                                cb += cells.Count;
                                if (cbTotal == 0 && cb > cbLimit) {
                                    cbTotal = cb;
                                    return AsyncCallbackResult.Cancel;
                                }

                                return AsyncCallbackResult.Continue;
                            });

                    table.BeginScan(
                        asyncResult, 
                        new ScanSpec().AddColumn("b", "c"), 
                        (ctx, cells) =>
                            {
                                cc += cells.Count;
                                return AsyncCallbackResult.Continue;
                            });

                    asyncResult.Join();
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : string.Empty);
                    Assert.IsTrue(asyncResult.IsCompleted);
                    Assert.IsFalse(asyncResult.IsCancelled);
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
                        (ctx, cells) =>
                            {
                                ca += cells.Count;
                                if (caTotal == 0 && ca > caLimit) {
                                    caTotal = ca;
                                    asyncResult.CancelAsyncScanner(ctx);
                                }

                                return AsyncCallbackResult.Continue;
                            });

                    table.BeginScan(
                        asyncResult, 
                        new ScanSpec().AddColumn("a", "b", "c"), 
                        (ctx, cells) =>
                            {
                                cb += cells.Count;
                                if (cbTotal == 0 && cb > cbLimit) {
                                    cbTotal = cb;
                                    asyncResult.CancelAsyncScanner(ctx);
                                }

                                return AsyncCallbackResult.Continue;
                            });

                    table.BeginScan(
                        asyncResult, 
                        new ScanSpec().AddColumn("b", "c"), 
                        (ctx, cells) =>
                            {
                                cc += cells.Count;
                                return AsyncCallbackResult.Continue;
                            });

                    asyncResult.Join();
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : string.Empty);
                    Assert.IsTrue(asyncResult.IsCompleted);
                    Assert.IsFalse(asyncResult.IsCancelled);
                    Assert.AreEqual(caTotal, ca);
                    Assert.AreEqual(cbTotal, cb);
                    Assert.AreEqual(CountB + CountC, cc);
                }
            }
        }

        [TestMethod]
        public void ScanTableCancelBlockingAsync() {
            if (!HasAsyncTableScanner) {
                return;
            }

            for (var r = 0; r < 5; ++r) {
                var c = 0;
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

                    // The official Hypertable version does not support re-using of the future object using the thrift API
                    if (!IsThrift) {
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
        }

        [TestMethod]
        public void ScanTableCancelBlockingAsyncScanner() {
            if (!HasAsyncTableScanner) {
                return;
            }

            var rng = new Random();
            var scanSpecA = new ScanSpec().AddColumn("a");
            var scanSpecB = new ScanSpec().AddColumn("a", "b", "c");
            var scanSpecC = new ScanSpec().AddColumn("b", "c");

            for (var r = 0; r < 5; ++r) {
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
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : string.Empty);
                    Assert.IsTrue(asyncResult.IsCompleted);
                    Assert.IsFalse(asyncResult.IsCancelled);
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
                    Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : string.Empty);
                    Assert.IsTrue(asyncResult.IsCompleted);
                    Assert.IsFalse(asyncResult.IsCancelled);
                    Assert.AreEqual(total[scanSpecA], c[scanSpecA]);
                    Assert.AreEqual(total[scanSpecB], c[scanSpecB]);
                    Assert.AreEqual(CountB + CountC, c[scanSpecC]);
                }
            }
        }

        [TestMethod]
        public void ScanTableDifferentContextAsync() {
            if (!HasAsyncTableScanner) {
                return;
            }

            if (!IsHyper && !IsThrift) {
                Assert.Fail("Check implementation below for the new provider {0}", ProviderName);
            }

            var properties = new Dictionary<string, object> { { "Provider", IsHyper ? "Thrift" : "Hyper" } };
            var nsOtherName = NsName + "/other";
            using (var ctx = Hypertable.Context.Create(ConnectionString, properties))
            using (var client = ctx.CreateClient()) {
                try {
                    using (var nsOther = client.OpenNamespace(nsOtherName, OpenDispositions.OpenAlways))
                    using (var tableOther = nsOther.OpenTable("ScanTableDifferentContextAsync", Schema, OpenDispositions.CreateAlways)) {
                        InitializeTableData(tableOther);

                        var c = 0;
                        using (var asyncResult = new AsyncResult(
                            (_ctx, cells) =>
                                {
                                    foreach (var cell in cells) {
                                        Assert.IsFalse(string.IsNullOrEmpty(cell.Key.Row));
                                        Interlocked.Increment(ref c);
                                    }

                                    return AsyncCallbackResult.Continue;
                                })) {
                            table.BeginScan(asyncResult, new ScanSpec().AddColumn("a"));
                            tableOther.BeginScan(asyncResult, new ScanSpec().AddColumn("b"));
                            asyncResult.Join();
                            Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : string.Empty);
                            Assert.IsTrue(asyncResult.IsCompleted);
                        }

                        Assert.AreEqual(CountA + CountB, c);

                        c = 0;
                        using (var asyncResult = new AsyncResult(
                            (_ctx, cells) =>
                                {
                                    foreach (var cell in cells) {
                                        Assert.IsFalse(string.IsNullOrEmpty(cell.Key.Row));
                                        Interlocked.Increment(ref c);
                                    }

                                    return AsyncCallbackResult.Continue;
                                })) {
                            table.BeginScan(asyncResult, new ScanSpec().AddColumn("b"));
                            tableOther.BeginScan(asyncResult, new ScanSpec().AddColumn("a"));
                            asyncResult.Join();
                            Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : string.Empty);
                            Assert.IsTrue(asyncResult.IsCompleted);
                        }

                        Assert.AreEqual(CountB + CountA, c);
                    }
                }
                finally {
                    client.DropNamespace(nsOtherName, DropDispositions.Complete);
                }
            }
        }

        [TestMethod]
        public void ScanTableDifferentContextBlockingAsync() {
            if (!HasAsyncTableScanner) {
                return;
            }

            if (!IsHyper && !IsThrift) {
                Assert.Fail("Check implementation below for the new provider {0}", ProviderName);
            }

            var properties = new Dictionary<string, object> { { "Provider", IsHyper ? "Thrift" : "Hyper" } };
            using (var ctx = Hypertable.Context.Create(ConnectionString, properties))
            using (var client = ctx.CreateClient()) {
                var nsNameOther = NsName + "/other";
                try {
                    using (var nsOther = client.OpenNamespace(nsNameOther, OpenDispositions.OpenAlways))
                    using (var tableOther = nsOther.OpenTable("ScanTableDifferentContextBlockingAsync", Schema, OpenDispositions.CreateAlways)) {
                        InitializeTableData(tableOther);

                        var c = 0;
                        using (var asyncResult = new BlockingAsyncResult()) {
                            table.BeginScan(asyncResult, new ScanSpec().AddColumn("a"));
                            tableOther.BeginScan(asyncResult, new ScanSpec().AddColumn("b"));
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
                            table.BeginScan(asyncResult, new ScanSpec().AddColumn("b"));
                            tableOther.BeginScan(asyncResult, new ScanSpec().AddColumn("a"));
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
            if (!HasAsyncTableScanner) {
                return;
            }

            var c = 0;
            using (var asyncResult = new AsyncResult(
                (ctx, cells) =>
                    {
                        foreach (var cell in cells) {
                            Assert.IsFalse(string.IsNullOrEmpty(cell.Key.Row));
                            Assert.IsNull(cell.Value);
                            ++c;
                        }

                        return AsyncCallbackResult.Continue;
                    })) {
                table.BeginScan(asyncResult, new ScanSpec { KeysOnly = true });
                asyncResult.Join();
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : string.Empty);
                Assert.IsTrue(asyncResult.IsCompleted);
            }

            Assert.AreEqual(CountA + CountB + CountC, c);
        }

        [TestMethod]
        public void ScanTableMaxRowsAsync() {
            if (!HasAsyncTableScanner) {
                return;
            }

            var c = 0;
            using (var asyncResult = new AsyncResult(
                (ctx, cells) =>
                    {
                        foreach (var cell in cells) {
                            Assert.IsFalse(string.IsNullOrEmpty(cell.Key.Row));
                            ++c;
                        }

                        return AsyncCallbackResult.Continue;
                    })) {
                table.BeginScan(asyncResult, new ScanSpec { MaxRows = CountC }.AddColumn("a"));
                asyncResult.Join();
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : string.Empty);
                Assert.IsTrue(asyncResult.IsCompleted);
            }

            Assert.AreEqual(CountC, c);

            c = 0;
            using (var asyncResult = new AsyncResult(
                (ctx, cells) =>
                    {
                        foreach (var cell in cells) {
                            Assert.IsFalse(string.IsNullOrEmpty(cell.Key.Row));
                            ++c;
                        }

                        return AsyncCallbackResult.Continue;
                    })) {
                table.BeginScan(asyncResult, new ScanSpec { MaxRows = CountB }.AddColumn("a"));
                asyncResult.Join();
                Assert.IsNull(asyncResult.Error, asyncResult.Error != null ? asyncResult.Error.ToString() : string.Empty);
                Assert.IsTrue(asyncResult.IsCompleted);
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

        [TestMethod]
        public void Unsupported() {
            if (HasAsyncTableScanner) {
                return;
            }

            try {
                using (var asyncResult = new AsyncResult((ctx, cells) => AsyncCallbackResult.Continue)) {
                    asyncResult.Join();
                }
            }
            catch (NotImplementedException) {
            }
        }

        #endregion

        #region Methods

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