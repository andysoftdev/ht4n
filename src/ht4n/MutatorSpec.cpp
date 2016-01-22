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

#include "MutatorSpec.h"

namespace Hypertable {
	using namespace System;
	using namespace System::Text;
	using namespace System::Globalization;

	MutatorSpec::MutatorSpec( ) {
		MaxChunkSize = MaxChunkSizeDefault;
		MaxCellCount = MaxCellCountDefault;
	}

	MutatorSpec::MutatorSpec( Hypertable::MutatorKind mutatorKind ) {
		MutatorKind = mutatorKind;
		MaxChunkSize = MaxChunkSizeDefault;
		MaxCellCount = MaxCellCountDefault;
	}

	MutatorSpec::MutatorSpec( MutatorSpec^ other ) {
		if( other == nullptr ) throw gcnew ArgumentNullException( L"other" );

		MutatorKind = other->MutatorKind;
		Timeout = other->Timeout;
		FlushInterval = other->FlushInterval;
		MaxChunkSize = other->MaxChunkSize;
		MaxCellCount = other->MaxCellCount;
		FlushEachChunk = other->FlushEachChunk;
		Queued = other->Queued;
		Capacity = other->Capacity;
		Flags = other->Flags;
	}

	String^ MutatorSpec::ToString() {

		#define APPEND_INT( what ) if( what > 0 ) sb->Append( String::Format(CultureInfo::InvariantCulture, L#what##L"={0}, ", what) );
		#define APPEND_BOOL( what ) if( what ) sb->Append( L#what##L", " );
		#define APPEND_TIMESPAN( what ) if( what.Ticks > 0 ) sb->Append( String::Format(CultureInfo::InvariantCulture, L#what##L"={0}, ", what) );

		StringBuilder^ sb = gcnew StringBuilder();
		sb->Append( GetType() );
		sb->Append( L"(" );

		APPEND_TIMESPAN( Timeout )
		APPEND_TIMESPAN( FlushInterval )
		APPEND_INT( MaxChunkSize )
		APPEND_INT( MaxCellCount )
		APPEND_BOOL( FlushEachChunk )
		APPEND_BOOL( Queued )
		APPEND_INT( Capacity )
		sb->Append( String::Format(CultureInfo::InvariantCulture, L"Flags={0}", Flags) );
		sb->Append( L")" );

		return sb->ToString();

		#undef APPEND_TIMESPAN
		#undef APPEND_BOOL
		#undef APPEND_INT
	}

	MutatorSpec^ MutatorSpec::Create() {
		return gcnew MutatorSpec(Hypertable::MutatorKind::Default);
	}

	MutatorSpec^ MutatorSpec::CreateQueued() {
		MutatorSpec^ mutatorSpec = gcnew MutatorSpec(Hypertable::MutatorKind::Default);
		mutatorSpec->Queued = true;
		return mutatorSpec;
	}

	MutatorSpec^ MutatorSpec::CreateChunked() {
		return gcnew MutatorSpec(Hypertable::MutatorKind::Chunked);
	}

	MutatorSpec^ MutatorSpec::CreateChunkedQueued() {
		MutatorSpec^ mutatorSpec = gcnew MutatorSpec(Hypertable::MutatorKind::Chunked);
		mutatorSpec->Queued = true;
		return mutatorSpec;
	}

}