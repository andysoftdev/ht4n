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

    /// <summary>
    /// The cell info.
    /// </summary>
    internal sealed class CellInfo
    {
        #region Constants and Fields

        public const int Limit = 64;

        private readonly Cell cell;

        private readonly DatabaseDirectoryInfo directoryInfo;

        private readonly byte[] value;

        #endregion

        #region Constructors and Destructors

        public CellInfo(DatabaseDirectoryInfo directoryInfo, Cell cell) {
            this.directoryInfo = directoryInfo;
            this.cell = cell;
            if (this.cell.Value != null) {
                this.CellValueSize = this.cell.Value.Length;
                if (this.CellValueSize > Limit) {
                    this.value = this.cell.Value;
                    Array.Resize(ref this.value, Limit);
                    this.cell.Value = null;
                }
            }
        }

        #endregion

        #region Public Properties

        public int CellValueSize { get; private set; }

        public Key Key {
            get {
                return this.cell.Key;
            }
        }

        public byte[] Value {
            get {
                if (this.cell.Value == null && this.CellValueSize > 0) {
                    var c = this.directoryInfo.Find(this.cell.Key);
                    if (c != null) {
                        return c.Value;
                    }
                }

                return this.cell.Value;
            }
        }

        public byte[] ValueInfo {
            get {
                return this.value ?? this.cell.Value;
            }
        }

        #endregion
    }
}