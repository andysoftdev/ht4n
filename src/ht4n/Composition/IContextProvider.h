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

namespace Hypertable {

	interface class IContext;

	namespace Composition {

	using namespace System;

	/// <summary>
	/// Represents a Hypertable context provider.
	/// </summary>
	/// <example>
	/// The following example shows how to export a context provider.
	/// <code>
	/// using System;
	/// using System.Collections.Generic;
	/// using Hypertable.Composition;
	///
	/// [ExportContextProvider("SampleContextProvider")]
	/// public class SampleContextProvider : IContextProvider
	/// {
	///     public Func&lt;IDictionary&lt;string, object&gt;, IContext&gt; Provider {
	///         get {
	///             return (properties) =&gt; new SampleContext(properties);
	///         }
	///     }
	/// }
	/// 
	/// void foo() {
	///    // set the "Hypertable.Composition.ComposablePartCatalogs" property which expects
	///    // a ComposablePartCatalog or an IEnumerable&lt;ComposablePartCatalog&gt;
	///    var properties = new Dictionary%lt;string, object&gt;
	///       {
	///          { "Hypertable.Composition.ComposablePartCatalogs", new AssemblyCatalog(GetAssembly(this.GetType())) }
	///       };
	///    using (var context = Context.Create("Provider=TestContextProvider", properties)) {
	///       // use the context
	///    }
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="Hypertable::IContext"/>
	public interface class IContextProvider {

		public:

			/// <summary>
			/// Gets the context provider.
			/// </summary>
			property Func<IDictionary<String^, Object^>^, IContext^>^ Provider {
				Func<IDictionary<String^, Object^>^, IContext^>^ get( );
			}

	};

	} }