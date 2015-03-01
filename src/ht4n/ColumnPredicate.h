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

#include "MatchKind.h"

namespace Hypertable {
	using namespace System;

	/// <summary>
	/// Represents a column predicate.
	/// </summary>
	[Serializable]
	public ref class ColumnPredicate : public IComparable<ColumnPredicate^>, public IEquatable<ColumnPredicate^>, public ICloneable {

		public:

			/// <summary>
			/// Initializes a new instance of the ColumnPredicate class.
			/// </summary>
			ColumnPredicate( );

			/// <summary>
			/// Initializes a new instance of the ColumnPredicate class using column family, match kind and search value.
			/// </summary>
			/// <param name="columnFamily">Column family.</param>
			/// <param name="match">Defines the match kind.</param>
			/// <param name="searchValue">Search value.</param>
			ColumnPredicate( String^ columnFamily, MatchKind match, cli::array<byte>^ searchValue );

			/// <summary>
			/// Initializes a new instance of the ColumnPredicate class using column family, qualifier, match kind.
			/// </summary>
			/// <param name="columnFamily">Column family.</param>
			/// <param name="columnFamily">Column qualifier.</param>
			/// <param name="match">Defines the match kind.</param>
			ColumnPredicate( String^ columnFamily, String^ columnQualifier, MatchKind match );

			/// <summary>
			/// Initializes a new instance of the ColumnPredicate class using column family, qualifier, match kind and search value.
			/// </summary>
			/// <param name="columnFamily">Column family.</param>
			/// <param name="columnFamily">Column qualifier.</param>
			/// <param name="match">Defines the match kind.</param>
			/// <param name="searchValue">Search value.</param>
			ColumnPredicate( String^ columnFamily, String^ columnQualifier, MatchKind match, cli::array<byte>^ searchValue );

			/// <summary>
			/// Initializes a new instance of the ColumnPredicate class that is a copy of the specified instance.
			/// </summary>
			/// <param name="columnPredicate">Column predicate to copy.</param>
			explicit ColumnPredicate( ColumnPredicate^ columnPredicate );

			/// <summary>
			/// Gets or sets the column family.
			/// </summary>
			property String^ ColumnFamily;

			/// <summary>
			/// Gets or sets the column qualifier.
			/// </summary>
			property String^ ColumnQualifier;

			/// <summary>
			/// Gets or sets the match kind.
			/// </summary>
			property MatchKind Match;

			/// <summary>
			/// Gets or sets the search value, might be null.
			/// </summary>
			property cli::array<Byte>^ SearchValue;

			/// <summary>
			/// Compares this instance with a specified ColumnPredicate object and indicates whether this instance precedes, follows,
			/// or appears in the same position in the sort order as the specified ColumnPredicate.
			/// </summary>
			/// <param name="other">Column predicate to compare, or null.</param>
			/// <returns>
			/// Signed integer that indicates the relationship between the comparand and this instance:
			/// <table class="comment">
			/// <tr><td>&lt; 0</td><td>if this instance precedes other.</td></tr>
			/// <tr><td>= 0</td><td>if this instance equals other.</td></tr>
			/// <tr><td>&gt;</td><td>0 if this instance follows other.</td></tr>
			/// </table>
			/// </returns>
			virtual int CompareTo( ColumnPredicate^ other );

			/// <summary>
			/// Determines whether this instance and an other ColumnPredicate object equals.
			/// </summary>
			/// <param name="other">ColumnPredicate to compare, or null.</param>
			/// <returns>true if the value of obj is the same as the value of this instance, otherwise false.</returns>
			virtual bool Equals( ColumnPredicate^ other );

			/// <summary>
			/// Determines whether this instance and a specified object, which must also be a ColumnPredicate object, equals.
			/// </summary>
			/// <param name="obj">Column predicate to compare, or null.</param>
			/// <returns>true if the value of obj is the same as the value of this instance, otherwise false.</returns>
			virtual bool Equals( Object^ obj ) override;

			/// <summary>
			/// Returns the hash code for this ColumnPredicate.
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
			/// <returns>A new ColumnPredicate instance equal to this instance.</returns>
			virtual Object^ Clone( );

			/// <summary>
			/// Compares two specified column predicates and returns an integer that indicates their relative position in the sort order.
			/// </summary>
			/// <param name="x">The first column predicate to compare, or null.</param>
			/// <param name="y">The second column predicate to compare, or null.</param>
			/// <returns>
			/// Signed integer that indicates the relationship between the two comparands:
			/// <table class="comment">
			/// <tr><td>&lt; 0</td><td>if x is less than y</td></tr>
			/// <tr><td>= 0</td><td>if x equals y</td></tr>
			/// <tr><td>&gt; 0</td><td>if x is greater than y</td></tr>
			/// </table>
			/// </returns>
			static int Compare( ColumnPredicate^ x, ColumnPredicate^ y );

			/// <summary>
			/// Determines whether two specified row intervals are equal.
			/// </summary>
			/// <param name="x">The first row interval to compare, or null.</param>
			/// <param name="y">The second row interval to compare, or null.</param>
			/// <returns>true if the value of x is the same as the value of y, otherwise false.</returns>
			static bool operator == ( ColumnPredicate^ x, ColumnPredicate^ y );

			/// <summary>
			/// Determines whether two specified row intervals are different.
			/// </summary>
			/// <param name="x">The first row interval to compare, or null.</param>
			/// <param name="y">The second row interval to compare, or null.</param>
			/// <returns>true if the value of x is different as the value of y, otherwise false.</returns>
			static bool operator != ( ColumnPredicate^ x, ColumnPredicate^ y );

			/// <summary>
			/// Determines whether one specified row interval is less than the other.
			/// </summary>
			/// <param name="x">The first row interval to compare, or null.</param>
			/// <param name="y">The second row interval to compare, or null.</param>
			/// <returns>true if the value of x is less than the value of y, otherwise false.</returns>
			static bool operator < ( ColumnPredicate^ x, ColumnPredicate^ y );

			/// <summary>
			/// Determines whether one specified row interval is greater than the other.
			/// </summary>
			/// <param name="x">The first row interval to compare, or null.</param>
			/// <param name="y">The second row interval to compare, or null.</param>
			/// <returns>true if the value of x is greater than the value of y, otherwise false.</returns>
			static bool operator > ( ColumnPredicate^ x, ColumnPredicate^ y );
	};

}