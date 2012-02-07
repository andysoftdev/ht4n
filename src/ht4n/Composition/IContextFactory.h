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

namespace Hypertable {

	interface class IContext;

	namespace Composition {

	using namespace System;
	using namespace System::Collections::Generic;

	/// <summary>
	/// Represents a Hypertable context factory.
	/// </summary>
	/// <example>
	/// The following example shows how to import the context factory.
	/// <code>
	/// using System;
	/// using System.ComponentModel.Composition;
	/// using System.ComponentModel.Composition.Hosting;
	/// using System.Reflection;
	/// 
	/// class ImportContextFactory
	/// {
	///    [Import(typeof(IContextFactory))]
	///    private IContextFactory contextFactory;
	///
	///    public Compose()
	//     {
	///       using (var catalog = new AggregateCatalog()) {
	///          catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetAssembly(typeof(IContextFactory))));
	///          using (var container = new CompositionContainer(catalog)) {
	///             container.ComposeParts(this);
	///
	///             using (var context = this.contextFactory.Create("Provider=Hyper; Uri=net.tcp://localhost")) {
	///                using (var client = context.CreateClient()) {
	///                   // use the client
	///                }
	///             }
	///          }
	///       }
	///    }
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="Hypertable::IContext"/>
	public interface class IContextFactory {

		public:

			/// <summary>
			/// Creates a new Context instance using specified connection string.
			/// </summary>
			/// <param name="connectionString">Connection string.</param>
			/// <returns>New Context instance.</returns>
			Hypertable::IContext^ Create( String^ connectionString );

			/// <summary>
			/// Creates a new Context instance using specified configuration properties.
			/// </summary>
			/// <param name="properties">Configuration properties.</param>
			/// <returns>New Context instance.</returns>
			/// <remarks>
			/// Following property types will be forwarded to the native ht4c providers:
			/// <table class="comment">
			/// <tr><td>string</td><td>IEnumerable&lt;string&gt;</td></tr>
			/// <tr><td>int</td><td>IEnumerable&lt;int&gt;</td></tr>
			/// <tr><td>long</td><td>IEnumerable&lt;long&gt;</td></tr>
			/// <tr><td>ushort</td></tr>
			/// <tr><td>double</td><td>IEnumerable&lt;double&gt;</td></tr>
			/// <tr><td>bool</td></tr>
			/// </table>
			/// </remarks>
			Hypertable::IContext^ Create( IDictionary<String^, Object^>^ properties );

			/// <summary>
			/// Creates a new Context instance using specified configuration properties.
			/// </summary>
			/// <param name="connectionString">Connection string.</param>
			/// <param name="properties">Configuration properties, might overwrite those properties from connection string.</param>
			/// <returns>New Context instance.</returns>
			/// <remarks>
			/// Following property types will be forwarded to the native ht4c providers:
			/// <table class="comment">
			/// <tr><td>string</td><td>IEnumerable&lt;string&gt;</td></tr>
			/// <tr><td>int</td><td>IEnumerable&lt;int&gt;</td></tr>
			/// <tr><td>long</td><td>IEnumerable&lt;long&gt;</td></tr>
			/// <tr><td>ushort</td></tr>
			/// <tr><td>double</td><td>IEnumerable&lt;double&gt;</td></tr>
			/// <tr><td>bool</td></tr>
			/// </table>
			/// </remarks>
			Hypertable::IContext^ Create( String^ connectionString, IDictionary<String^, Object^>^ properties );
	};

	} }