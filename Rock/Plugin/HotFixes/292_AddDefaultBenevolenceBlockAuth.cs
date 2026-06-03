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
    /// Grants the Benevolence security role Edit access to every Benevolence Request Detail block instance.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 292, "19.1" )]
    public class AddDefaultBenevolenceBlockAuth : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /*
                6/3/26 - KH

                Grant the Benevolence security role 'Edit' (Allow) on every existing
                Benevolence Request Detail block instance. The NOT EXISTS guard makes
                this re-runnable and leaves any identical rule an admin already added
                untouched.
            */
            Sql( @"
DECLARE @BlockEntityTypeId  INT = ( SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D89555CA-9AE4-4D62-8AF1-E5E463C1EF65' ); -- Rock.Model.Block
DECLARE @BenevolenceGroupId INT = ( SELECT [Id] FROM [Group] WHERE [Guid] = '02FA0881-3552-42B8-A519-D021139B800F' );      -- Benevolence role
DECLARE @BlockTypeId        INT = ( SELECT [Id] FROM [BlockType] WHERE [Guid] = '34275D0E-BC7E-4A9C-913E-623D086159A1' ); -- Benevolence Request Detail

IF @BlockEntityTypeId IS NOT NULL AND @BenevolenceGroupId IS NOT NULL AND @BlockTypeId IS NOT NULL
BEGIN
    DECLARE @Now DATETIME = GETDATE();

    INSERT INTO [Auth] (
        [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny],
        [SpecialRole], [GroupId], [PersonAliasId], [Guid],
        [CreatedDateTime], [ModifiedDateTime]
    )
    SELECT
        @BlockEntityTypeId,
        [b].[Id],
        0,
        'Edit',
        'A',
        0,
        @BenevolenceGroupId,
        NULL,
        NEWID(),
        @Now,
        @Now
    FROM [Block] AS [b]
    WHERE [b].[BlockTypeId] = @BlockTypeId
      AND NOT EXISTS (
          SELECT 1
          FROM [Auth] AS [a]
          WHERE [a].[EntityTypeId] = @BlockEntityTypeId
            AND [a].[EntityId]     = [b].[Id]
            AND [a].[Action]       = 'Edit'
            AND [a].[GroupId]      = @BenevolenceGroupId
            AND [a].[SpecialRole]  = 0
      );
END
" );
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
