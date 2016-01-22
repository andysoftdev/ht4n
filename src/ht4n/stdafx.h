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

#pragma once

#pragma warning( disable : 4355 ) // 'this' : used in base member initializer list

#include <msclr/lock.h>
#include <msclr/marshal.h>
#include <msclr/appdomain.h>
#include <stdint.h>


/// <summary>
/// Calculates a new hash code, from the hash code specified and a new hash value.
/// </summary>
/// <param name="hash">The existing hash code.</param>
/// <param name="newHashValue">The new hash value.</param>
/// <returns>The newly calculated hash code.</returns>
inline int Hash( int hash, int newHashValue ) {
	return hash * 29 + newHashValue;
}
