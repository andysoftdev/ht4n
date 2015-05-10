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

namespace Hypertable {
	using namespace System;
	using namespace System::ComponentModel;

	/// <summary>
	/// Converts a manages string into a unmanaged utf8 C string.
	/// </summary>
	class CM2U8 {

		public:

			/// <summary>
			/// Initializes a new instance of the CM2U8 class using a managed string.
			/// </summary>
			/// <param name="string">Managed string.</param>
			inline explicit CM2U8( String^ string ) {
				if( string != nullptr ) {
					pin_ptr<const wchar_t> wsz = PtrToStringChars( string );
					cstr = ToUtf8( wsz, string->Length );
				}
				else {
					cstr = 0;
				}
			}

			/// <summary>
			/// Destroys the CM2U8 instance.
			/// </summary>
			inline ~CM2U8( ) {
				if( cstr && cstr != cbuf ) {
					free( cstr );
				}
			}

			/// <summary>
			/// Gets the unmanaged utf8 C string.
			/// </summary>
			/// <returns>Unmanaged utf8 C string.</returns>
			inline operator const char* ( ) const {
				return c_str();
			}

			/// <summary>
			/// Gets the unmanaged utf8 C string.
			/// </summary>
			/// <returns>Unmanaged C string.</returns>
			inline const char* c_str( ) const {
				return cstr;
			}

			/// <summary>
			/// Creates a managed string from an unmanaged utf8 C string.
			/// </summary>
			/// <param name="string">Unmanaged utf8 C string.</param>
			/// <returns>Managed string.</returns>
			static String^ ToString( const char* string ) {
				return ToString( string, strlen(string) );
			}

			/// <summary>
			/// Creates a managed string from an unmanaged utf8 C string.
			/// </summary>
			/// <param name="string">Unmanaged utf8 C string.</param>
			/// <param name="len">The string length.</param>
			/// <returns>Managed string.</returns>
			static String^ ToString( const char* string, int len ) {
				if( len ) {
					wchar_t wbuf[SIZE + 1];
					int cc = len < SIZE ? MultiByteToWideChar(CP_UTF8, 0, string, len, wbuf, SIZE) : 0;
					if( !cc ) {
						cc = MultiByteToWideChar( CP_UTF8, 0, string, len, 0, 0 );
						wchar_t* wsz = static_cast<wchar_t*>( malloc((cc + 1) * sizeof(wchar_t)) );
						if( !wsz ) {
							throw gcnew OutOfMemoryException();
						}
						cc = MultiByteToWideChar( CP_UTF8, 0, string, len, wsz, cc );
						if( !cc ) {
							free( wsz );
							throw gcnew Win32Exception( GetLastError() );
						}

						wsz[cc] = 0;
						String^ s = gcnew String( wsz );
						free( wsz );
						return s;
					}
					wbuf[cc] = 0;
					return gcnew String( wbuf );
				}
				return String::Empty;
			}

		private:

			enum {
				SIZE = 64
			};

			char* ToUtf8( const wchar_t* wsz, int len ) {
				if( len ) {
					int cb = len < SIZE ? WideCharToMultiByte(CP_UTF8, 0, wsz, len, cbuf, SIZE, 0, 0) : 0;
					if( !cb ) {
						cb = WideCharToMultiByte( CP_UTF8, 0, wsz, len, 0, 0, 0, 0 );
						char* sz = static_cast<char*>( malloc(cb + 1) );
						if( !sz ) {
							throw gcnew OutOfMemoryException();
						}
						cb = WideCharToMultiByte( CP_UTF8, 0, wsz, len, sz, cb, 0, 0);
						if( !cb ) {
							free( sz );
							throw gcnew Win32Exception( GetLastError() );
						}
						sz[cb] = 0;
						return sz;
					}
					else {
						cbuf[cb] = 0;
					}
				}
				else {
					*cbuf = 0;
				}
				return cbuf;
			}

			char cbuf[SIZE + 1];
			char* cstr;
	};
}