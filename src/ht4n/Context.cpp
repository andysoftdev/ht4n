/** -*- C++ -*-
 * Copyright (C) 2010-2016 Thalmann Software & Consulting, http://www.softdev.ch
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

#include "stdafx.h"
#include <sstream>

#include "Context.h"
#include "Client.h"
#include "Exception.h"
#include "Logging.h"
#include "AppDomainHandler.h"
#include "CrossAppDomainAction.h"
#include "Composition/IContextProvider.h"
#include "Composition/IContextProviderMetadata.h"
#include "CM2U8.h"

#include "ht4c.Common/Config.h"
#include "ht4c.Common/Properties.h"
#include "ht4c.Common/SessionStateSink.h"
#include "ht4c.Context/Context.h"

namespace Hypertable { 
	using namespace System;
	using namespace System::Globalization;
	using namespace System::Reflection;
	using namespace System::Diagnostics;
#ifndef NETCORE
	using namespace System::ComponentModel::Composition;
	using namespace System::ComponentModel::Composition::Hosting;
	using namespace System::ComponentModel::Composition::Primitives;
#endif
	using namespace System::IO;

	class ShutdownHandler : public AppDomainHandler<ShutdownHandler> {

		public:

			static void finalize( ) {
				ht4c::Context::shutdown();
			}

		private:

			ShutdownHandler( );
			ShutdownHandler( const ShutdownHandler& );
			ShutdownHandler& operator= ( const ShutdownHandler& );
	};

	ShutdownHandler::map_t* ShutdownHandler::appDomains = 0;

	class SessionStateSink : public ht4c::Common::SessionStateSink
																, public CrossAppDomainAction<Action<SessionStateChangedEventArgs^>^, const std::pair<ht4c::Common::SessionState, ht4c::Common::SessionState>*>::Invoker
	{
		public:

			explicit SessionStateSink( ht4c::Context* _ctx, Action<SessionStateChangedEventArgs^>^ _stateChangedAction )
				: ctx( _ctx )
				, stateChangedAction( this, _stateChangedAction )
			{
				ctx->addSessionStateSink( this );
			}

			virtual ~SessionStateSink( ) {
				ctx->removeSessionStateSink( this );
			}

		private:

			SessionStateSink( const SessionStateSink& );
			SessionStateSink& operator= ( const SessionStateSink& );

			virtual void stateTransition( ht4c::Common::SessionState oldSessionState, ht4c::Common::SessionState newSessionState ) {
				std::pair<ht4c::Common::SessionState, ht4c::Common::SessionState> stateChangedEvent = std::make_pair( oldSessionState, newSessionState );
				stateChangedAction.invoke( &stateChangedEvent );
			}

			virtual void invoke( Action<SessionStateChangedEventArgs^>^ action, const std::pair<ht4c::Common::SessionState, ht4c::Common::SessionState>* stateChangedEvent ) {
				action->Invoke( gcnew SessionStateChangedEventArgs(stateChangedEvent->first, stateChangedEvent->second) );
			}

			CrossAppDomainAction<Action<SessionStateChangedEventArgs^>^, const std::pair<ht4c::Common::SessionState, ht4c::Common::SessionState>*> stateChangedAction;
			ht4c::Context* ctx;
	};

	namespace {

		bool prop_insert( ht4c::Common::Properties* prop, String^ name, Object^ obj, bool& unknownType ) {

			#define INSERT_ANY( t, tm )												\
				if( obj->GetType() == tm::typeid ) {						\
					t value = static_cast<t>( obj );							\
					return prop->addOrUpdate( propName, value );	\
				}

			#define INSERT_ANY_COLLECTION( t, tm )												\
				if( dynamic_cast<IEnumerable<tm>^>(obj) ) {									\
					std::vector<t> value;																			\
					for each( tm i in dynamic_cast<IEnumerable<tm>^>(obj) ) {	\
						value.push_back( static_cast<tm>(i) );									\
					}																													\
					return prop->addOrUpdate( propName, value );							\
				}

			unknownType = false;
			if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentNullException(L"Invalid property name", L"name");
			if( obj == nullptr ) throw gcnew ArgumentNullException(L"Invalid property value", L"obj");

			std::string propName = CM2U8(name);

			INSERT_ANY( bool, Boolean )
			INSERT_ANY( uint16_t, UInt16 )
			INSERT_ANY( int32_t, Int32 )
			INSERT_ANY_COLLECTION( int32_t, Int32 )
			INSERT_ANY( int64_t, Int64 )
			INSERT_ANY_COLLECTION( int64_t, Int64 )
			INSERT_ANY( double, Double )
			INSERT_ANY_COLLECTION( double, Double )

			if( obj->GetType() == String::typeid ) {
				std::string value( CM2U8(dynamic_cast<String^>(obj)) );
				return prop->addOrUpdate( propName, value );
			}
			if( dynamic_cast<IEnumerable<String^>^>(obj) ) {
				std::vector<std::string> value;
				for each( String^ str in dynamic_cast<IEnumerable<String^>^>(obj) ) {
					value.push_back( std::string(CM2U8(str)) );
				}
				return prop->addOrUpdate( propName, value );
			}

			unknownType = true;
			return false;

			#undef INSERT_ANY
			#undef INSERT_ANY_COLLECTION
		}

		Object^ prop_get( ht4c::Common::Properties* prop, const std::string& propName ) {

			#define GET_ANY( t )										\
				else if( type == typeid(t) ) {				\
					t value;														\
					if( prop->get(propName, value) ) {	\
						return value;											\
					}																		\
				}

			#define GET_ANY_COLLECTION( t, tm )													\
				else if( type == typeid(std::vector<t>) ) {								\
					std::vector<t> values;																	\
					if( prop->get(propName, values) ) {											\
						IList<tm>^ l = gcnew List<tm>( (int)values.size() );	\
						for each( const t& value in values ) {								\
							l->Add( value );																		\
						}																											\
						return l;																							\
					}																												\
				}

			const type_info& type = prop->type(propName);
			if( type == typeid(void) ) {
				std::stringstream ss;
				ss << "Property '" << propName << "' does not exist\n\tat " << __FUNCTION__ << " (" << __FILE__ << ':' << __LINE__ << ')';
#if _MSC_VER < 1900
				throw std::bad_cast(ss.str().c_str());
#else
				throw std::bad_cast::__construct_from_string_literal( ss.str().c_str() );
#endif
			}
			GET_ANY( bool )
			GET_ANY( uint16_t )
			GET_ANY( int32_t )
			GET_ANY_COLLECTION( int32_t, Int32 )
			GET_ANY( int64_t )
			GET_ANY_COLLECTION( int64_t, Int64 )
			GET_ANY( double )
			GET_ANY_COLLECTION( double, Double )

			else if( type == typeid(std::string) ) {
				std::string value;
				if( prop->get(propName, value) ) {
					return CM2U8::ToString( value.c_str() );
				}
			}
			else if( type == typeid(std::vector<std::string>) ) {
				std::vector<std::string> values;
				if( prop->get(propName, values) ) {
					IList<String^>^ l = gcnew List<String^>( (int)values.size() );
					for each( const std::string& value in values ) {
						l->Add( CM2U8::ToString(value.c_str()) );
					}
					return l;
				}
			}
			std::stringstream ss;
			ss << "Unknown property type for '" << propName << "'\n\tat " << __FUNCTION__ << " (" << __FILE__ << ':' << __LINE__ << ')';
#if _MSC_VER < 1900
			throw std::bad_cast(ss.str().c_str());
#else
			throw std::bad_cast::__construct_from_string_literal( ss.str().c_str() );
#endif

			#undef GET_ANY
			#undef GET_ANY_COLLECTION
		}

	}

	void Context::SessionStateChanged::add( EventHandler<SessionStateChangedEventArgs^>^ eventHandler ) {
		if( eventHandler != nullptr ) {
			{
				msclr::lock sync( syncRoot );
				if( !sessionStateChangedSink ) {
					sessionStateChangedSink = new SessionStateSink( ctx, gcnew Action<SessionStateChangedEventArgs^>( this, &Hypertable::Context::FireSessionStateChanged) );
				}
			}

			sessionStateChanged = static_cast<EventHandler<SessionStateChangedEventArgs^>^>( Delegate::Combine(sessionStateChanged, eventHandler) );
		}
	}

	void Context::SessionStateChanged::remove( EventHandler<SessionStateChangedEventArgs^>^ eventHandler ) {
		if( eventHandler != nullptr ) {
			sessionStateChanged = static_cast<EventHandler<SessionStateChangedEventArgs^>^>( Delegate::Remove(sessionStateChanged, eventHandler) );
		}
	}

	void Context::SessionStateChanged::raise( System::Object^ sender ,Hypertable::SessionStateChangedEventArgs^ eventArgs ) {
		if( sessionStateChanged != nullptr ) {
			sessionStateChanged->Invoke( sender, eventArgs );
		}
	}

	IContext^ Context::Create( String^ connectionString ) {
		if( connectionString == nullptr ) throw gcnew ArgumentNullException( L"connectionString" );
		return CreateProvider( MergeProperties(connectionString, nullptr) );
	}

	IContext^ Context::Create( IDictionary<String^, Object^>^ properties ) {
		if( properties == nullptr ) throw gcnew ArgumentNullException( L"properties" );
		return CreateProvider( MergeProperties(nullptr, properties) );
	}

	IContext^ Context::Create( String^ connectionString, IDictionary<String^, Object^>^ properties ) {
		if( connectionString == nullptr ) throw gcnew ArgumentNullException( L"connectionString" );
		if( properties == nullptr ) throw gcnew ArgumentNullException( L"properties" );
		return CreateProvider( MergeProperties(connectionString, properties) );
	}

	bool Context::HasContext( ContextKind contextKind ) {
		return ht4c::Context::hasContext( static_cast<ht4c::Common::ContextKind>(contextKind) );
	}

	void Context::RegisterProvider( String^ providerName, Func<IDictionary<String^, Object^>^, IContext^>^ provider ) {
		if( String::IsNullOrEmpty(providerName) ) throw gcnew ArgumentNullException( L"providerName" );
		if( provider == nullptr ) throw gcnew ArgumentNullException( L"provider" );
		msclr::lock sync( syncRoot );
		providers[providerName] = provider;
	}

	bool Context::UnregisterProvider( String^ providerName ) {
		if( String::IsNullOrEmpty(providerName) ) throw gcnew ArgumentNullException( L"providerName" );
		msclr::lock sync( syncRoot );
		return providers->Remove( providerName );
	}

	Context::~Context( ) {
		{
			msclr::lock sync( syncRoot );
			if( disposed ) {
				return;
			}
			disposed = true;
		}
		GC::SuppressFinalize(this);
		this->!Context();
	}

	Context::!Context( ) {
		HT4N_TRY {
			msclr::lock sync( syncRoot );
			if( sessionStateChangedSink ) {
				delete sessionStateChangedSink;
				sessionStateChangedSink = 0;
			}
			if( ctx ) {
				delete ctx;
				ctx = 0;
			}
		}
		HT4N_RETHROW
	}

	Hypertable::IClient^ Context::CreateClient() {
		HT4N_THROW_OBJECTDISPOSED( );

		String^ uriName = dynamic_cast<String^>( properties[gcnew String(Common::Config::Uri)] );
		if( String::IsNullOrEmpty(uriName) ) throw gcnew InvalidOperationException( L"Invalid uri property" );

		Uri^ uri = gcnew Uri( uriName );
		if( !uri->IsFile && !uri->IsLoopback ) {
			if(    uri->HostNameType == UriHostNameType::Basic
					|| uri->HostNameType == UriHostNameType::Dns ) {

				// resolve host name, throws an exception on failure
				System::Net::Dns::GetHostEntry( uri->DnsSafeHost );
			}
		}

		return gcnew Hypertable::Client( this );
	}

	bool Context::HasFeature( ContextFeature contextFeature ) {
		HT4N_THROW_OBJECTDISPOSED( );

		return ctx ? ctx->hasFeature( static_cast<ht4c::Common::ContextFeature>(contextFeature) ) : false;
	}

	ht4c::Context* Context::get() {
		return ctx;
	}

	static Context::Context( ) {
		Assembly^ assembly = Assembly::GetAssembly(Context::typeid);
		cli::array<Object^>^ company = assembly->GetCustomAttributes(AssemblyCompanyAttribute::typeid, false);
		cli::array<Object^>^ copyright = assembly->GetCustomAttributes(AssemblyCopyrightAttribute::typeid, false);
		Logging::TraceEvent(
			System::Diagnostics::TraceEventType::Information, 
			String::Format(CultureInfo::InvariantCulture,
										 L"{0} v{1} ({2}, {3})",
										 assembly->GetName()->Name,
										 assembly->GetName()->Version, 
										 ((AssemblyCompanyAttribute^)company[0])->Company,
										 ((AssemblyCopyrightAttribute^)copyright[0])->Copyright));
	}

	Context::Context( IDictionary<String^, Object^>^ _properties )
	: ctx( 0 )
	, properties( _properties )
	, sessionStateChangedSink( 0 )
	, disposed( false )
	{
		if( properties == nullptr ) throw gcnew ArgumentNullException( L"properties" );

		RegisterUnload();
		ht4c::Common::Properties* prop = 0;
		HT4N_TRY {
			Dictionary<String^, Object^>^ remainingProperties;
			prop = From( properties, remainingProperties );
			if( (ctx = ht4c::Context::create(*prop)) == 0 ) {
				throw gcnew ArgumentException( String::Format(CultureInfo::InvariantCulture, L"Unknown provider '{0}'", GetProviderName(properties)), L"properties" );
			}
		}
		HT4N_RETHROW
		finally {
			if( prop ) {
				delete prop;
			}
		}
	}

	void Context::FireSessionStateChanged( SessionStateChangedEventArgs^ eventArgs ) {
		if( eventArgs != nullptr ) {
			SessionStateChanged::raise( this, eventArgs );
		}
	}

	IContext^ Context::CreateProvider( IDictionary<String^, Object^>^ properties ) {
		ValidateUri( properties );

		String^ providerName;
		Func<IDictionary<String^, Object^>^, IContext^>^ provider;
		if( GetProvider(properties, providerName, provider) ) {
			return provider( properties );
		}

		if( false

#ifdef SUPPORT_HYPERTABLE

			|| String::Equals(providerName, gcnew String(Common::Config::ProviderHyper))

#endif

#ifdef SUPPORT_HYPERTABLE_THRIFT

			|| String::Equals(providerName, gcnew String(Common::Config::ProviderThrift))

#endif

#ifdef SUPPORT_HAMSTERDB

			|| String::Equals(providerName, gcnew String(Common::Config::ProviderHamster))

#endif

#ifdef SUPPORT_SQLITEDB

			|| String::Equals(providerName, gcnew String(Common::Config::ProviderSQLite))

#endif

#ifdef SUPPORT_ODBC

			|| String::Equals(providerName, gcnew String(Common::Config::ProviderOdbc))

#endif

			) {

			return gcnew Context( properties );
		}

#ifndef NETCORE

		String^ key = gcnew String( Common::Config::ComposablePartCatalogs );
		if( properties->ContainsKey(key) ) {
			IEnumerable<ComposablePartCatalog^>^ catalogs = dynamic_cast<IEnumerable<ComposablePartCatalog^>^>( properties[key] );
			if( catalogs == nullptr ) {
				ComposablePartCatalog^ catalog = dynamic_cast<ComposablePartCatalog^>( properties[key] );
				if( catalog == nullptr ) {
					throw gcnew ArgumentException( L"Invalid part catalog", L"properties" );
				}
				catalogs = gcnew List<ComposablePartCatalog^>( 1 );
				dynamic_cast<List<ComposablePartCatalog^>^>(catalogs)->Add( catalog );
			}
			AggregateCatalog^ catalog = nullptr;
			try {
				catalog = gcnew AggregateCatalog( catalogs );
				CompositionContainer^ container = nullptr;
				try {
					container = gcnew CompositionContainer( catalog );
					IEnumerable<Lazy<Composition::IContextProvider^, Composition::IContextProviderMetadata^>^>^ providers = container->GetExports<Composition::IContextProvider^, Composition::IContextProviderMetadata^>( );
					for each( Lazy<Composition::IContextProvider^, Composition::IContextProviderMetadata^>^ item in providers ) {
						if( String::IsNullOrEmpty(item->Metadata->ProviderName) ) throw gcnew InvalidOperationException( L"Provider name name must not be empty or null." );
						if( item->Value == nullptr ) throw gcnew InvalidOperationException( L"Context provider must not be null." );

						RegisterProvider( item->Metadata->ProviderName, item->Value->Provider );
					}
				}
				finally {
					if( container != nullptr ) {
						delete container;
					}
				}
			}
			finally {
				if( catalog != nullptr ) {
					delete catalog;
				}
			}
		}

#endif

		if( GetProvider(properties, providerName, provider) ) {
			return provider( properties );
		}

		throw gcnew ArgumentException( String::Format(CultureInfo::InvariantCulture, L"Unknown provider '{0}'", providerName), L"properties" );
	}

	bool Context::GetProvider( IDictionary<String^, Object^>^ properties, String^% providerName, Func<IDictionary<String^, Object^>^, IContext^>^%  provider) {
		provider = nullptr;
		providerName = GetProviderName( properties );
		return providers->TryGetValue( providerName, provider );
	}

	String^ Context::GetProviderName( IDictionary<String^, Object^>^ properties ) {
		if( properties == nullptr ) throw gcnew ArgumentNullException( L"properties" );
		if( !properties->ContainsKey(gcnew String(Common::Config::ProviderName)) ) throw gcnew ArgumentException( L"Missing provider property", L"properties" );
		String^ providerName = dynamic_cast<String^>( properties[gcnew String(Common::Config::ProviderName)] );
		if( String::IsNullOrEmpty(providerName) ) throw gcnew ArgumentException( L"Invalid provider property", L"properties" );
		return providerName;
	}

	void Context::ValidateUri( IDictionary<String^, Object^>^ properties ) {
		if( properties != nullptr && properties->ContainsKey(gcnew String(Common::Config::Uri)) ) {
			String^ uriName = dynamic_cast<String^>( properties[gcnew String(Common::Config::Uri)] );
			if( String::IsNullOrEmpty(uriName) ) throw gcnew ArgumentException( L"Invalid uri property", L"properties" );

			Uri^ uri = gcnew Uri( uriName );
			String^ providerName = GetProviderName( properties );

			if( false

#ifdef SUPPORT_HYPERTABLE

				|| String::Equals(providerName, gcnew String(Common::Config::ProviderHyper))

#endif

#ifdef SUPPORT_HYPERTABLE_THRIFT

				|| String::Equals(providerName, gcnew String(Common::Config::ProviderThrift))

#endif
				) {

				if( uri->IsFile ) {
					throw gcnew ArgumentException( L"Invalid uri scheme, net.tcp://hostname[:port] required", L"properties" );
				}
			}

#ifdef SUPPORT_HAMSTERDB

			else if( String::Equals(providerName, gcnew String(Common::Config::ProviderHamster)) ) {
				if(    !uri->IsFile
						&& !properties->ContainsKey(gcnew String(Common::Config::HamsterFilename)) ) {

					throw gcnew ArgumentException( L"Invalid uri scheme, file:///[drive][/path/]filename required", L"properties" );
				}
			}

#endif

#ifdef SUPPORT_SQLITEDB

			else if( String::Equals(providerName, gcnew String(Common::Config::ProviderSQLite)) ) {
				if(    !uri->IsFile
						&& !properties->ContainsKey(gcnew String(Common::Config::SQLiteFilename)) ) {

					throw gcnew ArgumentException( L"Invalid uri scheme, file:///[drive][/path/]filename required", L"properties" );
				}
			}

#endif

#ifdef SUPPORT_ODBC

			else if( String::Equals(providerName, gcnew String(Common::Config::ProviderOdbc)) ) {
				if( !properties->ContainsKey(gcnew String(Common::Config::OdbcConnectionString)) ) {

					throw gcnew ArgumentException( L"TODO", L"properties" );
				}
			}

#endif

		}
	}

	IDictionary<String^, Object^>^ Context::MergeProperties( String^ connectionString, IDictionary<String^, Object^>^ _properties ) {
		msclr::lock sync( syncRoot );
		try {
			ht4c::Common::Properties* properties = 0;
			HT4N_TRY {
				Dictionary<String^, Object^>^ remainingProperties;
				properties = From( _properties, remainingProperties );
				ht4c::Context::mergeProperties( CM2U8(connectionString), LoggingLevel(), *properties );
				_properties = From( properties );
				for each( KeyValuePair<String^, Object^> kv in remainingProperties ) {
					_properties->Add( kv.Key, kv.Value );
				}
				return _properties;
			}
			HT4N_RETHROW
			finally {
				if( properties ) {
					delete properties;
				}
			}
		}
		catch( HypertableException^ e ) {
			throw gcnew ArgumentException( e->Message, e );
		}
	}

	IDictionary<String^, Object^>^ Context::From( ht4c::Common::Properties* _properties ) {
		Dictionary<String^, Object^>^ properties = gcnew Dictionary<String^, Object^>();
		if( _properties ) {
			std::vector<std::string> names;
			_properties->names( names );
			for each( const std::string& name in names ) {
				properties->Add( CM2U8::ToString(name.c_str()), prop_get(_properties, name) );
			}
		}
		return properties;
	}

	ht4c::Common::Properties* Context::From( IDictionary<String^, Object^>^ _properties, Dictionary<String^, Object^>^% remainingProperties ) {
		remainingProperties = gcnew Dictionary<String^, Object^>();
		HT4N_TRY {
			ht4c::Common::Properties* properties = 0;
			try {
				bool unknownType;
				properties = ht4c::Common::Properties::create();
				if( _properties != nullptr ) {
					for each( KeyValuePair<String^, Object^> item in _properties ) {
						prop_insert( properties, item.Key, item.Value, unknownType );
						if( unknownType ) {
							remainingProperties->Add( item.Key, item.Value );
						}
					}
				}
				return properties;
			}
			catch( ... ) {
				if( properties ) {
					delete properties;
				}
				throw;
			}
		}
		HT4N_RETHROW
	}

	void Context::RegisterUnload( ) {
		msclr::lock sync( syncRoot );
		if( ShutdownHandler::add(0) ) {
			AppDomain::CurrentDomain->DomainUnload += gcnew EventHandler(&Hypertable::Context::Unload);
		}
	}

	const char* Context::LoggingLevel( ) {
		if( Logging::IsEnabled(TraceEventType::Verbose) ) {
			return "info";
		}
		if( Logging::IsEnabled(TraceEventType::Information) ) {
			return "notice";
		}
		if( Logging::IsEnabled(TraceEventType::Warning) ) {
			return "warn";
		}
		if( Logging::IsEnabled(TraceEventType::Error) ) {
			return "error";
		}

		return "crit";
	}

	void Context::Unload( Object^ sender, EventArgs^ ) {
		msclr::lock sync( syncRoot );
		ShutdownHandler::remove( );
	}

}