/** -*- C++ -*-
 * Copyright (C) 2010-2016 Thalmann Software & Consulting, http://www.softdev.ch
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

#include "ScanSpecBuilder.h"
#include "ScanSpec.h"

namespace Hypertable {
	using namespace System;
	using namespace System::Globalization;

	namespace Fluent {

		#define Continue( T ) \
			gcnew T( scanSpec );

		ref class ScanSpecBuilderBase  {

			protected:

				ScanSpecBuilderBase( ScanSpec^ _scanSpec )
				: scanSpec( _scanSpec ) {
				}

				ScanSpec^ scanSpec;
		};

		ref class ScanSpecBuilderBaseOp : public ScanSpecBuilderBase
																		, public IScanSpecBuilderOp {

			public:

				virtual IScanSpecBuilderOp^ MaxRows( int value ) {
					scanSpec->MaxRows = value;
					return this;
				}
				virtual IScanSpecBuilderOp^ MaxVersions( int value ) {
					scanSpec->MaxVersions = value;
					return this;
				}
				virtual IScanSpecBuilderOp^ MaxCells( int value ) {
					scanSpec->MaxCells = value;
					return this;
				}
				virtual IScanSpecBuilderOp^ MaxCellsPerColumnFamily( int value ) {
					scanSpec->MaxCellsColumnFamily = value;
					return this;
				}
				virtual IScanSpecBuilderOp^ RowOffset( int value ) {
					scanSpec->RowOffset = value;
					return this;
				}
				virtual IScanSpecBuilderOp^ CellOffset( int value ) {
					scanSpec->CellOffset = value;
					return this;
				}

				virtual IScanSpecBuilderOp^ StartDateTime( DateTime value ) {
					scanSpec->StartDateTime = value;
					return this;
				}
				virtual IScanSpecBuilderOp^ EndDateTime( DateTime value ) {
					scanSpec->EndDateTime = value;
					return this;
				}

				virtual IScanSpecBuilderOp^ RowRegex( String^ value ) {
					scanSpec->RowRegex = value;
					return this;
				}
				virtual IScanSpecBuilderOp^ ValueRegex( String^ value ) {
					scanSpec->ValueRegex = value;
					return this;
				}

				virtual IScanSpecBuilderOp^ ScanAndFilter( ) {
					scanSpec->ScanAndFilter = true;
					return this;
				}
				virtual IScanSpecBuilderOp^ ColumnPredicateAnd( ) {
					scanSpec->ColumnPredicateAnd = true;
					return this;
				}
				virtual IScanSpecBuilderOp^ KeysOnly( ) {
					scanSpec->KeysOnly = true;
					return this;
				}
				virtual IScanSpecBuilderOp^ NotUseQueryCache( ) {
					scanSpec->NotUseQueryCache = true;
					return this;
				}

				virtual IScanSpecBuilderOp^ Timeout( TimeSpan value ) {
					scanSpec->Timeout = value;
					return this;
				}
				virtual IScanSpecBuilderOp^ Flags( ScannerFlags value ) {
					scanSpec->Flags = value;
					return this;
				}

				virtual ScanSpec^ Build( ) {
					return scanSpec;
				}

			protected:

				ScanSpecBuilderBaseOp( ScanSpec^ _scanSpec )
				: ScanSpecBuilderBase( _scanSpec ) {
				}
		};

		ref class ScanSpecBuilderWithRowOp sealed : public ScanSpecBuilderBaseOp
																							, public IScanSpecBuilderWithRowOp {

			public:

				ScanSpecBuilderWithRowOp( ScanSpec^ scanSpec )
				: ScanSpecBuilderBaseOp( scanSpec ) {
				}

				virtual IScanSpecBuilderWithRowOp^ WithRows( ) {
					return this;
				}
				virtual IScanSpecBuilderWithRowOp^ WithRows( String^ row, ... cli::array<String^>^ moreRows ) {
					scanSpec->AddRow( row, moreRows );
					return this;
				}
				virtual IScanSpecBuilderWithRowOp^ WithRows( IEnumerable<String^>^ rows ) {
					scanSpec->AddRow( rows );
					return this;
				}
				virtual IScanSpecBuilderWithRowOp^ WithRows( RowInterval^ rowInterval, ... cli::array<RowInterval^>^ moreRowIntervals ) {
					scanSpec->AddRowInterval( rowInterval, moreRowIntervals );
					return this;
				}
				virtual IScanSpecBuilderWithRowOp^ WithRows( IEnumerable<RowInterval^>^ rowIntervals ) {
					scanSpec->AddRowInterval( rowIntervals );
					return this;
				}

				virtual IScanSpecBuilderWithRowOp^ WithColumnPredicates( ColumnPredicate^ columnPredicate, ... cli::array<ColumnPredicate^>^ moreColumnPredicates ) {
					scanSpec->AddColumnPredicate( columnPredicate, moreColumnPredicates );
					return this;
				}
				virtual IScanSpecBuilderWithRowOp^ WithColumnPredicates( IEnumerable<ColumnPredicate^>^ columnPredicates ) {
					scanSpec->AddColumnPredicate( columnPredicates );
					return this;
				}
		};

		ref class ScanSpecBuilderWithCellOp sealed : public ScanSpecBuilderBaseOp
																							 , public IScanSpecBuilderWithCellOp {

			public:

				ScanSpecBuilderWithCellOp( ScanSpec^ scanSpec )
				: ScanSpecBuilderBaseOp( scanSpec ) {
				}

				virtual IScanSpecBuilderWithCellOp^ WithCells( String^ row, String^ columnFamily, String^ columnQualifier ) {
					scanSpec->AddCell( row, columnFamily, columnQualifier );
					return this;
				}
				virtual IScanSpecBuilderWithCellOp^ WithCells( Key^ key, ... cli::array<Key^>^ moreKeys ) {
					scanSpec->AddCell( key, moreKeys );
					return this;
				}
				virtual IScanSpecBuilderWithCellOp^ WithCells( IEnumerable<Key^>^ keys ) {
					scanSpec->AddCell( keys );
					return this;
				}
				virtual IScanSpecBuilderWithCellOp^ WithCells( CellInterval^ cellInterval, ... cli::array<CellInterval^>^ moreCellIntervals ) {
					scanSpec->AddCellInterval( cellInterval, moreCellIntervals );
					return this;
				}
				virtual IScanSpecBuilderWithCellOp^ WithCells( IEnumerable<CellInterval^>^ cellIntervals ) {
					scanSpec->AddCellInterval( cellIntervals );
					return this;
				}
		};

		ref class ScanSpecBuilderWhere sealed : public ScanSpecBuilderBase
																					, public IScanSpecBuilderWhere {

			public:

				ScanSpecBuilderWhere( ScanSpec^ scanSpec )
				: ScanSpecBuilderBase( scanSpec ) {
				}

				virtual IScanSpecBuilderWithRowOp^ WithRows( ) {
					return Continue( ScanSpecBuilderWithRowOp );
				}
				virtual IScanSpecBuilderWithRowOp^ WithRows( String^ row, ... cli::array<String^>^ moreRows ) {
					scanSpec->AddRow( row, moreRows );
					return Continue( ScanSpecBuilderWithRowOp );
				}
				virtual IScanSpecBuilderWithRowOp^ WithRows( IEnumerable<String^>^ rows ) {
					scanSpec->AddRow( rows );
					return Continue( ScanSpecBuilderWithRowOp );
				}
				virtual IScanSpecBuilderWithRowOp^ WithRows( RowInterval^ rowInterval, ... cli::array<RowInterval^>^ moreRowIntervals ) {
					scanSpec->AddRowInterval( rowInterval, moreRowIntervals );
					return Continue( ScanSpecBuilderWithRowOp );
				}
				virtual IScanSpecBuilderWithRowOp^ WithRows( IEnumerable<RowInterval^>^ rowIntervals ) {
					scanSpec->AddRowInterval( rowIntervals );
					return Continue( ScanSpecBuilderWithRowOp );
				}

				virtual IScanSpecBuilderWithRowOp^ WithColumnPredicates( ColumnPredicate^ columnPredicate, ... cli::array<ColumnPredicate^>^ moreColumnPredicates ) {
					scanSpec->AddColumnPredicate( columnPredicate, moreColumnPredicates );
					return Continue( ScanSpecBuilderWithRowOp );
				}
				virtual IScanSpecBuilderWithRowOp^ WithColumnPredicates( IEnumerable<ColumnPredicate^>^ columnPredicates ) {
					scanSpec->AddColumnPredicate( columnPredicates );
					return Continue( ScanSpecBuilderWithRowOp );
				}

				virtual IScanSpecBuilderWithCellOp^ WithCells( String^ row, String^ columnFamily, String^ columnQualifier ) {
					scanSpec->AddCell( row, columnFamily, columnQualifier );
					return Continue( ScanSpecBuilderWithCellOp );
				}
				virtual IScanSpecBuilderWithCellOp^ WithCells( Key^ key, ... cli::array<Key^>^ moreKeys ) {
					scanSpec->AddCell( key, moreKeys );
					return Continue( ScanSpecBuilderWithCellOp );
				}
				virtual IScanSpecBuilderWithCellOp^ WithCells( IEnumerable<Key^>^ keys ) {
					scanSpec->AddCell( keys );
					return Continue( ScanSpecBuilderWithCellOp );
				}
				virtual IScanSpecBuilderWithCellOp^ WithCells( CellInterval^ cellInterval, ... cli::array<CellInterval^>^ moreCellIntervals ) {
					scanSpec->AddCellInterval( cellInterval, moreCellIntervals );
					return Continue( ScanSpecBuilderWithCellOp );
				}
				virtual IScanSpecBuilderWithCellOp^ WithCells( IEnumerable<CellInterval^>^ cellIntervals ) {
					scanSpec->AddCellInterval( cellIntervals );
					return Continue( ScanSpecBuilderWithCellOp );
				}
		};

		ref class ScanSpecBuilderColumns sealed : public ScanSpecBuilderBase
																					  , public IScanSpecBuilderColumns {

			public:

				ScanSpecBuilderColumns( ScanSpec^ scanSpec )
				: ScanSpecBuilderBase( scanSpec ) {
				}

				virtual IScanSpecBuilderWhere^ WithColumns( ) {
					return Continue( ScanSpecBuilderWhere );
				}
				virtual IScanSpecBuilderWhere^ WithColumns( String^ column, ... cli::array<String^>^ moreColumns ) {
					scanSpec->AddColumn( column, moreColumns );
					return Continue( ScanSpecBuilderWhere );
				}
				virtual IScanSpecBuilderWhere^ WithColumns( IEnumerable<String^>^ columns ) {
					scanSpec->AddColumn( columns );
					return Continue( ScanSpecBuilderWhere );
				}
		};
	}

	Fluent::IScanSpecBuilderColumns^ ScanSpecBuilder::Create( ) {
		return gcnew Fluent::ScanSpecBuilderColumns( gcnew ScanSpec(false) );
	}

	Fluent::IScanSpecBuilderColumns^ ScanSpecBuilder::CreateOrdered( ) {
		return gcnew Fluent::ScanSpecBuilderColumns( gcnew ScanSpec(true) );
	}

}