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
    using System.Collections.Generic;
    using System.Configuration;
    using System.Threading;

    using Hypertable;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test Context.
    /// </summary>
    [TestClass]
    public class TestContext
    {
        #region Constants and Fields

        private static string host;

        #endregion

        #region Public Methods

        [ClassInitialize]
        public static void AssemblyInitialize(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext) {
            host = ConfigurationManager.AppSettings["TestHost"].Trim();
            Assert.IsFalse(string.IsNullOrEmpty(host));
        }

        [TestMethod]
        [DeploymentItem(@"..\ht4n\src\ht4n.Test\conf\TestConfiguration.cfg")]
        public void CreateContexWithConfigurationFile() {
            using (var ctx = Context.Create(ContextKind.Hyper, "--config TestConfiguration.cfg", false)) {
                Assert.AreEqual(ctx.Properties["ThriftBroker.Timeout"], 60000);
            }
        }

        [TestMethod]
        public void CreateContext() {
            using (var ctx = Context.Create(ContextKind.Hyper)) {
                Assert.AreEqual(ctx.ContextKind, ContextKind.Hyper);
                Assert.IsInstanceOfType(ctx.Properties["hs-host"], typeof(IList<string>));
                Assert.AreEqual(1, ((IList<string>)ctx.Properties["hs-host"]).Count);
                Assert.AreEqual("localhost", ((IList<string>)ctx.Properties["hs-host"])[0]);
                Assert.AreEqual((ushort)38040, ctx.Properties["hs-port"]);
            }

            using (var ctx = Context.Create(ContextKind.Thrift)) {
                Assert.AreEqual(ContextKind.Thrift, ctx.ContextKind);
                Assert.AreEqual("localhost", ctx.Properties["thrift-host"]);
                Assert.AreEqual((ushort)38080, ctx.Properties["thrift-port"]);
            }
        }

        [TestMethod]
        public void CreateContextWithCmdLine() {
            using (var ctx = Context.Create(ContextKind.Hyper, "--ThriftBroker.Workers=10 --ThriftBroker.Port=12345 --verbose", false)) {
                Assert.AreEqual(ctx.Properties["ThriftBroker.Workers"], 10);
                Assert.AreEqual(ctx.Properties["thrift-port"], (ushort)12345);
                Assert.AreEqual(ctx.Properties["Hypertable.Verbose"], true);
            }

            using (var ctx = Context.Create(ContextKind.Thrift, "\"--thrift-broker\" \"qwerty:42345\" \"--verbose\"", false)) {
                Assert.AreEqual(ctx.Properties["thrift-host"], "qwerty");
                Assert.AreEqual(ctx.Properties["thrift-port"], (ushort)42345);
                Assert.AreEqual(ctx.Properties["Hypertable.Verbose"], true);
            }

            using (var ctx = Context.Create(ContextKind.Hyper, "\"" + Environment.GetCommandLineArgs()[0] + "\" --thrift-broker qwerty:52345 --verbose", true)) {
                Assert.AreEqual(ctx.Properties["thrift-host"], "qwerty");
                Assert.AreEqual(ctx.Properties["thrift-port"], (ushort)52345);
                Assert.AreEqual(ctx.Properties["Hypertable.Verbose"], true);
            }
        }

        [TestMethod]
        public void CreateContextWithHost() {
            using (var ctx = Context.Create(ContextKind.Hyper, "localhost", 1000)) {
                Assert.AreEqual(ctx.ContextKind, ContextKind.Hyper);
                Assert.IsInstanceOfType(ctx.Properties["hs-host"], typeof(IList<string>));
                Assert.AreEqual(1, ((IList<string>)ctx.Properties["hs-host"]).Count);
                Assert.AreEqual(((IList<string>)ctx.Properties["hs-host"])[0], "localhost");
                Assert.AreEqual(ctx.Properties["hs-port"], (ushort)1000);
            }

            using (var ctx = Context.Create(ContextKind.Thrift, "localhost", 1000)) {
                Assert.AreEqual(ctx.ContextKind, ContextKind.Thrift);
                Assert.AreEqual(ctx.Properties["thrift-host"], "localhost");
                Assert.AreEqual(ctx.Properties["thrift-port"], (ushort)1000);
            }
        }

        [TestMethod]
        public void CreateContextWithProperties() {
            IDictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("Hypertable.Silent", true);
            properties.Add("Hypertable.Logging.Level", "debug");
            properties.Add("Hypertable.Client.Workers", 5);
            properties.Add("DfsBroker.Port", 1234);
            properties.Add("Hypertable.RangeServer.MemoryLimit", 128000000L);
            properties.Add("false-positive", 0.0123);
            properties.Add("Hyperspace.Replica.Host", new[] { "abc", "xyz" });

            properties.Add("i16", (ushort)66);
            properties.Add("i32", 4565);
            properties.Add("i64", 235346L);
            properties.Add("f64", 12.4567);
            properties.Add("boo", true);
            properties.Add("str", "a string arg");

            properties.Add("i32s", new[] { 1, 2, 333333 });
            properties.Add("i64s", new long[] { 1, 2, 333333 });
            properties.Add("f64s", new[] { 1.1, 2.1, 333333.1 });
            properties.Add("strs", "a list of strings".Split(' '));

            using (var ctx = Context.Create(ContextKind.Hyper, properties)) {
                Assert.AreEqual(ctx.Properties["Hypertable.Silent"], true);
                Assert.AreEqual(ctx.Properties["Hypertable.Logging.Level"], "debug");
                Assert.AreEqual(ctx.Properties["Hypertable.Client.Workers"], 5);
                Assert.AreEqual(ctx.Properties["DfsBroker.Port"], (ushort)1234);
                Assert.AreEqual(ctx.Properties["Hypertable.RangeServer.MemoryLimit"], 128000000L);
                Assert.AreEqual(ctx.Properties["false-positive"], 0.0123);

                Assert.IsInstanceOfType(ctx.Properties["Hyperspace.Replica.Host"], typeof(IList<string>));
                Assert.AreEqual(2, ((IList<string>)ctx.Properties["Hyperspace.Replica.Host"]).Count);
                Assert.AreEqual(((IList<string>)ctx.Properties["Hyperspace.Replica.Host"])[0], "abc");
                Assert.AreEqual(((IList<string>)ctx.Properties["Hyperspace.Replica.Host"])[1], "xyz");

                Assert.AreEqual(ctx.Properties["i16"], (ushort)66);
                Assert.AreEqual(ctx.Properties["i32"], 4565);
                Assert.AreEqual(ctx.Properties["i64"], 235346L);
                Assert.AreEqual(ctx.Properties["f64"], 12.4567);
                Assert.AreEqual(ctx.Properties["boo"], true);
                Assert.AreEqual(ctx.Properties["str"], "a string arg");

                Assert.IsInstanceOfType(ctx.Properties["i32s"], typeof(IList<int>));
                Assert.AreEqual(3, ((IList<int>)ctx.Properties["i32s"]).Count);
                Assert.AreEqual(((IList<int>)ctx.Properties["i32s"])[0], 1);
                Assert.AreEqual(((IList<int>)ctx.Properties["i32s"])[1], 2);
                Assert.AreEqual(((IList<int>)ctx.Properties["i32s"])[2], 333333);

                Assert.IsInstanceOfType(ctx.Properties["i64s"], typeof(IList<long>));
                Assert.AreEqual(3, ((IList<long>)ctx.Properties["i64s"]).Count);
                Assert.AreEqual(((IList<long>)ctx.Properties["i64s"])[0], 1L);
                Assert.AreEqual(((IList<long>)ctx.Properties["i64s"])[1], 2L);
                Assert.AreEqual(((IList<long>)ctx.Properties["i64s"])[2], 333333L);

                Assert.IsInstanceOfType(ctx.Properties["f64s"], typeof(IList<double>));
                Assert.AreEqual(3, ((IList<double>)ctx.Properties["f64s"]).Count);
                Assert.AreEqual(((IList<double>)ctx.Properties["f64s"])[0], 1.1);
                Assert.AreEqual(((IList<double>)ctx.Properties["f64s"])[1], 2.1);
                Assert.AreEqual(((IList<double>)ctx.Properties["f64s"])[2], 333333.1);

                Assert.IsInstanceOfType(ctx.Properties["strs"], typeof(IList<string>));
                Assert.AreEqual(4, ((IList<string>)ctx.Properties["strs"]).Count);
                Assert.AreEqual(((IList<string>)ctx.Properties["strs"])[0], "a");
                Assert.AreEqual(((IList<string>)ctx.Properties["strs"])[1], "list");
                Assert.AreEqual(((IList<string>)ctx.Properties["strs"])[2], "of");
                Assert.AreEqual(((IList<string>)ctx.Properties["strs"])[3], "strings");
            }
        }

        [TestMethod]
        public void MultipleClients() {
            using (var ctx = Context.Create(ContextKind.Hyper, host)) {
                for (int i = 0; i < 5; ++i) {
                    using (var client = ctx.CreateClient()) {
                        Assert.IsTrue(client.NamespaceExists("/sys"));
                    }
                }

                for (int i = 0; i < 5; ++i) {
                    using (var clientA = ctx.CreateClient())
                    using (var clientB = ctx.CreateClient()) {
                        Assert.IsTrue(clientA.NamespaceExists("/sys"));
                        Assert.IsTrue(clientB.NamespaceExists("/sys"));
                    }
                }
            }

            Thread.Sleep(200);

            using (var ctx = Context.Create(ContextKind.Thrift, host)) {
                for (int i = 0; i < 5; ++i) {
                    using (var client = ctx.CreateClient()) {
                        Assert.IsTrue(client.NamespaceExists("/sys"));
                    }
                }

                for (int i = 0; i < 5; ++i) {
                    using (var clientA = ctx.CreateClient())
                    using (var clientB = ctx.CreateClient()) {
                        Assert.IsTrue(clientA.NamespaceExists("/sys"));
                        Assert.IsTrue(clientB.NamespaceExists("/sys"));
                    }
                }
            }

            Thread.Sleep(200);
        }

        [TestMethod]
        public void Reconnect() {
            for (int i = 0; i < 5; ++i) {
                using (var ctx = Context.Create(ContextKind.Hyper, host)) {
                    using (var client = ctx.CreateClient()) {
                        Assert.IsTrue(client.NamespaceExists("/sys"));
                    }
                }

                Thread.Sleep(200);

                using (var ctx = Context.Create(ContextKind.Thrift, host)) {
                    using (var client = ctx.CreateClient()) {
                        Assert.IsTrue(client.NamespaceExists("/sys"));
                    }
                }

                Thread.Sleep(200);
            }
        }

        #endregion
    }
}