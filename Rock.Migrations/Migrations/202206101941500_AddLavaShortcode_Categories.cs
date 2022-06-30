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

            var sql = $@"

                    DECLARE @EntityTypeID       INT; 

                    -- Get LavaShortcodeCategory EntityId    
                    SET @EntityTypeID = (SELECT TOP 1 [Id] FROM [EntityType] WHERE[Guid] = '{SystemGuid.EntityType.LAVA_SHORTCODE_CATEGORY}')
                    
                    IF (@EntityTypeID = 0)
                        BEGIN
                            RETURN
                        END
                    ELSE
                        BEGIN
                            DECLARE @GeneralID          INT;
                            DECLARE @ReportingID        INT;
                            DECLARE @WebsiteID          INT;

                            -- Shortcode IDs
                            DECLARE @AccordionID        INT;
                            DECLARE @ChartID            INT;
                            DECLARE @EasyPieChartID     INT;
                            DECLARE @GoogleHeatmapID    INT;
                            DECLARE @GoogleMapID        INT;
                            DECLARE @GoogleStaticMapID  INT;
                            DECLARE @KpiID              INT;
                            DECLARE @PanelID            INT;
                            DECLARE @ParallaxID         INT;
                            DECLARE @SparklineChartID   INT;
                            DECLARE @TrendChartID       INT;
                            DECLARE @VimeoID            INT;
                            DECLARE @WordCloudID        INT;
                            DECLARE @YouTubeID          INT;
                    
                            SET @AccordionID        = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name]   =   'Accordion')
                            SET @ChartID            = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name]   =   'Chart')
                            SET @EasyPieChartID     = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name]   =   'Easy Pie Chart')
                            SET @GoogleHeatmapID    = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name]   =   'Google Heatmap')
                            SET @GoogleMapID        = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name]   =   'Google Map')
                            SET @GoogleStaticMapID  = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name]   =   'Google Static Map')
                            SET @KpiID              = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name]   =   'KPI')
                            SET @PanelID            = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name]   =   'Panel')
                            SET @ParallaxID         = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name]   =   'Parallax')
                            SET @SparklineChartID   = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name]   =   'Sparkline Chart')
                            SET @TrendChartID       = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name]   =   'Trend Chart')
                            SET @VimeoID            = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name]   =   'Vimeo')
                            SET @WordCloudID        = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name]   =   'Word Cloud')
                            SET @YouTubeID          = (SELECT TOP 1 [Id] FROM [LavaShortcode] WHERE [Name]   =   'YouTube')
                  
                            SET @GeneralID          = (SELECT TOP 1 [Id] FROM [Category] WHERE [EntityTypeId] = @EntityTypeID AND [Guid]  = '12E91788-8545-4DE3-BB04-29F4968A4E2E')
                            SET @ReportingID        = (SELECT TOP 1 [Id] FROM [Category] WHERE [EntityTypeId] = @EntityTypeID AND [Guid]  = 'A5503FF2-01A2-49CB-8C22-E57C3D7FDC29')
                            SET @WebsiteID          = (SELECT TOP 1 [Id] FROM [Category] WHERE [EntityTypeId] = @EntityTypeID AND [Guid]  = 'C3270142-E72E-4FBF-BE94-9A2505DE7D54')
                    
                            DECLARE @InsertModifiedDate DATETIME;
                            SET @InsertModifiedDate = GETDATE()
                    
                            -- Add LavaShortcode Categories
                            INSERT INTO [LavaShortcodeCategory] (LavaShortcodeId, CategoryId)
                            VALUES 
                            (@AccordionID,      @GeneralID),
                            (@ChartID,          @ReportingID),
                            (@EasyPieChartID,   @ReportingID),
                            (@GoogleHeatmapID,  @ReportingID),
                            (@GoogleMapID,      @GeneralID),
                            (@GoogleStaticMapID,@GeneralID),
                            (@KpiID,            @ReportingID),
                            (@PanelID,          @WebsiteID),
                            (@ParallaxID,       @WebsiteID),
                            (@SparklineChartID, @ReportingID),
                            (@TrendChartID,     @ReportingID),
                            (@VimeoID,          @WebsiteID),
                            (@WordCloudID,      @GeneralID),
                            (@YouTubeID,        @WebsiteID);
                        END;
                    ";

            Sql( sql );
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
