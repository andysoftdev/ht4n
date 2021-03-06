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

#include "stdafx.h"

#include "Namespace.h"
#include "NamespaceListing.h"
#include "Context.h"
#include "Table.h"
#include "Cell.h"
#include "Exception.h"
#include "CM2U8.h"

#include "ht4c.Common/Namespace.h"
#include "ht4c.Common/Cells.h"

namespace Hypertable {
	using namespace System;
	using namespace System::Globalization;
	using namespace ht4c;

	Namespace::~Namespace( ) {
		disposed = true;
		GC::SuppressFinalize(this);
		this->!Namespace();
	}

	Namespace::!Namespace( ) {
		HT4N_TRY {
			msclr::lock sync( this );
			if( ns ) {
				delete ns;
				ns = 0;
			}
			client = nullptr;
		}
		HT4N_RETHROW
	}

	String^ Namespace::Name::get( ) {
		HT4N_THROW_OBJECTDISPOSED( );

		HT4N_TRY {
			return CM2U8::ToString( ns->getName().c_str() );
		}
		HT4N_RETHROW
	}

	IList<String^>^ Namespace::Namespaces::get( ) {
		HT4N_THROW_OBJECTDISPOSED( );

		HT4N_TRY {
			NamespaceListing^ nsListing = GetListing( false );
			IList<String^>^ namespaces = gcnew List<String^>( nsListing->Namespaces->Count );
			for each( NamespaceListing^ ni in nsListing->Namespaces ) {
				namespaces->Add( ni->Name );
			}
			return namespaces;
		}
		HT4N_RETHROW
	}

	IList<String^>^ Namespace::Tables::get( ) {
		HT4N_THROW_OBJECTDISPOSED( );

		HT4N_TRY {
			NamespaceListing^ nsListing = GetListing( false );
			return nsListing->Tables;
		}
		HT4N_RETHROW
	}

	void Namespace::CreateTable( String^ name, String^ schema ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentNullException( L"name" );
		if( String::IsNullOrEmpty(schema) ) throw gcnew ArgumentNullException( L"schema" );
		name = name->Trim(L'/');
		if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentException( L"Invalid table name", L"name" );

		HT4N_TRY {
			msclr::lock sync( syncRoot );
			ns->createTable( CM2U8(name), CM2U8(schema) );
		}
		HT4N_RETHROW
	}

	void Namespace::CreateTable( String^ name, String^ schema, CreateDispositions dispo ) {
		HT4N_THROW_OBJECTDISPOSED( );

		msclr::lock sync( syncRoot );
		if( (dispo & CreateDispositions::CreateIfNotExist) == CreateDispositions::CreateIfNotExist ) {
			if( TableExists(name) ) {
				return;
			}
		}

		if( (dispo & CreateDispositions::CreateIntermediate) == CreateDispositions::CreateIntermediate ) {
			if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentNullException( L"name" );
			name = name->Trim(L'/');
			if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentException( L"Invalid table name", L"name" );

			int pos = name->LastIndexOf('/');
			if( pos > 0 ) {
				CreateNamespace( name->Substring(0, pos), CreateDispositions::CreateIntermediate | CreateDispositions::CreateIfNotExist );
			}
		}
		CreateTable( name, schema );
	}

	void Namespace::CreateTableLike( String^ name, String^ like ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentNullException( L"name" );
		if( String::IsNullOrEmpty(like) ) throw gcnew ArgumentNullException( L"like" );
		HT4N_TRY {
			msclr::lock sync( syncRoot );
			ns->createTableLike( CM2U8(name), CM2U8(like) );
		}
		HT4N_RETHROW
	}

	void Namespace::CreateTableLike( String^ name, String^ like, CreateDispositions dispo ) {
		msclr::lock sync( syncRoot );
		if( (dispo & CreateDispositions::CreateIfNotExist) == CreateDispositions::CreateIfNotExist ) {
			if( TableExists(name) ) {
				return;
			}
		}
		CreateTableLike( name, like );
	}

	void Namespace::AlterTable( String^ name, String^ schema ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentNullException( L"name" );
		if( String::IsNullOrEmpty(schema) ) throw gcnew ArgumentNullException( L"schema" );
		HT4N_TRY {
			msclr::lock sync( syncRoot );
			ns->alterTable( CM2U8(name), CM2U8(schema) );
		}
		HT4N_RETHROW
	}

	ITable^ Namespace::OpenTable( String^ name ) {
		return OpenTable( name, OpenDispositions::OpenExisting );
	}

	ITable^ Namespace::OpenTable( String^ name, OpenDispositions dispo ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentNullException( L"name" );
		HT4N_TRY {
			msclr::lock sync( syncRoot );
			return gcnew Table( ns->openTable(CM2U8(name), (dispo & OpenDispositions::Force) == OpenDispositions::Force) );
		}
		HT4N_RETHROW
	}

	ITable^ Namespace::OpenTable( String^ name, String^ schema, OpenDispositions dispo ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentNullException( L"name" );
		if( String::IsNullOrEmpty(schema) ) throw gcnew ArgumentNullException( L"schema" );
		HT4N_TRY {
			msclr::lock sync( syncRoot );
			switch( dispo & (OpenDispositions::OpenExisting|OpenDispositions::OpenAlways|OpenDispositions::CreateAlways) ) {
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
			return gcnew Table( ns->openTable(CM2U8(name), (dispo & OpenDispositions::Force) == OpenDispositions::Force) );
		}
		HT4N_RETHROW
	}

	void Namespace::RenameTable( String^ nameOld, String^ nameNew ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( String::IsNullOrEmpty(nameOld) ) throw gcnew ArgumentNullException( L"nameOld" );
		if( String::IsNullOrEmpty(nameNew) ) throw gcnew ArgumentNullException( L"nameNew" );
		HT4N_TRY {
			if( nameOld != nameNew ) {
				msclr::lock sync( syncRoot );
				ns->renameTable( CM2U8(nameOld), CM2U8(nameNew) );
			}
		}
		HT4N_RETHROW
	}

	void Namespace::DropTable( String^ name ) {
		DropTable( name, DropDispositions::None );
	}

	void Namespace::DropTable( String^ name, DropDispositions dispo ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentNullException( L"name" );
		HT4N_TRY {
			msclr::lock sync( syncRoot );
			ns->dropTable( CM2U8(name), (dispo & DropDispositions::IfExists) == DropDispositions::IfExists );
		}
		HT4N_RETHROW
	}

	void Namespace::DropTables( ) {
		HT4N_THROW_OBJECTDISPOSED( );

		for each( String^ table in Tables ) {
			DropTable( table, DropDispositions::IfExists );
		}
	}
	
	void Namespace::DropTables( Regex^ regex, DropDispositions dispo ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( regex == nullptr ) throw gcnew ArgumentNullException( L"regex" );
		for each( String^ table in Tables ) {
			if( regex->Match(table)->Success ) {
				DropTable( table, dispo );
			}
		}
	}

	bool Namespace::TableExists( String^ name ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentNullException( L"name" );
		HT4N_TRY {
			msclr::lock sync( syncRoot );
			return ns->existsTable( CM2U8(name) );
		} 
		HT4N_RETHROW
	}

	String^ Namespace::GetTableSchema( String^ name ) {
		return GetTableSchema( name, false );
	}

	String^ Namespace::GetTableSchema( String^ name, bool withIds ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentNullException( L"name" );
		HT4N_TRY {
			return CM2U8::ToString( ns->getTableSchema(CM2U8(name), withIds).c_str() );
		}
		HT4N_RETHROW
	}

	void Namespace::CreateNamespace( String^ name ) {
		HT4N_THROW_OBJECTDISPOSED( );

		client->CreateNamespace( name, this );
	}

	void Namespace::CreateNamespace( String^ name, CreateDispositions dispo ) {
		HT4N_THROW_OBJECTDISPOSED( );

		client->CreateNamespace( name, this, dispo );
	}

	INamespace^ Namespace::OpenNamespace( String^ name ) {
		HT4N_THROW_OBJECTDISPOSED( );

		return OpenNamespace( name, OpenDispositions::OpenExisting );
	}

	INamespace^ Namespace::OpenNamespace( String^ name, OpenDispositions dispo ) {
		HT4N_THROW_OBJECTDISPOSED( );

		return client->OpenNamespace( name, this, dispo );
	}

	void Namespace::DropNamespace( String^ name ) {
		HT4N_THROW_OBJECTDISPOSED( );

		client->DropNamespace( name, this );
	}

	void Namespace::DropNamespace( String^ name, DropDispositions dispo ) {
		HT4N_THROW_OBJECTDISPOSED( );

		client->DropNamespace( name, this, dispo );
	}

	void Namespace::DropNamespaces( DropDispositions dispo ) {
		HT4N_THROW_OBJECTDISPOSED( );
		for each( String^ _ns in Namespaces ) {
			client->DropNamespace( _ns, this, DropDispositions::IfExists|dispo );
		}
	}

	void Namespace::DropNamespaces( Regex^ regex, DropDispositions dispo ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( regex == nullptr ) throw gcnew ArgumentNullException( L"regex" );
		for each( String^ _ns in Namespaces ) {
			if( regex->Match(_ns)->Success ) {
				client->DropNamespace( _ns, this, DropDispositions::IfExists|dispo );
			}
		}
	}

	bool Namespace::NamespaceExists( String^ name ) {
		HT4N_THROW_OBJECTDISPOSED( );

		return client->NamespaceExists( name, this );
	}

	NamespaceListing^ Namespace::GetListing( ) {
		return GetListing( false );
	}

	NamespaceListing^ Namespace::GetListing( bool deep ) {
		HT4N_THROW_OBJECTDISPOSED( );

		HT4N_TRY {
			Common::NamespaceListing nsListing( ns->getName() );
			ns->getListing( deep, nsListing );
			return gcnew NamespaceListing( this, nsListing );
		} 
		HT4N_RETHROW
	}

	void Namespace::Exec( ... cli::array<String^>^ hql ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( hql == nullptr ) throw gcnew ArgumentNullException( L"hql" );
		HT4N_TRY {
			for each( String^ command in hql ) {
				if( command != nullptr ) {
					String^ commandTrimmed = command->Trim();
					if( !String::IsNullOrEmpty(commandTrimmed) ) {
						ns->exec( CM2U8(commandTrimmed) );
					}
				}
			}
		}
		HT4N_RETHROW
	}

	IList<Cell^>^ Namespace::Query( ... cli::array<String^>^ hql ) {
		HT4N_THROW_OBJECTDISPOSED( );

		if( hql == nullptr ) throw gcnew ArgumentNullException( L"hql" );
		HT4N_TRY {
			List<Cell^>^ cells = gcnew List<Cell^>();
			Common::Cell* _cell = 0;
			Common::Cells* _cells = 0;
			try {
				for each( String^ command in hql ) {
					if( command != nullptr ) {
						String^ commandTrimmed = command->Trim();
						if( !String::IsNullOrEmpty(commandTrimmed) ) {
							_cells = ns->query( CM2U8(commandTrimmed) );
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
		HT4N_RETHROW
	}

	String^ Namespace::ToString() {
		HT4N_THROW_OBJECTDISPOSED( );

		return String::Format( CultureInfo::InvariantCulture
												 , L"{0}(Name={1})"
												 , GetType()
												 , Name != nullptr ? Name : L"null");
	}

	Namespace::Namespace( Client^ _client, Common::Namespace* _ns )
	: client( _client )
	, ns( _ns )
	, syncRoot( nullptr )
	, disposed( false )
	{
		if( client == nullptr ) throw gcnew ArgumentNullException(L"client");
		if( ns == 0 ) throw gcnew ArgumentNullException(L"ns");

		syncRoot = _client->SyncRoot;
	}

	Common::Namespace* Namespace::get() {
		return ns;
	}

}