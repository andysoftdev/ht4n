/** -*- C++ -*-
 * Copyright (C) 2011 Andy Thalmann
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

	ref class Table;
	ref class MutatorSpec;

	/// <summary>
	/// Represents a asynchronous table mutator context.
	/// </summary>
	public ref class AsyncMutatorContext sealed {

		public:

			/// <summary>
			/// Gets the asynchronous table mutator identifier.
			/// </summary>
			/// <seealso cref="Table"/>
			property int64_t Id {
				int64_t get() { return id; }
			}

			/// <summary>
			/// Gets the table.
			/// </summary>
			/// <seealso cref="Table"/>
			property Hypertable::Table^ Table {
				Hypertable::Table^ get() { return table; }
			}

			/// <summary>
			/// Gets the table mutator specification.
			/// </summary>
			/// <seealso cref="MutatorSpec"/>
			property Hypertable::MutatorSpec^ MutatorSpec {
				Hypertable::MutatorSpec^ get() { return mutatorSpec; }
			}

		internal:

			AsyncMutatorContext( int64_t _id, Hypertable::Table^ _table, Hypertable::MutatorSpec^ _mutatorSpec )
				: id( _id )
				, table( _table )
				, mutatorSpec( _mutatorSpec )
			{
			}

		private:

			int64_t id;
			Hypertable::Table^ table;
			Hypertable::MutatorSpec^ mutatorSpec;
	};

}