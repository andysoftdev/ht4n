/** -*- C# -*-
 * Copyright (C) 2011 Andy Thalmann
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
    using System.Configuration;
    using System.Diagnostics;
    using System.Reflection;
    using System.Text.RegularExpressions;

    using Hypertable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Base class for most of the ht4n test, provides some common methods/properties
    /// and reads the appconfig.
    /// </summary>
    [TestClass]
    public abstract class TestBase
    {
        #region Constants and Fields

        private static Client client;

        private static Context ctx;

        private static Namespace ns;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Hypertable client.
        /// </summary>
        protected static Client Client {
            get {
                return client ?? (client = Ctx.CreateClient());
            }
        }

        /// <summary>
        /// Gets the Hypertable context.
        /// </summary>
        protected static Context Ctx {
            get {
                return ctx ?? (ctx = Context.Create(CtxKind, Host));
            }
        }

        /// <summary>
        ///   Gets the 'test' context kind.
        /// </summary>
        protected static ContextKind CtxKind { get; private set; }

        /// <summary>
        /// Gets the 'test' Hypertable host name.
        /// </summary>
        protected static string Host { get; private set; }

        /// <summary>
        /// Gets the Hypertable namepace.
        /// </summary>
        protected static Namespace Ns {
            get {
                return ns ?? (ns = Client.OpenNamespace(NsName, OpenDispositions.OpenAlways));
            }
        }

        /// <summary>
        /// Gets the 'test' namespace name.
        /// </summary>
        protected static string NsName { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Assembly clean up, drop 'test' namespace.
        /// </summary>
        [AssemblyCleanup]
        public static void AssemblyCleanup() {
            if (ns != null) {
                ns.Dispose();
                ns = null;
            }

            if (client != null) {
                client.DropNamespace(NsName, DropDispositions.Complete);
                client.Dispose();
            }

            if (ctx != null) {
                ctx.Dispose();
            }
        }

        /// <summary>
        /// Initialize the assembly, read the appconfig.
        /// </summary>
        /// <param name = "testContext">The tets context.</param>
        [AssemblyInitialize]
        public static void AssemblyInitialize(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext) {
            NsName = ConfigurationManager.AppSettings["TestNamespace"].Trim();
            Assert.IsFalse(string.IsNullOrEmpty(NsName));
            Assert.AreNotEqual(NsName, "/"); // avoid using root namespace

            Host = ConfigurationManager.AppSettings["TestHost"].Trim();
            Assert.IsFalse(string.IsNullOrEmpty(Host));

            CtxKind = (ContextKind)Enum.Parse(typeof(ContextKind), ConfigurationManager.AppSettings["TestContextKind"].Trim());

            Logging.Logfile = Assembly.GetAssembly(typeof(TestBase)).Location + ".log";
            Logging.LogMessagePublished = message => Trace.WriteLine(message);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes all cells in the table specified.
        /// </summary>
        /// <param name = "table">Table.</param>
        protected static void Delete(Table table) {
            using (var scanner = table.CreateScanner(new ScanSpec { KeysOnly = true })) {
                using (var mutator = table.CreateMutator()) {
                    var cell = new Cell();
                    while (scanner.Next(cell)) {
                        mutator.Delete(cell.Key);
                    }
                }
            }
        }

        /// <summary>
        /// Deletes all cells in the table, column family specified.
        /// </summary>
        /// <param name = "table">Table.</param>
        /// <param name = "cf">Column family.</param>
        protected static void DeleteColumnFamily(Table table, string cf) {
            using (var scanner = table.CreateScanner(new ScanSpec { KeysOnly = true }.AddColumn(cf))) {
                using (var mutator = table.CreateMutator()) {
                    var cell = new Cell();
                    while (scanner.Next(cell)) {
                        mutator.Delete(cell.Key);
                    }
                }
            }
        }

        /// <summary>
        /// Drop tables from 'test' namespace.
        /// </summary>
        /// <param name = "regex">Table anme regular expression.</param>
        protected static void DropTables(Regex regex) {
            Ns.DropTables(regex, DropDispositions.IfExists);
        }

        /// <summary>
        /// Creates and opens a table in the 'test' namespace, drops existing table. Using the specified type name as the table name.
        /// </summary>
        /// <param name = "type">Table name (type.Name).</param>
        /// <param name = "schema">Table xml schema.</param>
        /// <returns>Opend table.</returns>
        protected static Table EnsureTable(Type type, string schema) {
            return EnsureTable(type.Name, schema);
        }

        /// <summary>
        /// Creates and opens a table in the 'test' namespace, drops existing table.
        /// </summary>
        /// <param name = "tableName">Table name.</param>
        /// <param name = "schema">Table xml schema.</param>
        /// <returns>Opend table.</returns>
        protected static Table EnsureTable(string tableName, string schema) {
            return Ns.OpenTable(tableName, schema, OpenDispositions.CreateAlways);
        }

        #endregion
    }
}