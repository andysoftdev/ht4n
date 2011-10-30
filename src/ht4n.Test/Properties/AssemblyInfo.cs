/** -*- C# -*-
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

using System.Reflection;
using System.Runtime.InteropServices;

#if X64
[assembly: AssemblyTitle("ht4n.Test (x64)")]
#else

[assembly: AssemblyTitle("ht4n.Test (x86)")]
#endif

[assembly: AssemblyProduct("ht4n.Test")]
[assembly: AssemblyDescription("Hypertable .NET client library tests")]
[assembly: AssemblyCompany("ht4n.softdev.ch")]
[assembly: AssemblyCopyright("Copyright © 2010-2011")]
[assembly: AssemblyVersion("0.9.5.0")]
[assembly: AssemblyFileVersion("0.9.5.0")]
[assembly: ComVisible(false)]
[assembly: Guid("3bfd2fed-0f46-4874-a318-b47e1f456893")]

#pragma warning disable 1699 // Use command line option '/keyfile' or appropriate project settings instead of 'AssemblyKeyFile'

[assembly: AssemblyKeyFile("../ht4n.snk")]
[assembly: AssemblyDelaySign(false)]

#pragma warning restore 1699