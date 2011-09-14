/** -*- C++ -*-
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

#pragma once

#ifndef __cplusplus_cli
#error "requires /clr"
#endif

#pragma warning( push )
#pragma warning( disable : 4638 ) // reference to unknown symbol 'CellFlag'

namespace Hypertable {
	using namespace System;
	using namespace System::Collections::Generic;

	ref class Key;
	ref class Cell;

	/// <summary>
	/// Defines a generalized table mutator.
	/// </summary>
	/// <example>
	/// The following example shows how to insert a cell.
	/// <code>
	/// byte[] value = ...;
	/// using( ITableMutator mutator = table.CreateMutator() ) {
	///    Key key = new Key(Guid.NewGuid().ToString(), "cf");
	///    mutator.Set( key, value );
	/// }
	/// </code>
	/// The following example shows how to insert a cell and create a unique row key.
	/// <code>
	/// byte[] value = ...;
	/// using( ITableMutator mutator = table.CreateMutator() ) {
	///    Key key = new Key() { ColumnFamily = "cf" }; // w'out row key
	///    mutator.Set( key, value, true ); // create row key and assign to key
	/// }
	/// </code>
	/// The following example shows how to delete an entire row.
	/// <code>
	/// string row = ...;
	/// using( ITableMutator mutator = table.CreateMutator() ) {
	///    mutator.Delete( row );
	/// }
	/// </code>
	/// The following example shows how to delete all cells in row, column family:column qualifier.
	/// <code>
	/// string row = ...;
	/// using( ITableMutator mutator = table.CreateMutator() ) {
	///    mutator.Delete( new Key(row, "cf", "cq") );
	/// }
	/// // or
	/// Key key = new Key( row, "cf", "cq" );
	/// using( ITableMutator mutator = table.CreateMutator() ) {
	///    mutator.Set( new Cell(key, CellFlag::DeleteCell) );
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="Key"/>
	/// <seealso cref="Cell"/>
	public interface class ITableMutator : public IDisposable {

		public:

			/// <summary>
			/// Gets a value indicating whether the object has been disposed.
			/// </summary>
			/// <remarks>true if the object has been disposed, otherwise false.</remarks>
			property bool IsDisposed {
				bool get( );
			}

			/// <summary>
			/// Inserts a new cell into a table.
			/// </summary>
			/// <param name="key">Cell key.</param>
			/// <param name="value">Cell value, might be null.</param>
			/// <seealso cref="Key"/>
			void Set( Key^ key, cli::array<Byte>^ value );

			/// <summary>
			/// Inserts a new cell into a table, optionally create row key.
			/// </summary>
			/// <param name="key">Cell key.</param>
			/// <param name="value">Cell value, might be null.</param>
			/// <param name="createRowKey">if true a base85 encoded GUID row key will be created.</param>
			/// <seealso cref="Key"/>
			/// <remarks>
			/// The row key (key.Row) will be updated if createRowKey is true.
			/// </remarks>
			void Set( Key^ key, cli::array<Byte>^ value, bool createRowKey );

			/// <summary>
			/// Inserts a new cell into a table or delete cells.
			/// </summary>
			/// <param name="cell">Cell to insert.</param>
			/// <remarks>
			/// In order to delete cells set the cell flag (cell.Flag) approriate.
			/// </remarks>
			/// <seealso cref="Cell"/>
			/// <seealso cref="CellFlag"/>
			void Set( Cell^ cell );

			/// <summary>
			/// Inserts a new cell into a table, optionally create row key.
			/// </summary>
			/// <param name="cell">Cell to insert.</param>
			/// <param name="createRowKey">if true a base85 encoded GUID row key will be created.</param>
			/// <seealso cref="Cell"/>
			/// <remarks>
			/// The row key (cell.Key.Row) will be updated if createRowKey is true.
			/// </remarks>
			void Set( Cell^ cell, bool createRowKey );

			/// <summary>
			/// Inserts multiple cells into a table a table or delete cells.
			/// </summary>
			/// <param name="cells">Cell collection to insert.</param>
			/// <remarks>
			/// In order to delete cells set the cell flag (cell.Flag) approriate.
			/// </remarks>
			/// <seealso cref="Cell"/>
			/// <seealso cref="CellFlag"/>
			void Set( IEnumerable<Cell^>^ cells );

			/// <summary>
			/// Inserts multiple cells into a table, optionally create row keys.
			/// </summary>
			/// <param name="cells">Cell collection to insert.</param>
			/// <param name="createRowKey">if true a base85 encoded GUID row keys will be created.</param>
			/// <seealso cref="Cell"/>
			/// <remarks>
			/// The row key (cell.Key.Row) will be updated for all cells if createRowKey is true.
			/// </remarks>
			void Set( IEnumerable<Cell^>^ cells, bool createRowKey );

			/// <summary>
			/// Deletes an entire row.
			/// </summary>
			/// <param name="row">Row key</param>
			void Delete( String^ row );

			/// <summary>
			/// Deletes an entire row, a column family in a particular row, or a specific cell within a row.
			/// </summary>
			/// <param name="key">Key.</param>
			/// <remarks>
			/// Cells will be deleted depending on the key properties populated:
			/// <table class="comment">
			/// <tr><th>Key.Row</th><th>Key.ColumnFamily</th><th>Key.ColumnQualifier</th><th>Cells to delete</th></tr>
			/// <tr><td>Available</td><td>null or empty</td><td>null</td><td>All cells in the entire row</td></tr>
			/// <tr><td>Available</td><td>Available</td><td>null</td><td>All cells in row,column family</td></tr>
			/// <tr><td>Available</td><td>Available</td><td>Available</td><td>All cells in row,column family:column qualifier</td></tr>
			/// </table>
			/// If Key.DateTime has been specified then only cells older or equal as Key.DateTime will be deleted.
			/// </remarks>
			void Delete( Key^ key );

			/// <summary>
			/// Deletes multiple cells.
			/// </summary>
			/// <param name="keys">Collection of key.</param>
			/// <remarks>
			/// Cells will be deleted depending on the key properties populated:
			/// <table class="comment">
			/// <tr><th>Key.Row</th><th>Key.ColumnFamily</th><th>Key.ColumnQualifier</th><th>Cells to delete</th></tr>
			/// <tr><td>Available</td><td>null or empty</td><td>null</td><td>All cells in the entire row</td></tr>
			/// <tr><td>Available</td><td>Available</td><td>null</td><td>All cells in row,column family</td></tr>
			/// <tr><td>Available</td><td>Available</td><td>Available</td><td>All cells in row,column family:column qualifier</td></tr>
			/// </table>
			/// If Key.DateTime has been specified then only cells older or equal as Key.DateTime will be deleted.
			/// </remarks>
			void Delete( IEnumerable<Key^>^ keys );

			/// <summary>
			/// Deletes multiple cells.
			/// </summary>
			/// <param name="cells">Collection of cells.</param>
			/// <remarks>
			/// Cells will be deleted depending on the cell.Key properties populated:
			/// <table class="comment">
			/// <tr><th>Key.Row</th><th>Key.ColumnFamily</th><th>Key.ColumnQualifier</th><th>Cells to delete</th></tr>
			/// <tr><td>Available</td><td>null or empty</td><td>null</td><td>All cells in the entire row</td></tr>
			/// <tr><td>Available</td><td>Available</td><td>null</td><td>All cells in row,column family</td></tr>
			/// <tr><td>Available</td><td>Available</td><td>Available</td><td>All cells in row,column family:column qualifier</td></tr>
			/// </table>
			/// If cell.Key.DateTime has been specified then only cells older or equal as Key.DateTime will be deleted.
			/// </remarks>
			void Delete( IEnumerable<Cell^>^ cells );

			/// <summary>
			/// Flushes the accumulated mutations to their respective range servers.
			/// </summary>
			void Flush();
	};

}

#pragma warning( pop )