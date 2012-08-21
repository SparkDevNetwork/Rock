//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using NuGet;

namespace Rock.Services.NuGet
{
	public class RockPackagePathResolver : DefaultPackagePathResolver
	{
		public RockPackagePathResolver( IFileSystem fileSystem ) : base( fileSystem, useSideBySidePaths: true ) { }

		public override string GetPackageDirectory( string packageId, SemanticVersion version )
		{
			return string.Empty;
		}
	}
}