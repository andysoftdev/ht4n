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

#include "stdafx.h"

#include "Client.h"
#include "Context.h"
#include "Namespace.h"
#include "Exception.h"
#include "CM2A.h"

#include "ht4c.Common/Client.h"
#include "ht4c.Context/Context.h"

namespace Hypertable {
	using namespace System;
	using namespace ht4c;

	Hypertable::ContextKind Client::ContextKind::get() {
		return (Hypertable::ContextKind)client->getContextKind();
	}

	Client::~Client( ) {
		disposed = true;
		this->!Client();
		GC::SuppressFinalize(this);
	}

	Client::!Client( ) {
		HT4C_TRY {
			if( client ) {
				delete client;
				client = 0;
			}
		}
		HT4C_RETHROW
	}

	Client::Client( Context^ _ctx )
	: client( 0 )
	, ctx( _ctx )
	, disposed( false )
	{
		if( ctx == nullptr ) throw gcnew ArgumentNullException( L"_ctx" );
		HT4C_TRY {
			client = ctx->get()->createClient();
		}
		HT4C_RETHROW
	}

	void Client::CreateNamespace( String^ name ) {
		return CreateNamespace( name, nullptr, CreateDispositions::None );
	}

	void Client::CreateNamespace( String^ name, CreateDispositions dispo ) {
		return CreateNamespace( name, nullptr, dispo );
	}

	void Client::CreateNamespace( String^ name, Namespace^ nsBase ) {
		return CreateNamespace( name, nsBase, CreateDispositions::None );
	}

	void Client::CreateNamespace( String^ name, Namespace^ nsBase, CreateDispositions dispo ) {
		if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentNullException( L"name" );
		HT4C_TRY {
			if( (dispo & CreateDispositions::CreateIfNotExist) == CreateDispositions::CreateIfNotExist
				&& client->existsNamespace(CM2A(name), nsBase != nullptr ? nsBase->get() : 0) ) {

				return;
			}
			client->createNamespace( CM2A(name), nsBase != nullptr ? nsBase->get() : 0, (dispo & CreateDispositions::CreateIntermediate) == CreateDispositions::CreateIntermediate );
		}
		HT4C_RETHROW
	}

	Namespace^ Client::OpenNamespace( String^ name ) {
		return OpenNamespace( name, nullptr, OpenDispositions::OpenExisting );
	}

	Namespace^ Client::OpenNamespace( String^ name, OpenDispositions dispo ) {
		return OpenNamespace( name, nullptr, dispo );
	}

	Namespace^ Client::OpenNamespace( String^ name, Namespace^ nsBase ) {
		return OpenNamespace( name, nsBase, OpenDispositions::OpenExisting );
	}

	Namespace^ Client::OpenNamespace( String^ name, Namespace^ nsBase, OpenDispositions dispo ) {
		if( name == nullptr ) throw gcnew ArgumentNullException( L"name" );
		HT4C_TRY {
			switch( dispo & OpenDispositions::Cases ) {
			case OpenDispositions::OpenAlways:
				if( !NamespaceExists(name, nsBase) ) {
					CreateNamespace( name, nsBase, (dispo & OpenDispositions::CreateIntermediate) == OpenDispositions::CreateIntermediate ? CreateDispositions::CreateIntermediate : CreateDispositions::None );
				}
				break;
			case OpenDispositions::CreateAlways:
				if( NamespaceExists(name, nsBase) ) {
					Namespace^ ns = gcnew Namespace( this, client->openNamespace(CM2A(name), nsBase != nullptr ? nsBase->get() : 0) );
					try {
						ns->DropTables();
						ns->DropNamespaces( DropDispositions::Complete );
						return ns;
					}
					catch( Object^ ){
						delete ns;
						throw;
					}
				}
				else {
					CreateNamespace( name, nsBase, (dispo & OpenDispositions::CreateIntermediate) == OpenDispositions::CreateIntermediate ? CreateDispositions::CreateIntermediate : CreateDispositions::None );
				}
				break;
			}
			return gcnew Namespace( this, client->openNamespace(CM2A(name), nsBase != nullptr ? nsBase->get() : 0) );
		}
		HT4C_RETHROW
	}

	void Client::DropNamespace( String^ name ) {
		DropNamespace( name, nullptr, DropDispositions::None );
	}

	void Client::DropNamespace( String^ name, DropDispositions dispo ) {
		DropNamespace( name, nullptr, dispo );
	}

	void Client::DropNamespace( String^ name, Namespace^ nsBase ) {
		DropNamespace( name, nsBase, DropDispositions::None );
	}

	void Client::DropNamespace( String^ name, Namespace^ nsBase, DropDispositions dispo ) {
		if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentNullException( L"name" );
		HT4C_TRY {
			client->dropNamespace( CM2A(name)
								 , nsBase != nullptr ? nsBase->get() : 0
								 , (dispo & DropDispositions::IfExists) == DropDispositions::IfExists
								 , (dispo & DropDispositions::IncludeTables) == DropDispositions::IncludeTables
								 , (dispo & DropDispositions::Deep) == DropDispositions::Deep );
		}
		HT4C_RETHROW
	}

	bool Client::NamespaceExists( String^ name ) {
		return NamespaceExists( name, nullptr );
	}

	bool Client::NamespaceExists( String^ name, Namespace^ nsBase ) {
		if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentNullException( L"name" );
		HT4C_TRY {
			return client->existsNamespace( CM2A(name), nsBase != nullptr ? nsBase->get() : 0 );
		}
		HT4C_RETHROW
	}

}