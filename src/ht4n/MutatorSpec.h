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

#include "MutatorKind.h"
#include "MutatorFlags.h"

#pragma warning( push )
#pragma warning( disable : 4638 ) // reference to unknown symbol 'Table'

namespace Hypertable {
	using namespace System;

	/// <summary>
	/// Represents a table mutator specification.
	/// </summary>
	/// <example>
	/// The following example shows how to create a chunked mutator.
	/// <code>
	/// MutatorSpec mutatorSpec = MutatorSpec.CreateChunked();
	/// mutatorSpec.FlushEachChunk = true;
	/// using( ITableMutator mutator = table.CreateMutator(mutatorSpec) ) {
	///    // do something
	/// }
	/// </code>
	/// The following example shows how to create a queued mutator.
	/// <code>
	/// using( ITableMutator mutator = table.CreateMutator(MutatorSpec.CreateQueued()) ) {
	///    // do something
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="Table"/>
	[Serializable]
	public ref class MutatorSpec sealed {

		public:

			/// <summary>
			/// Gets or sets the mutator kind.
			/// </summary>
			/// <seealso cref="MutatorKind"/>
			property Hypertable::MutatorKind MutatorKind;

			/// <summary>
			/// Gets or sets the maximum time to allow mutator methods to execute before time out, if zero timeout is disabled.
			/// </summary>
			property TimeSpan Timeout;

			/// <summary>
			/// Gets or sets the periodic flush interval, if zero periodic flush is disabled.
			/// </summary>
			/// <remark>The periodic flush interval will be ignored for asynchronous mutators.</remark>
			property TimeSpan FlushInterval;

			/// <summary>
			/// Gets or sets the maximum chunk size in bytes, only for chunked mutator.
			/// </summary>
			/// <remarks>Defaults to 64kB</remarks>
			/// <seealso cref="MutatorKind"/>
			property UInt32 MaxChunkSize;

			/// <summary>
			/// Gets or sets the default value for the maximum chunk size in bytes, only for chunked mutator.
			/// </summary>
			/// <remarks>Defaults to 64kB</remarks>
			/// <seealso cref="MutatorKind"/>
			static property UInt32 MaxChunkSizeDefault { 
				UInt32 get() { return maxChunkSizeDefault; }
				void set(UInt32 value) { maxChunkSizeDefault = value; }
			}

			/// <summary>
			/// Gets or sets the maximum cell count for a chunk, only for chunked mutator.
			/// </summary>
			/// <remarks>Defaults to 4096</remarks>
			/// <seealso cref="MutatorKind"/>
			property UInt32 MaxCellCount;

			/// <summary>
			/// Gets or sets the default maximum cell count for a chunk, only for chunked mutator.
			/// </summary>
			/// <remarks>Defaults to 4096</remarks>
			/// <seealso cref="MutatorKind"/>
			static property UInt32 MaxCellCountDefault { 
				UInt32 get() { return maxCellCountDefault; }
				void set(UInt32 value) { maxCellCountDefault = value; }
			}

			/// <summary>
			/// Gets or sets a value that indicates whether each chunk should be flushed or not, only for chunked mutator.
			/// </summary>
			/// <seealso cref="MutatorKind"/>
			property bool FlushEachChunk;

			/// <summary>
			/// Gets or sets a value that indicates whether to create a queued or synchronous mutator.
			/// </summary>
			property bool Queued;

			/// <summary>
			/// Gets or sets the bounded size of the blocking queue, only for queued mutator.
			/// </summary>
			/// <remarks>Set to zero (default value) for an unbounded blocking queue.</remarks>
			property int Capacity;

			/// <summary>
			/// Gets or sets the table mutator flags.
			/// </summary>
			property MutatorFlags Flags;

			/// <summary>
			/// Initializes a new instance of the MutatorSpec class.
			/// </summary>
			MutatorSpec( );

			/// <summary>
			/// Initializes a new instance of the MutatorSpec class using the specified mutator kind.
			/// </summary>
			/// <param name="mutatorKind">Mutator kind.</param>
			/// <seealso cref="MutatorKind"/>
			MutatorSpec( Hypertable::MutatorKind mutatorKind );

			/// <summary>
			/// Creates a new default MutatorSpec instance.
			/// </summary>
			/// <returns>New MutatorSpec instance.</returns>
			static MutatorSpec^ Create( );

			/// <summary>
			/// Creates a new MutatorSpec instance for creating a queued mutator.
			/// </summary>
			/// <returns>New MutatorSpec instance.</returns>
			static MutatorSpec^ CreateQueued( );

			/// <summary>
			/// Creates a new MutatorSpec instance for creating a chunked mutator.
			/// </summary>
			/// <returns>New MutatorSpec instance.</returns>
			static MutatorSpec^ CreateChunked( );

			/// <summary>
			/// Creates a new MutatorSpec instance for creating a queued chunked mutator.
			/// </summary>
			/// <returns>New MutatorSpec instance.</returns>
			static MutatorSpec^ CreateChunkedQueued( );

		private:

			static UInt32 maxChunkSizeDefault = 64 * 1024;
			static UInt32 maxCellCountDefault =  4 * 1024;
	};

}

#pragma warning( pop )