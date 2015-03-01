/** -*- C++ -*-
 * Copyright (C) 2010-2015 Thalmann Software & Consulting, http://www.softdev.ch
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

#include "ht4c.Common/SessionState.h"

namespace Hypertable {
	using namespace System;

	/// <summary>
	/// Specifies possible Hypertable session states.
	/// </summary>
	[Serializable]
	public enum class SessionState {

		/// <summary>
		/// Session has expired.
		/// </summary>
		Expired = ht4c::Common::SS_Expired,

		/// <summary>
		/// Session is in jeopardy.
		/// </summary>
		Jeopardy = ht4c::Common::SS_Jeopardy,

		/// <summary>
		/// Session is connected and okay.
		/// </summary>
		Safe = ht4c::Common::SS_Safe,

		/// <summary>
		/// Attempting to reconnect session.
		/// </summary>
		Disconnected = ht4c::Common::SS_Disconnected
	};

}
