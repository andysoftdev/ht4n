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

#include "CellKeyComparer.h"
#include "Key.h"

#define EMPTY_IF_NULL( s ) \
	(s != nullptr ? s : String::Empty)

namespace Hypertable {
	using namespace System;

	CellKeyComparer::CellKeyComparer( ) 
	: includeTimestamp( false )
	{
	}

	CellKeyComparer::CellKeyComparer( bool _includeTimestamp ) 
	: includeTimestamp( _includeTimestamp )
	{
	}

	bool CellKeyComparer::Equals( Key^ x, Key^ y ) {
		if( Object::ReferenceEquals(x, y) ) {
			return true;
		}
		else if( Object::ReferenceEquals(x, nullptr) || Object::ReferenceEquals(y, nullptr) ) {
			return false;
		}
		return String::Compare(x->Row, y->Row, StringComparison::Ordinal) == 0
				&& String::Compare(x->ColumnFamily, y->ColumnFamily, StringComparison::Ordinal) == 0
				&& String::Compare(EMPTY_IF_NULL(x->ColumnQualifier), EMPTY_IF_NULL(y->ColumnQualifier), StringComparison::Ordinal) == 0
				&& (!includeTimestamp || x->Timestamp == y->Timestamp);
	}

	int CellKeyComparer::GetHashCode( Key^ obj ) {
		if( obj == nullptr ) throw gcnew ArgumentNullException( L"obj" );

		int result = 0;

		if( obj->Row != nullptr ) result ^= obj->Row->GetHashCode();
		if( obj->ColumnFamily != nullptr ) result ^= obj->ColumnFamily->GetHashCode();
		result ^= EMPTY_IF_NULL(obj->ColumnQualifier)->GetHashCode();

		if( includeTimestamp ) {
			result ^= obj->Timestamp.GetHashCode();
		}

		return result;
	}

	bool CellKeyComparer::Equals( Object^ x, Object^ y ) {
		return Equals(dynamic_cast<Key^>(x), dynamic_cast<Key^>(y));
	}

	int CellKeyComparer::GetHashCode( Object^ obj ) {
		return GetHashCode(dynamic_cast<Key^>(obj));
	}
}