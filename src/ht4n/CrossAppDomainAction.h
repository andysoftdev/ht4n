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

#include <map>

namespace Hypertable {
	using namespace System;

	/// <summary>
	/// Invokes an action(T) across application domains.
	/// </summary>
	template< typename A, typename T >
	class CrossAppDomainAction {

		public:

			/// <summary>
			/// Action type.
			/// </summary>
			typedef typename A action_t;

			/// <summary>
			/// Interface class represents an action invoker.
			/// </summary>
			class Invoker {

				public:

					/// <summary>
					/// Invokes an action(T).
					/// </summary>
					/// <param name="action">Action to inkoke.</param>
					/// <param name="t">Action parameter.</param>
					virtual void invoke( A action, T t ) = 0;
			};

			/// <summary>
			/// Initializes a new instance of the CrossAppDomainAction class.
			/// </summary>
			/// <param name="_invoker">Invoker.</param>
			/// <param name="_action">Action to invoke.</param>
			CrossAppDomainAction( Invoker* _invoker, A _action )
			: appDomain( AppDomain::CurrentDomain->Id )
			, action( _action )
			, invoker( _invoker )
			{
				if( !_invoker ) throw gcnew ArgumentNullException( L"_invoker" );
			}

			/// <summary>
			/// Invokes the action(T).
			/// </summary>
			/// <param name="t">Action parameter.</param>
			void invoke( T t ) {
				if( appDomain == AppDomain::CurrentDomain->Id ) {
					invoker->invoke( action, t );
				}
				else {
					msclr::call_in_appdomain<CrossAppDomainAction*, T>( appDomain, &invoke_in_appdomain, this, t );
				}
			}

		private:

			typedef typename A action_t;

			static void invoke_in_appdomain( CrossAppDomainAction* crossAppDomainAction, T t ) {
				crossAppDomainAction->invoke_cb( t );
			}

			void invoke_cb( T t ) {
				invoker->invoke( action, t );
			}

			CrossAppDomainAction( const CrossAppDomainAction& );
			CrossAppDomainAction& operator= ( const CrossAppDomainAction& );

			int appDomain;
			Invoker* invoker;
			gcroot< A > action;
	};
}