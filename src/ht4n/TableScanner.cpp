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

#include "stdafx.h"

#include "TableScanner.h"
#include "Cell.h"
#include "ScanSpec.h"
#include "Exception.h"

#include "ht4c.Common/TableScanner.h"
#include "ht4c.Common/Cell.h"

namespace Hypertable {
	using namespace System;
	using namespace ht4c;

	ref class TableScannerEnumerator sealed : public IEnumerator<Cell^> {

		public:

			virtual ~TableScannerEnumerator( ) {
			}

			virtual property Cell^ generic_Current {
				Cell^ get( ) = IEnumerator<Cell^>::Current::get {
					return cell;
				}
			}

			virtual property Object^ Current {
				Object^ get( ) = System::Collections::IEnumerator::Current::get {
					return generic_Current;
				}
			}

			virtual bool MoveNext( ) {
				return tableScanner->MoveNext( cell );
			}

			virtual void Reset( ) {
				throw gcnew InvalidOperationException(L"Unable to reset table scanner enumerator");
			}

		internal:

			TableScannerEnumerator( TableScanner^ _tableScanner ) 
			: tableScanner( _tableScanner ) {
			}

		private:

			TableScanner^ tableScanner;
			Cell^ cell;
	};
	
	TableScanner::~TableScanner( ) {
		disposed = true;
		this->!TableScanner();
		GC::SuppressFinalize(this);
	}

	TableScanner::!TableScanner( ) {
		HT4N_TRY {
			if( tableScanner ) {
				delete tableScanner;
				tableScanner = 0;
			}
		} 
		HT4N_RETHROW
	}

	bool TableScanner::Next( Cell^ cell ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( cell == nullptr ) throw gcnew ArgumentNullException( L"cell" );
		HT4N_TRY {
			Common::Cell* _cell;
			msclr::lock sync( syncRoot );
			if( tableScanner->next(_cell) ) {
				cell->From( *_cell );
				return true;
			}
			return false;
		} 
		HT4N_RETHROW
	}

	bool TableScanner::Next( Cell^% cell ) {
		return MoveNext( cell );
	}

	IEnumerator<Cell^>^ TableScanner::generic_GetEnumerator( ) {
		HT4N_THROW_OBJECTDISPOSED( );

		return gcnew TableScannerEnumerator( this );
	}

	TableScanner::TableScanner( Common::TableScanner* _tableScanner, Hypertable::ScanSpec^ _scanSpec )
	: tableScanner( _tableScanner )
	, scanSpec( _scanSpec )
	, syncRoot( gcnew Object() )
	, disposed( false )
	{
		if( tableScanner == 0 ) throw gcnew ArgumentNullException( L"tableScanner" );
	}

	bool TableScanner::MoveNext( Cell^% cell ) {
		HT4N_THROW_OBJECTDISPOSED( );

		HT4N_TRY {
			Common::Cell* _cell;
			msclr::lock sync( syncRoot );
			if( tableScanner->next(_cell) ) {
				cell = gcnew Cell( _cell );
				return true;
			}
			cell = nullptr;
			return false;
		}
		HT4N_RETHROW
	}

}