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

#include "ht4c.Common/ContextKind.h"

namespace Hypertable {
	using namespace System;

	interface class ITable;
	ref class ScanSpec;

	/// <summary>
	/// Represents a asynchronous table scanner context.
	/// </summary>
	public ref class AsyncScannerContext sealed {

		public:

			/// <summary>
			/// Gets the asynchronous table scanner identifier.
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
			/// Gets the table scanner specification.
			/// </summary>
			/// <seealso cref="ScanSpec"/>
			property Hypertable::ScanSpec^ ScanSpec {
				Hypertable::ScanSpec^ get() { return scanSpec; }
			}

			/// <summary>
			/// Gets the user defined parameter.
			/// </summary>
			property Object^ Param {
				Object^ get() { return param; }
			}

		internal:

			property Common::ContextKind ContextKind {
				Common::ContextKind get() { return contextKind; }
			}

			AsyncScannerContext( Common::ContextKind _contextKind, int64_t _id, Hypertable::ITable^ _table, Hypertable::ScanSpec^ _scanSpec, Object^ _param )
				: contextKind( _contextKind )
				, id( _id )
				, table( _table )
				, scanSpec( _scanSpec )
				, param( _param )
			{
			}

		private:

			Common::ContextKind contextKind; //TODO, re-design and remove
			int64_t id;
			Hypertable::ITable^ table;
			Hypertable::ScanSpec^ scanSpec;
			Object^ param;
	};

}