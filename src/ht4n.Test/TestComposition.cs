/** -*- C# -*-
 * Copyright (C) 2010-2012 Thalmann Software & Consulting, http://www.softdev.ch
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
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Configuration;
    using System.Reflection;

    using Hypertable;
    using Hypertable.Composition;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [ExportContextProvider("TestContextProvider")]
    public class CustomContextProvider : IContextProvider
    {
        #region Public Properties

        /// <summary>
        /// Gets context provider.
        /// </summary>
        public Func<IDictionary<string, object>, IContext> Provider {
            get {
                return (properties) => new Context();
            }
        }

        #endregion

        private class Context : IContext
        {
            #region Public Properties

            public bool IsDisposed {
                get {
                    throw new NotImplementedException();
                }
            }

            public IDictionary<string, object> Properties {
                get {
                    throw new NotImplementedException();
                }
            }

            #endregion

            #region Public Methods

            public IClient CreateClient() {
                return null;
            }

            public void Dispose() {
            }

            public bool HasFeature(ContextFeature contextFeature) {
                throw new NotImplementedException();
            }

            #endregion
        }
    }

    /// <summary>
    /// Test Composition.
    /// </summary>
    [TestClass]
    public class TestComposition
    {
        #region Constants and Fields

        private static string connectionString;

#pragma warning disable 0649

        [Import(typeof(IContextFactory))]
        private IContextFactory contextFactory;

#pragma warning restore

        #endregion

        #region Public Methods

        [ClassInitialize]
        public static void AssemblyInitialize(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext) {
            connectionString = ConfigurationManager.AppSettings["ConnectionString"].Trim();
        }

        [TestMethod]
        public void ContextProvider() {
            var properties = new Dictionary<string, object> { { "Hypertable.Composition.ComposablePartCatalogs", new AssemblyCatalog(Assembly.GetAssembly(this.GetType())) } };
            using (var context = Context.Create("Provider=TestContextProvider", properties)) {
                Assert.IsNull(context.CreateClient());
            }
        }

        [TestMethod]
        public void ImportContextFactory() {
            using (var catalog = new AggregateCatalog()) {
                catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetAssembly(typeof(IContextFactory))));
                using (var container = new CompositionContainer(catalog)) {
                    container.ComposeParts(this);

                    Assert.IsNotNull(this.contextFactory);
                    using (var context = this.contextFactory.Create(connectionString)) {
                        using (var client = context.CreateClient()) {
                            Assert.IsTrue(client.NamespaceExists("/"));
                        }
                    }
                }
            }
        }

        #endregion
    }
}