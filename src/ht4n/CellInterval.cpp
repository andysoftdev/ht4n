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

#include "CellInterval.h"

namespace Hypertable {
	using namespace System;
	using namespace System::Globalization;

	CellInterval::CellInterval( ) {
	}

	CellInterval::CellInterval( String^ startRow, String^ startColumnFamily, String^ endRow, String^ endColumnFamily )
	: RowInterval( startRow, endRow )
	{
		if( String::IsNullOrEmpty(startColumnFamily) ) throw gcnew ArgumentNullException( L"startColumnFamily" );
		if( String::IsNullOrEmpty(endColumnFamily) ) throw gcnew ArgumentNullException( L"endColumnFamily" );
		
		StartColumnFamily = startColumnFamily;
		EndColumnFamily = endColumnFamily;
	}

	CellInterval::CellInterval( String^ startRow, String^ startColumnFamily, String^ startColumnQualifier, String^ endRow, String^ endColumnFamily, String^ endColumnQualifier )
	: RowInterval( startRow, endRow )
	{
		if( String::IsNullOrEmpty(startColumnFamily) ) throw gcnew ArgumentNullException( L"startColumnFamily" );
		if( String::IsNullOrEmpty(endColumnFamily) ) throw gcnew ArgumentNullException( L"endColumnFamily" );
		
		StartColumnFamily = startColumnFamily;
		StartColumnQualifier = startColumnQualifier;
		EndColumnFamily = endColumnFamily;
		EndColumnQualifier = endColumnQualifier;
	}

	CellInterval::CellInterval(  String^ startRow, String^ startColumnFamily, String^ startColumnQualifier, bool includeStartRow, String^ endRow, String^ endColumnFamily, String^ endColumnQualifier, bool includeEndRow )
	: RowInterval( startRow, includeStartRow, endRow, includeEndRow )
	{
		if( String::IsNullOrEmpty(startColumnFamily) ) throw gcnew ArgumentNullException( L"startColumnFamily" );
		if( String::IsNullOrEmpty(endColumnFamily) ) throw gcnew ArgumentNullException( L"endColumnFamily" );
		
		StartColumnFamily = startColumnFamily;
		StartColumnQualifier = startColumnQualifier;
		EndColumnFamily = endColumnFamily;
		EndColumnQualifier = endColumnQualifier;	
	}

	CellInterval::CellInterval( CellInterval^ cellInterval )
	: RowInterval( cellInterval )
	{
		if( cellInterval == nullptr ) throw gcnew ArgumentNullException(L"cellInterval");
		
		StartColumnFamily = cellInterval->StartColumnFamily;
		StartColumnQualifier = cellInterval->StartColumnQualifier;
		EndColumnFamily = cellInterval->EndColumnFamily;
		EndColumnQualifier = cellInterval->EndColumnQualifier;
	}

	int CellInterval::CompareTo( CellInterval^ other ) {
		if( Object::ReferenceEquals(other, nullptr) ) return 1;
		if( Object::ReferenceEquals(other, this) ) return 0;

		int result = RowInterval::CompareTo( other );
		if( result != 0 ) return result;
		result = String::Compare( StartColumnFamily, other->StartColumnFamily, StringComparison::Ordinal );
		if( result != 0 ) return result;
		result = String::Compare( StartColumnQualifier, other->StartColumnQualifier, StringComparison::Ordinal );
		if( result != 0 ) return result;
		result = String::Compare( EndColumnFamily, other->EndColumnFamily, StringComparison::Ordinal );
		if( result != 0 ) return result;
		return String::Compare( EndColumnQualifier, other->EndColumnQualifier, StringComparison::Ordinal );
	}

	bool CellInterval::Equals( CellInterval^ other ) {
		return CompareTo( other ) == 0;
	}

	bool CellInterval::Equals( Object^ obj ) {
		return Equals( dynamic_cast<CellInterval^>(obj) ) == 0;
	}

	int CellInterval::GetHashCode() {
		int result = 0;

		if( StartRow != nullptr ) result ^= StartRow->GetHashCode();
		result ^= IncludeStartRow.GetHashCode();
		if( EndRow != nullptr ) result ^= EndRow->GetHashCode();
		return result ^ IncludeEndRow.GetHashCode();
	}

	String^ CellInterval::ToString() {
		return String::Format( CultureInfo::InvariantCulture
												 , L"{0}(StartRow={1}, {2}StartColumnFamily={3}, StartColumnQualifier={4}, EndRow={5}, {6}EndColumnFamily={7}, EndColumnQualifier={8})"
												 , GetType()
												 , StartRow != nullptr ? StartRow : L"null"
												 , IncludeStartRow ? L"IncludeStartRow, " : String::Empty
												 , StartColumnFamily != nullptr ? StartColumnFamily : L"null"
												 , StartColumnQualifier != nullptr ? StartColumnQualifier : L"null"
												 , EndRow != nullptr ? EndRow : L"null"
												 , IncludeEndRow ? L"IncludeEndRow, " : String::Empty
												 , EndColumnFamily != nullptr ? EndColumnFamily : L"null"
												 , EndColumnQualifier != nullptr ? EndColumnQualifier : L"null" );
	}

	Object^ CellInterval::Clone() {
		return gcnew CellInterval( this );
	}

	int CellInterval::Compare( CellInterval^ x, CellInterval^ y ) {
		if( Object::ReferenceEquals(x, y) ) return 0;
		if( Object::ReferenceEquals(y, nullptr) ) return 1;
		if( Object::ReferenceEquals(x, nullptr) ) return -1;

		return x->CompareTo( y );
	}

	bool CellInterval::operator == ( CellInterval^ x, CellInterval^ y ) {
		return Compare( x, y ) == 0;
	}

	bool CellInterval::operator != ( CellInterval^ x, CellInterval^ y ) {
		return Compare( x, y ) != 0;
	} 

	bool CellInterval::operator < ( CellInterval^ x, CellInterval^ y ) {
		return Compare( x, y ) < 0;
	} 

	bool CellInterval::operator > ( CellInterval^ x, CellInterval^ y ) {
		return Compare( x, y ) > 0;
	}

}