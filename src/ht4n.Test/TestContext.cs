﻿/** -*- C# -*-
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

        private static string connectionString;

        #endregion

        #region Public Methods

        [ClassInitialize]
        public static void AssemblyInitialize(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext) {
            connectionString = ConfigurationManager.AppSettings["ConnectionString"].Trim();
        }

        [TestMethod]
        [DeploymentItem(@"..\ht4n\src\ht4n.Test\conf\TestConfiguration.cfg")]
        public void CreateContexConfigurationFile() {
            using (var context = Context.Create("--config TestConfiguration.cfg")) {
                Assert.AreEqual("Thrift", context.Properties["Hypertable.Client.Provider"]);
                Assert.AreEqual("Thrift", context.Properties["Provider"]);
                Assert.AreEqual("net.tcp://google.com", context.Properties["Hypertable.Client.Uri"]);
                Assert.AreEqual("net.tcp://google.com", context.Properties["Uri"]);
                Assert.AreEqual(60000, context.Properties["ThriftBroker.Timeout"]);
                Assert.AreEqual(60000, context.Properties["thrift-timeout"]);

                Assert.AreEqual("Unknown", context.Properties["X.Y.String"]);
                Assert.AreEqual("Yes", context.Properties["X.Y.Bool"]);
                Assert.AreEqual("32", context.Properties["X.Y.Int"]);
            }
        }

        [TestMethod]
        public void CreateContextConnectionString() {
            try {
                using (Context.Create("Provider=")) {
                    Assert.Fail();
                }
            }
            catch( ArgumentException) {
            }

            try {
                using (Context.Create("Provider=Unknown")) {
                    Assert.Fail();
                }
            }
            catch (ArgumentException) {
            }

            using (var context = Context.Create(string.Empty)) {
                Assert.AreEqual("Hyper", context.Properties["Provider"]);
                Assert.AreEqual("Hyper", context.Properties["Hypertable.Client.Provider"]);
                Assert.IsInstanceOfType(context.Properties["hs-host"], typeof(IList<string>));
                Assert.AreEqual(1, ((IList<string>)context.Properties["hs-host"]).Count);
                Assert.AreEqual("localhost", ((IList<string>)context.Properties["hs-host"])[0]);
                Assert.AreEqual((ushort)38040, context.Properties["hs-port"]);
                Assert.AreEqual("localhost", context.Properties["thrift-host"]);
                Assert.AreEqual((ushort)38080, context.Properties["thrift-port"]);
            }

            using (var context = Context.Create("Provider=Hyper")) {
                Assert.AreEqual("Hyper", context.Properties["Provider"]);
                Assert.AreEqual("Hyper", context.Properties["Hypertable.Client.Provider"]);
                Assert.IsInstanceOfType(context.Properties["hs-host"], typeof(IList<string>));
                Assert.AreEqual(1, ((IList<string>)context.Properties["hs-host"]).Count);
                Assert.AreEqual("localhost", ((IList<string>)context.Properties["hs-host"])[0]);
                Assert.AreEqual((ushort)38040, context.Properties["hs-port"]);
            }

            using (var context = Context.Create(" ;; ; ; ; ;;;Provider=Thrift ; ; ; ;;; ;")) {
                Assert.AreEqual("Thrift", context.Properties["Provider"]);
                Assert.AreEqual("Thrift", context.Properties["Hypertable.Client.Provider"]);
                Assert.AreEqual("localhost", context.Properties["thrift-host"]);
                Assert.AreEqual((ushort)38080, context.Properties["thrift-port"]);
            }

            using( var context = Context.Create("Provider=Hyper; Uri=net.tcp://google.com:1000") ) {
                Assert.AreEqual("Hyper", context.Properties["Provider"]);
                Assert.AreEqual("net.tcp://google.com:1000", context.Properties["Uri"]);
                Assert.AreEqual("net.tcp://google.com:1000", context.Properties["Hypertable.Client.Uri"]);
                Assert.IsInstanceOfType(context.Properties["hs-host"], typeof(IList<string>));
                Assert.AreEqual(1, ((IList<string>)context.Properties["hs-host"]).Count);
                Assert.AreEqual("google.com", ((IList<string>)context.Properties["hs-host"])[0]);
                Assert.AreEqual((ushort)1000, context.Properties["hs-port"]);
            }

            using( var context = Context.Create("Hypertable.Client.Provider=Hyper; Hypertable.Client.Uri net.tcp://google.com:1000") ) {
                Assert.AreEqual("Hyper", context.Properties["Provider"]);
                Assert.IsInstanceOfType(context.Properties["hs-host"], typeof(IList<string>));
                Assert.AreEqual(1, ((IList<string>)context.Properties["hs-host"]).Count);
                Assert.AreEqual("google.com", ((IList<string>)context.Properties["hs-host"])[0]);
                Assert.AreEqual((ushort)1000, context.Properties["hs-port"]);
            }

            using( var context = Context.Create("Provider=Hyper; Uri=net.tcp://127.0.0.1") ) {
                Assert.AreEqual("Hyper", context.Properties["Provider"]);
                Assert.IsInstanceOfType(context.Properties["hs-host"], typeof(IList<string>));
                Assert.AreEqual(1, ((IList<string>)context.Properties["hs-host"]).Count);
                Assert.AreEqual("127.0.0.1", ((IList<string>)context.Properties["hs-host"])[0]);
                Assert.AreEqual((ushort)38040, context.Properties["hs-port"]);
            }

            using( var context = Context.Create("Hypertable.Client.Provider=Thrift; Uri=net.tcp://google.com:1000") ) {
                Assert.AreEqual("Thrift", context.Properties["Provider"]);
                Assert.AreEqual("Thrift", context.Properties["Hypertable.Client.Provider"]);
                Assert.AreEqual("net.tcp://google.com:1000", context.Properties["Uri"]);
                Assert.AreEqual("net.tcp://google.com:1000", context.Properties["Hypertable.Client.Uri"]);
                Assert.AreEqual("google.com", context.Properties["thrift-host"]);
                Assert.AreEqual((ushort)1000, context.Properties["thrift-port"]);
            }

            using( var context = Context.Create("Provider=Thrift; Uri=net.tcp://127.0.0.1") ) {
                Assert.AreEqual("Thrift", context.Properties["Provider"]);
                Assert.AreEqual("127.0.0.1", context.Properties["thrift-host"]);
                Assert.AreEqual((ushort)38080, context.Properties["thrift-port"]);
            }

            using (var context = Context.Create("Hypertable.Client.Provider Hyper; hyperspace=qwerty:12345")) {
                Assert.IsInstanceOfType(context.Properties["hs-host"], typeof(IList<string>));
                Assert.AreEqual(1, ((IList<string>)context.Properties["hs-host"]).Count);
                Assert.AreEqual("qwerty", ((IList<string>)context.Properties["hs-host"])[0]);
                Assert.AreEqual((ushort)12345, context.Properties["hs-port"]);
            }

            using (var context = Context.Create("Hypertable.Client.Provider Thrift; thrift-broker=qwerty:12345")) {
                Assert.AreEqual("qwerty", context.Properties["thrift-host"]);
                Assert.AreEqual((ushort)12345, context.Properties["thrift-port"]);
            }

            using (var context = Context.Create("Hypertable.Client.Provider=Hyper; Hyperspace.Replica.Port=12345; verbose; X.Y.String=Unknown")) {
                Assert.AreEqual((ushort)12345, context.Properties["hs-port"]);
                Assert.AreEqual(true, context.Properties["Hypertable.Verbose"]);
                Assert.IsFalse(context.Properties.ContainsKey("X.Y.String"));
            }

            using (var context = Context.Create("\"--Hypertable.Client.Provider Thrift\" \"--thrift-broker\" \"qwerty:42345\" \"--verbose\" \"--X.Y.String\" \"Unknown\"")) {
                Assert.AreEqual("qwerty", context.Properties["thrift-host"]);
                Assert.AreEqual((ushort)42345, context.Properties["thrift-port"]);
                Assert.AreEqual(true, context.Properties["Hypertable.Verbose"]);
                Assert.IsFalse(context.Properties.ContainsKey("X.Y.String"));
            }

            using (var context = Context.Create("--Provider Thrift --thrift-broker qwerty:52345 --verbose")) {
                Assert.AreEqual("qwerty", context.Properties["thrift-host"]);
                Assert.AreEqual((ushort)52345, context.Properties["thrift-port"]);
                Assert.AreEqual(true, context.Properties["Hypertable.Verbose"]);
            }
        }

        [TestMethod]
        public void CreateContextProperties() {
            using (var context = Context.Create(new Dictionary<string, object>())) {
                Assert.AreEqual("Hyper", context.Properties["Provider"]);
                Assert.AreEqual("Hyper", context.Properties["Hypertable.Client.Provider"]);
                Assert.IsInstanceOfType(context.Properties["hs-host"], typeof(IList<string>));
                Assert.AreEqual(1, ((IList<string>)context.Properties["hs-host"]).Count);
                Assert.AreEqual("localhost", ((IList<string>)context.Properties["hs-host"])[0]);
                Assert.AreEqual((ushort)38040, context.Properties["hs-port"]);
                Assert.AreEqual("localhost", context.Properties["thrift-host"]);
                Assert.AreEqual((ushort)38080, context.Properties["thrift-port"]);
            }

            var properties = new Dictionary<string, object> { { "Hypertable.Client.Provider", "Hyper" }, { "Hypertable.Client.Uri", "net.tcp://google.com:1000" } };

            using (var context = Context.Create(properties)) {
                Assert.AreEqual("Hyper", context.Properties["Provider"]);
                Assert.AreEqual("Hyper", context.Properties["Hypertable.Client.Provider"]);
                Assert.AreEqual("net.tcp://google.com:1000", context.Properties["Uri"]);
                Assert.AreEqual("net.tcp://google.com:1000", context.Properties["Hypertable.Client.Uri"]);
                Assert.IsInstanceOfType(context.Properties["hs-host"], typeof(IList<string>));
                Assert.AreEqual(1, ((IList<string>)context.Properties["hs-host"]).Count);
                Assert.AreEqual("google.com", ((IList<string>)context.Properties["hs-host"])[0]);
                Assert.AreEqual((ushort)1000, context.Properties["hs-port"]);
            }

            properties = new Dictionary<string, object> { { "Hypertable.Client.Provider", "Thrift" }, { "Hypertable.Client.Uri", "net.tcp://google.com:1000" } };

            using (var context = Context.Create(properties)) {
                Assert.AreEqual("Thrift", context.Properties["Provider"]);
                Assert.AreEqual("Thrift", context.Properties["Hypertable.Client.Provider"]);
                Assert.AreEqual("net.tcp://google.com:1000", context.Properties["Uri"]);
                Assert.AreEqual("net.tcp://google.com:1000", context.Properties["Hypertable.Client.Uri"]);
                Assert.AreEqual("google.com", context.Properties["thrift-host"]);
                Assert.AreEqual((ushort)1000, context.Properties["thrift-port"]);
            }

            properties = new Dictionary<string, object> { { "Provider", "Hyper" }, { "Uri", "net.tcp://google.com:1000" } };

            using (var context = Context.Create(properties)) {
                Assert.AreEqual("Hyper", context.Properties["Provider"]);
                Assert.AreEqual("Hyper", context.Properties["Hypertable.Client.Provider"]);
                Assert.AreEqual("net.tcp://google.com:1000", context.Properties["Uri"]);
                Assert.AreEqual("net.tcp://google.com:1000", context.Properties["Hypertable.Client.Uri"]);
                Assert.IsInstanceOfType(context.Properties["hs-host"], typeof(IList<string>));
                Assert.AreEqual(1, ((IList<string>)context.Properties["hs-host"]).Count);
                Assert.AreEqual("google.com", ((IList<string>)context.Properties["hs-host"])[0]);
                Assert.AreEqual((ushort)1000, context.Properties["hs-port"]);
            }

            properties = new Dictionary<string, object> { { "Provider", "Thrift" }, { "Uri", "net.tcp://google.com:1000" } };

            using (var context = Context.Create(properties)) {
                Assert.AreEqual("Thrift", context.Properties["Provider"]);
                Assert.AreEqual("Thrift", context.Properties["Hypertable.Client.Provider"]);
                Assert.AreEqual("net.tcp://google.com:1000", context.Properties["Uri"]);
                Assert.AreEqual("net.tcp://google.com:1000", context.Properties["Hypertable.Client.Uri"]);
                Assert.AreEqual("google.com", context.Properties["thrift-host"]);
                Assert.AreEqual((ushort)1000, context.Properties["thrift-port"]);
            }

            properties = new Dictionary<string, object> { { "Provider", "Hyper" }, { "Uri", "net.tcp://google.com:1000" } };

            using (var context = Context.Create("Provider=Thrift, Uri=microsoft.com:1001", properties)) {
                Assert.AreEqual("Hyper", context.Properties["Provider"]);
                Assert.AreEqual("Hyper", context.Properties["Hypertable.Client.Provider"]);
                Assert.AreEqual("net.tcp://google.com:1000", context.Properties["Uri"]);
                Assert.AreEqual("net.tcp://google.com:1000", context.Properties["Hypertable.Client.Uri"]);
                Assert.IsInstanceOfType(context.Properties["hs-host"], typeof(IList<string>));
                Assert.AreEqual(1, ((IList<string>)context.Properties["hs-host"]).Count);
                Assert.AreEqual("google.com", ((IList<string>)context.Properties["hs-host"])[0]);
                Assert.AreEqual((ushort)1000, context.Properties["hs-port"]);
            }

            properties = new Dictionary<string, object> { { "Provider", "Thrift" }, { "Uri", "net.tcp://google.com:1000" } };

            using (var context = Context.Create("Provider=Hyper, Uri=microsoft.com:1001", properties)) {
                Assert.AreEqual("Thrift", context.Properties["Provider"]);
                Assert.AreEqual("Thrift", context.Properties["Hypertable.Client.Provider"]);
                Assert.AreEqual("net.tcp://google.com:1000", context.Properties["Uri"]);
                Assert.AreEqual("net.tcp://google.com:1000", context.Properties["Hypertable.Client.Uri"]);
                Assert.AreEqual("google.com", context.Properties["thrift-host"]);
                Assert.AreEqual((ushort)1000, context.Properties["thrift-port"]);
            }

            properties = new Dictionary<string, object> {
                    { "Hypertable.Client.Provider", "Thrift" }, 
                    { "Hypertable.Logging.Level", "debug" }
                };

            using( var context = Context.Create(properties) ) {
                Assert.AreEqual("debug", context.Properties["logging-level"]);
                Assert.AreEqual("debug", context.Properties["Hypertable.Logging.Level"]);
            }

            properties = new Dictionary<string, object> {
                    { "Hypertable.Client.Provider", "Thrift" }, 
                    { "Hypertable.Logging.Level", "crit" }
                };

            using( var context = Context.Create(properties) ) {
                Assert.AreEqual("crit", context.Properties["logging-level"]);
                Assert.AreEqual("crit", context.Properties["Hypertable.Logging.Level"]);
            }

            properties = new Dictionary<string, object> {
                    { "Hypertable.Client.Provider", "Thrift" }, 
                    { "Hypertable.Silent", true }, 
                    { "Hypertable.Logging.Level", "debug" }, 
                    { "Hypertable.Client.Workers", 5 }, 
                    { "DfsBroker.Port", (ushort)1234 }, 
                    { "Hypertable.RangeServer.MemoryLimit", 128000000L }, 
                    { "false-positive", 0.0123 }, 
                    { "Hyperspace.Replica.Host", new[] { "abc", "xyz" } }, 
                    { "i16", (ushort)66 }, 
                    { "i32", 4565 }, 
                    { "i64", 235346L }, 
                    { "f64", 12.4567 }, 
                    { "boo", true }, 
                    { "str", "a string arg" }, 
                    { "i32s", new[] { 1, 2, 333333 } }, 
                    { "i64s", new long[] { 1, 2, 333333 } }, 
                    { "f64s", new[] { 1.1, 2.1, 333333.1 } }, 
                    { "strs", "a list of strings".Split(' ') }
                };

            using (var context = Context.Create(properties)) {
                Assert.AreEqual("Thrift", context.Properties["Provider"]);
                Assert.AreEqual(true, context.Properties["Hypertable.Silent"]);
                Assert.AreEqual("debug", context.Properties["Hypertable.Logging.Level"]);
                Assert.AreEqual(5, context.Properties["Hypertable.Client.Workers"]);
                Assert.AreEqual((ushort)1234, context.Properties["DfsBroker.Port"]);
                Assert.AreEqual(128000000L, context.Properties["Hypertable.RangeServer.MemoryLimit"]);
                Assert.AreEqual(0.0123, context.Properties["false-positive"]);

                Assert.IsInstanceOfType(context.Properties["Hyperspace.Replica.Host"], typeof(IList<string>));
                Assert.AreEqual(2, ((IList<string>)context.Properties["Hyperspace.Replica.Host"]).Count);
                Assert.AreEqual("abc", ((IList<string>)context.Properties["Hyperspace.Replica.Host"])[0]);
                Assert.AreEqual("xyz", ((IList<string>)context.Properties["Hyperspace.Replica.Host"])[1]);

                Assert.AreEqual((ushort)66, context.Properties["i16"]);
                Assert.AreEqual(4565, context.Properties["i32"]);
                Assert.AreEqual(235346L, context.Properties["i64"]);
                Assert.AreEqual(12.4567, context.Properties["f64"]);
                Assert.AreEqual(context.Properties["boo"], true);
                Assert.AreEqual(context.Properties["str"], "a string arg");

                Assert.IsInstanceOfType(context.Properties["i32s"], typeof(IList<int>));
                Assert.AreEqual(3, ((IList<int>)context.Properties["i32s"]).Count);
                Assert.AreEqual(1, ((IList<int>)context.Properties["i32s"])[0]);
                Assert.AreEqual(2, ((IList<int>)context.Properties["i32s"])[1]);
                Assert.AreEqual(333333, ((IList<int>)context.Properties["i32s"])[2]);

                Assert.IsInstanceOfType(context.Properties["i64s"], typeof(IList<long>));
                Assert.AreEqual(3, ((IList<long>)context.Properties["i64s"]).Count);
                Assert.AreEqual(1L, ((IList<long>)context.Properties["i64s"])[0]);
                Assert.AreEqual(2L, ((IList<long>)context.Properties["i64s"])[1]);
                Assert.AreEqual(333333L, ((IList<long>)context.Properties["i64s"])[2]);

                Assert.IsInstanceOfType(context.Properties["f64s"], typeof(IList<double>));
                Assert.AreEqual(3, ((IList<double>)context.Properties["f64s"]).Count);
                Assert.AreEqual(1.1, ((IList<double>)context.Properties["f64s"])[0]);
                Assert.AreEqual(2.1, ((IList<double>)context.Properties["f64s"])[1]);
                Assert.AreEqual(333333.1, ((IList<double>)context.Properties["f64s"])[2]);

                Assert.IsInstanceOfType(context.Properties["strs"], typeof(IList<string>));
                Assert.AreEqual(4, ((IList<string>)context.Properties["strs"]).Count);
                Assert.AreEqual("a", ((IList<string>)context.Properties["strs"])[0]);
                Assert.AreEqual("list", ((IList<string>)context.Properties["strs"])[1]);
                Assert.AreEqual("of", ((IList<string>)context.Properties["strs"])[2]);
                Assert.AreEqual("strings", ((IList<string>)context.Properties["strs"])[3]);
            }
        }

        [TestMethod]
        public void MultipleClients() {
            using (var context = Context.Create(connectionString)) {
                for (int i = 0; i < 5; ++i) {
                    using (var client = context.CreateClient()) {
                        Assert.IsTrue(client.NamespaceExists("/"));
                    }
                }

                for (int i = 0; i < 5; ++i) {
                    using (var clientA = context.CreateClient())
                    using (var clientB = context.CreateClient()) {
                        Assert.IsTrue(clientA.NamespaceExists("/"));
                        Assert.IsTrue(clientB.NamespaceExists("/"));
                    }
                }
            }
        }

        [TestMethod]
        public void Reconnect() {
            for (int i = 0; i < 5; ++i) {
                using (var context = Context.Create(connectionString)) {
                    using (var client = context.CreateClient()) {
                        Assert.IsTrue(client.NamespaceExists("/"));
                    }
                }

                Thread.Sleep(200);

                using (var context = Context.Create(connectionString)) {
                    using (var client = context.CreateClient()) {
                        Assert.IsTrue(client.NamespaceExists("/"));
                    }
                }

                Thread.Sleep(200);
            }
        }

        #endregion
    }
}