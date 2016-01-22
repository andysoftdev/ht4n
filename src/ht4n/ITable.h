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

#include "AsyncScannerCallback.h"

namespace Hypertable {
	using namespace System;
	using namespace System::Collections::Generic;

	interface class ITableMutator;
	interface class ITableScanner;
	ref class MutatorSpec;
	ref class ScanSpec;
	ref class Cell;
	ref class AsyncResult;

	namespace Xml {

		ref class TableSchema;

	}

	/// <summary>
	/// Represents a Hypertable table.
	/// </summary>
	/// <example>
	/// The following example shows how to create an asynchronous table mutator.
	/// <code>
	/// using( var mutator = table.CreateMutator(MutatorSpec.CreateAsync()) ) {
	///   // use mutator
	/// }
	/// </code>
	/// The following example shows how to scan a few individual rows.
	/// <code>
	/// var scanSpec = new ScanSpec()
	///                    .AddRow("r1")
	///                    .AddRow("r2")
	///                    .AddRow("r3");
	///
	/// using( var scanner = table.CreateScanner(scanSpec) ) {
	///    foreach( var cell in scanner ) {
	///       // process cell
	///    }
	/// }
	/// </code>
	/// The following example shows how to scan a single table asynchronously.
	/// <code>
	/// using( var asynResult = table.BeginScan(param,
	///    delegate( AsyncScannerContext asyncScannerContext, IList&lt;Cell&gt; cells ) {
	///       // process cells
	///       return true; // continue, return false to cancel
	///    }) ) {
	///    asynResult.Join();
	/// }
	/// </code>
	/// The following example shows how to scan a multiple tables asynchronously.
	/// <code>
	/// using( var asynResult = new AsyncResult(
	///    delegate( AsyncScannerContext asyncScannerContext, IList&lt;Cell&gt; cells ) {
	///       // process cells
	///       return true; // continue, return false to cancel
	///    }) ) {
	///    tableA.BeginScan(new ScanSpec().AddColumn("a"), asynResult);
	///    tableB.BeginScan(new ScanSpec().AddColumn("b"), asynResult);
	///    asynResult.Join();
	///  }
	/// </code>
	/// The following example shows how to scan a multiple tables asynchronously
	/// using a blocking asynchronous result.
	/// <code>
	/// using( var asynResult = new BlockingAsyncResult() ) {
	///    tableA.BeginScan(asynResult);
	///    tableB.BeginScan(asynResult);
	///    AsyncScannerContext asyncScannerContext;
	///    IList&lt;Cell&gt; cells;
	///    while( asynResult.TryGetCells(out asyncScannerContext, out cells) ) {
	///       foreach( Cell cell in cells ) {
	///          // process cell
	///       }
	///    }
	/// }
	/// </code>
	/// </example>
	public interface class ITable : public IDisposable {

		public:

			/// <summary>
			/// Gets the table name.
			/// </summary>
			property String^ Name {
				String^ get( );
			}

			/// <summary>
			/// Gets the table xml schema.
			/// </summary>
			property String^ Schema {
				String^ get( );
			}

			/// <summary>
			/// Gets a value indicating whether the object has been disposed.
			/// </summary>
			/// <remarks>true if the object has been disposed, otherwise false.</remarks>
			property bool IsDisposed {
				bool get( );
			}

			/// <summary>
			/// Creates a new table mutator on this table.
			/// </summary>
			/// <returns>Newly created table mutator.</returns>
			ITableMutator^ CreateMutator( );

			/// <summary>
			/// Creates a new table mutator on this table using the specified mutator specification.
			/// </summary>
			/// <param name="mutatorSpec">Table mutator specification.</param>
			/// <returns>Newly created table mutator instance.</returns>
			ITableMutator^ CreateMutator( MutatorSpec^ mutatorSpec );

			/// <summary>
			/// Creates a new asynchronous table mutator on this table using the specified mutator specification.
			/// </summary>
			/// <param name="asyncResult">Asynchronous result instance.</param>
			/// <param name="mutatorSpec">Table mutator specification.</param>
			/// <returns>Newly created asynchronous table mutator instance.</returns>
			ITableMutator^ CreateAsyncMutator( AsyncResult^ asyncResult, MutatorSpec^ mutatorSpec );

			/// <summary>
			/// Creates a new table scanner on this table.
			/// </summary>
			/// <returns>Newly created table scanner instance.</returns>
			ITableScanner^ CreateScanner( );

			/// <summary>
			/// Creates a new table scanner on this table using the specified scanner specification.
			/// </summary>
			/// <param name="scanSpec">Table scanner specification.</param>
			/// <returns>Newly created table mutator instance.</returns>
			ITableScanner^ CreateScanner( ScanSpec^ scanSpec );

			/// <summary>
			/// Creates a new asynchronous scanner on this table and attach it
			/// to the specified asynchronous result instance.
			/// </summary>
			/// <param name="asyncResult">Asynchronous result instance.</param>
			/// <returns>Asynchronous scanner identifier.</returns>
			/// <remarks>Scans the entire table.</remarks>
			int64_t BeginScan( AsyncResult^ asyncResult );

			/// <summary>
			/// Creates a new asynchronous scanner on this table using the specified scanner specification
			/// and attach it to the specified asynchronous result instance.
			/// </summary>
			/// <param name="asyncResult">Asynchronous result instance.</param>
			/// <param name="scanSpec">Table scanner specification.</param>
			/// <returns>Asynchronous scanner identifier.</returns>
			int64_t BeginScan( AsyncResult^ asyncResult, ScanSpec^ scanSpec );

			/// <summary>
			/// Creates a new asynchronous scanner on this table using the specified scanner specification
			/// and attach to the specified asynchronous result instance.
			/// </summary>
			/// <param name="asyncResult">Asynchronous result instance.</param>
			/// <param name="scanSpec">Table scanner specification.</param>
			/// <param name="param">User defined parameter, which will be passed to the callback.</param>
			/// <returns>Asynchronous scanner identifier.</returns>
			int64_t BeginScan( AsyncResult^ asyncResult, ScanSpec^ scanSpec, Object^ param );

			/// <summary>
			/// Creates a new asynchronous scanner on this table and attach it
			/// to the specified asynchronous result instance.
			/// </summary>
			/// <param name="asyncResult">Asynchronous result instance.</param>
			/// <param name="callback">Asynchronous scanner callback.</param>
			/// <returns>Asynchronous scanner identifier.</returns>
			/// <remarks>Scans the entire table.</remarks>
			int64_t BeginScan( AsyncResult^ asyncResult, AsyncScannerCallback^ callback );

			/// <summary>
			/// Creates a new asynchronous scanner on this table using the specified scanner specification
			/// and attach it to the specified asynchronous result instance.
			/// </summary>
			/// <param name="asyncResult">Asynchronous result instance.</param>
			/// <param name="scanSpec">Table scanner specification.</param>
			/// <param name="callback">Asynchronous scanner callback.</param>
			/// <returns>Asynchronous scanner identifier.</returns>
			int64_t BeginScan( AsyncResult^ asyncResult, ScanSpec^ scanSpec, AsyncScannerCallback^ callback );

			/// <summary>
			/// Creates a new asynchronous scanner on this table using the specified scanner specification
			/// and attach to the specified asynchronous result instance.
			/// </summary>
			/// <param name="asyncResult">Asynchronous result instance.</param>
			/// <param name="scanSpec">Table scanner specification.</param>
			/// <param name="param">User defined parameter, which will be passed to the callback.</param>
			/// <param name="callback">Asynchronous scanner callback.</param>
			/// <returns>Asynchronous scanner identifier.</returns>
			int64_t BeginScan( AsyncResult^ asyncResult, ScanSpec^ scanSpec, Object^ param, AsyncScannerCallback^ callback );

			/// <summary>
			/// Gets a table schema instance.
			/// </summary>
			/// <returns>The table schem instance.</returns>
			Xml::TableSchema^ GetTableSchema( );
	};

}