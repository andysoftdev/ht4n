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

#include "stdafx.h"
#include <sstream>

#include "Context.h"
#include "Client.h"
#include "Exception.h"
#include "Logging.h"
#include "AppDomainHandler.h"
#include "CM2A.h"

#include "ht4c.Common/Properties.h"
#include "ht4c.Context/Context.h"

namespace Hypertable { 
	using namespace System;
	using namespace System::Globalization;
	using namespace System::Reflection;
	using namespace System::Diagnostics;

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

	bool prop_insert( ht4c::Common::Properties* prop, String^ name, Object^ obj ) {

		#define INSERT_ANY( t, tm )											\
			if( obj->GetType() == tm::typeid ) {					\
				t value = static_cast<t>( obj );						\
				return prop->insert( propName, value );			\
			}

		#define INSERT_ANY_COLLECTION( t, tm )												\
			if( dynamic_cast<IEnumerable<tm>^>(obj) ) {									\
				std::vector<t> value;																			\
				for each( tm i in dynamic_cast<IEnumerable<tm>^>(obj) ) {	\
					value.push_back( static_cast<tm>(i) );									\
				}																													\
				return prop->insert( propName, value );										\
			}
		if( String::IsNullOrEmpty(name) ) throw gcnew ArgumentNullException(L"Invalid property name", L"name");
		if( obj == nullptr ) throw gcnew ArgumentNullException(L"Invalid property value", L"obj");

		std::string propName = CM2A(name);

		INSERT_ANY( bool, Boolean )
		INSERT_ANY( uint16_t, UInt16 )
		INSERT_ANY( int32_t, Int32 )
		INSERT_ANY_COLLECTION( int32_t, Int32 )
		INSERT_ANY( int64_t, Int64 )
		INSERT_ANY_COLLECTION( int64_t, Int64 )
		INSERT_ANY( double, Double )
		INSERT_ANY_COLLECTION( double, Double )

		if( obj->GetType() == String::typeid ) {
			std::string value( CM2A(dynamic_cast<String^>(obj)) );
			return prop->insert( propName, value );
		}
		if( dynamic_cast<IEnumerable<String^>^>(obj) ) {
			std::vector<std::string> value;
			for each( String^ str in dynamic_cast<IEnumerable<String^>^>(obj) ) {
				value.push_back( std::string(CM2A(str)) );
			}
			return prop->insert( propName, value );
		}
		std::stringstream ss;
		ss << "Invalid property value for '" << propName << "'";
		throw std::bad_cast( ss.str().c_str() );

		#undef INSERT_ANY
		#undef INSERT_ANY_COLLECTION
	}

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

	Object^ prop_get( ht4c::Common::Properties* prop, const std::string& propName ) {
		const type_info& type = prop->type(propName);
		if( type == typeid(void) ) {
			std::stringstream ss;
			ss << "Property '" << propName << "' does not exist";
			throw std::bad_cast( ss.str().c_str() );
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
				return gcnew String( value.c_str() );
			}
		}
		else if( type == typeid(std::vector<std::string>) ) {
			std::vector<std::string> values;
			if( prop->get(propName, values) ) {
				IList<String^>^ l = gcnew List<String^>( (int)values.size() );
				for each( const std::string& value in values ) {
					l->Add( gcnew String(value.c_str()) );
				}
				return l;
			}
		}
		std::stringstream ss;
		ss << "Unknown property type for '" << propName << "'";
		throw std::bad_cast( ss.str().c_str() );
	}

	Hypertable::ContextKind Context::ContextKind::get() {
		HT4C_TRY {
			return (Hypertable::ContextKind)ctx->getContextKind();
		}
		HT4C_RETHROW
	}

	IDictionary<String^, Object^>^ Context::Properties::get() {
		ht4c::Common::Properties* prop = 0;
		HT4C_TRY {
			if( properties == nullptr ) {
				prop = ht4c::Common::Properties::create();
				ctx->getProperties( *prop );
				properties = gcnew Dictionary<String^, Object^>();
				std::vector<std::string> names;
				prop->names( names );
				for each( const std::string& name in names ) {
					properties->Add( gcnew String(name.c_str()), prop_get(prop, name) );
				}
			}
			return properties;
		}
		HT4C_RETHROW
		finally {
			if( prop ) {
				delete prop;
			}
		}
	}

	Context^ Context::Create( Hypertable::ContextKind ctxKind ) {
		return Create( ctxKind, nullptr, 0, nullptr );
	}

	Context^ Context::Create( Hypertable::ContextKind ctxKind, IDictionary<String^, Object^>^ properties ) {
		return Create( ctxKind, nullptr, 0, properties );
	}

	Context^ Context::Create( Hypertable::ContextKind ctxKind, String^ host ) {
		return Create( ctxKind, host, 0, nullptr );
	}

	Context^ Context::Create( Hypertable::ContextKind ctxKind, String^ host, uint16_t port ) {
		return Create( ctxKind, host, port, nullptr );
	}

	Context^ Context::Create( Hypertable::ContextKind ctxKind, String^ host, IDictionary<String^, Object^>^ properties ) {
		return Create( ctxKind, host, 0, properties );
	}

	Context^ Context::Create( Hypertable::ContextKind ctxKind, String^ host, uint16_t port, IDictionary<String^, Object^>^ properties ) {
		return gcnew Context( ctxKind, host, port, properties );
	}

	Context^ Context::Create( Hypertable::ContextKind ctxKind, String^ commandLine, bool includesModuleFileName ) {
		return gcnew Context( ctxKind, commandLine, includesModuleFileName );
	}

	Context::~Context( ) {
		disposed = true;
		this->!Context();
		GC::SuppressFinalize(this);
	}

	Context::!Context( ) {
		HT4C_TRY {
			delete ctx;
			ctx = 0;
		}
		HT4C_RETHROW
	}

	Hypertable::Client^ Context::CreateClient() {
		return gcnew Hypertable::Client( this );
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

	Context::Context( Hypertable::ContextKind ctxKind, String^ host, uint16_t port, IDictionary<String^, Object^>^ properties )
	: ctx( 0 )
	, disposed( false )
	{
		if( !String::IsNullOrEmpty(host) ) {
			UriHostNameType uriHostNameType = Uri::CheckHostName( host );
			if( uriHostNameType == UriHostNameType::Unknown ) {
				throw gcnew ArgumentException(L"Invalid host name", L"host");
			}
			else if(   uriHostNameType == UriHostNameType::Basic
							|| uriHostNameType == UriHostNameType::Dns ) {

				System::Net::Dns::GetHostEntry( host ); // resolve host name
			}
		}

		RegisterUnload();

		ht4c::Common::Properties* prop = 0;
		HT4C_TRY {
			prop = ht4c::Common::Properties::create();
			if( properties != nullptr ) {
				for each( KeyValuePair<String^, Object^> item in properties ) {
					prop_insert( prop, item.Key, item.Value );
				}
			}

			ctx = ht4c::Context::create( (ht4c::Common::ContextKind)ctxKind, CM2A(host), port, *prop, LoggingLevel() );
		}
		HT4C_RETHROW
		finally {
			if( prop ) {
				delete prop;
			}
		}
	}

	Context::Context( Hypertable::ContextKind ctxKind, String^ commandLine, bool includesModuleFileName )
	: ctx( 0 )
	, disposed( false )
	{
		RegisterUnload();
		HT4C_TRY {
			ctx = ht4c::Context::create( (ht4c::Common::ContextKind)ctxKind, CM2A(commandLine), includesModuleFileName, LoggingLevel() );
		}
		HT4C_RETHROW
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