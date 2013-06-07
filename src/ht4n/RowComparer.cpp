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

#include "RowComparer.h"
#include "Key.h"

namespace Hypertable {
	using namespace System;

	RowComparer::RowComparer( )
	{
	}

	bool RowComparer::Equals( Key^ x, Key^ y ) {
		if( Object::ReferenceEquals(x, y) ) {
			return true;
		}
		else if( Object::ReferenceEquals(x, nullptr) || Object::ReferenceEquals(y, nullptr) ) {
			return false;
		}
		return String::Equals(x->Row, y->Row);
	}

	int RowComparer::GetHashCode( Key^ obj ) {
		if( obj == nullptr ) throw gcnew ArgumentNullException( L"obj" );

		return obj->Row != nullptr ? obj->Row->GetHashCode() : 17;
	}

	bool RowComparer::Equals( Object^ x, Object^ y ) {
		return Equals( dynamic_cast<Key^>(x), dynamic_cast<Key^>(y) );
	}

	int RowComparer::GetHashCode( Object^ obj ) {
		return GetHashCode( dynamic_cast<Key^>(obj) );
	}
}