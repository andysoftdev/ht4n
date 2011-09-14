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

#include "ContextKind.h"
#include "ht4c.Common/Types.h"

namespace ht4c {
	class Context;
}

namespace Hypertable {
	using namespace System;
	using namespace System::Collections::Generic;

	ref class Client;

	/// <summary>
	/// Represents a Hypertable context, handles connection to a Hypertable instance.
	/// </summary>
	/// <remarks>
	/// Default initialization of the Hypertable context uses the configuration (conf/hypertable.cfg) of
	/// the executing process if exist.
	/// </remarks>
	/// <example>
	/// The following example shows how to create a Hypertable context.
	/// <code>
	/// using( Context ctx = Context.Create(ContextKind.Hyper, "localhost") ) {
	///    using( Client client = ctx.CreateClient() ) {
	///       // use client
	///    }
	/// }
	/// </code>
	/// The following example shows how to create a Hypertable context using configuration properties.
	/// <code>
	/// IDictionary&lt;string, object&gt; properties = new Dictionary&lt;string, object&gt;();
	/// properties.Add("Hypertable.Logging.Level", "info");
	/// using( Context ctx = Context.Create(ContextKind.Hyper, "localhost", properties) ) {
	///    using( Client client = ctx.CreateClient() ) {
	///       // use client
	///    }
	/// }
	/// </code>
	/// The following example shows how to create a Hypertable context using an external configuration file.
	/// <code>
	/// using( Context ctx = Context.Create(ContextKind.Hyper, "--config TestConfiguration.cfg", false) ) {
	///    using( Client client = ctx.CreateClient() ) {
	///       // use client
	///    }
	/// }
	/// </code>
	/// The following example shows how to access the configuration properties.
	/// <code>
	/// using( Context ctx = Context.Create(ContextKind.Thrift, "localhost") ) {
	///    int thriftBrokerTimeout = (int)ctx.Properties["ThriftBroker.Timeout"];
	/// }
	/// </code>
	/// </example>
	public ref class Context sealed : public IDisposable {

		public:

			/// <summary>
			/// Gets the context kind.
			/// </summary>
			/// <seealso cref="ContextKind"/>
			property Hypertable::ContextKind ContextKind {
				Hypertable::ContextKind get();
			}

			/// <summary>
			/// Gets the configuration properties.
			/// </summary>
			/// <remarks>
			/// Property values support following types:
			/// <table class="comment">
			/// <tr><td>string</td><td>IList&lt;string&gt;</td></tr>
			/// <tr><td>int</td><td>IList&lt;int&gt;</td></tr>
			/// <tr><td>long</td><td>IList&lt;long&gt;</td></tr>
			/// <tr><td>ushort</td></tr>
			/// <tr><td>double</td><td>IList&lt;double&gt;</td></tr>
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
				bool get( ) {
					return disposed;
				}
			}

			/// <summary>
			/// Creates a new Context instance.
			/// </summary>
			/// <param name="ctxKind">Context kind.</param>
			/// <returns>New Context instance.</returns>
			/// <seealso cref="ContextKind"/>
			static Context^ Create( Hypertable::ContextKind ctxKind );

			/// <summary>
			/// Creates a new Context instance using specified configuration properties.
			/// </summary>
			/// <param name="ctxKind">Context kind.</param>
			/// <param name="properties">Configuration properties.</param>
			/// <returns>New Context instance.</returns>
			/// <remarks>
			/// Property values support following types:
			/// <table class="comment">
			/// <tr><td>string</td><td>IList&lt;string&gt;</td></tr>
			/// <tr><td>int</td><td>IList&lt;int&gt;</td></tr>
			/// <tr><td>long</td><td>IList&lt;long&gt;</td></tr>
			/// <tr><td>ushort</td></tr>
			/// <tr><td>double</td><td>IList&lt;double&gt;</td></tr>
			/// <tr><td>bool</td></tr>
			/// </table>
			/// </remarks>
			/// <seealso cref="ContextKind"/>
			static Context^ Create( Hypertable::ContextKind ctxKind, IDictionary<String^, Object^>^ properties );

			/// <summary>
			/// Creates a new Context instance using specified Hypertable host.
			/// </summary>
			/// <param name="ctxKind">Context kind.</param>
			/// <param name="host">Host name.</param>
			/// <returns>New Context instance.</returns>
			/// <remarks>
			/// The host parameter specifies the Hyperspace host or the ThriftBroker host depending on
			/// the ctxKind parameter.
			/// </remarks>
			/// <seealso cref="ContextKind"/>
			static Context^ Create( Hypertable::ContextKind ctxKind, String^ host );

			/// <summary>
			/// Creates a new Context instance using specified Hypertable host and port.
			/// </summary>
			/// <param name="ctxKind">Context kind.</param>
			/// <param name="host">Host name.</param>
			/// <param name="port">Port number.</param>
			/// <returns>New Context instance.</returns>
			/// <remarks>
			/// The host and port parameters specify the Hyperspace host:port or the ThriftBroker host:port
			/// depending on the ctxKind parameter.
			/// </remarks>
			/// <seealso cref="ContextKind"/>
			static Context^ Create( Hypertable::ContextKind ctxKind, String^ host, uint16_t port );

			/// <summary>
			/// Creates a new Context instance using specified Hypertable host and configuration properties.
			/// </summary>
			/// <param name="ctxKind">Context kind.</param>
			/// <param name="host">Host name.</param>
			/// <param name="properties">Configuration properties.</param>
			/// <returns>New Context instance.</returns>
			/// <remarks>
			/// The host parameter specifies the Hyperspace host or the ThriftBroker host depending on
			/// the ctxKind parameter.<br/><br/>
			/// Property values support following types:
			/// <table class="comment">
			/// <tr><td>string</td><td>IList&lt;string&gt;</td></tr>
			/// <tr><td>int</td><td>IList&lt;int&gt;</td></tr>
			/// <tr><td>long</td><td>IList&lt;long&gt;</td></tr>
			/// <tr><td>ushort</td></tr>
			/// <tr><td>double</td><td>IList&lt;double&gt;</td></tr>
			/// <tr><td>bool</td></tr>
			/// </table>
			/// </remarks>
			/// <seealso cref="ContextKind"/>
			static Context^ Create( Hypertable::ContextKind ctxKind, String^ host, IDictionary<String^, Object^>^ properties );

			/// <summary>
			/// Creates a new Context instance using specified Hypertable host, port and configuration properties.
			/// </summary>
			/// <param name="ctxKind">Context kind.</param>
			/// <param name="host">Host name.</param>
			/// <param name="port">Port number.</param>
			/// <param name="properties">Configuration properties.</param>
			/// <returns>New Context instance.</returns>
			/// <remarks>
			/// The host and port parameters specify the Hyperspace host:port or the ThriftBroker host:port depending
			/// on the ctxKind.<br/><br/>
			/// Property values support following types:
			/// <table class="comment">
			/// <tr><td>string</td><td>IList&lt;string&gt;</td></tr>
			/// <tr><td>int</td><td>IList&lt;int&gt;</td></tr>
			/// <tr><td>long</td><td>IList&lt;long&gt;</td></tr>
			/// <tr><td>ushort</td></tr>
			/// <tr><td>double</td><td>IList&lt;double&gt;</td></tr>
			/// <tr><td>bool</td></tr>
			/// </table>
			/// </remarks>
			/// <seealso cref="ContextKind"/>
			static Context^ Create( Hypertable::ContextKind ctxKind, String^ host, uint16_t port, IDictionary<String^, Object^>^ properties );

			/// <summary>
			/// Creates a new Context instance using command line arguments.
			/// </summary>
			/// <param name="ctxKind">Context kind.</param>
			/// <param name="commandLine">Command line arguments.</param>
			/// <param name="includesModuleFileName">If true the commandLine parameter contains the module filename.</param>
			/// <returns>New Context instance.</returns>
			/// <seealso cref="ContextKind"/>
			static Context^ Create( Hypertable::ContextKind ctxKind, String^ commandLine, bool includesModuleFileName );

			/// <summary>
			/// Clean up all managed and unmanaged resources.
			/// </summary>
			virtual ~Context( );

			/// <summary>
			/// Clean up all unmanaged resources.
			/// </summary>
			!Context( );

			/// <summary>
			/// Creates a new Client instance.
			/// </summary>
			/// <returns>New Client instance.</returns>
			Hypertable::Client^ CreateClient( );

		internal:

			ht4c::Context* get();

		private:

			Context(  Hypertable::ContextKind ctxKind, String^ host, UInt16 port, IDictionary<String^, Object^>^ properties );
			Context( Hypertable::ContextKind ctxKind, String^ commandLine, bool includesModuleFileName );

			static void RegisterUnload( );
			static void Unload( Object^, EventArgs^ );

			ht4c::Context* ctx;
			IDictionary<String^, Object^>^ properties;
			static Object^ syncRoot = gcnew Object();
			bool disposed;
	};

}