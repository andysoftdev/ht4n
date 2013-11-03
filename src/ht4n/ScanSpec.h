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

#pragma once

#ifndef __cplusplus_cli
#error "requires /clr"
#endif

#include "ScannerFlags.h"

namespace ht4c { namespace Common {
	class ScanSpec;
} }

#pragma warning( push )
#pragma warning( disable : 4638 ) // reference to unknown symbol 'Table'

namespace Hypertable {
	using namespace System;
	using namespace System::Collections::Generic;
	using namespace System::Collections::ObjectModel;
	using namespace System::Runtime::InteropServices;
	using namespace ht4c;

	ref class Key;
	ref class RowInterval;
	ref class CellInterval;
	ref class ColumnPredicate;

	/// <summary>
	/// Represents a table scanner specification.
	/// </summary>
	/// <example>
	/// The following example shows how to add columns to the scan specification.
	/// <code>
	/// ScanSpec scanSpec = new ScanSpec()
	///                        .AddColumn("cf1")             // all cells in column family cf1
	///                        .AddColumn("cf2:cq")          // all cells in column family cf2 with column qualifier cq
	///                        .AddColumn("cf3:/abc[0-9]/")  // all cells in cf3 where column qualifier matches the regex
	///                        .AddColumn("cf4:");           // all cells in cf4 where no column qualifier exists
	///                        .AddColumn("cf5:^xyz");       // all cells in cf5 where the column qualifier starts with the prefix specified
	///
	/// using( var scanner = table.CreateScanner(scanSpec) ) {
	///    foreach( cell cell in scanner ) {
	///       // process cell
	///    }
	/// }
	/// </code>
	/// The following example shows how to use scan and filter rows.
	/// <code>
	/// IEnumerable&lt;string&gt; manyRows = ...; // 10% or more rows in table
	/// var scanSpec = new ScanSpec() { ScanAndFilter = true }.AddRows(manyRows);
	///
	/// using( var scanner = table.CreateScanner(scanSpec) ) {
	///    foreach( cell cell in scanner ) {
	///       // process cell
	///    }
	/// }
	/// </code>
	/// The following example shows how to add a row interval.
	/// <code>
	/// var scanSpec = new ScanSpec(new RowInterval("a", "z"));
	///
	/// using( var scanner = table.CreateScanner(scanSpec) ) {
	///    foreach( cell cell in scanner ) {
	///       // process cell
	///    }
	/// }
	/// </code>
	/// The following example shows how to filter rows by a regular expression.
	/// <code>
	/// var scanSpec = new ScanSpec() { RowRegex = "a.*" }.AddColumn("cf");
	///
	/// using( ITablScanner scanner = table.CreateScanner(scanSpec) ) {
	///    foreach( cell cell in scanner ) {
	///       // process cell
	///    }
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="ITable"/>
	[Serializable]
	public ref class ScanSpec sealed {

		public:

			/// <summary>
			/// Initializes a new instance of the ScanSpec class.
			/// </summary>
			ScanSpec( );

			/// <summary>
			/// Initializes a new instance of the ScanSpec class using sorted containers, removes duplicates.
			/// </summary>
			/// <param name="sorted">A value that indicates whether to use sorted collections or not.</param>
			explicit ScanSpec( bool sorted );

			/// <summary>
			/// Initializes a new instance of the ScanSpec class using the row specified.
			/// </summary>
			/// <param name="row">Row to add to the scan specification.</param>
			explicit ScanSpec( String^ row );

			/// <summary>
			/// Initializes a new instance of the ScanSpec class using the key specified.
			/// </summary>
			/// <param name="key">Cell to add to the scan specification.</param>
			/// <remarks>
			/// If the key contains a valid timestamp, the constructor adds 
			/// StartTimestamp = EndTimestamp = key.Timestamp predicate and MaxCell = 1.
			/// </remarks>
			explicit ScanSpec( Key^ key );

			/// <summary>
			/// Initializes a new instance of the ScanSpec class using the column predicate specified.
			/// </summary>
			/// <param name="columnPredicate">Column predicate to add to the scan specification.</param>
			explicit ScanSpec( ColumnPredicate^ columnPredicate );

			/// <summary>
			/// Initializes a new instance of the ScanSpec class using the row interval specified.
			/// </summary>
			/// <param name="rowInterval">Row interval to add to the scan specification.</param>
			explicit ScanSpec( RowInterval^ rowInterval );

			/// <summary>
			/// Initializes a new instance of the ScanSpec class using the cell interval specified.
			/// </summary>
			/// <param name="cellInterval">Cell interval to add to the scan specification.</param>
			explicit ScanSpec( CellInterval^ cellInterval );

			/// <summary>
			/// Gets or sets the maximum number of rows to return in the scan.
			/// </summary>
			/// <remarks>
			/// Set to zero (default) for unlimited numbers of rows.
			/// Applies to each row/cell interval individual.
			/// </remarks>
			property int MaxRows;

			/// <summary>
			/// Gets or sets the maximum number of revisions of each cell to return in the scan.
			/// </summary>
			/// <remarks>Set to zero (default) for unlimited numbers of revisions.</remarks>
			property int MaxVersions;

			/// <summary>
			/// Gets or sets the maximum number of cells to return.
			/// </summary>
			/// <remarks>
			/// Set to zero (default) for unlimited numbers of cells.
			/// Applies to each row/cell interval individual.
			/// </remarks>
			property int MaxCells;

			/// <summary>
			/// Gets or sets the maximum number of cells to return per column family and row.
			/// </summary>
			/// <remarks>Set to zero (default) for unlimited numbers of cells.</remarks>
			property int MaxCellsColumnFamily;

			/// <summary>
			/// Gets or sets the number of rows to be skipped at the beginning of the query.
			/// </summary>
			/// <remarks>
			/// Not valid in combination of cell offset. Applies to each row/cell interval individual.
			/// </remarks>
			property int RowOffset;

			/// <summary>
			/// Gets or sets the number of cells to be skipped at the beginning of the query.
			/// </summary>
			/// <remarks>
			/// Not valid in combination of row offset. Applies to each row/cell interval individual.
			/// </remarks>
			property int CellOffset;

			/// <summary>
			/// Gets or sets a value that indicates whether to return keys only or not.
			/// </summary>
			property bool KeysOnly;

			/// <summary>
			/// Gets or sets a value that indicates whether to add the query result to the query cache if possible or not.
			/// </summary>
			property bool NotUseQueryCache;

			/// <summary>
			/// Gets or sets a value that indicates whether to use scan and filter rows or not.
			/// </summary>
			/// <remarks>
			/// The scan and filter rows option can be used to improve query performance for queries that
			/// select a very large number of individual rows. The default algorithm for fetching a set of
			/// rows is to fetch each row individually, which involves a network roundtrip to a range server
			/// for each row. Supplying the scan and filter rows option tells the system to scan over the data
			/// and filter the requested rows at the range server, which will reduce the number of network
			/// roundtrips required when the number of rows requested is very large.<br/><br/>
			/// Scan and filter should be used if query more than 10% of all rows in a table.
			/// </remarks>
			property bool ScanAndFilter;

			/// <summary>
			/// Gets or sets the start time of the scan.
			/// </summary>
			/// <remarks>If set only cells with timestamp newer or equal will be returned.</remarks>
			property DateTime StartDateTime {
				DateTime get( );
				void set( DateTime value);
			}

			/// <summary>
			/// Gets or sets the start timestamp of the scan, in nanoseconds since 1970-01-01 00:00:00.0 UTC.
			/// </summary>
			/// <remarks>If set only cells with timestamp newer or equal will be returned.</remarks>
			property UInt64 StartTimestamp;

			/// <summary>
			/// Gets or sets the end time of the scan.
			/// </summary>
			/// <remarks>If set only cells with timestamp older or equal will be returned.</remarks>
			property DateTime EndDateTime {
				DateTime get( );
				void set( DateTime value);
			}

			/// <summary>
			/// Gets or sets the end time of the scan, in nanoseconds since 1970-01-01 00:00:00.0 UTC.
			/// </summary>
			/// <remarks>If set only cells with timestamp older or equal will be returned.</remarks>
			property UInt64 EndTimestamp;

			/// <summary>
			/// Gets or sets a regular expression to filter rows by.
			/// </summary>
			property String^ RowRegex;

			/// <summary>
			/// Gets or sets a regular expression to filter cell values by.
			/// </summary>
			property String^ ValueRegex;

			/// <summary>
			/// Gets or sets the maximum time to allow scanner methods to execute before time out, if zero timeout is disabled.
			/// </summary>
			property TimeSpan Timeout;

			/// <summary>
			/// Gets or sets the table scanner flags.
			/// </summary>
			property ScannerFlags Flags;

			/// <summary>
			/// Gets the number of rows.
			/// </summary>
			property int RowCount {
				int get( ) {
					return rows != nullptr ? rows->Count : 0;
				}
			}

			/// <summary>
			/// Gets the number of columns.
			/// </summary>
			property int ColumnCount {
				int get( ) {
					return columns != nullptr ? columns->Count : 0;
				}
			}

			/// <summary>
			/// Gets the number of column predicates.
			/// </summary>
			property int ColumnPredicateCount {
				int get( ) {
					return columnPredicates != nullptr ? columnPredicates->Count : 0;
				}
			}

			/// <summary>
			/// Gets the number of cells.
			/// </summary>
			property int CellCount {
				int get( ) {
					return keys != nullptr ? keys->Count : 0;
				}
			}

			/// <summary>
			/// Gets the number of row intervals.
			/// </summary>
			property int RowIntervalCount {
				int get( ) {
					return rowIntervals != nullptr ? rowIntervals->Count : 0;
				}
			}

			/// <summary>
			/// Gets the number of cell intervals.
			/// </summary>
			property int CellIntervalCount {
				int get( ) {
					return cellIntervals != nullptr ? cellIntervals->Count : 0;
				}
			}

			/// <summary>
			/// Gets the rows.
			/// </summary>
			property ReadOnlyCollection<String^>^ Rows {
				ReadOnlyCollection<String^>^ get( );
			}

			/// <summary>
			/// Gets the columns.
			/// </summary>
			property ReadOnlyCollection<String^>^ Columns {
				ReadOnlyCollection<String^>^ get( );
			}

			/// <summary>
			/// Gets the column predicates.
			/// </summary>
			property ReadOnlyCollection<ColumnPredicate^>^ ColumnPredicates {
				ReadOnlyCollection<ColumnPredicate^>^ get( );
			}

			/// <summary>
			/// Gets the cells.
			/// </summary>
			property ReadOnlyCollection<Key^>^ Cells {
				ReadOnlyCollection<Key^>^ get( );
			}

			/// <summary>
			/// Gets the row intervals.
			/// </summary>
			property ReadOnlyCollection<RowInterval^>^ RowIntervals {
				ReadOnlyCollection<RowInterval^>^ get( );
			}

			/// <summary>
			/// Gets the cell intervals.
			/// </summary>
			property ReadOnlyCollection<CellInterval^>^ CellIntervals {
				ReadOnlyCollection<CellInterval^>^ get( );
			}

			/// <summary>
			/// Adds one or more rows to be returned in the scan.
			/// </summary>
			/// <param name="row">Row to add.</param>
			/// <param name="moreRows">More rows to add.</param>
			/// <returns>This ScanSpec instance.</returns>
			ScanSpec^ AddRow( String^ row, ... cli::array<String^>^ moreRows );

			/// <summary>
			/// Adds a bunch of rows to be returned in the scan.
			/// </summary>
			/// <param name="rows">Rows to add.</param>
			/// <returns>This ScanSpec instance.</returns>
			ScanSpec^ AddRow( IEnumerable<String^>^ rows );

			/// <summary>
			/// Removes a row.
			/// </summary>
			/// <param name="row">Row to remove.</param>
			/// <returns>This ScanSpec instance.</returns>
			ScanSpec^ RemoveRow( String^ row );

			/// <summary>
			/// Checks if the scan specification contains a row.
			/// </summary>
			/// <param name="row">Row to check.</param>
			/// <returns>true if the scan specification contains the row.</returns>
			bool ContainsRow( String^ row );

			/// <summary>
			/// Adds one or more columns to be returned in the scan.
			/// </summary>
			/// <param name="column">Column to add.</param>
			/// <param name="moreColumns">More columns to add.</param>
			/// <returns>This ScanSpec instance.</returns>
			/// <remarks>
			/// column family[:[[^]column qualifier|column qualifier regexp]] defines a column.
			/// </remarks>
			ScanSpec^ AddColumn( String^ column, ... cli::array<String^>^ moreColumns );

			/// <summary>
			/// Adds a bunch of columns to be returned in the scan.
			/// </summary>
			/// <param name="columns">Columns to add.</param>
			/// <returns>This ScanSpec instance.</returns>
			/// <remarks>
			/// column family[:[[^]column qualifier|column qualifier regexp]] defines a column.
			/// </remarks>
			ScanSpec^ AddColumn( IEnumerable<String^>^ columns );

			/// <summary>
			/// Removes a column family.
			/// </summary>
			/// <param name="column">Column family to remove.</param>
			/// <returns>This ScanSpec instance.</returns>
			/// <remarks>
			/// column family[:[[^]column qualifier|column qualifier regexp]] defines a column.
			/// </remarks>
			ScanSpec^ RemoveColumn( String^ column );

			/// <summary>
			/// Adds one or more column predicates to the scan.
			/// </summary>
			/// <param name="columnPredicate">Column predicate to add.</param>
			/// <param name="moreColumnPredicates">More column predicates to add.</param>
			/// <returns>This ScanSpec instance.</returns>
			ScanSpec^ AddColumnPredicate( ColumnPredicate^ columnPredicate, ... cli::array<ColumnPredicate^>^ moreColumnPredicates );

			/// <summary>
			/// Adds a bunch of column predicates to the scan.
			/// </summary>
			/// <param name="columnPredicates">Column predicates to add.</param>
			/// <returns>This ScanSpec instance.</returns>
			ScanSpec^ AddColumnPredicate( IEnumerable<ColumnPredicate^>^ columnPredicates );

			/// <summary>
			/// Removes a column predicate.
			/// </summary>
			/// <param name="columnPredicate">Column predicate to remove.</param>
			/// <returns>This ScanSpec instance.</returns>
			ScanSpec^ RemoveColumnPredicate( ColumnPredicate^ columnPredicate );

			/// <summary>
			/// Adds a cell to be returned in the scan.
			/// </summary>
			/// <param name="row">Row.</param>
			/// <param name="columnFamily">Column family.</param>
			/// <param name="columnQualifier">Column qualifier.</param>
			/// <returns>This ScanSpec instance.</returns>
			ScanSpec^ AddCell( String^ row, String^ columnFamily, String^ columnQualifier );

			/// <summary>
			/// Adds one or more cells to be returned in the scan.
			/// </summary>
			/// <param name="key">Cell key.</param>
			/// <param name="moreKeys">More cell keys.</param>
			/// <returns>This ScanSpec instance.</returns>
			/// <remarks>
			/// The scan specification ignores any valid key's timestamp.
			/// </remarks>
			ScanSpec^ AddCell( Key^ key, ... cli::array<Key^>^ moreKeys );

			/// <summary>
			/// Adds a bunch of cells to be returned in the scan.
			/// </summary>
			/// <param name="keys">Cell keys.</param>
			/// <returns>This ScanSpec instance.</returns>
			/// <remarks>
			/// The scan specification ignores any valid key's timestamp.
			/// </remarks>
			ScanSpec^ AddCell( IEnumerable<Key^>^ keys );

			/// <summary>
			/// Removes a cell.
			/// </summary>
			/// <param name="row">Row.</param>
			/// <param name="columnFamily">Column family.</param>
			/// <param name="columnQualifier">Column qualifier.</param>
			/// <returns>This ScanSpec instance.</returns>
			ScanSpec^ RemoveCell( String^ row, String^ columnFamily, String^ columnQualifier );

			/// <summary>
			/// Removes a cell.
			/// </summary>
			/// <param name="key">Cell key.</param>
			/// <returns>This ScanSpec instance.</returns>
			ScanSpec^ RemoveCell( Key^ key );

			/// <summary>
			/// Adds one or more row intervals to be returned in the scan.
			/// </summary>
			/// <param name="rowInterval">Row interval to add.</param>
			/// <param name="moreRowIntervals">More row intervals to add.</param>
			/// <returns>This ScanSpec instance.</returns>
			ScanSpec^ AddRowInterval( RowInterval^ rowInterval, ... cli::array<RowInterval^>^ moreRowIntervals );

			/// <summary>
			/// Adds a bunch of row intervals to be returned in the scan.
			/// </summary>
			/// <param name="rowIntervals">Row intervals to add.</param>
			/// <returns>This ScanSpec instance.</returns>
			ScanSpec^ AddRowInterval( IEnumerable<RowInterval^>^ rowIntervals );

			/// <summary>
			/// Removes a row interval.
			/// </summary>
			/// <param name="rowInterval">Row interval to remove.</param>
			/// <returns>This ScanSpec instance.</returns>
			ScanSpec^ RemoveRowInterval( RowInterval^ rowInterval );

			/// <summary>
			/// Adds one or more cell intervals to be returned in the scan.
			/// </summary>
			/// <param name="cellInterval">Cell interval to add.</param>
			/// <param name="moreCellIntervals">More cell intervals to add.</param>
			/// <returns>This ScanSpec instance.</returns>
			ScanSpec^ AddCellInterval( CellInterval^ cellInterval, ... cli::array<CellInterval^>^ moreCellIntervals );

			/// <summary>
			/// Adds a bunch of cell intervals to be returned in the scan.
			/// </summary>
			/// <param name="cellIntervals">Cell intervals to add.</param>
			/// <returns>This ScanSpec instance.</returns>
			ScanSpec^ AddCellInterval( IEnumerable<CellInterval^>^ cellIntervals );

			/// <summary>
			/// Removes a cell interval.
			/// </summary>
			/// <param name="cellInterval">Cell interval to remove.</param>
			/// <returns>This ScanSpec instance.</returns>
			ScanSpec^ RemoveCellInterval( CellInterval^ cellInterval );

			/// <summary>
			/// Returns a string that represents the current object.
			/// </summary>
			/// <returns>A string that represents the current object.</returns>
			virtual String^ ToString() override;

			/// <summary>
			/// Returns distinct columns from a sequence of columns.
			/// </summary>
			/// <param name="source">The columns to remove redundant elements from.</param>
			/// <returns>Columns that contains distinct elements from the source sequence.</returns>
			/// <remarks>
			/// column family[:[[^]column qualifier|column qualifier regexp]] defines a column.
			/// </remarks>
			static ISet<String^>^ DistictColumn( IEnumerable<String^>^ source );

			/// <summary>
			/// Returns distinct columns from a sequence of columns.
			/// </summary>
			/// <param name="source">The columns to remove redundant elements from.</param>
			/// <param name="columnFamilies">Receives the column families. This parameter is passed uninitialized.</param>
			/// <returns>Columns that contains distinct elements from the source sequence.</returns>
			/// <remarks>
			/// column family[:[[^]column qualifier|column qualifier regexp]] defines a column.
			/// </remarks>
			static ISet<String^>^ DistictColumn( IEnumerable<String^>^ source, [Out] ISet<String^>^% columnFamilies );

		internal:

			void To( Common::ScanSpec& scanSpec );

		private:

			generic< typename T > inline
			ICollection<T>^ CreateCollection( ) {
				return  isSorted
							? static_cast<ICollection<T>^>(gcnew SortedSet<T>())
							: static_cast<ICollection<T>^>(gcnew List<T>());
			}

			generic< typename T > inline
			static ReadOnlyCollection<T>^ AsReadOnly( ICollection<T>^ collection ) {
				List<T>^ list = dynamic_cast<List<T>^>( collection );
				if( list == nullptr ) {
					list = gcnew List<T>( collection );
				}
				return list->AsReadOnly();
			}

			bool isSorted;
			ICollection<String^>^ rows;
			ISet<String^>^ columns;
			ISet<ColumnPredicate^>^ columnPredicates;
			ICollection<Key^>^ keys;
			ICollection<RowInterval^>^ rowIntervals;
			ICollection<CellInterval^>^ cellIntervals;

			static System::DateTime timestampOrigin = System::DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind::Utc );
	};

}

#pragma warning( pop )