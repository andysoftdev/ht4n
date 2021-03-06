﻿/** -*- C# -*-
 * Copyright (C) 2010-2016 Thalmann Software & Consulting, http://www.softdev.ch
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
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;

    using Hypertable;
    using Hypertable.Xml;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test the tables.
    /// </summary>
    [TestClass]
    public class TestTable : TestBase
    {
        #region Constants and Fields

        private readonly Regex regex = new Regex("test-|table_does_not_exists", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        #endregion

        #region Public Methods

        [TestMethod]
        public void AlterIndexedTable() {
            DropTables(this.regex);

            Assert.IsFalse(Ns.TableExists("test-1"));

            const string SchemaA =
                "<Schema><AccessGroup name=\"default\">" +
                "<ColumnFamily><Name>a</Name><Index>true</Index><QualifierIndex>true</QualifierIndex></ColumnFamily>" +
                "</AccessGroup>" + "</Schema>";

            Ns.CreateTable("test-1", SchemaA);
            Assert.IsTrue(Ns.TableExists("test-1"));
            Ns.CreateTable("test-1", SchemaA, CreateDispositions.CreateIfNotExist);

            if (IsHyper || IsThrift) {
                Assert.IsTrue(Ns.TableExists("^test-1"));
                Assert.IsTrue(Ns.TableExists("^^test-1"));
            }

            var _schemaA = Ns.GetTableSchema("test-1");
            using (var table = Ns.OpenTable("test-1")) {
                Assert.AreEqual(table.Name, Ns.Name + "/test-1");
                Assert.AreEqual(table.Schema, _schemaA);
            }

            const string SchemaB = 
                "<Schema><AccessGroup name=\"default\">" +
                "<ColumnFamily><Name>b</Name></ColumnFamily>" +
                "</AccessGroup></Schema>";

            Ns.AlterTable("test-1", SchemaB);

            if (IsHyper || IsThrift) {
                Assert.IsFalse(Ns.TableExists("^test-1"));
                Assert.IsFalse(Ns.TableExists("^^test-1"));
            }

            var _schemaB = Ns.GetTableSchema("test-1");
            Assert.AreNotEqual(_schemaB, _schemaA);
            using (var table = Ns.OpenTable("test-1")) {
                Assert.AreEqual(table.Name, Ns.Name + "/test-1");
                Assert.AreEqual(table.Schema, _schemaB);
            }

            const string SchemaC =
                "<Schema><AccessGroup name=\"default\">" +
                "<ColumnFamily><Name>a</Name></ColumnFamily>" +
                "<ColumnFamily><Name>b</Name><Index>true</Index></ColumnFamily>" +
                "</AccessGroup>" + "</Schema>";

            Ns.AlterTable("test-1", SchemaC);

            if (IsHyper || IsThrift)
            {
                Assert.IsTrue(Ns.TableExists("^test-1"));
                Assert.IsFalse(Ns.TableExists("^^test-1"));
            }

            const string SchemaD =
                "<Schema><AccessGroup name=\"default\">" +
                "<ColumnFamily><Name>a</Name><Index>true</Index><QualifierIndex>true</QualifierIndex></ColumnFamily>" +
                "<ColumnFamily><Name>b</Name><Index>true</Index></ColumnFamily>" +
                "</AccessGroup>" + "</Schema>";

            Ns.AlterTable("test-1", SchemaD);

            if (IsHyper || IsThrift)
            {
                Assert.IsTrue(Ns.TableExists("^test-1"));
                Assert.IsTrue(Ns.TableExists("^^test-1"));
            }
        }

        [TestMethod]
        public void AlterTable() {
            DropTables(this.regex);

            Assert.IsFalse(Ns.TableExists("test-1"));

            const string SchemaA = 
                "<Schema><AccessGroup name=\"default\">" +
                "<ColumnFamily><Name>a</Name></ColumnFamily>" +
                "</AccessGroup></Schema>";

            const string SchemaB =
                "<Schema><AccessGroup name=\"default\">" +
                "<ColumnFamily><Name>b</Name></ColumnFamily>" +
                "</AccessGroup></Schema>";

            Ns.CreateTable("test-1", SchemaA);
            Assert.IsTrue(Ns.TableExists("test-1"));
            Ns.CreateTable("test-1", SchemaA, CreateDispositions.CreateIfNotExist);
            var _schemaA = Ns.GetTableSchema("test-1");
            using (var table = Ns.OpenTable("test-1")) {
                Assert.AreEqual(table.Name, Ns.Name + "/test-1");
                Assert.AreEqual(table.Schema, _schemaA);

                using (var mutator = table.CreateMutator())
                {
                    mutator.Set(new Key("R1", "a"), new byte[] { 1, 2, 3});
                }
            }

            Ns.AlterTable("test-1", SchemaB);
            var _schemaB = Ns.GetTableSchema("test-1");
            Assert.AreNotEqual(_schemaB, _schemaA);
            using (var table = Ns.OpenTable("test-1")) {
                Assert.AreEqual(table.Name, Ns.Name + "/test-1");
                Assert.AreEqual(table.Schema, _schemaB);

                using (var mutator = table.CreateMutator())
                {
                    try
                    {
                        mutator.Set(new Key("R1", "a"), new byte[] { 1, 2, 3 });
                        Assert.Fail();
                    }
                    catch (BadKeyException)
                    {
                    }
                    catch
                    {
                        Assert.Fail(); 
                    }

                    mutator.Set(new Key("R2", "b"), new byte[] { 1, 2, 3 });
                }

                Ns.AlterTable("test-1", SchemaA);

                using (var mutator = table.CreateMutator())
                {
                    mutator.Set(new Key("R1", "a"), new byte[] { 1, 2, 3 });

                    try
                    {
                        mutator.Set(new Key("R2", "b"), new byte[] { 1, 2, 3 });
                    }
                    catch (BadKeyException)
                    {
                    }
                    catch
                    {
                        Assert.Fail();
                    }
                }

                var existingSchema = TableSchema.Parse(Ns.GetTableSchema("test-1", true));
                Assert.IsTrue(existingSchema.GenerationSpecified);
                Assert.IsTrue(existingSchema.AccessGroups.All(ag => ag.GenerationSpecified));
                Assert.IsTrue(existingSchema.AccessGroups.SelectMany(ag => ag.ColumnFamilies).All(cf => cf.GenerationSpecified));

                var newSchema = TableSchema.Parse(Ns.GetTableSchema("test-1"));
                newSchema.AccessGroups.First().ColumnFamilies.Insert(0, new ColumnFamily { Name = "new" });
                Ns.AlterTable("test-1", newSchema.ToString());
                var alteredSchema = TableSchema.Parse(Ns.GetTableSchema("test-1", true));
                Assert.IsTrue(alteredSchema.GenerationSpecified);
                Assert.IsTrue(existingSchema.Generation < alteredSchema.Generation);
                Assert.IsTrue(existingSchema.AccessGroups.All(ag => ag.GenerationSpecified));
                Assert.IsTrue(existingSchema.AccessGroups.SelectMany(ag => ag.ColumnFamilies).All(cf => cf.GenerationSpecified));

                var d = alteredSchema.AccessGroups.SelectMany(ag => ag.ColumnFamilies).ToDictionary(cf => cf.Id);
                foreach (var cf in existingSchema.AccessGroups.SelectMany(ag => ag.ColumnFamilies))
                {
                    Assert.IsTrue(d.ContainsKey(cf.Id));
                    Assert.AreEqual(cf.Name, d[cf.Id].Name);
                }
            }

            try
            {
                Ns.AlterTable("test-11", SchemaB);
                Assert.Fail();
            }
            catch (TableNotFoundException)
            {
            }
            catch
            {
                Assert.Fail();
            }

            try
            {
                Ns.CreateNamespace("test-22");
                Ns.AlterTable("test-22", SchemaB);
                Assert.Fail();
            }
            catch (TableNotFoundException)
            {
            }
            catch
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void CreateOpenDropTable() {
            DropTables(this.regex);

            try {
                Ns.OpenTable("table_does_not_exists");
                Assert.Fail();
            }
            catch (TableNotFoundException) {
            }
            catch {
                Assert.Fail();
            }

            Assert.IsFalse(Ns.TableExists("test-1"));
            Assert.IsFalse(Ns.TableExists("Test-2"));
            Assert.IsFalse(Ns.TableExists("test-3"));

            const string Schema =
                "<Schema><AccessGroup name=\"default\">" +
                "<ColumnFamily><Name>a</Name></ColumnFamily>" +
                "</AccessGroup></Schema>";

            Ns.CreateTable("test-1", Schema);
            Assert.IsTrue(Ns.TableExists("test-1"));
            using (var table = Ns.OpenTable("test-1")) {
                Assert.AreEqual(table.Name, Ns.Name + "/test-1");
            }

            Ns.CreateTableLike("Test-2", "test-1");
            Assert.IsTrue(Ns.TableExists("Test-2"));
            Ns.CreateTableLike("Test-2", "test-1", CreateDispositions.CreateIfNotExist);
            Assert.IsFalse(Ns.TableExists("test-2")); // case sensitive
            using (var table = Ns.OpenTable("Test-2")) {
                Assert.AreEqual(table.Name, Ns.Name + "/Test-2");
            }

            Ns.CreateTable("test-3", Ns.GetTableSchema("test-1"), CreateDispositions.CreateIfNotExist);
            Assert.IsTrue(Ns.TableExists("test-3"));
            using (var table = Ns.OpenTable("test-3")) {
                Assert.AreEqual(table.Name, Ns.Name + "/test-3");
            }

            var tables = Ns.Tables;
            Assert.IsTrue(tables.Contains("test-1"));
            Assert.IsTrue(tables.Contains("Test-2"));
            Assert.IsTrue(tables.Contains("test-3"));

            try
            {
                Ns.CreateTableLike("Test-2", "test-1");
                Assert.Fail();
            }
            catch (TableExistsException)
            {
            }
            catch
            {
                Assert.Fail();
            }

            try
            {
                Ns.CreateNamespace("test-1");
                Assert.Fail();
            }
            catch (NameAlreadyInUseException)
            {
            }
            catch
            {
                Assert.Fail();
            }

            const string Schema2 =
                "<Schema><AccessGroup name=\"default\">" +
                "<ColumnFamily id=\"2\"><Name>a</Name></ColumnFamily>" +
                "<ColumnFamily id=\"1\"><Name>b</Name></ColumnFamily>" +
                "</AccessGroup></Schema>";

            Ns.CreateTable("test-4", Schema2);
            Assert.IsTrue(Ns.TableExists("test-4"));
            using (var table = Ns.OpenTable("test-4"))
            {
                Assert.AreEqual("test/test-4", table.Name);
                Assert.AreEqual(2, table.GetTableSchema().AccessGroups.First().ColumnFamilies.First(cf => cf.Name == "a").Id);
            }

            const string Schema3 =
                 "<Schema><AccessGroup name=\"default\">" +
                 "<ColumnFamily id='2'><Name>a</Name></ColumnFamily>" +
                 "<ColumnFamily id='3'><Name>b</Name></ColumnFamily>" +
                 "<ColumnFamily id='1'><Name>c</Name></ColumnFamily>" +
                 "</AccessGroup></Schema>";

            Ns.CreateTable("test-5", Schema3);
            Assert.IsTrue(Ns.TableExists("test-5"));
            using (var table = Ns.OpenTable("test-5"))
            {
                Assert.AreEqual(3, table.GetTableSchema().AccessGroups.First().ColumnFamilies.First(cf => cf.Name == "b").Id);
            }

            Ns.CreateTable("1/2/3/test-5", Schema3, CreateDispositions.CreateIntermediate);
            Assert.IsTrue(Ns.NamespaceExists("1/2/3"));
            Assert.IsTrue(Ns.TableExists("1/2/3/test-5"));
            using (var table = Ns.OpenTable("1/2/3/test-5"))
            {
                Assert.AreEqual("test/1/2/3/test-5", table.Name);
                Assert.AreEqual(3, table.GetTableSchema().AccessGroups.First().ColumnFamilies.First(cf => cf.Name == "b").Id);
            }
        }

        [TestMethod]
        public void DropIndexedTable() {
            DropTables(this.regex);

            Assert.IsFalse(Ns.TableExists("test"));

            const string Schema =
                "<Schema><AccessGroup name=\"default\">" +
                "<ColumnFamily><Name>a</Name><Index>true</Index><QualifierIndex>true</QualifierIndex></ColumnFamily>" +
                "</AccessGroup></Schema>";

            Ns.CreateTable("test", Schema, CreateDispositions.CreateIfNotExist);
            Assert.IsTrue(Ns.TableExists("test"));

            if (IsHyper || IsThrift) {
                Assert.IsTrue(Ns.TableExists("^test"));
                Assert.IsTrue(Ns.TableExists("^^test"));
            }

            Ns.DropTable("test");

            if (IsHyper || IsThrift) {
                Assert.IsFalse(Ns.TableExists("^test"));
                Assert.IsFalse(Ns.TableExists("^^test"));
            }
        }

        [TestMethod]
        public void RenameIndexedTable() {
            DropTables(this.regex);

            Assert.IsFalse(Ns.TableExists("test-1"));
            Assert.IsFalse(Ns.TableExists("test-2"));
            Assert.IsFalse(Ns.TableExists("Test-2"));

            const string Schema =
                "<Schema><AccessGroup name=\"default\">" +
                "<ColumnFamily><Name>a</Name><Index>true</Index><QualifierIndex>true</QualifierIndex></ColumnFamily>" +
                "</AccessGroup></Schema>";

            Ns.CreateTable("test-1", Schema, CreateDispositions.CreateIfNotExist);
            Assert.IsTrue(Ns.TableExists("test-1"));

            if (IsHyper || IsThrift) {
                Assert.IsTrue(Ns.TableExists("^test-1"));
                Assert.IsTrue(Ns.TableExists("^^test-1"));
            }

            using (var table = Ns.OpenTable("test-1")) {
                Assert.AreEqual(table.Name, Ns.Name + "/test-1");
            }

            Ns.RenameTable("test-1", "test-2");
            Assert.IsFalse(Ns.TableExists("test-1"));
            Assert.IsTrue(Ns.TableExists("test-2"));

            if (IsHyper || IsThrift) {
                Assert.IsFalse(Ns.TableExists("^test-1"));
                Assert.IsFalse(Ns.TableExists("^^test-1"));
                Assert.IsTrue(Ns.TableExists("^test-2"));
                Assert.IsTrue(Ns.TableExists("^^test-2"));
            }

            Ns.RenameTable("test-2", "Test-2");
            Assert.IsFalse(Ns.TableExists("test-2"));
            using (var table = Ns.OpenTable("Test-2")) {
                Assert.AreEqual(table.Name, Ns.Name + "/Test-2");
            }

            Assert.IsTrue(Ns.TableExists("Test-2"));

            if (IsHyper || IsThrift) {
                Assert.IsFalse(Ns.TableExists("^test-2"));
                Assert.IsFalse(Ns.TableExists("^^test-2"));
                Assert.IsTrue(Ns.TableExists("^Test-2"));
                Assert.IsTrue(Ns.TableExists("^^Test-2"));
            }
        }

        [TestMethod]
        public void RenameTable() {
            DropTables(this.regex);
            DropNamespaces(this.regex);

            Assert.IsFalse(Ns.TableExists("test-1"));
            Assert.IsFalse(Ns.TableExists("test-2"));
            Assert.IsFalse(Ns.TableExists("Test-2"));

            const string Schema =
                "<Schema><AccessGroup name=\"default\">" +
                "<ColumnFamily><Name>a</Name></ColumnFamily>" +
                "</AccessGroup></Schema>";

            Ns.CreateTable("test-1", Schema, CreateDispositions.CreateIfNotExist);
            Assert.IsTrue(Ns.TableExists("test-1"));
            using (var table = Ns.OpenTable("test-1")) {
                Assert.AreEqual(table.Name, Ns.Name + "/test-1");
            }

            Ns.RenameTable("test-1", "test-2");
            Assert.IsFalse(Ns.TableExists("test-1"));
            Assert.IsTrue(Ns.TableExists("test-2"));

            Ns.RenameTable("test-2", "Test-2");
            Assert.IsFalse(Ns.TableExists("test-2"));
            using (var table = Ns.OpenTable("Test-2")) {
                Assert.AreEqual(table.Name, Ns.Name + "/Test-2");
            }

            Assert.IsTrue(Ns.TableExists("Test-2"));

            try
            {
                Ns.RenameTable("test-11", "test-111");
                Assert.Fail();
            }
            catch (TableNotFoundException)
            {
            }
            catch
            {
                Assert.Fail();
            }
            
            try
            {
                Ns.CreateNamespace("test-22");
                Ns.RenameTable("test-22", "test-222");
                Assert.Fail();
            }
            catch (TableNotFoundException)
            {
            }
            /*catch
            {
                Assert.Fail();
            }*/

            try
            {
                Ns.RenameTable("Test-2", "test-22");
                Assert.Fail();
            }
            catch (NameAlreadyInUseException)
            {
            }
            catch
            {
                Assert.Fail();
            }
        }

        #endregion
    }
}