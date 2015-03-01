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

#include "AsyncScannerCallback.h"

#include "ht4c.Common/Types.h"
#include "ht4c.Common/ContextKind.h"

namespace ht4c { namespace Common {
	class Table;
	class AsyncResult;
	class AsyncResultSink;
	class AsyncTableScanner;
} }

namespace Hypertable {
	using namespace System;
	using namespace System::Threading;
	using namespace System::Collections::Generic;
	using namespace ht4c;

	interface class ITable;
	interface class ITableMutator;

	ref class Cell;
	ref class ScanSpec;
	ref class MutatorSpec;
	ref class AsyncScannerContext;
	ref class AsyncMutatorContext;
	ref class HypertableException;

	class AsyncResultSink;

	/// <summary>
	/// Represents results from asynchronous table scan operations.
	/// </summary>
	/// <example>
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
	/// </example>
	public ref class AsyncResult : public IAsyncResult, public IDisposable {

		public:

			/// <summary>
			/// Gets a user-defined object that qualifies or contains information about an asynchronous operation.
			/// </summary>
			/// <remarks>Property has not been implemented so far, throws NotImplException.</remarks>
			property Object^ AsyncState {
				virtual Object^ get( );
			}

			/// <summary>
			/// Gets a WaitHandle that is used to wait for an asynchronous operation to complete.
			/// </summary>
			/// <remarks>Property has not been implemented so far, throws NotImplException.</remarks>
			/// <seealso cref="Join" />
			property WaitHandle^ AsyncWaitHandle {
				virtual WaitHandle^ get( );
			}

			/// <summary>
			/// Gets a value that indicates whether the asynchronous operation completed synchronously.
			/// </summary>
			/// <remarks>Returns always false.</remarks>
			/// <seealso cref="IsCompleted" />
			property bool CompletedSynchronously {
				virtual bool get( ) {
					return false;
				}
			}

			/// <summary>
			/// Gets a value that indicates whether the asynchronous operation has completed.
			/// </summary>
			/// <remarks>Returns false if the asynchronous operation has cancelled.</remarks>
			property bool IsCompleted {
				virtual bool get( );
			}

			/// <summary>
			/// Gets a value that indicates whether the asynchronous operation has cancelled.
			/// </summary>
			property bool IsCancelled {
				bool get( );
			}

			/// <summary>
			/// Gets the scanner callback.
			/// </summary>
			property AsyncScannerCallback^ ScannerCallback {
				AsyncScannerCallback^ get( );
			}

			/// <summary>
			/// Gets a value indicating which error occurred during an asynchronous operation.
			/// </summary>
			property System::Exception^ Error {
				System::Exception^ get( );
			}

			/// <summary>
			/// Gets a value indicating whether the object has been disposed.
			/// </summary>
			/// <remarks>true if the object has been disposed, otherwise false.</remarks>
			property bool IsDisposed {
				bool get( ) {
					return disposed;
				}
			}

			/// <summary>
			/// Initializes a new instance of the AsyncResult class.
			/// </summary>
			AsyncResult( );

			/// <summary>
			/// Initializes a new instance of the AsyncResult class using the specified AsyncScannerCallback.
			/// </summary>
			/// <param name="callback">The AsyncScannerCallback delegate to call when the asynchronous table scan operation returns cells.</param>
			/// <seealso cref="AsyncScannerCallback"/>
			AsyncResult( AsyncScannerCallback^ callback );

			/// <summary>
			/// Clean up all managed and unmanaged resources.
			/// </summary>
			virtual ~AsyncResult( );

			/// <summary>
			/// Clean up all unmanaged resources.
			/// </summary>
			!AsyncResult( );

			/// <summary>
			/// Blocks the calling thread until the asynchronous operation has completed.
			/// </summary>
			void Join( );

			/// <summary>
			/// Cancels any outstanding asynchronous operations.
			/// </summary>
			void Cancel( );

			/// <summary>
			/// Cancels an asynchronous scanner.
			/// </summary>
			/// <param name="asyncScannerContext">Asynchronous scanner context.</param>
			void CancelAsyncScanner( AsyncScannerContext^ asyncScannerContext );

			/// <summary>
			/// Cancels an asynchronous mutator.
			/// </summary>
			/// <param name="asyncMutatorContext">Asynchronous mutator context.</param>
			void CancelAsyncMutator( AsyncMutatorContext^ asyncMutatorContext );

		internal:

			AsyncResult( bool createResultSink );

			Common::AsyncResult& get( Common::ContextKind contextKind );

			virtual void AttachAsyncScanner( AsyncScannerContext^ asyncScannerContext, AsyncScannerCallback^ callback );
			virtual void AttachAsyncMutator( AsyncMutatorContext^ asyncMutatorContext, ITableMutator^ mutator );

			virtual Common::AsyncResult* CreateAsyncResult( Common::ContextKind contextKind, Common::AsyncResultSink* asyncResultSink );

			template< typename T > inline
			T* GetAsyncResult( int n ) {
				return n < size ? dynamic_cast<T*>( asyncResult[n] ) : 0;
			}

			void AddAsyncMutatorWeakReference( ITableMutator^ mutator ) {
				mutators->Add( gcnew WeakReference(mutator) );
			}

			static const int size = Common::CK_Last; //TODO re-design, support also custom providers

		private:

			Common::AsyncResult** asyncResult; // Hyper + Thrift
			AsyncResultSink* asyncResultSink;
			List<WeakReference^>^ mutators;
			AsyncScannerCallback^ scannerCallback;
			bool disposed;
	};

}