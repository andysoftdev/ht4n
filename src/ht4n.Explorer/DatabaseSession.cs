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
        /// <summary>
        /// The disconnected.
        /// </summary>
        Disconnected, 

        /// <summary>
        /// The disconnecting.
        /// </summary>
        Disconnecting, 

        /// <summary>
        /// The connecting.
        /// </summary>
        Connecting, 

        /// <summary>
        /// The connected.
        /// </summary>
        Connected
    }

    /// <summary>
    /// Connection state event argument.
    /// </summary>
    internal sealed class NotifyConnectionStateChangedEventArgs : EventArgs
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyConnectionStateChangedEventArgs"/> class.
        /// </summary>
        /// <param name="connectionState">
        /// The connection state.
        /// </param>
        public NotifyConnectionStateChangedEventArgs(ConnectionState connectionState)
        {
            this.ConnectionState = connectionState;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyConnectionStateChangedEventArgs"/> class.
        /// </summary>
        /// <param name="connectionState">
        /// The connection state.
        /// </param>
        /// <param name="connectionString">
        /// The connection string.
        /// </param>
        public NotifyConnectionStateChangedEventArgs(ConnectionState connectionState, string connectionString)
        {
            this.ConnectionState = connectionState;
            this.ConnectionString = connectionString;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the connection state.
        /// </summary>
        public ConnectionState ConnectionState { get; private set; }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        public string ConnectionString { get; private set; }

        #endregion
    }

    /// <summary>
    /// The database session.
    /// </summary>
    internal sealed class DatabaseSession : IDisposable
    {
        #region Static Fields

        /// <summary>
        /// The sync root.
        /// </summary>
        private static readonly object SyncRoot = new object();

        #endregion

        #region Fields

        /// <summary>
        /// The client.
        /// </summary>
        private IClient client;

        /// <summary>
        /// The context.
        /// </summary>
        private IContext context;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseSession"/> class.
        /// </summary>
        /// <param name="connectionString">
        /// The connection string.
        /// </param>
        private DatabaseSession(string connectionString)
        {
            this.context = Context.Create(connectionString + ";ConnectionTimeout=15000");
            this.client = this.context.CreateClient();
        }

        #endregion

        #region Public Events

        /// <summary>
        /// The connection state changed.
        /// </summary>
        public static event NotifyConnectionStateChangedEventHandler ConnectionStateChanged;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static DatabaseSession Instance { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The connect.
        /// </summary>
        /// <param name="connectionString">
        /// The connection string.
        /// </param>
        public static void Connect(string connectionString)
        {
            lock (SyncRoot)
            {
                Disconnect();
                FireConnectionStateChanged(new NotifyConnectionStateChangedEventArgs(ConnectionState.Connecting, connectionString));
                Instance = new DatabaseSession(connectionString);
                FireConnectionStateChanged(new NotifyConnectionStateChangedEventArgs(ConnectionState.Connected, connectionString));
            }
        }

        /// <summary>
        /// The disconnect.
        /// </summary>
        public static void Disconnect()
        {
            DatabaseScanner.Cancel();

            lock (SyncRoot)
            {
                if (Instance != null)
                {
                    FireConnectionStateChanged(new NotifyConnectionStateChangedEventArgs(ConnectionState.Disconnecting));
                    Instance.Close();
                    Instance = null;
                    FireConnectionStateChanged(new NotifyConnectionStateChangedEventArgs(ConnectionState.Disconnected));
                }
            }
        }

        /// <summary>
        /// The begin scan.
        /// </summary>
        /// <param name="namespaceName">
        /// The namespace name.
        /// </param>
        /// <param name="tableName">
        /// The table name.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        public void BeginScan(string namespaceName, string tableName, Action<IList<Cell>> action)
        {
            if (this.client == null)
            {
                throw new InvalidOperationException("Session has been closed");
            }

            DatabaseScanner.BeginScan(this.client, namespaceName, tableName, action);
        }

        /// <summary>
        /// The close.
        /// </summary>
        public void Close()
        {
            if (this.client != null)
            {
                this.client.Dispose();
                this.client = null;
            }

            if (this.context != null)
            {
                this.context.Dispose();
                this.context = null;
            }
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.Close();
        }

        /// <summary>
        /// The find.
        /// </summary>
        /// <param name="namespaceName">
        /// The namespace name.
        /// </param>
        /// <param name="tableName">
        /// The table name.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        public Cell Find(string namespaceName, string tableName, Key key)
        {
            if (this.client == null)
            {
                throw new InvalidOperationException("Session has been closed");
            }

            return DatabaseScanner.Find(this.client, namespaceName, tableName, key);
        }

        /// <summary>
        /// The get directory info.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        public DatabaseDirectoryInfo GetDirectoryInfo(string name)
        {
            if (this.client == null)
            {
                throw new InvalidOperationException("Session has been closed");
            }

            using (var ns = this.client.OpenNamespace(name))
            {
                return new DatabaseDirectoryInfo(ns.GetListing());
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The fire connection state changed.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void FireConnectionStateChanged(NotifyConnectionStateChangedEventArgs e)
        {
            if (ConnectionStateChanged != null)
            {
                ConnectionStateChanged(null, e);
            }
        }

        #endregion
    }
}