﻿/** -*- C# -*-
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
namespace Hypertable.Explorer
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// The directory info.
    /// </summary>
    internal sealed class DatabaseDirectoryInfo
    {
        #region Fields

        /// <summary>
        /// The namespace listing.
        /// </summary>
        private readonly NamespaceListing namespaceListing;

        /// <summary>
        /// The table.
        /// </summary>
        private readonly Tuple<string, string> table;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseDirectoryInfo"/> class.
        /// </summary>
        /// <param name="namespaceListing">
        /// The ns listing.
        /// </param>
        public DatabaseDirectoryInfo(NamespaceListing namespaceListing)
        {
            this.namespaceListing = namespaceListing;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseDirectoryInfo"/> class.
        /// </summary>
        /// <param name="namespaceName">
        /// The namespace name.
        /// </param>
        /// <param name="tableName">
        /// The table name.
        /// </param>
        private DatabaseDirectoryInfo(string namespaceName, string tableName)
        {
            this.table = new Tuple<string, string>(namespaceName, tableName);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the cells.
        /// </summary>
        public ObservableList<CellInfo> Cells
        {
            get
            {
                var observable = new ObservableList<CellInfo>();
                if (this.table != null)
                {
                    DatabaseSession.Instance.BeginScan(this.table.Item1, this.table.Item2, cells => observable.AddRange(cells.Select(c => new CellInfo(this, c)).ToList()));
                }

                return observable;
            }
        }

        /// <summary>
        /// Gets namespaces and tables.
        /// </summary>
        public ObservableList<DatabaseDirectoryInfo> Directories
        {
            get
            {
                var observable = new ObservableList<DatabaseDirectoryInfo>();
                if (this.namespaceListing != null && (this.namespaceListing.Namespaces.Count > 0 || this.namespaceListing.Tables.Count > 0))
                {
                    Task.Factory.StartNew(
                        () =>
                            {
                                foreach (var ns in this.namespaceListing.Namespaces)
                                {
                                    observable.Add(DatabaseSession.Instance.GetDirectoryInfo(ns.FullName));
                                }

                                foreach (var t in this.namespaceListing.Tables)
                                {
                                    observable.Add(new DatabaseDirectoryInfo(this.namespaceListing.FullName, t));
                                }
                            });
                }

                return observable;
            }
        }

        /// <summary>
        /// Gets the full name
        /// </summary>
        public string FullName
        {
            get
            {
                return "/" + (this.namespaceListing != null ? this.namespaceListing.FullName : this.table.Item1 + "/" + this.table.Item2);
            }
        }

        /// <summary>
        /// Gets the namespace name
        /// </summary>
        public string Name
        {
            get
            {
                return this.namespaceListing != null ? !string.IsNullOrEmpty(this.namespaceListing.Name) ? this.namespaceListing.Name : "/" : this.table.Item2;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The find.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// </returns>
        public Cell Find(Key key)
        {
            if (this.table != null)
            {
                return DatabaseSession.Instance.Find(this.table.Item1, this.table.Item2, key);
            }

            return null;
        }

        #endregion
    }
}