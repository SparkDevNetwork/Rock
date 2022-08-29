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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class AddLavaShortcode_Categories : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable();
            RockMigrationHelper.UpdateEntityType( "Rock.Model.LavaShortcodeCategory", SystemGuid.EntityType.LAVA_SHORTCODE_CATEGORY, false, true );
            AddShortcodeCategories();
            SeedCategories();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RemoveCategories();
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.LAVA_SHORTCODE_CATEGORY );
            DropTable();

        }

        #region Migration Methods

        private void DropTable()
        {
            DropForeignKey( "dbo.LavaShortcodeCategory", "CategoryId", "dbo.Category" );
            DropForeignKey( "dbo.LavaShortcodeCategory", "LavaShortcodeId", "dbo.LavaShortcode" );
            DropIndex( "dbo.LavaShortcodeCategory", new[] { "CategoryId" } );
            DropIndex( "dbo.LavaShortcodeCategory", new[] { "LavaShortcodeId" } );
            DropTable( "dbo.LavaShortcodeCategory" );
        }

        private void CreateTable()
        {
            CreateTable(
                "dbo.LavaShortcodeCategory",
                c => new
                {
                    LavaShortcodeId = c.Int( nullable: false ),
                    CategoryId = c.Int( nullable: false ),
                } )
                .PrimaryKey( t => new { t.LavaShortcodeId, t.CategoryId } )
                .ForeignKey( "dbo.LavaShortcode", t => t.LavaShortcodeId, cascadeDelete: true )
                .ForeignKey( "dbo.Category", t => t.CategoryId, cascadeDelete: true )
                .Index( t => t.LavaShortcodeId )
                .Index( t => t.CategoryId );
        }

        private void AddShortcodeCategories()
        {
            RockMigrationHelper.UpdateCategory( SystemGuid.EntityType.LAVA_SHORTCODE_CATEGORY, "General", "", "", "12E91788-8545-4DE3-BB04-29F4968A4E2E" );
            RockMigrationHelper.UpdateCategory( SystemGuid.EntityType.LAVA_SHORTCODE_CATEGORY, "Reporting", "", "", "A5503FF2-01A2-49CB-8C22-E57C3D7FDC29" );
            RockMigrationHelper.UpdateCategory( SystemGuid.EntityType.LAVA_SHORTCODE_CATEGORY, "Website", "", "", "C3270142-E72E-4FBF-BE94-9A2505DE7D54" );
        }

        public void SeedCategories()
        {

            Sql( $@"
                DECLARE @AccordionID INT = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name] = 'Accordion')
                DECLARE @ChartID INT = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name] = 'Chart')
                DECLARE @EasyPieChartID INT = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name] = 'Easy Pie Chart')
                DECLARE @GoogleHeatmapID INT = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name] = 'Google Heatmap')
                DECLARE @GoogleMapID INT = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name] = 'Google Map')
                DECLARE @GoogleStaticMapID INT = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name] = 'Google Static Map')
                DECLARE @KpiID INT = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name] = 'KPI')
                DECLARE @PanelID INT = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name] = 'Panel')
                DECLARE @ParallaxID INT = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name] = 'Parallax')
                DECLARE @SparklineChartID INT = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name] = 'Sparkline Chart')
                DECLARE @TrendChartID INT = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name] = 'Trend Chart')
                DECLARE @VimeoID INT = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name] = 'Vimeo')
                DECLARE @WordCloudID INT = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name] = 'Word Cloud')
                DECLARE @YouTubeID INT = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name] = 'YouTube')
        
                -- Get the Category IDs
                DECLARE @GeneralID INT = (SELECT [Id] FROM [Category] WHERE [Guid] = '12E91788-8545-4DE3-BB04-29F4968A4E2E')
                DECLARE @ReportingID INT = (SELECT [Id] FROM [Category] WHERE [Guid] = 'A5503FF2-01A2-49CB-8C22-E57C3D7FDC29')
                DECLARE @WebsiteID INT = (SELECT [Id] FROM [Category] WHERE [Guid] = 'C3270142-E72E-4FBF-BE94-9A2505DE7D54')
                    
                -- Add LavaShortcode Categories
                IF (@AccordionID IS NOT NULL AND @GeneralID IS NOT NULL)
                BEGIN INSERT INTO [LavaShortcodeCategory] (LavaShortcodeId, CategoryId) VALUES  (@AccordionID, @GeneralID) END

                IF (@ChartID IS NOT NULL AND @ReportingID IS NOT NULL)
                BEGIN INSERT INTO [LavaShortcodeCategory] (LavaShortcodeId, CategoryId) VALUES (@ChartID, @ReportingID) END

                IF (@EasyPieChartID IS NOT NULL AND @ReportingID IS NOT NULL)
                BEGIN INSERT INTO [LavaShortcodeCategory] (LavaShortcodeId, CategoryId) VALUES (@EasyPieChartID, @ReportingID) END

                IF (@GoogleHeatmapID IS NOT NULL AND @ReportingID IS NOT NULL)
                BEGIN INSERT INTO [LavaShortcodeCategory] (LavaShortcodeId, CategoryId) VALUES (@GoogleHeatmapID, @ReportingID) END

                IF (@GoogleMapID IS NOT NULL AND @GeneralID IS NOT NULL)
                BEGIN INSERT INTO [LavaShortcodeCategory] (LavaShortcodeId, CategoryId) VALUES (@GoogleMapID, @GeneralID) END

                IF (@GoogleStaticMapID IS NOT NULL AND @GeneralID IS NOT NULL)
                BEGIN INSERT INTO [LavaShortcodeCategory] (LavaShortcodeId, CategoryId) VALUES (@GoogleStaticMapID, @GeneralID) END

                IF (@KpiID IS NOT NULL AND @ReportingID IS NOT NULL)
                BEGIN INSERT INTO [LavaShortcodeCategory] (LavaShortcodeId, CategoryId) VALUES (@KpiID, @ReportingID) END

                IF (@PanelID IS NOT NULL AND @WebsiteID IS NOT NULL)
                BEGIN INSERT INTO [LavaShortcodeCategory] (LavaShortcodeId, CategoryId) VALUES (@PanelID, @WebsiteID) END

                IF (@ParallaxID IS NOT NULL AND @WebsiteID IS NOT NULL)
                BEGIN INSERT INTO [LavaShortcodeCategory] (LavaShortcodeId, CategoryId) VALUES (@ParallaxID, @WebsiteID) END

                IF (@SparklineChartID IS NOT NULL AND @ReportingID IS NOT NULL)
                BEGIN INSERT INTO [LavaShortcodeCategory] (LavaShortcodeId, CategoryId) VALUES (@SparklineChartID, @ReportingID) END

                IF (@TrendChartID IS NOT NULL AND @ReportingID IS NOT NULL)
                BEGIN INSERT INTO [LavaShortcodeCategory] (LavaShortcodeId, CategoryId) VALUES (@TrendChartID, @ReportingID) END

                IF (@VimeoID IS NOT NULL AND @WebsiteID IS NOT NULL)
                BEGIN INSERT INTO [LavaShortcodeCategory] (LavaShortcodeId, CategoryId) VALUES (@VimeoID, @WebsiteID) END

                IF (@WordCloudID IS NOT NULL AND @GeneralID IS NOT NULL)
                BEGIN INSERT INTO [LavaShortcodeCategory] (LavaShortcodeId, CategoryId) VALUES (@WordCloudID, @GeneralID) END

                IF (@YouTubeID IS NOT NULL AND @WebsiteID IS NOT NULL)
                BEGIN INSERT INTO [LavaShortcodeCategory] (LavaShortcodeId, CategoryId) VALUES (@YouTubeID, @WebsiteID) END" );
        }

        private void RemoveCategories()
        {
            // Add new categories
            RockMigrationHelper.DeleteCategory( "12E91788-8545-4DE3-BB04-29F4968A4E2E" );
            RockMigrationHelper.DeleteCategory( "A5503FF2-01A2-49CB-8C22-E57C3D7FDC29" );
            RockMigrationHelper.DeleteCategory( "C3270142-E72E-4FBF-BE94-9A2505DE7D54" );
        }

        #endregion Migration Methods
    }
}
