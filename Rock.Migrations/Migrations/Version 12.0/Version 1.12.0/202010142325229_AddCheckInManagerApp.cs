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
    public partial class AddCheckInManagerApp : Rock.Migrations.RockMigration
    {
        private const string OLD_CHECK_IN_MANAGER_BLOCK = "D38C2DA2-4F76-4BA5-9B26-ADA39D98DEDC";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();

            // Update Check-in Manager page's theme to the new RockManager theme
            Sql( $@"UPDATE [Site]
                    SET [Theme] = 'RockManager'
                    WHERE [Guid] = '{Rock.SystemGuid.Site.CHECK_IN_MANAGER}'" );

            // Make sure CHECK_IN_MANAGER_LOGIN and PERSON_PROFILE_CHECK_IN_MANAGER have Display-Never (They should have already been set to that)
            Sql( $@"UPDATE [Page]
                    SET [DisplayInNavWhen]=2
                    WHERE [Guid] IN ('{Rock.SystemGuid.Page.PERSON_PROFILE_CHECK_IN_MANAGER}','{Rock.SystemGuid.Page.CHECK_IN_MANAGER_LOGIN}')
                " );

            // Move the 'Check-in Type' page from a TopLevel page to be under CheckinManager instead,
            // and rename it to 'Settings' and change the icon to fa-gear.
            // Then make Checkin Manager a top level page
            Sql( $@"UPDATE [Page] 
SET 
    [ParentPageId] = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '{Rock.SystemGuid.Page.CHECK_IN_MANAGER}'),
    [InternalName] = 'Settings',
    [BrowserTitle] = 'Settings',
    [PageTitle] = 'Settings',
    [IconCssClass] = 'fa fa-gear',
    [Order] = (SELECT TOP 1 [Order] FROM [Page] WHERE [Guid] = '{Rock.SystemGuid.Page.CHECK_IN_MANAGER_LIVE_METRICS}')+1
WHERE [Guid] = '{Rock.SystemGuid.Page.CHECK_IN_MANAGER_SETTINGS}' " );
            Sql( $"UPDATE [Page] set [ParentPageId] = null WHERE [Guid] = '{Rock.SystemGuid.Page.CHECK_IN_MANAGER}'" );

            RockMigrationHelper.DeleteBlock( OLD_CHECK_IN_MANAGER_BLOCK );

            // Remove page display options for Check-in Room Manager page,etc
            Sql( $@"UPDATE [Page]
                SET [PageDisplayBreadCrumb] = 0, [PageDisplayDescription] = 0, [PageDisplayIcon] = 0, [PageDisplayTitle] = 0
                WHERE [Guid] IN ( 
                    '{Rock.SystemGuid.Page.CHECK_IN_MANAGER_ROOM_MANAGER}', 
                    '{Rock.SystemGuid.Page.CHECK_IN_MANAGER_ROSTER}', 
                    '{Rock.SystemGuid.Page.CHECK_IN_MANAGER_LIVE_METRICS}',
                    '{Rock.SystemGuid.Page.PERSON_PROFILE_CHECK_IN_MANAGER}',
                    '{Rock.SystemGuid.Page.CHECK_IN_MANAGER_ROOM_SETTINGS}',
                    '{Rock.SystemGuid.Page.CHECK_IN_MANAGER_LOGIN}',
                    '{Rock.SystemGuid.Page.CHECK_IN_MANAGER_LOGOUT}',
                    '{Rock.SystemGuid.Page.CHECK_IN_MANAGER_SETTINGS}'
                )" );

            // Remove page display options, except for Page Title for Check-in Manager Search
            Sql( $@"UPDATE [Page]
                SET [PageDisplayBreadCrumb] = 0, [PageDisplayDescription] = 0, [PageDisplayIcon] = 0, [PageDisplayTitle] = 1
                WHERE [Guid] IN ( 
                    '{Rock.SystemGuid.Page.CHECK_IN_MANAGER_SEARCH}'
                )" );

            // Update CheckinManager route to go to new Roster Page
            Sql( $@"
DECLARE @rosterPageId INT = (
		SELECT TOP 1 Id
		FROM Page
		WHERE Guid = '{Rock.SystemGuid.Page.CHECK_IN_MANAGER_ROSTER}'
		)

SELECT @rosterPageId

IF @rosterPageId IS NOT NULL
BEGIN
	UPDATE PageRoute
	SET PageId = @rosterPageId
	WHERE Guid = '{Rock.SystemGuid.PageRoute.CHECK_IN_MANAGER}'
END" );

            // If Fix CHECK_IN_MANAGER_LOGOUT page so the Parent Page is CHECK_IN_MANAGER_LOGIN (this same fix is in a migration roll up in case the prior version of this migration was run)
            Sql( $@"
UPDATE [Page]
SET [ParentPageId] = (
        SELECT TOP 1 [Id]
        FROM [Page] x
        WHERE x.[Guid] = '{Rock.SystemGuid.Page.CHECK_IN_MANAGER_LOGIN}'
        )
WHERE [Guid] = '{Rock.SystemGuid.Page.CHECK_IN_MANAGER_LOGOUT}' AND [ParentPageId] != (
        SELECT TOP 1 [Id]
        FROM [Page] x
        WHERE x.[Guid] = '{Rock.SystemGuid.Page.CHECK_IN_MANAGER_LOGIN}'
        )
" );
        }

        /// <summary>
        /// Codes the gen migrations up.
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            // Add Page Search to Site:Rock Check-in Manager       
            RockMigrationHelper.AddPage( true, "A4DCE339-9C11-40CA-9A02-D2FE64EA164B", "8305704F-928D-4379-967A-253E576E0923", "Search", "", "5BB14114-BE20-4330-943A-5BC7E367116E", "fa fa-search" );
            // Add Page Room Manager to Site:Rock Check-in Manager   
            RockMigrationHelper.AddPage( true, "A4DCE339-9C11-40CA-9A02-D2FE64EA164B", "8305704F-928D-4379-967A-253E576E0923", "Room Manager", "", "CECB1460-10D4-4054-B5C3-903991CA40AB", "fa fa-door-open" );
            // Add Page Roster to Site:Rock Check-in Manager        
            RockMigrationHelper.AddPage( true, "CECB1460-10D4-4054-B5C3-903991CA40AB", "8305704F-928D-4379-967A-253E576E0923", "Roster", "", "BA04BF01-5244-4637-B12D-7A962D2A9E77", "" );
            // Add Page Room Settings to Site:Rock Check-in Manager    
            RockMigrationHelper.AddPage( true, "CECB1460-10D4-4054-B5C3-903991CA40AB", "8305704F-928D-4379-967A-253E576E0923", "Room Settings", "", "0416FF62-3252-4A84-85DB-79F4CAE82C75", "" );
            // Add Page Live Metrics to Site:Rock Check-in Manager   
            RockMigrationHelper.AddPage( true, "A4DCE339-9C11-40CA-9A02-D2FE64EA164B", "8305704F-928D-4379-967A-253E576E0923", "Live Metrics", "", "04F70D50-5D27-4C12-A76D-B25E6E4CB177", "fa fa-chart-bar" );
            // Add/Update BlockType Live Metrics  
            RockMigrationHelper.UpdateBlockType( "Live Metrics", "Block used to view current check-in counts and locations.", "~/Blocks/CheckIn/Manager/LiveMetrics.ascx", "Check-in > Manager", "A14D43A7-46EE-493E-9993-F89B86DF1604" );
            // Add/Update BlockType Room Settings   
            RockMigrationHelper.UpdateBlockType( "Room Settings", "Block used to open and close classrooms, Etc.", "~/Blocks/CheckIn/Manager/RoomSettings.ascx", "Check-in > Manager", "C809C18F-A611-40F8-A67C-CB7289431507" );
            // Add/Update BlockType Roster     
            RockMigrationHelper.UpdateBlockType( "Roster", "Block used to view people currently checked into a classroom, mark a person as 'present' in the classroom, check them out, Etc.", "~/Blocks/CheckIn/Manager/Roster.ascx", "Check-in > Manager", "EA5C2CF9-8602-445F-B2B7-48D0A5CFEA8C" );
            // Add/Update BlockType Search  
            RockMigrationHelper.UpdateBlockType( "Search", "Block used to search current check-in.", "~/Blocks/CheckIn/Manager/Search.ascx", "Check-in > Manager", "B10EF525-6F2F-46B8-865C-B4249A297307" );
            // Add Block Page Menu to Layout: Full Width, Site: Rock Check-in Manager      
            RockMigrationHelper.AddBlock( true, null, "8305704F-928D-4379-967A-253E576E0923".AsGuid(), "A5FA7C3C-A238-4E0B-95DE-B540144321EC".AsGuid(), "CACB9D1A-A820-4587-986A-D66A69EE9948".AsGuid(), "Page Menu", "Navigation", @"", @"", 0, "B569F42E-6687-45C4-90A4-CC7D99427372" );
            // Add Block Search to Page: Search, Site: Rock Check-in Manager    
            RockMigrationHelper.AddBlock( true, "5BB14114-BE20-4330-943A-5BC7E367116E".AsGuid(), null, "A5FA7C3C-A238-4E0B-95DE-B540144321EC".AsGuid(), "B10EF525-6F2F-46B8-865C-B4249A297307".AsGuid(), "Search", "Main", @"", @"", 0, "1D20FBEE-5378-4485-9965-A92CDCA7FF51" );
            // Add Block Roster to Page: Roster, Site: Rock Check-in Manager      
            RockMigrationHelper.AddBlock( true, "BA04BF01-5244-4637-B12D-7A962D2A9E77".AsGuid(), null, "A5FA7C3C-A238-4E0B-95DE-B540144321EC".AsGuid(), "EA5C2CF9-8602-445F-B2B7-48D0A5CFEA8C".AsGuid(), "Roster", "Main", @"", @"", 0, "26B9D6B8-8153-4F17-805A-8512BAC656E0" );
            // Add Block Room Settings to Page: Room Settings, Site: Rock Check-in Manager     
            RockMigrationHelper.AddBlock( true, "0416FF62-3252-4A84-85DB-79F4CAE82C75".AsGuid(), null, "A5FA7C3C-A238-4E0B-95DE-B540144321EC".AsGuid(), "C809C18F-A611-40F8-A67C-CB7289431507".AsGuid(), "Room Settings", "Main", @"", @"", 0, "8668F9C4-A1D7-452F-90F4-D75FF918F009" );
            // Add Block Live Metrics to Page: Live Metrics, Site: Rock Check-in Manager             
            RockMigrationHelper.AddBlock( true, "04F70D50-5D27-4C12-A76D-B25E6E4CB177".AsGuid(), null, "A5FA7C3C-A238-4E0B-95DE-B540144321EC".AsGuid(), "A14D43A7-46EE-493E-9993-F89B86DF1604".AsGuid(), "Live Metrics", "Main", @"", @"", 0, "7D88718E-7D10-4288-A086-226870013EE7" );
            // Attribute for BlockType: Live Metrics:Navigation Mode          
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A14D43A7-46EE-493E-9993-F89B86DF1604", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Navigation Mode", "Mode", "Navigation Mode", @"Navigation and attendance counts can be grouped and displayed either by 'Group Type > Group Type (etc) > Group > Location' or by 'location > location (etc).'  Select the navigation hierarchy that is most appropriate for your organization.", 0, @"T", "93C680C2-1D23-4A9E-ABA6-707270177049" );
            // Attribute for BlockType: Live Metrics:Check-in Type   
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A14D43A7-46EE-493E-9993-F89B86DF1604", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Check-in Type", "GroupTypeTemplate", "Check-in Type", @"The Check-in Area to display.  This value can also be overridden through the URL query string key (e.g. when navigated to from the Check-in Type selection block).", 1, @"", "631A14E6-F8A2-4C38-8CD3-6D8710D2E1EA" );
            // Attribute for BlockType: Live Metrics:Person Page       
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A14D43A7-46EE-493E-9993-F89B86DF1604", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Page", "PersonPage", "Person Page", @"The page used to display a selected person's details.", 2, @"", "78E0FECC-3AEB-4808-AD30-F927710CE518" );
            // Attribute for BlockType: Live Metrics:Area Select Page          
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A14D43A7-46EE-493E-9993-F89B86DF1604", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Area Select Page", "AreaSelectPage", "Area Select Page", @"The page to redirect user to if area has not be configured or selected.", 3, @"", "AA482599-BA77-4CA6-93CA-5BCE9872EC75" );
            // Attribute for BlockType: Roster:Check-in Type          
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EA5C2CF9-8602-445F-B2B7-48D0A5CFEA8C", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Check-in Type", "CheckInAreaGuid", "Check-in Type", @"The Check-in Area for the rooms to be managed by this Block. This value can also be overriden through the URL query string 'Area' key (e.g. when navigated to from the Check-in Type selection block).", 1, @"", "D2F2AD2D-3DCF-46C4-B33E-30A15063CCA6" );
            // Attribute for BlockType: Roster:Person Page         
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EA5C2CF9-8602-445F-B2B7-48D0A5CFEA8C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Page", "PersonPage", "Person Page", @"The page used to display a selected person's details.", 2, @"", "19CBEFAA-67A5-4A0F-A72B-A6D0963B0719" );
            // Attribute for BlockType: Roster:Area Select Page     
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EA5C2CF9-8602-445F-B2B7-48D0A5CFEA8C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Area Select Page", "AreaSelectPage", "Area Select Page", @"The page to redirect user to if a Check-in Area has not been configured or selected.", 3, @"", "7ACBCE93-430A-4F70-81ED-535EE08A129C" );
            // Attribute for BlockType: Search:Person Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B10EF525-6F2F-46B8-865C-B4249A297307", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Page", "PersonPage", "Person Page", @"The page used to display a selected person's details.", 0, @"", "99ADC091-0C13-46D2-BBA8-3BBFFE31F5B0" );
            // Attribute for BlockType: Search:Search By Code    
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B10EF525-6F2F-46B8-865C-B4249A297307", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Search By Code", "SearchByCode", "Search By Code", @"A flag indicating if security codes should also be evaluated in the search box results.", 1, @"False", "3B4CEA5D-FC8D-4F3E-84F5-752044BB5B70" );

            // Add Block Attribute Value
            //   Block: Roster
            //   BlockType: Roster
            //   Block Location: Page=Roster, Site=Rock Check-in Manager
            //   Attribute: Check-in Type
            //   Attribute Value: fedd389a-616f-4a53-906c-63d8255631c5
            RockMigrationHelper.AddBlockAttributeValue( "26B9D6B8-8153-4F17-805A-8512BAC656E0", "D2F2AD2D-3DCF-46C4-B33E-30A15063CCA6", @"fedd389a-616f-4a53-906c-63d8255631c5" );

            // Add Block Attribute Value
            //   Block: Roster
            //   BlockType: Roster
            //   Block Location: Page=Roster, Site=Rock Check-in Manager
            //   Attribute: Person Page
            //   Attribute Value: f3062622-c6ad-48f3-add7-7f58e4bd4ef3
            RockMigrationHelper.AddBlockAttributeValue( "26B9D6B8-8153-4F17-805A-8512BAC656E0", "19CBEFAA-67A5-4A0F-A72B-A6D0963B0719", @"f3062622-c6ad-48f3-add7-7f58e4bd4ef3" );

            // Add Block Attribute Value
            //   Block: Roster
            //   BlockType: Roster
            //   Block Location: Page=Roster, Site=Rock Check-in Manager
            //   Attribute: Area Select Page
            //   Attribute Value: 62c70118-0a6f-432a-9d84-a5296655cb9e
            RockMigrationHelper.AddBlockAttributeValue( "26B9D6B8-8153-4F17-805A-8512BAC656E0", "7ACBCE93-430A-4F70-81ED-535EE08A129C", @"62c70118-0a6f-432a-9d84-a5296655cb9e" );

            // Add Block Attribute Value
            //   Block: Live Metrics
            //   BlockType: Live Metrics
            //   Block Location: Page=Live Metrics, Site=Rock Check-in Manager
            //   Attribute: Navigation Mode
            //   Attribute Value: T
            RockMigrationHelper.AddBlockAttributeValue( "7D88718E-7D10-4288-A086-226870013EE7", "93C680C2-1D23-4A9E-ABA6-707270177049", @"T" );

            // Add Block Attribute Value
            //   Block: Live Metrics
            //   BlockType: Live Metrics
            //   Block Location: Page=Live Metrics, Site=Rock Check-in Manager
            //   Attribute: Check-in Type
            //   Attribute Value: fedd389a-616f-4a53-906c-63d8255631c5
            RockMigrationHelper.AddBlockAttributeValue( "7D88718E-7D10-4288-A086-226870013EE7", "631A14E6-F8A2-4C38-8CD3-6D8710D2E1EA", @"fedd389a-616f-4a53-906c-63d8255631c5" );

            // Add Block Attribute Value
            //   Block: Live Metrics
            //   BlockType: Live Metrics
            //   Block Location: Page=Live Metrics, Site=Rock Check-in Manager
            //   Attribute: Person Page
            //   Attribute Value: f3062622-c6ad-48f3-add7-7f58e4bd4ef3
            RockMigrationHelper.AddBlockAttributeValue( "7D88718E-7D10-4288-A086-226870013EE7", "78E0FECC-3AEB-4808-AD30-F927710CE518", @"f3062622-c6ad-48f3-add7-7f58e4bd4ef3" );

            // Add Block Attribute Value
            //   Block: Live Metrics
            //   BlockType: Live Metrics
            //   Block Location: Page=Live Metrics, Site=Rock Check-in Manager
            //   Attribute: Area Select Page
            //   Attribute Value: 62c70118-0a6f-432a-9d84-a5296655cb9e
            RockMigrationHelper.AddBlockAttributeValue( "7D88718E-7D10-4288-A086-226870013EE7", "AA482599-BA77-4CA6-93CA-5BCE9872EC75", @"62c70118-0a6f-432a-9d84-a5296655cb9e" );

            // Block Attribute Value
            // Block: Page Menu
            // BlockType: Page Menu
            // Block Location: Layout=Full Width, Site=Rock Check-in Manager
            // Attribute: Include Current Parameters
            // Attribute Value: False
            RockMigrationHelper.AddBlockAttributeValue( "B569F42E-6687-45C4-90A4-CC7D99427372", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );

            // Block Attribute Value
            // Block: Page Menu
            // BlockType: Page Menu
            // Block Location: Layout=Full Width, Site=Rock Check-in Manager
            // Attribute: Template
            // Attribute Value: {% include '~~/Assets/Lava/PageNav.lava' %}
            RockMigrationHelper.AddBlockAttributeValue( "B569F42E-6687-45C4-90A4-CC7D99427372", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageNav.lava' %}" );

            // Add Block Attribute Value
            // Block: Page Menu
            // BlockType: Page Menu
            // Block Location: Layout=Full Width, Site=Rock Check-in Manager
            // Attribute: Root Page
            // Attribute Value: a4dce339-9c11-40ca-9a02-d2fe64ea164b
            RockMigrationHelper.AddBlockAttributeValue( "B569F42E-6687-45C4-90A4-CC7D99427372", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"a4dce339-9c11-40ca-9a02-d2fe64ea164b" );

            // Add Block Attribute Value
            //  - Block: Page Menu
            //  - BlockType: Page Menu
            //  - Block Location: Layout=Full Width, Site=Rock Check-in Manager
            //  - Attribute: Number of Levels
            //  - Attribute Value: 2
            RockMigrationHelper.AddBlockAttributeValue( "B569F42E-6687-45C4-90A4-CC7D99427372", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"2" );

            // Add Block Attribute Value
            //   Block: Page Menu
            //   BlockType: Page Menu
            //   Block Location: Layout=Full Width, Site=Rock Check-in Manager
            //   Attribute: Include Current QueryString
            //   Attribute Value: False
            RockMigrationHelper.AddBlockAttributeValue( "B569F42E-6687-45C4-90A4-CC7D99427372", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );

            // Block Attribute Value
            // Block: Page Menu
            // BlockType: Page Menu
            // Block Location: Layout=Full Width, Site=Rock Check-in Manager
            // Attribute: Is Secondary Block
            // Attribute Value: False
            RockMigrationHelper.AddBlockAttributeValue( "B569F42E-6687-45C4-90A4-CC7D99427372", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Add Block Attribute Value
            //   Block: Select Check-In Area
            //   BlockType: Select Check-In Area
            //   Block Location: Page=Check-in Type, Site=Rock Check-in Manager
            //   Attribute: Location Page
            //   Attribute Value: ba04bf01-5244-4637-b12d-7a962d2a9e77
            RockMigrationHelper.AddBlockAttributeValue( "F3D99F2C-417F-45C2-B518-E07BEC6E58D9", "64A3829D-66C2-43ED-8A34-67A27B21B9E3", @"ba04bf01-5244-4637-b12d-7a962d2a9e77" );

            // Add Block Attribute Value
            //   Block: Search
            //   BlockType: Search
            //   Block Location: Page=Search, Site=Rock Check-in Manager
            //   Attribute: Person Page
            //   Attribute Value: f3062622-c6ad-48f3-add7-7f58e4bd4ef3
            RockMigrationHelper.AddBlockAttributeValue( "1D20FBEE-5378-4485-9965-A92CDCA7FF51", "99ADC091-0C13-46D2-BBA8-3BBFFE31F5B0", @"f3062622-c6ad-48f3-add7-7f58e4bd4ef3" );

            // Add Block Attribute Value
            //   Block: Person Profile
            //   BlockType: Person Profile
            //   Block Location: Page=Person Profile, Site=Rock Check-in Manager
            //   Attribute: Manager Page
            //   Attribute Value: ba04bf01-5244-4637-b12d-7a962d2a9e77,a2b6ea1c-9e46-42c8-abe4-0fd32d562b48
            RockMigrationHelper.AddBlockAttributeValue( "1D33D2F9-D19C-495B-BBC8-4379AEF416FE", "DB666717-A429-4F44-B713-3F57702F3BD6", @"ba04bf01-5244-4637-b12d-7a962d2a9e77,a2b6ea1c-9e46-42c8-abe4-0fd32d562b48" );

            // Add Page Logout to Site:Rock Check-in Manager
            RockMigrationHelper.AddPage( true, "CECB1460-10D4-4054-B5C3-903991CA40AB", "8305704F-928D-4379-967A-253E576E0923", "Logout", "", "9762DE9F-F431-4108-9F1A-AE88DFEB3289", "" );

#pragma warning disable CS0618 // Type or member is obsolete
            // Add Page Route for Logout
            RockMigrationHelper.AddPageRoute( "9762DE9F-F431-4108-9F1A-AE88DFEB3289", "checkinmanager/logout", "36ED7574-0310-4A8F-9A30-11A82B74C525" );
#pragma warning restore CS0618 // Type or member is obsolete

            // Add Block Logout to Page: Logout, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlock( true, "9762DE9F-F431-4108-9F1A-AE88DFEB3289".AsGuid(), null, "A5FA7C3C-A238-4E0B-95DE-B540144321EC".AsGuid(), "CCB87054-8AA3-4F44-AA48-19BD028C4190".AsGuid(), "Logout", "Main", @"", @"", 0, "80D16848-7EF2-4452-B2C6-480EDC8268C3" );

            // Attribute for BlockType: Person Profile:Badges - Left
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "48BBB7A7-1E1D-461E-9B64-E9CAD815E9E1", "602F273B-7EC2-42E6-9AA7-A36A268192A3", "Badges - Left", "BadgesLeft", "Badges - Left", @"The badges to display on the left side of the badge bar.", 6, @"3F7D648D-D6BA-4F03-931C-AFBDFA24BBD8", "5729E01A-89C4-47C3-B706-7E6B63B7D210" );

            // Attribute for BlockType: Person Profile:Badges - Right
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "48BBB7A7-1E1D-461E-9B64-E9CAD815E9E1", "602F273B-7EC2-42E6-9AA7-A36A268192A3", "Badges - Right", "BadgesRight", "Badges - Right", @"The badges to display on the right side of the badge bar.", 7, @"8A9AD88E-359F-46FD-9BA1-8B0603644F17,66972BFF-42CD-49AB-9A7A-E1B9DECA4EBA,66972BFF-42CD-49AB-9A7A-E1B9DECA4EBE,E0455598-82B0-4F49-B806-C3A41C71E9DA", "4181A399-8ADB-4A4E-84FF-4E916AE62CB3" );

            // Attribute for BlockType: Roster:Show All Areas
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EA5C2CF9-8602-445F-B2B7-48D0A5CFEA8C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show All Areas", "ShowAllAreas", "Show All Areas", @"If enabled, all Check-in Areas will be shown. This setting will be ignored if a specific area is specified in the URL.", 2, @"True", "AB85D5AF-B136-4C06-804F-D7A5466ED8FB" );

            // Add Block Attribute Value
            //   Block: Roster
            //   BlockType: Roster
            //   Block Location: Page=Roster, Site=Rock Check-in Manager
            //   Attribute: Show All Areas
            //   Attribute Value: True
            RockMigrationHelper.AddBlockAttributeValue( "26B9D6B8-8153-4F17-805A-8512BAC656E0", "AB85D5AF-B136-4C06-804F-D7A5466ED8FB", @"True" );

            // Add Block Attribute Value
            //   Block: Logout
            //   BlockType: Logout
            //   Block Location: Page=Logout, Site=Rock Check-in Manager
            //   Attribute: Redirect Page
            //   Attribute Value: 31f51dbb-ac84-4724-9219-b46fadab9cb2
            RockMigrationHelper.AddBlockAttributeValue( "80D16848-7EF2-4452-B2C6-480EDC8268C3", "81C96E97-4FD9-40AA-B3CC-5EBA813630A5", @"31f51dbb-ac84-4724-9219-b46fadab9cb2" );

            // Add Block Attribute Value
            //   Block: Logout
            //   BlockType: Logout
            //   Block Location: Page=Logout, Site=Rock Check-in Manager
            //   Attribute: Message
            //   Attribute Value: <div class="alert alert-success">You have been logged out.</div>
            RockMigrationHelper.AddBlockAttributeValue( "80D16848-7EF2-4452-B2C6-480EDC8268C3", "A7F6BC82-6CEA-457C-9D56-A07D89FBDF8F", @"<div class=""alert alert-success"">You have been logged out.</div>" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();

            // Update Check-in Manager page's theme back to the 'Rock' theme
            Sql( $@"UPDATE 
                        [Site]
                    SET [Theme] = 'Rock'
                    WHERE
                        [Guid] = '{Rock.SystemGuid.Site.CHECK_IN_MANAGER}'
                " );
        }

        /// <summary>
        /// Codes the gen migrations down.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Search By Code Attribute for BlockType: Search      
            RockMigrationHelper.DeleteAttribute( "3B4CEA5D-FC8D-4F3E-84F5-752044BB5B70" );
            // Person Page Attribute for BlockType: Search     
            RockMigrationHelper.DeleteAttribute( "99ADC091-0C13-46D2-BBA8-3BBFFE31F5B0" );
            // Area Select Page Attribute for BlockType: Roster
            RockMigrationHelper.DeleteAttribute( "7ACBCE93-430A-4F70-81ED-535EE08A129C" );
            // Person Page Attribute for BlockType: Roster    
            RockMigrationHelper.DeleteAttribute( "19CBEFAA-67A5-4A0F-A72B-A6D0963B0719" );
            // Check-in Type Attribute for BlockType: Roster     
            RockMigrationHelper.DeleteAttribute( "D2F2AD2D-3DCF-46C4-B33E-30A15063CCA6" );
            // Area Select Page Attribute for BlockType: Live Metrics
            RockMigrationHelper.DeleteAttribute( "AA482599-BA77-4CA6-93CA-5BCE9872EC75" );
            // Person Page Attribute for BlockType: Live Metrics     
            RockMigrationHelper.DeleteAttribute( "78E0FECC-3AEB-4808-AD30-F927710CE518" );
            // Check-in Type Attribute for BlockType: Live Metrics    
            RockMigrationHelper.DeleteAttribute( "631A14E6-F8A2-4C38-8CD3-6D8710D2E1EA" );
            // Navigation Mode Attribute for BlockType: Live Metrics  
            RockMigrationHelper.DeleteAttribute( "93C680C2-1D23-4A9E-ABA6-707270177049" );
            // Remove Block: Page Menu, from , Layout: Full Width, Site: Rock Check-in Manager         
            RockMigrationHelper.DeleteBlock( "B569F42E-6687-45C4-90A4-CC7D99427372" );
            // Remove Block: Live Metrics, from Page: Live Metrics, Site: Rock Check-in Manager 
            RockMigrationHelper.DeleteBlock( "7D88718E-7D10-4288-A086-226870013EE7" );
            // Remove Block: Room Settings, from Page: Room Settings, Site: Rock Check-in Manager   
            RockMigrationHelper.DeleteBlock( "8668F9C4-A1D7-452F-90F4-D75FF918F009" );
            // Remove Block: Roster, from Page: Roster, Site: Rock Check-in Manager      
            RockMigrationHelper.DeleteBlock( "26B9D6B8-8153-4F17-805A-8512BAC656E0" );
            // Remove Block: Search, from Page: Search, Site: Rock Check-in Manager
            RockMigrationHelper.DeleteBlock( "1D20FBEE-5378-4485-9965-A92CDCA7FF51" );
            // Delete BlockType Search          
            RockMigrationHelper.DeleteBlockType( "B10EF525-6F2F-46B8-865C-B4249A297307" ); // Search  
            // Delete BlockType Roster   
            RockMigrationHelper.DeleteBlockType( "EA5C2CF9-8602-445F-B2B7-48D0A5CFEA8C" ); // Roster  
            // Delete BlockType Room Settings    
            RockMigrationHelper.DeleteBlockType( "C809C18F-A611-40F8-A67C-CB7289431507" ); // Room Settings  
            // Delete BlockType Live Metrics              
            RockMigrationHelper.DeleteBlockType( "A14D43A7-46EE-493E-9993-F89B86DF1604" ); // Live Metrics  
            // Delete BlockType Calendar Event Item Occurrence View              
            RockMigrationHelper.DeleteBlockType( "A6A21FDE-2C90-44B0-9DF2-30CA248D423D" ); // Calendar Event Item Occurrence View  
            // Delete BlockType Structured Content View              
            RockMigrationHelper.DeleteBlockType( "48561629-F290-45DB-8BF0-EAD590F592B7" ); // Structured Content View  
            // Delete Page Live Metrics from Site:Rock Check-in Manager       
            RockMigrationHelper.DeletePage( "04F70D50-5D27-4C12-A76D-B25E6E4CB177" ); //  Page: Live Metrics, Layout: Full Width, Site: Rock Check-in Manager  
            // Delete Page Room Settings from Site:Rock Check-in Manager              
            RockMigrationHelper.DeletePage( "0416FF62-3252-4A84-85DB-79F4CAE82C75" ); //  Page: Room Settings, Layout: Full Width, Site: Rock Check-in Manager  
            // Delete Page Roster from Site:Rock Check-in Manager             
            RockMigrationHelper.DeletePage( "BA04BF01-5244-4637-B12D-7A962D2A9E77" ); //  Page: Roster, Layout: Full Width, Site: Rock Check-in Manager  
            // Delete Page Room Manager from Site:Rock Check-in Manager            
            RockMigrationHelper.DeletePage( "CECB1460-10D4-4054-B5C3-903991CA40AB" ); //  Page: Room Manager, Layout: Full Width, Site: Rock Check-in Manager  
            // Delete Page Search from Site:Rock Check-in Manager             
            RockMigrationHelper.DeletePage( "5BB14114-BE20-4330-943A-5BC7E367116E" ); //  Page: Search, Layout: Full Width, Site: Rock Check-in Manager  
        }
    }
}
