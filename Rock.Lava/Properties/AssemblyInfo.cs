using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: InternalsVisibleTo( "Rock" )]
#if NET5_0_OR_GREATER
[assembly: InternalsVisibleTo( "Rock.NG" )]
#endif
