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

#include "IContext.h"
#include "ht4c.Common/Types.h"

namespace ht4c {
	class Context;
}

namespace ht4c { namespace Common {
	class Properties;
} }

namespace Hypertable {
	using namespace System;
	using namespace System::Runtime::InteropServices;
	using namespace System::Collections::Generic;

	interface class IClient;

	/// <summary>
	/// Represents a Hypertable context factory.
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
	/// <seealso cref="IContext"/>
	public ref class Context sealed : public IContext {

		public:

			#pragma region IContext properties

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
				virtual IDictionary<String^, Object^>^ get() {
					return properties;
				}
			}

			/// <summary>
			/// Gets a value indicating whether the object has been disposed.
			/// </summary>
			/// <remarks>true if the object has been disposed, otherwise false.</remarks>
			property bool IsDisposed {
				virtual bool get( ) {
					return disposed;
				}
			}

			#pragma endregion

			/// <summary>
			/// Creates a new Context instance using specified connection string.
			/// </summary>
			/// <param name="connectionString">Connection string.</param>
			/// <returns>New Context instance.</returns>
			static IContext^ Create( String^ connectionString );

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
			static IContext^ Create( IDictionary<String^, Object^>^ properties );

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
			static IContext^ Create( String^ connectionString, IDictionary<String^, Object^>^ properties );

			/// <summary>
			/// Registers a custom context provider.
			/// </summary>
			/// <param name="providerName">Provider name.</param>
			/// <param name="provider">Provider function.</param>
			static void RegisterProvider( String^ providerName, Func<IDictionary<String^, Object^>^, IContext^>^ provider );

			/// <summary>
			/// Unregisters a custom context provider.
			/// </summary>
			/// <param name="providerName">Provider name.</param>
			/// <returns>true if the provoder with the name specified has been removed, otherwise false.</returns>
			static bool UnregisterProvider( String^ providerName );

			#pragma region IContext methods

			/// <summary>
			/// Creates a new Client instance.
			/// </summary>
			/// <returns>New Client instance.</returns>
			/// <seealso cref="IClient"/>
			virtual Hypertable::IClient^ CreateClient( );

			/// <summary>
			/// Returns true if the actual provider supports the feature specified, otherwise false.
			/// </summary>
			/// <param name="contextFeature">Context feature.</param>
			/// <returns>true if the actual provider supports the feature specified, otherwise false.</returns>
			/// <seealso cref="ContextFeature"/>
			virtual bool HasFeature( ContextFeature contextFeature );

			#pragma endregion

			/// <summary>
			/// Clean up all managed and unmanaged resources.
			/// </summary>
			virtual ~Context( );

			/// <summary>
			/// Clean up all unmanaged resources.
			/// </summary>
			!Context( );

	internal:

			ht4c::Context* get();

		private:

			static Context( );
			Context( IDictionary<String^, Object^>^ properties );

			static IContext^ CreateProvider( IDictionary<String^, Object^>^ properties );
			static bool GetProvider( IDictionary<String^, Object^>^ properties, [Out] String^% providerName, [Out] Func<IDictionary<String^, Object^>^, IContext^>^%  provider );
			static String^ GetProviderName( IDictionary<String^, Object^>^ properties );
			static void ValidateUri( IDictionary<String^, Object^>^ properties );
			static IDictionary<String^, Object^>^ MergeProperties( String^ connectionString, IDictionary<String^, Object^>^ properties );
			static IDictionary<String^, Object^>^ From( ht4c::Common::Properties* properties );
			static ht4c::Common::Properties* From( IDictionary<String^, Object^>^ properties, [Out] Dictionary<String^, Object^>^% remainingProperties );
			static void RegisterUnload( );
			static const char* LoggingLevel( );
			static void Unload( Object^, EventArgs^ );


			ht4c::Context* ctx;
			IDictionary<String^, Object^>^ properties;
			bool disposed;

			static Dictionary<String^, Func<IDictionary<String^, Object^>^, IContext^>^>^ providers = gcnew Dictionary<String^, Func<IDictionary<String^, Object^>^, IContext^>^>();
			static Object^ syncRoot = gcnew Object();
	};

}