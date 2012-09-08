/** -*- C++ -*-
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

#pragma once

#ifndef __cplusplus_cli
#error "requires /clr"
#endif

namespace Hypertable {
	using namespace System;
	using namespace System::Collections::Generic;
	using namespace System::Runtime::InteropServices;

	ref class Cell;
	ref class ScanSpec;

	/// <summary>
	/// Defines a generalized table scanner.
	/// </summary>
	/// <example>
	/// The following example shows how to scan all cells of a table.
	/// <code>
	/// using( var scanner = table.CreateScanner() ) {
	///    foreach( cell cell in scanner ) {
	///       // process cell
	///    }
	/// }
	/// // or
	/// using( var scanner = table.CreateScanner() ) {
	///    Cell cell;
	///    while( scanner.Next(out cell) ) {
	///       // process cell
	///    }
	/// }
	/// </code>
	/// The following example shows how to scan all cells of a table using a single cell instance.
	/// <code>
	/// using( var scanner = table.CreateScanner() ) {
	///    Cell cell = new Cell();
	///    while( scanner.Next(cell) ) {
	///       // process cell
	///    }
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="ScanSpec"/>
	/// <seealso cref="Cell"/>
	public interface class ITableScanner : public IEnumerable<Cell^>, public IDisposable {

		public:

			/// <summary>
			/// Gets the scan specification associated with this table scanner.
			/// </summary>
			/// <remarks>
			/// Modifiying the table scan specification once the table scanner has been created has no effect.
			/// </remarks>
			property Hypertable::ScanSpec^ ScanSpec {
				Hypertable::ScanSpec^ get( );
			}

			/// <summary>
			/// Gets a value indicating whether the object has been disposed.
			/// </summary>
			/// <remarks>true if the object has been disposed, otherwise false.</remarks>
			property bool IsDisposed {
				bool get( );
			}

			/// <summary>
			/// Gets the next available cell using the specified cell instance.
			/// </summary>
			/// <param name="cell">Cell instance.</param>
			/// <returns>true if there are more cells available, otherwise false.</returns>
			/// <remarks>
			/// The methods updates the cell instance specified.
			/// </remarks>
			bool Move( Cell^ cell );

			/// <summary>
			/// Gets the next available cell, creating a new cell instance.
			/// </summary>
			/// <param name="cell">Scanned cell. This parameter is passed uninitialized.</param>
			/// <returns>true if there are more cells available, otherwise false.</returns>
			/// <remarks>
			/// The methods returns a new cell instance.
			/// </remarks>
			bool Next( [Out] Cell^% cell );
	};

}