/** -*- C# -*-
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

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

#if X64
[assembly: AssemblyTitle("ht4n.GettingStarted (x64)")]
#else

[assembly: AssemblyTitle("ht4n.GettingStarted (x86)")]
#endif

[assembly: AssemblyProduct("ht4n.GettingStarted")]
[assembly: AssemblyDescription("Hypertable GettingStarted")]
[assembly: AssemblyCompany("ht4n.softdev.ch")]
[assembly: AssemblyCopyright("Copyright © 2010-2014")]
[assembly: AssemblyVersion("0.9.7.19")]
[assembly: AssemblyFileVersion("0.9.7.19")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
[assembly: Guid("1BCEEC8B-7155-4DC9-9A35-8CAA058051FF")]
[assembly: NeutralResourcesLanguage("en-US")]

#pragma warning disable 1699 // Use command line option '/keyfile' or appropriate project settings instead of 'AssemblyKeyFile'

[assembly: AssemblyKeyFile("../ht4n.snk")]
[assembly: AssemblyDelaySign(false)]

#pragma warning restore 1699