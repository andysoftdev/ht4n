/** -*- C++ -*-
 * Copyright (C) 2010-2012 Thalmann Software & Consulting, http://www.softdev.ch
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

#include "AsyncResult.h"

namespace Hypertable {
	using namespace System;
	using namespace System::Runtime::InteropServices;
	using namespace System::Collections::Generic;

	ref class ScanSpec;
	ref class Cell;
	ref class AsyncScannerContext;
	ref class AsyncMutatorContext;

	/// <summary>
	/// Represents results from asynchronous table scan operations.
	/// </summary>
	/// <example>
	/// The following example shows how to scan a multiple tables asynchronously.
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
	public ref class BlockingAsyncResult : public AsyncResult {

		public:

			/// <summary>
			/// Initializes a new instance of the BlockingAsyncResult class.
			/// </summary>
			BlockingAsyncResult( );

			/// <summary>
			/// Initializes a new instance of the BlockingAsyncResult class using the specified capacity.
			/// </summary>
			/// <param name="capacity">Capacity in bytes of result queue. If zero then the queue capacity will be unbounded.</param>
			BlockingAsyncResult( size_t capacity );

			/// <summary>
			/// Gets the available cells, blocks the calling thread till there is a result available
			/// unless asynchronous operations have completed or cancelled.
			/// </summary>
			/// <param name="cells">Available cells. This parameter is passed uninitialized.</param>m>
			/// <returns>true if all outstanding operations have been completed.</returns>
			/// <seealso cref="ITable"/>
			bool TryGetCells( [Out] IList<Cell^>^% cells );

			/// <summary>
			/// Gets the available cells, blocks the calling thread till there is a result available
			/// unless asynchronous operations have completed or cancelled.
			/// </summary>
			/// <param name="asyncScannerContext">Table scanner context.</param>
			/// <param name="cells">Available cells. This parameter is passed uninitialized.</param>m>
			/// <returns>true if all outstanding operations have been completed.</returns>
			/// <seealso cref="ITable"/>
			bool TryGetCells( [Out] AsyncScannerContext^% asyncScannerContext, [Out] IList<Cell^>^% cells );

			/// <summary>
			/// Gets the available cells, blocks the calling thread till there is a result available
			/// unless asynchronous operations have completed, cancelled or a timeout occurs.
			/// </summary>
			/// <param name="timeout">Timespan to wait before a timeout occurs.</param>
			/// <param name="cells">Available cells. This parameter is passed uninitialized.</param>m>
			/// <returns>true if all outstanding operations have been completed.</returns>
			/// <seealso cref="ITable"/>
			bool TryGetCells( TimeSpan timeout, [Out] IList<Cell^>^% cells );

			/// <summary>
			/// Gets the available cells, blocks the calling thread till there is a result available
			/// unless asynchronous operations have completed, cancelled or a timeout occurs.
			/// </summary>
			/// <param name="timeout">Timespan to wait before a timeout occurs.</param>
			/// <param name="asyncScannerContext">Table scanner context.</param>
			/// <param name="cells">Available cells. This parameter is passed uninitialized.</param>m>
			/// <returns>true if all outstanding operations have been completed.</returns>
			/// <seealso cref="ITable"/>
			bool TryGetCells( TimeSpan timeout, [Out] AsyncScannerContext^% asyncScannerContext, [Out] IList<Cell^>^% cells );

	internal:

			virtual void AttachAsyncScanner( AsyncScannerContext^ asyncScannerContext, AsyncScannerCallback^ callback ) override;
			virtual void AttachAsyncMutator( AsyncMutatorContext^ asyncMutatorContext, ITableMutator^ mutator ) override;
			virtual Common::AsyncResult* CreateAsyncResult( Common::ContextKind contextKind, Common::AsyncResultSink* asyncResultSink ) override;

	private:

		size_t capacity;
		Dictionary<int64_t, AsyncScannerContext^>^ map;
		Object^ syncRoot;
	};

}