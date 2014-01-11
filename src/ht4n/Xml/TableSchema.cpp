/** -*- C++ -*-
 * Copyright (C) 2010-2014 Thalmann Software & Consulting, http://www.softdev.ch
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

#include "TableSchema.h"

namespace Hypertable { namespace Xml {
	using namespace System::Globalization;
	using namespace System::IO;
	using namespace System::Text;

	namespace {

		ref class UTF8StringWriter sealed : public StringWriter
		{
			public:

				UTF8StringWriter( StringBuilder^ sb )
					: StringWriter( sb, CultureInfo::InvariantCulture )
				{
				}

				property System::Text::Encoding^ Encoding { 
					virtual System::Text::Encoding^ get() override { 
						return System::Text::Encoding::UTF8;
					}
				}
		};

	}

	TableSchema^ TableSchema::Parse( String^ xml ) {
		if( String::IsNullOrEmpty(xml) ) throw gcnew ArgumentNullException( L"xml" );
		StringReader^ stringReader = nullptr;
		try {
			stringReader = gcnew StringReader( xml );
			XmlSerializer^ serializer = gcnew XmlSerializer( TableSchema::typeid );
			return safe_cast<TableSchema^>( serializer->Deserialize(stringReader) );
		}
		finally {
			if( stringReader != nullptr ) {
				delete stringReader;
			}
		}
	}

	String^ TableSchema::ToString( ) {
		StringBuilder^ sb = gcnew StringBuilder();
		XmlSerializerNamespaces^ ns = gcnew XmlSerializerNamespaces();
		ns->Add( String::Empty, String::Empty );
		StringWriter^ stringWriter = nullptr;
		try {
			stringWriter = gcnew UTF8StringWriter( sb );
			XmlSerializer^ serializer = gcnew XmlSerializer( TableSchema::typeid );
			serializer->Serialize( stringWriter, this, ns );
			stringWriter->Flush();
			return sb->ToString();
		}
		finally {
			if( stringWriter != nullptr ) {
				delete stringWriter;
			}
		}
	}

} }