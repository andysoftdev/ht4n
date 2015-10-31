/** -*- C# -*-
 * Copyright (C) 2010-2015 Thalmann Software & Consulting, http://www.softdev.ch
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
    using System.IO;
    using System.Linq;
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
        public void DumpAndLoadData()
        {
            if (!HasHQL)
            {
                return;
            }

            var filenames = new[] { "DumpTest.txt", "DumpTest.gz", "fs://DumpTest.txt", "fs://DumpTest.gz" };

            {
                Ns.Exec("DROP TABLE IF EXISTS fruit;CREATE TABLE fruit (tag, description)");

                Ns.Exec(
                    "INSERT INTO fruit VALUES" + "(\"cantelope\", \"tag:good\", \"Had with breakfast\"),"
                    + "(\"2009-08-02 08:30:00\", \"cantelope\", \"description\", \"A cultivated variety of muskmelon with orange flesh\"),"
                    + "(\"banana\", \"tag:great\", \"Had with lunch\")");

                ValidateFruitTable("fruit");

                foreach (var filename in filenames)
                {
                    Ns.Exec(string.Format("DUMP TABLE fruit INTO FILE '{0}'", filename));
                    Assert.IsTrue(filename.StartsWith("fs:") || File.Exists(filename));

                    Ns.Exec("DROP TABLE IF EXISTS fruit2;CREATE TABLE fruit2 (tag, description)");
                    Ns.Exec(string.Format("LOAD DATA INFILE '{0}' INTO TABLE fruit2", filename));
                    ValidateFruitTable("fruit2");

                    if (!filename.StartsWith("fs:"))
                    {
                        File.Delete(filename);
                    }
                }

                foreach (var filename in filenames)
                {
                    Ns.Exec(string.Format("SELECT * FROM fruit DISPLAY_TIMESTAMPS INTO FILE '{0}'", filename));
                    Assert.IsTrue(filename.StartsWith("fs:") || File.Exists(filename));

                    Ns.Exec("DROP TABLE IF EXISTS fruit2;CREATE TABLE fruit2 (tag, description)");
                    Ns.Exec(string.Format("LOAD DATA INFILE '{0}' INTO TABLE fruit2", filename));
                    ValidateFruitTable("fruit2");

                    if (!filename.StartsWith("fs:"))
                    {
                        File.Delete(filename);
                    }
                }

                Ns.Exec("DROP TABLE fruit;DROP TABLE fruit2");
            }

            {
                const string Schema = "<Schema><AccessGroup><ColumnFamily><Name>bin</Name></ColumnFamily></AccessGroup></Schema>";

                var rng = new Random();
                var buf = new byte[1024];
                var all = new byte[255];
                for (var i = 0; i < 255; ++i)
                {
                    all[i] = (byte)i;
                }

                var data = new Dictionary<string, byte[]>();

                var table = Ns.OpenTable("bin", Schema, OpenDispositions.CreateAlways);
                using (var mutator = table.CreateMutator())
                {
                    var key = new Key { ColumnFamily = "bin" };
                    for(var i = 0; i < 10000; ++i)
                    {
                        do
                        {
                            key.Row = Guid.NewGuid().ToString();
                        }
                        while (data.ContainsKey(key.Row));
                        
                        rng.NextBytes(buf);
                        mutator.Set(key, buf);

                        data.Add(key.Row, (byte[])buf.Clone());
                    }

                    do
                    {
                        key.Row = Guid.NewGuid().ToString();
                    }
                    while (data.ContainsKey(key.Row));

                    mutator.Set(key, all);
                    data.Add(key.Row, (byte[])all.Clone());
                }

                foreach (var filename in filenames)
                {
                    Ns.Exec(string.Format("DUMP TABLE bin INTO FILE '{0}'", filename));
                    Assert.IsTrue(filename.StartsWith("fs:") || File.Exists(filename));

                    Ns.Exec("DROP TABLE IF EXISTS bin2;CREATE TABLE bin2 (bin)");
                    Ns.Exec(string.Format("LOAD DATA INFILE '{0}' INTO TABLE bin2", filename));

                    foreach (var cell in table.CreateScanner())
                    {
                        Assert.IsTrue(data.ContainsKey(cell.Key.Row));
                        Assert.IsTrue(data[cell.Key.Row].SequenceEqual(cell.Value));
                    }

                    if (!filename.StartsWith("fs:"))
                    {
                        File.Delete(filename);
                    }
                }

                Ns.Exec("DROP TABLE bin;DROP TABLE bin2");
            }
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

            ValidateFruitTable("fruit");

            Ns.Exec("DELETE description FROM fruit WHERE ROW=\"cantelope\"");

            var cells = Ns.Query("SELECT * FROM fruit;SELECT * FROM fruit WHERE ROW=\"banana\""); // multiple queries
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
            Assert.AreEqual(cells[0].Key.DateTime, new DateTime(2009, 8, 2, 8, 30, 0, DateTimeKind.Local).ToUniversalTime());

            Ns.Exec("INSERT INTO fruit VALUES" + "(\"2009-08-02 08:30:01\", \"banana\", \"tag:great\", \"Had with dinner\")");

            cells = Ns.Query("SELECT * FROM fruit");
            Assert.IsNotNull(cells);
            Assert.AreEqual(2, cells.Count);
            Assert.AreEqual(cells[0].Key.Row, "banana");
            Assert.AreEqual(cells[0].Key.ColumnFamily, "tag");
            Assert.AreEqual(cells[0].Key.ColumnQualifier, "great");
            Assert.AreEqual(Encoding.Default.GetString(cells[0].Value), "Had with dinner");
            Assert.AreEqual(cells[0].Key.DateTime, new DateTime(2009, 8, 2, 8, 30, 1, DateTimeKind.Local).ToUniversalTime());

            Assert.AreEqual(cells[1].Key.Row, "banana");
            Assert.AreEqual(cells[1].Key.ColumnFamily, "tag");
            Assert.AreEqual(cells[1].Key.ColumnQualifier, "great");
            Assert.AreEqual(Encoding.Default.GetString(cells[1].Value), "Had with lunch");
            Assert.AreEqual(cells[1].Key.DateTime, new DateTime(2009, 8, 2, 8, 30, 0, DateTimeKind.Local).ToUniversalTime());

            Ns.Exec("INSERT INTO fruit VALUES" + "(\"2009-08-02 08:30:01\", \"banana\", \"tag:great\", \"Had with breakfast\")");

            cells = Ns.Query("SELECT * FROM fruit");
            Assert.IsNotNull(cells);
            Assert.AreEqual(2, cells.Count);
            Assert.AreEqual(cells[0].Key.Row, "banana");
            Assert.AreEqual(cells[0].Key.ColumnFamily, "tag");
            Assert.AreEqual(cells[0].Key.ColumnQualifier, "great");
            Assert.AreEqual(Encoding.Default.GetString(cells[0].Value), "Had with breakfast");
            Assert.AreEqual(cells[0].Key.DateTime, new DateTime(2009, 8, 2, 8, 30, 1, DateTimeKind.Local).ToUniversalTime());

            Assert.AreEqual(cells[1].Key.Row, "banana");
            Assert.AreEqual(cells[1].Key.ColumnFamily, "tag");
            Assert.AreEqual(cells[1].Key.ColumnQualifier, "great");
            Assert.AreEqual(Encoding.Default.GetString(cells[1].Value), "Had with lunch");
            Assert.AreEqual(cells[1].Key.DateTime, new DateTime(2009, 8, 2, 8, 30, 0, DateTimeKind.Local).ToUniversalTime());

            Ns.Exec("DROP TABLE fruit");
        }

        [TestInitialize]
        public void TestInitialize() {
            TestBase.ContinueExecution();
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

        #region Methods

        private void ValidateFruitTable(string tableName)
        {
            var cells = Ns.Query(string.Format("SELECT * FROM {0}", tableName));
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
            Assert.AreEqual(cells[2].Key.DateTime, new DateTime(2009, 8, 2, 8, 30, 0, DateTimeKind.Local).ToUniversalTime());
            Assert.AreEqual(Encoding.Default.GetString(cells[2].Value), "A cultivated variety of muskmelon with orange flesh");

            cells = Ns.Query(string.Format("SELECT tag FROM {0}", tableName));
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

            cells = Ns.Query(string.Format("SELECT * FROM {0} WHERE ROW=\"cantelope\"", tableName));
            Assert.IsNotNull(cells);
            Assert.AreEqual(2, cells.Count);
            Assert.AreEqual(cells[0].Key.Row, "cantelope");
            Assert.AreEqual(cells[0].Key.ColumnFamily, "tag");
            Assert.AreEqual(cells[0].Key.ColumnQualifier, "good");
            Assert.AreEqual(Encoding.Default.GetString(cells[0].Value), "Had with breakfast");
            Assert.AreEqual(cells[1].Key.Row, "cantelope");
            Assert.AreEqual(cells[1].Key.ColumnFamily, "description");
            Assert.AreEqual(cells[1].Key.ColumnQualifier, string.Empty);
            Assert.AreEqual(cells[1].Key.DateTime, new DateTime(2009, 8, 2, 8, 30, 0, DateTimeKind.Local).ToUniversalTime());
            Assert.AreEqual(Encoding.Default.GetString(cells[1].Value), "A cultivated variety of muskmelon with orange flesh");
        }

        #endregion
    }
}