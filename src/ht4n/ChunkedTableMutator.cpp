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

#include "stdafx.h"

#include "ChunkedTableMutator.h"
#include "Key.h"
#include "Cell.h"
#include "Exception.h"
#include "CM2U8.h"

#include "ht4c.Common/TableMutator.h"
#include "ht4c.Common/Cells.h"
#include "ht4c.Common/KeyBuilder.h"

namespace Hypertable {
	using namespace System;

	ChunkedTableMutator::~ChunkedTableMutator( ) {
		GC::SuppressFinalize(this);
		this->!ChunkedTableMutator();
	}

	ChunkedTableMutator::!ChunkedTableMutator( ) {
		HT4N_TRY {
			msclr::lock sync( syncRoot );
			if( cellChunk ) {
				SetChunk( true );
				delete cellChunk;
				cellChunk = 0;
			}
		} 
		HT4N_RETHROW
	}

	void ChunkedTableMutator::Set( Key^ key, cli::array<Byte>^ value, bool createRowKey ) {
		if( key == nullptr ) throw gcnew ArgumentNullException( L"key" );
		if( createRowKey || String::IsNullOrEmpty(key->Row) ) {
			key->Row = gcnew String( Common::KeyBuilder().c_str() );
		}
		HT4N_TRY {
			UInt32 len = value != nullptr ? value->Length : 0;
			msclr::lock sync( syncRoot );
			pin_ptr<Byte> pv = len ? &value[0] : nullptr;
			cellChunk->add( CM2U8(key->Row), CM2U8(key->ColumnFamily), CM2U8(key->ColumnQualifier), key->Timestamp, pv, len, (Byte)CellFlag::Default );
			lenTotal += len;
			SetChunk( false );
		}
		HT4N_RETHROW
	}

	void ChunkedTableMutator::Set( Cell^ cell, bool createRowKey ) {
		if( cell == nullptr ) throw gcnew ArgumentNullException( L"cell" );
		Key^ key = cell->Key;
		if( key == nullptr ) throw gcnew ArgumentException( L"Invalid parameter cell (cell.Key null)", L"cell" );
		if( createRowKey || String::IsNullOrEmpty(key->Row) ) {
			key->Row = gcnew String( Common::KeyBuilder().c_str() );
		}
		HT4N_TRY {
			UInt32 len = cell->Value != nullptr ? cell->Value->Length : 0;
			msclr::lock sync( syncRoot );
			pin_ptr<Byte> pv = len ? &cell->Value[0] : nullptr;
			cellChunk->add( CM2U8(key->Row), CM2U8(key->ColumnFamily), CM2U8(key->ColumnQualifier), key->Timestamp, pv, len, (Byte)cell->Flag );
			lenTotal += len;
			SetChunk( false );
		}
		HT4N_RETHROW
	}

	void ChunkedTableMutator::Set( IEnumerable<Cell^>^ cells, bool createRowKey ) {
		if( cells == nullptr ) throw gcnew ArgumentNullException( L"cells" );
		HT4N_TRY {
			for each( Cell^ cell in cells ) {
				if( cell != nullptr ) {
					Key^ key = cell->Key;
					if( key != nullptr ) {
						if( createRowKey || String::IsNullOrEmpty(key->Row) ) {
							key->Row = gcnew String( Common::KeyBuilder().c_str() );
						}
						UInt32 len = cell->Value != nullptr ? cell->Value->Length : 0;
						msclr::lock sync( syncRoot );
						pin_ptr<Byte> pv = len ? &cell->Value[0] : nullptr;
						cellChunk->add( CM2U8(key->Row), CM2U8(key->ColumnFamily), CM2U8(key->ColumnQualifier), key->Timestamp, pv, len, (Byte)cell->Flag );
						lenTotal += len;
						SetChunk( false );
					}
				}
			}
		}
		HT4N_RETHROW
	}

		void ChunkedTableMutator::Delete( String^ row ) {
		if( String::IsNullOrEmpty(row) ) throw gcnew ArgumentException( L"Invalid parameter row (null or empty)", L"row" );
		Set( gcnew Cell(gcnew Key(row), CellFlag::DeleteRow) );
	}

	void ChunkedTableMutator::Delete( Key^ key ) {
		if( key == nullptr ) throw gcnew ArgumentNullException( L"key" );
		Set( gcnew Cell(key, Cell::DeleteFlagFromKey(key), true) );
	}

	void ChunkedTableMutator::Delete( IEnumerable<Key^>^ keys ) {
		if( keys == nullptr ) throw gcnew ArgumentNullException( L"keys" );
		for each( Key^ key in keys ) {
			if( key != nullptr ) {
					Delete( key );
			}
		}
	}

	void ChunkedTableMutator::Delete( IEnumerable<Cell^>^ cells ) {
		if( cells == nullptr ) throw gcnew ArgumentNullException( L"cells" );
		for each( Cell^ cell in cells ) {
			if( cell != nullptr && cell->Key != nullptr ) {
				Delete( cell->Key );
			}
		}
	}

	void ChunkedTableMutator::Flush( ) {
		if( !SetChunk(true) ) {
			TableMutator::Flush();
		}
	}

	ChunkedTableMutator::ChunkedTableMutator( Common::TableMutator* _tableMutator, UInt32 _maxChunkSize, UInt32 _maxCellCount, bool _flushEachChunk )
	: TableMutator( _tableMutator )
	, cellChunk( Common::Cells::create(__min(_maxCellCount, 64 * 1024)) )
	, lenTotal( 0 )
	, maxChunkSize( _maxChunkSize )
	, maxCellCount( _maxCellCount )
	, flushEachChunk( _flushEachChunk )
	{
	}

	bool ChunkedTableMutator::SetChunk( bool force ) {
		if( force || lenTotal >= maxChunkSize || cellChunk->size() >= maxCellCount ) {
			if( cellChunk->size() ) {
				msclr::lock sync( syncRoot );
				HT4N_TRY {
					tableMutator->set( *cellChunk );
					if( flushEachChunk ) {
						TableMutator::Flush();
						return true;
					}
				}
				HT4N_RETHROW
				finally {
					cellChunk->clear();
					lenTotal = 0;
				}
			}
		}
		return false;
	}

}