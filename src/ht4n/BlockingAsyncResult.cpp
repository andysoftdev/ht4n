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

#include "stdafx.h"

#include "BlockingAsyncResult.h"
#include "Cell.h"
#include "AsyncScannerContext.h"
#include "AsyncMutatorContext.h"
#include "Exception.h"

#include "ht4c.Common/Cell.h"
#include "ht4c.Common/Cells.h"
#include "ht4c.Common/AsyncResultSink.h"

#include "ht4c.Hyper/HyperBlockingAsyncResult.h"
#include "ht4c.Thrift/ThriftBlockingAsyncResult.h"

namespace Hypertable {
	using namespace System;
	using namespace ht4c;

	class BlockingAsyncResultSink : public Common::AsyncResultSink {

		public:

			explicit BlockingAsyncResultSink( List<Cell^>^ _result )
			: result( _result )
			, asyncScannerId( 0 )
			, exception( 0 )
			, resetException( false )
			{
				if( _result == nullptr ) throw gcnew ArgumentNullException( L"_result" );
			}

			virtual ~BlockingAsyncResultSink( ) {
				if( exception ) {
					delete exception;
				}
			}

			int64_t getAsyncScannerId( ) const {
				return asyncScannerId;
			}

			Common::HypertableException* error( ) const {
				return exception;
			}

			void resetError( ) {
				resetException = true;
			}

		private:

			virtual void detachAsyncScanner( int64_t /*asyncScannerId*/ ) {
			}

			virtual void detachAsyncMutator( int64_t /*asyncMutatorId*/ ) {
			}

			virtual Common::AsyncCallbackResult scannedCells( int64_t _asyncScannerId, Common::Cells& cells ) {
				Common::Cell* _cell = 0;
				try {
					result->Capacity = (int)cells.size();
					_cell = Common::Cell::create();
					for( size_t n = 0; n < cells.size(); ++n ) {
						cells.get_unchecked( n, _cell );
						result->Add( gcnew Cell(_cell) );
					}
				}
				finally {
					if( _cell ) delete _cell;
				}
				asyncScannerId = _asyncScannerId;
				return Common::ACR_Continue;
			}

			virtual void failure( Common::HypertableException& e ) {
				if( resetException && exception ) {
					delete exception;
					exception = 0;
					resetException = false;
				}
				if( !exception ) {
					exception = new Common::HypertableException( e );
				}
			}

			BlockingAsyncResultSink( const BlockingAsyncResultSink& );
			BlockingAsyncResultSink& operator = ( const BlockingAsyncResultSink& );

			gcroot<List<Cell^>^> result;
			int64_t asyncScannerId;
			Common::HypertableException* exception;
			bool resetException;
	};

	BlockingAsyncResult::BlockingAsyncResult( )
	: AsyncResult( false )
	, capacity( 0 )
	, map( gcnew Dictionary<int64_t, AsyncScannerContext^>() )
	, syncRoot( gcnew Object() )
	{
	}

	BlockingAsyncResult::BlockingAsyncResult( size_t _capacity )
	: AsyncResult( false )
	, capacity( _capacity )
	, map( gcnew Dictionary<int64_t, AsyncScannerContext^>() )
	, syncRoot( gcnew Object() )
	{
		if( capacity < 0 ) throw gcnew ArgumentException( L"Invalid capcacity specified", L"capacity" );
	}

	bool BlockingAsyncResult::TryGetCells( IList<Cell^>^% cells ) {
		AsyncScannerContext^ asyncScannerContext;
		return TryGetCells( asyncScannerContext, cells );
	}

	bool BlockingAsyncResult::TryGetCells( AsyncScannerContext^% asyncScannerContext, IList<Cell^>^% cells ) {
		asyncScannerContext = nullptr;

		BlockingAsyncResultSink* asyncResultSink = 0;
		HT4N_TRY {
			List<Cell^>^ l = gcnew List<Cell^>();
			cells = l;
			asyncResultSink = new BlockingAsyncResultSink( l );
			std::vector<bool> completed(size, false);
			for( int probe = 0; probe < 2; ++probe ) {
				for( int n = 0; n < size; ++n ) {
					if( !completed[n] ) {
						Common::BlockingAsyncResult* blockingAsyncResult = GetAsyncResult<Common::BlockingAsyncResult>( n );
						if( blockingAsyncResult ) {
							if( probe > 0 || !blockingAsyncResult->isEmpty() ) {
								if( blockingAsyncResult->getCells(asyncResultSink) ) {
									msclr::lock sync( syncRoot );
									map->TryGetValue( asyncResultSink->getAsyncScannerId(), asyncScannerContext );
									return true;
								}
								completed[n] = true;
							}
						}
						else {
							completed[n] = true;
						}
					}
				}
			}
		}
		HT4N_RETHROW
		finally {
			if( asyncResultSink ) delete asyncResultSink;
		}
		return false;
	}

	bool BlockingAsyncResult::TryGetCells( TimeSpan timeout, IList<Cell^>^% cells ) {
		AsyncScannerContext^ asyncScannerContext;
		return TryGetCells( timeout, asyncScannerContext, cells );
	}

	bool BlockingAsyncResult::TryGetCells( TimeSpan timeout, AsyncScannerContext^% asyncScannerContext, IList<Cell^>^% cells ) {
		asyncScannerContext = nullptr;

		BlockingAsyncResultSink* asyncResultSink = 0;
		HT4N_TRY {
			List<Cell^>^ l = gcnew List<Cell^>();
			cells = l;
			asyncResultSink = new BlockingAsyncResultSink( l );
			std::vector<bool> completed(size, false);
			for( int probe = 0; probe < 2; ++probe ) {
				for( int n = 0; n < size; ++n ) {
					if( !completed[n] ) {
						Common::BlockingAsyncResult* blockingAsyncResult = GetAsyncResult<Common::BlockingAsyncResult>( n );
						if( blockingAsyncResult ) {
							if( probe > 0 || !blockingAsyncResult->isEmpty() ) {
								int32_t _timeout = (int32_t)timeout.TotalMilliseconds;
								bool timedOut;
								bool result = blockingAsyncResult->getCells( asyncResultSink, _timeout, timedOut );
								if( timedOut ) {
									throw gcnew Hypertable::TimeoutException( L"Asynchronous operations have timed out" );
								}
								if( result ) {
									msclr::lock sync( syncRoot );
									map->TryGetValue( asyncResultSink->getAsyncScannerId(), asyncScannerContext );
									return true;
								}
								completed[n] = true;
							}
						}
						else {
							completed[n] = true;
						}
					}
				}
			}
		}
		HT4N_RETHROW
		finally {
			if( asyncResultSink ) delete asyncResultSink;
		}
		return false;
	}

	void BlockingAsyncResult::AttachAsyncScanner( AsyncScannerContext^ asyncScannerContext, AsyncScannerCallback^ ) {
		if( asyncScannerContext == nullptr ) throw gcnew ArgumentNullException( L"asyncScannerContext" );
		msclr::lock sync( syncRoot );
		map[asyncScannerContext->Id] = asyncScannerContext;
		Common::BlockingAsyncResult* blockingAsyncResult = GetAsyncResult<Common::BlockingAsyncResult>( asyncScannerContext->ContextKind );
		if( blockingAsyncResult ) {
			blockingAsyncResult->attachAsyncScanner( asyncScannerContext->Id );
		}
	}

	void BlockingAsyncResult::AttachAsyncMutator( AsyncMutatorContext^ asyncMutatorContext, ITableMutator^ mutator ) {
		if( asyncMutatorContext == nullptr ) throw gcnew ArgumentNullException( L"asyncMutatorContext" );
		if( mutator == nullptr ) throw gcnew ArgumentNullException( L"mutator" );
		AddAsyncMutatorWeakReference( mutator );
		Common::BlockingAsyncResult* blockingAsyncResult = GetAsyncResult<Common::BlockingAsyncResult>( asyncMutatorContext->ContextKind );
		if( blockingAsyncResult ) {
			blockingAsyncResult->attachAsyncMutator( asyncMutatorContext->Id );
		}
	}

	Common::AsyncResult* BlockingAsyncResult::CreateAsyncResult( Common::ContextKind contextKind, Common::AsyncResultSink* asyncResultSink ) {
		HT4N_TRY {
			return		contextKind == Common::CK_Hyper
							? static_cast<Common::AsyncResult*>(Hyper::HyperBlockingAsyncResult::create(capacity) )
							: static_cast<Common::AsyncResult*>(Thrift::ThriftBlockingAsyncResult::create(capacity) );
		}
		HT4N_RETHROW
	}

}