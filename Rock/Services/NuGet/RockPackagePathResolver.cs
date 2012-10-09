//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using NuGet;

namespace Rock.Services.NuGet
{
    /// <summary>
    /// 
    /// </summary>
    public class RockPackagePathResolver : DefaultPackagePathResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RockPackagePathResolver" /> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        public RockPackagePathResolver( IFileSystem fileSystem ) : base( fileSystem, useSideBySidePaths: true ) { }

        /// <summary>
        /// Gets the package directory.
        /// </summary>
        /// <param name="packageId">The package id.</param>
        /// <param name="version">The version.</param>
        /// <returns></returns>
        public override string GetPackageDirectory( string packageId, SemanticVersion version )
        {
            return string.Empty;
        }
    }
}