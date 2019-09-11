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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    ///MigrationRollupsForV8_7_5
    /// </summary>
    [MigrationNumber( 68, "1.8.6" )]
    public class MigrationRollupsForV8_7_5 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //UpdatedGoogleMapsShortCode();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {

        }

        /// <summary>
        /// SK: Fixed Typo in Google Maps Lava Shortcode Marker
        /// </summary>
        private void UpdatedGoogleMapsShortCode()
        {
            Sql( @"UPDATE
	[LavaShortcode]
SET
	[Documentation] = REPLACE([Documentation],'markerannimation','markeranimation'),
	[Markup] = REPLACE([Markup],'markerannimation','markeranimation'),
	[Parameters] = REPLACE([Parameters],'markerannimation','markeranimation')
WHERE
	[Guid]='FE298210-1307-49DF-B28B-3735A414CCA0'
" );
        }

    }
}
