/** -*- C++ -*-
 * Copyright (C) 2010-2012 Thalmann Software & Consulting, http://www.softdev.ch
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

#include "Key.h"
#include "CM2A.h"

#include "ht4c.Common/Cell.h"
#include "ht4c.Common/KeyBuilder.h"
#include "ht4c.Common/Types.h"

namespace Hypertable {
	using namespace System;
	using namespace System::Text;
	using namespace System::Globalization;
	using namespace ht4c;

	Key::Key( ) {
	}

	Key::Key( String^ row ) {
		Row = row;
	}

	Key::Key( String^ row, String^ columnFamily ) {
		Row = row;
		ColumnFamily = columnFamily;
	}

	Key::Key( String^ row, String^ columnFamily, String^ columnQualifier ) {
		Row = row;
		ColumnFamily = columnFamily;
		ColumnQualifier = columnQualifier;
	}

	Key::Key( String^ row, String^ columnFamily, String^ columnQualifier, System::DateTime dateTime ) {
		Row = row;
		ColumnFamily = columnFamily;
		ColumnQualifier = columnQualifier;
		DateTime = dateTime;
	}

	Key::Key( Key^ key ) {
		if( key == nullptr ) throw gcnew ArgumentNullException(L"key");
		Row = key->Row;
		ColumnFamily = key->ColumnFamily;
		ColumnQualifier = key->ColumnQualifier;
		Timestamp = key->Timestamp;
	}

	System::DateTime Key::DateTime::get() {
		return timestampOrigin + TimeSpan::FromTicks(Timestamp / 100);
	}

	void Key::DateTime::set( System::DateTime value ) {
		if( value < timestampOrigin ) throw gcnew ArgumentException( L"Invalid DateTime" );
		if( value.Kind == DateTimeKind::Unspecified ) throw gcnew ArgumentException( L"Unspecified DateTime Kind" );
		Timestamp = (value.ToUniversalTime() - timestampOrigin).Ticks * 100;
	}

	Key::Key( const Common::Cell& cell ) {
		From( cell );
	}

	void Key::From( const Common::Cell& cell ) {
		Row = gcnew String( cell.row() );
		ColumnFamily = gcnew String( cell.columnFamily() );
		ColumnQualifier = gcnew String( cell.columnQualifier() );
		Timestamp = cell.timestamp();
	}

	int Key::CompareTo( Key^ other ) {
		if( Object::ReferenceEquals(other, nullptr) ) return 1;
		if( Object::ReferenceEquals(other, this) ) return 0;

		int result = String::Compare( Row, other->Row, StringComparison::Ordinal );
		if( result != 0 ) return result;
		result = String::Compare( ColumnFamily, other->ColumnFamily, StringComparison::Ordinal );
		if( result != 0 ) return result;
		result = String::Compare( ColumnQualifier, other->ColumnQualifier, StringComparison::Ordinal );
		if( result != 0 ) return result;
		return Timestamp.CompareTo( other->Timestamp );
	}

	bool Key::Equals( Key^ other ) {
		if( Object::ReferenceEquals(other, this) ) return true;
		if( Object::ReferenceEquals(other, nullptr) ) return false;

		return String::Equals(Row, other->Row)
				&& String::Equals(ColumnFamily, other->ColumnFamily)
				&& String::Equals(ColumnQualifier, other->ColumnQualifier)
				&& Timestamp == other->Timestamp;
	}

	bool Key::Equals( Object^ obj ) {
		return Equals( dynamic_cast<Key^>(obj) );
	}

	int Key::GetHashCode() {
		int result = 0;

		if( Row != nullptr ) result ^= Row->GetHashCode();
		if( ColumnFamily != nullptr ) result ^= ColumnFamily->GetHashCode();
		if( ColumnQualifier != nullptr ) result ^= ColumnQualifier->GetHashCode();
		return result ^ Timestamp.GetHashCode();
	}

	String^ Key::ToString() {
		if( Row == nullptr && ColumnFamily == nullptr && ColumnQualifier == nullptr && DateTime == timestampOrigin ) {
			return String::Format( CultureInfo::InvariantCulture, L"{0}(Empty)", GetType() );
		}

		#define APPEND_STRING( what ) if( what != nullptr ) sb->Append( String::Format(CultureInfo::InvariantCulture, L#what##L"={0}, ", what) );
		#define APPEND_DATETIME( what ) if( what != timestampOrigin ) sb->Append( String::Format(CultureInfo::InvariantCulture, L#what##L"={0}, ", what) );

		StringBuilder^ sb = gcnew StringBuilder();
		sb->Append( GetType() );
		sb->Append( L"(" );

		APPEND_STRING( Row )
		APPEND_STRING( ColumnFamily )
		APPEND_STRING( ColumnQualifier )
		APPEND_DATETIME( DateTime )

		sb->Remove( sb->Length - 2, 2 );
		sb->Append( L")" );
		return sb->ToString();

		#undef APPEND_DATETIME
		#undef APPEND_STRING
	}

	Object^ Key::Clone() {
		return gcnew Key( this );
	}

	int Key::Compare( Key^ keyA, Key^ keyB ) {
		if( Object::ReferenceEquals(keyA, keyB) ) return 0;
		if( Object::ReferenceEquals(keyB, nullptr) ) return 1;
		if( Object::ReferenceEquals(keyA, nullptr) ) return -1;
		return keyA->CompareTo( keyB );
	}

	bool Key::operator == ( Key^ keyA, Key^ keyB ) {
		return Compare( keyA, keyB ) == 0;
	}

	bool Key::operator != ( Key^ keyA, Key^ keyB ) {
		return Compare( keyA, keyB ) != 0;
	}

	bool Key::operator < ( Key^ keyA, Key^ keyB ) {
		return Compare( keyA, keyB ) < 0;
	}

	bool Key::operator > ( Key^ keyA, Key^ keyB ) {
		return Compare( keyA, keyB ) > 0;
	}

	String^ Key::Generate( ) {
		return gcnew String( Common::KeyBuilder().c_str() );
	}

	String^ Key::Encode( System::Guid value ) {
		cli::array<Byte>^ key = value.ToByteArray();
		pin_ptr<Byte> pkey = &key[0];
		return gcnew String( Common::KeyBuilder(pkey).c_str() );
	}

	Guid Key::Decode( String^ value ) {
		if( value == nullptr ) throw gcnew ArgumentNullException( L"value" );
		if( value->Length != Common::KeyBuilder::sizeKey ) throw gcnew ArgumentException( L"Invalid value length", L"value" );

		Common::uint8_t _guid[Common::KeyBuilder::sizeGuid];
		Common::KeyBuilder::decode( CM2A(value), _guid );
		cli::array<Byte>^ guid = gcnew cli::array<Byte>( Common::KeyBuilder::sizeGuid );
		pin_ptr<Byte> pguid = &guid[0];
		memcpy( pguid, _guid, sizeof(_guid) );
		return System::Guid( guid );
	}
}