using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "Rock" )]
[assembly: InternalsVisibleTo( "Rock.Blocks" )]

#if NET5_0_OR_GREATER
[assembly: InternalsVisibleTo( "Rock.Blocks.NG" )]
#endif
