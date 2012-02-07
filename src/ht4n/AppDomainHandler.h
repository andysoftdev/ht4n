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

#include <map>

namespace Hypertable {
	using namespace System;

	/// <summary>
	/// Associates a type T with the current application domain id.
	/// </summary>
	template< typename T >
	class AppDomainHandler {

		public:

			/// <summary>
			/// Associative container type.
			/// </summary>
			typedef std::map<int, T*> map_t;

			/// <summary>
			/// Adds a instance of type T into the associative container using the current application domain id.
			/// </summary>
			/// <param name="t">Instance of type T to add.</param>
			/// <remarks>Instance t must be created using the new operator.</remarks>
			static bool add( T* t ) {
				if( !appDomains ) {
					appDomains = new map_t();
				}
				int id = AppDomain::CurrentDomain->Id;
				map_t::iterator it = appDomains->find( id );
				if( it == appDomains->end() ) {
					appDomains->insert( map_t::value_type(id, t) );
					return true;
				}
				else if( t ) {
					delete t;
				}
				return false;
			}

			/// <summary>
			/// Removes the current application domain from the associative container.
			/// </summary>
			/// <remarks>Calls T::finalize if the last application domain has been removed.</remarks>
			static bool remove( ) {
				if( appDomains ) {
					map_t::iterator it = appDomains->find( AppDomain::CurrentDomain->Id );
					if( it != appDomains->end() ) {
						if( (*it).second ) {
							delete (*it).second;
						}
						appDomains->erase( it );
						if( appDomains->empty() ) {
							delete appDomains;
							appDomains = 0;
							T::finalize();
						}
						return true;
					}
				}
				return false;
			}

			/// <summary>
			/// Returns true if the associative container contains the current application domain.
			/// </summary>
			static bool contains( ) {
				if( appDomains ) {
					map_t::iterator it = appDomains->find( AppDomain::CurrentDomain->Id );
					if( it != appDomains->end() ) {
						return true;
					}
				}
				return false;
			}

		protected:

			AppDomainHandler( ) { }

		private:

			AppDomainHandler( const AppDomainHandler& );
			AppDomainHandler& operator= ( const AppDomainHandler& );

			static map_t* appDomains;
	};
}