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
    /// Rollup of migrations for v21.0: removes the obsolete AI Provider category attribute,
    /// removes the legacy Check-in Manager "RoomSettings" WebForms block, and makes Next-Gen
    /// check-in the public-facing default by swapping the /checkin routes.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 320, "21.0" )]
    public class MigrationRollupsForV21_0 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DH_RemoveAiProviderAttribute_Up();
            NA_Remove_LegacyCheckInManagerRoomSettingsBlock_Up();
            JPH_MakeNextGenCheckInTheDefault_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JPH_MakeNextGenCheckInTheDefault_Down();
        }

        #region DH_RemoveAiProviderAttribute

        /// <summary>
        /// DH: Removes the AI Provider attribute from category, replaced by Rock Intelligence.
        /// </summary>
        public void DH_RemoveAiProviderAttribute_Up()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.AI_AUTOMATION_AI_PROVIDER );
#pragma warning restore CS0618 // Type or member is obsolete
        }

        #endregion DH_RemoveAiProviderAttribute

        #region NA_Remove_LegacyCheckInManagerRoomSettingsBlock

        /// <summary>
        /// Delete the legacy WebForms Check-in Manager "RoomSettings" block.
        /// It has been integrated directly into the new Obsidian Check-in Manager "Roster" block.
        /// </summary>
        public void NA_Remove_LegacyCheckInManagerRoomSettingsBlock_Up()
        {
            Sql( @"
    DECLARE @RoomSettingsBlockTypeId INT = ( SELECT TOP (1) [Id] FROM [BlockType] WHERE [Path] = '~/Blocks/CheckIn/Manager/RoomSettings.ascx' OR [Guid] = 'C809C18F-A611-40F8-A67C-CB7289431507' );

    IF @RoomSettingsBlockTypeId IS NOT NULL
    BEGIN
        DELETE FROM [Block]
        WHERE [BlockTypeId] = @RoomSettingsBlockTypeId;

        DELETE FROM [BlockType]
        WHERE [Id] = @RoomSettingsBlockTypeId;
    END
    " );
        }

        #endregion NA_Remove_LegacyCheckInManagerRoomSettingsBlock

        #region JPH_MakeNextGenCheckInTheDefault

        /// <summary>
        /// JPH: Make Next-Gen check-in the public-facing default - up.
        /// </summary>
        public void JPH_MakeNextGenCheckInTheDefault_Up()
        {
            // Move classic check-in onto the `checkin-classic` prefix first, freeing up `checkin` for Next-Gen.
            Sql( @"
UPDATE [PageRoute]
SET [Route] = STUFF([Route], 1, LEN('checkin'), 'checkin-classic')
WHERE [Route] = 'checkin' OR [Route] LIKE 'checkin/%';

UPDATE [PageRoute]
SET [Route] = STUFF([Route], 1, LEN('nextgen-checkin'), 'checkin')
WHERE [Route] = 'nextgen-checkin' OR [Route] LIKE 'nextgen-checkin/%';" );

            var nextGenCheckInLink = @"<li class=""list-group-item""><a href=""~/nextgen-checkin"">Check-in</a></li>";
            var checkInLink = @"<li class=""list-group-item""><a href=""~/checkin"">Check-in</a></li>";
            var classicCheckInLink = @"<li class=""list-group-item""><a href=""~/checkin"">Classic Check-in</a></li>";

            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "Content" );

            // Point the "Check-in" link at the swapped route, then drop the "Classic Check-in" link (with its line break).
            Sql( $@"
DECLARE @BlockId INT = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '6A648E77-ABA9-4AAF-A8BB-027A12261ED9');

UPDATE [HtmlContent]
SET [Content] = REPLACE({targetColumn}, '{nextGenCheckInLink}', '{checkInLink}')
WHERE [BlockId] = @BlockId
    AND {targetColumn} LIKE '%{nextGenCheckInLink}%';

UPDATE [HtmlContent]
SET [Content] = REPLACE({targetColumn}, CHAR(13) + CHAR(10) + '{classicCheckInLink}', '')
WHERE [BlockId] = @BlockId
    AND {targetColumn} LIKE '%{classicCheckInLink}%';" );
        }

        /// <summary>
        /// JPH: Make Next-Gen check-in the public-facing default - down.
        /// </summary>
        public void JPH_MakeNextGenCheckInTheDefault_Down()
        {
            var nextGenCheckInLink = @"<li class=""list-group-item""><a href=""~/nextgen-checkin"">Check-in</a></li>";
            var checkInLink = @"<li class=""list-group-item""><a href=""~/checkin"">Check-in</a></li>";
            var classicCheckInLink = @"<li class=""list-group-item""><a href=""~/checkin"">Classic Check-in</a></li>";

            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "Content" );

            // Re-add the "Classic Check-in" link after "Check-in", then point "Check-in" back at the pre-swap route.
            Sql( $@"
DECLARE @BlockId INT = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '6A648E77-ABA9-4AAF-A8BB-027A12261ED9');

UPDATE [HtmlContent]
SET [Content] = REPLACE({targetColumn}, '{checkInLink}', '{checkInLink}' + CHAR(13) + CHAR(10) + '{classicCheckInLink}')
WHERE [BlockId] = @BlockId
    AND {targetColumn} LIKE '%{checkInLink}%'
    AND {targetColumn} NOT LIKE '%Classic Check-in%';

UPDATE [HtmlContent]
SET [Content] = REPLACE({targetColumn}, '{checkInLink}', '{nextGenCheckInLink}')
WHERE [BlockId] = @BlockId
    AND {targetColumn} LIKE '%{checkInLink}%';" );

            // Move Next-Gen check-in back onto `nextgen-checkin` first, freeing up `checkin` for Classic. This runs
            // before the Classic restore so the reclaimed `checkin` routes below aren't re-caught by this statement.
            Sql( @"
UPDATE [PageRoute]
SET [Route] = STUFF([Route], 1, LEN('checkin'), 'nextgen-checkin')
WHERE [Route] = 'checkin' OR [Route] LIKE 'checkin/%';

UPDATE [PageRoute]
SET [Route] = STUFF([Route], 1, LEN('checkin-classic'), 'checkin')
WHERE [Route] = 'checkin-classic' OR [Route] LIKE 'checkin-classic/%';" );
        }

        #endregion JPH_MakeNextGenCheckInTheDefault
    }
}
