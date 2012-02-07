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

namespace Hypertable { namespace Composition {
	using namespace System;

	interface class IContextProvider;
	ref class ExportContextProviderAttribute;

	/// <summary>
	/// Represents metadata for the context provider export.
	/// </summary>
	/// <seealso cref="IContextProvider"/>
	/// <seealso cref="ExportContextProviderAttribute"/>
	public interface class IContextProviderMetadata {

		public:

			/// <summary>
			/// Gets the context provider name.
			/// </summary>
			property String^ ProviderName {
				String^ get( );
			}

	};

	} }