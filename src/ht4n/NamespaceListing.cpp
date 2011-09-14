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

#include "NamespaceListing.h"
#include "Namespace.h"
#include "Exception.h"
#include "CM2A.h"

#include "ht4c.Common/NamespaceListing.h"

namespace Hypertable {
	using namespace System;
	using namespace ht4c;

	NamespaceListing::NamespaceListing( Namespace^ ns, const ht4c::Common::NamespaceListing& nsListing )
	: name( gcnew String(nsListing.getName().c_str()) )
	, namespaces( gcnew List<NamespaceListing^>() )
	, tables( gcnew List<String^>() )
	{
		if( ns == nullptr ) throw gcnew ArgumentNullException(L"ns");
		fullName = ns->Name;
		for( ht4c::Common::NamespaceListing::tables_t::const_iterator it = nsListing.getTables().begin(); it != nsListing.getTables().end(); ++it ) {
			tables->Add( gcnew String((*it).c_str()) );
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
		NamespaceListing^ nsListing = gcnew NamespaceListing( parent, gcnew String(_nsListing.getName().c_str()) );
		for( ht4c::Common::NamespaceListing::tables_t::const_iterator it = _nsListing.getTables().begin(); it != _nsListing.getTables().end(); ++it ) {
			nsListing->tables->Add( gcnew String((*it).c_str()) );
		}
		for( ht4c::Common::NamespaceListing::namespaces_t::const_iterator it = _nsListing.getNamespaces().begin(); it != _nsListing.getNamespaces().end(); ++it ) {
			nsListing->namespaces->Add( GetListing(nsListing, *it) );
		}
		return nsListing;
	}

}