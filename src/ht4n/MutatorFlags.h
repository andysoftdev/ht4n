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

#include "ht4c.Common/MutatorFlags.h"

#pragma warning( push )
#pragma warning( disable : 4638 ) // reference to unknown symbol 'MutatorSpec'

namespace Hypertable {
	using namespace System;

	/// <summary>
	/// Specifies possible table mutator flag values.
	/// </summary>
	/// <seealso cref="MutatorSpec" />
	[Serializable, Flags]
	public enum class MutatorFlags {

		/// <summary>
		/// Default behaviour.
		/// </summary>
		Default = ht4c::Common::MF_Default,

		/// <summary>
		/// Indicates whether to force a commit log sync on update or not.
		/// </summary>
		NoLogSync = ht4c::Common::MF_NoLogSync,

		/// <summary>
		/// Indicates whether to ignore unknown column families or not.
		/// </summary>
		IgnoreUnknownCf = ht4c::Common::MF_IgnoreUnknownCf
	};

}

#pragma warning( pop )