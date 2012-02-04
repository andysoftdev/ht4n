/** -*- C++ -*-
 * Copyright (C) 2010-2012 Thalmann Software & Consulting, http://www.softdev.ch
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

#include "ContextFeature.h"

namespace Hypertable {
	using namespace System;
	using namespace System::Collections::Generic;

	interface class IClient;

	/// <summary>
	/// Represents a Hypertable context, handles the connection to the storage provider instance.
	/// </summary>
	/// <remarks>
	/// Default initialization of the Hypertable context uses the configuration (conf/hypertable.cfg) of
	/// the executing process if exist.
	/// </remarks>
	/// <example>
	/// The following example shows how to create a Hypertable context.
	/// <code>
	/// using( var ctx = Context.Create("Provider=Hyper; Uri=localhost") ) {
	///    using( var client = ctx.CreateClient() ) {
	///       // use client
	///    }
	/// }
	/// </code>
	/// The following example shows how to create a Hypertable context using configuration properties.
	/// <code>
	/// IDictionary&lt;string, object&gt; properties = new Dictionary&lt;string, object&gt;();
	/// properties.Add("Hypertable.Logging.Level", "info");
	/// using( var ctx = Context.Create("Provider=Hyper; Uri=localhost", properties) ) {
	///    using( var client = ctx.CreateClient() ) {
	///       // use client
	///    }
	/// }
	/// </code>
	/// The following example shows how to create a Hypertable context using an external configuration file.
	/// <code>
	/// using( var ctx = Context.Create("Provider=Hyper; Uri=localhost; config=TestConfiguration.cfg") ) {
	///    using( var client = ctx.CreateClient() ) {
	///       // use client
	///    }
	/// }
	/// </code>
	/// The following example shows how to access the configuration properties.
	/// <code>
	/// using( var ctx = Context.Create("Provider=Thrift; Uri=localhost") ) {
	///    int thriftBrokerTimeout = (int)ctx.Properties["ThriftBroker.Timeout"];
	/// }
	/// </code>
	/// </example>
	public interface class IContext : public IDisposable {

		public:

			/// <summary>
			/// Gets the configuration properties.
			/// </summary>
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
			property IDictionary<String^, Object^>^ Properties {
				IDictionary<String^, Object^>^ get();
			}

			/// <summary>
			/// Gets a value indicating whether the object has been disposed.
			/// </summary>
			/// <remarks>true if the object has been disposed, otherwise false.</remarks>
			property bool IsDisposed {
				bool get( );
			}

			/// <summary>
			/// Creates a new Client instance.
			/// </summary>
			/// <returns>New Client instance.</returns>
			/// <seealso cref="IClient"/>
			Hypertable::IClient^ CreateClient( );

			/// <summary>
			/// Returns true if the actual provider supports the feature specified, otherwise false.
			/// </summary>
			/// <param name="contextFeature">Context feature.</param>
			/// <returns>true if the actual provider supports the feature specified, otherwise false.</returns>
			/// <seealso cref="ContextFeature"/>
			bool HasFeature( ContextFeature contextFeature );
	};

}