﻿using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;



#if NET_40
#if DEBUG
[assembly: AssemblyConfiguration("net-4.0.win32; Debug")]
#else
[assembly: AssemblyConfiguration("net-4.0.win32; Release")]
#endif
#else
#if DEBUG
[assembly: AssemblyConfiguration( "net-3.5.win32; Debug" )]
#else
[assembly: AssemblyConfiguration("net-3.5.win32; Release")]
#endif
#endif

#if NET_40
[assembly: AssemblyProduct("Quarz.NET 2.0 beta 1 for .NET 4.0")]
[assembly: SecurityRules( SecurityRuleSet.Level1 )]
#else
[assembly: AssemblyProduct( "Quarz.NET 2.0 beta 1 for .NET 3.5 SP1" )]
#endif
[assembly: AssemblyDescription( "Quartz Scheduling Framework for .NET" )]
[assembly: AssemblyCompany( "http://www.quartz-scheduler.net/" )]
[assembly: AssemblyCopyright( "Copyright 2001-2010 Terracotta Inc. and partially Marko Lahma" )]
[assembly: AssemblyTrademark( "Apache License, Version 2.0" )]
[assembly: AssemblyCulture( "" )]
[assembly: CLSCompliant( true )]
[assembly: ComVisible( false )]

[assembly: AssemblyVersion( "2.0.0.100" )]

[assembly: AssemblyDelaySign( false )]

[assembly: System.Security.AllowPartiallyTrustedCallers]
[assembly: AssemblyTitleAttribute( "Quartz" )]