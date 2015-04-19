/** -*- C++ -*-
 * Copyright (C) 2010-2015 Thalmann Software & Consulting, http://www.softdev.ch
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

#include "ITable.h"
#include "AsyncScannerCallback.h"

namespace ht4c { namespace Common {
	class Table;
	class ScanSpec;
} }

namespace Hypertable {
	using namespace System;
	using namespace System::Collections::Generic;
	using namespace ht4c;

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
	/// <seealso cref="ITable"/>
	ref class Table sealed : public ITable {

		public:

			/// <summary>
			/// Clean up all managed and unmanaged resources.
			/// </summary>
			virtual ~Table( );

			/// <summary>
			/// Clean up all unmanaged resources.
			/// </summary>
			!Table( );

			#pragma region ITable properties

			property String^ Name {
				virtual String^ get( );
			}

			property String^ Schema {
				virtual String^ get( );
			}

			property bool IsDisposed {
				virtual bool get( ) {
					return disposed;
				}
			}

			#pragma endregion

			#pragma region ITable methods

			virtual ITableMutator^ CreateMutator( );
			virtual ITableMutator^ CreateMutator( MutatorSpec^ mutatorSpec );
			virtual ITableMutator^ CreateAsyncMutator( AsyncResult^ asyncResult, MutatorSpec^ mutatorSpec );
			virtual ITableScanner^ CreateScanner( );
			virtual ITableScanner^ CreateScanner( ScanSpec^ scanSpec );
			virtual int64_t BeginScan( AsyncResult^ asyncResult );
			virtual int64_t BeginScan( AsyncResult^ asyncResult, ScanSpec^ scanSpec );
			virtual int64_t BeginScan( AsyncResult^ asyncResult, ScanSpec^ scanSpec, Object^ param );
			virtual int64_t BeginScan( AsyncResult^ asyncResult, AsyncScannerCallback^ callback );
			virtual int64_t BeginScan( AsyncResult^ asyncResult, ScanSpec^ scanSpec, AsyncScannerCallback^ callback );
			virtual int64_t BeginScan( AsyncResult^ asyncResult, ScanSpec^ scanSpec, Object^ param, AsyncScannerCallback^ callback );
			virtual Xml::TableSchema^ GetTableSchema( );

			#pragma endregion

			/// <summary>
			/// Returns a string that represents the current object.
			/// </summary>
			/// <returns>A string that represents the current object.</returns>
			virtual String^ ToString() override;

		internal:

			Table( Common::Table* table );

		private:

			static Common::ScanSpec* From( ScanSpec^ scanSpec, UInt32& timeout, UInt32& flags );

			Common::Table* table;
			bool disposed;
	};

}