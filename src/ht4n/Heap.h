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

namespace Hypertable {
	using namespace System;

	/// <summary>
	/// Represents a native heap.
	/// </summary>
	public ref class Heap abstract sealed {

		public:

			/// <summary>
			/// Allocates the number of bytes on the a native heap.
			/// </summary>
			/// <param name="size">The number of bytes to allocate.</param>
			/// <returns>The pointer to the block of memory.</returns>
			static IntPtr Alloc( int64_t size );

			/// <summary>
			/// Reallocate the memory block specified.
			/// </summary>
			/// <param name="p">The pointer to the block of memory.</param>
			/// <param name="size">The new number of bytes.</param>
			/// <returns>The new pointer to the block of memory.</returns>
			static IntPtr ReAlloc( IntPtr p, int64_t size );

			/// <summary>
			/// Free the memory block specified.
			/// </summary>
			/// <param name="p">The pointer to the block of memory.</param>
			static void Free( IntPtr p );

	};

}