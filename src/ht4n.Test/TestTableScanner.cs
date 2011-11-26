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
    /// Test table scanners.
    /// </summary>
    [TestClass]
    public class TestTableScanner : TestBase
    {
        #region Constants and Fields

        private const int CountA = 100000;

        private const int CountB = 10000;

        private const int CountC = 1000;

        private const int R = 100;

        private const string Schema =
            "<Schema>" + "<AccessGroup name=\"default\" blksz=\"1024\">" + "<ColumnFamily>" + "<Name>a</Name>" + "<deleted>false</deleted>" + "</ColumnFamily>" + "<ColumnFamily>"
            + "<Name>b</Name>" + "<deleted>false</deleted>" + "</ColumnFamily>" + "<ColumnFamily>" + "<Name>c</Name>" + "<deleted>false</deleted>" + "</ColumnFamily>"
            + "<ColumnFamily>" + "<Name>d</Name>" + "<deleted>false</deleted>" + "</ColumnFamily>" + "<ColumnFamily>" + "<Name>e</Name>" + "<deleted>false</deleted>"
            + "</ColumnFamily>" + "<ColumnFamily>" + "<Name>f</Name>" + "<deleted>false</deleted>" + "</ColumnFamily>" + "<ColumnFamily>" + "<Name>g</Name>"
            + "<deleted>false</deleted>" + "</ColumnFamily>" + "</AccessGroup>" + "</Schema>";

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
            table = EnsureTable(typeof(TestTableScanner), Schema);
            InitializeTableData(table);
        }

        [TestMethod]
        public void ScanTable() {
            using (var scanner = table.CreateScanner()) {
                Assert.IsNull(scanner.ScanSpec);
                int c = 0;
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountA + CountB + CountC, c);
            }
        }

        [TestMethod]
        public void ScanTableCancel() {
            using (var scanner = table.CreateScanner()) {
                int c = 0;
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    if (c == CountA) {
                        break;
                    }

                    ++c;
                }

                Assert.AreEqual(CountA, c);
            }
        }

        [TestMethod]
        public void ScanTableCellInterval() {
            using (var _table = EnsureTable("ScanTableCellInterval", Schema)) {
                var key = new Key { ColumnFamily = "d" };
                using (var mutator = _table.CreateMutator()) {
                    for (int i = 0; i < 100; ++i) {
                        key.Row = i.ToString("D2");
                        mutator.Set(key, BitConverter.GetBytes(i));
                    }
                }

                using (var scanner = _table.CreateScanner(new ScanSpec(new CellInterval(null, "d", "50", "d")))) {
                    int c = 0;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                        ++c;
                    }

                    Assert.AreEqual(51, c);
                }

                using (var scanner = _table.CreateScanner(new ScanSpec(new CellInterval(string.Empty, "d", "50", "d")))) {
                    int c = 0;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                        ++c;
                    }

                    Assert.AreEqual(51, c);
                }

                using (var scanner = _table.CreateScanner(new ScanSpec(new CellInterval("90", "d", null, "d")))) {
                    int c = 90;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                        ++c;
                    }

                    Assert.AreEqual(100, c);
                }

                using (var scanner = _table.CreateScanner(new ScanSpec(new CellInterval("90", "d", string.Empty, "d")))) {
                    int c = 90;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                        ++c;
                    }

                    Assert.AreEqual(100, c);
                }

                using (var scanner = _table.CreateScanner(new ScanSpec(new CellInterval("10", "d", "50", "d")))) {
                    int c = 10;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                        ++c;
                    }

                    Assert.AreEqual(51, c);
                }

                using (var scanner = _table.CreateScanner(new ScanSpec(new CellInterval("10", "d", null, true, "50", "d", null, false)))) {
                    int c = 10;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                        ++c;
                    }

                    Assert.AreEqual(50, c);
                }

                using (var scanner = _table.CreateScanner(new ScanSpec(new CellInterval("10", "d", null, false, "50", "d", null, false)))) {
                    int c = 11;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                        ++c;
                    }

                    Assert.AreEqual(50, c);
                }

                using (var scanner = _table.CreateScanner(new ScanSpec(new CellInterval("10", "d", "20", "d")).AddCellInterval(new CellInterval("40", "d", "50", "d")))) {
                    int c = 10;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                        if (c != 20) {
                            ++c;
                        }
                        else {
                            c = 40;
                        }
                    }

                    Assert.AreEqual(51, c);
                }

                key.ColumnFamily = "c";
                using (var mutator = _table.CreateMutator()) {
                    for (int i = 0; i < 100; ++i) {
                        key.Row = i.ToString("D2");
                        mutator.Set(key, BitConverter.GetBytes(i));
                    }
                }

                using (var scanner = _table.CreateScanner(new ScanSpec(new CellInterval("10", "c", "50", "d")))) {
                    int c = 10;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                        Assert.AreEqual("c", cell.Key.ColumnFamily);

                        Assert.IsTrue(scanner.Next(out cell));
                        Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                        Assert.AreEqual("d", cell.Key.ColumnFamily);
                        ++c;
                    }

                    Assert.AreEqual(51, c);
                }

                key.ColumnQualifier = "1";
                using (var mutator = _table.CreateMutator()) {
                    for (int i = 0; i < 100; ++i) {
                        key.Row = i.ToString("D2");
                        mutator.Set(key, BitConverter.GetBytes(i));
                    }
                }

                using (var scanner = _table.CreateScanner(new ScanSpec(new CellInterval("10", "c", "50", "d")))) {
                    int c = 10;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                        Assert.AreEqual("c", cell.Key.ColumnFamily);
                        Assert.AreEqual(string.Empty, cell.Key.ColumnQualifier);

                        Assert.IsTrue(scanner.Next(out cell));
                        Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                        Assert.AreEqual("c", cell.Key.ColumnFamily);
                        Assert.AreEqual("1", cell.Key.ColumnQualifier);

                        Assert.IsTrue(scanner.Next(out cell));
                        Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                        Assert.AreEqual("d", cell.Key.ColumnFamily);
                        Assert.AreEqual(string.Empty, cell.Key.ColumnQualifier);
                        ++c;
                    }

                    Assert.AreEqual(51, c);
                }
            }
        }

        [TestMethod]
        public void ScanTableCellOffset() {
            const int cellOffset = CountB;

            using (var scanner = table.CreateScanner(new ScanSpec { CellOffset = cellOffset })) {
                int c = 0;
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountA + CountB + CountC - cellOffset, c);
            }

            using( var scanner = table.CreateScanner(new ScanSpec { CellOffset = cellOffset }.AddColumn("a")) ) {
                int c = 0;
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountA - cellOffset, c);
            }

            string[] rows = { "AA", "BB", "CC", "DD" };
            string[] columnFamilies = { "d", "e", "f", "g" };
            string[] columnQualifiers = { "0", "1", "2", "3" };

            foreach (string row in rows) {
                using (var mutator = table.CreateMutator()) {
                    foreach (string columnFamily in columnFamilies) {
                        foreach (string columnQualifier in columnQualifiers) {
                            mutator.Set(new Key(row, columnFamily, columnQualifier), Encoding.GetBytes(row + columnFamily + columnQualifier));
                        }
                    }
                }
            }

            using( var scanner = table.CreateScanner(new ScanSpec { CellOffset = rows.Length * columnFamilies.Length * columnQualifiers.Length / 2, ScanAndFilter = true }.AddRows(rows)) ) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(rows.Length * columnFamilies.Length * columnQualifiers.Length / 2, c);
            }

            using( var scanner = table.CreateScanner(new ScanSpec { CellOffset = 1 }.AddRowInterval(new RowInterval("AA", "BB")).AddRowInterval(new RowInterval("CC", "DD"))) ) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(rows.Length * columnFamilies.Length * columnQualifiers.Length - 2, c); // CellOffset applies to each row interval
            }
        }

        [TestMethod]
        public void ScanTableColumnFamily() {
            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("a"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountA, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("b"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountB, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("c"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountC, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("a").AddColumn("b"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountA + CountB, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("a").AddColumn("c"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountA + CountC, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("b").AddColumn("c"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountB + CountC, c);
            }
        }

        [TestMethod]
        public void ScanTableColumnQualifierRegex() {
            var key = new Key { ColumnFamily = "d" };
            using (var mutator = table.CreateMutator()) {
                for (int i = 0; i < 100; ++i) {
                    key.Row = i.ToString("D2");
                    key.ColumnQualifier = (100 + i / 10).ToString("D3");
                    mutator.Set(key, Encoding.GetBytes((1000 + i).ToString("D4")));
                }
            }

            for (int i = 0; i < 10; ++i) {
                using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("d:" + (100 + i).ToString("D3")))) {
                    int c = 0;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        Assert.AreEqual((1000 + 10 * i + c).ToString("D4"), Encoding.GetString(cell.Value));
                        ++c;
                    }

                    Assert.AreEqual(10, c);
                }
            }

            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("d:" + "/10[56]/"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(20, c);
            }
        }

        [TestMethod]
        public void ScanTableColumnQualifyer() {
            string[] columnQualifiers = { null, "1", "2", "3" };

            var key = new Key { Row = Guid.NewGuid().ToString(), ColumnFamily = "d" };
            using (var mutator = table.CreateMutator()) {
                foreach (string columnQualifier in columnQualifiers) {
                    key.ColumnQualifier = columnQualifier;
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                }

                key = new Key { Row = Guid.NewGuid().ToString(), ColumnFamily = "d" };
                foreach (string columnQualifier in columnQualifiers) {
                    key.ColumnQualifier = columnQualifier;
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                }
            }

            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("d"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(2 * columnQualifiers.Length, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(key.Row).AddColumn("d"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    Assert.AreEqual(key.Row, cell.Key.Row);
                    ++c;
                }

                Assert.AreEqual(columnQualifiers.Length, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("d:"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(2, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(key.Row).AddColumn("d:"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    Assert.AreEqual(key.Row, cell.Key.Row);
                    ++c;
                }

                Assert.AreEqual(1, c);
            }

            foreach (string columnQualifier in columnQualifiers) {
                using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("d:" + (!string.IsNullOrEmpty(columnQualifier) ? columnQualifier : string.Empty)))) {
                    int c = 0;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                        ++c;
                    }

                    Assert.AreEqual(2, c);
                }
            }

            foreach (string columnQualifier in columnQualifiers) {
                using (var scanner = table.CreateScanner(new ScanSpec().AddCell(key.Row, "d", !string.IsNullOrEmpty(columnQualifier) ? columnQualifier : string.Empty))) {
                    int c = 0;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                        Assert.AreEqual(key.Row, cell.Key.Row);
                        ++c;
                    }

                    Assert.AreEqual(1, c);
                }
            }
        }

        [TestMethod]
        public void ScanTableDateTimeInterval() {
            string[] items = { "0", "1", "2", "3" };
            var dateTimeSet = new DateTime[items.Length + 1];

            const int Wait = 100;
            int c = 0;
            var key = new Key { ColumnFamily = "d" };
            using (var mutator = table.CreateMutator()) {
                foreach (string item in items) {
                    key.Row = Guid.NewGuid().ToString();
                    dateTimeSet[c++] = key.DateTime = DateTime.UtcNow;
                    mutator.Set(key, Encoding.GetBytes(item));
                    Thread.Sleep(Wait);
                }
            }

            dateTimeSet[c] = DateTime.UtcNow;

            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("d"))) {
                c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.IsTrue(cell.Key.DateTime >= dateTimeSet[0]);
                    Assert.IsTrue(cell.Key.DateTime < dateTimeSet[dateTimeSet.Length - 1]);
                    ++c;
                }

                Assert.AreEqual(items.Length, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { StartDateTime = dateTimeSet[0], EndDateTime = dateTimeSet[dateTimeSet.Length - 1] }.AddColumn("d"))) {
                c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(items.Length, c);
            }

            for (int n = 0; n < items.Length; ++n) {
                using (
                    var scanner =
                        table.CreateScanner(new ScanSpec { StartDateTime = dateTimeSet[n], EndDateTime = dateTimeSet[n] + TimeSpan.FromMilliseconds(Wait / 2) }.AddColumn("d"))) {
                    c = 0;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        ++c;
                    }

                    Assert.AreEqual(1, c);
                }
            }
        }

        [TestMethod]
        public void ScanTableKeyOnly() {
            using (var scanner = table.CreateScanner(new ScanSpec { KeysOnly = true })) {
                int c = 0;
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    Assert.IsFalse(string.IsNullOrEmpty(cell.Key.Row));
                    Assert.IsNull(cell.Value);
                    ++c;
                }

                Assert.AreEqual(CountA + CountB + CountC, c);
            }
        }

        [TestMethod]
        public void ScanTableMaxCells() {
            string[] rows = { "A", "B", "C", "D" };
            string[] columnFamilies = { "d", "e", "f", "g" };
            string[] columnQualifiers = { "0", "1", "2", "3" };

            foreach (string row in rows) {
                using (var mutator = table.CreateMutator()) {
                    foreach (string columnFamily in columnFamilies) {
                        foreach (string columnQualifier in columnQualifiers) {
                            mutator.Set(new Key(row, columnFamily, columnQualifier), Encoding.GetBytes(row + columnFamily + columnQualifier));
                        }
                    }
                }
            }

            using (var scanner = table.CreateScanner(new ScanSpec { MaxCells = 5 })) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(5, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { MaxCellsColumnFamily = 2 }.AddRows(rows))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(rows.Length * columnFamilies.Length * 2, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { MaxCellsColumnFamily = 2, ScanAndFilter = true }.AddRows(rows))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(rows.Length * columnFamilies.Length * 2, c);
            }

            foreach (string columnFamily in columnFamilies) {
                using (var scanner = table.CreateScanner(new ScanSpec { MaxCells = 3, ScanAndFilter = true }.AddRows(rows).AddColumn(columnFamily))) {
                    int c = 0;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        ++c;
                    }

                    Assert.AreEqual(3, c);
                }
            }

            using (var scanner = table.CreateScanner(new ScanSpec { MaxCells = 5 }.AddRowInterval(new RowInterval("A", "B")).AddRowInterval(new RowInterval("C", "D")))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(2 * 5, c); // Applies to each row interval individual
            }

            using (var scanner = table.CreateScanner(new ScanSpec { MaxCellsColumnFamily = 2 }.AddRowInterval(new RowInterval("A", "B")).AddRowInterval(new RowInterval("C", "D")))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(rows.Length * columnFamilies.Length * 2, c);
            }
        }

        [TestMethod]
        public void ScanTableMaxRows() {
            using (var scanner = table.CreateScanner(new ScanSpec { MaxRows = CountC }.AddColumn("a"))) {
                int c = 0;
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountC, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { MaxRows = CountB }.AddColumn("a"))) {
                int c = 0;
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountB, c);
            }

            string[] rows = { "AA", "BB", "CC", "DD" };
            string[] columnFamilies = { "d", "e", "f", "g" };
            string[] columnQualifiers = { "0", "1", "2", "3" };

            foreach (string row in rows) {
                using (var mutator = table.CreateMutator()) {
                    foreach (string columnFamily in columnFamilies) {
                        foreach (string columnQualifier in columnQualifiers) {
                            mutator.Set(new Key(row, columnFamily, columnQualifier), Encoding.GetBytes(row + columnFamily + columnQualifier));
                        }
                    }
                }
            }

            using (var scanner = table.CreateScanner(new ScanSpec { MaxRows = rows.Length / 2, ScanAndFilter = true }.AddRows(rows))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(rows.Length / 2 * columnFamilies.Length * columnQualifiers.Length, c);
            }

            using (
                var scanner = table.CreateScanner(
                    new ScanSpec { MaxRows = rows.Length / 2 }.AddRowInterval(new RowInterval("AA", "BB")).AddRowInterval(new RowInterval("CC", "DD")))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(2 * rows.Length / 2 * columnFamilies.Length * columnQualifiers.Length, c); // MaxRows applies to each row interval individual
            }
        }

        [TestMethod]
        public void ScanTableRandomRows() {
            var rowKeys = new List<string>();
            var cell = new Cell();
            ITableScanner scanner;
            var random = new Random();

            while (rowKeys.Count < R) {
                using (scanner = table.CreateScanner(new ScanSpec { KeysOnly = true }.AddColumn("a"))) {
                    while (scanner.Next(cell)) {
                        Assert.IsNull(cell.Value);
                        if ((random.Next() % 3) == 0) {
                            rowKeys.Add(cell.Key.Row);
                            if (rowKeys.Count == R) {
                                break;
                            }
                        }
                    }
                }
            }

            var scanSpec1 = new ScanSpec().AddColumn("a");
            var scanSpec2 = new ScanSpec();
            foreach (string t in rowKeys) {
                string rowKey = rowKeys[random.Next(rowKeys.Count)];
                scanSpec1.AddRow(rowKey);
                scanSpec2.AddCell(rowKey, "a", null);
            }

            Assert.AreEqual(R, scanSpec1.RowCount);
            Assert.AreEqual(R, scanSpec2.Cells.Count);
            int c = 0;
            using (scanner = table.CreateScanner(scanSpec1)) {
                Assert.AreSame(scanSpec1, scanner.ScanSpec);
                while (scanner.Next(cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    Assert.IsTrue(c < scanSpec1.RowCount);
                    Assert.AreEqual(scanSpec1.Rows[c], cell.Key.Row);
                    ++c;
                }
            }

            Assert.AreEqual(R, c);

            c = 0;
            using (scanner = table.CreateScanner(scanSpec2)) {
                Assert.AreSame(scanSpec2, scanner.ScanSpec);
                while (scanner.Next(cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    Assert.IsTrue(c < scanSpec2.Cells.Count);
                    Assert.AreEqual(scanSpec2.Cells[c].Row, cell.Key.Row);
                    ++c;
                }
            }

            Assert.AreEqual(R, c);
        }

        [TestMethod]
        public void ScanTableRandomRowsIndividualScanner() {
            var rowKeys = new List<string>();
            var cell = new Cell();
            ITableScanner scanner;
            var random = new Random();

            while (rowKeys.Count < R) {
                using (scanner = table.CreateScanner(new ScanSpec { KeysOnly = true }.AddColumn("a"))) {
                    while (scanner.Next(cell)) {
                        Assert.IsNull(cell.Value);
                        if ((random.Next() % 3) == 0) {
                            rowKeys.Add(cell.Key.Row);
                            if (rowKeys.Count == R) {
                                break;
                            }
                        }
                    }
                }
            }

            Assert.AreEqual(R, rowKeys.Count);
            int c = 0;
            foreach (string t in rowKeys) {
                using (scanner = table.CreateScanner(new ScanSpec(new Key(rowKeys[random.Next(rowKeys.Count)], "a", null)))) {
                    while (scanner.Next(cell)) {
                        Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                        ++c;
                    }
                }
            }

            Assert.AreEqual(R, c);
        }

        [TestMethod]
        public void ScanTableRandomRowsScanAndFilter() {
            this.ScanTableRandomRowsScanAndFilter(1);
            this.ScanTableRandomRowsScanAndFilter(2);
            this.ScanTableRandomRowsScanAndFilter(3);
            this.ScanTableRandomRowsScanAndFilter(R);
        }

        [TestMethod]
        public void ScanTableRandomRowsSortedRowKeys() {
            var cell = new Cell();
            ITableScanner scanner;
            var random = new Random();
            var scanSpec1 = new ScanSpec(true).AddColumn("a");
            var scanSpec2 = new ScanSpec(true);

            while (scanSpec1.RowCount < R) {
                using (scanner = table.CreateScanner(new ScanSpec { KeysOnly = true }.AddColumn("a"))) {
                    while (scanner.Next(cell)) {
                        Assert.IsNull(cell.Value);
                        if ((random.Next() % 3) == 0) {
                            scanSpec1.AddRow(cell.Key.Row);
                            scanSpec2.AddCell(cell.Key.Row, "a", null);
                            if (scanSpec1.RowCount == R) {
                                break;
                            }
                        }
                    }
                }
            }

            Assert.AreEqual(R, scanSpec1.RowCount);
            Assert.AreEqual(R, scanSpec2.Cells.Count);

            int c = 0;
            string previousRow = string.Empty;
            using (scanner = table.CreateScanner(scanSpec1)) {
                while (scanner.Next(cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    Assert.IsTrue(c < scanSpec1.RowCount);
                    Assert.AreEqual(scanSpec1.Rows[c], cell.Key.Row);
                    Assert.IsTrue(string.Compare(previousRow, cell.Key.Row) < 0);
                    previousRow = cell.Key.Row;
                    ++c;
                }
            }

            Assert.AreEqual(R, c);

            c = 0;
            previousRow = string.Empty;
            using (scanner = table.CreateScanner(scanSpec2)) {
                while (scanner.Next(cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    Assert.IsTrue(c < scanSpec2.Cells.Count);
                    Assert.AreEqual(scanSpec2.Cells[c].Row, cell.Key.Row);
                    Assert.IsTrue(string.Compare(previousRow, cell.Key.Row) < 0);
                    previousRow = cell.Key.Row;
                    ++c;
                }
            }

            Assert.AreEqual(R, c);
        }

        [TestMethod]
        public void ScanTableRowInterval() {
            var key = new Key { ColumnFamily = "d" };
            using (var mutator = table.CreateMutator()) {
                for (int i = 0; i < 100; ++i) {
                    key.Row = i.ToString("D2");
                    mutator.Set(key, BitConverter.GetBytes(i));
                }
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new RowInterval(null, "50")).AddColumn("d"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                    ++c;
                }

                Assert.AreEqual(51, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new RowInterval(null, false, "50", false)).AddColumn("d"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                    ++c;
                }

                Assert.AreEqual(50, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new RowInterval(string.Empty, "50")).AddColumn("d"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                    ++c;
                }

                Assert.AreEqual(51, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new RowInterval(string.Empty, false, "50", false)).AddColumn("d"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                    ++c;
                }

                Assert.AreEqual(50, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new RowInterval("90", null)).AddColumn("d"))) {
                int c = 90;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                    ++c;
                }

                Assert.AreEqual(100, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new RowInterval("90", false, null, false)).AddColumn("d"))) {
                int c = 91;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                    ++c;
                }

                Assert.AreEqual(100, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new RowInterval("90", string.Empty)).AddColumn("d"))) {
                int c = 90;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                    ++c;
                }

                Assert.AreEqual(100, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new RowInterval("90", false, string.Empty, false)).AddColumn("d"))) {
                int c = 91;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                    ++c;
                }

                Assert.AreEqual(100, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new RowInterval("10", "50")).AddColumn("d"))) {
                int c = 10;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                    ++c;
                }

                Assert.AreEqual(51, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new RowInterval("10", true, "50", false)).AddColumn("d"))) {
                int c = 10;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                    ++c;
                }

                Assert.AreEqual(50, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new RowInterval("10", false, "50", false)).AddColumn("d"))) {
                int c = 11;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                    ++c;
                }

                Assert.AreEqual(50, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new RowInterval("10", "20")).AddRowInterval(new RowInterval("40", "50")).AddColumn("d"))) {
                int c = 10;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                    if (c != 20) {
                        ++c;
                    }
                    else {
                        c = 40;
                    }
                }

                Assert.AreEqual(51, c);
            }
        }

        [TestMethod]
        public void ScanTableRowOffset() {
            var rowKeys = new List<string>(CountA);
            using (var scanner = table.CreateScanner(new ScanSpec { KeysOnly = true }.AddColumn("a"))) {
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    rowKeys.Add(cell.Key.Row);
                }

                Assert.AreEqual(CountA, rowKeys.Count);
            }

            const int rowOffset = CountB;
            int countB = 0;
            int countC = 0;
            using (var scanner = table.CreateScanner(new ScanSpec { KeysOnly = true }.AddColumn("b").AddColumn("c").AddRowInterval(new RowInterval(rowKeys[rowOffset], null)))) {
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    if (cell.Key.ColumnFamily == "b") {
                        ++countB;
                    }
                    else if (cell.Key.ColumnFamily == "c") {
                        ++countC;
                    }
                    else {
                        Assert.Fail();
                    }
                }
            }

            using (var scanner = table.CreateScanner(new ScanSpec { RowOffset = rowOffset })) {
                int c = 0;
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountA - rowOffset + countB + countC, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { RowOffset = rowOffset })) {
                int ca = 0;
                int cb = 0;
                int cc = 0;
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    if (cell.Key.ColumnFamily == "a") {
                        ++ca;
                    }
                    else if (cell.Key.ColumnFamily == "b") {
                        ++cb;
                    }
                    else if (cell.Key.ColumnFamily == "c") {
                        ++cc;
                    }
                    else {
                        Assert.Fail();
                    }
                }

                Assert.AreEqual(CountA - rowOffset, ca);
                Assert.AreEqual(countB, cb);
                Assert.AreEqual(countC, cc);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { RowOffset = rowOffset }.AddColumn("a"))) {
                int c = 0;
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountA - rowOffset, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { RowOffset = rowOffset }.AddColumn("a").AddColumn("b"))) {
                int ca = 0;
                int cb = 0;
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    if (cell.Key.ColumnFamily == "a") {
                        ++ca;
                    }
                    else if (cell.Key.ColumnFamily == "b") {
                        ++cb;
                    }
                    else {
                        Assert.Fail();
                    }
                }

                Assert.AreEqual(CountA - rowOffset, ca);
                Assert.AreEqual(countB, cb);
            }

            string[] rows = { "AA", "BB", "CC", "DD" };
            string[] columnFamilies = { "d", "e", "f", "g" };
            string[] columnQualifiers = { "0", "1", "2", "3" };

            foreach (string row in rows) {
                using (var mutator = table.CreateMutator()) {
                    foreach (string columnFamily in columnFamilies) {
                        foreach (string columnQualifier in columnQualifiers) {
                            mutator.Set(new Key(row, columnFamily, columnQualifier), Encoding.GetBytes(row + columnFamily + columnQualifier));
                        }
                    }
                }
            }

            using (var scanner = table.CreateScanner(new ScanSpec { RowOffset = rows.Length / 2, ScanAndFilter = true }.AddRows(rows))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(rows.Length / 2 * columnFamilies.Length * columnQualifiers.Length, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { RowOffset = 1 }.AddRowInterval(new RowInterval("AA", "BB")).AddRowInterval(new RowInterval("CC", "DD")))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(2 * columnFamilies.Length * columnQualifiers.Length, c); // RowOffset applies to each row interval
            }
        }

        [TestMethod]
        public void ScanTableRowRegex() {
            var key = new Key { Row = "AAA", ColumnFamily = "d" };
            using (var mutator = table.CreateMutator()) {
                mutator.Set(key, Encoding.GetBytes(key.Row));
                key.Row = "AAB";
                mutator.Set(key, Encoding.GetBytes(key.Row));
                key.Row = "ABB";
                mutator.Set(key, Encoding.GetBytes(key.Row));
                key.Row = "BBB";
                mutator.Set(key, Encoding.GetBytes(key.Row));
            }

            using (var scanner = table.CreateScanner(new ScanSpec { RowRegex = "A.*" }.AddColumn("d"))) {
                int c = 0;
                foreach (var cell in scanner) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(3, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { RowRegex = "AA." }.AddColumn("d"))) {
                int c = 0;
                foreach (var cell in scanner) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(2, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { RowRegex = "AB.*" }.AddColumn("d"))) {
                int c = 0;
                foreach (var cell in scanner) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(2, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { RowRegex = "^B.*" }.AddColumn("d"))) {
                int c = 0;
                foreach (var cell in scanner) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(1, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { RowRegex = "^BB.*" }.AddColumn("d"))) {
                int c = 0;
                foreach (var cell in scanner) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(1, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { RowRegex = "^B.." }.AddColumn("d"))) {
                int c = 0;
                foreach (var cell in scanner) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(1, c);
            }
        }

        [TestMethod]
        public void ScanTableValueRegex() {
            var key = new Key { Row = "XXX", ColumnFamily = "d" };
            using (var mutator = table.CreateMutator()) {
                mutator.Set(key, Encoding.GetBytes(key.Row));
                key.Row = "XXY";
                mutator.Set(key, Encoding.GetBytes(key.Row));
                key.Row = "XYY";
                mutator.Set(key, Encoding.GetBytes(key.Row));
                key.Row = "YYY";
                mutator.Set(key, Encoding.GetBytes(key.Row));
            }

            using (var scanner = table.CreateScanner(new ScanSpec { ValueRegex = "X.*" }.AddColumn("d"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(3, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { ValueRegex = "XX." }.AddColumn("d"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(2, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { ValueRegex = "XY.*" }.AddColumn("d"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(2, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { ValueRegex = "^Y.*" }.AddColumn("d"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(1, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { ValueRegex = "^YY.*" }.AddColumn("d"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(1, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { ValueRegex = "^Y.." }.AddColumn("d"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(1, c);
            }
        }

        [TestMethod]
        public void ScanTableVersionLimit() {
            string[] rows = { "A", "B", "C", "D" };
            string[] versions = { "0", "1", "2", "3" };

            foreach (string row in rows) {
                var key = new Key { Row = row, ColumnFamily = "d" };
                using (var mutator = table.CreateMutator()) {
                    foreach (string version in versions) {
                        mutator.Set(key, Encoding.GetBytes(version));
                    }
                }
            }

            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("d"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(versions[versions.Length - (c % versions.Length) - 1], Encoding.GetString(cell.Value));
                    Assert.AreEqual(rows[c / versions.Length], cell.Key.Row);
                    ++c;
                }

                Assert.AreEqual(rows.Length * versions.Length, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { MaxVersions = 1 }.AddColumn("d"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(versions[versions.Length - 1], Encoding.GetString(cell.Value));
                    Assert.AreEqual(rows[c], cell.Key.Row);
                    ++c;
                }

                Assert.AreEqual(rows.Length, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { MaxVersions = 2 }.AddColumn("d"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(versions[versions.Length - (c % 2) - 1], Encoding.GetString(cell.Value));
                    Assert.AreEqual(rows[c / 2], cell.Key.Row);
                    ++c;
                }

                Assert.AreEqual(rows.Length * 2, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { MaxVersions = 1 }.AddColumn("d").AddRow("A").AddRow("B"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(2, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { ScanAndFilter = true, MaxVersions = 1 }.AddColumn("d").AddRow("A").AddRow("B"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(2, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { MaxVersions = 2 }.AddColumn("d").AddRow("B").AddRow("C"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(4, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { ScanAndFilter = true, MaxVersions = 2 }.AddColumn("d").AddRow("B").AddRow("C"))) {
                int c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(4, c);
            }
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

        private void ScanTableRandomRowsScanAndFilter(int count) {
            var cell = new Cell();
            ITableScanner scanner;
            var random = new Random();
            var scanSpec = new ScanSpec { ScanAndFilter = true }.AddColumn("a");

            while (scanSpec.RowCount < count) {
                using (scanner = table.CreateScanner(new ScanSpec { KeysOnly = true }.AddColumn("a"))) {
                    while (scanner.Next(cell)) {
                        Assert.IsNull(cell.Value);
                        if ((random.Next() % 3) == 0) {
                            scanSpec.AddRow(cell.Key.Row);
                            if (scanSpec.RowCount == count) {
                                break;
                            }
                        }
                    }
                }
            }

            Assert.AreEqual(count, scanSpec.RowCount);

            int c = 0;
            string previousRow = string.Empty;
            using (scanner = table.CreateScanner(scanSpec)) {
                while (scanner.Next(cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    Assert.IsTrue(c < scanSpec.RowCount);
                    Assert.IsTrue(string.Compare(previousRow, cell.Key.Row) < 0);
                    previousRow = cell.Key.Row;
                    ++c;
                }
            }

            Assert.AreEqual(count, c);
        }

        #endregion
    }
}