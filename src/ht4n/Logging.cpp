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

#include "stdafx.h"

#include "Logging.h"
#include "Exception.h"
#include "AppDomainHandler.h"
#include "CrossAppDomainAction.h"
#include "CM2U8.h"

#include "ht4c.Context/Logging.h"

namespace Hypertable { 
	using namespace System;
	using namespace System::Linq;
	using namespace System::Diagnostics;
	using namespace System::Collections::Generic;

	class LoggingSink : public AppDomainHandler<LoggingSink>
										, public ht4c::LoggingSink
										, public CrossAppDomainAction<Action<TraceEventType, String^>^, const std::pair<int, const char*>*>::Invoker
										, public CrossAppDomainAction<Action<String^>^, const char*>::Invoker
	{
		public:

			static bool add( Action<TraceEventType, String^>^ logEventAction, Action<String^>^ logMessageAction ) {
				return AppDomainHandler<LoggingSink>::add( new LoggingSink(logEventAction, logMessageAction) );
			}

			static void finalize( ) {
			}

			virtual ~LoggingSink( ) {
				ht4c::Logging::removeLoggingSink( this );
			}

		private:

			explicit LoggingSink( Action<TraceEventType, String^>^ _logEventAction, Action<String^>^ _logMessageAction )
				: logEventAction( this, _logEventAction )
				, logMessageAction( this, _logMessageAction )
			{
				ht4c::Logging::addLoggingSink( this );
			}

			LoggingSink( const LoggingSink& );
			LoggingSink& operator= ( const LoggingSink& );

			virtual void logEvent( int priority, const std::string& message ) {
				std::pair<int, const char*> logEvent = std::make_pair( priority, message.c_str() );
				logEventAction.invoke( &logEvent );
			}

			virtual void invoke( Action<TraceEventType, String^>^ action, const std::pair<int, const char*>* logEvent ) {

				enum {
					Critical    = 200,
					Error       = 300,
					Warning     = 400,
					Notice      = 500,
				};

				TraceEventType traceEventType = TraceEventType::Verbose;
				if( logEvent-> first <= Critical ) {
					traceEventType = TraceEventType::Critical;
				}
				else if( logEvent-> first <= Error ) {
					traceEventType = TraceEventType::Error;
				}
				else if( logEvent-> first <= Warning ) {
					traceEventType = TraceEventType::Warning;
				}
				else if( logEvent-> first <= Notice ) {
					traceEventType = TraceEventType::Information;
				}

				action->Invoke( traceEventType, CM2U8::ToString(logEvent->second) );
			}

			virtual void logMessage( const std::string& message ) {
				logMessageAction.invoke( message.c_str() );
			}

			virtual void invoke( Action<String^>^ action, const char* message ) {
				action->Invoke( CM2U8::ToString(message) );
			}

			CrossAppDomainAction<Action<TraceEventType, String^>^, const std::pair<int, const char*>*> logEventAction;
			CrossAppDomainAction<Action<String^>^, const char*> logMessageAction;
	};

	LoggingSink::map_t* LoggingSink::appDomains = 0;

	System::Diagnostics::TraceSource^ Logging::TraceSource::get( ) {
		msclr::lock sync( syncRoot );
		return GetTraceSource();
	}

	void Logging::TraceSource::set( System::Diagnostics::TraceSource^ _traceSource ) {
		if( _traceSource == nullptr ) throw gcnew ArgumentNullException( L"traceSource" );
		msclr::lock sync( syncRoot );
		traceSource = _traceSource;
		SetupLoggingSink();
	}

	Action<String^>^ Logging::LogMessagePublished::get( ) {
		msclr::lock sync( syncRoot );
		return logMessagePublished;
	}

	void Logging::LogMessagePublished::set( Action<String^>^ _logMessagePublished ) {
		msclr::lock sync( syncRoot );
		if( _logMessagePublished != nullptr ) {
			SetupLoggingSink();
		}
		logMessagePublished = _logMessagePublished;
	}

	String^ Logging::Logfile::get( ) {
		HT4N_TRY {
			msclr::lock sync( syncRoot );
			return CM2U8::ToString( ht4c::Logging::getLogfile().c_str() );
		}
		HT4N_RETHROW
	}

	void Logging::Logfile::set( String^ logfile ) {
		HT4N_TRY {
			msclr::lock sync( syncRoot );
			ht4c::Logging::setLogfile( CM2U8(logfile) );
		}
		HT4N_RETHROW
	}

	bool Logging::IsEnabled( System::Diagnostics::TraceEventType traceEventType ) {
		msclr::lock sync( syncRoot );
		System::Diagnostics::TraceSource^ ts = GetTraceSource();
		if( ts != nullptr && ts->Switch != nullptr ) {
			try {
				return ts->Switch->ShouldTrace( traceEventType );
			}
			catch( Object^ ) {
			}
		}

		return false;
	}

	void Logging::TraceEvent( System::Diagnostics::TraceEventType traceEventType, String^ message ) {
		if( message != nullptr ) {
			msclr::lock sync( syncRoot );
			System::Diagnostics::TraceSource^ ts = GetTraceSource();
			if( ts != nullptr && ts->Switch != nullptr ) {
				try {
					if( ts->Switch->ShouldTrace(traceEventType) ) {
						ts->TraceEvent( traceEventType, 0, message );
					}
				}
				catch( Object^ ) {
				}
			}
		}
	}

	void Logging::TraceEvent( System::Diagnostics::TraceEventType traceEventType, Func<String^>^ func ) {
		if( func != nullptr ) {
			msclr::lock sync( syncRoot );
			System::Diagnostics::TraceSource^ ts = GetTraceSource();
			if( ts != nullptr && ts->Switch != nullptr ) {
				try {
					if( ts->Switch->ShouldTrace(traceEventType) ) {
						ts->TraceEvent( traceEventType, 0, func() );
					}
				}
				catch( Object^ ) {
				}
			}
		}
	}

	void Logging::TraceException( Exception^ exception ) {
		if( exception != nullptr ) {
			msclr::lock sync( syncRoot );
			System::Diagnostics::TraceSource^ ts = GetTraceSource();
			if( ts != nullptr && ts->Switch != nullptr ) {
				try {
					if( ts->Switch->ShouldTrace(TraceEventType::Error) ) {
						ts->TraceEvent( TraceEventType::Error, 0, exception->ToString() );
					}
				}
				catch( Object^ ) {
				}
			}
		}
	}

	static Logging::Logging( ) {
		SetupLoggingSink();
	}

	System::Diagnostics::TraceSource^ Logging::GetTraceSource( ) {
		if( traceSource == nullptr ) {
			traceSource = gcnew System::Diagnostics::TraceSource( L"ht4n", System::Diagnostics::SourceLevels::All );

#ifdef _DEBUG

			List<TraceListener^>^ listeners = gcnew List<TraceListener^>();
			listeners->AddRange(Enumerable::Cast<TraceListener^>(Trace::Listeners));
			listeners->AddRange(Enumerable::Cast<TraceListener^>(Debug::Listeners));

			HashSet<String^> names = gcnew HashSet<String^>();
			for each( TraceListener^ listener in traceSource->Listeners ) {
				names.Add( listener->Name );
			}

			for each( TraceListener^ listener in listeners ) {
				if( !names.Contains(listener->Name) ) {
					traceSource->Listeners->Add(listener);
					names.Add( listener->Name );
				}
			}

#endif

			SetupLoggingSink();
		}

		return traceSource;
	}

	void Logging::PublishLogMessage( String^ message ) {
		msclr::lock sync( syncRoot );
		if( logMessagePublished != nullptr ) {
			try {
				logMessagePublished->Invoke( message );
			}
			catch( Object^ ) {
			}
		}
	}

	void Logging::SetupLoggingSink( ) {
		if( !LoggingSink::contains() ) {
			if( LoggingSink::add(gcnew Action<TraceEventType, String^>(&Hypertable::Logging::TraceEvent)
													,gcnew Action<String^>(&Hypertable::Logging::PublishLogMessage)) ) {
				AppDomain::CurrentDomain->DomainUnload += gcnew EventHandler(&Hypertable::Logging::DomainUnload);
			}
		}
	}

	void Logging::DomainUnload( Object^ sender, EventArgs^ ) {
		msclr::lock sync( syncRoot );
		LoggingSink::remove( );
	}

}