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
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    using Hypertable.Xml;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test the table schema.
    /// </summary>
    [TestClass]
    public class TestTableSchema : TestBase
    {
        #region Fields

        /// <summary>
        /// The regex.
        /// </summary>
        private readonly Regex regex = new Regex("test-", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Tests the xml table schema parse.
        /// </summary>
        [TestMethod]
        public void Parse()
        {
            var schema = TableSchema.Parse(
                "<Schema><AccessGroup name=\"default\"><ColumnFamily><Name>a</Name></ColumnFamily></AccessGroup></Schema>");
            Assert.IsNotNull(schema);

            Assert.IsNotNull(schema.AccessGroups);
            Assert.AreEqual(1, schema.AccessGroups.Count);
            
            var accessGroup = schema.AccessGroups[0];
            Assert.AreEqual("default", accessGroup.Name);
            Assert.IsNull(accessGroup.Options);
            Assert.IsNotNull(accessGroup.ColumnFamilies);
            Assert.AreEqual(1, accessGroup.ColumnFamilies.Count);

            var columnFamily = accessGroup.ColumnFamilies[0];
            Assert.AreEqual("a", columnFamily.Name);

            schema = TableSchema.Parse(
                "<Schema><AccessGroup name=\"default\"><ColumnFamily><Name>a</Name></ColumnFamily><ColumnFamily><Name>b</Name><Options><Counter>true</Counter></Options></ColumnFamily></AccessGroup></Schema>");
            Assert.IsNotNull(schema);

            Assert.IsNotNull(schema.AccessGroups);
            Assert.AreEqual(1, schema.AccessGroups.Count);

            accessGroup = schema.AccessGroups[0];
            Assert.AreEqual("default", accessGroup.Name);
            Assert.IsNull(accessGroup.Options);
            Assert.IsNotNull(accessGroup.ColumnFamilies);
            Assert.AreEqual(2, accessGroup.ColumnFamilies.Count);

            columnFamily = accessGroup.ColumnFamilies[0];
            Assert.AreEqual("a", columnFamily.Name);

            columnFamily = accessGroup.ColumnFamilies[1];
            Assert.AreEqual("b", columnFamily.Name);
            Assert.IsNotNull(columnFamily.Options);
            Assert.IsTrue(columnFamily.Options.CounterSpecified);
            Assert.IsTrue(columnFamily.Options.Counter);
        }

        /// <summary>
        /// Tests the TableSchema rendering.
        /// </summary>
        [TestMethod]
        public void Render()
        {
            DropTables(this.regex);

            Assert.IsFalse(Ns.TableExists("test-1"));

            var schema = new TableSchema { AccessGroups = new List<AccessGroup>() };

            var accessGroup = new AccessGroup { Name = "default", ColumnFamilies = new List<ColumnFamily>() };
            accessGroup.ColumnFamilies.Add(new ColumnFamily { Name = "a" });
            accessGroup.ColumnFamilies.Add(new ColumnFamily { Name = "b", Options = new ColumnFamilyOptions { MaxVersions = 5, MaxVersionsSpecified = true } });
            schema.AccessGroups.Add(accessGroup);

            accessGroup = new AccessGroup { Name = "other", ColumnFamilies = new List<ColumnFamily>() };
            accessGroup.ColumnFamilies.Add(new ColumnFamily { Name = "c" });
            schema.AccessGroups.Add(accessGroup);

            var xml = schema.ToString();

            schema = TableSchema.Parse(xml);
            Assert.IsNotNull(schema);
            Assert.IsFalse(schema.GenerationSpecified);

            Assert.IsNotNull(schema.AccessGroups);
            Assert.AreEqual(2, schema.AccessGroups.Count);
            
            accessGroup = schema.AccessGroups[0];
            Assert.AreEqual("default", accessGroup.Name);
            Assert.IsNotNull(accessGroup.ColumnFamilies);
            Assert.AreEqual(2, accessGroup.ColumnFamilies.Count);

            var columnFamily = accessGroup.ColumnFamilies[0];
            Assert.AreEqual("a", columnFamily.Name);

            columnFamily = accessGroup.ColumnFamilies[1];
            Assert.AreEqual("b", columnFamily.Name);
            Assert.IsNotNull(columnFamily.Options);
            Assert.IsTrue(columnFamily.Options.MaxVersionsSpecified);
            Assert.AreEqual(5, columnFamily.Options.MaxVersions);

            accessGroup = schema.AccessGroups[1];
            Assert.AreEqual("other", accessGroup.Name);
            Assert.IsNotNull(accessGroup.ColumnFamilies);
            Assert.AreEqual(1, accessGroup.ColumnFamilies.Count);

            columnFamily = accessGroup.ColumnFamilies[0];
            Assert.AreEqual("c", columnFamily.Name);

            Ns.CreateTable("test-1", xml);
            Assert.IsTrue(Ns.TableExists("test-1"));
        }

        #endregion
    }
}