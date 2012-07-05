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

#include "NamespaceListing.h"
#include "INamespace.h"
#include "Exception.h"
#include "CM2U8.h"

#include "ht4c.Common/NamespaceListing.h"

namespace Hypertable {
	using namespace System;
	using namespace System::Globalization;
	using namespace ht4c;

	inline String^ getLeafName( const std::string& name ) {
		std::string::size_type pos = name.find_last_of('/');
		if( pos != std::string::npos ) {
			return CM2U8::ToString( name.substr(pos + 1).c_str() );
		}
		return CM2U8::ToString( name.c_str() );
	}

	String^ NamespaceListing::ToString() {
		return String::Format( CultureInfo::InvariantCulture
												 , L"{0}(Name={1}, FullName={2}, Namespaces.Count={3}, Tables.Count={4})"
												 , GetType()
												 , Name != nullptr ? Name : L"null"
												 , FullName != nullptr ? FullName : L"null"
												 , Namespaces != nullptr ? Namespaces->Count : 0
												 , Tables != nullptr ? Tables->Count : 0 );
	}

	NamespaceListing::NamespaceListing( INamespace^ ns, const ht4c::Common::NamespaceListing& nsListing )
	: name( getLeafName(nsListing.getName()) )
	, namespaces( gcnew List<NamespaceListing^>() )
	, tables( gcnew List<String^>() )
	{
		if( ns == nullptr ) throw gcnew ArgumentNullException(L"ns");
		fullName = ns->Name;
		for( ht4c::Common::NamespaceListing::tables_t::const_iterator it = nsListing.getTables().begin(); it != nsListing.getTables().end(); ++it ) {
			tables->Add( CM2U8::ToString((*it).c_str()) );
		}
		for( ht4c::Common::NamespaceListing::namespaces_t::const_iterator it = nsListing.getNamespaces().begin(); it != nsListing.getNamespaces().end(); ++it ) {
			namespaces->Add( GetListing(this, *it) );
		}
	}

	NamespaceListing::NamespaceListing( NamespaceListing^ _parent, String^ _name )
	: name( _name )
	, parent( _parent )
	, namespaces( gcnew List<NamespaceListing^>() )
	, tables( gcnew List<String^>() )
	{
		if( name == nullptr ) throw gcnew ArgumentNullException(L"name");
		if( parent == nullptr ) throw gcnew ArgumentNullException(L"parent");
		fullName = parent->fullName + "/" + name;
	}

	NamespaceListing^ NamespaceListing::GetListing( NamespaceListing^ parent, const ht4c::Common::NamespaceListing& _nsListing ) {
		NamespaceListing^ nsListing = gcnew NamespaceListing( parent, getLeafName(_nsListing.getName()) );
		for( ht4c::Common::NamespaceListing::tables_t::const_iterator it = _nsListing.getTables().begin(); it != _nsListing.getTables().end(); ++it ) {
			nsListing->tables->Add( CM2U8::ToString((*it).c_str()) );
		}
		for( ht4c::Common::NamespaceListing::namespaces_t::const_iterator it = _nsListing.getNamespaces().begin(); it != _nsListing.getNamespaces().end(); ++it ) {
			nsListing->namespaces->Add( GetListing(nsListing, *it) );
		}
		return nsListing;
	}

}