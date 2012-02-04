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

namespace Hypertable {
	using namespace System;

	/// <summary>
	/// Represents a row interval.
	/// </summary>
	[Serializable]
	public ref class RowInterval : public IComparable<RowInterval^>, public IEquatable<RowInterval^>, public ICloneable {

		public:

			/// <summary>
			/// Initializes a new instance of the RowInterval class.
			/// </summary>
			RowInterval( );

			/// <summary>
			/// Initializes a new instance of the RowInterval class using start/end row.
			/// </summary>
			/// <param name="startRow">Start row, might be null if unspecified.</param>
			/// <param name="endRow">End row, might be null if unspecified.</param>
			RowInterval( String^ startRow, String^ endRow );
			
			/// <summary>
			/// Initializes a new instance of the RowInterval class using start/end row.
			/// </summary>
			/// <param name="startRow">Start row, might be null if unspecified.</param>
			/// <param name="includeStartRow">Value that indicates whether the start row should be included.</param>
			/// <param name="endRow">End row, might be null if unspecified.</param>
			/// <param name="includeEndRow">Value that indicates whether the end row should be included.</param>
			RowInterval( String^ startRow, bool includeStartRow, String^ endRow, bool includeEndRow );

			/// <summary>
			/// Initializes a new instance of the RowInterval class that is a copy of the specified instance.
			/// </summary>
			/// <param name="rowInterval">Row interval to copy.</param>
			RowInterval( RowInterval^ rowInterval );

			/// <summary>
			/// Gets or sets the start row.
			/// </summary>
			property String^ StartRow;

			/// <summary>
			/// Gets or sets a value that indicates whether the start row should be included.
			/// </summary>
			property bool IncludeStartRow;

			/// <summary>
			/// Gets or sets the end row.
			/// </summary>
			property String^ EndRow;

			/// <summary>
			/// Gets or sets a value that indicates whether the end row should be included.
			/// </summary>
			property bool IncludeEndRow;

			/// <summary>
			/// Compares this instance with a specified RowInterval object and indicates whether this instance precedes, follows,
			/// or appears in the same position in the sort order as the specified RowInterval.
			/// </summary>
			/// <param name="other">Row interval to compare, or null.</param>
			/// <returns>
			/// Signed integer that indicates the relationship between the comparand and this instance:
			/// <table class="comment">
			/// <tr><td>&lt; 0</td><td>if this instance precedes other.</td></tr>
			/// <tr><td>= 0</td><td>if this instance equals other.</td></tr>
			/// <tr><td>&gt;</td><td>0 if this instance follows other.</td></tr>
			/// </table>
			/// </returns>
			virtual int CompareTo( RowInterval^ other );

			/// <summary>
			/// Determines whether this instance and an other RowInterval object equals.
			/// </summary>
			/// <param name="other">RowInterval to compare, or null.</param>
			/// <returns>true if the value of obj is the same as the value of this instance, otherwise false.</returns>
			virtual bool Equals( RowInterval^ other );

			/// <summary>
			/// Determines whether this instance and a specified object, which must also be a RowInterval object, equals.
			/// </summary>
			/// <param name="obj">Row interval to compare, or null.</param>
			/// <returns>true if the value of obj is the same as the value of this instance, otherwise false.</returns>
			virtual bool Equals( Object^ obj ) override;

			/// <summary>
			/// Returns the hash code for this RowInterval.
			/// </summary>
			/// <returns>Signed hash code.</returns>
			virtual int GetHashCode() override;

			/// <summary>
			/// Returns a string that represents the current object.
			/// </summary>
			/// <returns>A string that represents the current object.</returns>
			virtual String^ ToString() override;

			/// <summary>
			/// Creates a new object that is a copy of this instance.
			/// </summary>
			/// <returns>A new RowInterval instance equal to this instance.</returns>
			virtual Object^ Clone( );

			/// <summary>
			/// Compares two specified row intervals and returns an integer that indicates their relative position in the sort order.
			/// </summary>
			/// <param name="x">The first row interval to compare, or null.</param>
			/// <param name="y">The second row interval to compare, or null.</param>
			/// <returns>
			/// Signed integer that indicates the relationship between the two comparands:
			/// <table class="comment">
			/// <tr><td>&lt; 0</td><td>if x is less than y</td></tr>
			/// <tr><td>= 0</td><td>if x equals y</td></tr>
			/// <tr><td>&gt; 0</td><td>if x is greater than y</td></tr>
			/// </table>
			/// </returns>
			static int Compare( RowInterval^ x, RowInterval^ y );

			/// <summary>
			/// Determines whether two specified row intervals are equal.
			/// </summary>
			/// <param name="x">The first row interval to compare, or null.</param>
			/// <param name="y">The second row interval to compare, or null.</param>
			/// <returns>true if the value of x is the same as the value of y, otherwise false.</returns>
			static bool operator == ( RowInterval^ x, RowInterval^ y );

			/// <summary>
			/// Determines whether two specified row intervals are different.
			/// </summary>
			/// <param name="x">The first row interval to compare, or null.</param>
			/// <param name="y">The second row interval to compare, or null.</param>
			/// <returns>true if the value of x is different as the value of y, otherwise false.</returns>
			static bool operator != ( RowInterval^ x, RowInterval^ y );

			/// <summary>
			/// Determines whether one specified row interval is less than the other.
			/// </summary>
			/// <param name="x">The first row interval to compare, or null.</param>
			/// <param name="y">The second row interval to compare, or null.</param>
			/// <returns>true if the value of x is less than the value of y, otherwise false.</returns>
			static bool operator < ( RowInterval^ x, RowInterval^ y );

			/// <summary>
			/// Determines whether one specified row interval is greater than the other.
			/// </summary>
			/// <param name="x">The first row interval to compare, or null.</param>
			/// <param name="y">The second row interval to compare, or null.</param>
			/// <returns>true if the value of x is greater than the value of y, otherwise false.</returns>
			static bool operator > ( RowInterval^ x, RowInterval^ y );
	};

}