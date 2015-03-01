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
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    using Hypertable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Base class for most of the ht4n test, provides some common methods/properties
    /// and reads the app config.
    /// </summary>
    [TestClass]
    public abstract class TestBase
    {
        #region Constants and Fields

        private static IClient client;

        private static IContext context;

        private static INamespace ns;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Hypertable client.
        /// </summary>
        protected static IClient Client {
            get {
                return client ?? (client = Context.CreateClient());
            }
        }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        protected static string ConnectionString { get; private set; }

        /// <summary>
        /// Gets the Hypertable context.
        /// </summary>
        protected static IContext Context {
            get {
                return context ?? (context = Hypertable.Context.Create(ConnectionString));
            }
        }

        /// <summary>
        /// Returns true if the actual provider supports asynchronous table mutator.
        /// </summary>
        protected static bool HasAsyncTableMutator {
            get {
                return Context.HasFeature(ContextFeature.AsyncTableMutator);
            }
        }

        /// <summary>
        /// Returns true if the actual provider supports asynchronous table scanner.
        /// </summary>
        protected static bool HasAsyncTableScanner {
            get {
                return Context.HasFeature(ContextFeature.AsyncTableScanner);
            }
        }

        /// <summary>
        /// Returns true if the actual provider supports HQL.
        /// </summary>
        protected static bool HasHQL {
            get {
                return Context.HasFeature(ContextFeature.HQL);
            }
        }

        /// <summary>
        /// Returns true if the actual provider supports periodic flush table mutator.
        /// </summary>
        protected static bool HasPeriodicFlushTableMutator {
            get {
                return Context.HasFeature(ContextFeature.PeriodicFlushTableMutator);
            }
        }

        /// <summary>
        /// Returns true if the current provider is the hamsterdb provider.
        /// </summary>
        protected static bool IsHamster {
            get {
                return Equals("Hamster", ProviderName);
            }
        }

        /// <summary>
        /// Returns true if the current provider is the hypertable native provider.
        /// </summary>
        protected static bool IsHyper {
            get {
                return Equals("Hyper", ProviderName);
            }
        }

        /// <summary>
        /// Returns true if the current provider is the hypertable thrift provider.
        /// </summary>
        protected static bool IsThrift {
            get {
                return Equals("Thrift", ProviderName);
            }
        }

        /// <summary>
        /// Returns true if the current provider is the sqlite provider.
        /// </summary>
        protected static bool IsSQlite {
            get {
                return Equals("SQlite", ProviderName);
            }
        }

        /// <summary>
        /// Returns true if the current provider is the odbc provider.
        /// </summary>
        protected static bool IsOdbc
        {
            get
            {
                return Equals("Odbc", ProviderName);
            }
        }

        /// <summary>
        /// Gets the Hypertable namepace.
        /// </summary>
        protected static INamespace Ns {
            get {
                return ns ?? (ns = Client.OpenNamespace(NsName, OpenDispositions.OpenAlways));
            }
        }

        /// <summary>
        /// Gets the namespace name.
        /// </summary>
        protected static string NsName { get; private set; }

        /// <summary>
        /// Returns the current provider name.
        /// </summary>
        protected static string ProviderName {
            get {
                return (string)context.Properties["Ht4n.Provider"];
            }
        }

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

            if (context != null) {
                context.Dispose();
            }
        }

        /// <summary>
        /// Initialize the assembly, read the appconfig.
        /// </summary>
        /// <param name = "testContext">The test context.</param>
        [AssemblyInitialize]
        public static void AssemblyInitialize(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext) {
            ConnectionString = ConfigurationManager.AppSettings["ConnectionString"].Trim();
            NsName = ConfigurationManager.AppSettings["Namespace"].Trim();
            Assert.IsFalse(string.IsNullOrEmpty(NsName));
            Assert.AreNotEqual(NsName, "/"); // avoid using root namespace
            Logging.Logfile = Assembly.GetAssembly(typeof(TestBase)).Location + ".log";
            Logging.LogMessagePublished = message => Trace.WriteLine(message);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Preventing the system from entering sleep.
        /// </summary>
        protected static void ContinueExecution() {
            NativeMethods.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_AWAYMODE_REQUIRED);
        }

        /// <summary>
        /// Deletes all cells in the table specified.
        /// </summary>
        /// <param name = "table">Table.</param>
        protected static void Delete(ITable table) {
            using (var scanner = table.CreateScanner(new ScanSpec { KeysOnly = true })) {
                using (var mutator = table.CreateMutator()) {
                    var cell = new Cell();
                    while( scanner.Move(cell) ) {
                        mutator.Delete(cell.Key);
                    }
                }
            }

            var c = 0;
            using (var scanner = table.CreateScanner()) {
                var cell = new Cell();
                while( scanner.Move(cell) ) {
                    ++c;
                }
            }

            Assert.AreEqual(0, c);
        }

        /// <summary>
        /// Deletes all cells in the table, column family specified.
        /// </summary>
        /// <param name = "table">Table.</param>
        /// <param name = "cf">Column family.</param>
        protected static void DeleteColumnFamily(ITable table, string cf) {
            var scanSpec = new ScanSpec { KeysOnly = true }.AddColumn(cf);
            using (var scanner = table.CreateScanner(scanSpec)) {
                using (var mutator = table.CreateMutator()) {
                    var cell = new Cell();
                    while( scanner.Move(cell) ) {
                        mutator.Delete(cell.Key);
                    }
                }
            }
        }

        /// <summary>
        /// Drop namespaces from 'test' namespace.
        /// </summary>
        /// <param name = "regex">Namespace name regular expression.</param>
        protected static void DropNamespaces(Regex regex)
        {
            Ns.DropNamespaces(regex, DropDispositions.IfExists);
        }

        /// <summary>
        /// Drop tables from 'test' namespace.
        /// </summary>
        /// <param name = "regex">Table name regular expression.</param>
        protected static void DropTables(Regex regex) {
            Ns.DropTables(regex, DropDispositions.IfExists);
        }

        /// <summary>
        /// Creates and opens a table in the 'test' namespace, drops existing table. Using the specified type name as the table name.
        /// </summary>
        /// <param name = "type">Table name (type.Name).</param>
        /// <param name = "schema">Table xml schema.</param>
        /// <returns>Opened table.</returns>
        protected static ITable EnsureTable(Type type, string schema) {
            return EnsureTable(type.Name, schema);
        }

        /// <summary>
        /// Creates and opens a table in the 'test' namespace, drops existing table.
        /// </summary>
        /// <param name = "tableName">Table name.</param>
        /// <param name = "schema">Table xml schema.</param>
        /// <returns>Opened table.</returns>
        protected static ITable EnsureTable(string tableName, string schema) {
            return Ns.OpenTable(tableName, schema, OpenDispositions.CreateAlways);
        }

        /// <summary>
        /// Randomly shuffle the enumerable specified.
        /// </summary>
        /// <param name="enumerable">
        /// The enumerable.
        /// </param>
        /// <returns>
        /// The shuffled enumerable.
        /// </returns>
        protected static IEnumerable<T> Shuffle<T>(IEnumerable<T> enumerable) {
            if (enumerable == null) {
                throw new ArgumentNullException("enumerable");
            }

            var random = new Random((int)DateTime.Now.Ticks ^ enumerable.GetHashCode());
            return enumerable.Select(t => Tuple.Create(random.Next(), t)).OrderBy(pair => pair.Item1).Select(pair => pair.Item2).ToList();
        }

        #endregion
    }
}