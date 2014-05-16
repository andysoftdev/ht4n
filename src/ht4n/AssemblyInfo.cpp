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

using namespace System;
using namespace System::Reflection;
using namespace System::Runtime::InteropServices;
using namespace System::Security;
using namespace System::Security::Permissions;

#if _WIN64
[assembly: AssemblyTitle("ht4n (x64)")]
#else
[assembly: AssemblyTitle("ht4n (x86)")]
#endif
[assembly: AssemblyProduct("ht4n")];
[assembly: AssemblyDescription("Hypertable .NET client library")];
[assembly: AssemblyCompany("ht4n.softdev.ch")];
[assembly: AssemblyCopyright("Copyright © 2010-2014")];
[assembly: AssemblyVersion("0.9.7.18")];
[assembly: AssemblyFileVersion("0.9.7.18")];

[assembly: CLSCompliant(true)];
[assembly: ComVisible(false)];
[assembly: Guid("51dadf8f-3aba-4959-949e-b943ba1b50a0")]
//obsolete [assembly: SecurityPermission(SecurityAction::RequestMinimum, UnmanagedCode = true)];
[assembly: AssemblyKeyFile("../ht4n.snk")];
[assembly: AssemblyDelaySign(true)];
