/** -*- C++ -*-
 * Copyright (C) 2010-2014 Thalmann Software & Consulting, http://www.softdev.ch
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

#include "SessionStateChangedEventArgs.h"

namespace Hypertable {
	using namespace System;
	using namespace ht4c;

	SessionStateChangedEventArgs::SessionStateChangedEventArgs( Common::SessionState oldSessionState, Common::SessionState newSessionState )
	: oldSessionState( static_cast<SessionState>(oldSessionState) )
	, newSessionState( static_cast<SessionState>(newSessionState) ) 
	{
	}

}