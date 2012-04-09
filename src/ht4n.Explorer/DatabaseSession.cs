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

namespace Hypertable.Explorer
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Notify connection state changed delegate.
    /// </summary>
    internal delegate void NotifyConnectionStateChangedEventHandler(object sender, NotifyConnectionStateChangedEventArgs e);

    /// <summary>
    /// Connection state.
    /// </summary>
    internal enum ConnectionState
    {
        Disconnected, 

        Disconnecting, 

        Connecting, 

        Connected
    }

    /// <summary>
    /// Connection state event argument.
    /// </summary>
    internal sealed class NotifyConnectionStateChangedEventArgs : EventArgs
    {
        #region Constructors and Destructors

        public NotifyConnectionStateChangedEventArgs(ConnectionState connectionState) {
            this.ConnectionState = connectionState;
        }

        public NotifyConnectionStateChangedEventArgs(ConnectionState connectionState, string connectionString) {
            this.ConnectionState = connectionState;
            this.ConnectionString = connectionString;
        }

        #endregion

        #region Public Properties

        public ConnectionState ConnectionState { get; private set; }

        public string ConnectionString { get; private set; }

        #endregion
    }

    /// <summary>
    /// The database session.
    /// </summary>
    internal sealed class DatabaseSession : IDisposable
    {
        #region Constants and Fields

        private static readonly object syncRoot = new object();

        private IClient client;

        private IContext context;

        #endregion

        #region Constructors and Destructors

        private DatabaseSession(string connectionString) {
            this.context = Context.Create(connectionString + ";ConnectionTimeout=15000");
            this.client = this.context.CreateClient();
        }

        #endregion

        #region Public Events

        public static event NotifyConnectionStateChangedEventHandler ConnectionStateChanged;

        #endregion

        #region Public Properties

        public static DatabaseSession Instance { get; private set; }

        #endregion

        #region Public Methods

        public static void Connect(string connectionString) {
            lock (syncRoot) {
                Disconnect();
                FireConnectionStateChanged(new NotifyConnectionStateChangedEventArgs(ConnectionState.Connecting, connectionString));
                Instance = new DatabaseSession(connectionString);
                FireConnectionStateChanged(new NotifyConnectionStateChangedEventArgs(ConnectionState.Connected, connectionString));
            }
        }

        public static void Disconnect() {
            DatabaseScanner.Cancel();

            lock (syncRoot) {
                if (Instance != null) {
                    FireConnectionStateChanged(new NotifyConnectionStateChangedEventArgs(ConnectionState.Disconnecting));
                    Instance.Close();
                    Instance = null;
                    FireConnectionStateChanged(new NotifyConnectionStateChangedEventArgs(ConnectionState.Disconnected));
                }
            }
        }

        public void BeginScan(string nsName, string tableName, Action<IList<Cell>> action) {
            if (this.client == null) {
                throw new InvalidOperationException("Session has been closed");
            }

            DatabaseScanner.BeginScan(this.client, nsName, tableName, action);
        }

        public void Close() {
            if (this.client != null) {
                this.client.Dispose();
                this.client = null;
            }

            if (this.context != null) {
                this.context.Dispose();
                this.context = null;
            }
        }

        public void Dispose() {
            this.Close();
        }

        public Cell Find(string nsName, string tableName, Key key) {
            if (this.client == null) {
                throw new InvalidOperationException("Session has been closed");
            }

            return DatabaseScanner.Find(this.client, nsName, tableName, key);
        }

        public DatabaseDirectoryInfo GetDirectoryInfo(string name) {
            if (this.client == null) {
                throw new InvalidOperationException("Session has been closed");
            }

            using (var ns = this.client.OpenNamespace(name)) {
                return new DatabaseDirectoryInfo(ns.GetListing());
            }
        }

        #endregion

        #region Methods

        private static void FireConnectionStateChanged(NotifyConnectionStateChangedEventArgs e) {
            if (ConnectionStateChanged != null) {
                ConnectionStateChanged(null, e);
            }
        }

        #endregion
    }
}