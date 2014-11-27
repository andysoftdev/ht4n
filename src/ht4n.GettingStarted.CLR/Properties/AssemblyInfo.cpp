#include "stdafx.h"

using namespace System;
using namespace System::Resources;
using namespace System::Reflection;
using namespace System::Runtime::CompilerServices;
using namespace System::Runtime::InteropServices;
using namespace System::Security::Permissions;

#if _WIN64
[assembly: AssemblyTitle("ht4n.GettingStarted.CLR (x64)")];
#else
[assembly:AssemblyTitle("ht4n.GettingStarted.CLR (x86)")];
#endif

[assembly:AssemblyProduct("ht4n.GettingStarted.CLR")];
[assembly:AssemblyDescription("Hypertable GettingStarted C++ /CLR")];
[assembly:AssemblyCompany("ht4n.softdev.ch")];
[assembly:AssemblyCopyright("Copyright © 2010-2014")];
[assembly:AssemblyVersion("0.9.8.4")];
[assembly:AssemblyFileVersion("0.9.8.4")];
[assembly:ComVisible(false)];
[assembly:CLSCompliant(true)];
[assembly:Guid("D3EB9E18-901A-4F74-B67A-00899966D0D8")];
[assembly:NeutralResourcesLanguage("en-US")];
[assembly:RuntimeCompatibility(WrapNonExceptionThrows = true)];

//#pragma warning(disable: 1699 // Use command line option '/keyfile' or appropriate project settings instead of 'AssemblyKeyFile'

[assembly:AssemblyKeyFile("../ht4n.snk")];
[assembly:AssemblyDelaySign(true)];

//#pragma warning restore 1699

//obsolete [assembly:SecurityPermission(SecurityAction::RequestMinimum, UnmanagedCode = true)];

