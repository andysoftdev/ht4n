/** -*- C++ -*-
 * Copyright (C) 2010-2014 Thalmann Software & Consulting, http://www.softdev.ch
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

#pragma once

#ifndef __cplusplus_cli
#error "requires /clr"
#endif

#include "ht4c.Common/CellFlag.h"

#pragma warning( push )
#pragma warning( disable : 4638 ) // reference to unknown symbol 'ITableMutator'

namespace Hypertable {
	using namespace System;

	/// <summary>
	/// Specifies possible cell flag values.
	/// </summary>
	/// <remarks>
	/// The delete flags are used by the table mutator.
	/// </remarks>
	/// <example>
	/// The following example shows how to delete two rows from a table using the cell flags.
	/// <code>
	/// IList&lt;Cell&gt; cells = new List&lt;Cell&gt;();
	/// cells.Add( new Cell(new Key("A"), CellFlag.DeleteRow) );
	/// cells.Add( new Cell(new Key("B"), CellFlag.DeleteRow) );
	/// using( var mutator = table.CreateMutator() ) {
	///    mutator.Set(cells);
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="ITableMutator" />
	[Serializable]
	public enum class CellFlag {

		/// <summary>
		/// Default cell flag.
		/// </summary>
		Default = ht4c::Common::CF_Default,

		/// <summary>
		/// Deletes all cells in a particular row.
		/// </summary>
		DeleteRow = ht4c::Common::CF_DeleteRow,

		/// <summary>
		/// Deletes all cells in a particular row/column family.
		/// </summary>
		DeleteColumnFamily = ht4c::Common::CF_DeleteColumnFamily,

		/// <summary>
		/// Deletes a particular cell.
		/// </summary>
		DeleteCell = ht4c::Common::CF_DeleteCell,

		/// <summary>
		/// Deletes a particular cell version.
		/// </summary>
		DeleteCellVersion = ht4c::Common::CF_DeleteCellVersion
	};

}

#pragma warning( pop )