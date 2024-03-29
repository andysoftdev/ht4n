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

#include "IClient.h"

namespace ht4c { namespace Common {
	class Client;
} }

namespace Hypertable {
	using namespace System;
	using namespace ht4c;

	interface class INamespace;

	ref class Context;
	ref class NamespaceListing;

	/// <summary>
	/// Represents a Hypertable client, provides methods to handle Hypertable namespaces.
	/// </summary>
	/// <seealso cref="IClient"/>
	ref class Client sealed : public IClient {

		public:

			/// <summary>
			/// Clean up all managed and unmanaged resources.
			/// </summary>
			virtual ~Client( );

			/// <summary>
			/// Clean up all unmanaged resources.
			/// </summary>
			!Client( );

			#pragma region IClient properties

			property IContext^ Context {
				virtual IContext^ get( );
			}

			property bool IsDisposed {
				virtual bool get( ) {
					return disposed;
				}
			}

			#pragma endregion

			#pragma region IClient methods

			virtual void CreateNamespace( String^ name );
			virtual void CreateNamespace( String^ name, CreateDispositions dispo );
			virtual void CreateNamespace( String^ name, INamespace^ nsBase );
			virtual void CreateNamespace( String^ name, INamespace^ nsBase, CreateDispositions dispo );
			virtual INamespace^ OpenNamespace( String^ name );
			virtual INamespace^ OpenNamespace( String^ name, OpenDispositions dispo );
			virtual INamespace^ OpenNamespace( String^ name, INamespace^ nsBase );
			virtual INamespace^ OpenNamespace( String^ name, INamespace^ nsBase, OpenDispositions dispo );
			virtual void DropNamespace( String^ name );
			virtual void DropNamespace( String^ name, DropDispositions dispo );
			virtual void DropNamespace( String^ name, INamespace^ nsBase );
			virtual void DropNamespace( String^ name, INamespace^ nsBase, DropDispositions dispo );
			virtual bool NamespaceExists( String^ name );
			virtual bool NamespaceExists( String^ name, INamespace^ nsBase );
			virtual void Optimize( );

			#pragma endregion

		internal:

			Client( Hypertable::Context^ _ctx );

			property Object^ SyncRoot { 
				Object^ get() { 
					return syncRoot;
				}
			}

		private:

			Object^ syncRoot;
			Common::Client* client;
			Hypertable::Context^ ctx;
			bool disposed;
	};

}