using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.ccvonline.Residency.Migrations
{
    /// <summary>
    /// 
    /// </summary>
    [MigrationNumber( 7, "1.0.8" )]
    public class ResidencySite : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Create site for Residency and Update residency pages
            Sql( @"
DECLARE @PageId int
DECLARE @SiteId int
DECLARE @LoginPageId int

-- 'Resident Home' page is default page for site
SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '826C0BFF-C831-4427-98F9-57FF462D82F5')
SET @LoginPageId = (select [Id] from [Page] where [Guid] = '07770489-9C8D-43FA-85B3-E99BB54D3353')

INSERT INTO [Site] (IsSystem, Name, Description, Theme, DefaultPageId, LoginPageId, Guid)
    VALUES (0, 'Residency', 'The site for the Residency Resident pages', 'Stark', @PageId, @LoginPageId, '960F1D98-891A-4508-8F31-3CF206F5406E')" );

            RockMigrationHelper.AddLayout( "960F1D98-891A-4508-8F31-3CF206F5406E", "Dialog", "Dialog", "", "D60C73F6-C2D7-414D-8DDA-09BF15403861" );
            RockMigrationHelper.AddLayout( "960F1D98-891A-4508-8F31-3CF206F5406E", "Error", "Error", "", "23489D43-583C-40DF-A908-83CEEB9A9757" );
            RockMigrationHelper.AddLayout( "960F1D98-891A-4508-8F31-3CF206F5406E", "FullWidth", "Full Width", "", "5F8237AF-06E6-4596-9410-208CBD032559" );
            RockMigrationHelper.AddLayout( "960F1D98-891A-4508-8F31-3CF206F5406E", "Homepage", "Homepage", "", "523CE2B2-24AA-4C44-91D5-C912A64506B7" );
            RockMigrationHelper.AddLayout( "960F1D98-891A-4508-8F31-3CF206F5406E", "LeftSidebar", "Left Sidebar", "", "8A855392-A965-4CF5-9091-CCE6EFBFC62F" );
            RockMigrationHelper.AddLayout( "960F1D98-891A-4508-8F31-3CF206F5406E", "RightSidebar", "Right Sidebar", "", "65330865-732D-46BC-BA7C-8438237CFBE1" );
            RockMigrationHelper.AddLayout( "960F1D98-891A-4508-8F31-3CF206F5406E", "ThreeColum", "	Three Column", "", "1950A08D-A96B-452F-8C91-D142310C7FED" );

            Sql( @"
DECLARE @LayoutId int
SET @LayoutId = (SELECT [Id] FROM [Layout] WHERE [Guid] = '5F8237AF-06E6-4596-9410-208CBD032559')

-- Update Resident pages to use new site (full page layout)
UPDATE [Page] SET 
    [LayoutId] = @LayoutId
WHERE [Guid] in (
'130FA92D-9D5F-45D1-84AA-B399F2E868E6',
'83DBB422-38C5-44F3-9FDE-3737AC8CF2A7',
'0DF59029-C17B-474D-8DD1-ED312B734202',
'4827C8D3-B0FA-4AA4-891F-1F27C7D76606',
'A4BE6749-0190-4655-B3F6-0CEEC2DDD5C4',
'A16C4B0F-66C6-4CF0-8B54-B232DDF553B9',
'5D729D30-8E33-4913-A56F-98F803479C6D',
'56F3E462-28EF-4EC5-A58C-C5FDE48356E0',
'ADE663B9-386B-479C-ABD9-3349E1B4B827',
'826C0BFF-C831-4427-98F9-57FF462D82F5',
'07770489-9C8D-43FA-85B3-E99BB54D3353',
'162927F6-E503-43C4-B075-55F1E592E96E',
'BDA4C473-01CD-449E-97D4-4B054E3F0959',
'9A3A80AA-A9B0-4824-B81D-68F070131E92')

-- Update Resident Login page to use new site (full page layout, too)
UPDATE [Page] SET 
    [LayoutId] = @LayoutId
WHERE [Guid] in (
'07770489-9C8D-43FA-85B3-E99BB54D3353')

" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            //
        }
    }
}
