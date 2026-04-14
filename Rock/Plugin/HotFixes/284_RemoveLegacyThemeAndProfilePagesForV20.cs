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
    [MigrationNumber( 284, "19.0" )]
    public class RemoveLegacyThemeAndProfilePagesForV20 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            NA_RemoveLegacyThemeAndProfilePagesForV20_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// 
        /// </summary>
        private void NA_RemoveLegacyThemeAndProfilePagesForV20_Up()
        {
            // Delete the Legacy "Rock" theme.
            Sql( @"DELETE FROM [Theme] WHERE [Name] = 'Rock' AND [RootPath] = '/Themes/Rock'" );

            Sql( @"
-- Find the parent 'People' page by its known Guid.
DECLARE @PeopleParentPageId INT =
(
    SELECT [Id]
    FROM [Page]
    WHERE [Guid] = '97ECDC48-6DF6-492E-8C72-161F76AE111B'
);

-- Find the specific legacy starting page under that parent.
DECLARE @LegacyPersonPagesStartingPageId INT =
(
    SELECT [Id]
    FROM [Page]
    WHERE [InternalName] = 'Person Pages V1'
      AND [IsSystem] = 1
      AND [ParentPageId] = @PeopleParentPageId
);

-- Build the full descendant tree under ""Person Pages V1"".
-- Depth 0 = direct children of the starting page.
-- Higher depth = deeper descendants.
;WITH PageTree AS
(
    SELECT
        p.[Id],
        p.[ParentPageId],
        0 AS [Depth]
    FROM [Page] p
    WHERE p.[ParentPageId] = @LegacyPersonPagesStartingPageId

    UNION ALL

    SELECT
        c.[Id],
        c.[ParentPageId],
        pt.[Depth] + 1
    FROM [Page] c
    INNER JOIN PageTree pt
        ON c.[ParentPageId] = pt.[Id]
)
SELECT
    [Id],
    [Depth]
INTO #PagesToDelete
FROM PageTree;

-- Start at the deepest level so children are deleted before parents.
DECLARE @Depth INT = (SELECT MAX([Depth]) FROM #PagesToDelete);

WHILE @Depth IS NOT NULL AND @Depth >= 0
BEGIN
    -- Delete only the pages at the current depth.
    DELETE p
    FROM [Page] p
    INNER JOIN #PagesToDelete d
        ON d.[Id] = p.[Id]
    WHERE d.[Depth] = @Depth;

    SET @Depth = @Depth - 1;
END

-- After all descendants are removed, delete the starting page itself.
DELETE FROM [Page]
WHERE [Id] = @LegacyPersonPagesStartingPageId;

-- Clean up the temp table.
DROP TABLE #PagesToDelete;
" );

            RockMigrationHelper.DeleteBlock( "19C2140D-498A-4675-B8A2-18B281736F6E" ); // "Login Status"; BlockType: Login Status
            RockMigrationHelper.DeleteBlock( "148E5996-00DE-4341-8541-20CB3FFB7C74" ); // "Menu"; BlockType: Page Menu
            RockMigrationHelper.DeleteBlock( "AE29A24E-6F85-4BC8-8C14-A8BF97A5D263" ); // "Footer Content"; BlockType: HTML Content
            RockMigrationHelper.DeleteBlock( "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A" ); // "Bio"; BlockType: Person Bio (V1)
            RockMigrationHelper.DeleteBlock( "4CC50BE8-72ED-43E0-8D11-7E2A590453CC" ); // "Family Members"; BlockType: Group Members (V1)
            RockMigrationHelper.DeleteBlock( "F82E5FF2-F412-405C-9CC5-BF6E0401EB38" ); // "Sub Page Menu"; BlockType: Page Menu
            RockMigrationHelper.DeleteBlock( "98A30DD7-8665-4C6D-B1BB-A8380E862A04" ); // "Badges 1"; BlockType: Badges
            RockMigrationHelper.DeleteBlock( "AA588E23-D34C-433A-BA3D-B0B82797A22F" ); // "Badges 2"; BlockType: Badges
            RockMigrationHelper.DeleteBlock( "F3E6CC14-C540-4FFC-A5A9-48AD9CC0A61B" ); // "Badges 3"; BlockType: Badges
            RockMigrationHelper.DeleteBlock( "B01F3EF1-ECB9-4C7E-AB92-B45C4C29D5C0" ); // "Smart Search"; BlockType: Smart Search

            Sql( @"
    -- Identify the legacy person detail layout that is being retired.
    DECLARE @LegacyPersonDetailLayoutId INT =
    (
        SELECT [Id]
        FROM [Layout]
        WHERE [Guid] = 'F66758C6-3E3D-4598-AF4C-B317047B5987'
    );

    -- Identify the replacement layout for any pages still pointing to the legacy layout.
    DECLARE @ReplacementPersonDetailLayoutId INT =
    (
        SELECT [Id]
        FROM [Layout]
        WHERE [Guid] = '6AD84AFC-B3A1-4E30-B53B-C6E57B513839'
    );

    -- Clear the LayoutId from any remaining blocks that still reference the legacy layout.
    UPDATE [Block]
    SET [LayoutId] = NULL
    WHERE [LayoutId] = @LegacyPersonDetailLayoutId;

    -- Re-point any remaining pages from the legacy layout to the replacement layout. (not likely to be any)
    UPDATE [Page]
    SET [LayoutId] = @ReplacementPersonDetailLayoutId
    WHERE [LayoutId] = @LegacyPersonDetailLayoutId;
" );

            RockMigrationHelper.DeleteLayout( "F66758C6-3E3D-4598-AF4C-B317047B5987" ); // "PersonDetail"

            RockMigrationHelper.DeleteBlockType( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C" ); // BlockType: Crm/PersonDetail/BioV1.ascx
            RockMigrationHelper.DeleteBlockType( "FC137BDA-4F05-4ECE-9899-A249C90D11FC" ); // BlockType: Crm/PersonDetail/GroupMembersV1.ascx
        }
    }
}
