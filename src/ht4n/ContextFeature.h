/** -*- C++ -*-
 * Copyright (C) 2010-2012 Thalmann Software & Consulting, http://www.softdev.ch
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

#include "ht4c.Common/ContextFeature.h"

namespace Hypertable {
	using namespace System;

	/// <summary>
	/// Declares extended context features, apart from the regular features.
	/// </summary>
	[Serializable]
	public enum class ContextFeature {

		/// <summary>
		/// Unknown provider feature.
		/// </summary>
		Unknown = ht4c::Common::CF_Unknown,

		/// <summary>
		/// Hypertable query language (HQL).
		/// </summary>
		HQL = ht4c::Common::CF_HQL,

		/// <summary>
		/// Asynchronous table mutator.
		/// </summary>
		AsyncTableMutator = ht4c::Common::CF_AsyncTableMutator,

		/// <summary>
		/// Periodic flush mutator.
		/// </summary>
		PeriodicFlushTableMutator = ht4c::Common::CF_PeriodicFlushTableMutator,

		/// <summary>
		/// Asynchronous table scanner.
		/// </summary>
		AsyncTableScanner = ht4c::Common::CF_AsyncTableScanner
	};

}
