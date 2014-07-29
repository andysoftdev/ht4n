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

#pragma comment( lib, "ws2_32.lib" )
#pragma comment( lib, "netapi32.lib" )
#pragma comment( lib, "advapi32.lib" )
#pragma comment( lib, "version.lib" )

#pragma comment( lib, "zlib.lib" )					// free of charge
#pragma comment( lib, "expat.lib" )					// free of charge
#pragma comment( lib, "thrift.lib" )				// Apache License 2.0
#ifdef _USE_SIGAR
#pragma comment( lib, "sigar.lib" )					// GPL v2
#endif

#pragma comment( lib, "Common.lib" )
#ifdef _USE_SIGAR
#pragma comment( lib, "SystemInfo.lib" )
#else
#pragma comment( lib, "SystemInfoWin.lib" )
#endif
#pragma comment( lib, "AsyncComm.lib" )
#pragma comment( lib, "FsBroker.lib" )
#pragma comment( lib, "Hypertools.lib" )
#pragma comment( lib, "Hyperspace.lib" )
#pragma comment( lib, "Hypertable.lib" )
#pragma comment( lib, "HyperAppHelper.lib" )
#pragma comment( lib, "ThriftBroker.lib" )

#pragma comment( lib, "ht4c.Common.lib" )
#pragma comment( lib, "ht4c.Context.lib" )
#pragma comment( lib, "ht4c.Hyper.lib" )
#pragma comment( lib, "ht4c.Thrift.lib" )

#ifdef SUPPORT_HAMSTERDB

#pragma comment( lib, "re2.lib" )						// New BSD
#pragma comment( lib, "hamsterdb.lib" )			// GPL & commercial
#pragma comment( lib, "ht4c.Hamster.lib" )

#endif

#ifdef SUPPORT_SQLITEDB

#pragma comment( lib, "re2.lib" )						// New BSD
#pragma comment( lib, "ht4c.SQLite.lib" )

#endif

#ifdef SUPPORT_ODBC

#pragma comment( lib, "re2.lib" )						// New BSD
#pragma comment( lib, "ht4c.Odbc.lib" )

#endif
