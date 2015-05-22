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

#include "CellFlag.h"

namespace Hypertable {
	using namespace System;
	using namespace ht4c;

	ref class Key;
	ref class Cell;

	/// <summary>
	/// Represents a Hypertable counter, provide accessors to the counter attributes.
	/// </summary>
		/// <example>
	/// The following example shows how to insert a counter value.
	/// <code>
	/// using( var mutator = table.CreateMutator() ) {
	///    Key key = new Key("row", "cf");
	///    Counter counter = new Counter(key, 1234);
	///    mutator.Set(counter.ToCell());
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="Cell"/>
	[Serializable]
	public ref class Counter sealed {

		public:

			/// <summary>
			/// Initializes a new instance of the Counter class.
			/// </summary>
			Counter( );

			/// <summary>
			/// Initializes a new instance of the Counter class.
			/// </summary>
			/// <param name="key">Cell key.</param>
			/// <seealso cref="Key"/>
			Counter( Key^ key );

			/// <summary>
			/// Initializes a new instance of the Counter class using the specified key and counter value.
			/// </summary>
			/// <param name="key">Cell key.</param>
			/// <param name="counter">Counter value.</param>
			/// <seealso cref="Key"/>
			Counter( Key^ key, int64_t counter );

			/// <summary>
			/// Initializes a new instance of the Counter class using the specified key and counter value.
			/// </summary>
			/// <param name="key">Cell key.</param>
			/// <param name="counter">Counter value.</param>
			/// <param name="cloneKey">If true the constructor creates a deep copy if the specified key.</param>
			/// <seealso cref="Key"/>
			Counter( Key^ key, int64_t counter, bool cloneKey );

			/// <summary>
			/// Initializes a new instance of the Counter class using the specified cell.
			/// </summary>
			/// <param name="key">Cell key.</param>
			Counter( Cell^ key );

			/// <summary>
			/// Gets or sets the key.
			/// </summary>
			/// <seealso cref="Key"/>
			property Key^ Key;

			/// <summary>
			/// Gets or sets the cell flag.
			/// </summary>
			/// <seealso cref="CellFlag"/>
			property CellFlag Flag;

			/// <summary>
			/// Gets the counter value.
			/// </summary>
			property Nullable<int64_t> Value {
				Nullable<int64_t> get();
			}

			/// <summary>
			/// Resets the counter value to the value specified.
			/// </summary>
			/// <param name="n">Reset value.</param>
			void ResetCounter( int64_t n );

			/// <summary>
			/// Increments the counter value by the value specified.
			/// </summary>
			/// <param name="n">Increment value.</param>
			void IncrementCounter( int64_t n );

			/// <summary>
			/// Decrements the counter value by the value specified.
			/// </summary>
			/// <param name="n">Decrement value.</param>
			void DecrementCounter( int64_t n );

			/// <summary>
			/// Gets the counter value bytes, might be null.
			/// </summary>
			cli::array<Byte>^ GetBytes();

			/// <summary>
			/// Returns the cell that represents the counter.
			/// </summary>
			Cell^ ToCell();

			/// <summary>
			/// Returns a string that represents the current object.
			/// </summary>
			/// <returns>A string that represents the current object.</returns>
			virtual String^ ToString() override;

		private:

			Nullable<int64_t> value;
			String^ instruction;

	};

}