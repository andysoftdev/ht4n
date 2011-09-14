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

#include "RowInterval.h"

namespace Hypertable {
	using namespace System;

	RowInterval::RowInterval( ) {
	}

	RowInterval::RowInterval( String^ startRow, String^ endRow ) {
		if( String::IsNullOrEmpty(startRow) ) throw gcnew ArgumentNullException( L"startRow" );
		if( String::IsNullOrEmpty(endRow) ) throw gcnew ArgumentNullException( L"endRow" );
		
		StartRow = startRow;
		IncludeStartRow = true;
		EndRow = endRow;
		IncludeEndRow = true;
	}

	RowInterval::RowInterval( String^ startRow, bool includeStartRow, String^ endRow, bool includeEndRow ) {
		if( String::IsNullOrEmpty(startRow) ) throw gcnew ArgumentNullException( L"startRow" );
		if( String::IsNullOrEmpty(endRow) ) throw gcnew ArgumentNullException( L"endRow" );
		
		StartRow = startRow;
		IncludeStartRow = includeStartRow;
		EndRow = endRow;
		IncludeEndRow = includeEndRow;
	}

	RowInterval::RowInterval( RowInterval^ rowInterval ) {
		if( rowInterval == nullptr ) throw gcnew ArgumentNullException(L"rowInterval");
		
		StartRow = rowInterval->StartRow;
		IncludeStartRow = rowInterval->IncludeStartRow;
		EndRow = rowInterval->EndRow;
		IncludeEndRow = rowInterval->IncludeEndRow;
	}

	int RowInterval::CompareTo( RowInterval^ other ) {
		if( Object::ReferenceEquals(other, nullptr) ) return 1;

		int result = String::Compare( StartRow, other->StartRow, StringComparison::Ordinal );
		if( result != 0 ) return result;
		result = IncludeStartRow.CompareTo( other->IncludeStartRow );
		if( result != 0 ) return result;
		result = String::Compare( EndRow, other->EndRow, StringComparison::Ordinal );
		if( result != 0 ) return result;
		return IncludeEndRow.CompareTo( other->IncludeEndRow );
	}

	bool RowInterval::Equals( Object^ obj ) {
		if( obj == nullptr || obj->GetType() != RowInterval::typeid ) return false;
		return CompareTo( dynamic_cast<RowInterval^>(obj) ) == 0;
	}  

	int RowInterval::GetHashCode() {
		int result = 0;

		if( StartRow != nullptr ) result ^= StartRow->GetHashCode();
		result ^= IncludeStartRow.GetHashCode();
		if( EndRow != nullptr ) result ^= EndRow->GetHashCode();
		return result ^ IncludeEndRow.GetHashCode();
	}  
			
	int RowInterval::Compare( RowInterval^ x, RowInterval^ y ) {
		if( Object::ReferenceEquals(x, y) ) return 0;
		if( Object::ReferenceEquals(y, nullptr) ) return 1;
		if( Object::ReferenceEquals(x, nullptr) ) return -1;
		return x->CompareTo( y );
	}

	bool RowInterval::operator == ( RowInterval^ x, RowInterval^ y ) {
		return Compare( x, y ) == 0;
	}  
		
	bool RowInterval::operator != ( RowInterval^ x, RowInterval^ y ) {
		return Compare( x, y ) != 0;
	} 

	bool RowInterval::operator < ( RowInterval^ x, RowInterval^ y ) {
		return Compare( x, y ) < 0;
	} 

	bool RowInterval::operator > ( RowInterval^ x, RowInterval^ y ) {
		return Compare( x, y ) > 0;
	}  

	Object^ RowInterval::Clone() {
		return gcnew RowInterval( this );
	}

}