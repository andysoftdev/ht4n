/** -*- C++ -*-
 * Copyright (C) 2010-2015 Thalmann Software & Consulting, http://www.softdev.ch
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

#include "ColumnPredicate.h"

namespace Hypertable {
	using namespace System;
	using namespace System::Globalization;

	inline int ArrayCompare( cli::array<byte>^ x, cli::array<byte>^ y ) {
		if( Object::ReferenceEquals(x, y) ) return 0;
		if( Object::ReferenceEquals(y, nullptr) ) return 1;
		if( Object::ReferenceEquals(x, nullptr) ) return -1;

		int lengthDiff = x->Length - y->Length;
		if( lengthDiff != 0 ) {
			return Math::Sign( lengthDiff );
		}
		pin_ptr<byte> px = &x[0];
		pin_ptr<byte> py = &y[0];
		return memcmp( px, py, x->Length );
	}

	inline int ArrayHashCode( cli::array<byte>^ x ) {
		int hash = 0;
		if( x != nullptr ) {
			int len = x->Length;
			if( len ) {
				pin_ptr<byte> px = &x[0];
				uint8_t* p = px;
				while( --len ) {
					hash ^= 617 * (*p++);
				}
			}
			else {
				hash = 31;
			}
		}

		return hash;
	}

	ColumnPredicate::ColumnPredicate( ) {
	}

	ColumnPredicate::ColumnPredicate( String^ columnFamily, MatchKind match, cli::array<byte>^ searchValue ) {
		if( String::IsNullOrEmpty(columnFamily) ) throw gcnew ArgumentNullException(L"columnFamily");
		if( match == MatchKind::Undefined ) throw gcnew ArgumentException(L"Invalid match", L"match");

		ColumnFamily = columnFamily;
		Match = match;
		SearchValue = searchValue;
	}

	ColumnPredicate::ColumnPredicate( String^ columnFamily, String^ columnQualifier,  MatchKind match ) {
		if( String::IsNullOrEmpty(columnFamily) ) throw gcnew ArgumentNullException(L"columnFamily");
		if( match != MatchKind::QualifierExact && match != MatchKind::QualifierPrefix && match != MatchKind::QualifierRegex ) throw gcnew ArgumentException(L"Invalid match", L"match");

		ColumnFamily = columnFamily;
		ColumnQualifier = columnQualifier;
		Match = match;
	}

	ColumnPredicate::ColumnPredicate( String^ columnFamily, String^ columnQualifier,  MatchKind match, cli::array<byte>^ searchValue ) {
		if( String::IsNullOrEmpty(columnFamily) ) throw gcnew ArgumentNullException(L"columnFamily");
		if( match == MatchKind::Undefined ) throw gcnew ArgumentException(L"Invalid match", L"match");

		ColumnFamily = columnFamily;
		ColumnQualifier = columnQualifier;
		Match = match;
		SearchValue = searchValue;
	}

	ColumnPredicate::ColumnPredicate( ColumnPredicate^ columnPredicate ) {
		if( columnPredicate == nullptr ) throw gcnew ArgumentNullException(L"columnPredicate");

		ColumnFamily = columnPredicate->ColumnFamily;
		ColumnQualifier = columnPredicate->ColumnQualifier;
		Match = columnPredicate->Match;
		SearchValue = columnPredicate->SearchValue;
	}

	int ColumnPredicate::CompareTo( ColumnPredicate^ other ) {
		if( Object::ReferenceEquals(other, nullptr) ) return 1;
		if( Object::ReferenceEquals(other, this) ) return 0;

		int result = String::Compare( ColumnFamily, other->ColumnFamily, StringComparison::Ordinal );
		if( result != 0 ) return result;
		result = String::Compare( ColumnQualifier, other->ColumnQualifier, StringComparison::Ordinal );
		if( result != 0 ) return result;
		result = Match.CompareTo( other->Match );
		if( result != 0 ) return result;
		return ArrayCompare( SearchValue, other->SearchValue );
	}

	bool ColumnPredicate::Equals( ColumnPredicate^ other ) {
		return CompareTo( other ) == 0;
	}

	bool ColumnPredicate::Equals( Object^ obj ) {
		return Equals( dynamic_cast<ColumnPredicate^>(obj) );
	}

	int ColumnPredicate::GetHashCode() {
		int result = 17;

		if( ColumnFamily != nullptr ) result = ::Hash( result, ColumnFamily->GetHashCode());
		if( ColumnQualifier != nullptr ) result = ::Hash( result, ColumnQualifier->GetHashCode());
		result = ::Hash( result, Match.GetHashCode());
		if( SearchValue != nullptr ) result = ::Hash( result, ArrayHashCode(SearchValue) );
		return result;
	}

	String^ ColumnPredicate::ToString() {
		return String::Format( CultureInfo::InvariantCulture
												 , L"{0}(ColumnFamily={1}, ColumnQualifier={2}, Match={3})"
												 , GetType()
												 , ColumnFamily != nullptr ? ColumnFamily : L"null"
												 , ColumnQualifier != nullptr ? ColumnQualifier : L"null"
												 , Match );
	}

	Object^ ColumnPredicate::Clone() {
		return gcnew ColumnPredicate( this );
	}

	int ColumnPredicate::Compare( ColumnPredicate^ x, ColumnPredicate^ y ) {
		if( Object::ReferenceEquals(x, y) ) return 0;
		if( Object::ReferenceEquals(y, nullptr) ) return 1;
		if( Object::ReferenceEquals(x, nullptr) ) return -1;

		return x->CompareTo( y );
	}

	bool ColumnPredicate::operator == ( ColumnPredicate^ x, ColumnPredicate^ y ) {
		return Compare( x, y ) == 0;
	}

	bool ColumnPredicate::operator != ( ColumnPredicate^ x, ColumnPredicate^ y ) {
		return Compare( x, y ) != 0;
	}

	bool ColumnPredicate::operator < ( ColumnPredicate^ x, ColumnPredicate^ y ) {
		return Compare( x, y ) < 0;
	}

	bool ColumnPredicate::operator > ( ColumnPredicate^ x, ColumnPredicate^ y ) {
		return Compare( x, y ) > 0;
	}

}