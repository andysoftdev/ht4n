/** -*- C# -*-
 * Copyright (C) 2010-2012 Thalmann Software & Consulting, http://www.softdev.ch
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
using System.Windows;

#if X64
[assembly: AssemblyTitle("ht4n.Explorer (x64)")]
#else

[assembly: AssemblyTitle("ht4n.Explorer (x86)")]
#endif

[assembly: AssemblyProduct("ht4n.Explorer")]
[assembly: AssemblyDescription("Hypertable Explorer")]
[assembly: AssemblyCompany("ht4n.softdev.ch")]
[assembly: AssemblyCopyright("Copyright © 2010-2012")]
[assembly: AssemblyVersion("0.9.6.5")]
[assembly: AssemblyFileVersion("0.9.6.5")]
[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
[assembly: Guid("2D587F25-C918-4C14-B94F-0C2CD2E19E17")]
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]
[assembly: NeutralResourcesLanguage("en-US")]

#pragma warning disable 1699 // Use command line option '/keyfile' or appropriate project settings instead of 'AssemblyKeyFile'

[assembly: AssemblyKeyFile("../ht4n.snk")]
[assembly: AssemblyDelaySign(false)]

#pragma warning restore 1699