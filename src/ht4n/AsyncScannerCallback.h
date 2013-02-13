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

#include "AsyncCallbackResult.h"

namespace Hypertable {
	using namespace System;
	using namespace System::Collections::Generic;

	ref class AsyncScannerContext;
	ref class Cell;

	/// <summary>
	/// Represents a callback method to be executed by an asynchronous table scan operation.
	/// </summary>
	/// <param name="ctx">Asynchronous table scanner context.</param>
	/// <param name="cells">Scanned cells.</param>
	/// <returns>The asynchronous table scanner callback result.</returns>
	/// <seealso cref="AsyncScannerContext"/>
	/// <seealso cref="AsyncCallbackResult"/>
	public delegate AsyncCallbackResult AsyncScannerCallback( AsyncScannerContext^ asyncScannerContext, IList<Cell^>^ cells );

}