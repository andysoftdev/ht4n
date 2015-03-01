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

#include <map>

namespace Hypertable {
	using namespace System;

	/// <summary>
	/// Invokes an R function(T) across application domains.
	/// </summary>
	template< typename F, typename T, typename R >
	class CrossAppDomainFunc {

		public:

			/// <summary>
			/// Interface class represents an function invoker.
			/// </summary>
			class Invoker {

				public:

					/// <summary>
					/// Function type.
					/// </summary>
					typedef typename F func_t;

					/// <summary>
					/// Invokes an R function(T).
					/// </summary>
					/// <param name="func">Function to inkoke.</param>
					/// <param name="t">Function parameter.</param>
					/// <returns>Function result.</returns>
					virtual R invoke( F func, T t ) = 0;
			};

			/// <summary>
			/// Initializes a new instance of the CrossAppDomainFunc class.
			/// </summary>
			/// <param name="_invoker">Invoker.</param>
			/// <param name="_func">Function to invoke.</param>
			CrossAppDomainFunc( Invoker* _invoker, F _func )
			: appDomain( AppDomain::CurrentDomain->Id )
			, func( _func )
			, invoker( _invoker )
			{
				if( !_invoker ) throw gcnew ArgumentNullException( L"_invoker" );
			}

			/// <summary>
			/// Invokes the R function(T).
			/// </summary>
			/// <param name="t">Function parameter.</param>
			R invoke( T t ) {
				if( appDomain == AppDomain::CurrentDomain->Id ) {
					return invoker->invoke( func, t );
				}
				else {
					return msclr::call_in_appdomain<R, CrossAppDomainFunc*, T>( appDomain, &invoke_in_appdomain, this, t );
				}
			}

		private:

			typedef typename F func_t;

			static R invoke_in_appdomain( CrossAppDomainFunc* crossAppDomainFunc, T t ) {
				return crossAppDomainFunc->invoke_cb( t );
			}

			R invoke_cb( T t ) {
				return invoker->invoke( func, t );
			}

			CrossAppDomainFunc( const CrossAppDomainFunc& );
			CrossAppDomainFunc& operator= ( const CrossAppDomainFunc& );

			uint32_t appDomain;
			Invoker* invoker;
			gcroot< F > func;
	};

}