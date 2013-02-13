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


namespace Hypertable {
	using namespace System;

	/// <summary>
	/// Represents a logging helper class.
	/// </summary>
	/// <example>
	/// Routing ht4n logs through NLog
	/// <code>
	/// &lt;system.diagnostics&gt;
	///   &lt;trace autoflush="true" /&gt;
	///   &lt;sources&gt;
	///     &lt;source name="ht4n" switchValue="All"&gt;
	///       &lt;listeners&gt;
	///         &lt;clear/&gt;
	///         &lt;add name="nlog" /&gt;
	///       &lt;/listeners&gt;
	///     &lt;/source&gt;
	///   &lt;/sources&gt;
	///   &lt;sharedListeners&gt;
	///     &lt;add name="nlog" type="NLog.NLogTraceListener, NLog" /&gt;
	///   &lt;/sharedListeners&gt;
	/// &lt;/system.diagnostics&gt;
	/// </code>
	/// The following example shows how to use a logfile.
	/// <code>
	/// Logging.Logfile = Assembly.GetAssembly(typeof(Program)).Location + ".log";
	/// </code>
	/// The following example shows how to trace log entries.
	/// <code>
	/// Logging.LogMessagePublished = message =&gt; System.Diagnostics.Trace.WriteLine(message);
	/// </code>
	/// </example>
	public ref class Logging abstract sealed {

		public:

			/// <summary>
			/// Gets or sets the trace source to be used.
			/// </summary>
			/// <remarks>
			/// The default trace source name is "ht4n", the native hypertable logging level might be configured using the "Hypertable.Logging.Level" property.
			/// </remarks>
			static property System::Diagnostics::TraceSource^ TraceSource {
				System::Diagnostics::TraceSource^ get();
				void set( System::Diagnostics::TraceSource^ traceSource );
			}

			/// <summary>
			/// Gets or sets the log message published delegate.
			/// </summary>
			/// <remarks>
			/// Gets called only for the native hypertable client log events, the native hypertable logging level might be configured using the "Hypertable.Logging.Level" property.
			/// </remarks>
			static property Action<String^>^ LogMessagePublished {
				Action<String^>^ get();
				void set( Action<String^>^ logMessagePublished );
			}

			/// <summary>
			/// Gets or sets the logfile to be used.
			/// </summary>
			/// <remarks>
			/// Logs only the native hypertable client log events, the native hypertable logging level might be configured using the "Hypertable.Logging.Level" property.
			/// </remarks>
			static property String^ Logfile {
				String^ get();
				void set( String^ logfile );
			}

			/// <summary>
			/// Determines whether the trace listener should trace the event of the type specified.
			/// </summary>
			/// <param name="traceEventType">The event type.</param>
			/// <returns>true if the trace listener should trace the event of the type specified, otherwise false.</returns>
			static bool IsEnabled( System::Diagnostics::TraceEventType traceEventType );

			/// <summary>
			/// Writes a trace event to the trace listeners in the trace source listener collection using the specified event type and message.
			/// </summary>
			/// <param name="traceEventType">The event type of the trace message.</param>
			/// <param name="message">The trace message.</param>
			static void TraceEvent( System::Diagnostics::TraceEventType traceEventType, String^ message );

			/// <summary>
			/// Writes a trace event to the trace listeners in the trace source listener collection using the specified event type and message.
			/// </summary>
			/// <param name="traceEventType">The event type of the trace message.</param>
			/// <param name="func">Function returning the trace message, evaluated if the trace event type specified has been enabled.</param>
			static void TraceEvent( System::Diagnostics::TraceEventType traceEventType, Func<String^>^ func );

			/// <summary>
			/// Writes a error event to the trace listeners in the trace source listener collection using the specified event type and message.
			/// </summary>
			/// <param name="exception">The exception to trace.</param>
			static void TraceException( Exception^ exception );

		private:

			static Logging( );
			static System::Diagnostics::TraceSource^ GetTraceSource( );
			static void PublishLogMessage( String^ message );
			static void SetupLoggingSink( );
			static void DomainUnload( Object^, EventArgs^ );

			static Action<String^>^ logMessagePublished;
			static System::Diagnostics::TraceSource^ traceSource;
			static Object^ syncRoot = gcnew Object();

	};

}