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

#include "TableMutator.h"

namespace ht4c { namespace Common {
	class Cells;
} }

namespace Hypertable {
	using namespace System;
	using namespace ht4c;

	/// <summary>
	/// Represents a chunked table mutator.
	/// </summary>
	/// <seealso cref="ITableMutator"/>
	ref class ChunkedTableMutator sealed : public TableMutator {

		public:

			/// <summary>
			/// Clean up all managed and unmanaged resources.
			/// </summary>
			virtual ~ChunkedTableMutator( );

			/// <summary>
			/// Clean up all unmanaged resources.
			/// </summary>
			!ChunkedTableMutator( );

			#pragma region ITableMutator methods

			virtual void Set( Key^ key, cli::array<Byte>^ value, bool createRowKey ) override;
			virtual void Set( Cell^ cell, bool createRowKey ) override;
			virtual void Set( IEnumerable<Cell^>^ cells, bool createRowKey ) override;
			virtual void Delete( String^ row ) override;
			virtual void Delete( Key^ key ) override;
			virtual void Delete( IEnumerable<Key^>^ keys ) override;
			virtual void Delete( IEnumerable<Cell^>^ cells ) override;
			virtual void Flush() override;

			#pragma endregion

		internal:

			ChunkedTableMutator( Common::TableMutator* tableMutator, UInt32 maxChunkSize, UInt32 maxCellCount, bool flushEachChunk );

		private:

			bool SetChunk( bool force );

			Common::Cells* cellChunk;
			UInt32 lenTotal;

			const UInt32 maxChunkSize;
			const UInt32 maxCellCount;
			const bool flushEachChunk;
	};

}