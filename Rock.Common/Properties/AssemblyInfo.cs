using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo( "Rock" )]
[assembly: InternalsVisibleTo( "Rock.Blocks" )]
[assembly: InternalsVisibleTo( "Rock.WebStartup" )]
[assembly: InternalsVisibleTo( "Rock.Lava.Tests" )]
[assembly: InternalsVisibleTo( "Rock.Tests" )]

#if !WEBFORMS
[assembly: InternalsVisibleTo( "Rock.NG" )]
[assembly: InternalsVisibleTo( "Rock.Blocks.NG" )]
#endif
