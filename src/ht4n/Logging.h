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


namespace Hypertable {
	using namespace System;

	/// <summary>
	/// Represents a logging helper class.
	/// </summary>
	/// <example>
	/// The following example shows how to use a logfile.
	/// <code>
	/// Logging.Logfile = Assembly.GetAssembly(typeof(Program)).Location + ".log";
	/// </code>
	/// The following example shows how to process published log entries.
	/// <code>
	/// Logging.LogEntryPublished += delegate( String msg ) {
	///    System.Diagnostics.Debug.Write(msg);
	/// };
	/// </code>
	/// </example>
	public ref class Logging sealed {

		public:

			/// <summary>
			/// Gets or sets the logfile to be used.
			/// </summary>
			/// <remarks>
			/// The logging level might be configured using the "Hypertable.Logging.Level" property.
			/// </remarks>
			static property String^ Logfile {
				String^ get();
				void set( String^ logfile );
			}

			/// <summary>
			/// Occurs when a log entry has been published to the logging system.
			/// </summary>
			/// <remarks>
			/// The logging level might be configured using the "Hypertable.Logging.Level" property.
			/// </remarks>
			static event Action<String^>^ LogEntryPublished {
				static void add( Action<String^>^ action );
				static void remove( Action<String^>^ action );
				static void raise( String^ log );
			}

		private:

			Logging( ) { }

			static void Unload( Object^, EventArgs^ );
			static Action<String^>^ action;
			static Object^ syncRoot = gcnew Object();

	};

}