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
    using System.Collections.Generic;

    using Hypertable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test the scan spec.
    /// </summary>
    [TestClass]
    public class TestScanSpec
    {
        #region Public Methods

        [TestMethod]
        public void TestDistictColumn() {
            var distictColumn = ScanSpec.DistictColumn(new[] { "a", "b:1", "b:2", "c:X", "c", "a:1" });
            Assert.AreEqual(4, distictColumn.Count);
            Assert.IsTrue(distictColumn.Contains("a"));
            Assert.IsFalse(distictColumn.Contains("a:1"));
            Assert.IsFalse(distictColumn.Contains("b"));
            Assert.IsTrue(distictColumn.Contains("b:1"));
            Assert.IsTrue(distictColumn.Contains("b:2"));
            Assert.IsTrue(distictColumn.Contains("c"));
            Assert.IsFalse(distictColumn.Contains("c:X"));

            ISet<string> columnFamilies;
            distictColumn = ScanSpec.DistictColumn(new[] { "a", "b:1", "b:2", "c:X", "c", "a:1" }, out columnFamilies);
            Assert.AreEqual(4, distictColumn.Count);
            Assert.IsTrue(distictColumn.Contains("a"));
            Assert.IsFalse(distictColumn.Contains("a:1"));
            Assert.IsFalse(distictColumn.Contains("b"));
            Assert.IsTrue(distictColumn.Contains("b:1"));
            Assert.IsTrue(distictColumn.Contains("b:2"));
            Assert.IsTrue(distictColumn.Contains("c"));
            Assert.IsFalse(distictColumn.Contains("c:X"));

            Assert.AreEqual(3, columnFamilies.Count);
            Assert.IsTrue(columnFamilies.Contains("a"));
            Assert.IsFalse(columnFamilies.Contains("a:1"));
            Assert.IsTrue(columnFamilies.Contains("b"));
            Assert.IsFalse(columnFamilies.Contains("b:1"));
            Assert.IsFalse(columnFamilies.Contains("b:2"));
            Assert.IsTrue(columnFamilies.Contains("c"));
            Assert.IsFalse(columnFamilies.Contains("c:X"));
        }

        #endregion
    }
}