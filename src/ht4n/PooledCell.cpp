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

#include "PooledCell.h"
#include "Cell.h"
#include "Key.h"

#include "ht4c.Common/Cell.h"

namespace Hypertable {
	using namespace System;
	using namespace System::Globalization;
	using namespace System::Runtime::InteropServices;
	using namespace ht4c;

	PooledCell::PooledCell( ) {
		Flag = CellFlag::Default;
	}

	String^ PooledCell::ToString() {
		return String::Format( CultureInfo::InvariantCulture
												 , L"{0}(Key={1}, Value.Length={2}, Flag={3})"
												 , GetType()
												 , Key != nullptr ? Key->ToString() : L"null"
												 , ValueLength
												 , Flag );
	}

	PooledCell::PooledCell( const Common::Cell* cell ) {
		if( cell ) {
			From( *cell );
		}
	}

	void PooledCell::From( const Common::Cell& cell ) {
		Key = gcnew Hypertable::Key( cell );

		if( (valueLength = static_cast<int>(cell.valueLength())) > 0 ) {
			value = valueLength <= smallPoolSize ? smallPool->Rent( valueLength ) : largePool->Rent( valueLength );
			pin_ptr<Byte> pv = &value[0];
			memcpy( pv, cell.value(), valueLength );
		}
		else {
			value = nullptr;
		}

		Flag = (CellFlag)cell.flag();
	}

}