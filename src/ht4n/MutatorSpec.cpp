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

#include "MutatorSpec.h"

namespace Hypertable {
	using namespace System;

	MutatorSpec::MutatorSpec( ) {
		MaxChunkSize = MaxChunkSizeDefault;
		MaxCellCount = MaxCellCountDefault;
	}

	MutatorSpec::MutatorSpec( Hypertable::MutatorKind mutatorKind ) {
		MutatorKind = mutatorKind;
		MaxChunkSize = MaxChunkSizeDefault;
		MaxCellCount = MaxCellCountDefault;
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