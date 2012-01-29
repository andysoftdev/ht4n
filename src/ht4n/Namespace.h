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

#pragma once

#ifndef __cplusplus_cli
#error "requires /clr"
#endif

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

	ref class Table;
	ref class NamespaceListing;
	ref class Cell;

	/// <summary>
	/// Represents a Hypertable namespace, provides methods to handle Hypertable tables.
	/// </summary>
	/// <example>
	/// The following example shows how to create a table using the xml schema.
	/// <code>
	/// string schema = 
	///    "&lt;Schema&gt;"
	/// +     "&lt;AccessGroup name=\"default\"&gt;"
	/// +     "&lt;ColumnFamily&gt;"
	/// +         "&lt;Name&gt;cf&lt;/Name&gt;"
	/// +         "&lt;deleted&gt;false&lt;/deleted&gt;"
	/// +     "&lt;/ColumnFamily&gt;"
	/// +     "&lt;/AccessGroup&gt;"
	/// + "&lt;/Schema&gt;";
	///
	/// using( Context ctx = Context.Create(ContextKind.Hyper, "localhost") ) {
	///    using( Client client = ctx.CreateClient() ) {
	///       using( Namespace ns = client.OpenNamespace("test", OpenDispositions.OpenAlways) ) {
	///          using( Table table = ns.OpenTable("t1", schema, OpenDispositions.OpenAlways) ) {
	///             // use table
	///          }
	///       }
	///    }
	/// }
	/// </code>
	/// The following example shows how to create a table using HQL.
	/// <code>
	/// using( Context ctx = Context.Create(ContextKind.Hyper, "localhost") ) {
	///    using( Client client = ctx.CreateClient() ) {
	///       using( Namespace ns = client.OpenNamespace("test", OpenDispositions.OpenAlways) ) {
	///          if( !table.TableExists("t1") ) {
	///             ns.Exec("CREATE TABLE t1 (cf)");
	///          }
	///          using( Table table = ns.OpenTable("t1") ) {
	///             // use table
	///          }
	///       }
	///    }
	/// }
	/// </code>
	/// </example>
	public ref class Namespace sealed : public IDisposable {

		public:

			/// <summary>
			/// Clean up all managed and unmanaged resources.
			/// </summary>
			virtual ~Namespace( );

			/// <summary>
			/// Clean up all unmanaged resources.
			/// </summary>
			!Namespace( );

			/// <summary>
			/// Gets the namespace name.
			/// </summary>
			property String^ Name {
				String^ get();
			}

			/// <summary>
			/// Gets all sub namespace names.
			/// </summary>
			property IList<String^>^ Namespaces {
				IList<String^>^ get();
			}

			/// <summary>
			/// Gets all table names.
			/// </summary>
			property IList<String^>^ Tables {
				IList<String^>^ get();
			}

			/// <summary>
			/// Gets a value indicating whether the object has been disposed.
			/// </summary>
			/// <remarks>true if the object has been disposed, otherwise false.</remarks>
			property bool IsDisposed {
				bool get( ) {
					return disposed;
				}
			}

			/// <summary>
			/// Creates a new table.
			/// </summary>
			/// <param name="name">Table name</param>
			/// <param name="schema">XML table schema</param>
			void CreateTable( String^ name, String^ schema );

			/// <summary>
			/// Creates a new table.
			/// </summary>
			/// <param name="name">Table name</param>
			/// <param name="schema">XML table schema</param>
			/// <param name="dispo">An action to take if the table exists or does not exist.</param>
			/// <seealso cref="CreateDispositions"/>
			void CreateTable( String^ name, String^ schema, CreateDispositions dispo );

			/// <summary>
			/// Creates a new table like another existing.
			/// </summary>
			/// <param name="name">Table name</param>
			/// <param name="like">Name of another existing table</param>
			void CreateTableLike( String^ name, String^ like );

			/// <summary>
			/// Creates a new table like another existing.
			/// </summary>
			/// <param name="name">Table name</param>
			/// <param name="like">Name of another existing table</param>
			/// <param name="dispo">An action to take if the table exists or does not exist.</param>
			/// <seealso cref="CreateDispositions"/>
			void CreateTableLike( String^ name, String^ like, CreateDispositions dispo );

			/// <summary>
			/// Alter an existing table.
			/// </summary>
			/// <param name="name">Table name</param>
			/// <param name="schema">XML table schema</param>
			void AlterTable( String^ name, String^ schema );

			/// <summary>
			/// Rename an existing table.
			/// </summary>
			/// <param name="nameOld">Old table name</param>
			/// <param name="nameNew">New table name</param>
			void RenameTable( String^ nameOld, String^ nameNew );

			/// <summary>
			/// Opens an existing table.
			/// </summary>
			/// <param name="name">Table name</param>
			/// <returns>Opened table.</returns>
			Table^ OpenTable( String^ name );

			/// <summary>
			/// Opens an existing table.
			/// </summary>
			/// <param name="name">Table name</param>
			/// <param name="dispo">An action to take if the table exists or does not exist.</param>
			/// <returns>Opened table.</returns>
			/// <seealso cref="OpenDispositions"/>
			Table^ OpenTable( String^ name, OpenDispositions dispo );

			/// <summary>
			/// Opens or create a table.
			/// </summary>
			/// <param name="name">Table name</param>
			/// <param name="schema">XML table schema</param>
			/// <param name="dispo">An action to take if the table exists or does not exist.</param>
			/// <returns>Opened table.</returns>
			/// <seealso cref="OpenDispositions"/>
			Table^ OpenTable( String^ name, String^ schema, OpenDispositions dispo );

			/// <summary>
			/// Drops a table.
			/// </summary>
			/// <param name="name">Table name</param>
			void DropTable( String^ name );

			/// <summary>
			/// Drops an existing table.
			/// </summary>
			/// <param name="name">Table name.</param>
			/// <param name="dispo">Defines details for the drop operation.</param>
			/// <seealso cref="DropDispositions"/>
			void DropTable( String^ name, DropDispositions dispo );

			/// <summary>
			/// Drops all tables in this namespace.
			/// </summary>
			void DropTables( );

			/// <summary>
			/// Drops existing tables.
			/// </summary>
			/// <param name="regex">Regular expression to match the table names.</param>
			/// <param name="dispo">Defines details for the drop operation.</param>
			/// <seealso cref="DropDispositions"/>
			void DropTables( Regex^ regex, DropDispositions dispo );

			/// <summary>
			/// Checks if a table exists in this namespace.
			/// </summary>
			/// <param name="name">Table name.</param>
			/// <returns>true if the table exists.</returns>
			bool TableExists( String^ name );

			/// <summary>
			/// Returns the xml table schema for the specified table.
			/// </summary>
			/// <param name="name">Table name.</param>
			/// <returns>XML table schema.</returns>
			String^ GetTableSchema( String^ name );

			/// <summary>
			/// Returns the xml table schema for the specified table.
			/// </summary>
			/// <param name="name">Table name.</param>
			/// <param name="withIds">Include generation and column family ID attributes.</param>
			/// <returns>XML table schema.</returns>
			String^ GetTableSchema( String^ name, bool withIds );

			/// <summary>
			/// Creates a new sub namespace.
			/// </summary>
			/// <param name="name">Namespace name.</param>
			/// <remarks>
			/// Use '/' separator character to separate namespace names. Fails if the namespace already exist or
			/// intermediate namespaces do not exist. Namespace names are case sensitive.
			/// </remarks>
			void CreateNamespace( String^ name );

			/// <summary>
			/// Creates a new sub namespace.
			/// </summary>
			/// <param name="name">Namespace name.</param>
			/// <param name="dispo">An action to take if the namespace exists or does not exist.</param>
			/// <remarks>
			/// Use '/' separator character to separate namespace names. Namespace names are case sensitive.
			/// </remarks>
			/// <seealso cref="CreateDispositions"/>
			void CreateNamespace( String^ name, CreateDispositions dispo );

			/// <summary>
			/// Opens an existig sub namespace.
			/// </summary>
			/// <param name="name">Namespace name.</param>
			/// <returns>Opened namespace.</returns>
			/// <remarks>
			/// Use '/' separator character to separate namespace names. Namespace names are case sensitive.
			/// </remarks>
			Namespace^ OpenNamespace( String^ name );

			/// <summary>
			/// Opens or create a sub namespace.
			/// </summary>
			/// <param name="name">Namespace name.</param>
			/// <param name="dispo">An action to take if the namespace exists or does not exist.</param>
			/// <returns>Opened namespace.</returns>
			/// <remarks>
			/// Use '/' separator character to separate namespace names. Namespace names are case sensitive.
			/// </remarks>
			/// <seealso cref="OpenDispositions"/>
			Namespace^ OpenNamespace( String^ name, OpenDispositions dispo );

			/// <summary>
			/// Drops an existing sub namespace.
			/// </summary>
			/// <param name="name">Namespace name.</param>
			/// <remarks>
			/// Use '/' separator character to separate namespace names. Fails if the namespace
			/// is not empty. Namespace names are case sensitive.
			/// </remarks>
			void DropNamespace( String^ name );

			/// <summary>
			/// Drops an existing sub namespace.
			/// </summary>
			/// <param name="name">Namespace name.</param>
			/// <param name="dispo">Defines details for the drop operation.</param>
			/// <remarks>
			/// Use '/' separator character to separate namespace names. Namespace names are case sensitive.
			/// </remarks>
			/// <seealso cref="DropDispositions"/>
			void DropNamespace( String^ name, DropDispositions dispo );

			/// <summary>
			/// Drops all sub namespaces.
			/// </summary>
			/// <param name="dispo">Defines details for the drop operation.</param>
			void DropNamespaces( DropDispositions dispo );

			/// <summary>
			/// Drops existing sub namespaces.
			/// </summary>
			/// <param name="regex">Regular expression to match the sub namespace names.</param>
			/// <param name="dispo">Defines details for the drop operation.</param>
			void DropNamespaces( Regex^ regex, DropDispositions dispo );

			/// <summary>
			/// Checks if a sub namespace exists.
			/// </summary>
			/// <param name="name">Namespace name.</param>
			/// <returns>true if the namespace exists.</returns>
			/// <remarks>
			/// Use '/' separator character to separate namespace names. Namespace names are case sensitive.
			/// </remarks>
			bool NamespaceExists( String^ name );

			/// <summary>
			/// Gets the namespace listing.
			/// </summary>
			/// <returns>Namespace listing.</returns>
			NamespaceListing^ GetListing( );

			/// <summary>
			/// Gets the namespace listing.
			/// </summary>
			/// <param name="deep">If true all sub-namespaces and there containing tables/namespaces will be returned.</param>
			/// <returns>Namespace listing.</returns>
			NamespaceListing^ GetListing( bool deep );

			/// <summary>
			/// Executes a HQL command.
			/// </summary>
			/// <param name="hql">HQL command.</param>
			/// <remarks>
			/// Use ';' to separate multiple HQL commands.
			/// </remarks>
			void Exec( String^ hql );

			/// <summary>
			/// Executes a HQL query.
			/// </summary>
			/// <param name="hql">HQL query.</param>
			/// <returns>Resulting cells.</returns>
			/// <remarks>
			/// Use ';' to separate multiple HQL queries.
			/// </remarks>
			IList<Cell^>^ Query( String^ hql );

			/// <summary>
			/// Returns a string that represents the current object.
			/// </summary>
			/// <returns>A string that represents the current object.</returns>
			virtual String^ ToString() override;

		internal:

			Namespace( Client^ client, Common::Namespace* ns );

			Common::Namespace* get();

		private:

			Client^ client;
			Common::Namespace* ns;
			bool disposed;
	};

}