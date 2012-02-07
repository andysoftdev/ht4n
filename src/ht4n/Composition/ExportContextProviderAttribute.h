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

#include "IContextProviderMetadata.h"

namespace Hypertable { namespace Composition {
	using namespace System;
	using namespace System::ComponentModel::Composition;

	interface class IContextProvider;

	/// <summary>
	/// Specifies a particular context provider export.
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
	///          { "Hypertable.Composition.ComposablePartCatalogs", new AssemblyCatalog(Assembly.GetExecutingAssembly()) }
	///       };
	///    using (var context = Context.Create("Provider=SampleContextProvider", properties)) {
	///       // use the context
	///    }
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="IContextProvider"/>
	/// <seealso cref="IContextProviderMetadata"/>
	[MetadataAttribute]
	[AttributeUsageAttribute(AttributeTargets::Class, AllowMultiple = false)]
	public ref class ExportContextProviderAttribute sealed : public ExportAttribute, public IContextProviderMetadata {

		public:

			/// <summary>
			/// Initializes a new instance of the ExportContextProviderAttribute class.
			/// </summary>
			/// <param name="providerName">Context provider name.</param>
			ExportContextProviderAttribute( String^ providerName );

			/// <summary>
			/// Gets the context provider name.
			/// </summary>
			property String^ ProviderName {
				virtual String^ get( ) {
					return providerName;
				}
			}

		private:

			String^ providerName;
	};

	} }