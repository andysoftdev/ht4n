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

#include "Namespace.h"
#include "NamespaceListing.h"
#include "Context.h"
#include "Table.h"
#include "Cell.h"
#include "Exception.h"
#include "CM2A.h"

#include "ht4c.Common/Namespace.h"
#include "ht4c.Common/Cells.h"

namespace Hypertable {
	using namespace System;
	using namespace ht4c;

	Namespace::~Namespace( ) {
		disposed = true;
		this->!Namespace();
		GC::SuppressFinalize(this);
	}

	Namespace::!Namespace( ) {
		HT4C_TRY {
			if( ns ) {
				delete ns;
				ns = 0;
			}
			client = nullptr;
		}
		HT4C_RETHROW
	}

	String^ Namespace::Name::get( ) {
		HT4C_TRY {
			return gcnew String( ns->getName().c_str() );
		}
		HT4C_RETHROW
	}

	IList<String^>^ Namespace::Namespaces::get( ) {
		HT4C_TRY {
			NamespaceListing^ nsListing = GetListing( false );
			IList<String^>^ namespaces = gcnew List<String^>( nsListing->Namespaces->Count );
			for each( NamespaceListing^ ni in nsListing->Namespaces ) {
				namespaces->Add( ni->Name );
			}
			return namespaces;
		}
		HT4C_RETHROW
	}

	IList<String^>^ Namespace::Tables::get( ) {
		HT4C_TRY {
			NamespaceListing^ nsListing = GetListing( false );
			return nsListing->Tables;
		}
		HT4C_RETHROW
	}

	void Namespace::CreateTable( String^ name, String^ schema ) {
		if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentNullException( L"name" );
		if( String::IsNullOrEmpty(schema) ) throw gcnew ArgumentNullException( L"schema" );
		HT4C_TRY {
			ns->createTable( CM2A(name), CM2A(schema) );
		}
		HT4C_RETHROW
	}

	void Namespace::CreateTable( String^ name, String^ schema, CreateDispositions dispo ) {
		if( (dispo & CreateDispositions::CreateIfNotExist) == CreateDispositions::CreateIfNotExist ) {
			if( TableExists(name) ) {
				return;
			}
		}
		CreateTable( name, schema );
	}

	void Namespace::CreateTableLike( String^ name, String^ like ) {
		if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentNullException( L"name" );
		if( String::IsNullOrEmpty(like) ) throw gcnew ArgumentNullException( L"like" );
		HT4C_TRY {
			ns->createTableLike( CM2A(name), CM2A(like) );
		}
		HT4C_RETHROW
	}

	void Namespace::CreateTableLike( String^ name, String^ like, CreateDispositions dispo ) {
		if( (dispo & CreateDispositions::CreateIfNotExist) == CreateDispositions::CreateIfNotExist ) {
			if( TableExists(name) ) {
				return;
			}
		}
		CreateTableLike( name, like );
	}

	void Namespace::AlterTable( String^ name, String^ schema ) {
		if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentNullException( L"name" );
		if( String::IsNullOrEmpty(schema) ) throw gcnew ArgumentNullException( L"schema" );
		HT4C_TRY {
			ns->alterTable( CM2A(name), CM2A(schema) );
		}
		HT4C_RETHROW
	}

	Table^ Namespace::OpenTable( String^ name ) {
		return OpenTable( name, OpenDispositions::OpenExisting );
	}

	Table^ Namespace::OpenTable( String^ name, OpenDispositions dispo ) {
		if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentNullException( L"name" );
		HT4C_TRY {
			return gcnew Table( ns->openTable(CM2A(name), (dispo & OpenDispositions::Force) == OpenDispositions::Force) );
		}
		HT4C_RETHROW
	}

	Table^ Namespace::OpenTable( String^ name, String^ schema, OpenDispositions dispo ) {
		if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentNullException( L"name" );
		if( String::IsNullOrEmpty(schema) ) throw gcnew ArgumentNullException( L"schema" );
		HT4C_TRY {
			switch( dispo & OpenDispositions::Cases ) {
			case OpenDispositions::OpenAlways:
				if( !TableExists(name) ) {
					CreateTable( name, schema );
				}
				break;
			case OpenDispositions::CreateAlways:
				DropTable( name, DropDispositions::IfExists );
				CreateTable( name, schema );
				break;
			}
			return gcnew Table( ns->openTable(CM2A(name), (dispo & OpenDispositions::Force) == OpenDispositions::Force) );
		}
		HT4C_RETHROW
	}

	void Namespace::RenameTable( String^ nameOld, String^ nameNew ) {
		if( String::IsNullOrEmpty(nameOld) ) throw gcnew ArgumentNullException( L"nameOld" );
		if( String::IsNullOrEmpty(nameNew) ) throw gcnew ArgumentNullException( L"nameNew" );
		HT4C_TRY {
			if( nameOld != nameNew ) {
				ns->renameTable( CM2A(nameOld), CM2A(nameNew) );
			}
		}
		HT4C_RETHROW
	}

	void Namespace::DropTable( String^ name ) {
		DropTable( name, DropDispositions::None );
	}

	void Namespace::DropTable( String^ name, DropDispositions dispo ) {
		if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentNullException( L"name" );
		HT4C_TRY {
			ns->dropTable( CM2A(name), (dispo & DropDispositions::IfExists) == DropDispositions::IfExists );
		}
		HT4C_RETHROW
	}

	void Namespace::DropTables( ) {
		for each( String^ table in Tables ) {
			DropTable( table, DropDispositions::IfExists );
		}
	}
	
	void Namespace::DropTables( Regex^ regex, DropDispositions dispo ) {
		if( regex == nullptr ) throw gcnew ArgumentNullException( L"regex" );
		for each( String^ table in Tables ) {
			if( regex->Match(table)->Success ) {
				DropTable( table, dispo );
			}
		}
	}

	bool Namespace::TableExists( String^ name ) {
		if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentNullException( L"name" );
		HT4C_TRY {
			return ns->existsTable( CM2A(name) );
		} 
		HT4C_RETHROW
	}

	String^ Namespace::GetTableSchema( String^ name ) {
		return GetTableSchema( name, false );
	}

	String^ Namespace::GetTableSchema( String^ name, bool withIds ) {
		if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentNullException( L"name" );
		HT4C_TRY {
			return gcnew String( ns->getTableSchema(CM2A(name), withIds).c_str() );
		}
		HT4C_RETHROW
	}

	void Namespace::CreateNamespace( String^ name ) {
		client->CreateNamespace( name, this );
	}

	void Namespace::CreateNamespace( String^ name, CreateDispositions dispo ) {
		client->CreateNamespace( name, this, dispo );
	}

	Namespace^ Namespace::OpenNamespace( String^ name ) {
		return OpenNamespace( name, OpenDispositions::OpenExisting );
	}

	Namespace^ Namespace::OpenNamespace( String^ name, OpenDispositions dispo ) {
		return client->OpenNamespace( name, this, dispo );
	}

	void Namespace::DropNamespace( String^ name ) {
		client->DropNamespace( name, this );
	}

	void Namespace::DropNamespace( String^ name, DropDispositions dispo ) {
		client->DropNamespace( name, this, dispo );
	}

	void Namespace::DropNamespaces( DropDispositions dispo ) {
		for each( String^ _ns in Namespaces ) {
			client->DropNamespace( _ns, this, DropDispositions::IfExists|dispo );
		}
	}

	void Namespace::DropNamespaces( Regex^ regex, DropDispositions dispo ) {
		if( regex == nullptr ) throw gcnew ArgumentNullException( L"regex" );
		for each( String^ _ns in Namespaces ) {
			if( regex->Match(_ns)->Success ) {
				client->DropNamespace( _ns, this, DropDispositions::IfExists|dispo );
			}
		}
	}

	bool Namespace::NamespaceExists( String^ name ) {
		return client->NamespaceExists( name, this );
	}

	NamespaceListing^ Namespace::GetListing( ) {
		return GetListing( false );
	}

	NamespaceListing^ Namespace::GetListing( bool deep ) {
		HT4C_TRY {
			Common::NamespaceListing nsListing( ns->getName() );
			ns->getListing( deep, nsListing );
			return gcnew NamespaceListing( this, nsListing );
		} 
		HT4C_RETHROW
	}

	void Namespace::Exec( String^ hql ) {
		if( String::IsNullOrEmpty(hql) ) throw gcnew ArgumentNullException( L"hql" );
		HT4C_TRY {
			cli::array<String^>^ commands = hql->Split( L';' );
			for each( String^ command in commands ) {
				String^ commandTrimmed = command->Trim();
				if( !String::IsNullOrEmpty(commandTrimmed) ) {
					ns->exec( CM2A(command) );
				}
			}
		}
		HT4C_RETHROW
	}

	IList<Cell^>^ Namespace::Query( String^ hql ) {
		if( String::IsNullOrEmpty(hql) ) throw gcnew ArgumentNullException( L"hql" );
		HT4C_TRY {
			List<Cell^>^ cells = gcnew List<Cell^>();
			cli::array<String^>^ commands = hql->Split( L';' );
			Common::Cell* _cell = 0;
			Common::Cells* _cells = 0;
			try {
				for each( String^ command in commands ) {
					String^ commandTrimmed = command->Trim();
					if( !String::IsNullOrEmpty(commandTrimmed) ) {
						_cells = ns->query( CM2A(commandTrimmed) );
						if( _cells ) {
							cells->Capacity += (int)_cells->size();
							_cell = Common::Cell::create();
							for( size_t n = 0; n < _cells->size(); ++n ) {
								_cells->get_unchecked( n, _cell );
								cells->Add( gcnew Cell(_cell) );
							}
							delete _cell;
							_cell = 0;
							delete _cells;
							_cells = 0;
						}
					}
				}
			}
			finally {
				if( _cell ) {
					delete _cell;
				}
				if( _cells ) {
					delete _cells;
				}
			}
			return cells;
		}
		HT4C_RETHROW
	}

	Namespace::Namespace( Client^ _client, Common::Namespace* _ns )
	: client( _client )
	, ns( _ns )
	, disposed( false )
	{
		if( client == nullptr ) throw gcnew ArgumentNullException(L"client");
		if( ns == 0 ) throw gcnew ArgumentNullException(L"ns");
	}

	Common::Namespace* Namespace::get() {
		return ns;
	}

}