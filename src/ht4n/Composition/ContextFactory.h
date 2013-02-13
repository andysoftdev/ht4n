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

#include "IContextFactory.h"

namespace Hypertable {

	interface class IContext;

	namespace Composition {

	using namespace System;
	using namespace System::ComponentModel::Composition;
	using namespace System::Collections::Generic;

	/// <summary>
	/// Represents a Hypertable context factory.
	/// </summary>
	/// <seealso cref="IContextFactory"/>
	[Export(IContextFactory::typeid)]
	public ref class ContextFactory sealed : public IContextFactory {

		public:

			/// <summary>
			/// Initializes a new instance of the ContextFactory class.
			/// </summary>
			ContextFactory( ) { }

			#pragma region IContextFactory methods

			/// <summary>
			/// Creates a new Context instance using specified connection string.
			/// </summary>
			/// <param name="connectionString">Connection string.</param>
			/// <returns>New Context instance.</returns>
			virtual Hypertable::IContext^ Create( String^ connectionString );

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
			virtual Hypertable::IContext^ Create( IDictionary<String^, Object^>^ properties );

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
			virtual Hypertable::IContext^ Create( String^ connectionString, IDictionary<String^, Object^>^ properties );

			#pragma endregion
	};

	} }