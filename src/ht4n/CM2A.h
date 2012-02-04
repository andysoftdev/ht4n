/** -*- C++ -*-
 * Copyright (C) 2010-2012 Thalmann Software & Consulting, http://www.softdev.ch
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

#include <msclr/marshal.h>

namespace Hypertable {
	using namespace System;
	using namespace msclr::interop;

	/// <summary>
	/// Converts a manages string into a unmanaged C string.
	/// </summary>
	class CM2A {

		public:

			/// <summary>
			/// Initializes a new instance of the CM2A class using a managed string.
			/// </summary>
			/// <param name="string">Managed string.</param>
			inline explicit CM2A( String^ string ) {
				if( string != nullptr ) {
					ctx = gcnew marshal_context();
					cstr = ctx->marshal_as<const char*>(string);
				}
				else {
					cstr = 0;
				}
			}

			/// <summary>
			/// Destroys the CM2A instance.
			/// </summary>
			inline ~CM2A( ) {
				if( (marshal_context^)ctx != nullptr ) {
					delete (marshal_context^)ctx;
				}
			}

			/// <summary>
			/// Gets the unmanaged C string.
			/// </summary>
			/// <returns>Unmanaged C string.</returns>
			inline operator const char* ( ) const {
				return c_str();
			}

			/// <summary>
			/// Gets the unmanaged C string.
			/// </summary>
			/// <returns>Unmanaged C string.</returns>
			inline const char* c_str( ) const {
				return cstr;
			}

		private:

			const char* cstr;
			gcroot< marshal_context^ > ctx;
	};
}