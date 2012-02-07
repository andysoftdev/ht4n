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

	Client::~Client( ) {
		disposed = true;
		this->!Client();
		GC::SuppressFinalize(this);
	}

	Client::!Client( ) {
		HT4N_TRY {
			if( client ) {
				delete client;
				client = 0;
			}
		}
		HT4N_RETHROW
	}

	Client::Client( Context^ _ctx )
	: client( 0 )
	, ctx( _ctx )
	, disposed( false )
	{
		if( ctx == nullptr ) throw gcnew ArgumentNullException( L"_ctx" );
		HT4N_TRY {
			client = ctx->get()->createClient();
		}
		HT4N_RETHROW
	}

	void Client::CreateNamespace( String^ name ) {
		return CreateNamespace( name, nullptr, CreateDispositions::None );
	}

	void Client::CreateNamespace( String^ name, CreateDispositions dispo ) {
		return CreateNamespace( name, nullptr, dispo );
	}

	void Client::CreateNamespace( String^ name, INamespace^ nsBase ) {
		return CreateNamespace( name, nsBase, CreateDispositions::None );
	}

	void Client::CreateNamespace( String^ name, INamespace^ _nsBase, CreateDispositions dispo ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentNullException( L"name" );
		Namespace^ nsBase = dynamic_cast<Namespace^>( _nsBase );
		if( nsBase == nullptr && _nsBase != nullptr ) {
			throw gcnew ArgumentException( String::Format(CultureInfo::InvariantCulture, L"Base namespace type, {0} expected", GetType()), L"nsBase" );
		}

		HT4N_TRY {
			client->createNamespace( CM2A(name)
														 , nsBase != nullptr ? nsBase->get() : 0
														 , (dispo & CreateDispositions::CreateIntermediate) == CreateDispositions::CreateIntermediate
														 , (dispo & CreateDispositions::CreateIfNotExist) == CreateDispositions::CreateIfNotExist );
		}
		HT4N_RETHROW
	}

	INamespace^ Client::OpenNamespace( String^ name ) {
		return OpenNamespace( name, nullptr, OpenDispositions::OpenExisting );
	}

	INamespace^ Client::OpenNamespace( String^ name, OpenDispositions dispo ) {
		return OpenNamespace( name, nullptr, dispo );
	}

	INamespace^ Client::OpenNamespace( String^ name, INamespace^ nsBase ) {
		return OpenNamespace( name, nsBase, OpenDispositions::OpenExisting );
	}

	INamespace^ Client::OpenNamespace( String^ name, INamespace^ _nsBase, OpenDispositions dispo ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( name == nullptr ) throw gcnew ArgumentNullException( L"name" );
		Namespace^ nsBase = dynamic_cast<Namespace^>( _nsBase );
		if( nsBase == nullptr && _nsBase != nullptr ) {
			throw gcnew ArgumentException( String::Format(CultureInfo::InvariantCulture, L"Base namespace type, {0} expected", GetType()), L"nsBase" );
		}

		HT4N_TRY {
			switch( dispo & (OpenDispositions::OpenExisting|OpenDispositions::OpenAlways|OpenDispositions::CreateAlways) ) {
			case OpenDispositions::OpenAlways:
				if( !NamespaceExists(name, nsBase) ) {
					CreateNamespace( name, nsBase, (dispo & OpenDispositions::CreateIntermediate) == OpenDispositions::CreateIntermediate ? CreateDispositions::CreateIntermediate : CreateDispositions::None );
				}
				break;
			case OpenDispositions::CreateAlways:
				if( NamespaceExists(name, nsBase) ) {
					INamespace^ ns = gcnew Namespace( this, client->openNamespace(CM2A(name), nsBase != nullptr ? nsBase->get() : 0) );
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
		HT4N_RETHROW
	}

	void Client::DropNamespace( String^ name ) {
		DropNamespace( name, nullptr, DropDispositions::None );
	}

	void Client::DropNamespace( String^ name, DropDispositions dispo ) {
		DropNamespace( name, nullptr, dispo );
	}

	void Client::DropNamespace( String^ name, INamespace^ nsBase ) {
		DropNamespace( name, nsBase, DropDispositions::None );
	}

	void Client::DropNamespace( String^ name, INamespace^ _nsBase, DropDispositions dispo ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentNullException( L"name" );
		if( (_nsBase == nullptr || String::IsNullOrEmpty(_nsBase->Name)) && name == "/" ) throw gcnew ArgumentException( L"Cannot drop root namespace", L"name" );
		Namespace^ nsBase = dynamic_cast<Namespace^>( _nsBase );
		if( nsBase == nullptr && _nsBase != nullptr ) {
			throw gcnew ArgumentException( String::Format(CultureInfo::InvariantCulture, L"Base namespace type, {0} expected", GetType()), L"nsBase" );
		}

		HT4N_TRY {
			client->dropNamespace( CM2A(name)
								 , nsBase != nullptr ? nsBase->get() : 0
								 , (dispo & DropDispositions::IfExists) == DropDispositions::IfExists
								 , (dispo & DropDispositions::IncludeTables) == DropDispositions::IncludeTables
								 , (dispo & DropDispositions::Deep) == DropDispositions::Deep );
		}
		HT4N_RETHROW
	}

	bool Client::NamespaceExists( String^ name ) {
		return NamespaceExists( name, nullptr );
	}

	bool Client::NamespaceExists( String^ name, INamespace^ _nsBase ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentNullException( L"name" );
		Namespace^ nsBase = dynamic_cast<Namespace^>( _nsBase );
		if( nsBase == nullptr && _nsBase != nullptr ) {
			throw gcnew ArgumentException( String::Format(CultureInfo::InvariantCulture, L"Base namespace type, {0} expected", GetType()), L"nsBase" );
		}

		HT4N_TRY {
			return client->existsNamespace( CM2A(name), nsBase != nullptr ? nsBase->get() : 0 );
		}
		HT4N_RETHROW
	}

}