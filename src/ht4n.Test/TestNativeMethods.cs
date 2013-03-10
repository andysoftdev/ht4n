/** -*- C# -*-
 * Copyright (C) 2010-2013 Thalmann Software & Consulting, http://www.softdev.ch
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
    using System.Runtime.InteropServices;

    /// <summary>
    /// The executio state.
    /// </summary>
    [Flags]
    internal enum EXECUTION_STATE : uint
    {
        ES_AWAYMODE_REQUIRED = 0x00000040, 
        ES_CONTINUOUS = 0x80000000, 
        ES_DISPLAY_REQUIRED = 0x00000002, 
        ES_SYSTEM_REQUIRED = 0x00000001

        // Legacy flag, should not be used.
        // ES_USER_PRESENT = 0x00000004
    }

    /// <summary>
    /// The native methods.
    /// </summary>
    internal static class NativeMethods
    {
        #region Methods

        /// <summary>
        /// The set thread execution state.
        /// </summary>
        /// <param name="esFlags">
        /// The es flags.
        /// </param>
        /// <returns>
        /// The <see cref="EXECUTION_STATE"/>.
        /// </returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        #endregion
    }
}