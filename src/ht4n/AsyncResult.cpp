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

#include "stdafx.h"

#include "AsyncResult.h"
#include "ITable.h"
#include "ITableMutator.h"
#include "ScanSpec.h"
#include "Cell.h"
#include "AsyncScannerContext.h"
#include "AsyncMutatorContext.h"
#include "CrossAppDomainFunc.h"
#include "Exception.h"

#include "ht4c.Common/Cell.h"
#include "ht4c.Common/Cells.h"
#include "ht4c.Common/AsyncResult.h"
#include "ht4c.Common/AsyncResultSink.h"

#include "ht4c.Hyper/HyperAsyncResult.h"
#include "ht4c.Thrift/ThriftAsyncResult.h"

namespace Hypertable {
	using namespace System;
	using namespace ht4c;

	namespace {

		class AsyncScannerCtx;

		typedef CrossAppDomainFunc<AsyncScannerCallback^, AsyncScannerCtx*, Common::AsyncCallbackResult> CrossAppDomainAsyncScannerCallbackBase;

		/// <summary>
		/// Application domain aware scanner callback.
		/// </summary>
		class CrossAppDomainAsyncScannerCallback : public CrossAppDomainAsyncScannerCallbackBase
																						 , public CrossAppDomainAsyncScannerCallbackBase::Invoker {

			public:

				CrossAppDomainAsyncScannerCallback( AsyncScannerCallback^ callback )
					: CrossAppDomainAsyncScannerCallbackBase( this, callback )
				{
				}

				inline Common::AsyncCallbackResult invoke( AsyncScannerCtx* ctx ) {
					return CrossAppDomainAsyncScannerCallbackBase::invoke( ctx );
				}

			protected:

				virtual Common::AsyncCallbackResult invoke( AsyncScannerCallback^ callback, AsyncScannerCtx* ctx );
		};


		/// <summary>
		/// Base class for table related asynchronous operation context.
		/// </summary>
		template< typename T >
		class AsyncCtx {

			public:

				AsyncCtx( )
					: appDomain( AppDomain::CurrentDomain->Id )
					
				{
				}

				virtual ~AsyncCtx() {
				}

				static void free( T* ctx ) {
					if( ctx ) {
						if( ctx->appDomain == AppDomain::CurrentDomain->Id ) {
							delete ctx;
						}
						else {
							msclr::call_in_appdomain<T*>( ctx->appDomain, &_free, ctx );
						}
					}
				}

			private:

				static void _free( T* ctx ) {
					delete ctx;
				}

				int appDomain;
		};


		/// <summary>
		/// Asynchronous scanner context.
		/// </summary>
		class AsyncScannerCtx : public AsyncCtx<AsyncScannerCtx> {

			public:

				gcroot<AsyncScannerContext^> ctx;
				Common::Cells* cells;
				CrossAppDomainAsyncScannerCallback* callback;

				AsyncScannerCtx( AsyncScannerContext^ _ctx, AsyncScannerCallback^ _callback )
				: AsyncCtx<AsyncScannerCtx>( )
				, ctx( _ctx )
				, cells( 0 )
				, callback( _callback != nullptr ? new CrossAppDomainAsyncScannerCallback(_callback) : 0 )
				{
				}

				virtual ~AsyncScannerCtx( ) {
					if( callback ) {
						delete callback;
					}
				}
		};


		/// <summary>
		/// Asynchronous mutator context.
		/// </summary>
		class AsyncMutatorCtx : public AsyncCtx<AsyncMutatorCtx> {

			public:

				gcroot<AsyncMutatorContext^> ctx;

				AsyncMutatorCtx( AsyncMutatorContext^ _ctx )
				: AsyncCtx<AsyncMutatorCtx>( )
				, ctx( _ctx )
				{
				}
		};

		Common::AsyncCallbackResult CrossAppDomainAsyncScannerCallback::invoke( AsyncScannerCallback^ callback, AsyncScannerCtx* ctx ) {
			Common::Cell* cell = Common::Cell::create();
			try {
				const Common::Cells& _cells = *ctx->cells;
				List<Cell^>^ cells = gcnew List<Cell^>( (int)_cells.size() );
				for( size_t n = 0; n < _cells.size(); ++n ) {
					_cells.get_unchecked( n, cell );
					cells->Add( gcnew Cell(cell) );
				}
				return static_cast<Common::AsyncCallbackResult>( callback->Invoke(ctx->ctx, cells) );
			}
			finally {
				delete cell;
			}
		}

	}


	/// <summary>
	/// Asynchronous opreation result sink.
	/// </summary>
	class AsyncResultSink : public Common::AsyncResultSink
	{

		public:

			AsyncResultSink( )
			: callback( nullptr )
			, exception( 0 )
			, resetException( false )
			{
				::InitializeCriticalSection( &async_scanner_crit );
				::InitializeCriticalSection( &async_mutator_crit );
			}

			explicit AsyncResultSink( AsyncScannerCallback^ _callback )
			: callback( _callback )
			, exception( 0 )
			, resetException( false )
			{
				::InitializeCriticalSection( &async_scanner_crit );
				::InitializeCriticalSection( &async_mutator_crit );
			}

			virtual ~AsyncResultSink( ) {
				freeAsyncScannerCtx();
				freeAsyncMutatorCtx();
				if( exception ) {
					delete exception;
				}
				::DeleteCriticalSection( &async_scanner_crit );
				::DeleteCriticalSection( &async_mutator_crit );
			}

			void attachAsyncScanner( AsyncScannerContext^ asyncScannerContext, AsyncScannerCallback^ callback ) {
				AsyncScannerCtx* ctx = new AsyncScannerCtx( asyncScannerContext, callback );
				::EnterCriticalSection( &async_scanner_crit );
				try {
					async_scanner_map_t::iterator it = async_scanner_map.find( asyncScannerContext->Id );
					if( it == async_scanner_map.end() ) {
						async_scanner_map.insert( async_scanner_map_t::value_type(asyncScannerContext->Id, ctx) );
					}
					else {
						AsyncScannerCtx::free( (*it).second );
						(*it).second = ctx;
					}
				}
				finally {
					::LeaveCriticalSection( &async_scanner_crit );
				}
			}

			void attachAsyncMutator( AsyncMutatorContext^ asyncMutatorContext ) {
				AsyncMutatorCtx* ctx = new AsyncMutatorCtx( asyncMutatorContext );
				::EnterCriticalSection( &async_mutator_crit );
				try {
					async_mutator_map_t::iterator it = async_mutator_map.find( asyncMutatorContext->Id );
					if( it == async_mutator_map.end() ) {
						async_mutator_map.insert( async_mutator_map_t::value_type(asyncMutatorContext->Id, ctx) );
					}
					else {
						AsyncMutatorCtx::free( (*it).second );
						(*it).second = ctx;
					}
				}
				finally {
					::LeaveCriticalSection( &async_mutator_crit );
				}
			}

			Common::HypertableException* error( ) const {
				return exception;
			}

			void resetError( ) {
				resetException = true;
			}

		private:

			virtual void detachAsyncScanner( int64_t asyncScannerId ) {
				freeAsyncScannerCtx( asyncScannerId );
			}

			virtual void detachAsyncMutator( int64_t asyncMutatorId ) {
				freeAsyncMutatorCtx( asyncMutatorId );
			}

			virtual Common::AsyncCallbackResult scannedCells( int64_t asyncScannerId, Common::Cells& cells ) {
				Common::Cell* _cell = 0;
				AsyncScannerCtx* ctx = 0;

				::EnterCriticalSection( &async_scanner_crit );
				try {
					ctx = async_scanner_map[asyncScannerId];
				}
				finally {
					::LeaveCriticalSection( &async_scanner_crit );
				}

				if( ctx ) {
					HT4N_TRY {
						ctx->cells = &cells;
						if( ctx->callback ) {
							return ctx->callback->invoke( ctx );
						}
						else {
							return callback.invoke( ctx );
						}
					}
					HT4N_RETHROW
					finally {
						ctx->cells = 0;
					}
				}
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

			void freeAsyncScannerCtx( ) {
				::EnterCriticalSection( &async_scanner_crit );
				try {
					for( async_scanner_map_t::iterator it = async_scanner_map.begin(); it != async_scanner_map.end(); ++it ) {
						AsyncScannerCtx::free( (*it).second );
					}
				}
				finally {
					::LeaveCriticalSection( &async_scanner_crit );
				}
			}

			void freeAsyncScannerCtx( int64_t asyncScannerId ) {
				::EnterCriticalSection( &async_scanner_crit );
				try {
					async_scanner_map_t::iterator it = async_scanner_map.find( asyncScannerId );
					if( it != async_scanner_map.end() ) {
						AsyncScannerCtx::free( (*it).second );
						async_scanner_map.erase( it );
					}
				}
				finally {
					::LeaveCriticalSection( &async_scanner_crit );
				}
			}

			void freeAsyncMutatorCtx( ) {
				::EnterCriticalSection( &async_mutator_crit );
				try {
					for( async_mutator_map_t::iterator it = async_mutator_map.begin(); it != async_mutator_map.end(); ++it ) {
						AsyncMutatorCtx::free( (*it).second );
					}
				}
				finally {
					::LeaveCriticalSection( &async_mutator_crit );
				}
			}

			void freeAsyncMutatorCtx( int64_t asyncMutatorId ) {
				::EnterCriticalSection( &async_mutator_crit );
				try {
					async_mutator_map_t::iterator it = async_mutator_map.find( asyncMutatorId );
					if( it != async_mutator_map.end() ) {
						AsyncMutatorCtx::free( (*it).second );
						async_mutator_map.erase( it );
					}
				}
				finally {
					::LeaveCriticalSection( &async_mutator_crit );
				}
			}

			AsyncResultSink( const AsyncResultSink& );
			AsyncResultSink& operator = ( const AsyncResultSink& );

			CrossAppDomainAsyncScannerCallback callback;
			Common::HypertableException* exception;
			bool resetException;

			typedef std::map<int64_t, AsyncScannerCtx*> async_scanner_map_t;
			async_scanner_map_t async_scanner_map;
			CRITICAL_SECTION async_scanner_crit;

			typedef std::map<int64_t, AsyncMutatorCtx*> async_mutator_map_t;
			async_mutator_map_t async_mutator_map;
			CRITICAL_SECTION async_mutator_crit;
	};

	Object^ AsyncResult::AsyncState::get( ) {
		// FIXME
		throw gcnew System::NotImplementedException( );
	}

	WaitHandle^ AsyncResult::AsyncWaitHandle::get( ) {
		// FIXME
		throw gcnew System::NotImplementedException( );
	}

	bool AsyncResult::IsCompleted::get( ) {
		HT4N_TRY {
			if( asyncResult ) {
				for( int n = 0; n < size; ++n ) {
					if( asyncResult[n] && !asyncResult[n]->isCompleted() ) {
						return false;
					}
				}
			}
			return true;
		}
		HT4N_RETHROW
	}

	bool AsyncResult::IsCancelled::get( ) {
		HT4N_TRY {
			if( asyncResult ) {
				for( int n = 0; n < size; ++n ) {
					if( asyncResult[n] && asyncResult[n]->isCancelled() ) {
						return true;
					}
				}
			}
			return false;
		}
		HT4N_RETHROW
	}

	AsyncScannerCallback^ AsyncResult::ScannerCallback::get( ) {
		return scannerCallback;
	}

	System::Exception^ AsyncResult::Error::get( ) {
		HT4N_TRY {
			if( asyncResultSink && asyncResultSink->error() ) {
				System::Exception^ exception = HypertableException::Create( *asyncResultSink->error() );
				asyncResultSink->resetError();
				return exception;
			}
			return nullptr;
		}
		HT4N_RETHROW
	}

	AsyncResult::AsyncResult( )
	: asyncResultSink( new AsyncResultSink() )
	, asyncResult( new Common::AsyncResult*[size] )
	, mutators( gcnew List<WeakReference^>() )
	, scannerCallback( nullptr )
	, disposed( false )
	{
		ZeroMemory( asyncResult, sizeof(Common::AsyncResult*) * size );
	}

	AsyncResult::AsyncResult( AsyncScannerCallback^ callback )
	: asyncResultSink( new AsyncResultSink(callback) )
	, asyncResult( new Common::AsyncResult*[size] )
	, mutators( gcnew List<WeakReference^>() )
	, scannerCallback( callback )
	, disposed( false )
	{
		ZeroMemory( asyncResult, sizeof(Common::AsyncResult*) * size );
	}

	AsyncResult::~AsyncResult( ) {
		disposed = true;
		this->!AsyncResult();
		GC::SuppressFinalize(this);
	}

	AsyncResult::!AsyncResult( ) {
		HT4N_TRY {
			for( int n = 0; n < size; ++n ) {
				if( asyncResult[n] ) {
					delete asyncResult[n];
					asyncResult[n] = 0;
				}
			}
			delete [] asyncResult;
			asyncResult = 0;

			if( asyncResultSink ) {
				delete asyncResultSink;
				asyncResultSink = 0;
			}
		}
		HT4N_RETHROW
	}

	void AsyncResult::Join( ) {
		HT4N_THROW_OBJECTDISPOSED( );

		HT4N_TRY {
			for( int i = 0; i < mutators->Count; ) {
				WeakReference^ wr = mutators[i];
				if( wr->IsAlive ) {
					ITableMutator^ mutator = dynamic_cast<ITableMutator^>( wr->Target );
					if( !mutator->IsDisposed ) {
						mutator->Flush();
						++i;
						continue;
					}
				}
				mutators->RemoveAt( i );
			}
			if( asyncResult ) {
				for( int n = 0; n < size; ++n ) {
					if( asyncResult[n] ) {
						asyncResult[n]->join();
					}
				}
			}
		}
		HT4N_RETHROW
	}

	void AsyncResult::Cancel( ) {
		HT4N_THROW_OBJECTDISPOSED( );

		HT4N_TRY {
			if( asyncResult ) {
				for( int n = 0; n < size; ++n ) {
					if( asyncResult[n] ) {
						asyncResult[n]->cancel();
					}
				}
			}
		}
		HT4N_RETHROW
	}

	void AsyncResult::CancelAsyncScanner( AsyncScannerContext^ asyncScannerContext ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( asyncScannerContext == nullptr ) throw gcnew ArgumentNullException( L"asyncScannerContext" );
		if( asyncScannerContext->Table == nullptr ) throw gcnew ArgumentException( L"Invalid table", L"asyncScannerContext" );

		HT4N_TRY {
			if( asyncResult ) {
				int n = static_cast<int>( asyncScannerContext->ContextKind );
				if( n < size ) {
					if( asyncResult[n] ) {
						asyncResult[n]->cancelAsyncScanner( asyncScannerContext->Id );
					}
				}
			}
		}
		HT4N_RETHROW
	}

	void AsyncResult::CancelAsyncMutator( AsyncMutatorContext^ asyncMutatorContext ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( asyncMutatorContext == nullptr ) throw gcnew ArgumentNullException( L"asyncMutatorContext" );
		if( asyncMutatorContext->Table == nullptr ) throw gcnew ArgumentException( L"Invalid table", L"asyncMutatorContext" );

		HT4N_TRY {
			if( asyncResult ) {
				int n = static_cast<int>( asyncMutatorContext->ContextKind );
				if( n < size ) {
					if( asyncResult[n] ) {
						asyncResult[n]->cancelAsyncMutator( asyncMutatorContext->Id );
					}
				}
			}
		}
		HT4N_RETHROW
	}

	AsyncResult::AsyncResult( bool createResultSink )
	: asyncResultSink( createResultSink ? new AsyncResultSink() : 0 )
	, asyncResult( new Common::AsyncResult*[size] )
	, mutators( gcnew List<WeakReference^>() )
	, disposed( false )
	{
		ZeroMemory( asyncResult, sizeof(Common::AsyncResult*) * size );
	}

	Common::AsyncResult& AsyncResult::get( Common::ContextKind contextKind ) {
		if( !asyncResult ) throw gcnew InvalidOperationException( );
		if( !asyncResult[contextKind] ) {
			asyncResult[contextKind] = CreateAsyncResult( contextKind, asyncResultSink );
		}
		return *(asyncResult[contextKind]);
	}

	void AsyncResult::AttachAsyncScanner( AsyncScannerContext^ asyncScannerContext, AsyncScannerCallback^ callback ) {
		if( asyncScannerContext == nullptr ) throw gcnew ArgumentNullException( L"asyncScannerContext" );
		if( !asyncResultSink ) throw gcnew InvalidOperationException( L"Async result sink has not been initialized" );
		asyncResultSink->attachAsyncScanner( asyncScannerContext, callback );
		Common::ContextKind contextKind = asyncScannerContext->ContextKind;
		if( asyncResult[contextKind] ) {
			asyncResult[contextKind]->attachAsyncScanner( asyncScannerContext->Id );
		}
	}

	void AsyncResult::AttachAsyncMutator( AsyncMutatorContext^ asyncMutatorContext, ITableMutator^ mutator ) {
		if( asyncMutatorContext == nullptr ) throw gcnew ArgumentNullException( L"asyncMutatorContext" );
		if( mutator == nullptr ) throw gcnew ArgumentNullException( L"mutator" );
		if( !asyncResultSink ) throw gcnew InvalidOperationException( L"Async result sink has not been initialized" );
		asyncResultSink->attachAsyncMutator( asyncMutatorContext );
		mutators->Add( gcnew WeakReference(mutator) );
		Common::ContextKind contextKind = asyncMutatorContext->ContextKind;
		if( asyncResult[contextKind] ) {
			asyncResult[contextKind]->attachAsyncMutator( asyncMutatorContext->Id );
		}
	}

	Common::AsyncResult* AsyncResult::CreateAsyncResult( Common::ContextKind contextKind, Common::AsyncResultSink* _asyncResultSink ) {
		HT4N_TRY {
			if( !_asyncResultSink ) throw gcnew ArgumentNullException( L"asyncResultSink" );
			return    contextKind == Common::CK_Hyper
							? static_cast<Common::AsyncResult*>(Hyper::HyperAsyncResult::create(_asyncResultSink) )
							: static_cast<Common::AsyncResult*>(Thrift::ThriftAsyncResult::create(_asyncResultSink) );
		}
		HT4N_RETHROW
	}

}