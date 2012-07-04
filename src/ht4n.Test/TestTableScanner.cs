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
    using System.Globalization;
    using System.Linq;
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
            table = EnsureTable(typeof(TestTableScanner), Schema);
            InitializeTableData(table);
        }

        [TestMethod]
        public void ScanTable() {
            using (var scanner = table.CreateScanner()) {
                Assert.IsNull(scanner.ScanSpec);
                var c = 0;
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
                var c = 0;
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
                    for (var i = 0; i < 100; ++i) {
                        key.Row = i.ToString("D2");
                        mutator.Set(key, BitConverter.GetBytes(i));
                    }
                }

                using (var scanner = _table.CreateScanner(new ScanSpec(new CellInterval(null, "d", "50", "d")))) {
                    var c = 0;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                        ++c;
                    }

                    Assert.AreEqual(51, c);
                }

                using (var scanner = _table.CreateScanner(new ScanSpec(new CellInterval(string.Empty, "d", "50", "d")))) {
                    var c = 0;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                        ++c;
                    }

                    Assert.AreEqual(51, c);
                }

                using (var scanner = _table.CreateScanner(new ScanSpec(new CellInterval("90", "d", null, "d")))) {
                    var c = 90;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                        ++c;
                    }

                    Assert.AreEqual(100, c);
                }

                using (var scanner = _table.CreateScanner(new ScanSpec(new CellInterval("90", "d", string.Empty, "d")))) {
                    var c = 90;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                        ++c;
                    }

                    Assert.AreEqual(100, c);
                }

                using (var scanner = _table.CreateScanner(new ScanSpec(new CellInterval("10", "d", "50", "d")))) {
                    var c = 10;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                        ++c;
                    }

                    Assert.AreEqual(51, c);
                }

                using (var scanner = _table.CreateScanner(new ScanSpec(new CellInterval("10", "d", null, true, "50", "d", null, false)))) {
                    var c = 10;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                        ++c;
                    }

                    Assert.AreEqual(50, c);
                }

                using (var scanner = _table.CreateScanner(new ScanSpec(new CellInterval("10", "d", null, false, "50", "d", null, false)))) {
                    var c = 11;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                        ++c;
                    }

                    Assert.AreEqual(50, c);
                }

                using (var scanner = _table.CreateScanner(new ScanSpec().AddCellInterval(new CellInterval("10", "d", "20", "d"), new CellInterval("40", "d", "50", "d")))
                    ) {
                    var c = 10;
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
                    for (var i = 0; i < 100; ++i) {
                        key.Row = i.ToString("D2");
                        mutator.Set(key, BitConverter.GetBytes(i));
                    }
                }

                using (var scanner = _table.CreateScanner(new ScanSpec(new CellInterval("10", "c", "50", "d")))) {
                    var c = 10;
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
                    for (var i = 0; i < 100; ++i) {
                        key.Row = i.ToString("D2");
                        mutator.Set(key, BitConverter.GetBytes(i));
                    }
                }

                using (var scanner = _table.CreateScanner(new ScanSpec(new CellInterval("10", "c", "50", "d")))) {
                    var c = 10;
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

                key.ColumnQualifier = null;
                using (var mutator = _table.CreateMutator()) {
                    key.Row = "10";
                    key.ColumnFamily = "a";
                    mutator.Set(key, Encoding.GetBytes(key.Row + key.ColumnFamily));
                    key.ColumnFamily = "b";
                    mutator.Set(key, Encoding.GetBytes(key.Row + key.ColumnFamily));
                    key.ColumnFamily = "e";
                    mutator.Set(key, Encoding.GetBytes(key.Row + key.ColumnFamily));
                    key.Row = "11";
                    key.ColumnFamily = "a";
                    mutator.Set(key, Encoding.GetBytes(key.Row + key.ColumnFamily));
                    key.ColumnFamily = "b";
                    mutator.Set(key, Encoding.GetBytes(key.Row + key.ColumnFamily));
                    key.ColumnFamily = "e";
                    mutator.Set(key, Encoding.GetBytes(key.Row + key.ColumnFamily));
                }

                using (var scanner = _table.CreateScanner(new ScanSpec(new CellInterval("10", "c", "11", "d")))) {
                    Cell cell;
                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("10", cell.Key.Row);
                    Assert.AreEqual("c", cell.Key.ColumnFamily);
                    Assert.AreEqual(string.Empty, cell.Key.ColumnQualifier);
                    Assert.AreEqual(10, BitConverter.ToInt32(cell.Value, 0));

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("10", cell.Key.Row);
                    Assert.AreEqual("c", cell.Key.ColumnFamily);
                    Assert.AreEqual("1", cell.Key.ColumnQualifier);
                    Assert.AreEqual(10, BitConverter.ToInt32(cell.Value, 0));

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("10", cell.Key.Row);
                    Assert.AreEqual("d", cell.Key.ColumnFamily);
                    Assert.AreEqual(string.Empty, cell.Key.ColumnQualifier);
                    Assert.AreEqual(10, BitConverter.ToInt32(cell.Value, 0));

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("10", cell.Key.Row);
                    Assert.AreEqual("e", cell.Key.ColumnFamily);
                    Assert.AreEqual(string.Empty, cell.Key.ColumnQualifier);
                    Assert.AreEqual("10e", Encoding.GetString(cell.Value));

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("11", cell.Key.Row);
                    Assert.AreEqual("11a", Encoding.GetString(cell.Value));
                    Assert.AreEqual("a", cell.Key.ColumnFamily);
                    Assert.AreEqual(string.Empty, cell.Key.ColumnQualifier);

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("11", cell.Key.Row);
                    Assert.AreEqual("b", cell.Key.ColumnFamily);
                    Assert.AreEqual(string.Empty, cell.Key.ColumnQualifier);
                    Assert.AreEqual("11b", Encoding.GetString(cell.Value));

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("11", cell.Key.Row);
                    Assert.AreEqual("c", cell.Key.ColumnFamily);
                    Assert.AreEqual(string.Empty, cell.Key.ColumnQualifier);
                    Assert.AreEqual(11, BitConverter.ToInt32(cell.Value, 0));

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("11", cell.Key.Row);
                    Assert.AreEqual("c", cell.Key.ColumnFamily);
                    Assert.AreEqual("1", cell.Key.ColumnQualifier);
                    Assert.AreEqual(11, BitConverter.ToInt32(cell.Value, 0));

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("11", cell.Key.Row);
                    Assert.AreEqual("d", cell.Key.ColumnFamily);
                    Assert.AreEqual(string.Empty, cell.Key.ColumnQualifier);
                    Assert.AreEqual(11, BitConverter.ToInt32(cell.Value, 0));

                    Assert.IsFalse(scanner.Next(out cell));
                }

                key.ColumnQualifier = "1";
                using (var mutator = _table.CreateMutator()) {
                    key.Row = "11";
                    key.ColumnFamily = "d";
                    mutator.Set(key, Encoding.GetBytes(key.Row + key.ColumnFamily + key.ColumnQualifier));
                }

                using (var scanner = _table.CreateScanner(new ScanSpec(new CellInterval("10", "c", "1", "11", "d", "1")))) {
                    Cell cell;
                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("10", cell.Key.Row);
                    Assert.AreEqual("c", cell.Key.ColumnFamily);
                    Assert.AreEqual("1", cell.Key.ColumnQualifier);
                    Assert.AreEqual(10, BitConverter.ToInt32(cell.Value, 0));

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("10", cell.Key.Row);
                    Assert.AreEqual("d", cell.Key.ColumnFamily);
                    Assert.AreEqual(string.Empty, cell.Key.ColumnQualifier);
                    Assert.AreEqual(10, BitConverter.ToInt32(cell.Value, 0));

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("10", cell.Key.Row);
                    Assert.AreEqual("e", cell.Key.ColumnFamily);
                    Assert.AreEqual(string.Empty, cell.Key.ColumnQualifier);
                    Assert.AreEqual("10e", Encoding.GetString(cell.Value));

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("11", cell.Key.Row);
                    Assert.AreEqual("a", cell.Key.ColumnFamily);
                    Assert.AreEqual(string.Empty, cell.Key.ColumnQualifier);
                    Assert.AreEqual("11a", Encoding.GetString(cell.Value));

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("11", cell.Key.Row);
                    Assert.AreEqual("b", cell.Key.ColumnFamily);
                    Assert.AreEqual(string.Empty, cell.Key.ColumnQualifier);
                    Assert.AreEqual("11b", Encoding.GetString(cell.Value));

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("11", cell.Key.Row);
                    Assert.AreEqual("c", cell.Key.ColumnFamily);
                    Assert.AreEqual(string.Empty, cell.Key.ColumnQualifier);
                    Assert.AreEqual(11, BitConverter.ToInt32(cell.Value, 0));

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("11", cell.Key.Row);
                    Assert.AreEqual("c", cell.Key.ColumnFamily);
                    Assert.AreEqual("1", cell.Key.ColumnQualifier);
                    Assert.AreEqual(11, BitConverter.ToInt32(cell.Value, 0));

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("11", cell.Key.Row);
                    Assert.AreEqual("d", cell.Key.ColumnFamily);
                    Assert.AreEqual(string.Empty, cell.Key.ColumnQualifier);
                    Assert.AreEqual(11, BitConverter.ToInt32(cell.Value, 0));

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("11", cell.Key.Row);
                    Assert.AreEqual("d", cell.Key.ColumnFamily);
                    Assert.AreEqual("1", cell.Key.ColumnQualifier);
                    Assert.AreEqual("11d1", Encoding.GetString(cell.Value));

                    Assert.IsFalse(scanner.Next(out cell));
                }

                using (var scanner = _table.CreateScanner(new ScanSpec(new CellInterval("10", "c", "1", false, "11", "d", "1", false)))) {
                    Cell cell;
                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("10", cell.Key.Row);
                    Assert.AreEqual("d", cell.Key.ColumnFamily);
                    Assert.AreEqual(string.Empty, cell.Key.ColumnQualifier);
                    Assert.AreEqual(10, BitConverter.ToInt32(cell.Value, 0));

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("10", cell.Key.Row);
                    Assert.AreEqual("e", cell.Key.ColumnFamily);
                    Assert.AreEqual(string.Empty, cell.Key.ColumnQualifier);
                    Assert.AreEqual("10e", Encoding.GetString(cell.Value));

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("11", cell.Key.Row);
                    Assert.AreEqual("a", cell.Key.ColumnFamily);
                    Assert.AreEqual(string.Empty, cell.Key.ColumnQualifier);
                    Assert.AreEqual("11a", Encoding.GetString(cell.Value));

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("11", cell.Key.Row);
                    Assert.AreEqual("b", cell.Key.ColumnFamily);
                    Assert.AreEqual(string.Empty, cell.Key.ColumnQualifier);
                    Assert.AreEqual("11b", Encoding.GetString(cell.Value));

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("11", cell.Key.Row);
                    Assert.AreEqual("c", cell.Key.ColumnFamily);
                    Assert.AreEqual(string.Empty, cell.Key.ColumnQualifier);
                    Assert.AreEqual(11, BitConverter.ToInt32(cell.Value, 0));

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("11", cell.Key.Row);
                    Assert.AreEqual("c", cell.Key.ColumnFamily);
                    Assert.AreEqual("1", cell.Key.ColumnQualifier);
                    Assert.AreEqual(11, BitConverter.ToInt32(cell.Value, 0));

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("11", cell.Key.Row);
                    Assert.AreEqual("d", cell.Key.ColumnFamily);
                    Assert.AreEqual(string.Empty, cell.Key.ColumnQualifier);
                    Assert.AreEqual(11, BitConverter.ToInt32(cell.Value, 0));

                    Assert.IsFalse(scanner.Next(out cell));
                }

                using (var scanner = _table.CreateScanner(new ScanSpec(new CellInterval("10", "c", "1", false, "11", "d", null, false)))) {
                    Cell cell;
                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual(10, BitConverter.ToInt32(cell.Value, 0));
                    Assert.AreEqual("d", cell.Key.ColumnFamily);
                    Assert.AreEqual(string.Empty, cell.Key.ColumnQualifier);

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("10e", Encoding.GetString(cell.Value));
                    Assert.AreEqual("e", cell.Key.ColumnFamily);
                    Assert.AreEqual(string.Empty, cell.Key.ColumnQualifier);

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("11a", Encoding.GetString(cell.Value));
                    Assert.AreEqual("a", cell.Key.ColumnFamily);
                    Assert.AreEqual(string.Empty, cell.Key.ColumnQualifier);

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual("11b", Encoding.GetString(cell.Value));
                    Assert.AreEqual("b", cell.Key.ColumnFamily);
                    Assert.AreEqual(string.Empty, cell.Key.ColumnQualifier);

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual(11, BitConverter.ToInt32(cell.Value, 0));
                    Assert.AreEqual("c", cell.Key.ColumnFamily);
                    Assert.AreEqual(string.Empty, cell.Key.ColumnQualifier);

                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual(11, BitConverter.ToInt32(cell.Value, 0));
                    Assert.AreEqual("c", cell.Key.ColumnFamily);
                    Assert.AreEqual("1", cell.Key.ColumnQualifier);

                    Assert.IsFalse(scanner.Next(out cell));
                }
            }
        }

        [TestMethod]
        public void ScanTableCellOffset() {
            const int cellOffset = CountB;

            using (var scanner = table.CreateScanner(new ScanSpec { CellOffset = cellOffset })) {
                var c = 0;
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountA + CountB + CountC - cellOffset, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { CellOffset = cellOffset }.AddColumn("a"))) {
                var c = 0;
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

            foreach (var row in rows) {
                using (var mutator = table.CreateMutator()) {
                    foreach (var columnFamily in columnFamilies) {
                        foreach (var columnQualifier in columnQualifiers) {
                            mutator.Set(new Key(row, columnFamily, columnQualifier), Encoding.GetBytes(row + columnFamily + columnQualifier));
                        }
                    }
                }
            }

            using (
                var scanner =
                    table.CreateScanner(new ScanSpec { CellOffset = rows.Length * columnFamilies.Length * columnQualifiers.Length / 2, ScanAndFilter = true }.AddRow(rows))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(rows.Length * columnFamilies.Length * columnQualifiers.Length / 2, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { CellOffset = 1 }.AddRowInterval(new RowInterval("AA", "BB"), new RowInterval("CC", "DD")))) {
                var c = 0;
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
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountA, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("b"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountB, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("c"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountC, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("a", "b"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountA + CountB, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("a", "c"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountA + CountC, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("b", "c"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountB + CountC, c);
            }
        }

        [TestMethod]
        public void ScanTableColumnPredicate() {
            var key = new Key { Row = "XXX", ColumnFamily = "d" };
            using (var mutator = table.CreateMutator()) {
                mutator.Set(key, Encoding.GetBytes(key.Row));
                key.Row = "XXY";
                mutator.Set(key, Encoding.GetBytes(key.Row));
                key.Row = "XYY";
                mutator.Set(key, Encoding.GetBytes(key.Row));
                key.Row = "YYY";
                mutator.Set(key, Encoding.GetBytes(key.Row));
                key.Row = "ZZX";
                mutator.Set(key, Encoding.GetBytes(string.Empty));
                key.Row = "ZZY";
                mutator.Set(key, null);
                key.Row = "ZZZ";
                mutator.Set(key, Encoding.GetBytes("What a wonderful world"));
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new ColumnPredicate("d", MatchKind.Exact, Encoding.GetBytes("XYY"))))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(1, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new ColumnPredicate("d", MatchKind.Exact, Encoding.GetBytes(string.Empty))))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.IsNull(cell.Value);
                    ++c;
                }

                Assert.AreEqual(2, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new ColumnPredicate("d", MatchKind.Exact, null)))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.IsNull(cell.Value);
                    ++c;
                }

                Assert.AreEqual(2, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new ColumnPredicate("d", MatchKind.Prefix, Encoding.GetBytes(string.Empty))))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(7, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new ColumnPredicate("d", MatchKind.Prefix, null)))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(7, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new ColumnPredicate("d", MatchKind.Prefix, Encoding.GetBytes("X"))))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(3, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new ColumnPredicate("d", MatchKind.Prefix, Encoding.GetBytes("XX"))))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(2, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new ColumnPredicate("d", MatchKind.Prefix, Encoding.GetBytes("Y"))))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(1, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new ColumnPredicate("d", MatchKind.Prefix, Encoding.GetBytes("ZYX"))))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(0, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new ColumnPredicate("d", MatchKind.Contains, Encoding.GetBytes("wonder"))))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual("ZZZ", cell.Key.Row);
                    ++c;
                }

                Assert.AreEqual(1, c);
            }

            key = new Key { Row = "XXX", ColumnFamily = "e" };
            using (var mutator = table.CreateMutator()) {
                mutator.Set(key, Encoding.GetBytes(key.Row));
                key.Row = "XXY";
                mutator.Set(key, Encoding.GetBytes(key.Row));
                key.Row = "XYY";
                mutator.Set(key, Encoding.GetBytes(key.Row));
                key.Row = "YYY";
                mutator.Set(key, Encoding.GetBytes(key.Row));
                key.Row = "ZZX";
                mutator.Set(key, Encoding.GetBytes(string.Empty));
                key.Row = "ZZY";
                mutator.Set(key, null);
                key.Row = "ZZZ";
                mutator.Set(key, Encoding.GetBytes("What's going on?"));
            }

            var scanSpec = new ScanSpec()
                .AddColumnPredicate(new ColumnPredicate("d", MatchKind.Exact, Encoding.GetBytes("XYY")))
                .AddColumnPredicate(new ColumnPredicate("e", MatchKind.Exact, Encoding.GetBytes("XYY")));

            using (var scanner = table.CreateScanner(scanSpec)) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(2, c);
            }

            scanSpec = new ScanSpec()
                .AddColumnPredicate(new ColumnPredicate("d", MatchKind.Prefix, Encoding.GetBytes("X")))
                .AddColumnPredicate(new ColumnPredicate("e", MatchKind.Prefix, Encoding.GetBytes("X")));

            using (var scanner = table.CreateScanner(scanSpec)) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(6, c);
            }

            scanSpec = new ScanSpec()
                .AddColumnPredicate(new ColumnPredicate("d", MatchKind.Prefix, Encoding.GetBytes("XX")))
                .AddColumnPredicate(new ColumnPredicate("e", MatchKind.Prefix, Encoding.GetBytes("XX")));

            using (var scanner = table.CreateScanner(scanSpec)) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(4, c);
            }

            scanSpec = new ScanSpec()
                .AddColumnPredicate(new ColumnPredicate("d", MatchKind.Prefix, Encoding.GetBytes("Y")))
                .AddColumnPredicate(new ColumnPredicate("e", MatchKind.Prefix, Encoding.GetBytes("Y")));

            using (var scanner = table.CreateScanner(scanSpec)) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(2, c);
            }

            scanSpec = new ScanSpec()
                .AddColumnPredicate(new ColumnPredicate("d", MatchKind.Exact, Encoding.GetBytes("ZYX")))
                .AddColumnPredicate(new ColumnPredicate("e", MatchKind.Exact, Encoding.GetBytes("ZYX")));

            using (var scanner = table.CreateScanner(scanSpec)) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(0, c);
            }

            scanSpec = new ScanSpec()
                .AddColumnPredicate(new ColumnPredicate("d", MatchKind.Exact, Encoding.GetBytes(string.Empty)))
                .AddColumnPredicate(new ColumnPredicate("e", MatchKind.Exact, Encoding.GetBytes(string.Empty)));

            using (var scanner = table.CreateScanner(scanSpec)) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.IsTrue(cell.Key.Row.StartsWith("ZZ"));
                    Assert.IsNull(cell.Value);
                    ++c;
                }

                Assert.AreEqual(4, c);
            }

            scanSpec = new ScanSpec()
                .AddColumnPredicate(new ColumnPredicate("d", MatchKind.Exact, null))
                .AddColumnPredicate(new ColumnPredicate("e", MatchKind.Exact, null));

            using (var scanner = table.CreateScanner(scanSpec)) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.IsTrue(cell.Key.Row.StartsWith("ZZ"));
                    Assert.IsNull(cell.Value);
                    ++c;
                }

                Assert.AreEqual(4, c);
            }

            scanSpec = new ScanSpec()
                .AddColumnPredicate(new ColumnPredicate("d", MatchKind.Prefix, Encoding.GetBytes(string.Empty)))
                .AddColumnPredicate(new ColumnPredicate("e", MatchKind.Prefix, Encoding.GetBytes(string.Empty)));

            using (var scanner = table.CreateScanner(scanSpec)) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(14, c);
            }

            scanSpec = new ScanSpec()
                .AddColumnPredicate(new ColumnPredicate("d", MatchKind.Prefix, null))
                .AddColumnPredicate(new ColumnPredicate("e", MatchKind.Prefix, null));

            using (var scanner = table.CreateScanner(scanSpec)) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(14, c);
            }

            scanSpec = new ScanSpec()
                .AddColumnPredicate(new ColumnPredicate("d", MatchKind.Contains, Encoding.GetBytes("wonder")))
                .AddColumnPredicate(new ColumnPredicate("e", MatchKind.Contains, Encoding.GetBytes("What's")));

            using (var scanner = table.CreateScanner(scanSpec)) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual("ZZZ", cell.Key.Row);
                    ++c;
                }

                Assert.AreEqual(2, c);
            }

            scanSpec = new ScanSpec()
                .AddColumnPredicate(new ColumnPredicate("d", MatchKind.Contains, Encoding.GetBytes("on")))
                .AddColumnPredicate(new ColumnPredicate("e", MatchKind.Contains, Encoding.GetBytes("on")));

            using (var scanner = table.CreateScanner(scanSpec)) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual("ZZZ", cell.Key.Row);
                    ++c;
                }

                Assert.AreEqual(2, c);
            }
        }

        [TestMethod]
        public void ScanTableColumnQualifierIndex() {
            const string ScanTableValueIndexSchema =
                "<Schema><AccessGroup name=\"default\" blksz=\"1024\">" +
                "<ColumnFamily><Name>a</Name><QualifierIndex>true</QualifierIndex></ColumnFamily>" +
                "<ColumnFamily><Name>b</Name><QualifierIndex>true</QualifierIndex></ColumnFamily>" +
                "<ColumnFamily><Name>c</Name><QualifierIndex>true</QualifierIndex></ColumnFamily>" +
                "</AccessGroup></Schema>";

            const int Count = 10000;
            using (var _table = EnsureTable("ScanTableColumnQualifierIndex", ScanTableValueIndexSchema)) {
                if (IsHyper || IsThrift) {
                    Assert.IsTrue(Ns.TableExists("^^ScanTableColumnQualifierIndex"));
                }

                var key = new Key();
                using (var mutator = _table.CreateMutator()) {
                    for (var i = 0; i < Count; ++i) {
                        var qualifier = i.ToString();
                        var value = Encoding.GetBytes(qualifier);

                        key.ColumnFamily = "a";
                        key.ColumnQualifier = qualifier;
                        key.Row = null;
                        mutator.Set(key, value);

                        key.ColumnFamily = "b";
                        key.ColumnQualifier = qualifier;
                        key.Row = null;
                        mutator.Set(key, value);

                        key.ColumnFamily = "c";
                        key.ColumnQualifier = qualifier;
                        key.Row = null;
                        mutator.Set(key, value);
                    }
                }

                string[] columnFamilies = { "a", "b", "c" };
                var rng = new Random();
                for (var i = 0; i < Count / 100; ++i) {
                    var qualifier = rng.Next(Count).ToString();
                    var scanSpec = new ScanSpec().AddColumn(columnFamilies[rng.Next(columnFamilies.Length)] + ":" + qualifier);
                    using (var scanner = _table.CreateScanner(scanSpec)) {
                        Cell cell;
                        Assert.IsTrue(scanner.Next(out cell));
                        Assert.AreEqual(qualifier, Encoding.GetString(cell.Value));
                        Assert.IsFalse(scanner.Next(out cell));
                    }
                }

                int[] ranges = { 1000, 300, 56, 9999, 789, 4 };
                int[] occurrence = { 1, 11, 111, 1, 11, 1111 };
                Assert.AreEqual(ranges.Length, occurrence.Length);
                for (var i = 0; i < ranges.Length; ++i) {
                    var qualifier = ranges[i].ToString();
                    var scanSpec = new ScanSpec().AddColumn(columnFamilies[rng.Next(columnFamilies.Length)] + ":^" + qualifier);
                    using (var scanner = _table.CreateScanner(scanSpec)) {
                        var c = 0;
                        var cell = new Cell();
                        while (scanner.Next(cell)) {
                            Assert.IsTrue(Encoding.GetString(cell.Value).StartsWith(qualifier));
                            ++c;
                        }

                        Assert.AreEqual(occurrence[i], c, "range = {0}", ranges[i]);
                    }
                }
            }
        }

        [TestMethod]
        public void ScanTableColumnQualifierPrefix() {
            var key = new Key { ColumnFamily = "d" };
            using (var mutator = table.CreateMutator()) {
                for (var i = 0; i < 100; ++i) {
                    key.Row = i.ToString("D2");
                    key.ColumnQualifier = (100 + i / 10).ToString("D3") + (1000 + i).ToString("D4");
                    mutator.Set(key, Encoding.GetBytes((1000 + i).ToString("D4")));
                }
            }

            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("d:^1"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(100, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("d:^10"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(100, c);
            }

            for (var i = 0; i < 10; ++i) {
                using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("d:^" + (100 + i).ToString("D3")))) {
                    var c = 0;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        Assert.AreEqual((1000 + 10 * i + c).ToString("D4"), Encoding.GetString(cell.Value));
                        ++c;
                    }

                    Assert.AreEqual(10, c);
                }
            }

            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("d:^10510"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(10, c);
            }
        }

        [TestMethod]
        public void ScanTableColumnQualifierRegex() {
            var key = new Key { ColumnFamily = "d" };
            using (var mutator = table.CreateMutator()) {
                for (var i = 0; i < 100; ++i) {
                    key.Row = i.ToString("D2");
                    key.ColumnQualifier = (100 + i / 10).ToString("D3");
                    mutator.Set(key, Encoding.GetBytes((1000 + i).ToString("D4")));
                }
            }

            for (var i = 0; i < 10; ++i) {
                using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("d:" + (100 + i).ToString("D3")))) {
                    var c = 0;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        Assert.AreEqual((1000 + 10 * i + c).ToString("D4"), Encoding.GetString(cell.Value));
                        ++c;
                    }

                    Assert.AreEqual(10, c);
                }
            }

            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("d:/10[56]/"))) {
                var c = 0;
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
                foreach (var columnQualifier in columnQualifiers) {
                    key.ColumnQualifier = columnQualifier;
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                }

                key = new Key { Row = Guid.NewGuid().ToString(), ColumnFamily = "d" };
                foreach (var columnQualifier in columnQualifiers) {
                    key.ColumnQualifier = columnQualifier;
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                }
            }

            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("d"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(2 * columnQualifiers.Length, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(key.Row).AddColumn("d"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    Assert.AreEqual(key.Row, cell.Key.Row);
                    ++c;
                }

                Assert.AreEqual(columnQualifiers.Length, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("d:"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(2, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(key.Row).AddColumn("d:"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    Assert.AreEqual(key.Row, cell.Key.Row);
                    ++c;
                }

                Assert.AreEqual(1, c);
            }

            foreach (var columnQualifier in columnQualifiers) {
                using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("d:" + (!string.IsNullOrEmpty(columnQualifier) ? columnQualifier : string.Empty)))) {
                    var c = 0;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                        ++c;
                    }

                    Assert.AreEqual(2, c);
                }
            }

            foreach (var columnQualifier in columnQualifiers) {
                using (var scanner = table.CreateScanner(new ScanSpec().AddCell(key.Row, "d", !string.IsNullOrEmpty(columnQualifier) ? columnQualifier : string.Empty))) {
                    var c = 0;
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
            var c = 0;
            var key = new Key { ColumnFamily = "d" };
            using (var mutator = table.CreateMutator()) {
                foreach (var item in items) {
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

            for (var n = 0; n < items.Length; ++n) {
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
        public void ScanTableDateTimeIntervalDesc() {
            const string SchemaWithTimeOrder =
                "<Schema><AccessGroup name=\"default\">" +
                "<ColumnFamily><Name>desc</Name><TimeOrder>DESC</TimeOrder></ColumnFamily>" +
                "</AccessGroup></Schema>";

            using( var _table = EnsureTable("ScanTableDateTimeIntervalDesc", SchemaWithTimeOrder) ) {
                string[] items = { "0", "1", "2", "3" };
                var dateTimeSet = new DateTime[items.Length + 1];

                const int Wait = 100;
                var c = 0;
                var key = new Key("row") { ColumnFamily = "desc" };
                using( var mutator = _table.CreateMutator() ) {
                    foreach (var item in items) {
                        dateTimeSet[c++] = key.DateTime = DateTime.UtcNow;
                        mutator.Set(key, Encoding.GetBytes(item));
                        Thread.Sleep(Wait);
                    }
                }

                dateTimeSet[c] = DateTime.UtcNow;

                using( var scanner = _table.CreateScanner() ) {
                    var prevDateTime = new DateTime(0);
                    c = 0;
                    Cell cell;
                    while( scanner.Next(out cell) ) {
                        Assert.IsTrue(cell.Key.DateTime > prevDateTime);
                        prevDateTime = cell.Key.DateTime;
                        ++c;
                    }

                    Assert.AreEqual(items.Length, c);
                }

                var scanSpec = ScanSpecBuilder
                    .Create()
                        .WithColumns("desc")
                        .WithRows()
                    .Build();

                using( var scanner = _table.CreateScanner(scanSpec) ) {
                    var prevDateTime = new DateTime(0);
                    c = 0;
                    Cell cell;
                    while( scanner.Next(out cell) ) {
                        Assert.IsTrue(cell.Key.DateTime > prevDateTime);
                        prevDateTime = cell.Key.DateTime;
                        ++c;
                    }

                    Assert.AreEqual(items.Length, c);
                }

                scanSpec = ScanSpecBuilder
                    .Create()
                        .WithColumns("desc")
                        .WithRows()
                        .StartDateTime(dateTimeSet[0])
                        .EndDateTime(dateTimeSet[dateTimeSet.Length - 1])
                    .Build();

                using( var scanner = _table.CreateScanner(scanSpec) ) {
                    c = 0;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        ++c;
                    }

                    Assert.AreEqual(items.Length, c);
                }

                for (var n = 0; n < items.Length; ++n) {
                    scanSpec = ScanSpecBuilder
                        .Create()
                            .WithColumns("desc")
                            .WithRows()
                            .StartDateTime(dateTimeSet[n])
                            .EndDateTime(dateTimeSet[n] + TimeSpan.FromMilliseconds(Wait / 2))
                        .Build();

                    using (var scanner = _table.CreateScanner(scanSpec)) {
                        c = 0;
                        Cell cell;
                        while (scanner.Next(out cell)) {
                            ++c;
                        }

                        Assert.AreEqual(1, c);
                    }
                }
            }
        }

        [TestMethod]
        public void ScanTableDateTimeIntervalAscDesc() {
            const string SchemaWithTimeOrder =
                "<Schema><AccessGroup name=\"default\">" +
                "<ColumnFamily><Name>asc</Name><TimeOrder>ASC</TimeOrder></ColumnFamily>" +
                "<ColumnFamily><Name>desc</Name><TimeOrder>DESC</TimeOrder></ColumnFamily>" +
                "</AccessGroup></Schema>";

            using( var _table = EnsureTable("ScanTableDateTimeIntervalAscDesc", SchemaWithTimeOrder) ) {
                string[] items = { "0", "1", "2", "3" };
                var dateTimeSet = new DateTime[items.Length + 1];

                const int Wait = 100;
                var c = 0;
                var key = new Key("row");
                using( var mutator = _table.CreateMutator() ) {
                    foreach( var item in items ) {
                        dateTimeSet[c++] = key.DateTime = DateTime.UtcNow;
                        key.ColumnFamily = "asc";
                        mutator.Set(key, Encoding.GetBytes(item));
                        key.ColumnFamily = "desc";
                        mutator.Set(key, Encoding.GetBytes(item));
                        Thread.Sleep(Wait);
                    }
                }

                dateTimeSet[c] = DateTime.UtcNow;

                // Ascending
                var scanSpec = ScanSpecBuilder
                    .Create()
                        .WithColumns("asc")
                        .WithRows()
                    .Build();

                using( var scanner = _table.CreateScanner(scanSpec) ) {
                    var prevDateTime = DateTime.UtcNow;
                    c = 0;
                    Cell cell;
                    while( scanner.Next(out cell) ) {
                        Assert.IsTrue(cell.Key.DateTime < prevDateTime);
                        prevDateTime = cell.Key.DateTime;
                        ++c;
                    }

                    Assert.AreEqual(items.Length, c);
                }

                scanSpec = ScanSpecBuilder
                    .Create()
                        .WithColumns("asc")
                        .WithRows()
                        .StartDateTime(dateTimeSet[0])
                        .EndDateTime(dateTimeSet[dateTimeSet.Length - 1])
                    .Build();

                using( var scanner = _table.CreateScanner(scanSpec) ) {
                    c = 0;
                    Cell cell;
                    while( scanner.Next(out cell) ) {
                        ++c;
                    }

                    Assert.AreEqual(items.Length, c);
                }

                for( var n = 0; n < items.Length; ++n ) {
                    scanSpec = ScanSpecBuilder
                        .Create()
                            .WithColumns("asc")
                            .WithRows()
                            .StartDateTime(dateTimeSet[n])
                            .EndDateTime(dateTimeSet[n] + TimeSpan.FromMilliseconds(Wait / 2))
                        .Build();

                    using( var scanner = _table.CreateScanner(scanSpec) ) {
                        c = 0;
                        Cell cell;
                        while( scanner.Next(out cell) ) {
                            ++c;
                        }

                        Assert.AreEqual(1, c);
                    }
                }

                // Descending
                scanSpec = ScanSpecBuilder
                    .Create()
                        .WithColumns("desc")
                        .WithRows()
                    .Build();

                using( var scanner = _table.CreateScanner(scanSpec) ) {
                    var prevDateTime = new DateTime(0);
                    c = 0;
                    Cell cell;
                    while( scanner.Next(out cell) ) {
                        Assert.IsTrue(cell.Key.DateTime > prevDateTime);
                        prevDateTime = cell.Key.DateTime;
                        ++c;
                    }

                    Assert.AreEqual(items.Length, c);
                }

                scanSpec = ScanSpecBuilder
                    .Create()
                        .WithColumns("desc")
                        .WithRows()
                        .StartDateTime(dateTimeSet[0])
                        .EndDateTime(dateTimeSet[dateTimeSet.Length - 1])
                    .Build();

                using( var scanner = _table.CreateScanner(scanSpec) ) {
                    c = 0;
                    Cell cell;
                    while( scanner.Next(out cell) ) {
                        ++c;
                    }

                    Assert.AreEqual(items.Length, c);
                }

                for( var n = 0; n < items.Length; ++n ) {
                    scanSpec = ScanSpecBuilder
                        .Create()
                            .WithColumns("desc")
                            .WithRows()
                            .StartDateTime(dateTimeSet[n])
                            .EndDateTime(dateTimeSet[n] + TimeSpan.FromMilliseconds(Wait / 2))
                        .Build();

                    using( var scanner = _table.CreateScanner(scanSpec) ) {
                        c = 0;
                        Cell cell;
                        while( scanner.Next(out cell) ) {
                            ++c;
                        }

                        Assert.AreEqual(1, c);
                    }
                }

                // Mixed
                scanSpec = ScanSpecBuilder
                    .Create()
                        .WithColumns("asc", "desc")
                        .WithRows()
                        .StartDateTime(dateTimeSet[1])
                        .EndDateTime(dateTimeSet[dateTimeSet.Length - 2])
                    .Build();

                using( var scanner = _table.CreateScanner(scanSpec) ) {
                    c = 0;
                    Cell cell;
                    while( scanner.Next(out cell) ) {
                        Assert.IsTrue(cell.Key.DateTime >= scanSpec.StartDateTime);
                        Assert.IsTrue(cell.Key.DateTime < scanSpec.EndDateTime);
                        var value = Encoding.GetString(cell.Value);
                        Assert.IsTrue(value != items[0] && value != items[items.Length - 1]);
                        ++c;
                    }

                    Assert.AreEqual(2 * (items.Length - 2), c);
                }
            }
        }

        [TestMethod]
        public void ScanTableKeyOnly() {
            using (var scanner = table.CreateScanner(new ScanSpec { KeysOnly = true })) {
                var c = 0;
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

            foreach (var row in rows) {
                using (var mutator = table.CreateMutator()) {
                    foreach (var columnFamily in columnFamilies) {
                        foreach (var columnQualifier in columnQualifiers) {
                            mutator.Set(new Key(row, columnFamily, columnQualifier), Encoding.GetBytes(row + columnFamily + columnQualifier));
                        }
                    }
                }
            }

            using (var scanner = table.CreateScanner(new ScanSpec { MaxCells = 5 })) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(5, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { MaxCellsColumnFamily = 2 }.AddRow(rows))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(rows.Length * columnFamilies.Length * 2, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { MaxCellsColumnFamily = 2, ScanAndFilter = true }.AddRow(rows))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(rows.Length * columnFamilies.Length * 2, c);
            }

            foreach (var columnFamily in columnFamilies) {
                using (var scanner = table.CreateScanner(new ScanSpec { MaxCells = 3, ScanAndFilter = true }.AddRow(rows).AddColumn(columnFamily))) {
                    var c = 0;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        ++c;
                    }

                    Assert.AreEqual(3, c);
                }
            }

            using (var scanner = table.CreateScanner(new ScanSpec { MaxCells = 5 }.AddRowInterval(new RowInterval("A", "B"), new RowInterval("C", "D")))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(2 * 5, c); // Applies to each row interval individual
            }

            using (var scanner = table.CreateScanner(new ScanSpec { MaxCellsColumnFamily = 2 }.AddRowInterval(new RowInterval("A", "B"), new RowInterval("C", "D")))) {
                var c = 0;
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
                var c = 0;
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountC, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { MaxRows = CountB }.AddColumn("a"))) {
                var c = 0;
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

            foreach (var row in rows) {
                using (var mutator = table.CreateMutator()) {
                    foreach (var columnFamily in columnFamilies) {
                        foreach (var columnQualifier in columnQualifiers) {
                            mutator.Set(new Key(row, columnFamily, columnQualifier), Encoding.GetBytes(row + columnFamily + columnQualifier));
                        }
                    }
                }
            }

            using (var scanner = table.CreateScanner(new ScanSpec { MaxRows = rows.Length / 2, ScanAndFilter = true }.AddRow(rows))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(rows.Length / 2 * columnFamilies.Length * columnQualifiers.Length, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { MaxRows = rows.Length / 2 }.AddRowInterval(new RowInterval("AA", "BB"), new RowInterval("CC", "DD")))
                ) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(2 * rows.Length / 2 * columnFamilies.Length * columnQualifiers.Length, c); // MaxRows applies to each row interval individual
            }
        }

        [TestMethod]
        public void ScanTableRandomCells() {
            var random = new Random();
            const string Cf = "abcdefg";
            const int Count = 10000;
            using (var _table = EnsureTable("ScanTableRandomCells", Schema)) {
                var keys = new List<Key>(Count);
                using (var mutator = _table.CreateMutator()) {
                    for (var i = 0; i < Count; ++i) {
                        var key = new Key
                            {
                                Row = Guid.NewGuid().ToString(),
                                ColumnFamily = new string(new[] { Cf[random.Next(Cf.Length)] }),
                                ColumnQualifier = random.Next(Cf.Length).ToString(CultureInfo.InvariantCulture)
                            };
                        keys.Add(key);
                        mutator.Set(key, Encoding.GetBytes(key.Row));
                    }
                }

                for (var r = 0; r < 10; ++r) {
                    var countCells = 10 + random.Next(Count - 10);
                    var scanSpec = new ScanSpec();
                    foreach (var k in Shuffle(keys)) {
                        scanSpec.AddCell(k);
                        if (scanSpec.CellCount == countCells) {
                            break;
                        }
                    }

                    var comparer = new KeyComparer(false);
                    using (var scanner = _table.CreateScanner(scanSpec)) {
                        Assert.AreSame(scanSpec, scanner.ScanSpec);
                        var cell = new Cell();
                        var c = 0;
                        while (scanner.Next(cell)) {
                            Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                            Assert.IsTrue(comparer.Equals(scanSpec.Cells[c++], cell.Key));
                        }

                        Assert.AreEqual(scanSpec.CellCount, c);
                    }
                }

                for (var r = 0; r < 10; ++r) {
                    var countCells = 10 + random.Next(Count - 10);
                    var scanSpec = new ScanSpec();
                    foreach (var k in Shuffle(keys)) {
                        scanSpec.AddRow(k.Row);
                        if (scanSpec.RowCount == countCells) {
                            break;
                        }
                    }

                    using (var scanner = _table.CreateScanner(scanSpec)) {
                        Assert.AreSame(scanSpec, scanner.ScanSpec);
                        var cell = new Cell();
                        var c = 0;
                        while (scanner.Next(cell)) {
                            Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                            Assert.AreEqual(scanSpec.Rows[c++], cell.Key.Row);
                        }

                        Assert.AreEqual(scanSpec.RowCount, c);
                    }
                }

                for (var r = 0; r < 10; ++r) {
                    var rows = new HashSet<string>();
                    var countCells = 10 + random.Next(Count - 10);
                    var scanSpec = new ScanSpec(true);
                    foreach (var k in Shuffle(keys)) {
                        scanSpec.AddRow(k.Row);
                        rows.Add(k.Row);
                        if (scanSpec.RowCount == countCells) {
                            break;
                        }
                    }

                    using (var scanner = _table.CreateScanner(scanSpec)) {
                        Assert.AreSame(scanSpec, scanner.ScanSpec);
                        var cell = new Cell();
                        var c = 0;
                        while (scanner.Next(cell)) {
                            Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                            Assert.IsTrue(rows.Contains(cell.Key.Row));
                            ++c;
                        }

                        Assert.AreEqual(scanSpec.RowCount, c);
                    }
                }

                for (var r = 0; r < 10; ++r) {
                    var rows = new HashSet<string>();
                    var columnFamily = new string(new[] { Cf[random.Next(Cf.Length)] });
                    var columnQualifier = random.Next(Cf.Length).ToString(CultureInfo.InvariantCulture);
                    var countCells = 10 + random.Next(Count / 10);
                    var scanSpec = new ScanSpec { ScanAndFilter = true };
                    foreach (var k in Shuffle(keys).Where(k => k.ColumnFamily == columnFamily && k.ColumnQualifier == columnQualifier)) {
                        Assert.AreEqual(columnFamily, k.ColumnFamily);
                        Assert.AreEqual(columnQualifier, k.ColumnQualifier);
                        scanSpec.AddColumn(k.ColumnFamily + ":" + k.ColumnQualifier);
                        scanSpec.AddRow(k.Row);
                        rows.Add(k.Row);
                        if (scanSpec.RowCount == countCells) {
                            break;
                        }
                    }

                    using (var scanner = _table.CreateScanner(scanSpec)) {
                        Assert.AreSame(scanSpec, scanner.ScanSpec);
                        var cell = new Cell();
                        var c = 0;
                        while (scanner.Next(cell)) {
                            Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                            Assert.AreEqual(columnFamily, cell.Key.ColumnFamily);
                            Assert.AreEqual(columnQualifier, cell.Key.ColumnQualifier);
                            Assert.IsTrue(rows.Contains(cell.Key.Row));
                            ++c;
                        }

                        Assert.AreEqual(scanSpec.RowCount, c);
                    }
                }

                for (var r = 0; r < 10; ++r) {
                    var rows = new HashSet<string>();
                    var columnQualifier = random.Next(Cf.Length).ToString(CultureInfo.InvariantCulture);
                    var countCells = 10 + random.Next(Count / 10);
                    var scanSpec = new ScanSpec { ScanAndFilter = true };
                    foreach (var k in Shuffle(keys).Where(k => k.ColumnQualifier == columnQualifier)) {
                        Assert.AreEqual(columnQualifier, k.ColumnQualifier);
                        scanSpec.AddColumn(k.ColumnFamily + ":" + k.ColumnQualifier);
                        scanSpec.AddRow(k.Row);
                        rows.Add(k.Row);
                        if (scanSpec.RowCount == countCells) {
                            break;
                        }
                    }

                    using (var scanner = _table.CreateScanner(scanSpec)) {
                        Assert.AreSame(scanSpec, scanner.ScanSpec);
                        var cell = new Cell();
                        var c = 0;
                        while (scanner.Next(cell)) {
                            Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                            Assert.AreEqual(columnQualifier, cell.Key.ColumnQualifier);
                            Assert.IsTrue(rows.Contains(cell.Key.Row));
                            ++c;
                        }

                        Assert.AreEqual(scanSpec.RowCount, c);
                    }
                }

                using (var mutator = _table.CreateMutator()) {
                    var key = new Key { Row = "A", ColumnFamily = "a", ColumnQualifier = "1" };
                    mutator.Set(key, Encoding.GetBytes(key.Row));

                    key = new Key { Row = "B", ColumnFamily = "a", ColumnQualifier = "2" };
                    mutator.Set(key, Encoding.GetBytes(key.Row));

                    key = new Key { Row = "C", ColumnFamily = "c", ColumnQualifier = "3" };
                    mutator.Set(key, Encoding.GetBytes(key.Row));

                    key = new Key { Row = "D", ColumnFamily = "c", ColumnQualifier = "4" };
                    mutator.Set(key, Encoding.GetBytes(key.Row));

                    key = new Key { Row = "E", ColumnFamily = "b", ColumnQualifier = "5" };
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                }

                {
                    var scanSpec = new ScanSpec { ScanAndFilter = true };
                    scanSpec.AddColumn("a:1");
                    scanSpec.AddColumn("a:2");
                    scanSpec.AddColumn("b:5");
                    scanSpec.AddColumn("c:3");
                    scanSpec.AddColumn("c:4");
                    scanSpec.AddRow("A");
                    scanSpec.AddRow("B");

                    using (var scanner = _table.CreateScanner(scanSpec)) {
                        Assert.AreSame(scanSpec, scanner.ScanSpec);
                        var cell = new Cell();
                        var c = 0;
                        while (scanner.Next(cell)) {
                            Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                            ++c;
                        }

                        Assert.AreEqual(scanSpec.RowCount, c);
                    }

                    scanSpec = new ScanSpec { ScanAndFilter = true };
                    scanSpec.AddColumn("a", "b:5", "c");
                    scanSpec.AddRow("A");
                    scanSpec.AddRow("C");
                    scanSpec.AddRow("E");

                    using (var scanner = _table.CreateScanner(scanSpec)) {
                        Assert.AreSame(scanSpec, scanner.ScanSpec);
                        var cell = new Cell();
                        var c = 0;
                        while (scanner.Next(cell)) {
                            Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                            ++c;
                        }

                        Assert.AreEqual(scanSpec.RowCount, c);
                    }
                }
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
            foreach (var t in rowKeys) {
                var rowKey = rowKeys[random.Next(rowKeys.Count)];
                scanSpec1.AddRow(rowKey);
                scanSpec2.AddCell(rowKey, "a", null);
            }

            Assert.AreEqual(R, scanSpec1.RowCount);
            Assert.AreEqual(R, scanSpec2.Cells.Count);
            var c = 0;
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
            var c = 0;
            foreach (var t in rowKeys) {
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

            var c = 0;
            var previousRow = string.Empty;
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
                for (var i = 0; i < 100; ++i) {
                    key.Row = i.ToString("D2");
                    mutator.Set(key, BitConverter.GetBytes(i));
                }
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new RowInterval(null, "50")).AddColumn("d"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                    ++c;
                }

                Assert.AreEqual(51, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new RowInterval(null, false, "50", false)).AddColumn("d"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                    ++c;
                }

                Assert.AreEqual(50, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new RowInterval(string.Empty, "50")).AddColumn("d"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                    ++c;
                }

                Assert.AreEqual(51, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new RowInterval(string.Empty, false, "50", false)).AddColumn("d"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                    ++c;
                }

                Assert.AreEqual(50, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new RowInterval("90", null)).AddColumn("d"))) {
                var c = 90;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                    ++c;
                }

                Assert.AreEqual(100, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new RowInterval("90", false, null, false)).AddColumn("d"))) {
                var c = 91;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                    ++c;
                }

                Assert.AreEqual(100, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new RowInterval("90", string.Empty)).AddColumn("d"))) {
                var c = 90;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                    ++c;
                }

                Assert.AreEqual(100, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new RowInterval("90", false, string.Empty, false)).AddColumn("d"))) {
                var c = 91;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                    ++c;
                }

                Assert.AreEqual(100, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new RowInterval("10", "50")).AddColumn("d"))) {
                var c = 10;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                    ++c;
                }

                Assert.AreEqual(51, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new RowInterval("10", true, "50", false)).AddColumn("d"))) {
                var c = 10;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                    ++c;
                }

                Assert.AreEqual(50, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new RowInterval("10", false, "50", false)).AddColumn("d"))) {
                var c = 11;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                    ++c;
                }

                Assert.AreEqual(50, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec(new RowInterval("10", "20")).AddRowInterval(new RowInterval("40", "50")).AddColumn("d"))) {
                var c = 10;
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
            var countB = 0;
            var countC = 0;
            using (var scanner = table.CreateScanner(new ScanSpec { KeysOnly = true }.AddColumn("b", "c").AddRowInterval(new RowInterval(rowKeys[rowOffset], null)))) {
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
                var c = 0;
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountA - rowOffset + countB + countC, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { RowOffset = rowOffset })) {
                var ca = 0;
                var cb = 0;
                var cc = 0;
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
                var c = 0;
                var cell = new Cell();
                while (scanner.Next(cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(CountA - rowOffset, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { RowOffset = rowOffset }.AddColumn("a").AddColumn("b"))) {
                var ca = 0;
                var cb = 0;
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

            foreach (var row in rows) {
                using (var mutator = table.CreateMutator()) {
                    foreach (var columnFamily in columnFamilies) {
                        foreach (var columnQualifier in columnQualifiers) {
                            mutator.Set(new Key(row, columnFamily, columnQualifier), Encoding.GetBytes(row + columnFamily + columnQualifier));
                        }
                    }
                }
            }

            using (var scanner = table.CreateScanner(new ScanSpec { RowOffset = rows.Length / 2, ScanAndFilter = true }.AddRow(rows))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(rows.Length / 2 * columnFamilies.Length * columnQualifiers.Length, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { RowOffset = 1 }.AddRowInterval(new RowInterval("AA", "BB"), new RowInterval("CC", "DD")))) {
                var c = 0;
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
                var c = 0;
                foreach (var cell in scanner) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(3, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { RowRegex = "AA." }.AddColumn("d"))) {
                var c = 0;
                foreach (var cell in scanner) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(2, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { RowRegex = "AB.*" }.AddColumn("d"))) {
                var c = 0;
                foreach (var cell in scanner) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(2, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { RowRegex = "^B.*" }.AddColumn("d"))) {
                var c = 0;
                foreach (var cell in scanner) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(1, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { RowRegex = "^BB.*" }.AddColumn("d"))) {
                var c = 0;
                foreach (var cell in scanner) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(1, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { RowRegex = "^B.." }.AddColumn("d"))) {
                var c = 0;
                foreach (var cell in scanner) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(1, c);
            }
        }

        [TestMethod]
        public void ScanTableThreaded() {
            var t1 = new Thread(
                () =>
                    {
                        using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("a"))) {
                            Thread.Sleep(100);

                            var c = 0;
                            Cell cell;
                            while (scanner.Next(out cell)) {
                                Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                                ++c;
                            }

                            Assert.AreEqual(CountA, c);
                        }
                    });

            var t2 = new Thread(
                () =>
                    {
                        using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("b"))) {
                            Thread.Sleep(50);

                            var c = 0;
                            Cell cell;
                            while (scanner.Next(out cell)) {
                                Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                                ++c;
                            }

                            Assert.AreEqual(CountB, c);
                        }
                    });

            var t3 = new Thread(
                () =>
                    {
                        using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("c"))) {
                            Thread.Sleep(25);

                            var c = 0;
                            Cell cell;
                            while (scanner.Next(out cell)) {
                                Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                                ++c;
                            }

                            Assert.AreEqual(CountC, c);
                        }
                    });

            t1.Start();
            t2.Start();
            t3.Start();
            t1.Join();
            t2.Join();
            t3.Join();

            using( var _table = EnsureTable("ScanTableThreaded", Schema) ) {
                var action = new Action<string>(
                    cf => {
                        using( var t = Ns.OpenTable("ScanTableThreaded") ) {
                            var key = new Key { ColumnFamily = cf };
                            using (var mutator = t.CreateMutator()) {
                                for (var i = 0; i < 10000; ++i) {
                                    key.Row = i.ToString("D6");
                                    mutator.Set(key, BitConverter.GetBytes(i));
                                }
                            }

                            using (var scanner = t.CreateScanner(new ScanSpec().AddColumn(cf))) {
                                var c = 0;
                                Cell cell;
                                while (scanner.Next(out cell)) {
                                    Assert.AreEqual(c.ToString("D6"), cell.Key.Row);
                                    Assert.AreEqual(c, BitConverter.ToInt32(cell.Value, 0));
                                    ++c;
                                }

                                Assert.AreEqual(10000, c);
                            }
                        }
                    });

                t1 = new Thread(() => action("a"));
                t2 = new Thread(() => action("b"));
                t3 = new Thread(() => action("c"));
                t1.Start();
                t2.Start();
                t3.Start();
                t1.Join();
                t2.Join();
                t3.Join();
            }
        }

        [TestMethod]
        public void ScanTableValueIndex() {
            const string ScanTableValueIndexSchema =
                "<Schema><AccessGroup name=\"default\" blksz=\"1024\">" +
                "<ColumnFamily><Name>a</Name><Index>true</Index></ColumnFamily>" +
                "<ColumnFamily><Name>b</Name><Index>true</Index></ColumnFamily>" +
                "<ColumnFamily><Name>c</Name><Index>true</Index></ColumnFamily>" +
                "</AccessGroup></Schema>";

            const int Count = 10000;
            using (var _table = EnsureTable("ScanTableValueIndex", ScanTableValueIndexSchema)) {
                if (IsHyper || IsThrift) {
                    Assert.IsTrue(Ns.TableExists("^ScanTableValueIndex"));
                }

                var key = new Key();
                using (var mutator = _table.CreateMutator()) {
                    for (var i = 0; i < Count; ++i) {
                        var value = Encoding.GetBytes(i.ToString());

                        key.ColumnFamily = "a";
                        key.Row = null;
                        mutator.Set(key, value);

                        key.ColumnFamily = "b";
                        key.Row = null;
                        mutator.Set(key, value);

                        key.ColumnFamily = "c";
                        key.Row = null;
                        mutator.Set(key, value);
                    }
                }

                string[] columnFamilies = { "a", "b", "c" };
                var rng = new Random();
                for (var i = 0; i < Count / 100; ++i) {
                    var search = rng.Next(Count).ToString();
                    var scanSpec = new ScanSpec(new ColumnPredicate(columnFamilies[rng.Next(columnFamilies.Length)], MatchKind.Exact, Encoding.GetBytes(search)));
                    using (var scanner = _table.CreateScanner(scanSpec)) {
                        Cell cell;
                        Assert.IsTrue(scanner.Next(out cell));
                        Assert.AreEqual(search, Encoding.GetString(cell.Value));
                        Assert.IsFalse(scanner.Next(out cell));
                    }
                }

                int[] ranges = { 1000, 300, 56, 9999, 789, 4 };
                int[] occurrence = { 1, 11, 111, 1, 11, 1111 };
                Assert.AreEqual(ranges.Length, occurrence.Length);
                for (var i = 0; i < ranges.Length; ++i) {
                    var search = ranges[i].ToString();
                    var scanSpec = new ScanSpec(new ColumnPredicate(columnFamilies[rng.Next(columnFamilies.Length)], MatchKind.Prefix, Encoding.GetBytes(search)));
                    using (var scanner = _table.CreateScanner(scanSpec)) {
                        var c = 0;
                        var cell = new Cell();
                        while (scanner.Next(cell)) {
                            Assert.IsTrue(Encoding.GetString(cell.Value).StartsWith(search));
                            ++c;
                        }

                        Assert.AreEqual(occurrence[i], c, "range = {0}", ranges[i]);
                    }
                }
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
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(3, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { ValueRegex = "XX." }.AddColumn("d"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(2, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { ValueRegex = "XY.*" }.AddColumn("d"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(2, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { ValueRegex = "^Y.*" }.AddColumn("d"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(1, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { ValueRegex = "^YY.*" }.AddColumn("d"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(1, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { ValueRegex = "^Y.." }.AddColumn("d"))) {
                var c = 0;
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

            foreach (var row in rows) {
                var key = new Key { Row = row, ColumnFamily = "d" };
                using (var mutator = table.CreateMutator()) {
                    foreach (var version in versions) {
                        mutator.Set(key, Encoding.GetBytes(version));
                    }
                }
            }

            using (var scanner = table.CreateScanner(new ScanSpec().AddColumn("d"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(rows[c / versions.Length], cell.Key.Row);
                    Assert.AreEqual(versions[versions.Length - (c % versions.Length) - 1], Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(rows.Length * versions.Length, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { MaxVersions = 1 }.AddColumn("d"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(rows[c], cell.Key.Row);
                    Assert.AreEqual(versions[versions.Length - 1], Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(rows.Length, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { MaxVersions = 2 }.AddColumn("d"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    Assert.AreEqual(rows[c / 2], cell.Key.Row);
                    Assert.AreEqual(versions[versions.Length - (c % 2) - 1], Encoding.GetString(cell.Value));
                    ++c;
                }

                Assert.AreEqual(rows.Length * 2, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { MaxVersions = 1 }.AddColumn("d").AddRow("A", "B"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(2, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { ScanAndFilter = true, MaxVersions = 1 }.AddColumn("d").AddRow("A", "B"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(2, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { MaxVersions = 2 }.AddColumn("d").AddRow("B", "C"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(4, c);
            }

            using (var scanner = table.CreateScanner(new ScanSpec { ScanAndFilter = true, MaxVersions = 2 }.AddColumn("d").AddRow("B", "C"))) {
                var c = 0;
                Cell cell;
                while (scanner.Next(out cell)) {
                    ++c;
                }

                Assert.AreEqual(4, c);
            }
        }

        [TestMethod]
        public void ScanTableWithSpecialCharacters() {
            var specialCharacters = new[]
                {
                    '~',
                    '!',
                    '@',
                    '#',
                    '$',
                    '%',
                    '^',
                    '&',
                    '*',
                    '(',
                    ')',
                    '{',
                    '}',
                    '\\',
                    '"',
                    '\'',
                    ',',
                    ';',
                    ':',
                    '.',
                    '/',
                    '?',
                    'Ü',
                    'Ä',
                    'Ö'
                };

            var escapeCharacters = new[]
                {
                    '"',
                    '\'',
                    '\\',
                    '/'
                };

            var rows = new List<string>();
            var key = new Key { ColumnFamily = "d" };
            using( var mutator = table.CreateMutator() ) {
                foreach( var c in specialCharacters ) {
                    key.Row = c.ToString();
                    mutator.Set(key, Encoding.GetBytes(key.Row));
                    rows.Add(key.Row);

                    foreach( var e in escapeCharacters ) {
                        key.Row = e.ToString() + c.ToString();
                        mutator.Set(key, Encoding.GetBytes(key.Row));
                        rows.Add(key.Row);

                        key.Row = e.ToString() + c.ToString() + e.ToString();
                        mutator.Set(key, Encoding.GetBytes(key.Row));
                        rows.Add(key.Row);
                    }
                }
            }

            using( var scanner = table.CreateScanner(new ScanSpec().AddColumn("d")) ) {
                var c = 0;
                Cell cell;
                while( scanner.Next(out cell) ) {
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    ++c;

                    Assert.IsTrue(rows.Contains(cell.Key.Row));
                }

                Assert.AreEqual(rows.Count, c);
            }

            foreach( var row in Shuffle(rows) ) {
                using( var scanner = table.CreateScanner(new ScanSpec(row).AddColumn("d")) ) {
                    Cell cell;
                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    Assert.IsFalse(scanner.Next(out cell));
                }
            }

            foreach( var row in Shuffle(rows) ) {
                using( var scanner = table.CreateScanner(new ScanSpec(row) { ScanAndFilter = true }.AddColumn("d")) ) {
                    Cell cell;
                    Assert.IsTrue(scanner.Next(out cell));
                    Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                    Assert.IsFalse(scanner.Next(out cell));
                }
            }

            for( int i = 0; i < 10; ++i ) {
                var rowsScanAndFilter = new HashSet<string>();
                foreach (var row in Shuffle(rows)) {
                    rowsScanAndFilter.Add(row);
                    if (rowsScanAndFilter.Count >= 3) {
                        break;
                    }
                }

                using (var scanner = table.CreateScanner(new ScanSpec { ScanAndFilter = true }.AddColumn("d").AddRow(rowsScanAndFilter))) {
                    var c = 0;
                    Cell cell;
                    while (scanner.Next(out cell)) {
                        Assert.AreEqual(cell.Key.Row, Encoding.GetString(cell.Value));
                        ++c;

                        Assert.IsTrue(rowsScanAndFilter.Contains(cell.Key.Row));
                    }

                    Assert.AreEqual(rowsScanAndFilter.Count, c);
                }
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

            var c = 0;
            var previousRow = string.Empty;
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