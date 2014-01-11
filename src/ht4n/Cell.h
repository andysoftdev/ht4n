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

#include "CellFlag.h"

namespace ht4c { namespace Common {
	class Cell;
} }

namespace Hypertable {
	using namespace System;
	using namespace ht4c;

	ref class Key;

	/// <summary>
	/// Represents a Hypertable cell, provide accessors to the cell attributes.
	/// </summary>
	/// <remarks>
	/// Encapsulates cell key and cell value. The cell key identifies the location of the cell in a multi-dimensional table.
	/// See also <a href="http://code.google.com/p/hypertable/wiki/ArchitecturalOverview" target="_blank">architectural overview</a>.
	/// </remarks>
	/// <example>
	/// The following example shows how to iterate through all cells of a table using only one Cell instance.
	/// <code>
	/// using( var scanner = table.CreateScanner() ) {
	///    Cell cell = new Cell();
	///    while( scanner.Move(cell) ) {
	///       // process cell
	///    }
	/// }
	/// </code>
	/// The following example shows how to use the cloneKey parameter.
	/// <code>
	/// Key key = new Key() { ColumnFamily = "cf" };
	/// IList&lt;Cell&gt; cells = new List&lt;Cell&gt;();
	/// for( int n = 0; n &lt; 10; ++n ) {
	///    key.Row = Guid.NewGuid().ToString();
	///    cells.Add(new Cell(key, Encoding.UTF8.GetBytes(key.Row), true)); // creates a key clone
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="Key"/>
	[Serializable]
	public ref class Cell sealed {

		public:

			/// <summary>
			/// Initializes a new instance of the Cell class.
			/// </summary>
			Cell( );

			/// <summary>
			/// Initializes a new instance of the Cell class using the specified cell key and cell value.
			/// </summary>
			/// <param name="key">Cell key.</param>
			/// <param name="value">Cell value, might be null.</param>
			/// <seealso cref="Key"/>
			Cell( Key^ key, cli::array<Byte>^ value );

			/// <summary>
			/// Initializes a new instance of the Cell class using the specified cell key and cell value.
			/// </summary>
			/// <param name="key">Cell key.</param>
			/// <param name="value">Cell value, might be null.</param>
			/// <param name="cloneKey">If true the constructor creates a deep copy if the specified cell key.</param>
			/// <seealso cref="Key"/>
			Cell( Key^ key, cli::array<Byte>^ value, bool cloneKey );

			/// <summary>
			/// Initializes a new instance of the Cell class using the specified cell key and cell flag.
			/// </summary>
			/// <param name="key">Cell key.</param>
			/// <param name="flag">Cell flag.</param>
			/// <seealso cref="Key"/>
			/// <seealso cref="CellFlag"/>
			Cell( Key^ key, CellFlag flag );

			/// <summary>
			/// Initializes a new instance of the Cell class using the specified cell key and cell flag.
			/// </summary>
			/// <param name="key">Cell key.</param>
			/// <param name="flag">Cell flag.</param>
			/// <param name="cloneKey">If true the constructor creates a deep copy if the specified cell key.</param>
			/// <seealso cref="Key"/>
			/// <seealso cref="CellFlag"/>
			Cell( Key^ key, CellFlag flag, bool cloneKey );

			/// <summary>
			/// Initializes a new instance of the Cell class using the specified cell key, cell value and cell flag.
			/// </summary>
			/// <param name="key">Cell key.</param>
			/// <param name="value">Cell value, might be null.</param>
			/// <param name="flag">Cell flag.</param>
			/// <seealso cref="Key"/>
			/// <seealso cref="CellFlag"/>
			Cell( Key^ key, cli::array<Byte>^ value, CellFlag flag );

			/// <summary>
			/// Initializes a new instance of the Cell class using the specified cell key, cell value and cell flag.
			/// </summary>
			/// <param name="key">Cell key.</param>
			/// <param name="value">Cell value, might be null.</param>
			/// <param name="flag">Cell flag.</param>
			/// <param name="cloneKey">If true the constructor creates a deep copy if the specified cell key.</param>
			/// <seealso cref="Key"/>
			/// <seealso cref="CellFlag"/>
			Cell( Key^ key, cli::array<Byte>^ value, CellFlag flag, bool cloneKey );

			/// <summary>
			/// Gets or sets the cell key.
			/// </summary>
			/// <seealso cref="Key"/>
			property Key^ Key;

			/// <summary>
			/// Gets or sets the cell value, might be null.
			/// </summary>
			property cli::array<Byte>^ Value;

			/// <summary>
			/// Gets or sets the cell flag.
			/// </summary>
			/// <seealso cref="CellFlag"/>
			property CellFlag Flag;

			/// <summary>
			/// Returns a string that represents the current object.
			/// </summary>
			/// <returns>A string that represents the current object.</returns>
			virtual String^ ToString() override;

		internal:

			Cell( const Common::Cell* cell );
			void From( const Common::Cell& cell );

			static CellFlag DeleteFlagFromKey( Hypertable::Key^ key );
	};

}