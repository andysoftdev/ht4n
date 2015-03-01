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

#include "RowInterval.h"

namespace Hypertable {
	using namespace System;

	/// <summary>
	/// Represents a cell interval.
	/// </summary>
	[Serializable]
	public ref class CellInterval sealed : public RowInterval, public IComparable<CellInterval^>, public IEquatable<RowInterval^> {

		public:

			/// <summary>
			/// Initializes a new instance of the CellInterval class.
			/// </summary>
			CellInterval( );

			/// <summary>
			/// Initializes a new instance of the CellInterval class using start/end row and start/end column family.
			/// </summary>
			/// <param name="startRow">Start row, might be null if unspecified.</param>
			/// <param name="startColumnFamily">Start column family.</param>
			/// <param name="endRow">End row, might be null if unspecified.</param>
			/// <param name="endColumnFamily">End column family.</param>
			CellInterval( String^ startRow, String^ startColumnFamily, String^ endRow, String^ endColumnFamily );

			/// <summary>
			/// Initializes a new instance of the CellInterval class using start/end cell.
			/// </summary>
			/// <param name="startRow">Start row, might be null if unspecified.</param>
			/// <param name="startColumnFamily">Start column family.</param>
			/// <param name="startColumnQualifier">Start column qualifier.</param>
			/// <param name="endRow">End row, might be null if unspecified.</param>
			/// <param name="endColumnFamily">End column family.</param>
			/// <param name="endColumnQualifier">End column qualifier.</param>
			CellInterval( String^ startRow, String^ startColumnFamily, String^ startColumnQualifier, String^ endRow, String^ endColumnFamily, String^ endColumnQualifier );

			/// <summary>
			/// Initializes a new instance of the CellInterval class using start/end cell.
			/// </summary>
			/// <param name="startRow">Start row, might be null if unspecified.</param>
			/// <param name="startColumnFamily">Start column family.</param>
			/// <param name="startColumnQualifier">Start column qualifier.</param>
			/// <param name="includeStartRow">Value that indicates whether the start row should be included.</param>
			/// <param name="endRow">End row, might be null if unspecified.</param>
			/// <param name="endColumnFamily">End column family.</param>
			/// <param name="endColumnQualifier">End column qualifier.</param>
			/// <param name="includeEndRow">Value that indicates whether the end row should be included.</param>
			CellInterval( String^ startRow, String^ startColumnFamily, String^ startColumnQualifier, bool includeStartRow, String^ endRow, String^ endColumnFamily, String^ endColumnQualifier, bool includeEndRow );

			/// <summary>
			/// Initializes a new instance of the CellInterval class that is a copy of the specified instance.
			/// </summary>
			/// <param name="cellInterval">Cell interval to copy.</param>
			explicit CellInterval( CellInterval^ cellInterval );

			/// <summary>
			/// Gets or sets the start column family.
			/// </summary>
			property String^ StartColumnFamily;

			/// <summary>
			/// Gets or sets the start column qualifier.
			/// </summary>
			property String^ StartColumnQualifier;

			/// <summary>
			/// Gets or sets the end column family.
			/// </summary>
			property String^ EndColumnFamily;

			/// <summary>
			/// Gets or sets the end column qualifier.
			/// </summary>
			property String^ EndColumnQualifier;

			/// <summary>
			/// Compares this instance with a specified CellInterval object and indicates whether this instance precedes, follows,
			/// or appears in the same position in the sort order as the specified CellInterval.
			/// </summary>
			/// <param name="other">Cell interval to compare, or null.</param>
			/// <returns>
			/// Signed integer that indicates the relationship between the comparand and this instance:
			/// <table class="comment">
			/// <tr><td>&lt; 0</td><td>if this instance precedes other.</td></tr>
			/// <tr><td>= 0</td><td>if this instance equals other.</td></tr>
			/// <tr><td>&gt; 0</td><td>if this instance follows other.</td></tr>
			/// </table>
			/// </returns>
			virtual int CompareTo( CellInterval^ other );

			/// <summary>
			/// Determines whether this instance and an other CellInterval object equals.
			/// </summary>
			/// <param name="other">Cell interval to compare, or null.</param>
			/// <returns>true if the value of obj is the same as the value of this instance, otherwise false.</returns>
			virtual bool Equals( CellInterval^ other );

			/// <summary>
			/// Determines whether this instance and a specified object, which must also be a CellInterval object, equals.
			/// </summary>
			/// <param name="obj">Cell interval to compare, or null.</param>
			/// <returns>true if the value of obj is the same as the value of this instance, otherwise false.</returns>
			virtual bool Equals( Object^ obj ) override;

			/// <summary>
			/// Returns the hash code for this CellInterval.
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
			/// <returns>A new CellInterval instance equal to this instance.</returns>
			virtual Object^ Clone( ) override;

			/// <summary>
			/// Compares two specified cell intervals and returns an integer that indicates their relative position in the sort order.
			/// </summary>
			/// <param name="x">The first cell interval to compare, or null.</param>
			/// <param name="y">The second cell interval to compare, or null.</param>
			/// <returns>
			/// Signed integer that indicates the relationship between the two comparands:
			/// <table class="comment">
			/// <tr><td>&lt; 0</td><td>if x is less than y</td></tr>
			/// <tr><td>= 0</td><td>if x equals y</td></tr>
			/// <tr><td>&gt; 0</td><td>if x is greater than y</td></tr>
			/// </table>
			/// </returns>
			static int Compare( CellInterval^ x, CellInterval^ y );

			/// <summary>
			/// Determines whether two specified cell intervals are equal.
			/// </summary>
			/// <param name="x">The first cell interval to compare, or null.</param>
			/// <param name="y">The second cell interval to compare, or null.</param>
			/// <returns>true if the value of x is the same as the value of y, otherwise false.</returns>
			static bool operator == ( CellInterval^ x, CellInterval^ y );

			/// <summary>
			/// Determines whether two specified cell intervals are different.
			/// </summary>
			/// <param name="x">The first cell interval to compare, or null.</param>
			/// <param name="y">The second cell interval to compare, or null.</param>
			/// <returns>true if the value of x is different as the value of y, otherwise false.</returns>
			static bool operator != ( CellInterval^ x, CellInterval^ y );

			/// <summary>
			/// Determines whether one specified cell interval is less than the other.
			/// </summary>
			/// <param name="x">The first cell interval to compare, or null.</param>
			/// <param name="y">The second cell interval to compare, or null.</param>
			/// <returns>true if the value of x is less than the value of y, otherwise false.</returns>
			static bool operator < ( CellInterval^ x, CellInterval^ y );

			/// <summary>
			/// Determines whether one specified cell interval is greater than the other.
			/// </summary>
			/// <param name="x">The first cell interval to compare, or null.</param>
			/// <param name="y">The second cell interval to compare, or null.</param>
			/// <returns>true if the value of x is greater than the value of y, otherwise false.</returns>
			static bool operator > ( CellInterval^ x, CellInterval^ y );
	};

}