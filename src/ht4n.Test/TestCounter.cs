/** -*- C# -*-
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
    using Hypertable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test the scan spec.
    /// </summary>
    [TestClass]
    public class TestCounter : TestBase
    {
        #region Static Fields

        private static ITable table;

        #endregion

        #region Public Methods and Operators

        [ClassCleanup]
        public static void ClassCleanup()
        {
            if( table != null ) {
                table.Dispose();
            }
            Ns.DropNamespaces(DropDispositions.Complete);
            Ns.DropTables();
        }

        [ClassInitialize]
        public static void ClassInitialize(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            if( !HasCounterColumn ) {
                Assert.IsFalse(IsHyper);
                Assert.IsFalse(IsThrift);

                return;
            }

            const string Schema =
                "<Schema><AccessGroup name=\"default\" blksz=\"1024\">" + "<ColumnFamily><Name>a</Name><Counter>true</Counter></ColumnFamily>"
                + "<ColumnFamily><Name>b</Name><Counter>true</Counter></ColumnFamily>" + "<ColumnFamily><Name>c</Name><Counter>true</Counter></ColumnFamily>"
                + "</AccessGroup></Schema>";

            table = EnsureTable(typeof(TestCounter), Schema);
        }

        [TestMethod]
        public void IncrementDecrementCounter()
        {
            if (!HasCounterColumn)
            {
                return;
            }

            var keyA = new Key { ColumnFamily = "a", Row = "COUNTER" };
            var keyB = new Key { ColumnFamily = "b", Row = "COUNTER" };
            var keyC = new Key { ColumnFamily = "c", Row = "COUNTER" };

            var counterA = this.SetCounterValue(keyA, 0);
            var counterB = this.SetCounterValue(keyB, 0);
            var counterC = this.SetCounterValue(keyC, 0);

            const int Count = 100000;
            using (var mutator = table.CreateMutator())
            {
                for (var i = 0; i < Count; ++i)
                {
                    counterA.IncrementCounter(1);
                    mutator.Set(counterA.ToCell());

                    counterB.IncrementCounter(5);
                    mutator.Set(counterB.ToCell());

                    if ((i % 5) == 0)
                    {
                        counterC.IncrementCounter(3);
                        mutator.Set(counterC.ToCell());
                    }
                }
            }

            counterA = this.GetCounterValue(keyA, Count);
            counterB = this.GetCounterValue(keyB, 5 * Count);
            counterC = this.GetCounterValue(keyC, 3 * (Count / 5));

            using (var mutator = table.CreateMutator())
            {
                for (var i = 0; i < Count; ++i)
                {
                    counterA.DecrementCounter(1);
                    mutator.Set(counterA.ToCell());

                    counterB.DecrementCounter(5);
                    mutator.Set(counterB.ToCell());

                    if ((i % 5) == 0)
                    {
                        counterC.DecrementCounter(3);
                        mutator.Set(counterC.ToCell());
                    }
                }
            }

            this.GetCounterValue(keyA, 0);
            this.GetCounterValue(keyB, 0);
            this.GetCounterValue(keyC, 0);
        }

        [TestMethod]
        public void SetCounterValue()
        {
            if (!HasCounterColumn)
            {
                return;
            }

            var key = new Key { ColumnFamily = "a", Row = "COUNTER" };

            this.SetCounterValue(key, 0);
            this.SetCounterValue(key, 1234567);
            this.SetCounterValue(key, -1234567);
        }

        #endregion

        #region Methods

        private Counter GetCounterValue(Key key, int expectedValue)
        {
            using (var scanner = table.CreateScanner(new ScanSpec(key)))
            {
                var cell = new Cell();
                Assert.IsTrue(scanner.Move(cell));
                var counter = new Counter(cell);
                Assert.IsTrue(counter.Value.HasValue);
                Assert.AreEqual(expectedValue, counter.Value);
                Assert.IsFalse(scanner.Move(cell));
                return counter;
            }
        }

        private Counter SetCounterValue(Key key, int value)
        {
            using (var mutator = table.CreateMutator())
            {
                var counter = new Counter(key, value);
                Assert.IsTrue(counter.Value.HasValue);
                Assert.AreEqual(value, counter.Value);
                mutator.Set(counter.ToCell());
            }

            return this.GetCounterValue(key, value);
        }

        #endregion
    }
}