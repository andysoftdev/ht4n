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
#pragma warning( disable : 4638 ) // reference to unknown symbol 'Client'

namespace Hypertable {
	using namespace System;

	/// <summary>
	/// Specifies possible create disposition values.
	/// </summary>
	/// <remarks>
	/// Create dispositions are used if creating namespaces or tables.
	/// </remarks>
	/// <seealso cref="IClient" />
	[Serializable, Flags]
	public enum class CreateDispositions {

		/// <summary>
		/// Default behaviour.
		/// </summary>
		None = 0,

		/// <summary>
		/// Create intermediates if not exists.
		/// </summary>
		CreateIntermediate = 1,

		/// <summary>
		/// Create if not exist, does not fail if exist.
		/// </summary>
		CreateIfNotExist = 2
	};

}

#pragma warning( pop )