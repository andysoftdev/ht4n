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

#pragma once

#ifndef __cplusplus_cli
#error "requires /clr"
#endif

#include "CreateDispositions.h"
#include "OpenDispositions.h"
#include "DropDispositions.h"

namespace Hypertable {
	using namespace System;

	interface class INamespace;

	/// <summary>
	/// Represents a Hypertable client, provides methods to handle Hypertable namespaces.
	/// </summary>
	/// <example>
	/// The following example shows how to create and use the Client class.
	/// <code>
	/// using( var ctx = Context.Create("Provider=Hyper; Uri=localhost") ) {
	///    using( var client = ctx.CreateClient() ) {
	///       using( var ns = client.OpenNamespace("test", OpenDispositions.OpenAlways) ) { // here "test" or "/test" is equivalent
	///          // use namespace
	///       }
	///       client.DropNamespace( "test", DropDispositions.Complete );
	///    }
	/// }
	/// </code>
	/// The following example shows how to open the root namespace.
	/// <code>
	/// using( var ctx = Context.Create("Provider=Hyper; Uri=localhost") ) {
	///    using( var client = ctx.CreateClient() ) {
	///       using( var ns = client.OpenNamespace("/") ) {
	///          // use namespace
	///       }
	///    }
	/// }
	/// </code>
	/// </example>
	public interface class IClient : public IDisposable {

		public:

			/// <summary>
			/// Gets a value indicating whether the object has been disposed.
			/// </summary>
			/// <remarks>true if the object has been disposed, otherwise false.</remarks>
			property bool IsDisposed {
				bool get( );
			}

			/// <summary>
			/// Creates a new namespace.
			/// </summary>
			/// <param name="name">Namespace name.</param>
			/// <remarks>
			/// Use '/' separator character to separate namespace names. Fails if the namespace already exist or
			/// intermediate namespaces do not exist. Namespace names are case sensitive.
			/// </remarks>
			void CreateNamespace( String^ name );

			/// <summary>
			/// Creates a new namespace.
			/// </summary>
			/// <param name="name">Namespace name.</param>
			/// <param name="dispo">An action to take if the namespace exists or does not exist.</param>
			/// <remarks>
			/// Use '/' separator character to separate namespace names. Namespace names are case sensitive.
			/// </remarks>
			/// <seealso cref="CreateDispositions"/>
			void CreateNamespace( String^ name, CreateDispositions dispo );

			/// <summary>
			/// Creates a new namespace relative to an existing namespace.
			/// </summary>
			/// <param name="name">Namespace name.</param>
			/// <param name="nsBase">Base namespace, might be null.</param>
			/// <remarks>
			/// Use '/' separator character to separate namespace names. Optionally specify nsBase
			/// to create the new namespace relative to an existing namespace. Fails if the namespace
			/// already exist or intermediate namespaces do not exist. Namespace names are case sensitive.
			/// </remarks>
			void CreateNamespace( String^ name, INamespace^ nsBase );

			/// <summary>
			/// Creates a new namespace relative to an existing namespace.
			/// </summary>
			/// <param name="name">Namespace name.</param>
			/// <param name="nsBase">Base namespace, might be null.</param>
			/// <param name="dispo">An action to take if the namespace exists or does not exist.</param>
			/// <remarks>
			/// Use '/' separator character to separate namespace names. Optionally specify nsBase
			/// to create the new namespace relative to an existing namespace. Namespace names are case sensitive.
			/// </remarks>
			/// <seealso cref="CreateDispositions"/>
			void CreateNamespace( String^ name, INamespace^ nsBase, CreateDispositions dispo );

			/// <summary>
			/// Opens an existig namespace.
			/// </summary>
			/// <param name="name">Namespace name.</param>
			/// <returns>Opened namespace.</returns>
			/// <remarks>
			/// Use '/' separator character to separate namespace names. Namespace names are case sensitive.
			/// </remarks>
			INamespace^ OpenNamespace( String^ name );

			/// <summary>
			/// Opens or create a namespace.
			/// </summary>
			/// <param name="name">Namespace name.</param>
			/// <param name="dispo">An action to take if the namespace exists or does not exist.</param>
			/// <returns>Opened namespace.</returns>
			/// <remarks>
			/// Use '/' separator character to separate namespace names. Namespace names are case sensitive.
			/// </remarks>
			/// <seealso cref="OpenDispositions"/>
			INamespace^ OpenNamespace( String^ name, OpenDispositions dispo );

			/// <summary>
			/// Opens an existig namespace relative to an existing namespace.
			/// </summary>
			/// <param name="name">Namespace name</param>
			/// <param name="nsBase">Base namespace, might be null</param>
			/// <returns>Opened namespace.</returns>
			/// <remarks>
			/// Use '/' separator character to separate namespace names. Optionally specify nsBase
			/// if the namespace to open is relative to an existing namespace. Namespace names are case sensitive.
			/// </remarks>
			INamespace^ OpenNamespace( String^ name, INamespace^ nsBase );

			/// <summary>
			/// Opens or creates a namespace relative to an existing namespace.
			/// </summary>
			/// <param name="name">Namespace name.</param>
			/// <param name="nsBase">Base namespace, might be null.</param>
			/// <param name="dispo">An action to take if the namespace exists or does not exist.</param>
			/// <returns>Opened namespace.</returns>
			/// <remarks>
			/// Use '/' separator character to separate namespace names. Optionally specify nsBase
			/// if the namespace to open is relative to an existing namespace. Namespace names are case sensitive.
			/// </remarks>
			/// <seealso cref="OpenDispositions"/>
			INamespace^ OpenNamespace( String^ name, INamespace^ nsBase, OpenDispositions dispo );

			/// <summary>
			/// Drops an existing namespace.
			/// </summary>
			/// <param name="name">Namespace name.</param>
			/// <remarks>
			/// Use '/' separator character to separate namespace names. Fails if the namespace
			/// is not empty. Namespace names are case sensitive.
			/// </remarks>
			void DropNamespace( String^ name );

			/// <summary>
			/// Drops an existing namespace.
			/// </summary>
			/// <param name="name">Namespace name.</param>
			/// <param name="dispo">Defines details for the drop operation.</param>
			/// <remarks>
			/// Use '/' separator character to separate namespace names. Namespace names are case sensitive.
			/// </remarks>
			/// <seealso cref="DropDispositions"/>
			void DropNamespace( String^ name, DropDispositions dispo );

			/// <summary>
			/// Drops an existing namespace relative to an existing namespace.
			/// </summary>
			/// <param name="name">Namespace name.</param>
			/// <param name="nsBase">Base namespace, might be null.</param>
			/// <remarks>
			/// Use '/' separator character to separate namespace names. Optionally specify nsBase
			/// if the namespace to drop is relative to an existing namespace. Fails if the namespace
			/// is not empty. Namespace names are case sensitive.
			/// </remarks>
			void DropNamespace( String^ name, INamespace^ nsBase );

			/// <summary>
			/// Drops an existing namespace relative to an existing namespace.
			/// </summary>
			/// <param name="name">Namespace name.</param>
			/// <param name="nsBase">Base namespace, might be null.</param>
			/// <param name="dispo">Defines details for the drop operation.</param>
			/// <remarks>
			/// Use '/' separator character to separate namespace names. Optionally specify nsBase
			/// if the namespace to drop is relative to an existing namespace. Namespace names are case sensitive.
			/// </remarks>
			/// <seealso cref="DropDispositions"/>
			void DropNamespace( String^ name, INamespace^ nsBase, DropDispositions dispo );

			/// <summary>
			/// Checks if a namespace exists.
			/// </summary>
			/// <param name="name">Namespace name.</param>
			/// <returns>true if the namespace exists.</returns>
			/// <remarks>
			/// Use '/' separator character to separate namespace names. Namespace names are case sensitive.
			/// </remarks>
			bool NamespaceExists( String^ name );

			/// <summary>
			/// Checks if a namespace exists relative to an existing namespace.
			/// </summary>
			/// <param name="name">Namespace name.</param>
			/// <param name="nsBase">Base namespace, might be null.</param>
			/// <returns>true if the namespace exists.</returns>
			/// <remarks>
			/// Use '/' separator character to separate namespace names. Optionally specify nsBase
			/// if the namespace to check is relative to an existing namespace. Namespace names are case sensitive.
			/// </remarks>
			bool NamespaceExists( String^ name, INamespace^ nsBase );
	};

}