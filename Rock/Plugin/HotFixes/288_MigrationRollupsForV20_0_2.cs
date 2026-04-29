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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 288, "19.0" )]
    public class MigrationRollupsForV20_0_2 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /*
                4/29/2026 - NA

                We ran into a race/timing issue with this block deletion that affects developers
                who create a fresh database and the pre-alpha deployment process (which starts
                from a v15 database or earlier). In these situations, this deletion interferes with the
                "Chop Block Types 17.1 (18.0.6)" chop job that runs in v18.0 via
                202505131801097_Rollup_20250513.cs because, by the time that job runs, the block has been
                deleted by this data migration.

                Reason: Deferring this deletion until the next migration squish, at which point
                there is no chance the block will still exist.
            */
            //NA_RemoveObsoleteAppleTVPageListObsidianBlock_Up();

            NA_RenameChoppedBlocksForV20_Up();
            NA_ReCleanupUnusedPluginManagerBlockType_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// No longer used.  See enginnering note above.
        /// </summary>
        private void NA_RemoveObsoleteAppleTVPageListObsidianBlock_Up()
        {
            RockMigrationHelper.DeleteBlockType( "a759218b-1c72-446c-8994-8559ba72941e" ); // BlockType: AppleTvPageList (Obsolete)
            RockMigrationHelper.DeleteEntityType( "4e89a96e-88a2-4ca4-a86b-b9ffdcacf49f" );// EntityType: AppleTvPageList (Obsolete)
        }

        private void NA_RenameChoppedBlocksForV20_Up()
        {
            Sql( @"
                -- Correct chopped Financial Batch List block type
                UPDATE [BlockType]
                SET [Name] = 'Financial Batch List',
                    [Description] = 'Displays a list of financial batches.'   
                WHERE [Guid] = 'AB345CE7-5DC6-41AF-BBDC-8D23D52AFE25'

                -- Correct chopped Group Attendance Detail block type
                UPDATE [BlockType]
                SET [Name] = 'Group Attendance Detail',
                    [Description] = 'Lists the group members for a specific occurrence date time and allows selecting if they attended or not.'   
                WHERE [Guid] = 'FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B'

                -- Correct chopped Notes block type
                UPDATE [BlockType]
                SET [Name] = 'Notes',
                    [Description] = 'Context aware block for adding notes to an entity.'   
                WHERE [Guid] = '2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3'
            " );
        }

        private void NA_ReCleanupUnusedPluginManagerBlockType_Up()
        {
            // For any pre-alpha systems that already ran 202602041840490_Rollup_20260204, this will delete the blocktype
            // This goes along with the deletion of the block via commit 7a115db90ed8c60d2d6ad4088ecc181524813d2e
            Sql( @"
            -- Delete old, very obsolete (v0.1) PluginManager block
            DECLARE @PluginManagerBlockTypeId INT = ( SELECT TOP (1) [Id] FROM [BlockType] WHERE [Path] = '~/Blocks/Core/PluginManager.ascx' AND [Guid] = 'F80268E6-2625-4565-AA2E-790C5E40A119' );

            IF @PluginManagerBlockTypeId IS NOT NULL
            BEGIN
                DELETE FROM [Block]
                WHERE [BlockTypeId] = @PluginManagerBlockTypeId;

                DELETE FROM [BlockType]
                WHERE [Id] = @PluginManagerBlockTypeId;
            END
            " );
        }
    }
}
