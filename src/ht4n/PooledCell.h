/** -*- C++ -*-
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

#pragma once

#ifndef __cplusplus_cli
#error "requires /clr"
#endif

#include "CellFlag.h"
#include "ICell.h"

namespace ht4c { namespace Common {
	class Cell;
} }

namespace Hypertable {
	using namespace System;
	using namespace System::Buffers;
	using namespace ht4c;

	ref class Key;
	ref class Counter;

	/// <summary>
	/// Represents a Hypertable cell, provide accessors to the cell attributes. The cell value buffer is retained.
	/// </summary>
	/// <remarks>
	/// Encapsulates cell key and cell value. The cell key identifies the location of the cell in a multi-dimensional table.
	/// See also <a href="http://code.google.com/p/hypertable/wiki/ArchitecturalOverview" target="_blank">architectural overview</a>.
	/// </remarks>
	/// <example>
	/// The following example shows how to iterate through all cells of a table using only one BufferedCell instance.
	/// <code>
	/// using( var scanner = table.CreateScanner() ) {
	///    PooledCell cell = new PooledCell();
	///    while( scanner.Move(cell) ) {
	///       // process cell
	///       PooledCell.Release(cell.Value);
	///    }
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="Key"/>
	/// <seealso cref="Counter"/>
	[Serializable]
	public ref class PooledCell : public ICell {

		public:

			/// <summary>
			/// Initializes a new instance of the PooledCell class.
			/// </summary>
			PooledCell( );

			/// <summary>
			/// Gets or sets the cell key.
			/// </summary>
			/// <seealso cref="Key"/>
			virtual property Key^ Key;

			/// <summary>
			/// Gets or sets the cell value, might be null.
			/// </summary>
			virtual property cli::array<Byte>^ Value {
				cli::array<Byte>^ get() {
					return value;
				}
			}

			/// <summary>
			/// Gets the cell value length.
			/// </summary>
			virtual property int ValueLength {
				int get() {
					return valueLength;
				}
			}

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

			/// <summary>
			/// Returns the value back to pool.
			/// </summary>
			static void Return( cli::array<Byte>^ value) {
				if( value != nullptr ) {
					pool->Return(value, false);
				}
			}

		internal:

			PooledCell( const Common::Cell* cell );
			void From( const Common::Cell& cell );

		private:

			cli::array<Byte>^ value;
			int valueLength;

			static ArrayPool<Byte>^ pool = ArrayPool<Byte>::Create();
	};

}