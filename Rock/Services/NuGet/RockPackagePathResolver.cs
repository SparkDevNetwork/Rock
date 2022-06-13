// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using NuGet;

namespace Rock.Services.NuGet
{
    /// <summary>
    /// 
    /// </summary>
    [Obsolete( "NuGet package processing is going to be removed in a future release." )]
    [RockObsolete( "1.13.3" )]
    public class RockPackagePathResolver : DefaultPackagePathResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RockPackagePathResolver" /> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        [Obsolete( "NuGet package processing is going to be removed in a future release." )]
        [RockObsolete( "1.13.3" )]
        public RockPackagePathResolver( IFileSystem fileSystem ) : base( fileSystem, useSideBySidePaths: true ) { }

        #pragma warning disable CS0809

        /// <summary>
        /// Gets the package directory.
        /// </summary>
        /// <param name="packageId">The package id.</param>
        /// <param name="version">The version.</param>
        /// <returns></returns>
        [Obsolete( "NuGet package processing is going to be removed in a future release." )]
        [RockObsolete( "1.13.3" )]
        public override string GetPackageDirectory( string packageId, SemanticVersion version )
        {
            return string.Empty;
        }

        #pragma warning restore CS0809
    }
}