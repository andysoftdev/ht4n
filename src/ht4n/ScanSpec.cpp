/** -*- C++ -*-
 * Copyright (C) 2010-2013 Thalmann Software & Consulting, http://www.softdev.ch
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

#include "stdafx.h"

#include "ScanSpec.h"
#include "Key.h"
#include "ColumnPredicate.h"
#include "RowInterval.h"
#include "CellInterval.h"
#include "Exception.h"
#include "CM2U8.h"

#include "ht4c.Common/ScanSpec.h"

namespace Hypertable {
	using namespace System;
	using namespace System::Globalization;
	using namespace System::Text;
	using namespace ht4c;

	ScanSpec::ScanSpec( )
	{
	}

	ScanSpec::ScanSpec( bool sorted )
	{
		isSorted = sorted;
	}

	ScanSpec::ScanSpec( String^ row )
	{
		AddRow( row );
	}
	
	ScanSpec::ScanSpec( Key^ key )
	{
		if( key == nullptr ) throw gcnew ArgumentNullException( L"key" );

		AddCell( key );

		if( key->Timestamp > 0 ) {
			MaxCells = 1;
			StartTimestamp = key->Timestamp;
			EndTimestamp = key->Timestamp;
		}
	}

	ScanSpec::ScanSpec( ColumnPredicate^ columnPredicate )
	{
		AddColumnPredicate( columnPredicate );
	}

	ScanSpec::ScanSpec( RowInterval^ rowInterval )
	{
		AddRowInterval( rowInterval );
	}

	ScanSpec::ScanSpec( CellInterval^ cellInterval )
	{
		AddCellInterval( cellInterval );
	}

	DateTime ScanSpec::StartDateTime::get( ) {
		return timestampOrigin + TimeSpan::FromTicks(StartTimestamp / 100);
	}
	
	void ScanSpec::StartDateTime::set( DateTime value) {
		if( value < timestampOrigin ) throw gcnew ArgumentException( L"Invalid DateTime" );
		if( value.Kind == DateTimeKind::Unspecified ) throw gcnew ArgumentException( L"Unspecified DateTime Kind" );
		StartTimestamp = (value.ToUniversalTime() - timestampOrigin).Ticks * 100;
	}

	DateTime ScanSpec::EndDateTime::get( ) {
		return timestampOrigin + TimeSpan::FromTicks(EndTimestamp / 100);
	}

	void ScanSpec::EndDateTime::set( DateTime value ) {
		if( value < timestampOrigin ) throw gcnew ArgumentException( L"Invalid DateTime" );
		if( value.Kind == DateTimeKind::Unspecified ) throw gcnew ArgumentException( L"Unspecified DateTime Kind" );
		EndTimestamp = (value.ToUniversalTime() - timestampOrigin).Ticks * 100;
	}

	ReadOnlyCollection<String^>^ ScanSpec::Rows::get( ) {
		if( rows == nullptr ) {
			rows = CreateCollection<String^>();
		}
		return AsReadOnly( rows );
	}
	
	ReadOnlyCollection<String^>^ ScanSpec::Columns::get( ) {
		if( columns == nullptr ) {
			columns = gcnew SortedSet<String^>();
		}
		return AsReadOnly( columns );
	}

	ReadOnlyCollection<ColumnPredicate^>^ ScanSpec::ColumnPredicates::get( ) {
		if( columnPredicates == nullptr ) {
			columnPredicates = gcnew HashSet<ColumnPredicate^>();
		}
		return AsReadOnly( columnPredicates );
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

	ScanSpec^ ScanSpec::AddRow( String^ row, ... cli::array<String^>^ moreRows ) {
		if( String::IsNullOrEmpty(row) ) throw gcnew ArgumentNullException( L"row" );

		if( rows == nullptr ) {
			rows = CreateCollection<String^>();
		}
		rows->Add( row );

		if( moreRows != nullptr ) {
			for each( String^ row in moreRows ) {
				AddRow( row );
			}
		}
		return this;
	}

	ScanSpec^ ScanSpec::AddRow( IEnumerable<String^>^ _rows ) {
		if( _rows == nullptr ) throw gcnew ArgumentNullException( L"rows" );

		if( rows == nullptr ) {
			rows = CreateCollection<String^>();
		}

		List<String^>^ list = dynamic_cast<List<String^>^>( rows );
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

	ScanSpec^ ScanSpec::AddColumn( String^ column, ... cli::array<String^>^ moreColumns ) {
		if( String::IsNullOrEmpty(column) ) throw gcnew ArgumentNullException( L"column" );

		if( columns == nullptr ) {
			columns = gcnew SortedSet<String^>();
		}
		columns->Add( column );

		if( moreColumns != nullptr ) {
			for each( String^ column in moreColumns ) {
				AddColumn( column );
			}
		}
		return this;
	}

	ScanSpec^ ScanSpec::AddColumn( IEnumerable<String^>^ _columns ) {
		if( _columns == nullptr ) throw gcnew ArgumentNullException( L"columns" );

		for each( String^ column in _columns ) {
			AddColumn( column );
		}
		return this;
	}

	ScanSpec^ ScanSpec::RemoveColumn( String^ column ) {
		if( String::IsNullOrEmpty(column) ) throw gcnew ArgumentNullException( L"column" );

		if( columns != nullptr ) {
			columns->Remove( column );
		}
		return this;
	}

	ScanSpec^ ScanSpec::AddColumnPredicate( ColumnPredicate^ columnPredicate, ... cli::array<ColumnPredicate^>^ moreColumnPredicates ) {
		if( columnPredicate == nullptr ) throw gcnew ArgumentNullException( L"columnPredicate" );
		if( String::IsNullOrEmpty(columnPredicate->ColumnFamily) ) throw gcnew ArgumentException(L"Invalid column family in column predicate", L"columnPredicate");
		if( columnPredicate->Match == MatchKind::Undefined ) throw gcnew ArgumentException(L"Invalid match kind in column predicate", L"columnPredicate");

		if( columnPredicates == nullptr ) {
			columnPredicates = gcnew HashSet<ColumnPredicate^>();
		}
		columnPredicates->Add( columnPredicate );

		if( moreColumnPredicates != nullptr ) {
			for each( ColumnPredicate^ columnPredicate in moreColumnPredicates ) {
				AddColumnPredicate( columnPredicate );
			}
		}
		return this;
	}

	ScanSpec^ ScanSpec::AddColumnPredicate( IEnumerable<ColumnPredicate^>^ _columnPredicates ) {
		if( _columnPredicates == nullptr ) throw gcnew ArgumentNullException( L"columnPredicates" );

		for each( ColumnPredicate^ columnPredicate in _columnPredicates ) {
			AddColumnPredicate( columnPredicate );
		}
		return this;
	}

	ScanSpec^ ScanSpec::RemoveColumnPredicate( ColumnPredicate^ columnPredicate ) {
		if( columnPredicate == nullptr ) throw gcnew ArgumentNullException( L"columnPredicate" );

		if( columnPredicates != nullptr ) {
			columnPredicates->Remove( columnPredicate );
		}
		return this;
	}

	ScanSpec^ ScanSpec::AddCell( String^ row, String^ columnFamily, String^ columnQualifier ) {
		AddCell( gcnew Key(row, columnFamily, columnQualifier) );
		return this;
	}

	ScanSpec^ ScanSpec::AddCell( Key^ key, ... cli::array<Key^>^ moreKeys ) {
		if( key == nullptr ) throw gcnew ArgumentNullException( L"key" );
		if( String::IsNullOrEmpty(key->Row) ) throw gcnew ArgumentException( L"Invalid parameter key (key.Row null or empty)", L"key" );
		if( String::IsNullOrEmpty(key->ColumnFamily) ) throw gcnew ArgumentException( L"Invalid parameter key (key.ColumnFamily null or empty)", L"key" );

		if( keys == nullptr ) {
			keys = CreateCollection<Key^>();
		}
		keys->Add( key );

		if( moreKeys != nullptr ) {
			for each( Key^ key in moreKeys ) {
				AddCell( key );
			}
		}
		return this;
	}

	ScanSpec^ ScanSpec::AddCell( IEnumerable<Key^>^ _keys ) {
		if( _keys == nullptr ) throw gcnew ArgumentNullException( L"keys" );

		for each( Key^ key in _keys ) {
			AddCell( key );
		}
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

	ScanSpec^ ScanSpec::AddRowInterval( RowInterval^ rowInterval, ... cli::array<RowInterval^>^ moreRowIntervals ) {
		if( rowInterval == nullptr ) throw gcnew ArgumentNullException( L"rowInterval" );

		if( rowIntervals == nullptr ) {
			rowIntervals = CreateCollection<RowInterval^>();
		}
		rowIntervals->Add( rowInterval );

		if( moreRowIntervals != nullptr ) {
			for each( RowInterval^ rowInterval in moreRowIntervals ) {
				AddRowInterval( rowInterval );
			}
		}
		return this;
	}

	ScanSpec^ ScanSpec::AddRowInterval( IEnumerable<RowInterval^>^ _rowIntervals ) {
		if( _rowIntervals == nullptr ) throw gcnew ArgumentNullException( L"rowIntervals" );

		for each( RowInterval^ rowInterval in _rowIntervals ) {
			AddRowInterval( rowInterval );
		}
		return this;
	}

	ScanSpec^ ScanSpec::RemoveRowInterval( RowInterval^ rowInterval ) {
		if( rowInterval == nullptr ) throw gcnew ArgumentNullException( L"rowInterval" );

		if( rowIntervals != nullptr ) {
			rowIntervals->Remove( rowInterval );
		}
		return this;
	}

	ScanSpec^ ScanSpec::AddCellInterval( CellInterval^ cellInterval, ... cli::array<CellInterval^>^ moreCellIntervals ) {
		if( cellInterval == nullptr ) throw gcnew ArgumentNullException( L"cellInterval" );
		if( String::IsNullOrEmpty(cellInterval->StartColumnFamily) ) throw gcnew ArgumentException( L"Invalid parameter cellInterval (cellInterval.StartColumnFamily null or empty)", L"cellInterval" );
		if( String::IsNullOrEmpty(cellInterval->EndColumnFamily) ) throw gcnew ArgumentException( L"Invalid parameter cellInterval (cellInterval.EndColumnFamily null or empty)", L"cellInterval" );

		if( cellIntervals == nullptr ) {
			cellIntervals = CreateCollection<CellInterval^>();
		}
		cellIntervals->Add( cellInterval );

		if( moreCellIntervals != nullptr ) {
			for each( CellInterval^ cellInterval in moreCellIntervals ) {
				AddCellInterval( cellInterval );
			}
		}
		return this;
	}

	ScanSpec^ ScanSpec::AddCellInterval( IEnumerable<CellInterval^>^ _cellIntervals ) {
		if( _cellIntervals == nullptr ) throw gcnew ArgumentNullException( L"cellIntervals" );

		for each( CellInterval^ cellInterval in _cellIntervals ) {
			AddCellInterval( cellInterval );
		}
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

	String^ ScanSpec::ToString() {

		#define APPEND_INT( what ) if( what > 0 ) sb->Append( String::Format(CultureInfo::InvariantCulture, L#what##L"={0}, ", what) );
		#define APPEND_BOOL( what ) if( what ) sb->Append( L#what##L", " );
		#define APPEND_STRING( what ) if( what != nullptr ) sb->Append( String::Format(CultureInfo::InvariantCulture, L#what##L"={0}, ", what) );
		#define APPEND_DATETIME( what ) if( what.Ticks != 0 ) sb->Append( String::Format(CultureInfo::InvariantCulture, L#what##L"={0}, ", what) );
		#define APPEND_TIMESPAN( what ) if( what.Ticks != 0 ) sb->Append( String::Format(CultureInfo::InvariantCulture, L#what##L"={0}, ", what) );

		StringBuilder^ sb = gcnew StringBuilder();
		sb->Append( GetType() );
		sb->Append( L"(" );

		APPEND_INT( MaxRows )
		APPEND_INT( MaxVersions )
		APPEND_INT( MaxCells )
		APPEND_INT( MaxCellsColumnFamily )
		APPEND_INT( RowOffset )
		APPEND_INT( CellOffset )
		APPEND_BOOL( KeysOnly )
		APPEND_BOOL( ScanAndFilter )
		APPEND_DATETIME( StartDateTime )
		APPEND_DATETIME( EndDateTime )
		APPEND_STRING( RowRegex )
		APPEND_STRING( ValueRegex )
		APPEND_TIMESPAN( Timeout )
		APPEND_INT( RowCount )
		APPEND_INT( ColumnCount )
		if( ColumnCount > 0 ) {
			sb->Append(L"Columns=[");
			for each( String^ column in columns ) {
				sb->Append(column);
				sb->Append(L", ");
			}
			sb->Length -= 2;
			sb->Append(L"], ");
		}
		if( ColumnPredicateCount > 0 ) {
			sb->Append(L"ColumnPredicates=[");
			for each( ColumnPredicate^ columnPredicate in columnPredicates ) {
				sb->Append(columnPredicate->ToString());
				sb->Append(L", ");
			}
			sb->Length -= 2;
			sb->Append(L"], ");
		}
		APPEND_INT( CellCount )
		APPEND_INT( RowIntervalCount )
		APPEND_INT( CellIntervalCount )
		sb->Append( String::Format(CultureInfo::InvariantCulture, L"Flags={0}", Flags) );
		sb->Append( L")" );

		return sb->ToString();

		#undef APPEND_TIMESPAN
		#undef APPEND_DATETIME
		#undef APPEND_STRING
		#undef APPEND_BOOL
		#undef APPEND_INT
	}

	ISet<String^>^ ScanSpec::DistictColumn( IEnumerable<String^>^ source ) {
		if( source == nullptr ) {
			return nullptr;
		}

		SortedSet<String^>^ _source = gcnew SortedSet<String^>( source );
		SortedSet<String^>^ distinct = gcnew SortedSet<String^>( );
		for each( String^ column in _source ) {
			cli::array<String^>^ _column = column->Split(L':');
			if( !distinct->Contains(_column[0]) ) {
				distinct->Add( column );
			}
		}

		return distinct;
	}

	ISet<String^>^ ScanSpec::DistictColumn( IEnumerable<String^>^ source, [Out] ISet<String^>^% columnFamilies ) {
		if( source == nullptr ) {
			return nullptr;
		}

		columnFamilies = gcnew SortedSet<String^>( );
		SortedSet<String^>^ _source = gcnew SortedSet<String^>( source );
		SortedSet<String^>^ distinct = gcnew SortedSet<String^>( );
		for each( String^ column in _source ) {
			cli::array<String^>^ _column = column->Split(L':');
			if( !distinct->Contains(_column[0]) ) {
				distinct->Add( column );
				columnFamilies->Add(_column[0]);
			}
		}

		return distinct;
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
		if( MaxCellsColumnFamily > 0 ) {
			scanSpec.maxCellsColumnFamily( MaxCellsColumnFamily );
		}
		if( RowOffset > 0 ) {
			scanSpec.rowOffset( RowOffset );
		}
		else if( CellOffset > 0 ) {
			scanSpec.cellOffset( CellOffset );
		}
		scanSpec.keysOnly( KeysOnly );
		scanSpec.scanAndFilter( ScanAndFilter );

		if( StartTimestamp > 0 ) {
			scanSpec.startTimestamp( StartTimestamp );
		}
		if( EndTimestamp > 0 ) {
			scanSpec.endTimestamp( EndTimestamp );
		}
		if( !String::IsNullOrEmpty(RowRegex) ) {
			scanSpec.rowRegex( CM2U8(RowRegex) );
		}
		if( !String::IsNullOrEmpty(ValueRegex) ) {
			scanSpec.valueRegex( CM2U8(ValueRegex) );
		}
		if( rows != nullptr && rows->Count > 0 ) {
			scanSpec.reserveRows( rows->Count );
			for each( String^ row in rows ) {
				scanSpec.addRow( CM2U8(row) );
			}
		}
		HashSet<String^>^ _columnFamilies = gcnew HashSet<String^>();
		if( columnPredicates != nullptr && columnPredicates->Count > 0 ) {
			for each( ColumnPredicate^ columnPredicate in columnPredicates ) {
				if( String::IsNullOrEmpty(columnPredicate->ColumnFamily) ) throw gcnew BadScanSpecException(L"Invalid column family in column predicate");
				if( columnPredicate->Match == MatchKind::Undefined ) throw gcnew BadScanSpecException(L"Invalid match kind in column predicate");

				if( !_columnFamilies->Contains(columnPredicate->ColumnFamily) ) {
					scanSpec.addColumn( CM2U8(columnPredicate->ColumnFamily) );
					_columnFamilies->Add( columnPredicate->ColumnFamily );
				}

				if( columnPredicate->SearchValue != nullptr ) {
					if( columnPredicate->SearchValue->Length > 0 ) {
						pin_ptr<byte> searchValue = &columnPredicate->SearchValue[0];
						scanSpec.addColumnPredicate( CM2U8(columnPredicate->ColumnFamily), (uint32_t)columnPredicate->Match, reinterpret_cast<const char*>(searchValue), columnPredicate->SearchValue->Length );
					}
					else {
						scanSpec.addColumnPredicate( CM2U8(columnPredicate->ColumnFamily), (uint32_t)columnPredicate->Match, "", 0 );
					}
				}
				else {
					scanSpec.addColumnPredicate( CM2U8(columnPredicate->ColumnFamily), (uint32_t)columnPredicate->Match, 0, 0 );
				}
			}
		}
		if( columns != nullptr && columns->Count > 0 ) {
			scanSpec.reserveColumns( columns->Count );
			for each( String^ column in columns ) {
				cli::array<String^>^ _column = column->Split(L':');
				if( !_columnFamilies->Contains(_column[0]) ) {
					scanSpec.addColumn( CM2U8(column) );
					if( _column->Length == 1 ) {
						_columnFamilies->Add( _column[0] );
					}
				}
			}
		}
		if( keys != nullptr && keys->Count > 0 ) {
			scanSpec.reserveCells( keys->Count );
			for each( Key^ key in keys ) {
				if( key->ColumnQualifier == nullptr ) {
					scanSpec.addCell( CM2U8(key->Row), CM2U8(key->ColumnFamily) );
				}
				else {
					StringBuilder^ sb = gcnew StringBuilder();
					sb->Append( key->ColumnFamily );
					sb->Append( L":" );
					sb->Append( key->ColumnQualifier );
					scanSpec.addCell( CM2U8(key->Row), CM2U8(sb->ToString()) );
				}
			}
		}
		if( rowIntervals != nullptr ) {
			for each( RowInterval^ rowInterval in rowIntervals ) {
				scanSpec.addRowInterval( CM2U8(rowInterval->StartRow), rowInterval->IncludeStartRow, CM2U8(rowInterval->EndRow), rowInterval->IncludeEndRow );
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
				scanSpec.addCellInterval( CM2U8(cellInterval->StartRow), CM2U8(startColumn), cellInterval->IncludeStartRow, CM2U8(cellInterval->EndRow), CM2U8(endColumn), cellInterval->IncludeEndRow );
			}
		}
	}

}