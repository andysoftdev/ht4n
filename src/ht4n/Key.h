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

namespace ht4c { namespace Common {
	class Cell;
} }

namespace Hypertable {
	using namespace System;
	using namespace ht4c;

	/// <summary>
	/// Represents a Hypertable key, provide accessors to the key attributes.
	/// </summary>
	/// <remarks>
	/// The Hypertable data model consists of a multi-dimensional table of information that can be queried using a single primary key.
	/// The first dimension of the table is the row key. The row key is the primary key and defines the order in which the table data
	/// is physically stored. The second dimension is the column family. This dimension is somewhat analogous to a traditional database
	/// column. The third dimension is the column qualifier. Within each column family, there can be a theoretically infinite number of
	/// qualified instances. The fourth and final dimension is the time dimension. The key is essentially the concatenation of the four
	/// dimension keys (row, column family, column qualifier and timestamp).
	/// See also <a href="http://code.google.com/p/hypertable/wiki/ArchitecturalOverview" target="_blank">architectural overview</a>.
	/// </remarks>
	[Serializable]
	public ref class Key sealed : public IComparable<Key^>, public IEquatable<Key^>, public ICloneable {

		public:

			/// <summary>
			/// Initializes a new instance of the Key class.
			/// </summary>
			Key( );

			/// <summary>
			/// Initializes a new instance of the Key class using row key.
			/// </summary>
			/// <param name="row">Row key.</param>
			Key( String^ row );

			/// <summary>
			/// Initializes a new instance of the Key class using row key and column family.
			/// </summary>
			/// <param name="row">Row key.</param>
			/// <param name="columnFamily">Column family.</param>
			Key( String^ row, String^ columnFamily );

			/// <summary>
			/// Initializes a new instance of the Key class using row key, column family and column qualifier.
			/// </summary>
			/// <param name="row">Row key.</param>
			/// <param name="columnFamily">Column family.</param>
			/// <param name="columnQualifier">column qualifier.</param>
			Key( String^ row, String^ columnFamily, String^ columnQualifier );

			/// <summary>
			/// Initializes a new instance of the Key class using row key, column family, column qualifier and timestamp.
			/// </summary>
			/// <param name="row">Row key.</param>
			/// <param name="columnFamily">Column family.</param>
			/// <param name="columnQualifier">column qualifier.</param>
			/// <param name="dateTime">Timestamp.</param>
			Key( String^ row, String^ columnFamily, String^ columnQualifier, System::DateTime dateTime );

			/// <summary>
			/// Initializes a new instance of the Key class that is a copy of the specified instance.
			/// </summary>
			/// <param name="key">Key to copy.</param>
			Key( Key^ key );

			/// <summary>
			/// Gets or sets the row key.
			/// </summary>
			property String^ Row;

			/// <summary>
			/// Gets or sets the column family.
			/// </summary>
			property String^ ColumnFamily;

			/// <summary>
			/// Gets or sets the column qualifier.
			/// </summary>
			property String^ ColumnQualifier;

			/// <summary>
			/// Gets or sets the timestamp in nanoseconds since 1970-01-01 00:00:00.0 UTC.
			/// </summary>
			/// <remarks>
			/// If 0 the database assigns the timestamp automatically on a insert/update cell operation.
			/// </remarks>
			property UInt64 Timestamp;

			/// <summary>
			/// Gets or sets the timestamp.
			/// </summary>
			property System::DateTime DateTime {
				System::DateTime get();
				void set( System::DateTime value );
			}

			/// <summary>
			/// Compares this instance with a specified Key object and indicates whether this instance precedes, follows,
			/// or appears in the same position in the sort order as the specified Key.
			/// </summary>
			/// <param name="other">Key to compare, or null.</param>
			/// <returns>
			/// Signed integer that indicates the relationship between the comparand and this instance:
			/// <table class="comment">
			/// <tr><td>&lt; 0</td><td>if this instance precedes other.</td></tr>
			/// <tr><td>= 0</td><td>if this instance equals other.</td></tr>
			/// <tr><td>&gt; 0</td><td>if this instance follows other.</td></tr>
			/// </table>
			/// </returns>
			virtual int CompareTo( Key^ other );

			/// <summary>
			/// Determines whether this instance and an other Key object equals.
			/// </summary>
			/// <param name="other">Key to compare, or null.</param>
			/// <returns>true if the value of obj is the same as the value of this instance, otherwise false.</returns>
			virtual bool Equals( Key^ other );

			/// <summary>
			/// Determines whether this instance and a specified object, which must also be a Key object, equals.
			/// </summary>
			/// <param name="obj">Key to compare, or null.</param>
			/// <returns>true if the value of obj is the same as the value of this instance, otherwise false.</returns>
			virtual bool Equals( Object^ obj ) override;

			/// <summary>
			/// Returns the hash code for this Key.
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
			/// <returns>A new Key instance equal to this instance.</returns>
			virtual Object^ Clone( );

			/// <summary>
			/// Compares two specified keys and returns an integer that indicates their relative position in the sort order.
			/// </summary>
			/// <param name="x">The first key to compare, or null.</param>
			/// <param name="y">The second key to compare, or null.</param>
			/// <returns>
			/// Signed integer that indicates the relationship between the two comparands:
			/// <table class="comment">
			/// <tr><td>&lt; 0</td><td>if x is less than y</td></tr>
			/// <tr><td>= 0</td><td>if x equals y</td></tr>
			/// <tr><td>&gt; 0</td><td>if x is greater than y</td></tr>
			/// </table>
			/// </returns>
			static int Compare( Key^ x, Key^ y );

			/// <summary>
			/// Determines whether two specified keys are equal.
			/// </summary>
			/// <param name="x">The first key to compare, or null.</param>
			/// <param name="y">The second key to compare, or null.</param>
			/// <returns>true if the value of x is the same as the value of y, otherwise false.</returns>
			static bool operator == ( Key^ x, Key^ y );

			/// <summary>
			/// Determines whether two specified keys are different.
			/// </summary>
			/// <param name="x">The first key to compare, or null.</param>
			/// <param name="y">The second key to compare, or null.</param>
			/// <returns>true if the value of x is different as the value of y, otherwise false.</returns>
			static bool operator != ( Key^ x, Key^ y );

			/// <summary>
			/// Determines whether one specified key is less than the other.
			/// </summary>
			/// <param name="x">The first key to compare, or null.</param>
			/// <param name="y">The second key to compare, or null.</param>
			/// <returns>true if the value of x is less than the value of y, otherwise false.</returns>
			static bool operator < ( Key^ x, Key^ y );

			/// <summary>
			/// Determines whether one specified key is greater than the other.
			/// </summary>
			/// <param name="x">The first key to compare, or null.</param>
			/// <param name="y">The second key to compare, or null.</param>
			/// <returns>true if the value of x is greater than the value of y, otherwise false.</returns>
			static bool operator > ( Key^ x, Key^ y );

			/// <summary>
			/// Generates a base85 encoded GUID.
			/// </summary>
			/// <returns>A new base85 encoded GUID.</returns>
			static String^ Generate( );

			/// <summary>
			/// Encodes a specified GUID to base85.
			/// </summary>
			/// <param name="value">GUID to base85 encode.</param>
			/// <returns>base85 encoded GUID.</returns>
			static String^ Encode( System::Guid value );

			/// <summary>
			/// Decodes a base85 encoded GUID.
			/// </summary>
			/// <param name="value">base85 GUID to decode.</param>
			/// <returns>Decoded GUID.</returns>
			static System::Guid Decode( String^ value );

		internal:

			Key( const Common::Cell& cell );
			void From( const Common::Cell& cell );

		private:

			static System::DateTime timestampOrigin = System::DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind::Utc );
	};

}