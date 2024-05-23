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

using Rock.SystemGuid;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 199, "1.16.4" )]
    public class MigrationRollupsForV17_0_7: Migration
    {
        private const string AdministrativeSettingsPageGuid = "A7E36E7A-EFBD-4912-B46E-BB61A74B86FF";

        /// <summary>
        /// KH: v17 ONLY - Create Administrative Settings Page/Page Search Block and move various Admin Tool Settings Pages under the new Administrative Settings Page
        /// </summary>
        public override void Up()
        {
            CreateAdministrativeSettingsPage();
            MoveSettingsPages();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
        }

        private void CreateAdministrativeSettingsPage()
        {
            // Add Page 
            //  Internal Name: Administrative Settings
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "550A898C-EDEA-48B5-9C58-B20EC13AF13B", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Administrative Settings", "", AdministrativeSettingsPageGuid, "" );

            // Add Page Route
            //   Page:Administrative Settings
            //   Route:admin/settings
            RockMigrationHelper.AddPageRoute( AdministrativeSettingsPageGuid, "admin/settings", "A000D38F-D19C-4F99-B498-227E3509A5C7" );

            // Update page order
            Sql( $@"UPDATE [dbo].[Page] SET [Order] = 0 WHERE [Guid] = '{AdministrativeSettingsPageGuid}'" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.PageSearch
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.PageSearch", "Page Search", "Rock.Blocks.Cms.PageSearch, Rock.Blocks, Version=1.17.0.18, Culture=neutral, PublicKeyToken=null", false, false, "85BA51A4-41CF-4F60-9EAE-1D8B1E73C736" );

            // Add/Update Obsidian Block Type
            //   Name:Rock.Blocks.Cms.PageSearch
            //   Category:
            //   EntityType:Rock.Blocks.Cms.PageSearch
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Page Search", "Displays a search page to find child pages", "Rock.Blocks.Cms.PageSearch", "CMS", "A279A88E-D4E0-4867-A108-2AA743B3CFD0" );

            // Add Block 
            //  Block Name: Page Search
            //  Page Name: Administrative Settings
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, AdministrativeSettingsPageGuid.AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "A279A88E-D4E0-4867-A108-2AA743B3CFD0".AsGuid(), "Page Search", "Main", @"", @"", 0, "2B8A3D46-8E5F-44E2-AC84-2554DCA502EC" );
        }

        private void MoveSettingsPages()
        {
            // General
            var generalSettingsPageGuid = "0B213645-FA4E-44A5-8E4C-B2D8EF054985";

            Sql( $@"UPDATE [Page]
SET InternalName = 'General',
PageTitle = 'General',
BrowserTitle = 'General'
WHERE [Page].[Guid] = '{generalSettingsPageGuid}'" );

            RockMigrationHelper.MovePage( generalSettingsPageGuid, AdministrativeSettingsPageGuid );

            // Security
            RockMigrationHelper.MovePage( "91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F", AdministrativeSettingsPageGuid );

            // Communications
            RockMigrationHelper.MovePage( "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA", AdministrativeSettingsPageGuid );

            // CMS
            // Move all the pages back to the CMS Configuration Page
            var cmsConfigurationPageGuid = "B4A24AB7-9369-4055-883F-4F4892C39AE3";

            Sql( $@"UPDATE [Page]
SET InternalName = 'CMS',
PageTitle = 'CMS',
BrowserTitle = 'CMS'
WHERE [Guid] = '{cmsConfigurationPageGuid}'" );

            RockMigrationHelper.MovePage( cmsConfigurationPageGuid, AdministrativeSettingsPageGuid );

            Sql( $@"
DECLARE @newParentPageId INT = (SELECT [Id] FROM [Page] WHERE [Guid] = '{cmsConfigurationPageGuid}')

UPDATE [dbo].[Page] SET [ParentPageId] = @newParentPageId WHERE [ParentPageId] IN (
   SELECT [Id] FROM [dbo].[Page] WHERE [Guid] IN (
   'CCDFEA8F-CF33-49C7-86C0-C4B10DCF1E89'  -- Website Configuration Section 
   ,'889D7F7F-EB0F-40CD-9E80-E58A00EE69F7' -- Content Channels Section
   ,'B892DF6D-4789-4AC3-9E6C-2BFE0D9E30E4' -- Personalization Section
   ,'04FE297E-D45E-44EC-B521-181423F05A1C' -- Content Platform Section
   ,'82726ACD-3480-4514-A920-FE920A71C046' -- Digital Media Applications Section
   )
)" );

            // Check In
            RockMigrationHelper.MovePage( "66C5DD58-094C-4FF9-9AFB-44801FCFCC2D", AdministrativeSettingsPageGuid );

            // Power Tools
            RockMigrationHelper.MovePage( "7F1F4130-CB98-473B-9DE1-7A886D2283ED", AdministrativeSettingsPageGuid );

            // System Settings
            var systemSettingsPageGuid = "C831428A-6ACD-4D49-9B2D-046D399E3123";

            Sql( $@"UPDATE [dbo].[Page]
SET [InternalName] = 'System',
[PageTitle] = 'System',
[BrowserTitle] = 'System'
WHERE [Page].[Guid] = '{systemSettingsPageGuid}'" );

            RockMigrationHelper.MovePage( systemSettingsPageGuid, AdministrativeSettingsPageGuid );
        }
    }
}
