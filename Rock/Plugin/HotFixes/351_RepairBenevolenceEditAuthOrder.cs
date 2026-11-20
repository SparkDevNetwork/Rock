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
    /// Moves the default Benevolence role Edit rule on Benevolence Request Detail blocks behind the other Edit rules on
    /// the same block.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 351, "19.9" )]
    public class RepairBenevolenceEditAuthOrder : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /*
                11/19/26 - JPH

                Migration 292 added the Benevolence role Edit rule at Order 0, ahead of Edit rules an administrator
                had already set on the block. Only rules older than the 292 row (a lower Id) count, since rules added
                later through the Security dialog already land after it. A row that a person created or edited (an
                audit person alias is set) is left alone, since 292 sets neither. The 292 row moves one past the
                highest Order of those older rules, so it still sorts ahead of any later rule at that Order (Rock
                reads by Order, then Id). Nothing is deleted, and a rerun matches nothing because the moved row is no
                longer at Order 0.

                Reason: Keep administrator Edit rules ahead of the default Benevolence rule.
            */
            Sql( @"
DECLARE @BlockEntityTypeId  INT = ( SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D89555CA-9AE4-4D62-8AF1-E5E463C1EF65' ); /* Rock.Model.Block */
DECLARE @BenevolenceGroupId INT = ( SELECT [Id] FROM [Group] WHERE [Guid] = '02FA0881-3552-42B8-A519-D021139B800F' );      /* Benevolence role */
DECLARE @BlockTypeId        INT = ( SELECT [Id] FROM [BlockType] WHERE [Guid] = '34275D0E-BC7E-4A9C-913E-623D086159A1' ); /* Benevolence Request Detail */

IF @BlockEntityTypeId IS NOT NULL AND @BenevolenceGroupId IS NOT NULL AND @BlockTypeId IS NOT NULL
BEGIN
    UPDATE [r]
    SET [r].[Order] = (
            SELECT MAX( [x].[Order] ) + 1
            FROM [Auth] AS [x]
            WHERE [x].[EntityTypeId] = @BlockEntityTypeId
              AND [x].[EntityId]     = [r].[EntityId]
              AND [x].[Action]       = 'Edit'
              AND [x].[Id]           < [r].[Id]
        )
        , [r].[ModifiedDateTime] = GETDATE()
    FROM [Auth] AS [r]
    INNER JOIN [Block] AS [b]
        ON [b].[Id] = [r].[EntityId]
        AND [b].[BlockTypeId] = @BlockTypeId
    WHERE [r].[EntityTypeId]  = @BlockEntityTypeId
      AND [r].[Action]        = 'Edit'
      AND [r].[AllowOrDeny]   = 'A'
      AND [r].[SpecialRole]   = 0
      AND [r].[GroupId]       = @BenevolenceGroupId
      AND [r].[PersonAliasId] IS NULL
      AND [r].[Order]         = 0
      AND [r].[CreatedByPersonAliasId]  IS NULL
      AND [r].[ModifiedByPersonAliasId] IS NULL
      AND EXISTS (
          SELECT 1
          FROM [Auth] AS [e]
          WHERE [e].[EntityTypeId] = @BlockEntityTypeId
            AND [e].[EntityId]     = [r].[EntityId]
            AND [e].[Action]       = 'Edit'
            AND [e].[Id]           < [r].[Id]
            AND [e].[Order]        > 0
      );
END" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }
    }
}
