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
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class Site
    {
        /// <summary>
        /// Gets the file URL.
        /// </summary>
        /// <param name="fileId">The configuration mobile phone file identifier.</param>
        /// <returns>full path of resource from Binary file path</returns>
        private static string GetFileUrl( int? fileId )
        {
            string virtualPath = string.Empty;
            if ( fileId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var binaryFile = new BinaryFileService( rockContext ).Get( ( int ) fileId );
                    if ( binaryFile != null )
                    {
                        if ( binaryFile.Path.Contains( "~" ) )
                        {
                            // Need to build out full path
                            virtualPath = binaryFile.Path.Substring( 1 );
                            var globalAttributes = GlobalAttributesCache.Get();
                            string publicAppRoot = globalAttributes.GetValue( "PublicApplicationRoot" );
                            virtualPath = $"{publicAppRoot}{virtualPath}";
                        }
                        else
                        {
                            virtualPath = binaryFile.Path;
                        }
                    }
                }
            }

            return virtualPath;
        }
    }
}
