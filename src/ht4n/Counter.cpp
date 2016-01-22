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

#include "Counter.h"
#include "Cell.h"
#include "Key.h"

#include "ht4c.Common/Utils.h"

namespace Hypertable {
	using namespace System;
	using namespace System::Globalization;
	using namespace System::Text;
	using namespace System::Runtime::InteropServices;
	using namespace ht4c;

	Counter::Counter( ) {
		Flag = CellFlag::Default;
	}

	Counter::Counter( Hypertable::Key^ key ) {
		if( key == nullptr ) throw gcnew ArgumentNullException( L"key" );
		Key = key;
		Flag = CellFlag::Default;
	}

	Counter::Counter( Hypertable::Key^ key, int64_t counter ) {
		if( key == nullptr ) throw gcnew ArgumentNullException( L"key" );
		Key = key;
		ResetCounter(counter);
		Flag = CellFlag::Default;
	}

	Counter::Counter( Hypertable::Key^ key, int64_t counter, bool cloneKey ) {
		if( key == nullptr ) throw gcnew ArgumentNullException( L"key" );
		Key = cloneKey ? dynamic_cast<Hypertable::Key^>(key->Clone()) : key;
		ResetCounter(counter);
		Flag = CellFlag::Default;
	}

	Counter::Counter( Cell^ cell ) {
		if( cell == nullptr ) throw gcnew ArgumentNullException( L"cell" );
		if( cell->Key == nullptr ) throw gcnew ArgumentException( L"Invalid key", L"cell" );
		if( cell->Key->Timestamp != 0 ) {
			Key = dynamic_cast<Hypertable::Key^>(cell->Key->Clone());
			Key->Timestamp = 0;
		}
		else {
			Key = cell->Key;
		}

		int64_t v;
		if( cell->Value != nullptr&& Int64::TryParse(Encoding::UTF8->GetString(cell->Value), v) ) {
			value = v;
		}
		Flag = cell->Flag;
	}

	Nullable<int64_t> Counter::Value::get() {
		return value;
	}

	void Counter::ResetCounter( int64_t n ) {
		value = n;
		Common::Int64Formatter fmt( n, L'=' );
		instruction = gcnew String( fmt.c_str(), 0, static_cast<int>(fmt.size()) );
	}

	void Counter::IncrementCounter( int64_t n ) {
		if( n < 0 ) {
			DecrementCounter( -n );
		}
		else {
			value = Nullable<int64_t>();
			Common::Int64Formatter fmt( n, L'+' );
			instruction = gcnew String( fmt.c_str(), 0, static_cast<int>(fmt.size()) );
		}
	}

	void Counter::DecrementCounter( int64_t n ) {
		if( n < 0 ) {
			IncrementCounter( -n );
		}
		else {
			value = Nullable<int64_t>();
			Common::Int64Formatter fmt( n, L'-' );
			instruction = gcnew String( fmt.c_str(), 0, static_cast<int>(fmt.size()) );
		}
	}

	cli::array<Byte>^ Counter::GetBytes() {
		return instruction != nullptr ? Encoding::UTF8->GetBytes(instruction) : nullptr;
	}

	Cell^ Counter::ToCell() {
		return gcnew Cell(
			  Key
			, GetBytes()
			, Flag );

	}

	String^ Counter::ToString() {
		return String::Format( CultureInfo::InvariantCulture
												 , L"{0}(Key={1}, counterValue={2}, Flag={3})"
												 , GetType()
												 , Key != nullptr ? Key->ToString() : L"null"
												 , instruction != nullptr ? instruction : L"null"
												 , Flag );
	}

}
