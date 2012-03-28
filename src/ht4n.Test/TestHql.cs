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
    using System.Text;

    using Hypertable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test HQL commands and queries.
    /// </summary>
    [TestClass]
    public class TestHql : TestBase
    {
        #region Public Methods

        [ClassInitialize]
        public static void ClassInitialize(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext) {
            if (!HasHQL) {
                Assert.IsFalse(IsHyper);
                Assert.IsFalse(IsThrift);
            }
        }

        [TestMethod]
        public void Exec() {
            if (!HasHQL) {
                return;
            }

            try {
                Ns.Exec("this is not hql");
                Assert.Fail();
            }
            catch (HqlParseException) {
            }
            catch {
                Assert.Fail();
            }

            try {
                Ns.Exec("DROP TABLE table_does_not_exists");
                Assert.Fail();
            }
            catch (TableNotFoundException) {
            }
            catch {
                Assert.Fail();
            }

            Ns.Exec("DROP TABLE IF EXISTS table_does_not_exists");

            try {
                Ns.Exec("DROP NAMESPACE namespace_does_not_exists");
                Assert.Fail();
            }
            catch (NamespaceDoesNotExistsException) {
            }
            catch {
                Assert.Fail();
            }

            Ns.Exec("DROP NAMESPACE IF EXISTS namespace_does_not_exists");

            Ns.Exec("CREATE NAMESPACE abc");
            try {
                Ns.Exec("USE abc");
                Assert.Fail();
            }
            catch (BadNamespaceException) {
            }
            catch {
                Assert.Fail();
            }

            Ns.Exec("DROP NAMESPACE abc");
            Ns.Exec("CREATE TABLE t (a, description)");
            Ns.Exec("ALTER TABLE t ADD (e) RENAME COLUMN FAMILY (a, tag)");
            Ns.Exec("RENAME TABLE t TO fruit");
            Ns.Exec(
                "INSERT INTO fruit VALUES" + "(\"cantelope\", \"tag:good\", \"Had with breakfast\"),"
                + "(\"2009-08-02 08:30:00\", \"cantelope\", \"description\", \"A cultivated variety of muskmelon with orange flesh\"),"
                + "(\"banana\", \"tag:great\", \"Had with lunch\")");
            Ns.Exec("DELETE * FROM fruit WHERE ROW=\"banana\";DROP TABLE fruit"); // multiple commands
        }

        [TestMethod]
        public void Query() {
            if (!HasHQL) {
                return;
            }

            Ns.Exec("CREATE TABLE fruit (tag, description)");
            Ns.Exec(
                "INSERT INTO fruit VALUES" + "(\"cantelope\", \"tag:good\", \"Had with breakfast\"),"
                + "(\"2009-08-02 08:30:00\", \"cantelope\", \"description\", \"A cultivated variety of muskmelon with orange flesh\"),"
                + "(\"banana\", \"tag:great\", \"Had with lunch\")");

            var cells = Ns.Query("SELECT * FROM fruit");
            Assert.IsNotNull(cells);
            Assert.AreEqual(3, cells.Count);
            Assert.AreEqual(cells[0].Key.Row, "banana");
            Assert.AreEqual(cells[0].Key.ColumnFamily, "tag");
            Assert.AreEqual(cells[0].Key.ColumnQualifier, "great");
            Assert.AreEqual(Encoding.Default.GetString(cells[0].Value), "Had with lunch");
            Assert.AreEqual(cells[1].Key.Row, "cantelope");
            Assert.AreEqual(cells[1].Key.ColumnFamily, "tag");
            Assert.AreEqual(cells[1].Key.ColumnQualifier, "good");
            Assert.AreEqual(Encoding.Default.GetString(cells[1].Value), "Had with breakfast");
            Assert.AreEqual(cells[2].Key.Row, "cantelope");
            Assert.AreEqual(cells[2].Key.ColumnFamily, "description");
            Assert.AreEqual(cells[2].Key.ColumnQualifier, string.Empty);
            Assert.AreEqual(cells[2].Key.DateTime, new DateTime(2009, 8, 2, 8, 30, 0, DateTimeKind.Utc));
            Assert.AreEqual(Encoding.Default.GetString(cells[2].Value), "A cultivated variety of muskmelon with orange flesh");

            cells = Ns.Query("SELECT tag FROM fruit");
            Assert.IsNotNull(cells);
            Assert.AreEqual(2, cells.Count);
            Assert.AreEqual(cells[0].Key.Row, "banana");
            Assert.AreEqual(cells[0].Key.ColumnFamily, "tag");
            Assert.AreEqual(cells[0].Key.ColumnQualifier, "great");
            Assert.AreEqual(Encoding.Default.GetString(cells[0].Value), "Had with lunch");
            Assert.AreEqual(cells[1].Key.Row, "cantelope");
            Assert.AreEqual(cells[1].Key.ColumnFamily, "tag");
            Assert.AreEqual(cells[1].Key.ColumnQualifier, "good");
            Assert.AreEqual(Encoding.Default.GetString(cells[1].Value), "Had with breakfast");

            cells = Ns.Query("SELECT * FROM fruit WHERE ROW=\"cantelope\"");
            Assert.IsNotNull(cells);
            Assert.AreEqual(2, cells.Count);
            Assert.AreEqual(cells[0].Key.Row, "cantelope");
            Assert.AreEqual(cells[0].Key.ColumnFamily, "tag");
            Assert.AreEqual(cells[0].Key.ColumnQualifier, "good");
            Assert.AreEqual(Encoding.Default.GetString(cells[0].Value), "Had with breakfast");
            Assert.AreEqual(cells[1].Key.Row, "cantelope");
            Assert.AreEqual(cells[1].Key.ColumnFamily, "description");
            Assert.AreEqual(cells[1].Key.ColumnQualifier, string.Empty);
            Assert.AreEqual(cells[1].Key.DateTime, new DateTime(2009, 8, 2, 8, 30, 0, DateTimeKind.Utc));
            Assert.AreEqual(Encoding.Default.GetString(cells[1].Value), "A cultivated variety of muskmelon with orange flesh");

            Ns.Exec("DELETE description FROM fruit WHERE ROW=\"cantelope\"");

            cells = Ns.Query("SELECT * FROM fruit;SELECT * FROM fruit WHERE ROW=\"banana\""); // multiple queries
            Assert.IsNotNull(cells);
            Assert.AreEqual(3, cells.Count);
            Assert.AreEqual(cells[0].Key.Row, "banana");
            Assert.AreEqual(cells[0].Key.ColumnFamily, "tag");
            Assert.AreEqual(cells[0].Key.ColumnQualifier, "great");
            Assert.AreEqual(Encoding.Default.GetString(cells[0].Value), "Had with lunch");
            Assert.AreEqual(cells[1].Key.Row, "cantelope");
            Assert.AreEqual(cells[1].Key.ColumnFamily, "tag");
            Assert.AreEqual(cells[1].Key.ColumnQualifier, "good");
            Assert.AreEqual(Encoding.Default.GetString(cells[1].Value), "Had with breakfast");
            Assert.AreEqual(cells[2].Key.Row, "banana");
            Assert.AreEqual(cells[2].Key.ColumnFamily, "tag");
            Assert.AreEqual(cells[2].Key.ColumnQualifier, "great");
            Assert.AreEqual(Encoding.Default.GetString(cells[0].Value), "Had with lunch");

            Ns.Exec("DROP TABLE fruit");
        }

        [TestMethod]
        public void Revisions() {
            if (!HasHQL) {
                return;
            }

            Ns.Exec("CREATE TABLE fruit (tag, description)");
            Ns.Exec("INSERT INTO fruit VALUES" + "(\"2009-08-02 08:30:00\", \"banana\", \"tag:great\", \"Had with lunch\")");

            var cells = Ns.Query("SELECT * FROM fruit");
            Assert.IsNotNull(cells);
            Assert.AreEqual(1, cells.Count);
            Assert.AreEqual(cells[0].Key.Row, "banana");
            Assert.AreEqual(cells[0].Key.ColumnFamily, "tag");
            Assert.AreEqual(cells[0].Key.ColumnQualifier, "great");
            Assert.AreEqual(Encoding.Default.GetString(cells[0].Value), "Had with lunch");
            Assert.AreEqual(cells[0].Key.DateTime, new DateTime(2009, 8, 2, 8, 30, 0, DateTimeKind.Utc));

            Ns.Exec("INSERT INTO fruit VALUES" + "(\"2009-08-02 08:30:01\", \"banana\", \"tag:great\", \"Had with dinner\")");

            cells = Ns.Query("SELECT * FROM fruit");
            Assert.IsNotNull(cells);
            Assert.AreEqual(2, cells.Count);
            Assert.AreEqual(cells[0].Key.Row, "banana");
            Assert.AreEqual(cells[0].Key.ColumnFamily, "tag");
            Assert.AreEqual(cells[0].Key.ColumnQualifier, "great");
            Assert.AreEqual(Encoding.Default.GetString(cells[0].Value), "Had with dinner");
            Assert.AreEqual(cells[0].Key.DateTime, new DateTime(2009, 8, 2, 8, 30, 1, DateTimeKind.Utc));

            Assert.AreEqual(cells[1].Key.Row, "banana");
            Assert.AreEqual(cells[1].Key.ColumnFamily, "tag");
            Assert.AreEqual(cells[1].Key.ColumnQualifier, "great");
            Assert.AreEqual(Encoding.Default.GetString(cells[1].Value), "Had with lunch");
            Assert.AreEqual(cells[1].Key.DateTime, new DateTime(2009, 8, 2, 8, 30, 0, DateTimeKind.Utc));

            Ns.Exec("INSERT INTO fruit VALUES" + "(\"2009-08-02 08:30:01\", \"banana\", \"tag:great\", \"Had with breakfast\")");

            cells = Ns.Query("SELECT * FROM fruit");
            Assert.IsNotNull(cells);
            Assert.AreEqual(2, cells.Count);
            Assert.AreEqual(cells[0].Key.Row, "banana");
            Assert.AreEqual(cells[0].Key.ColumnFamily, "tag");
            Assert.AreEqual(cells[0].Key.ColumnQualifier, "great");
            Assert.AreEqual(Encoding.Default.GetString(cells[0].Value), "Had with breakfast");
            Assert.AreEqual(cells[0].Key.DateTime, new DateTime(2009, 8, 2, 8, 30, 1, DateTimeKind.Utc));

            Assert.AreEqual(cells[1].Key.Row, "banana");
            Assert.AreEqual(cells[1].Key.ColumnFamily, "tag");
            Assert.AreEqual(cells[1].Key.ColumnQualifier, "great");
            Assert.AreEqual(Encoding.Default.GetString(cells[1].Value), "Had with lunch");
            Assert.AreEqual(cells[1].Key.DateTime, new DateTime(2009, 8, 2, 8, 30, 0, DateTimeKind.Utc));

            Ns.Exec("DROP TABLE fruit");
        }

        [TestInitialize]
        public void TestInitialize() {
            Ns.DropTables();
            Ns.DropNamespaces(DropDispositions.Complete);
        }

        [TestMethod]
        public void Unsupported() {
            if (HasHQL) {
                return;
            }

            try {
                Ns.Exec("CREATE NAMESPACE abc");
                Assert.Fail();
            }
            catch (NotImplementedException) {
            }
        }

        #endregion
    }
}