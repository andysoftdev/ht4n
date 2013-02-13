/** -*- C++ -*-
 * Copyright (C) 2010-2013 Thalmann Software & Consulting, http://www.softdev.ch
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

namespace ht4c { namespace Common {
	class NamespaceListing;
} }

namespace Hypertable {
	using namespace System;
	using namespace System::Collections::Generic;

	interface class INamespace;

	/// <summary>
	/// Represents a Hypertable namespace listing.
	/// </summary>
	/// <example>
	/// The following example shows how to get the entire databse namespace listing.
	/// <code>
	/// using( var ctx = Context.Create("Provider=Hyper; Uri=localhost") ) {
	///    using( var client = ctx.CreateClient() ) {
	///       using( var ns = client.OpenNamespace("/") ) {
	///          var nsListing = ns.GetListing(true);
	///           // use name space listing
	///       }
	///    }
	/// }
	/// 
	/// 
	/// 
	/// </code>
	/// </example>
	/// <seealso cref="INamespace"/>
	[Serializable]
	public ref class NamespaceListing sealed {

		public:

			/// <summary>
			/// Gets the namespace name.
			/// </summary>
			property String^ Name {
				String^ get() {
					return name;
				}
			}

			/// <summary>
			/// Gets the full namespace name.
			/// </summary>
			property String^ FullName {
				String^ get() {
					return fullName;
				}
			}

			/// <summary>
			/// Gets the parent namespace listing or null for the root namespace.
			/// </summary>
			property NamespaceListing^ Parent {
				NamespaceListing^ get() {
					return parent;
				}
			}

			/// <summary>
			/// Gets all sub namespace listings.
			/// </summary>
			property IList<NamespaceListing^>^ Namespaces {
				IList<NamespaceListing^>^ get() {
					return namespaces;
				}
			}

			/// <summary>
			/// Gets all table names.
			/// </summary>
			property IList<String^>^ Tables {
				IList<String^>^ get() {
					return tables;
				}
			}

			/// <summary>
			/// Returns a string that represents the current object.
			/// </summary>
			/// <returns>A string that represents the current object.</returns>
			virtual String^ ToString() override;

		internal:

			NamespaceListing( INamespace^ ns, const ht4c::Common::NamespaceListing& nsListing );

		private:

			NamespaceListing( NamespaceListing^ parent, String^ name );

			static NamespaceListing^ GetListing( NamespaceListing^ parent, const ht4c::Common::NamespaceListing& nsListing );

			String^ name;
			String^ fullName;
			NamespaceListing^ parent;
			List<NamespaceListing^>^ namespaces;
			List<String^>^ tables;
	};

}