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
    /// Experimental: places the "App Shell Navigation" block in the
    /// Navigation zone of the internal site and parks the now-redundant Page Menu
    /// and Smart Search chrome blocks (the sidebar block provides navigation and
    /// search itself). The Login Status block is left in place in the Login zone,
    /// which the RockNextGen Site.Master renders in the sidebar footer as the
    /// account menu.
    ///
    /// Up() also un-parks any Login Status block stranded in the now-removed
    /// 'app-shell-parked-Login' zone by an earlier revision of this migration, moving
    /// it back to the Login zone so the sidebar footer renders it.
    ///
    /// Guarded so it only runs when the internal site is actually using the
    /// RockNextGen theme, and fully reversible via Down().
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 999, "19.4" )]
    public class AppShellNavigationExperiment : Migration
    {
        private const string InternalSiteGuid = "C2D29296-6A87-47A9-A753-EE4E9159C4C4";
        private const string AppShellNavBlockTypeGuid = "DE118B96-19C4-4992-A3BC-5F777B3D1C68";
        private const string PageMenuBlockTypeGuid = "CACB9D1A-A820-4587-986A-D66A69EE9948";
        private const string SmartSearchBlockTypeGuid = "9D406BD5-88C1-45E5-AFEA-70F9CFB66C74";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /* Ensure the entity block type exists before we reference it in SQL
               (block-type auto-registration runs after migrations on startup). */
            RockMigrationHelper.AddOrUpdateEntityBlockType(
                "App Shell Navigation",
                "An experimental app-shell-style navigation sidebar wired to Rock's real page navigation and universal search.",
                "Rock.Blocks.Example.AppShellNavigation",
                "Obsidian > Example",
                AppShellNavBlockTypeGuid );

            Sql( $@"
DECLARE @SiteId INT = ( SELECT [Id] FROM [Site] WHERE [Guid] = '{InternalSiteGuid}' );
DECLARE @Theme NVARCHAR(100) = ( SELECT [Theme] FROM [Site] WHERE [Id] = @SiteId );
DECLARE @NavBlockTypeId INT = ( SELECT [Id] FROM [BlockType] WHERE [Guid] = '{AppShellNavBlockTypeGuid}' );
DECLARE @PageMenuBlockTypeId INT = ( SELECT [Id] FROM [BlockType] WHERE [Guid] = '{PageMenuBlockTypeGuid}' );
DECLARE @SmartSearchBlockTypeId INT = ( SELECT [Id] FROM [BlockType] WHERE [Guid] = '{SmartSearchBlockTypeGuid}' );

/* Only rewire chrome when the internal site is actually on the RockNextGen theme. */
IF @SiteId IS NOT NULL AND @NavBlockTypeId IS NOT NULL AND @PageMenuBlockTypeId IS NOT NULL AND @Theme = 'RockNextGen'
BEGIN
    /* One-time repair: an earlier revision of this migration parked the Login Status
       block into a 'app-shell-parked-Login' zone. That zone has since been removed from
       Site.Master, so any block left there renders nowhere. Move anything stranded in
       it back to the Login zone, which the sidebar footer now renders. The zone name
       is unique to this experiment, so this is safe regardless of block type. */
    UPDATE [b] SET [b].[Zone] = 'Login'
    FROM [Block] AS [b]
    INNER JOIN [Layout] AS [l] ON [l].[Id] = [b].[LayoutId] AND [l].[SiteId] = @SiteId
    WHERE [b].[Zone] = 'app-shell-parked-Login';

    /* Add an App Shell Navigation block to the Navigation zone of every internal-site
       layout that currently hosts a Page Menu, mirroring how the Page Menu is seeded. */
    INSERT INTO [Block] ( [IsSystem], [BlockTypeId], [LayoutId], [Zone], [Order], [Name], [Guid], [PreHtml], [PostHtml], [OutputCacheDuration] )
    SELECT 0, @NavBlockTypeId, [pm].[LayoutId], 'Navigation', [pm].[Order], 'App Shell Navigation', NEWID(), '', '', 0
    FROM [Block] AS [pm]
    INNER JOIN [Layout] AS [l] ON [l].[Id] = [pm].[LayoutId] AND [l].[SiteId] = @SiteId
    WHERE [pm].[BlockTypeId] = @PageMenuBlockTypeId
        AND [pm].[Zone] = 'Navigation'
        AND NOT EXISTS (
            SELECT 1
            FROM [Block] AS [existing]
            WHERE [existing].[BlockTypeId] = @NavBlockTypeId
                AND [existing].[LayoutId] = [pm].[LayoutId]
                AND [existing].[Zone] = 'Navigation' );

    /* Park the now-redundant Page Menu so it stops rendering. The Login Status block
       is intentionally left in its Login zone; Site.Master renders that zone in the
       sidebar footer as the account menu. */
    UPDATE [b] SET [b].[Zone] = 'app-shell-parked-Navigation'
    FROM [Block] AS [b]
    INNER JOIN [Layout] AS [l] ON [l].[Id] = [b].[LayoutId] AND [l].[SiteId] = @SiteId
    WHERE [b].[BlockTypeId] = @PageMenuBlockTypeId AND [b].[Zone] = 'Navigation';

    IF @SmartSearchBlockTypeId IS NOT NULL
    BEGIN
        UPDATE [b] SET [b].[Zone] = 'app-shell-parked-Header'
        FROM [Block] AS [b]
        INNER JOIN [Layout] AS [l] ON [l].[Id] = [b].[LayoutId] AND [l].[SiteId] = @SiteId
        WHERE [b].[BlockTypeId] = @SmartSearchBlockTypeId AND [b].[Zone] = 'Header';
    END
END
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( $@"
DECLARE @SiteId INT = ( SELECT [Id] FROM [Site] WHERE [Guid] = '{InternalSiteGuid}' );
DECLARE @NavBlockTypeId INT = ( SELECT [Id] FROM [BlockType] WHERE [Guid] = '{AppShellNavBlockTypeGuid}' );
DECLARE @PageMenuBlockTypeId INT = ( SELECT [Id] FROM [BlockType] WHERE [Guid] = '{PageMenuBlockTypeGuid}' );
DECLARE @SmartSearchBlockTypeId INT = ( SELECT [Id] FROM [BlockType] WHERE [Guid] = '{SmartSearchBlockTypeGuid}' );

IF @SiteId IS NOT NULL AND @NavBlockTypeId IS NOT NULL
BEGIN
    /* Remove the App Shell Navigation blocks we added. */
    DELETE [b]
    FROM [Block] AS [b]
    INNER JOIN [Layout] AS [l] ON [l].[Id] = [b].[LayoutId] AND [l].[SiteId] = @SiteId
    WHERE [b].[BlockTypeId] = @NavBlockTypeId;

    /* Restore the parked chrome blocks to their original zones. Login Status was
       never parked, so there is nothing to restore for it. */
    UPDATE [b] SET [b].[Zone] = 'Navigation'
    FROM [Block] AS [b]
    INNER JOIN [Layout] AS [l] ON [l].[Id] = [b].[LayoutId] AND [l].[SiteId] = @SiteId
    WHERE [b].[BlockTypeId] = @PageMenuBlockTypeId AND [b].[Zone] = 'app-shell-parked-Navigation';

    IF @SmartSearchBlockTypeId IS NOT NULL
    BEGIN
        UPDATE [b] SET [b].[Zone] = 'Header'
        FROM [Block] AS [b]
        INNER JOIN [Layout] AS [l] ON [l].[Id] = [b].[LayoutId] AND [l].[SiteId] = @SiteId
        WHERE [b].[BlockTypeId] = @SmartSearchBlockTypeId AND [b].[Zone] = 'app-shell-parked-Header';
    END
END
" );
        }
    }
}
