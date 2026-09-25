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

using Rock.Model;
using Rock.Security;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Adds the new version security notice block to the homepage.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 325, "17.8" )]
    public class AddVersionSecurityNoticeBlock : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RegisterAndAddBlockInstanceUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// Registers and adds the new version security notice block instance
        /// to the homepage.
        /// </summary>
        private void RegisterAndAddBlockInstanceUp()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Administration.SecurityVersionNotice
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Administration.SecurityVersionNotice",
                "Security Version Notice",
                "Rock.Blocks.Administration.SecurityVersionNotice, Rock.Blocks, Version=17.8.0.0, Culture=neutral, PublicKeyToken=null",
                false,
                false,
                "31922DA8-A172-4B85-8FC7-BB07BCBBCDE5" );

            // Add/Update Obsidian Block Type
            //   Name:Security Version Notice
            //   Category:Administration
            //   EntityType:Rock.Blocks.Administration.SecurityVersionNotice
            RockMigrationHelper.UpdateMobileBlockType( "Security Version Notice",
                "Displays a notice to administrators when the running version of Rock has known security vulnerabilities.",
                "Rock.Blocks.Administration.SecurityVersionNotice",
                "Administration",
                "82BA1CB3-7220-46D1-9B5B-BC07F6D9005A" );

            // Move all blocks in the Main zone of the Homepage down one to make room
            // for the new Security Version Notice block at the top of the zone. Only
            // move blocks if the new block does not already exist.
            Sql( @"
DECLARE @SecurityVersionNoticeBlockId INT = ( SELECT [Id] FROM [Block] WHERE [Guid] = '0BCD1E06-ECC3-4867-8B62-B3308B9EF710' )
IF @SecurityVersionNoticeBlockId IS NULL
BEGIN
    DECLARE @HomepagePageId INT = ( SELECT [Id] FROM [Page] WHERE [Guid] = '" + SystemGuid.Page.INTERNAL_HOMEPAGE + @"' )
    UPDATE [Block]
    SET [Order] = [Order] + 1
    WHERE [PageId] = @HomepagePageId AND [Zone] = 'Main'
END" );

            // Add Block 
            //  Block Name: Security Version Notice
            //  Page Name: Homepage
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true,
                SystemGuid.Page.INTERNAL_HOMEPAGE.AsGuid(),
                null,
                null,
                "82BA1CB3-7220-46D1-9B5B-BC07F6D9005A".AsGuid(),
                "Security Version Notice",
                "Main",
                @"",
                @"",
                0,
                "0BCD1E06-ECC3-4867-8B62-B3308B9EF710" );

            // Make the block only visible to RSR - Rock Administration group.
            RockMigrationHelper.AddSecurityAuthForBlock( "0BCD1E06-ECC3-4867-8B62-B3308B9EF710",
                0,
                Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                SpecialRole.None,
                "EDC80490-A153-4C07-9566-C5FE1CF57B21" );

            RockMigrationHelper.AddSecurityAuthForBlock( "0BCD1E06-ECC3-4867-8B62-B3308B9EF710",
                1,
                Authorization.VIEW,
                false,
                null,
                SpecialRole.AllUsers,
                "EDC80490-A153-4C07-9566-C5FE1CF57B21" );
        }
    }
}
