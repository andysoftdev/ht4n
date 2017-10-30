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

#pragma once

#ifndef __cplusplus_cli
#error "requires /clr"
#endif

#include "ht4c.Common/ContextKind.h"

namespace Hypertable {
	using namespace System;

	/// <summary>
	/// Declares the context kind.
	/// </summary>
	[Serializable]
	public enum class ContextKind {

		/// <summary>
		/// Unknown provider Kind.
		/// </summary>
		Unknown = ht4c::Common::CK_Unknown,

		/// <summary>
		/// Hypertable native protocol context kind.
		/// </summary>
		Hyper = ht4c::Common::CK_Hyper,

		/// <summary>
		/// Hypertable thrift API context kind.
		/// </summary>
		Thrift = ht4c::Common::CK_Thrift,

		/// <summary>
		/// Hypertable SQLite API context kind.
		/// </summary>
		SQLite = ht4c::Common::CK_SQLite,

		/// <summary>
		/// Hypertable hamster API context kind.
		/// </summary>
		Hamster = ht4c::Common::CK_Hamster,

		/// <summary>
		/// Hypertable ODBC API context kind.
		/// </summary>
		ODBC = ht4c::Common::CK_ODBC,
	};

}
