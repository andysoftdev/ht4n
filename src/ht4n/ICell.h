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

#pragma once

#ifndef __cplusplus_cli
#error "requires /clr"
#endif

namespace Hypertable {
	using namespace System;

	ref class Key;

	/// <summary>
	/// Represents a Hypertable cell.
	/// </summary>
	public interface class ICell {

		public:

			/// <summary>
			/// Gets or sets the cell key.
			/// </summary>
			/// <seealso cref="Key"/>
			property Key^ Key {
				Hypertable::Key^ get();
			}

			/// <summary>
			/// Gets or sets the cell value, might be null.
			/// </summary>
			property cli::array<Byte>^ Value {
				cli::array<Byte>^ get();
			}

			/// <summary>
			/// Gets the cell value length.
			/// </summary>
			property int ValueLength {
				int get();
			}
	};

}