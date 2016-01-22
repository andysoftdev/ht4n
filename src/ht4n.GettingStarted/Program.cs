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

namespace Hypertable.GettingStarted
{
    using System;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// The ht4n GettingStarted application.
    /// </summary>
    internal sealed class Program
    {
        #region Methods

        /// <summary>
        /// The program entry point.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        private static void Main(string[] args) {

            // Preamble
            Console.WriteLine("Welcome to ht4n getting started.");
            Console.WriteLine("For more information about ht4n, visit http://ht4n.softdev.ch/");
            Console.WriteLine();

            try {

                // Compose the connection string - Provider={0};Uri={1}
                var connectionString = string.Format(
                    CultureInfo.InvariantCulture,
                    "Provider={0};Uri={1}",
                    args.Length > 0 ? args[0] : "Hyper",
                    args.Length > 1 ? args[1] : "net.tcp://localhost");

                // Connect to the database instance
                Console.WriteLine("Connecting {0}", connectionString);
                using (var context = Context.Create(connectionString))
                using (var client = context.CreateClient()) {

                    // Open or create namespace
                    Console.WriteLine("Open or create namespace 'tutorials'");
                    using (var ns = client.OpenNamespace("tutorials", OpenDispositions.OpenAlways | OpenDispositions.CreateIntermediate)) {

                        // Define the table schema using xml
                        const string TableSchema =
                            "<Schema>" +
                            "<AccessGroup name=\"default\">" +
                            "<ColumnFamily><Name>color</Name></ColumnFamily>" +
                            "<ColumnFamily><Name>energy</Name></ColumnFamily>" +
                            "<ColumnFamily><Name>protein</Name></ColumnFamily>" +
                            "<ColumnFamily><Name>vitamins</Name></ColumnFamily>" +
                            "</AccessGroup>" +
                            "</Schema>";

                        // Open or create table
                        Console.WriteLine("Open or create table 'fruits'");
                        using (var table = ns.OpenTable("fruits", TableSchema, OpenDispositions.OpenAlways)) {

                            // Insert some fruits
                            using (var mutator = table.CreateMutator()) {
                                Console.WriteLine("Insert 'apple' into 'fruits'");
                                var key = new Key { Row = "apple", ColumnFamily = "color" };
                                mutator.Set(key, Encoding.UTF8.GetBytes("red"));

                                key.ColumnFamily = "energy";
                                mutator.Set(key, BitConverter.GetBytes(207)); // [KJ]

                                key.ColumnFamily = "protein";
                                mutator.Set(key, BitConverter.GetBytes(0.4)); // [g]

                                key.ColumnFamily = "vitamins";
                                key.ColumnQualifier = "C";
                                mutator.Set(key, BitConverter.GetBytes(15.0)); // [mg]

                                key.ColumnQualifier = "B1";
                                mutator.Set(key, BitConverter.GetBytes(0.02)); // [mg]

                                Console.WriteLine("Insert 'banana' into 'fruits'");
                                key = new Key { Row = "banana", ColumnFamily = "color" };
                                mutator.Set(key, Encoding.UTF8.GetBytes("yellow"));

                                key.ColumnFamily = "energy";
                                mutator.Set(key, BitConverter.GetBytes(375)); // [KJ]

                                key.ColumnFamily = "protein";
                                mutator.Set(key, BitConverter.GetBytes(1.2)); // [g]

                                key.ColumnFamily = "vitamins";
                                key.ColumnQualifier = "C";
                                mutator.Set(key, BitConverter.GetBytes(10.0)); // [mg]

                                key.ColumnQualifier = "B1";
                                mutator.Set(key, BitConverter.GetBytes(0.04)); // [mg]
                            }

                            Console.WriteLine();

                            // Some query examples
                            Console.WriteLine("Select all cells from 'fruits'");
                            using (var scanner = table.CreateScanner()) {
                                foreach (var cell in scanner) {
                                    Console.WriteLine(cell);
                                }
                            }

                            Console.WriteLine();

                            Console.WriteLine("Select all 'apple' from 'fruits'");
                            var scanSpec = new ScanSpec("apple");
                            using (var scanner = table.CreateScanner(scanSpec)) {
                                Cell cell;
                                while (scanner.Next(out cell)) {
                                    Console.WriteLine(cell);
                                }
                            }

                            Console.WriteLine();

                            Console.WriteLine("Select all 'vitamins' for the 'banana'");
                            scanSpec = new ScanSpec("banana").AddColumn("vitamins");
                            using (var scanner = table.CreateScanner(scanSpec)) {
                                var cell = new Cell();
                                while (scanner.Move(cell)) { // caution, re-use cell instance
                                    Console.WriteLine("{0} {1} {2}mg", cell.Key.Row, cell.Key.Column, BitConverter.ToDouble(cell.Value, 0));
                                }
                            }

                            Console.WriteLine();
                        }

                        // Drop table
                        Console.WriteLine("Drop table 'fruits'");
                        ns.DropTable("fruits");
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine();
                Console.WriteLine(e);
            }
        }

        #endregion
    }
}