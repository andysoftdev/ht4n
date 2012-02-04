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

#include "stdafx.h"

#include "TableMutator.h"
#include "Key.h"
#include "Cell.h"
#include "Exception.h"
#include "CM2A.h"

#include "ht4c.Common/TableMutator.h"
#include "ht4c.Common/Cells.h"
#include "ht4c.Common/KeyBuilder.h"

namespace Hypertable {
	using namespace System;
	using namespace ht4c;

	TableMutator::~TableMutator( ) {
		disposed = true;
		this->!TableMutator();
		GC::SuppressFinalize(this);
	}

	TableMutator::!TableMutator( ) {
		HT4N_TRY {
			if( tableMutator ) {
				tableMutator->flush();
				delete tableMutator;
				tableMutator = 0;
			}
		} 
		HT4N_RETHROW
	}

	void TableMutator::Set( Key^ key, cli::array<Byte>^ value ) {
		Set( key, value, false );
	}

	void TableMutator::Set( Key^ key, cli::array<Byte>^ value, bool createRowKey ) {
		Set( key, value, CellFlag::Default, createRowKey );
	}

	void TableMutator::Set( Cell^ cell ) {
		if( cell == nullptr ) throw gcnew ArgumentNullException( L"cell" );
		Set( cell, false );
	}

	void TableMutator::Set( Cell^ cell, bool createRowKey ) {
		if( cell == nullptr ) throw gcnew ArgumentNullException( L"cell" );
		Set( cell->Key, cell->Value, cell->Flag, createRowKey );
	}

	void TableMutator::Set( IEnumerable<Cell^>^ cells ) {
		Set( cells, false );
	}

	void TableMutator::Set( IEnumerable<Cell^>^ cells, bool createRowKey ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( cells == nullptr ) throw gcnew ArgumentNullException( L"cells" );
		Common::Cells* _cells = 0;
		HT4N_TRY {
			ICollection<Cell^>^ cells_collection = dynamic_cast<ICollection<Cell^>^>( cells );
			_cells = Common::Cells::create( cells_collection != nullptr ? cells_collection->Count : 1024 );
			for each( Cell^ cell in cells ) {
				if( cell != nullptr && cell->Key != nullptr ) {
					Key^ key = cell->Key;
					if( createRowKey || String::IsNullOrEmpty(key->Row) ) {
						key->Row = gcnew String( Common::KeyBuilder().c_str() );
					}
					UInt32 len = cell->Value != nullptr ? cell->Value->Length : 0;
					pin_ptr<Byte> pv = len ? &cell->Value[0] : nullptr;
					_cells->add( CM2A(key->Row), CM2A(key->ColumnFamily), CM2A(key->ColumnQualifier), key->Timestamp, pv, len, (Byte)cell->Flag );
				}
			}
			msclr::lock sync( syncRoot );
			tableMutator->set( *_cells );
		}
		HT4N_RETHROW
		finally {
			if( _cells ) delete _cells;
		}
	}

	void TableMutator::Delete( String^ row ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( String::IsNullOrEmpty(row) ) throw gcnew ArgumentException( L"Invalid parameter row (null or empty)", L"row" );
		HT4N_TRY {
			msclr::lock sync( syncRoot );
			tableMutator->del( CM2A(row), 0, 0, 0 );
		} 
		HT4N_RETHROW
	}

	void TableMutator::Delete( Key^ key ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( key == nullptr ) throw gcnew ArgumentNullException( L"key" );
		HT4N_TRY {
			msclr::lock sync( syncRoot );
			tableMutator->del( CM2A(key->Row), CM2A(key->ColumnFamily), CM2A(key->ColumnQualifier), key->Timestamp );
		} 
		HT4N_RETHROW
	}

	void TableMutator:: Delete( IEnumerable<Key^>^ keys ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( keys == nullptr ) throw gcnew ArgumentNullException( L"keys" );
		HT4N_TRY {
			msclr::lock sync( syncRoot );
			for each( Key^ key in keys ) {
				if( key != nullptr ) {
					tableMutator->del( CM2A(key->Row), CM2A(key->ColumnFamily), CM2A(key->ColumnQualifier), key->Timestamp );
				}
			}
		} 
		HT4N_RETHROW
	}

	void TableMutator::Delete( IEnumerable<Cell^>^ cells ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( cells == nullptr ) throw gcnew ArgumentNullException( L"cells" );
		HT4N_TRY {
			msclr::lock sync( syncRoot );
			for each( Cell^ cell in cells ) {
				if( cell != nullptr ) {
					Key^ key = cell->Key;
					if( key != nullptr ) {
						tableMutator->del( CM2A(key->Row), CM2A(key->ColumnFamily), CM2A(key->ColumnQualifier), key->Timestamp );
					}
				}
			}
		} 
		HT4N_RETHROW
	}

	void TableMutator::Flush( ) {
		HT4N_THROW_OBJECTDISPOSED( );

		HT4N_TRY {
			msclr::lock sync( syncRoot );
			tableMutator->flush();
		} 
		HT4N_RETHROW
	}

	TableMutator::TableMutator( Common::TableMutator* _tableMutator )
	: tableMutator( _tableMutator )
	, syncRoot( gcnew Object() )
	, disposed( false )
	{
		if( tableMutator == 0 ) throw gcnew ArgumentNullException(L"tableMutator");
	}


	void TableMutator::Set( Key^ key, cli::array<Byte>^ value, CellFlag cellFlag, bool createRowKey ) {
		if( key == nullptr ) throw gcnew ArgumentNullException( L"key" );
		HT4N_TRY {
			UInt32 len = value != nullptr ? value->Length : 0;
			if( createRowKey || String::IsNullOrEmpty(key->Row) ) {
				std::string row;
				{
					msclr::lock sync( syncRoot );
					pin_ptr<Byte> pv = len ? &value[0] : nullptr;
					tableMutator->set( CM2A(key->ColumnFamily), CM2A(key->ColumnQualifier), key->Timestamp, pv, len, row );
				}
				key->Row = gcnew String( row.c_str() );
			}
			else {
				msclr::lock sync( syncRoot );
				pin_ptr<Byte> pv = len ? &value[0] : nullptr;
				tableMutator->set( CM2A(key->Row), CM2A(key->ColumnFamily), CM2A(key->ColumnQualifier), key->Timestamp, pv, len, (uint8_t)cellFlag );
			}
		}
		HT4N_RETHROW
	}

}