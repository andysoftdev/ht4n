/** -*- C++ -*-
 * Copyright (C) 2010-2014 Thalmann Software & Consulting, http://www.softdev.ch
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

#include "QueuedTableMutator.h"
#include "Key.h"
#include "Cell.h"
#include "Exception.h"
#include "Logging.h"

#include "ht4c.Common/KeyBuilder.h"

namespace Hypertable {
	using namespace System;
	using namespace System::Diagnostics;

	QueuedTableMutator::~QueuedTableMutator( ) {
		disposed = true;
		GC::SuppressFinalize(this);
		this->!QueuedTableMutator();
	}

	QueuedTableMutator::!QueuedTableMutator( ) {
		bc->CompleteAdding();
		task->Wait();
		delete bc;
		delete task;
		delete mre;
		delete inner;
	}

	void QueuedTableMutator::Set( Key^ key, cli::array<Byte>^ value ) {
		Set( key, value, false );
	}

	void QueuedTableMutator::Set( Key^ key, cli::array<Byte>^ value, bool createRowKey ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( key == nullptr ) throw gcnew ArgumentNullException( L"key" );
		if( createRowKey || String::IsNullOrEmpty(key->Row) ) {
			key->Row = gcnew String( Common::KeyBuilder().c_str() );
		}
		AddCell( gcnew Cell(key, value, true) );
	}

	void QueuedTableMutator::Set( Cell^ cell ) {
		if( cell == nullptr ) throw gcnew ArgumentNullException( L"cell" );
		Set( cell, false );
	}

	void QueuedTableMutator::Set( Cell^ cell, bool createRowKey ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( cell == nullptr ) throw gcnew ArgumentNullException( L"cell" );
		Key^ key = cell->Key;
		if( key == nullptr ) throw gcnew ArgumentException( L"Invalid parameter cell (cell.Key null)", L"cell" );
		if( createRowKey || String::IsNullOrEmpty(key->Row) ) {
			key->Row = gcnew String( Common::KeyBuilder().c_str() );
		}
		AddCell( gcnew Cell(key, cell->Value, cell->Flag, true) );
	}

	void QueuedTableMutator::Set( IEnumerable<Cell^>^ cells ) {
		Set( cells, false );
	}

	void QueuedTableMutator::Set( IEnumerable<Cell^>^ cells, bool createRowKey ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( cells == nullptr ) throw gcnew ArgumentNullException( L"cells" );
		for each( Cell^ cell in cells ) {
			if( cell != nullptr ) {
				Key^ key = cell->Key;
				if( key != nullptr ) {
					if( createRowKey || String::IsNullOrEmpty(key->Row) ) {
						key->Row = gcnew String( Common::KeyBuilder().c_str() );
					}
					AddCell( gcnew Cell(key, cell->Value, cell->Flag, true) );
				}
			}
		}
	}

	void QueuedTableMutator::Delete( String^ row ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( String::IsNullOrEmpty(row) ) throw gcnew ArgumentException( L"Invalid parameter row (null or empty)", L"row" );
		AddCell( gcnew Cell(gcnew Key(row), CellFlag::DeleteRow) );
	}

	void QueuedTableMutator::Delete( Key^ key ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( key == nullptr ) throw gcnew ArgumentNullException( L"key" );
		AddCell( gcnew Cell(key, Cell::DeleteFlagFromKey(key), true) );
	}

	void QueuedTableMutator::Delete( IEnumerable<Key^>^ keys ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( keys == nullptr ) throw gcnew ArgumentNullException( L"keys" );
		for each( Key^ key in keys ) {
			if( key != nullptr ) {
					Delete( key );
			}
		}
	}

	void QueuedTableMutator::Delete( IEnumerable<Cell^>^ cells ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( cells == nullptr ) throw gcnew ArgumentNullException( L"cells" );
		for each( Cell^ cell in cells ) {
			if( cell != nullptr && cell->Key !=nullptr ) {
				Delete( cell->Key );
			}
		}
	}

	void QueuedTableMutator::Flush( ) {
		HT4N_THROW_OBJECTDISPOSED( );

		mre->WaitOne();
		inner->Flush();
	}

	QueuedTableMutator::QueuedTableMutator( ITableMutator^ _inner, int capacity )
	: task( nullptr )
	, bc( capacity > 0 ? gcnew BlockingCollection<Cell^>(capacity) : gcnew BlockingCollection<Cell^>() )
	, mre( gcnew ManualResetEvent(true) )
	, inner( _inner )
	, disposed( false )
	{
		if( inner == nullptr ) throw gcnew ArgumentNullException( L"inner" );
		task = Task::Factory->StartNew( gcnew Action(this, &Hypertable::QueuedTableMutator::SetCell) );
	}

	void QueuedTableMutator::AddCell( Cell^ cell ) {
		mre->Reset();
		bc->Add( cell );
	}

	void QueuedTableMutator::SetCell() {
		try {
			while( true ) {
				if( bc->Count == 0 ) {
					mre->Set();
				}
				inner->Set(bc->Take());
			}
		}
		catch( InvalidOperationException^ ) {
		}
		catch( AggregateException^ aggregateException ) {
				for each( Exception^ e in aggregateException->Flatten()->InnerExceptions ) {
						Logging::TraceException( e );
				}
				throw;
		}
		catch( Exception^ e ) {
			Logging::TraceException( e );
			throw;
		}
	}

}