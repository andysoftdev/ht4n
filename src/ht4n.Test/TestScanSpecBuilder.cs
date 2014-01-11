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

namespace Hypertable.Test
{
    using System;
    using System.Linq;
    using System.Text;

    using Hypertable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test the scan spec builder.
    /// </summary>
    [TestClass]
    public class TestScanSpecBuilder
    {
        #region Public Methods

        [TestMethod]
        public void TestWithColumns() {
            var scanSpec = ScanSpecBuilder
                .Create()
                    .WithColumns()
                    .WithRows()
                .Build();

            Assert.IsNotNull(scanSpec);
            Assert.AreEqual(0, scanSpec.ColumnCount);

            scanSpec = ScanSpecBuilder
                .CreateOrdered()
                    .WithColumns()
                    .WithRows()
                .Build();

            Assert.IsNotNull(scanSpec);
            Assert.AreEqual(0, scanSpec.ColumnCount);

            scanSpec = ScanSpecBuilder
                .Create()
                    .WithColumns("a")
                    .WithRows()
               .Build();

            Assert.IsNotNull(scanSpec);
            Assert.AreEqual(1, scanSpec.ColumnCount);
            Assert.IsTrue(scanSpec.Columns.SequenceEqual(new[] { "a" }));

            scanSpec = ScanSpecBuilder
                .Create()
                    .WithColumns("x", "a", "z")
                    .WithRows()
               .Build();

            Assert.IsNotNull(scanSpec);
            Assert.AreEqual(3, scanSpec.ColumnCount);
            Assert.IsTrue(scanSpec.Columns.SequenceEqual(new[] { "a", "x", "z" }));

            scanSpec = ScanSpecBuilder
                .CreateOrdered()
                    .WithColumns(new[] { "x", "a", "z" })
                    .WithRows()
               .Build();

            Assert.IsNotNull(scanSpec);
            Assert.AreEqual(3, scanSpec.ColumnCount);
            Assert.IsTrue(scanSpec.Columns.SequenceEqual(new[] { "a", "x", "z" }));
        }

        [TestMethod]
        public void TestWithRows() {
            var scanSpec = ScanSpecBuilder
                .Create()
                    .WithColumns()
                    .WithRows("a")
                .Build();

            Assert.IsNotNull(scanSpec);
            Assert.AreEqual(1, scanSpec.RowCount);
            Assert.IsTrue(scanSpec.Rows.SequenceEqual(new[] { "a" }));

            scanSpec = ScanSpecBuilder
                .Create()
                    .WithColumns()
                    .WithRows("x", "a")
                    .WithRows("b", "a", "z")
               .Build();

            Assert.IsNotNull(scanSpec);
            Assert.AreEqual(5, scanSpec.RowCount);
            Assert.IsTrue(scanSpec.Rows.SequenceEqual(new[] { "x", "a", "b", "a", "z" }));

            scanSpec = ScanSpecBuilder
                .Create()
                    .WithColumns()
                    .WithRows("x", "a")
                    .WithRows(new[] { "b", "a", "z"})
               .Build();

            Assert.IsNotNull(scanSpec);
            Assert.AreEqual(5, scanSpec.RowCount);
            Assert.IsTrue(scanSpec.Rows.SequenceEqual(new[] { "x", "a", "b", "a", "z" }));

            scanSpec = ScanSpecBuilder
                .CreateOrdered()
                    .WithColumns()
                    .WithRows("x", "a")
                    .WithRows("b", "a", "z")
               .Build();

            Assert.IsNotNull(scanSpec);
            Assert.AreEqual(4, scanSpec.RowCount);
            Assert.IsTrue(scanSpec.Rows.SequenceEqual(new[] { "a", "b", "x", "z" }));

            scanSpec = ScanSpecBuilder
                .CreateOrdered()
                    .WithColumns()
                    .WithRows("x", "a")
                    .WithRows(new[] { "b", "a", "z" })
               .Build();

            Assert.IsNotNull(scanSpec);
            Assert.AreEqual(4, scanSpec.RowCount);
            Assert.IsTrue(scanSpec.Rows.SequenceEqual(new[] { "a", "b", "x", "z" }));
        }

        [TestMethod]
        public void TestWithColumnPredicates() {
            var cp1 = new ColumnPredicate("x", MatchKind.Exact, Encoding.UTF8.GetBytes("xyz"));
            var cp2 = new ColumnPredicate("a", MatchKind.Exact, Encoding.UTF8.GetBytes("xyz"));

            Assert.IsTrue(cp2 < cp1);

            var scanSpec = ScanSpecBuilder
                .Create()
                    .WithColumns()
                    .WithColumnPredicates(cp1)
                .Build();

            Assert.IsNotNull(scanSpec);
            Assert.AreEqual(1, scanSpec.ColumnPredicateCount);
            Assert.IsTrue(scanSpec.ColumnPredicates.SequenceEqual(new[] { cp1 }));

            scanSpec = ScanSpecBuilder
                .Create()
                    .WithColumns()
                    .WithColumnPredicates(cp1, cp2)
               .Build();

            Assert.IsNotNull(scanSpec);
            Assert.AreEqual(2, scanSpec.ColumnPredicateCount);
            Assert.IsTrue(scanSpec.ColumnPredicates.SequenceEqual(new[] { cp1, cp2 }));

            scanSpec = ScanSpecBuilder
                .CreateOrdered()
                    .WithColumns()
                    .WithColumnPredicates(cp1, cp2)
                    .WithColumnPredicates(new ColumnPredicate("a", MatchKind.Exact, Encoding.UTF8.GetBytes("xyz")), cp1)
               .Build();

            Assert.IsNotNull(scanSpec);
            Assert.AreEqual(2, scanSpec.ColumnPredicateCount);
            Assert.IsTrue(scanSpec.ColumnPredicates.SequenceEqual(new[] { cp1, cp2 }));
        }

        [TestMethod]
        public void TestWithCells() {
            var k1 = new Key("b", "cf");
            var k2 = new Key("b", "cf", "2");

            Assert.IsTrue(k1 < k2);

            var scanSpec = ScanSpecBuilder
                .Create()
                    .WithColumns()
                    .WithCells("x", "cf", "cq")
                    .WithCells(k2, k1)
                    .WithCells(new[] { k1, k2 })
                .Build();

            Assert.IsNotNull(scanSpec);
            Assert.AreEqual(5, scanSpec.CellCount);
            Assert.IsTrue(scanSpec.Cells.SequenceEqual(new[] { new Key("x", "cf", "cq"), k2, k1, k1, k2 }));

            scanSpec = ScanSpecBuilder
                .CreateOrdered()
                    .WithColumns()
                    .WithCells("x", "cf", "cq")
                    .WithCells(k2, k1)
                    .WithCells(new[] { k1, k2 })
                .Build();

            Assert.IsNotNull(scanSpec);
            Assert.AreEqual(3, scanSpec.CellCount);
            Assert.IsTrue(scanSpec.Cells.SequenceEqual(new[] { k1, k2, new Key("x", "cf", "cq") }));
        }

        [TestMethod]
        public void TestOp() {
            var scanSpec = ScanSpecBuilder
                .Create()
                    .WithColumns()
                    .WithRows("a")
                    .StartDateTime(new DateTime(2011, 3, 10, 13, 45, 12, DateTimeKind.Local))
                    .EndDateTime(new DateTime(2012, 4, 11, 17, 5, 52, DateTimeKind.Utc))
                .Build();

            Assert.IsNotNull(scanSpec);
            Assert.AreEqual(new DateTime(2011, 3, 10, 13, 45, 12, DateTimeKind.Local).ToUniversalTime(), scanSpec.StartDateTime);
            Assert.AreEqual(new DateTime(2012, 4, 11, 17, 5, 52, DateTimeKind.Utc), scanSpec.EndDateTime);
            Assert.IsFalse(scanSpec.ScanAndFilter);
            Assert.IsFalse(scanSpec.KeysOnly);

            scanSpec = ScanSpecBuilder
                .Create()
                    .WithColumns()
                    .WithRows("a")
                    .RowOffset(3)
                    .CellOffset(7)
                    .MaxCells(11)
                    .MaxCellsPerColumnFamily(13)
                    .MaxRows(17)
                    .MaxVersions(23)
                    .ScanAndFilter()
                .Build();

            Assert.IsNotNull(scanSpec);
            Assert.AreEqual(3, scanSpec.RowOffset);
            Assert.AreEqual(7, scanSpec.CellOffset);
            Assert.AreEqual(11, scanSpec.MaxCells);
            Assert.AreEqual(13, scanSpec.MaxCellsColumnFamily);
            Assert.AreEqual(17, scanSpec.MaxRows);
            Assert.AreEqual(23, scanSpec.MaxVersions);
            Assert.IsTrue(scanSpec.ScanAndFilter);
            Assert.IsFalse(scanSpec.KeysOnly);

            scanSpec = ScanSpecBuilder
                .Create()
                    .WithColumns()
                    .WithRows("a")
                    .Timeout(TimeSpan.FromMilliseconds(12345))
                    .Flags(ScannerFlags.BypassTableCache)
                    .KeysOnly()
                .Build();

            Assert.IsNotNull(scanSpec);
            Assert.AreEqual(TimeSpan.FromMilliseconds(12345), scanSpec.Timeout);
            Assert.AreEqual(ScannerFlags.BypassTableCache, scanSpec.Flags);
            Assert.IsFalse(scanSpec.ScanAndFilter);
            Assert.IsTrue(scanSpec.KeysOnly);
        }

        #endregion
    }
}