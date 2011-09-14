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

#include "Logging.h"
#include "Exception.h"
#include "AppDomainHandler.h"
#include "CrossAppDomainAction.h"
#include "CM2A.h"

#include "ht4c.Context/Logging.h"

namespace Hypertable { 
	using namespace System;

	class LoggingSink : public AppDomainHandler<LoggingSink>
										, public ht4c::LoggingSink
										, public CrossAppDomainAction<Action<String^>^, const char*>::Invoker
	{
		public:

			static bool add( Action<String^>^ action ) {
				return AppDomainHandler<LoggingSink>::add( new LoggingSink(action) );
			}

			static void finalize( ) {
			}

			virtual ~LoggingSink( ) {
				ht4c::Logging::removeLogsink( this );
			}

		private:

			explicit LoggingSink( Action<String^>^ _action )
				: action( this, _action )
			{
				ht4c::Logging::addLogsink( this );
			}

			LoggingSink( const LoggingSink& );
			LoggingSink& operator= ( const LoggingSink& );

			virtual void logEvent( const std::string& log ) {
				action.invoke( log.c_str() );
			}

			virtual void invoke( Action<String^>^ action, const char* log ) {
				action->Invoke( gcnew String(log) );
			}

			CrossAppDomainAction<Action<String^>^, const char*> action;
	};

	LoggingSink::map_t* LoggingSink::appDomains = 0;

	String^ Logging::Logfile::get( ) {
		return gcnew String( ht4c::Logging::getLogfile().c_str() );
	}

	void Logging::Logfile::set( String^ logfile ) {
		HT4C_TRY {
			ht4c::Logging::setLogfile( CM2A(logfile) );
		}
		HT4C_RETHROW
	}

	void Logging::LogEntryPublished::add( Action<String^>^ _action ) {
		if( _action != nullptr ) {
			if( action == nullptr ) {
				msclr::lock sync( syncRoot );
				if( LoggingSink::add(gcnew Action<String^>(&Hypertable::Logging::LogEntryPublished::raise)) ) {
					AppDomain::CurrentDomain->DomainUnload += gcnew EventHandler(&Hypertable::Logging::Unload);
				}
			}
			action = static_cast<Action<String^>^>( Delegate::Combine(action, _action) );
		}
	}

	void Logging::LogEntryPublished::remove( Action<String^>^ _action ) {
		action = static_cast<Action<String^>^>( Delegate::Remove(action, _action) );
	}

	void Logging::LogEntryPublished::raise( String^ log ) {
		if( action != nullptr ) {
			action->Invoke( log );
		}
	}

	void Logging::Unload( Object^ sender, EventArgs^ ) {
		msclr::lock sync( syncRoot );
		LoggingSink::remove( );
	}

}