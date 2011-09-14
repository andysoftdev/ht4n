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
    using System.Linq;

    using Hypertable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test Hypertable namespaces.
    /// </summary>
    [TestClass]
    public class TestNamespace : TestBase
    {
        #region Public Methods

        [TestMethod]
        public void CreateOpenDropNamespaceA() {
            using (var client = Ctx.CreateClient()) {
                try {
                    client.OpenNamespace("ns_does_not_exists");
                    Assert.Fail();
                }
                catch (NamespaceDoesNotExistsException) {
                }
                catch {
                    Assert.Fail();
                }

                string[] namespaces = { "test-1", "Test-1", "/test-2", "/test-3/" };

                foreach (string name in namespaces) {
                    client.DropNamespace(name, DropDispositions.IfExists);
                    Assert.IsFalse(client.NamespaceExists(name));
                    client.CreateNamespace(name);
                    Assert.IsTrue(client.NamespaceExists(name));
                    Assert.IsFalse(client.NamespaceExists(name.ToUpper())); // case sensitive
                    using (var ns = client.OpenNamespace(name)) {
                        Assert.AreEqual(ns.Name, name.Trim('/'));
                        Assert.AreEqual(0, ns.Namespaces.Count);
                        Assert.AreEqual(0, ns.Tables.Count);
                    }
                    client.DropNamespace(name);
                    Assert.IsFalse(client.NamespaceExists(name));
                }
            }
        }

        [TestMethod]
        public void CreateOpenDropNamespaceB() {
            using (var client = Ctx.CreateClient()) {
                string[] namespaces = { "/test-x", "/test-x/1", "/test-x/1/2", "/test-x/1/2/3" };

                client.DropNamespace("/test-x", DropDispositions.Complete);
                foreach (string name in namespaces) {
                    Assert.IsFalse(client.NamespaceExists(name));
                    client.CreateNamespace(name);
                    Assert.IsTrue(client.NamespaceExists(name));
                    Assert.IsFalse(client.NamespaceExists(name.ToUpper())); // case sensitive
                    using (var ns = client.OpenNamespace(name)) {
                        Assert.AreEqual(ns.Name, name.Trim('/'));
                    }
                }

                for (int n = 0; n < namespaces.Length - 1; ++n) {
                    using (var ns = client.OpenNamespace(namespaces[n])) {
                        var nsListing = ns.GetListing(true);
                        Assert.AreEqual(ns.Name, nsListing.FullName);
                        Assert.IsTrue(ns.Name.EndsWith(nsListing.Name));
                        Assert.AreEqual(1, nsListing.Namespaces.Count);
                        Assert.AreEqual(nsListing.Namespaces[0].FullName, namespaces[n + 1].Trim('/'));

                        for (int m = n + 1; m < namespaces.Length - 1; ++m) {
                            nsListing = nsListing.Namespaces[0];
                            Assert.AreEqual(1, nsListing.Namespaces.Count);
                            Assert.AreEqual(nsListing.Namespaces[0].FullName, namespaces[m + 1].Trim('/'));
                        }
                    }
                }

                using (var ns = client.OpenNamespace("/")) {
                    var nsListing = ns.GetListing(true);
                    Assert.IsNull(nsListing.Parent);
                    foreach (var nsSubListing in nsListing.Namespaces) {
                        Assert.AreSame(nsListing, nsSubListing.Parent);
                    }
                }

                foreach (string name in namespaces.Reverse()) {
                    client.DropNamespace(name);
                    Assert.IsFalse(client.NamespaceExists(name));
                }
            }
        }

        [TestMethod]
        public void CreateOpenDropNamespaceC() {
            using (var client = Ctx.CreateClient()) {
                string[] namespaces = { "/test-x/1/2", "/test-x/1/2/3/4/5/6" };

                client.DropNamespace("/test-x", DropDispositions.Complete);
                foreach (string name in namespaces) {
                    Assert.IsFalse(client.NamespaceExists(name));
                    client.CreateNamespace(name, CreateDispositions.CreateIntermediate);
                    Assert.IsTrue(client.NamespaceExists(name));
                    Assert.IsFalse(client.NamespaceExists(name.ToUpper())); // case sensitive
                    using (var ns = client.OpenNamespace(name)) {
                        Assert.AreEqual(ns.Name, name.Trim('/'));
                    }
                }
                client.DropNamespace("/test-x", DropDispositions.Complete);
                Assert.IsFalse(client.NamespaceExists("/test-x"));
            }
        }

        [TestMethod]
        public void CreateOpenDropNamespaceUseBaseNamespace() {
            using (var client = Ctx.CreateClient()) {
                string[] namespacesA = { "/test-x", "/test-y" };

                string[] namespacesB = { "/B1", "B2/" };

                string[] namespacesC = { "/C1/1/2", "C2/1/2/" };

                foreach (string nameA in namespacesA) {
                    client.DropNamespace(nameA, DropDispositions.Complete);
                    Assert.IsFalse(client.NamespaceExists(nameA));
                    client.CreateNamespace(nameA);
                    Assert.IsTrue(client.NamespaceExists(nameA));
                    Assert.IsFalse(client.NamespaceExists(nameA.ToUpper())); // case sensitive
                    using (var nsA = client.OpenNamespace(nameA)) {
                        Assert.AreEqual(nsA.Name, nameA.Trim('/'));

                        foreach (string nameB in namespacesB) {
                            string name = nameA + "/" + nameB;
                            Assert.IsFalse(client.NamespaceExists(name));
                            client.CreateNamespace(nameB, nsA);
                            Assert.IsTrue(client.NamespaceExists(nameB, nsA));

                            using (var nsB = client.OpenNamespace(nameB, nsA)) {
                                Assert.AreEqual(nsB.Name, name.Replace("//", "/").Trim('/'));

                                foreach (string nameC in namespacesC) {
                                    name = nameA + "/" + nameB + "/" + nameC;
                                    Assert.IsFalse(client.NamespaceExists(name));
                                    client.CreateNamespace(nameC, nsB, CreateDispositions.CreateIntermediate);
                                    Assert.IsTrue(client.NamespaceExists(nameC, nsB));

                                    using (var nsC = client.OpenNamespace(nameC, nsB)) {
                                        Assert.AreEqual(nsC.Name, name.Replace("//", "/").Replace("//", "/").Trim('/'));
                                    }
                                }
                            }
                        }

                        var nsChildren = nsA.Namespaces;
                        Assert.AreEqual(namespacesB.Length, nsChildren.Count);
                        foreach (string nschild in nsChildren) {
                            Assert.IsTrue(namespacesB.Where(ns => ns.Contains(nschild)).Count() == 1);
                        }
                    }
                }

                foreach (string name in namespacesA) {
                    client.DropNamespace(name, DropDispositions.Complete);
                    Assert.IsFalse(client.NamespaceExists(name));
                }
            }
        }

        [TestMethod]
        public void CreateOpenDropNamespaceWithBackslashes() {
            using (var client = Ctx.CreateClient()) {
                string[] namespaces = { @"\", @"\\", @"\test", @"test\", @"\test\", @"\test\1", @"test\1", @"test\1\", @"\test\1\" };

                foreach (string name in namespaces) {
                    client.DropNamespace(name, DropDispositions.IfExists);
                    Assert.IsFalse(client.NamespaceExists(name));
                    client.CreateNamespace(name);
                    Assert.IsTrue(client.NamespaceExists(name));
                    using (var ns = client.OpenNamespace(name)) {
                        Assert.AreEqual(ns.Name, name.Trim('/'));
                        Assert.AreEqual(0, ns.Namespaces.Count);
                        Assert.AreEqual(0, ns.Tables.Count);
                    }
                    client.DropNamespace(name);
                    Assert.IsFalse(client.NamespaceExists(name));
                }
            }
        }

        [TestInitialize]
        public void TestInitialize() {
            Ns.DropTables();
            Ns.DropNamespaces(DropDispositions.Complete);
        }

        #endregion
    }
}
