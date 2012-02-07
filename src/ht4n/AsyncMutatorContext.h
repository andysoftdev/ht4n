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

#include "ht4c.Common/ContextKind.h"

namespace Hypertable {
	using namespace System;

	interface class ITable;
	ref class MutatorSpec;

	/// <summary>
	/// Represents a asynchronous table mutator context.
	/// </summary>
	public ref class AsyncMutatorContext sealed {

		public:

			/// <summary>
			/// Gets the asynchronous table mutator identifier.
			/// </summary>
			/// <seealso cref="ITable"/>
			property int64_t Id {
				int64_t get() { return id; }
			}

			/// <summary>
			/// Gets the table.
			/// </summary>
			/// <seealso cref="ITable"/>
			property Hypertable::ITable^ Table {
				Hypertable::ITable^ get() { return table; }
			}

			/// <summary>
			/// Gets the table mutator specification.
			/// </summary>
			/// <seealso cref="MutatorSpec"/>
			property Hypertable::MutatorSpec^ MutatorSpec {
				Hypertable::MutatorSpec^ get() { return mutatorSpec; }
			}

		internal:

			property Common::ContextKind ContextKind {
				Common::ContextKind get() { return contextKind; }
			}

			AsyncMutatorContext( Common::ContextKind _contextKind, int64_t _id, Hypertable::ITable^ _table, Hypertable::MutatorSpec^ _mutatorSpec )
				: contextKind( _contextKind )
				, id( _id )
				, table( _table )
				, mutatorSpec( _mutatorSpec )
			{
			}

		private:

			Common::ContextKind contextKind; //TODO, re-design and remove
			int64_t id;
			Hypertable::ITable^ table;
			Hypertable::MutatorSpec^ mutatorSpec;
	};

}