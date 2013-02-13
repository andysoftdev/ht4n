/** -*- C++ -*-
 * Copyright (C) 2010-2013 Thalmann Software & Consulting, http://www.softdev.ch
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

#include "stdafx.h"

#include "Table.h"
#include "MutatorSpec.h"
#include "TableMutator.h"
#include "ChunkedTableMutator.h"
#include "QueuedTableMutator.h"
#include "ScanSpec.h"
#include "TableScanner.h"
#include "AsyncResult.h"
#include "BlockingAsyncResult.h"
#include "AsyncScannerContext.h"
#include "AsyncMutatorContext.h"
#include "Exception.h"
#include "CM2U8.h"

#include "ht4c.Common/Types.h"
#include "ht4c.Common/Table.h"
#include "ht4c.Common/TableMutator.h"
#include "ht4c.Common/AsyncTableMutator.h"
#include "ht4c.Common/MutatorFlags.h"
#include "ht4c.Common/ScanSpec.h"

namespace Hypertable {
	using namespace System;
	using namespace System::Globalization;
	using namespace ht4c;

	Table::~Table( ) {
		disposed = true;
		this->!Table();
		GC::SuppressFinalize(this);
	}

	Table::!Table( ) {
		HT4N_TRY {
			if( table ) {
				delete table;
				table = 0;
			}
		}
		HT4N_RETHROW
	}

	String^ Table::Name::get( ) {
		HT4N_THROW_OBJECTDISPOSED( );

		HT4N_TRY {
			return CM2U8::ToString( table->getName().c_str() );
		}
		HT4N_RETHROW
	}

	String^ Table::Schema::get( ) {
		HT4N_THROW_OBJECTDISPOSED( );

		HT4N_TRY {
			return CM2U8::ToString( table->getSchema().c_str() );
		} 
		HT4N_RETHROW
	}

	ITableMutator^ Table::CreateMutator( ) {
		return CreateMutator( nullptr );
	}

	ITableMutator^ Table::CreateMutator( MutatorSpec^ mutatorSpec ) {
		HT4N_THROW_OBJECTDISPOSED( );

		HT4N_TRY {
			uint32_t timeout = 0;
			uint32_t flags = ht4c::Common::MF_Default;
			uint32_t flushInterval = 0;
			if( mutatorSpec != nullptr ) {
				if( mutatorSpec->Timeout.Ticks ) {
					if( mutatorSpec->Timeout.TotalMilliseconds < 0 ) throw gcnew ArgumentException( L"Invalid parameter mutatorSpec (Timeout < 0)", L"mutatorSpec" );
					timeout = (uint32_t)mutatorSpec->Timeout.TotalMilliseconds;
				}
				if( mutatorSpec->FlushInterval.Ticks ) {
					if( mutatorSpec->FlushInterval.TotalMilliseconds < 0 ) throw gcnew ArgumentException( L"Invalid parameter mutatorSpec (FlushInterval < 0)", L"mutatorSpec" );
					flushInterval = (uint32_t)mutatorSpec->FlushInterval.TotalMilliseconds;
				}

				flags = (uint32_t) mutatorSpec->Flags;

				ITableMutator^ mutator = nullptr;
				switch( mutatorSpec->MutatorKind ) {
					case MutatorKind::Default:
						mutator = gcnew TableMutator( table->createMutator(timeout, flags, flushInterval) );
						break;
					case MutatorKind::Chunked:
						mutator = gcnew ChunkedTableMutator( table->createMutator(timeout, flags, flushInterval), mutatorSpec->MaxChunkSize, mutatorSpec->MaxCellCount, mutatorSpec->FlushEachChunk );
						break;
				}

				if( mutatorSpec->Queued ) {
					mutator = gcnew QueuedTableMutator( mutator, mutatorSpec->Capacity );
				}

				return mutator;
			}
			return gcnew TableMutator( table->createMutator(timeout, flags, flushInterval) );
		} 
		HT4N_RETHROW
	}

	ITableMutator^ Table::CreateAsyncMutator( AsyncResult^ asyncResult, MutatorSpec^ mutatorSpec ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( asyncResult == nullptr ) throw gcnew ArgumentNullException( L"asyncResult" );
		HT4N_TRY {
			const Common::ContextKind contextKind = table->getContextKind();
			ITableMutator^ mutator = nullptr;
			Common::AsyncTableMutator* asyncMutator = 0;
			uint32_t timeout = 0;
			uint32_t flags = ht4c::Common::MF_Default;
			if( mutatorSpec != nullptr ) {
				if( mutatorSpec->Timeout.Ticks ) {
					if( mutatorSpec->Timeout.TotalMilliseconds < 0 ) throw gcnew ArgumentException( L"Invalid parameter mutatorSpec (Timeout < 0)", L"mutatorSpec" );
					timeout = (uint32_t)mutatorSpec->Timeout.TotalMilliseconds;
				}

				flags = (uint32_t) mutatorSpec->Flags;

				asyncMutator = table->createAsyncMutator( asyncResult->get(contextKind), timeout, flags );
				switch( mutatorSpec->MutatorKind ) {
					case MutatorKind::Default:
						mutator = gcnew TableMutator( asyncMutator );
						break;
					case MutatorKind::Chunked:
						mutator = gcnew ChunkedTableMutator( asyncMutator, mutatorSpec->MaxChunkSize, mutatorSpec->MaxCellCount, mutatorSpec->FlushEachChunk );
						break;
				}

				if( mutatorSpec->Queued ) {
					mutator = gcnew QueuedTableMutator( mutator, mutatorSpec->Capacity );
				}
			}
			else {
				asyncMutator = table->createAsyncMutator( asyncResult->get(contextKind), timeout, flags );
				mutator = gcnew TableMutator( asyncMutator );
			}
			asyncResult->AttachAsyncMutator( gcnew AsyncMutatorContext(contextKind, asyncMutator->id(), this, mutatorSpec), mutator );
			return mutator;
		} 
		HT4N_RETHROW
	}

	ITableScanner^ Table::CreateScanner( ) {
		return CreateScanner( nullptr );
	}

	ITableScanner^ Table::CreateScanner( ScanSpec^ scanSpec ) {
		HT4N_THROW_OBJECTDISPOSED( );

		Common::ScanSpec* _scanSpec = 0;
		HT4N_TRY {
			uint32_t timeout;
			uint32_t flags;
			_scanSpec = From( scanSpec, timeout, flags );

			return gcnew TableScanner( table->createScanner(*_scanSpec, timeout, flags), scanSpec );
		}
		HT4N_RETHROW
		finally {
			if( _scanSpec ) delete _scanSpec;
		}
	}

	int64_t Table::BeginScan( AsyncResult^ asyncResult ) {
		return BeginScan( asyncResult, nullptr, nullptr, nullptr );
	}

	int64_t Table::BeginScan( AsyncResult^ asyncResult, ScanSpec^ scanSpec ) {
		return BeginScan( asyncResult, scanSpec, nullptr, nullptr );
	}

	int64_t Table::BeginScan( AsyncResult^ asyncResult, ScanSpec^ scanSpec, Object^ param ) {
		return BeginScan( asyncResult, scanSpec, param, nullptr );
	}

	int64_t Table::BeginScan( AsyncResult^ asyncResult, AsyncScannerCallback^ callback ) {
		return BeginScan( asyncResult, nullptr, nullptr, callback );
	}

	int64_t Table::BeginScan( AsyncResult^ asyncResult, ScanSpec^ scanSpec, AsyncScannerCallback^ callback ) {
		return BeginScan( asyncResult, scanSpec, nullptr, callback );
	}

	int64_t Table::BeginScan( AsyncResult^ asyncResult, ScanSpec^ scanSpec, Object^ param, AsyncScannerCallback^ callback ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( asyncResult == nullptr ) throw gcnew ArgumentNullException( L"asyncResult" );
		BlockingAsyncResult^ blockingAsyncResult = dynamic_cast<BlockingAsyncResult^>( asyncResult );
		if( callback == nullptr && asyncResult->ScannerCallback == nullptr && blockingAsyncResult == nullptr ) throw gcnew ArgumentNullException( L"callback" );
		if( callback != nullptr && blockingAsyncResult != nullptr ) throw gcnew ArgumentException( L"Callback must be null for blocking async results", L"callback" );
		Common::ScanSpec* _scanSpec = 0;
		HT4N_TRY {
			const Common::ContextKind contextKind = table->getContextKind();
			uint32_t timeout;
			uint32_t flags;
			_scanSpec = From( scanSpec, timeout, flags );
			int64_t asyncScannerId = table->createAsyncScannerId( *_scanSpec, asyncResult->get(contextKind), timeout, flags );
			if( asyncScannerId ) {
				asyncResult->AttachAsyncScanner( gcnew AsyncScannerContext(contextKind, asyncScannerId, this, scanSpec, param), callback );
				return asyncScannerId;
			}
		}
		HT4N_RETHROW
		finally {
			if( _scanSpec ) delete _scanSpec;
		}
		return 0;
	}

	String^ Table::ToString() {
		HT4N_THROW_OBJECTDISPOSED( );

		return String::Format( CultureInfo::InvariantCulture
												 , L"{0}(Name={1})"
												 , GetType()
												 , Name != nullptr ? Name : L"null");
	}

	Table::Table( Common::Table* _table )
	: table( _table )
	, disposed( false )
	{
		if( table == 0 ) throw gcnew ArgumentNullException( L"table" );
	}

	Common::ScanSpec* Table::From( ScanSpec^ scanSpec, UInt32& timeout, UInt32& flags ) {
		timeout = 0;
		flags = ht4c::Common::SF_Default;

		Common::ScanSpec* _scanSpec = Common::ScanSpec::create();
		if( scanSpec != nullptr ) {
			scanSpec->To( *_scanSpec );
			if( scanSpec->Timeout.Ticks ) {
				if( scanSpec->Timeout.TotalMilliseconds < 0 ) throw gcnew ArgumentException( L"Invalid parameter scanSpec (Timeout < 0)", L"scanSpec" );
				timeout = (uint32_t)scanSpec->Timeout.TotalMilliseconds;
			}
			flags = (uint32_t)scanSpec->Flags;
		}
		return _scanSpec;
	}

}