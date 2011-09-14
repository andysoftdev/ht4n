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

#include "ScanSpec.h"
#include "Key.h"
#include "RowInterval.h"
#include "CellInterval.h"
#include "Exception.h"
#include "CM2A.h"

#include "ht4c.Common/ScanSpec.h"

namespace Hypertable {
	using namespace System;
	using namespace System::Text;
	using namespace ht4c;

	ScanSpec::ScanSpec( ) {
	}

	ScanSpec::ScanSpec( bool sorted ) {
		isSorted = sorted;
	}

	ScanSpec::ScanSpec( String^ row ) {
		AddRow( row );
	}
	
	ScanSpec::ScanSpec( Key^ key ) {
		AddCell( key );
	}

	ScanSpec::ScanSpec( RowInterval^ rowInterval ) {
		AddRowInterval( rowInterval );
	}

	ScanSpec::ScanSpec( CellInterval^ cellInterval ) {
		AddCellInterval( cellInterval );
	}

	ReadOnlyCollection<String^>^ ScanSpec::Rows::get( ) {
		if( rows == nullptr ) {
			rows = CreateCollection<String^>();
		}
		return AsReadOnly( rows );
	}
	
	ReadOnlyCollection<String^>^ ScanSpec::Columns::get( ) {
		if( columns == nullptr ) {
			columns = gcnew HashSet<String^>();
		}
		return AsReadOnly( columns );
	}

	ReadOnlyCollection<Key^>^ ScanSpec::Cells::get( ) {
		if( keys == nullptr ) {
			keys = CreateCollection<Key^>();
		}
		return AsReadOnly( keys );
	}

	ReadOnlyCollection<RowInterval^>^ ScanSpec::RowIntervals::get( ) {
		if( rowIntervals == nullptr ) {
			rowIntervals = CreateCollection<RowInterval^>();
		}
		return AsReadOnly( rowIntervals );
	}

	ReadOnlyCollection<CellInterval^>^ ScanSpec::CellIntervals::get( ) {
		if( cellIntervals == nullptr ) {
			cellIntervals = CreateCollection<CellInterval^>();
		}
		return AsReadOnly( cellIntervals );
	}

	ScanSpec^ ScanSpec::AddRow( String^ row ) {
		if( String::IsNullOrEmpty(row) ) throw gcnew ArgumentNullException( L"row" );

		if( rows == nullptr ) {
			rows = CreateCollection<String^>();
		}
		rows->Add( row );
		return this;
	}

	ScanSpec^ ScanSpec::AddRows( IEnumerable<String^>^ _rows ) {
		if( _rows == nullptr ) throw gcnew ArgumentNullException( L"rows" );

		if( rows == nullptr ) {
			rows = CreateCollection<String^>();
		}

		List<String^>^ list = dynamic_cast<List<String^>^>( _rows );
		if( list == nullptr ) {
			for each( String^ row in _rows ) {
				rows->Add( row );
			}
		}
		else {
			list->AddRange( _rows );
		}
		return this;
	}

	ScanSpec^ ScanSpec::RemoveRow( String^ row ) {
		if( String::IsNullOrEmpty(row) ) throw gcnew ArgumentNullException( L"row" );

		if( rows != nullptr ) {
			rows->Remove( row );
		}
		return this;
	}

	bool ScanSpec::ContainsRow( String^ row ) {
		if( String::IsNullOrEmpty(row) ) throw gcnew ArgumentNullException( L"row" );

		return rows != nullptr ? rows->Contains( row ) : false;
	}

	ScanSpec^ ScanSpec::AddColumn( String^ column ) {
		if( String::IsNullOrEmpty(column) ) throw gcnew ArgumentNullException( L"column" );

		if( columns == nullptr ) {
			columns = gcnew HashSet<String^>();
		}
		columns->Add( column );
		return this;
	}

	ScanSpec^ ScanSpec::RemoveColumn( String^ column ) {
		if( String::IsNullOrEmpty(column) ) throw gcnew ArgumentNullException( L"column" );

		if( columns != nullptr ) {
			columns->Remove( column );
		}
		return this;
	}

	ScanSpec^ ScanSpec::AddCell( String^ row, String^ columnFamily, String^ columnQualifier ) {
		AddCell( gcnew Key(row, columnFamily, columnQualifier) );
		return this;
	}

	ScanSpec^ ScanSpec::AddCell( Key^ key ) {
		if( key == nullptr ) throw gcnew ArgumentNullException( L"key" );
		if( String::IsNullOrEmpty(key->Row) ) throw gcnew ArgumentException( L"Invalid parameter key (key.Row null or empty)", L"key" );
		if( String::IsNullOrEmpty(key->ColumnFamily) ) throw gcnew ArgumentException( L"Invalid parameter key (key.ColumnFamily null or empty)", L"key" );

		if( keys == nullptr ) {
			keys = CreateCollection<Key^>();
		}
		keys->Add( key );
		return this;
	}

	ScanSpec^ ScanSpec::RemoveCell( String^ row, String^ columnFamily, String^ columnQualifier ) {
		RemoveCell( gcnew Key(row, columnFamily, columnQualifier) );
		return this;
	}

	ScanSpec^ ScanSpec::RemoveCell( Key^ key ) {
		if( key == nullptr ) throw gcnew ArgumentNullException( L"key" );
		if( String::IsNullOrEmpty(key->Row) ) throw gcnew ArgumentException( L"Invalid parameter key (key.Row null or empty)", L"key" );
		if( String::IsNullOrEmpty(key->ColumnFamily) ) throw gcnew ArgumentException( L"Invalid parameter key (key.ColumnFamily null or empty)", L"key" );

		if( keys != nullptr ) {
			keys->Remove( key );
		}
		return this;
	}

	ScanSpec^ ScanSpec::AddRowInterval( RowInterval^ rowInterval ) {
		if( rowInterval == nullptr ) throw gcnew ArgumentNullException( L"rowInterval" );
		if( String::IsNullOrEmpty(rowInterval->StartRow) ) throw gcnew ArgumentException( L"Invalid parameter rowInterval (rowInterval.StartRow null or empty)", L"rowInterval" );
		if( String::IsNullOrEmpty(rowInterval->EndRow) ) throw gcnew ArgumentException( L"Invalid parameter rowInterval (rowInterval.EndRow null or empty)", L"rowInterval" );

		if( rowIntervals == nullptr ) {
			rowIntervals = CreateCollection<RowInterval^>();
		}
		rowIntervals->Add( rowInterval );
		return this;
	}

	ScanSpec^ ScanSpec::RemoveRowInterval( RowInterval^ rowInterval ) {
		if( rowInterval == nullptr ) throw gcnew ArgumentNullException( L"rowInterval" );
		if( String::IsNullOrEmpty(rowInterval->StartRow) ) throw gcnew ArgumentException( L"Invalid parameter rowInterval (rowInterval.StartRow null or empty)", L"rowInterval" );
		if( String::IsNullOrEmpty(rowInterval->EndRow) ) throw gcnew ArgumentException( L"Invalid parameter rowInterval (rowInterval.EndRow null or empty)", L"rowInterval" );

		if( rowIntervals != nullptr ) {
			rowIntervals->Remove( rowInterval );
		}
		return this;
	}

	ScanSpec^ ScanSpec::AddCellInterval( CellInterval^ cellInterval ) {
		if( cellInterval == nullptr ) throw gcnew ArgumentNullException( L"cellInterval" );
		if( String::IsNullOrEmpty(cellInterval->StartRow) ) throw gcnew ArgumentException( L"Invalid parameter cellInterval (cellInterval.StartRow null or empty)", L"cellInterval" );
		if( String::IsNullOrEmpty(cellInterval->StartColumnFamily) ) throw gcnew ArgumentException( L"Invalid parameter cellInterval (cellInterval.StartColumnFamily null or empty)", L"cellInterval" );
		if( String::IsNullOrEmpty(cellInterval->EndRow) ) throw gcnew ArgumentException( L"Invalid parameter cellInterval (cellInterval.EndRow null or empty)", L"cellInterval" );
		if( String::IsNullOrEmpty(cellInterval->EndColumnFamily) ) throw gcnew ArgumentException( L"Invalid parameter cellInterval (cellInterval.EndColumnFamily null or empty)", L"cellInterval" );

		if( cellIntervals == nullptr ) {
			cellIntervals = CreateCollection<CellInterval^>();
		}
		cellIntervals->Add( cellInterval );
		return this;
	}

	ScanSpec^ ScanSpec::RemoveCellInterval( CellInterval^ cellInterval ) {
		if( cellInterval == nullptr ) throw gcnew ArgumentNullException( L"cellInterval" );
		if( String::IsNullOrEmpty(cellInterval->StartRow) ) throw gcnew ArgumentException( L"Invalid parameter cellInterval (cellInterval.StartRow null or empty)", L"cellInterval" );
		if( String::IsNullOrEmpty(cellInterval->StartColumnFamily) ) throw gcnew ArgumentException( L"Invalid parameter cellInterval (cellInterval.StartColumnFamily null or empty)", L"cellInterval" );
		if( String::IsNullOrEmpty(cellInterval->EndRow) ) throw gcnew ArgumentException( L"Invalid parameter cellInterval (cellInterval.EndRow null or empty)", L"cellInterval" );
		if( String::IsNullOrEmpty(cellInterval->EndColumnFamily) ) throw gcnew ArgumentException( L"Invalid parameter cellInterval (cellInterval.EndColumnFamily null or empty)", L"cellInterval" );

		if( cellIntervals != nullptr ) {
			cellIntervals->Remove( cellInterval );
		}
		return this;
	}

	void ScanSpec::To( Common::ScanSpec& scanSpec ) {
		if( MaxRows > 0 ) {
			scanSpec.maxRows( MaxRows );
		}
		if( MaxVersions > 0 ) {
			scanSpec.maxVersions( MaxVersions );
		}
		if( MaxCells > 0 ) {
			scanSpec.maxCells( MaxCells );
		}
		scanSpec.keysOnly( KeysOnly );
		scanSpec.scanAndFilter( ScanAndFilter );

		if( StartDateTime.Ticks ) {
			if( StartDateTime.Kind == DateTimeKind::Unspecified ) throw gcnew BadScanSpecException( L"Unspecified StartDateTime Kind" );
			scanSpec.startTimestamp( (StartDateTime.ToUniversalTime() - timestampOrigin).Ticks * 100 );
		}
		if( EndDateTime.Ticks ) {
			if( EndDateTime.Kind == DateTimeKind::Unspecified ) throw gcnew BadScanSpecException( L"Unspecified EndDateTime Kind" );
			scanSpec.endTimestamp( (EndDateTime.ToUniversalTime() - timestampOrigin).Ticks * 100 );
		}
		if( !String::IsNullOrEmpty(RowRegex) ) {
			scanSpec.rowRegex( CM2A(RowRegex) );
		}
		if( !String::IsNullOrEmpty(ValueRegex) ) {
			scanSpec.valueRegex( CM2A(ValueRegex) );
		}
		if( rows != nullptr && rows->Count > 0 ) {
			scanSpec.reserveRows( rows->Count );
			for each( String^ row in rows ) {
				scanSpec.addRow( CM2A(row) );
			}
		}
		if( columns != nullptr && columns->Count > 0 ) {
			scanSpec.reserveColumns( columns->Count );
			for each( String^ column in columns ) {
				scanSpec.addColumn( CM2A(column) );
			}
		}
		if( keys != nullptr && keys->Count > 0 ) {
			scanSpec.reserveCells( keys->Count );
			for each( Key^ key in keys ) {
				if( key->ColumnQualifier == nullptr ) {
					scanSpec.addCell( CM2A(key->Row), CM2A(key->ColumnFamily) );
				}
				else {
					StringBuilder^ sb = gcnew StringBuilder();
					sb->Append( key->ColumnFamily );
					sb->Append( L":" );
					sb->Append( key->ColumnQualifier );
					scanSpec.addCell( CM2A(key->Row), CM2A(sb->ToString()) );
				}
			}
		}
		if( rowIntervals != nullptr ) {
			for each( RowInterval^ rowInterval in rowIntervals ) {
				scanSpec.addRowInterval( CM2A(rowInterval->StartRow), rowInterval->IncludeStartRow, CM2A(rowInterval->EndRow), rowInterval->IncludeEndRow );
			}
		}
		if( cellIntervals != nullptr ) {
			for each( CellInterval^ cellInterval in cellIntervals ) {
				String^ startColumn;
				if( cellInterval->StartColumnQualifier == nullptr ) {
					startColumn = cellInterval->StartColumnFamily;
				}
				else {
					StringBuilder^ sb = gcnew StringBuilder();
					sb->Append( cellInterval->StartColumnFamily );
					sb->Append( L":" );
					sb->Append( cellInterval->StartColumnQualifier );
					startColumn = sb->ToString();
				}
				String^ endColumn;
				if( cellInterval->StartColumnQualifier == nullptr ) {
					endColumn = cellInterval->EndColumnFamily;
				}
				else {
					StringBuilder^ sb = gcnew StringBuilder();
					sb->Append( cellInterval->EndColumnFamily );
					sb->Append( L":" );
					sb->Append( cellInterval->EndColumnQualifier );
					endColumn = sb->ToString();
				}
				scanSpec.addCellInterval( CM2A(cellInterval->StartRow), CM2A(startColumn), cellInterval->IncludeStartRow, CM2A(cellInterval->EndRow), CM2A(endColumn), cellInterval->IncludeEndRow );
			}
		}
	}

}