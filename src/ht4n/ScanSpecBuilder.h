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

#include "ScannerFlags.h"

namespace Hypertable {
	using namespace System;
	using namespace System::Collections::Generic;

	ref class Key;
	ref class ScanSpec;
	ref class ScanSpecBuilder;
	ref class RowInterval;
	ref class ColumnPredicate;
	ref class CellInterval;

	namespace Fluent {

		interface class IScanSpecBuilderWithCellOp;
		interface class IScanSpecBuilderWithColumnPredicateOp;
		interface class IScanSpecBuilderWithRowOp;

		/// <summary>
		/// The scan specification common options and limits builder.
		/// </summary>
		/// <seealso cref="ScanSpecBuilder"/>
		public interface class IScanSpecBuilderOp {

			public:

				/// <summary>
				/// Sets the maximum number of rows to return in the scan.
				/// </summary>
				/// <param name="value">Maximum number of rows to return in the scan.</param>
				/// <returns>The scan specification common options and limits builder.</returns>
				/// <remarks>
				/// Set to zero (default) for unlimited numbers of rows.
				/// Applies to each row/cell interval individual.
				/// </remarks>
				IScanSpecBuilderOp^ MaxRows( int value );

				/// <summary>
				/// Sets the maximum number of revisions of each cell to return in the scan.
				/// </summary>
				/// <param name="value">Maximum number of revisions of each cell to return in the scan.</param>
				/// <returns>The scan specification common options and limits builder.</returns>
				/// <remarks>Set to zero (default) for unlimited numbers of revisions.</remarks>
				IScanSpecBuilderOp^ MaxVersions( int value );

				/// <summary>
				/// Sets the maximum number of cells to return.
				/// </summary>
				/// <param name="value">Maximum number of cells to return.</param>
				/// <returns>The scan specification common options and limits builder.</returns>
				/// <remarks>
				/// Set to zero (default) for unlimited numbers of cells.
				/// Applies to each row/cell interval individual.
				/// </remarks>
				IScanSpecBuilderOp^ MaxCells( int value );

				/// <summary>
				/// Gets or sets the maximum number of cells to return per column family and row.
				/// </summary>
				/// <param name="value">Maximum number of cells to return per column family and row.</param>
				/// <returns>The scan specification common options and limits builder.</returns>
				/// <remarks>Set to zero (default) for unlimited numbers of cells.</remarks>
				IScanSpecBuilderOp^ MaxCellsPerColumnFamily( int value );

				/// <summary>
				/// Sets the number of rows to be skipped at the beginning of the query.
				/// </summary>
				/// <param name="value">Number of rows to be skipped at the beginning of the query.</param>
				/// <returns>The scan specification common options and limits builder.</returns>
				/// <remarks>
				/// Not valid in combination of cell offset. Applies to each row/cell interval individual.
				/// </remarks>
				IScanSpecBuilderOp^ RowOffset( int value );

				/// <summary>
				/// Sets the number of cells to be skipped at the beginning of the query.
				/// </summary>
				/// <param name="value">Number of cells to be skipped at the beginning of the query.</param>
				/// <returns>The scan specification common options and limits builder.</returns>
				/// <remarks>
				/// Not valid in combination of row offset. Applies to each row/cell interval individual.
				/// </remarks>
				IScanSpecBuilderOp^ CellOffset( int value );

				/// <summary>
				/// Sets the start time of the scan.
				/// </summary>
				/// <param name="value">Start time of the scan.</param>
				/// <returns>The scan specification common options and limits builder.</returns>
				/// <remarks>If set only cells with timestamp newer or equal will be returned.</remarks>
				IScanSpecBuilderOp^ StartDateTime( DateTime value );

				/// <summary>
				/// Sets the end time of the scan.
				/// </summary>
				/// <param name="value">End time of the scan.</param>
				/// <returns>The scan specification common options and limits builder.</returns>
				/// <remarks>If set only cells with timestamp older or equal will be returned.</remarks>
				IScanSpecBuilderOp^ EndDateTime( DateTime value );

				/// <summary>
				/// Sets a regular expression to filter rows by.
				/// </summary>
				/// <param name="value">Regular expression to filter rows by.</param>
				/// <returns>The scan specification common options and limits builder.</returns>
				IScanSpecBuilderOp^ RowRegex( String^ value );

				/// <summary>
				/// Sets a regular expression to filter cell values by.
				/// </summary>
				/// <param name="value">Regular expression to filter cell values by.</param>
				/// <returns>The scan specification common options and limits builder.</returns>
				IScanSpecBuilderOp^ ValueRegex( String^ value );

				/// <summary>
				/// Use the scan and filter rows option.
				/// </summary>
				/// <returns>The scan specification common options and limits builder.</returns>
				/// <remarks>
				/// The scan and filter rows option can be used to improve query performance for queries that
				/// select a very large number of individual rows. The default algorithm for fetching a set of
				/// rows is to fetch each row individually, which involves a network roundtrip to a range server
				/// for each row. Supplying the scan and filter rows option tells the system to scan over the data
				/// and filter the requested rows at the range server, which will reduce the number of network
				/// roundtrips required when the number of rows requested is very large.<br/><br/>
				/// Scan and filter should be used if query more than 10% of all rows in a table.
				/// </remarks>
				IScanSpecBuilderOp^ ScanAndFilter( );

				/// <summary>
				/// AND together all the column predicates, if not set then the OR logic will be used.
				/// </summary>
				IScanSpecBuilderOp^ ColumnPredicateAnd( );

				/// <summary>
				/// Return keys only.
				/// </summary>
				/// <returns>The scan specification common options and limits builder.</returns>
				IScanSpecBuilderOp^ KeysOnly( );

				/// <summary>
				/// Do not use the query cache.
				/// </summary>
				/// <returns>The scan specification common options and limits builder.</returns>
				IScanSpecBuilderOp^ NotUseQueryCache( );

				/// <summary>
				/// Sets the maximum time to allow scanner methods to execute before time out, if zero timeout is disabled.
				/// </summary>
				/// <param name="value">Maximum time to allow scanner methods to execute before time out, if zero timeout is disabled.</param>
				/// <returns>The scan specification common options and limits builder.</returns>
				IScanSpecBuilderOp^ Timeout( TimeSpan value );

				/// <summary>
				/// Sets the table scanner flags.
				/// </summary>
				/// <param name="value">Table scanner flags.</param>
				/// <returns>The scan specification common options and limits builder.</returns>
				IScanSpecBuilderOp^ Flags( ScannerFlags value );

				/// <summary>
				/// Returns the final scan specification.
				/// </summary>
				/// <returns>The final scan specification.</returns>
				ScanSpec^ Build( );
		};

		/// <summary>
		/// The scan specification row predicate builder.
		/// </summary>
		/// <seealso cref="ScanSpecBuilder"/>
		public interface class IScanSpecBuilderWithRow {

			public:

				/// <summary>
				/// All rows in the scan.
				/// </summary>
				/// <returns>The scan specification row predicate builder, including common options and limits.</returns>
				IScanSpecBuilderWithRowOp^ WithRows( );

				/// <summary>
				/// Adds one or more rows to be returned in the scan.
				/// </summary>
				/// <param name="row">Row to add.</param>
				/// <param name="moreRows">More rows to add.</param>
				/// <returns>The scan specification row predicate builder, including common options and limits.</returns>
				IScanSpecBuilderWithRowOp^ WithRows( String^ row, ... cli::array<String^>^ moreRows );

				/// <summary>
				/// Adds a bunch of rows to be returned in the scan.
				/// </summary>
				/// <param name="rows">Rows to add.</param>
				/// <returns>The scan specification row predicate builder, including common options and limits.</returns>
				IScanSpecBuilderWithRowOp^ WithRows( IEnumerable<String^>^ rows );

				/// <summary>
				/// Adds one or more row intervals to be returned in the scan.
				/// </summary>
				/// <param name="rowInterval">Row interval to add.</param>
				/// <param name="moreRowIntervals">More row intervals to add.</param>
				/// <returns>The scan specification row predicate builder, including common options and limits.</returns>
				IScanSpecBuilderWithRowOp^ WithRows( RowInterval^ rowInterval, ... cli::array<RowInterval^>^ moreRowIntervals );

				/// <summary>
				/// Adds a bunch of row intervals to be returned in the scan.
				/// </summary>
				/// <param name="rowIntervals">Row intervals to add.</param>
				/// <returns>The scan specification row predicate builder, including common options and limits.</returns>
				IScanSpecBuilderWithRowOp^ WithRows( IEnumerable<RowInterval^>^ rowIntervals );

				/// <summary>
				/// Adds one or more column predicates to the scan.
				/// </summary>
				/// <param name="columnPredicate">Column predicate to add.</param>
				/// <param name="moreColumnPredicates">More column predicates to add.</param>
				/// <returns>The scan specification column predicate builder, including common options and limits.</returns>
				IScanSpecBuilderWithRowOp^ WithColumnPredicates( ColumnPredicate^ columnPredicate, ... cli::array<ColumnPredicate^>^ moreColumnPredicates );

				/// <summary>
				/// Adds a bunch of column predicates to the scan.
				/// </summary>
				/// <param name="columnPredicates">Column predicates to add.</param>
				/// <returns>The scan specification column predicate builder, including common options and limits.</returns>
				IScanSpecBuilderWithRowOp^ WithColumnPredicates( IEnumerable<ColumnPredicate^>^ columnPredicates );
		};

		/// <summary>
		/// The scan specification row predicate builder, including common options and limits.
		/// </summary>
		/// <seealso cref="ScanSpecBuilder"/>
		public interface class IScanSpecBuilderWithRowOp : public IScanSpecBuilderWithRow
																										 , public IScanSpecBuilderOp {
		};

		/// <summary>
		/// The scan specification cell predicate builder.
		/// </summary>
		/// <seealso cref="ScanSpecBuilder"/>
		public interface class IScanSpecBuilderWithCell {

			public:

				/// <summary>
				/// Adds a cell to be returned in the scan.
				/// </summary>
				/// <param name="row">Row.</param>
				/// <param name="columnFamily">Column family.</param>
				/// <param name="columnQualifier">Column qualifier.</param>
				/// <returns>The scan specification cell predicate builder, including common options and limits.</returns>
				IScanSpecBuilderWithCellOp^ WithCells( String^ row, String^ columnFamily, String^ columnQualifier );

				/// <summary>
				/// Adds one or more cella to be returned in the scan.
				/// </summary>
				/// <param name="key">Cell key.</param>
				/// <param name="moreKeys">More cell keys.</param>
				/// <returns>The scan specification cell predicate builder, including common options and limits.</returns>
				/// <remarks>
				/// The scan spec builder ignores any valid key's timestamp.
				/// </remarks>
				IScanSpecBuilderWithCellOp^ WithCells( Key^ key, ... cli::array<Key^>^ moreKeys );

				/// <summary>
				/// Adds a bunch of cells to be returned in the scan.
				/// </summary>
				/// <param name="keys">Cell keys.</param>
				/// <returns>The scan specification cell predicate builder, including common options and limits.</returns>
				/// <remarks>
				/// The scan spec builder ignores any valid key's timestamp.
				/// </remarks>
				IScanSpecBuilderWithCellOp^ WithCells( IEnumerable<Key^>^ keys );

				/// <summary>
				/// Adds one or more cell intervals to be returned in the scan.
				/// </summary>
				/// <param name="cellInterval">Cell interval to add.</param>
				/// <param name="moreCellIntervals">More cell intervals to add.</param>
				/// <returns>The scan specification cell predicate builder, including common options and limits.</returns>
				IScanSpecBuilderWithCellOp^ WithCells( CellInterval^ cellInterval, ... cli::array<CellInterval^>^ moreCellIntervals );

				/// <summary>
				/// Adds a bunch of cell intervals to be returned in the scan.
				/// </summary>
				/// <param name="cellIntervals">Cell intervals to add.</param>
				/// <returns>The scan specification cell predicate builder, including common options and limits.</returns>
				IScanSpecBuilderWithCellOp^ WithCells( IEnumerable<CellInterval^>^ cellIntervals );
		};

		/// <summary>
		/// The scan specification cell predicate builder, including common options and limits.
		/// </summary>
		/// <seealso cref="ScanSpecBuilder"/>
		public interface class IScanSpecBuilderWithCellOp : public IScanSpecBuilderWithCell
																											, public IScanSpecBuilderOp {
		};

		/// <summary>
		/// The scan specification where clause builder.
		/// </summary>
		/// <seealso cref="ScanSpecBuilder"/>
		public interface class IScanSpecBuilderWhere : public IScanSpecBuilderWithRow
																								 , public IScanSpecBuilderWithCell {

		};

		/// <summary>
		/// The scan specification column builder.
		/// </summary>
		/// <seealso cref="ScanSpecBuilder"/>
		public interface class IScanSpecBuilderColumns {

			public:

				/// <summary>
				/// Adds all column to be returned in the scan.
				/// </summary>
				/// <returns>The scan specification where clause builder.</returns>
				/// <remarks>
				/// Corresponds to SELECT * FROM ...
				/// </remarks>
				IScanSpecBuilderWhere^ WithColumns( );

				/// <summary>
				/// Adds one or more columns to be returned in the scan.
				/// </summary>
				/// <param name="column">Column to add.</param>
				/// <param name="moreColumns">More columns to add.</param>
				/// <returns>The scan specification where clause builder.</returns>
				/// <remarks>
				/// column family[:[[^]column qualifier|column qualifier regexp]] defines a column.
				/// </remarks>
				IScanSpecBuilderWhere^ WithColumns( String^ column, ... cli::array<String^>^ moreColumns );

				/// <summary>
				/// Adds one or more columns to be returned in the scan.
				/// </summary>
				/// <param name="columns">Columns to add.</param>
				/// <returns>The scan specification where clause builder.</returns>
				/// <remarks>
				/// column family[:[[^]column qualifier|column qualifier regexp]] defines a column.
				/// </remarks>
				IScanSpecBuilderWhere^ WithColumns( IEnumerable<String^>^ columns );
		};

	}

	/// <summary>
	/// Provides a simple way to create a table scanner specification.
	/// </summary>
	/// <example>
	/// The following example creates a scan specification for SELECT * FROM &lt;table&gt; WHERE ROW="r1" OR ROW="r2" OR ROW="xyz"
	/// <code>
	/// ScanSpec scanSpec = ScanSpecBuilder
	///    .Create()
	///       .WithColumns()
	///       .WithRows("r1", "r2", "xyz")
	///    .Build();
	/// </code>
	/// or with the column family specified, like SELECT cf1, cf2 FROM &lt;table&gt; WHERE ROW="r1" OR ROW="r2" OR ROW="xyz".
	/// <code>
	/// ScanSpec scanSpec = ScanSpecBuilder
	///    .Create()
	///       .WithColumns("cf1", "cf")
	///       .WithRows("r1", "r2", "xyz")
	///    .Build();
	/// </code>
	/// </example>
	/// <seealso cref="ScanSpec"/>
	public ref class ScanSpecBuilder abstract sealed {

		public:

			/// <summary>
			/// Initializes a new scan specification.
			/// </summary>
			/// <returns>The scan specification column builder.</returns>
			static Fluent::IScanSpecBuilderColumns^ Create( );

			/// <summary>
			/// Initializes a new ordered scan specification.
			/// </summary>
			/// <returns>The scan specification column builder.</returns>
			static Fluent::IScanSpecBuilderColumns^ CreateOrdered( );
	};

}