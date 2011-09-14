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

#include "Cell.h"
#include "Key.h"

#include "ht4c.Common/Cell.h"

namespace Hypertable {
	using namespace System;
	using namespace System::Runtime::InteropServices;
	using namespace ht4c;

	Cell::Cell( ) {
		Flag = CellFlag::Default;
	}

	Cell::Cell( Hypertable::Key^ key, cli::array<Byte>^ value ) {
		if( key == nullptr ) throw gcnew ArgumentNullException( L"key" );
		Key = key;
		Value = value;
		Flag = CellFlag::Default;
	}

	Cell::Cell( Hypertable::Key^ key, cli::array<Byte>^ value, bool cloneKey ) {
		if( key == nullptr ) throw gcnew ArgumentNullException( L"key" );
		Key = cloneKey ? dynamic_cast<Hypertable::Key^>(key->Clone()) : key;
		Value = value;
		Flag = CellFlag::Default;
	}

	Cell::Cell( Hypertable::Key^ key, CellFlag flag ) {
		if( key == nullptr ) throw gcnew ArgumentNullException( L"key" );
		Key = key;
		Flag = flag;
	}

	Cell::Cell( Hypertable::Key^ key, CellFlag flag, bool cloneKey ) {
		if( key == nullptr ) throw gcnew ArgumentNullException( L"key" );
		Key = cloneKey ? dynamic_cast<Hypertable::Key^>(key->Clone()) : key;
		Flag = flag;
	}

	Cell::Cell( Hypertable::Key^ key, cli::array<Byte>^ value, CellFlag flag ) {
		if( key == nullptr ) throw gcnew ArgumentNullException( L"key" );
		Key = key;
		Value = value;
		Flag = flag;
	}

	Cell::Cell( Hypertable::Key^ key, cli::array<Byte>^ value, CellFlag flag, bool cloneKey ) {
		if( key == nullptr ) throw gcnew ArgumentNullException( L"key" );
		Key = cloneKey ? dynamic_cast<Hypertable::Key^>(key->Clone()) : key;
		Value = value;
		Flag = flag;
	}

	Cell::Cell( const Common::Cell* cell ) {
		if( cell ) {
			From( *cell );
		}
	}

	void Cell::From( const Common::Cell& cell ) {
		Key = gcnew Hypertable::Key( cell );
		
		size_t len;
		if( (len = cell.valueLength()) > 0 ) {
			if( Value == nullptr || Value->LongLength != len ) {
				Value = gcnew cli::array<Byte>( (int)len );
			}
			pin_ptr<Byte> pv = &Value[0];
			memcpy( pv, cell.value(), len );
		}
		else {
			Value = nullptr;
		}
		Flag = (CellFlag)cell.flag();
	}

	CellFlag Cell::DeleteFlagFromKey( Hypertable::Key^ key ) {
		if( key == nullptr ) throw gcnew ArgumentNullException( L"key" );

		return  String::IsNullOrEmpty(key->ColumnFamily)
					? CellFlag::DeleteRow
					: key->ColumnQualifier != nullptr
					? CellFlag::DeleteCell
					: CellFlag::DeleteColumnFamily;
	}

}