/** -*- C++ -*-
 * Copyright (C) 2010-2012 Thalmann Software & Consulting, http://www.softdev.ch
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
#pragma warning( disable : 4638 ) // reference to unknown symbol 'MutatorSpec', 'Table'.

namespace Hypertable {
	using namespace System;

	/// <summary>
	/// Specifies match kinds.
	/// </summary>
	/// <remarks>
	/// Match kind is used on column predicates.
	/// </remarks>
	/// <seealso cref="ColumnPredicate" />
	[Serializable]
	public enum class MatchKind {

		/// <summary>
		/// Undefinde
		/// </summary>
		Undefined,

		/// <summary>
		/// Exact match for the search.
		/// </summary>
		Exact,

		/// <summary>
		/// Prefix match for the search.
		/// </summary>
		Prefix,

		/// <summary>
		/// Contains.
		/// </summary>
		Contains
	};

}

#pragma warning( pop )