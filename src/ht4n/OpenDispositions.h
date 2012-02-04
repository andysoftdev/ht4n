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

#pragma warning( push )
#pragma warning( disable : 4638 ) // reference to unknown symbol 'Client', 'Namespace'.

namespace Hypertable {
	using namespace System;

	/// <summary>
	/// Specifies possible open disposition values.
	/// </summary>
	/// <remarks>
	/// Open dispositions are used if opening namespaces or tables.
	/// </remarks>
	/// <seealso cref="IClient" />
	/// <seealso cref="INamespace" />
	[Serializable, Flags]
	public enum class OpenDispositions {

		/// <summary>
		/// Open only if exist, fails if not exist.
		/// </summary>
		OpenExisting = 0x00,

		/// <summary>
		/// Open always, create if not exist.
		/// </summary>
		OpenAlways = 0x01,

		/// <summary>
		/// Create always, drop and re-create if exist (drops complete namspace sub tree if opening namespaces).
		/// </summary>
		CreateAlways = 0x02,

		/// <summary>
		/// Create intermediates if not exists, only relevant if opening namespaces.
		/// </summary>
		CreateIntermediate = 0x10,

		/// <summary>
		/// Bypass any table cache, only relevant if opening tables.
		/// </summary>
		Force = 0x20
	};

}

#pragma warning( pop )