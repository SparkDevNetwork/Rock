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