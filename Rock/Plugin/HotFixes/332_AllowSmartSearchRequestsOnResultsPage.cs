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
    /// Adds the Allow Smart Search Requests setting to the Universal Search block
    /// type and turns it on for Rock's built-in Universal Search results block.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 332, "17.8" )]
    public class AllowSmartSearchRequestsOnResultsPage : Migration
    {
        private const string UniversalSearchBlockTypeGuid = "FDF1BBFF-7A7B-4F4E-BF34-831203B0FEAC";
        private const string UniversalSearchResultsBlockGuid = "309A2477-9A5B-4FD4-A722-735F87861A05";
        private const string AllowSmartSearchRequestsAttributeGuid = "2BAFBB7F-654B-42BC-AEC2-240ECD4BF96A";
        private const string AllowSmartSearchRequestsAttributeKey = "AllowSmartSearchRequests";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /*
                10/19/2026 - CH

                The Universal Search block now ignores the SmartSearch page parameter
                unless its new Allow Smart Search Requests setting is on, and that
                setting defaults to off. Without this migration the header Smart Search
                (Universal Search component) would stop using the Smart Search settings
                on Rock's built-in results page.

                Only the block Rock seeds on the internal Universal Search page is turned
                on, and only if it still exists and is still a Universal Search block.
                Every other Universal Search block, including any on a public page, stays
                off until an administrator turns the setting on.

                Reason: Keep the built-in Smart Search results page working after the fix.
            */
            AddAllowSmartSearchRequestsAttributeUp();
            EnableAllowSmartSearchRequestsOnResultsBlockUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// Adds the Allow Smart Search Requests block type attribute so it exists
        /// before the block type is registered at startup.
        /// </summary>
        private void AddAllowSmartSearchRequestsAttributeUp()
        {
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                UniversalSearchBlockTypeGuid,
                SystemGuid.FieldType.BOOLEAN,
                "Allow Smart Search Requests",
                AllowSmartSearchRequestsAttributeKey,
                "",
                "When enabled, a request with the SmartSearch page parameter uses the global Smart Search entities and field criteria instead of this block's Enabled Models and Base Field Filters. Only enable this on the internal Smart Search results page.",
                12,
                "False",
                AllowSmartSearchRequestsAttributeGuid );
        }

        /// <summary>
        /// Turns on Allow Smart Search Requests for the Universal Search block Rock
        /// seeds on the internal Universal Search page. Does nothing if that block
        /// is missing or has been changed to another block type.
        /// </summary>
        private void EnableAllowSmartSearchRequestsOnResultsBlockUp()
        {
            Sql( $@"
DECLARE @BlockTypeId int = (
    SELECT [Id]
    FROM [BlockType]
    WHERE [Guid] = '{UniversalSearchBlockTypeGuid}' )

DECLARE @BlockId int = (
    SELECT [Id]
    FROM [Block]
    WHERE [Guid] = '{UniversalSearchResultsBlockGuid}'
        AND [BlockTypeId] = @BlockTypeId )

DECLARE @AttributeId int = (
    SELECT [Id]
    FROM [Attribute]
    WHERE [Guid] = '{AllowSmartSearchRequestsAttributeGuid}' )

IF @BlockId IS NOT NULL AND @AttributeId IS NOT NULL
BEGIN
    IF EXISTS ( SELECT 1 FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId )
    BEGIN
        UPDATE [AttributeValue]
        SET [Value] = 'True'
        WHERE [AttributeId] = @AttributeId
            AND [EntityId] = @BlockId
    END
    ELSE
    BEGIN
        INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
        VALUES ( 1, @AttributeId, @BlockId, 'True', NEWID() )
    END
END" );
        }
    }
}
