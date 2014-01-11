/** -*- C++ -*-
 * Copyright (C) 2010-2014 Thalmann Software & Consulting, http://www.softdev.ch
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
	using namespace System::Collections::Generic;

	ref class Key;

	/// <summary>
	/// Represents a row key comparer.
	/// </summary>
	[Serializable]
	public ref class RowComparer sealed : public IEqualityComparer<Key^>, public System::Collections::IEqualityComparer {

		public:

			/// <summary>
			/// Initializes a new instance of the RowComparer class.
			/// </summary>
			RowComparer( );

			#pragma region IEqualityComparer<Key^> methods

			/// <summary>
			/// Determines whether the specified objects are equal.
			/// </summary>
			/// <param name="x">The first key to compare, or null.</param>
			/// <param name="y">The second key to compare, or null.</param>
			/// <returns>true if the value of obj is the same as the value of this instance, otherwise false.</returns>
			bool virtual Equals( Key^ x, Key^ y );

			/// <summary>
			/// Returns a hash code for the specified object.
			/// </summary>
			/// <param name="obj">Key to get a hash code.</param>
			/// <returns>A hash code for the specified object.</returns>
			int virtual GetHashCode( Key^ obj );

			#pragma endregion

			#pragma region IEqualityComparer methods

			/// <summary>
			/// Determines whether the specified objects are equal.
			/// </summary>
			/// <param name="x">The first key to compare, or null.</param>
			/// <param name="y">The second key to compare, or null.</param>
			/// <returns>true if the value of obj is the same as the value of this instance, otherwise false.</returns>
			bool virtual Equals( Object^ x, Object^ y ) new;

			/// <summary>
			/// Returns a hash code for the specified object.
			/// </summary>
			/// <param name="obj">Key to get a hash code.</param>
			/// <returns>A hash code for the specified object.</returns>
			int virtual GetHashCode( Object^ obj );

			#pragma endregion

	};

}