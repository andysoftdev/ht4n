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

#include "ITableScanner.h"

namespace ht4c { namespace Common {
	class TableScanner;
} }

namespace Hypertable {
	using namespace System;
	using namespace System::Collections::Generic;
	using namespace System::Runtime::InteropServices;
	using namespace ht4c;

	ref class Cell;
	ref class ScanSpec;

	/// <summary>
	/// Represents a table scanner.
	/// </summary>
	/// <seealso cref="ITableScanner"/>
	ref class TableScanner sealed : public ITableScanner {

		public:

			/// <summary>
			/// Clean up all managed and unmanaged resources.
			/// </summary>
			virtual ~TableScanner( );

			/// <summary>
			/// Clean up all unmanaged resources.
			/// </summary>
			!TableScanner( );

			#pragma region ITableScanner methods

			property Hypertable::ScanSpec^ ScanSpec {
				virtual Hypertable::ScanSpec^ get( ) {
					return scanSpec;
				}
			}

			property bool IsDisposed {
				virtual bool get( ) {
					return disposed;
				}
			}

			virtual bool Move( Cell^ cell );
			virtual bool Next( [Out] Cell^% cell );

			virtual IEnumerator<Cell^>^ generic_GetEnumerator( ) = IEnumerable<Cell^>::GetEnumerator;

			virtual System::Collections::IEnumerator^ GetEnumerator( ) = System::Collections::IEnumerable::GetEnumerator {
				return generic_GetEnumerator();
			}

			#pragma endregion

		internal:

			TableScanner( Common::TableScanner* tableScanner, Hypertable::ScanSpec^ scanSpec );

			bool MoveNext( [Out] Cell^% cell );

		private:

			Common::TableScanner* tableScanner;
			Hypertable::ScanSpec^ scanSpec;
			Object^ syncRoot;
			bool disposed;
	};

}