/** -*- C++ -*-
 * Copyright (C) 2010-2015 Thalmann Software & Consulting, http://www.softdev.ch
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

#pragma warning( push )
#pragma warning( disable : 4638 ) // reference to unknown symbol 'Client', 'Namespace'.

namespace Hypertable {
	using namespace System;

	/// <summary>
	/// Specifies possible drop disposition values.
	/// </summary>
	/// <remarks>
	/// Drop dispositions are used if dropping namespaces or tables.
	/// </remarks>
	/// <seealso cref="IClient" />
	/// <seealso cref="INamespace" />
	[Serializable, Flags]
	public enum class DropDispositions {

		/// <summary>
		/// Default behaviour.
		/// </summary>
		None = 0,

		/// <summary>
		/// Drop only if exist, does not fail if not exist.
		/// </summary>
		IfExists = 1,

		/// <summary>
		/// Include tables, only relevant if dropping namespaces.
		/// </summary>
		IncludeTables = 1 << 1,

		/// <summary>
		/// Include all sub namespaces, only relevant if dropping namespaces.
		/// </summary>
		Deep = 1 << 2,

		/// <summary>
		/// Drop complete namespace tree including sub namespaces and all tables.
		/// </summary>
		Complete = IfExists|IncludeTables|Deep
	};

}

#pragma warning( pop )