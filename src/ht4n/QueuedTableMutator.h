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

#include "ITableMutator.h"

namespace Hypertable {
	using namespace System;
	using namespace System::Threading;
	using namespace System::Threading::Tasks;
	using namespace System::Collections::Concurrent;

	/// <summary>
	/// Represents a asynchronous table mutator.
	/// </summary>
	/// <seealso cref="ITableMutator"/>
	ref class QueuedTableMutator sealed : public ITableMutator {

		public:

			/// <summary>
			/// Clean up all managed and unmanaged resources.
			/// </summary>
			virtual ~QueuedTableMutator( );

			/// <summary>
			/// Clean up all unmanaged resources.
			/// </summary>
			!QueuedTableMutator( );

			#pragma region ITableMutator methods

			property bool IsDisposed {
				virtual bool get( ) {
					return disposed;
				}
			}

			virtual void Set( Key^ key, cli::array<Byte>^ value );
			virtual void Set( Key^ key, cli::array<Byte>^ value, bool createRowKey );

			virtual void Set( Cell^ cell );
			virtual void Set( Cell^ cell, bool createRowKey );

			virtual void Set( IEnumerable<Cell^>^ cells );
			virtual void Set( IEnumerable<Cell^>^ cells, bool createRowKey );

			virtual void Delete( String^ row );
			virtual void Delete( Key^ key );
			virtual void Delete( IEnumerable<Key^>^ keys );
			virtual void Delete( IEnumerable<Cell^>^ cells );

			virtual void Flush();

			#pragma endregion

		internal:

			QueuedTableMutator( ITableMutator^ inner, int capacity );

		private:

			void AddCell( Cell^ cell );
			void SetCell();

			Task^ task;
			BlockingCollection<Cell^>^ bc;
			ManualResetEvent^ mre;
			ITableMutator^ inner;
			Exception^ innerException;
			bool disposed;

			void ThrowIfInnerExceptionOccurred( ) {
				if( innerException != nullptr ) {
					throw gcnew AggregateException( "Queuing have been aborted", innerException );
				}
			}
	};

}