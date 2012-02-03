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

#pragma once

#ifndef __cplusplus_cli
#error "requires /clr"
#endif

#include "stdafx.h"

#include "ContextFactory.h"
#include "../Context.h"

namespace Hypertable { namespace Composition {
	using namespace System;
	using namespace System::Collections::Generic;

	Hypertable::IContext^ ContextFactory::Create( String^ connectionString ) {
		return Context::Create( connectionString );
	}

	Hypertable::IContext^ ContextFactory::Create( IDictionary<String^, Object^>^ properties ) {
		return Context::Create( properties );
	}

	Hypertable::IContext^ ContextFactory::Create( String^ connectionString, IDictionary<String^, Object^>^ properties ) {
		return Context::Create( connectionString, properties );
	}

} }