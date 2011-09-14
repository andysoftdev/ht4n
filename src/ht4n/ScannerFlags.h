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

#include "ht4c.Common/ScannerFlags.h"

#pragma warning( push )
#pragma warning( disable : 4638 ) // reference to unknown symbol 'ScanSpec'

namespace Hypertable {
	using namespace System;

	/// <summary>
	/// Specifies possible table scanner flag values.
	/// </summary>
	/// <seealso cref="ScanSpec" />
	[Flags, Serializable]
	public enum class ScannerFlags {

		/// <summary>
		/// Default behaviour.
		/// </summary>
		Default = ht4c::Common::SF_Default,

		/// <summary>
		/// Bypass table cache.
		/// </summary>
		BypassTableCache = ht4c::Common::SF_BypassTableCache,

		/// <summary>
		/// Refresh table cache.
		/// </summary>
		RefreshTableCache = ht4c::Common::SF_RefreshTableCache,

		/// <summary>
		/// Do not refresh table cache automatically.
		/// </summary>
		NoAutoTableRefresh = ht4c::Common::SF_NoAutoTableRefresh
	};

}

#pragma warning( pop )