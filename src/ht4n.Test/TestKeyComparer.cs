/** -*- C# -*-
 * Copyright (C) 2010-2013 Thalmann Software & Consulting, http://www.softdev.ch
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

    using Hypertable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test the key comparer.
    /// </summary>
    [TestClass]
    public class TestKeyComparer
    {
        #region Public Methods

        [TestMethod]
        public void CompareExcludeTimestamp() {
            var comparer = new KeyComparer(false);
            var x = new Key();
            Assert.IsFalse(comparer.Equals(x, null));
            Assert.IsFalse(comparer.Equals(null, x));

            var y = new Key();
            Assert.IsTrue(comparer.Equals(x, y));
            Assert.AreEqual(comparer.GetHashCode(x), comparer.GetHashCode(y));

            x.Row = "A";
            y.Row = "B";
            Assert.IsFalse(comparer.Equals(x, y));

            x.Row = "AB";
            y.Row = "AB";
            Assert.IsTrue(comparer.Equals(x, y));
            Assert.AreEqual(comparer.GetHashCode(x), comparer.GetHashCode(y));

            x.ColumnFamily = "A";
            y.ColumnFamily = "B";
            Assert.IsFalse(comparer.Equals(x, y));

            x.ColumnFamily = null;
            y.ColumnFamily = string.Empty;
            Assert.IsFalse(comparer.Equals(x, y));

            x.ColumnFamily = "AB";
            y.ColumnFamily = "AB";
            Assert.IsTrue(comparer.Equals(x, y));
            Assert.AreEqual(comparer.GetHashCode(x), comparer.GetHashCode(y));

            x.ColumnQualifier = "A";
            y.ColumnQualifier = "B";
            Assert.IsFalse(comparer.Equals(x, y));

            x.ColumnQualifier = null;
            y.ColumnQualifier = string.Empty;
            Assert.IsTrue(comparer.Equals(x, y));
            Assert.AreEqual(comparer.GetHashCode(x), comparer.GetHashCode(y));

            x.ColumnQualifier = "AB";
            y.ColumnQualifier = "AB";
            Assert.IsTrue(comparer.Equals(x, y));
            Assert.AreEqual(comparer.GetHashCode(x), comparer.GetHashCode(y));

            x.DateTime = new DateTime(2011, 1, 1, 1, 2, 3, DateTimeKind.Utc);
            y.DateTime = new DateTime(2011, 1, 1, 1, 2, 4, DateTimeKind.Utc);
            Assert.IsTrue(comparer.Equals(x, y));
            Assert.AreEqual(comparer.GetHashCode(x), comparer.GetHashCode(y));

            x.DateTime = new DateTime(2011, 1, 1, 1, 2, 3, DateTimeKind.Utc);
            y.DateTime = new DateTime(2011, 1, 1, 1, 2, 3, DateTimeKind.Utc);
            Assert.IsTrue(comparer.Equals(x, y));
            Assert.AreEqual(comparer.GetHashCode(x), comparer.GetHashCode(y));
        }

        [TestMethod]
        public void CompareIncludeTimestamp() {
            var comparer = new KeyComparer(true);
            var x = new Key();
            Assert.IsFalse(comparer.Equals(x, null));
            Assert.IsFalse(comparer.Equals(null, x));

            var y = new Key();
            Assert.IsTrue(comparer.Equals(x, y));
            Assert.AreEqual(comparer.GetHashCode(x), comparer.GetHashCode(y));

            x.Row = "A";
            y.Row = "B";
            Assert.IsFalse(comparer.Equals(x, y));

            x.Row = "AB";
            y.Row = "AB";
            Assert.IsTrue(comparer.Equals(x, y));
            Assert.AreEqual(comparer.GetHashCode(x), comparer.GetHashCode(y));

            x.ColumnFamily = "A";
            y.ColumnFamily = "B";
            Assert.IsFalse(comparer.Equals(x, y));

            x.ColumnFamily = null;
            y.ColumnFamily = string.Empty;
            Assert.IsFalse(comparer.Equals(x, y));

            x.ColumnFamily = "AB";
            y.ColumnFamily = "AB";
            Assert.IsTrue(comparer.Equals(x, y));
            Assert.AreEqual(comparer.GetHashCode(x), comparer.GetHashCode(y));

            x.ColumnQualifier = "A";
            y.ColumnQualifier = "B";
            Assert.IsFalse(comparer.Equals(x, y));

            x.ColumnQualifier = null;
            y.ColumnQualifier = string.Empty;
            Assert.IsTrue(comparer.Equals(x, y));
            Assert.AreEqual(comparer.GetHashCode(x), comparer.GetHashCode(y));

            x.ColumnQualifier = "AB";
            y.ColumnQualifier = "AB";
            Assert.IsTrue(comparer.Equals(x, y));
            Assert.AreEqual(comparer.GetHashCode(x), comparer.GetHashCode(y));

            x.DateTime = new DateTime(2011, 1, 1, 1, 2, 3, DateTimeKind.Utc);
            y.DateTime = new DateTime(2011, 1, 1, 1, 2, 4, DateTimeKind.Utc);
            Assert.IsFalse(comparer.Equals(x, y));

            x.DateTime = new DateTime(2011, 1, 1, 1, 2, 3, DateTimeKind.Utc);
            y.DateTime = new DateTime(2011, 1, 1, 1, 2, 3, DateTimeKind.Utc);
            Assert.IsTrue(comparer.Equals(x, y));
            Assert.AreEqual(comparer.GetHashCode(x), comparer.GetHashCode(y));
        }

        #endregion
    }
}