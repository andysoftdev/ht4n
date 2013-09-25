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

#include "SessionState.h"

namespace Hypertable {
	using namespace System;
	using namespace ht4c;

	/// <summary>
	/// Represents the session state changed event arguments.
	/// </summary>
	public ref class SessionStateChangedEventArgs sealed : public EventArgs {

		public:

			/// <summary>
			/// Gets the old session state.
			/// </summary>
			/// <seealso cref="SessionState"/>
			property SessionState^ OldSessionState {
				SessionState^ get( ) {
					return oldSessionState;
				}
			}

			/// <summary>
			/// Gets the new session state.
			/// </summary>
			/// <seealso cref="SessionState"/>
			property SessionState^ NewSessionState {
				SessionState^ get( ) {
					return newSessionState;
				}
			}

		internal:

			SessionStateChangedEventArgs( Common::SessionState oldSessionState, Common::SessionState newSessionState );

		private:

			SessionState oldSessionState;
			SessionState newSessionState;
	};

}