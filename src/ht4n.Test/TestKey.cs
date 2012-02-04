/** -*- C# -*-
 * Copyright (C) 2010-2012 Thalmann Software & Consulting, http://www.softdev.ch
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

    using Hypertable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test Key.
    /// </summary>
    [TestClass]
    public class TestKey
    {
        #region Public Methods

        [TestMethod]
        public void Compare() {
            var x = new Key();
            Assert.IsTrue(x.CompareTo(null) > 0);
            Assert.IsFalse(x.Equals(null));
            Assert.IsFalse(x.Equals(string.Empty));
            Assert.IsTrue(x != null);

            var y = new Key();
            Assert.AreEqual(x, y);
            Assert.IsTrue(x.CompareTo(y) == 0);
            Assert.IsTrue(x == y);

            x.Row = "A";
            y.Row = "B";
            Assert.AreNotEqual(x, y);
            Assert.IsTrue(x.CompareTo(y) < 0);
            Assert.IsTrue(y.CompareTo(x) > 0);
            Assert.IsTrue(x != y);
            Assert.IsTrue(x < y);
            Assert.IsFalse(x > y);

            x.Row = "AB";
            y.Row = "AB";
            Assert.AreEqual(x, y);
            Assert.IsTrue(x.CompareTo(y) == 0);
            Assert.IsTrue(y.CompareTo(x) == 0);
            Assert.IsTrue(x == y);

            x.ColumnFamily = "A";
            y.ColumnFamily = "B";
            Assert.AreNotEqual(x, y);
            Assert.IsTrue(x.CompareTo(y) < 0);
            Assert.IsTrue(y.CompareTo(x) > 0);
            Assert.IsTrue(x != y);
            Assert.IsTrue(x < y);
            Assert.IsFalse(x > y);

            x.ColumnFamily = null;
            y.ColumnFamily = string.Empty;
            Assert.AreNotEqual(x, y);
            Assert.IsTrue(x.CompareTo(y) < 0);
            Assert.IsTrue(y.CompareTo(x) > 0);

            x.ColumnFamily = "AB";
            y.ColumnFamily = "AB";
            Assert.AreEqual(x, y);
            Assert.IsTrue(x.CompareTo(y) == 0);
            Assert.IsTrue(y.CompareTo(x) == 0);
            Assert.IsTrue(x == y);

            x.ColumnQualifier = "A";
            y.ColumnQualifier = "B";
            Assert.AreNotEqual(x, y);
            Assert.IsTrue(x.CompareTo(y) < 0);
            Assert.IsTrue(y.CompareTo(x) > 0);
            Assert.IsTrue(x != y);
            Assert.IsTrue(x < y);
            Assert.IsFalse(x > y);

            x.ColumnQualifier = null;
            y.ColumnQualifier = string.Empty;
            Assert.AreNotEqual(x, y);
            Assert.IsTrue(x.CompareTo(y) < 0);
            Assert.IsTrue(y.CompareTo(x) > 0);
            Assert.IsTrue(x != y);
            Assert.IsTrue(x < y);
            Assert.IsFalse(x > y);

            x.ColumnQualifier = "AB";
            y.ColumnQualifier = "AB";
            Assert.AreEqual(x, y);
            Assert.IsTrue(x.CompareTo(y) == 0);
            Assert.IsTrue(y.CompareTo(x) == 0);
            Assert.IsTrue(x == y);

            x.DateTime = new DateTime(2011, 1, 1, 1, 2, 3, DateTimeKind.Utc);
            y.DateTime = new DateTime(2011, 1, 1, 1, 2, 4, DateTimeKind.Utc);
            Assert.AreNotEqual(x, y);
            Assert.IsTrue(x.CompareTo(y) < 0);
            Assert.IsTrue(y.CompareTo(x) > 0);
            Assert.IsTrue(x != y);
            Assert.IsTrue(x < y);
            Assert.IsFalse(x > y);

            x.DateTime = new DateTime(2011, 1, 1, 1, 2, 3, DateTimeKind.Utc);
            y.DateTime = new DateTime(2011, 1, 1, 1, 2, 3, DateTimeKind.Utc);
            Assert.AreEqual(x, y);
            Assert.IsTrue(x.CompareTo(y) == 0);
            Assert.IsTrue(y.CompareTo(x) == 0);
            Assert.AreEqual(x, y);
        }

        [TestMethod]
        public void EncodeDecodeGuid() {
            var guid = Guid.Empty;
            Assert.AreEqual(Key.Decode(Key.Encode(guid)), guid);
            guid = Guid.Parse("F0000000-0000-0000-F000-000000000000");
            Assert.AreEqual(Key.Decode(Key.Encode(guid)), guid);
            guid = Guid.Parse("00000000-0000-000F-0000-00000000000F");
            Assert.AreEqual(Key.Decode(Key.Encode(guid)), guid);
            guid = Guid.Parse("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");
            Assert.AreEqual(Key.Decode(Key.Encode(guid)), guid);
            guid = Guid.Parse("FFFFFFFF-FFFF-FFFF-0000-000000000000");
            Assert.AreEqual(Key.Decode(Key.Encode(guid)), guid);
            guid = Guid.Parse("00000000-0000-0000-FFFF-FFFFFFFFFFFF");
            Assert.AreEqual(Key.Decode(Key.Encode(guid)), guid);
            guid = Guid.NewGuid();
            Assert.AreEqual(Key.Decode(Key.Encode(guid)), guid);

            IList<string> guids = new List<string>();
            for (int n = 0; n < 100000; ++n) {
                guids.Add(Key.Generate());
            }

            foreach (string s in guids) {
                Assert.AreEqual(Key.Encode(Key.Decode(s)), s);
            }
        }

        #endregion
    }
}