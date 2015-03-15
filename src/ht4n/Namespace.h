/** -*- C++ -*-
 * Copyright (C) 2010-2015 Thalmann Software & Consulting, http://www.softdev.ch
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

#include "INamespace.h"
#include "Client.h"

namespace ht4c { namespace Common {
	class Namespace;
} }

namespace Hypertable {
	using namespace System;
	using namespace System::Runtime::InteropServices;
	using namespace System::Collections::Generic;
	using namespace System::Text::RegularExpressions;
	using namespace ht4c;

	interface class ITable;
	ref class Client;
	ref class NamespaceListing;
	ref class Cell;

	/// <summary>
	/// Represents a Hypertable namespace.
	/// </summary>
	/// <seealso cref="INamespace"/>
	ref class Namespace sealed : public INamespace {

		public:

			/// <summary>
			/// Clean up all managed and unmanaged resources.
			/// </summary>
			virtual ~Namespace( );

			/// <summary>
			/// Clean up all unmanaged resources.
			/// </summary>
			!Namespace( );

			#pragma region INamespace properties

			property String^ Name {
				virtual String^ get();
			}

			property IList<String^>^ Namespaces {
				virtual IList<String^>^ get();
			}

			property IList<String^>^ Tables {
				virtual IList<String^>^ get();
			}

			property bool IsDisposed {
				virtual bool get( ) {
					return disposed;
				}
			}

			#pragma endregion

			#pragma region INamespace methods

			virtual void CreateTable( String^ name, String^ schema );
			virtual void CreateTable( String^ name, String^ schema, CreateDispositions dispo );
			virtual void CreateTableLike( String^ name, String^ like );
			virtual void CreateTableLike( String^ name, String^ like, CreateDispositions dispo );
			virtual void AlterTable( String^ name, String^ schema );
			virtual void RenameTable( String^ nameOld, String^ nameNew );
			virtual ITable^ OpenTable( String^ name );
			virtual ITable^ OpenTable( String^ name, OpenDispositions dispo );
			virtual ITable^ OpenTable( String^ name, String^ schema, OpenDispositions dispo );
			virtual void DropTable( String^ name );
			virtual void DropTable( String^ name, DropDispositions dispo );
			virtual void DropTables( );
			virtual void DropTables( Regex^ regex, DropDispositions dispo );
			virtual bool TableExists( String^ name );
			virtual String^ GetTableSchema( String^ name );
			virtual String^ GetTableSchema( String^ name, bool withIds );
			virtual void CreateNamespace( String^ name );
			virtual void CreateNamespace( String^ name, CreateDispositions dispo );
			virtual INamespace^ OpenNamespace( String^ name );
			virtual INamespace^ OpenNamespace( String^ name, OpenDispositions dispo );
			virtual void DropNamespace( String^ name );
			virtual void DropNamespace( String^ name, DropDispositions dispo );
			virtual void DropNamespaces( DropDispositions dispo );
			virtual void DropNamespaces( Regex^ regex, DropDispositions dispo );
			virtual bool NamespaceExists( String^ name );
			virtual NamespaceListing^ GetListing( );
			virtual NamespaceListing^ GetListing( bool deep );
			virtual void Exec( String^ hql );
			virtual IList<Cell^>^ Query( String^ hql );

			#pragma endregion

			/// <summary>
			/// Returns a string that represents the current object.
			/// </summary>
			/// <returns>A string that represents the current object.</returns>
			virtual String^ ToString() override;

		internal:

			Namespace( Client^ client, Common::Namespace* ns );

			Common::Namespace* get();

		private:

			Object^ syncRoot;
			IClient^ client;
			Common::Namespace* ns;
			bool disposed;
	};

}