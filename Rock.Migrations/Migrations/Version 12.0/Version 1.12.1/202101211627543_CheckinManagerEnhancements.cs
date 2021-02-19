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
    public partial class CheckinManagerEnhancements : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            PagesBlocksMigration_Up();
            PagesBlockChanges_Up();
            ReprintLabelsSecurity_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            PagesBlocksMigration_Down();
        }

        /// <summary>
        /// Up for ReprintLabels security.
        /// </summary>
        private void ReprintLabelsSecurity_Up()
        {
            // set security on PersonRight block's 'ReprintLabels' to whatever is the security on the 'Rock Manager' site
            Sql( $@"
DECLARE @entityTypeIdSite INT = (
		SELECT TOP 1 Id
		FROM EntityType
		WHERE [Guid] = '{Rock.SystemGuid.EntityType.SITE}'
		), @siteIdCheckinManager INT = (
		SELECT TOP 1 Id
		FROM [Site]
		WHERE [Guid] = '{Rock.SystemGuid.Site.CHECK_IN_MANAGER}'
		), @entityTypeIdBlock INT = (
		SELECT TOP 1 Id
		FROM EntityType
		WHERE [Guid] = '{Rock.SystemGuid.EntityType.BLOCK}'
		), @blockIdPersonRightCheckinPerson INT = (
		SELECT Id
		FROM [Block]
		WHERE [Guid] = '{Rock.SystemGuid.Block.CHECKIN_MANAGER_PERSON_RECENT_ATTENDANCES}'
		)

INSERT INTO [dbo].[Auth] ([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid])
SELECT @entityTypeIdBlock [EntityTypeId], @blockIdPersonRightCheckinPerson [EntityId], a.[Order], 'ReprintLabels', a.[AllowOrDeny], a.[SpecialRole], a.[GroupId], NEWID()
FROM [Auth] a
WHERE a.EntityTypeId = @entityTypeIdSite AND a.EntityId = @siteIdCheckinManager AND NOT EXISTS (
		SELECT *
		FROM [Auth] aa
		WHERE [Action] = 'ReprintLabels' AND aa.[Order] = a.[Order] AND aa.EntityId = @blockIdPersonRightCheckinPerson AND aa.EntityTypeId = @entityTypeIdBlock
		)


" );
        }

        /// <summary>
        /// Does the Pages/Blocks migration up. (Code Generated from SQL script)
        /// </summary>
        private void PagesBlocksMigration_Up()
        {
            RockMigrationHelper.UpdateBlockType( "Person Profile (Obsolete)", "Obsolete: Use PersonLeft/PersonRight instead", "~/Blocks/CheckIn/Manager/Person.ascx", "Check-in > Manager", "48BBB7A7-1E1D-461E-9B64-E9CAD815E9E1" );

            // Add Page Room List to Site:Rock Check-in Manager
            // insert it after the Live Metrics page
            RockMigrationHelper.AddPage( true, "A4DCE339-9C11-40CA-9A02-D2FE64EA164B", "8305704F-928D-4379-967A-253E576E0923", "Room List", "", "CF03D854-AC02-412C-9B21-FB27B9F56BAB", "fa fa-clipboard-list",
                insertAfterPageGuid: Rock.SystemGuid.Page.CHECK_IN_MANAGER_LIVE_METRICS );

            // Add Page Attendance Detail to Site:Rock Check-in Manager
            RockMigrationHelper.AddPage( true, "F3062622-C6AD-48F3-ADD7-7F58E4BD4EF3", "2669A579-48A5-4160-88EA-C3A10024E1E1", "Attendance Detail", "", "758ECFCD-9E20-48B5-827B-973492E39C0D", "" );


            // Add/Update BlockType Room List
            RockMigrationHelper.UpdateBlockType( "Room List", "Shows all locations of the type room for the campus (context) and selected schedules.", "~/Blocks/CheckIn/Manager/RoomList.ascx", "Check-in > Manager", "2DEA7808-9AC1-4913-BF58-1CDC7922C901" );

            // Add/Update BlockType Attendance Detail
            RockMigrationHelper.UpdateBlockType( "Attendance Detail", "Block to show details of a person's attendance", "~/Blocks/CheckIn/Manager/AttendanceDetail.ascx", "Check-in > Manager", "CA59CE67-9313-4B9F-8593-380044E5AE6A" );

            // Add/Update BlockType Person Profile 
            RockMigrationHelper.UpdateBlockType( "Person Profile ", "Displays person details for a checked-in person", "~/Blocks/CheckIn/Manager/PersonLeft.ascx", "Check-in > Manager", "D54909DB-8A5D-4665-97ED-E2C8577E3C64" );

            // Add/Update BlockType Person Recent Attendances
            RockMigrationHelper.UpdateBlockType( "Person Recent Attendances", "Shows most recent attendances for a person.", "~/Blocks/CheckIn/Manager/PersonRight.ascx", "Check-in > Manager", "486892AE-B5FD-447C-9E27-15A4BF3667CB" );

            // Add Block Person Profile to Page: Person Profile, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlock( true, "F3062622-C6AD-48F3-ADD7-7F58E4BD4EF3".AsGuid(), null, "A5FA7C3C-A238-4E0B-95DE-B540144321EC".AsGuid(), "D54909DB-8A5D-4665-97ED-E2C8577E3C64".AsGuid(), "Person Profile", "Sidebar1", @"", @"", 0, "02B35C67-CA4C-4E0B-968E-CD23EBB6B1B6" );

            // Add Block Person Recent Attendances to Page: Person Profile, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlock( true, "F3062622-C6AD-48F3-ADD7-7F58E4BD4EF3".AsGuid(), null, "A5FA7C3C-A238-4E0B-95DE-B540144321EC".AsGuid(), "486892AE-B5FD-447C-9E27-15A4BF3667CB".AsGuid(), "Person Recent Attendances", "Main", @"", @"", 0, "087749E0-3C35-4093-980A-483D8D4C3142" );

            // Add Block Room List to Page: Room List, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlock( true, "CF03D854-AC02-412C-9B21-FB27B9F56BAB".AsGuid(), null, "A5FA7C3C-A238-4E0B-95DE-B540144321EC".AsGuid(), "2DEA7808-9AC1-4913-BF58-1CDC7922C901".AsGuid(), "Room List", "Main", @"", @"", 0, "F1226BEC-BAD5-4297-BF9E-FF9A9D456F81" );

            // Add Block Person Profile to Page: Attendance Detail, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlock( true, "758ECFCD-9E20-48B5-827B-973492E39C0D".AsGuid(), null, "A5FA7C3C-A238-4E0B-95DE-B540144321EC".AsGuid(), "D54909DB-8A5D-4665-97ED-E2C8577E3C64".AsGuid(), "Person Profile", "Sidebar1", @"", @"", 0, "296190D4-6702-499C-A434-9F49F61FBE54" );

            // Add Block Attendance Detail to Page: Attendance Detail, Site: Rock Check-in Manager
            RockMigrationHelper.AddBlock( true, "758ECFCD-9E20-48B5-827B-973492E39C0D".AsGuid(), null, "A5FA7C3C-A238-4E0B-95DE-B540144321EC".AsGuid(), "CA59CE67-9313-4B9F-8593-380044E5AE6A".AsGuid(), "Attendance Detail", "Main", @"", @"", 0, "8A9E16B9-B909-4897-A853-AE9F572A7AE8" );

            // update block order for pages with new blocks if the page,zone has multiple blocks

            // Update Order for Page: Attendance Detail,  Zone: Main,  Block: Attendance Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '8A9E16B9-B909-4897-A853-AE9F572A7AE8'" );

            // Update Order for Page: Attendance Detail,  Zone: Sidebar1,  Block: Person Profile
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '296190D4-6702-499C-A434-9F49F61FBE54'" );

            // Attribute for BlockType: Roster:Enable Group Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EA5C2CF9-8602-445F-B2B7-48D0A5CFEA8C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Group Column", "EnableGroupColumn", "Enable Group Column", @"When enabled, a column showing the group(s) the person checked into will be shown.", 5, @"False", "98A9419E-9196-4552-9279-8D61D3D6C251" );

            // Attribute for BlockType: Roster:Enable Checkout All
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EA5C2CF9-8602-445F-B2B7-48D0A5CFEA8C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Checkout All", "EnableCheckoutAll", "Enable Checkout All", @"When enabled, a button will be shown to allow checking out all individuals.", 6, @"False", "8BB0C826-FA7E-4BF1-AFAC-70325AB0A328" );

            // Attribute for BlockType: Roster:Enable Staying Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EA5C2CF9-8602-445F-B2B7-48D0A5CFEA8C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Staying Button", "EnableStayingButton", "Enable Staying Button", @"When enabled, a 'Staying' button will be shown to mark the person as checked into the selected service( shown in modal )", 7, @"False", "C6E6B620-62FE-48EF-A7E6-5E97CD0C91DE" );

            // Attribute for BlockType: Roster:Enable Not Present Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EA5C2CF9-8602-445F-B2B7-48D0A5CFEA8C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Not Present Button", "EnableNotPresentButton", "Enable Not Present Button", @"When enabled, a 'Not Present' button will be shown to mark the person as not being present in the room.", 8, @"False", "EDD21152-CFB3-4D1A-A8BC-B30550D095C5" );

            // Attribute for BlockType: Roster:Enable Mark Present
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EA5C2CF9-8602-445F-B2B7-48D0A5CFEA8C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Mark Present", "EnableMarkPresentButton", "Enable Mark Present", @"When enabled, a button will be shown in 'Checkedout' mode allowing the person to be marked present.", 9, @"False", "6FA98257-1952-4CE1-B2E7-1BA51A1F9871" );

            // Attribute for BlockType: Room List:Area Select Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2DEA7808-9AC1-4913-BF58-1CDC7922C901", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Area Select Page", "AreaSelectPage", "Area Select Page", @"If Show All Areas is not enabled, the page to redirect user to if a Check-in Area has not been configured or selected.", 2, @"", "11F9DEC7-5FAB-4005-9101-AE38703F891E" );

            // Attribute for BlockType: Room List:Roster Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2DEA7808-9AC1-4913-BF58-1CDC7922C901", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Roster Page", "RosterPage", "Roster Page", @"", 4, @"", "619F0DDD-B7CC-4500-99BE-BA5108B087BD" );

            // Attribute for BlockType: Room List:Show All Areas
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2DEA7808-9AC1-4913-BF58-1CDC7922C901", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show All Areas", "ShowAllAreas", "Show All Areas", @"If enabled, all Check-in Areas will be shown. This setting will be ignored if a specific area is specified in the URL.", 1, @"True", "C375A3E4-B03C-4F14-9F82-1EE5A4C4A252" );

            // Attribute for BlockType: Room List:Check-in Area
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2DEA7808-9AC1-4913-BF58-1CDC7922C901", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Check-in Area", "CheckInAreaGuid", "Check-in Area", @"If Show All Areas is not enabled, the Check-in Area for the rooms to be managed by this Block.", 3, @"", "2C9B39CE-3AF2-4070-8DC0-E85AA61C8929" );

            // Attribute for BlockType: Person Profile :Show Related People
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D54909DB-8A5D-4665-97ED-E2C8577E3C64", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Related People", "ShowRelatedPeople", "Show Related People", @"Should anyone who is allowed to check-in the current person also be displayed with the family members?", 1, @"False", "3F6FAAE7-D6AF-48EF-94DD-D57A66C8A543" );

            // Attribute for BlockType: Person Profile :Show Share Person Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D54909DB-8A5D-4665-97ED-E2C8577E3C64", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Share Person Button", "ShowSharePersonButton", "Show Share Person Button", @"", 5, @"True", "59F28558-89E1-48D1-96A7-7A2FEAED4555" );

            // Attribute for BlockType: Person Profile :Send SMS From
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D54909DB-8A5D-4665-97ED-E2C8577E3C64", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Send SMS From", "SMSFrom", "Send SMS From", @"The phone number SMS messages should be sent from", 2, @"", "56198D10-8AD7-4D51-B923-5A50FBB8F5D6" );

            // Attribute for BlockType: Person Profile :Share Person Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D54909DB-8A5D-4665-97ED-E2C8577E3C64", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Share Person Page", "SharePersonPage", "Share Person Page", @"", 6, @"AD899394-13AD-4CAB-BCB3-CFFD79C9ADCC,FCC0CCFF-8E18-48D8-A5EB-3D0F81D68280", "8D6E2F07-7F17-46BA-9371-E2F474B6456F" );

            // Attribute for BlockType: Person Profile :Child Attribute Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D54909DB-8A5D-4665-97ED-E2C8577E3C64", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Child Attribute Category", "ChildAttributeCategory", "Child Attribute Category", @"The children Attribute Category to display attributes from.", 3, @"", "223FFE2A-00EA-4EA6-8EC1-58F2AD721F49" );

            // Attribute for BlockType: Person Profile :Adult Attribute Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D54909DB-8A5D-4665-97ED-E2C8577E3C64", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Adult Attribute Category", "AdultAttributeCategory", "Adult Attribute Category", @"The adult Attribute Category to display attributes from.", 4, @"", "53210698-04E8-4BE2-9CDC-D1D9478DF696" );

            // Attribute for BlockType: Person Recent Attendances:Badges - Left
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "486892AE-B5FD-447C-9E27-15A4BF3667CB", "602F273B-7EC2-42E6-9AA7-A36A268192A3", "Badges - Left", "BadgesLeft", "Badges - Left", @"The badges to display on the left side of the badge bar.", 6, @"3F7D648D-D6BA-4F03-931C-AFBDFA24BBD8", "BD2B1623-1836-4CDD-A091-2FA4214C7F3A" );

            // Attribute for BlockType: Person Recent Attendances:Badges - Right
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "486892AE-B5FD-447C-9E27-15A4BF3667CB", "602F273B-7EC2-42E6-9AA7-A36A268192A3", "Badges - Right", "BadgesRight", "Badges - Right", @"The badges to display on the right side of the badge bar.", 7, @"8A9AD88E-359F-46FD-9BA1-8B0603644F17,66972BFF-42CD-49AB-9A7A-E1B9DECA4EBA,66972BFF-42CD-49AB-9A7A-E1B9DECA4EBE,E0455598-82B0-4F49-B806-C3A41C71E9DA", "2E18EB74-6973-4100-87AA-772801EAC959" );

            // Attribute for BlockType: Person Recent Attendances:Manager Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "486892AE-B5FD-447C-9E27-15A4BF3667CB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Manager Page", "ManagerPage", "Manager Page", @"Page used to manage check-in locations", 0, @"", "826301C5-E639-47B7-ACF5-8759EBFD9B6E" );

            // Attribute for BlockType: Person Recent Attendances:Attendance Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "486892AE-B5FD-447C-9E27-15A4BF3667CB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Attendance Detail Page", "AttendanceDetailPage", "Attendance Detail Page", @"Page to show details of an attendance.", 1, @"758ECFCD-9E20-48B5-827B-973492E39C0D", "77D94E03-B90B-4303-A4AE-3C1F3ED431D2" );

            // Attribute for BlockType: Person Recent Attendances:Allow Label Reprinting
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "486892AE-B5FD-447C-9E27-15A4BF3667CB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Label Reprinting", "AllowLabelReprinting", "Allow Label Reprinting", @"Determines if reprinting labels should be allowed.", 5, @"False", "29AC77F0-5496-40F7-87B5-C836523B36DF" );

            // Attribute for BlockType: Person Profile (Obsolete):Share Person Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "48BBB7A7-1E1D-461E-9B64-E9CAD815E9E1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Share Person Page", "SharePersonPage", "Share Person Page", @"", 9, @"AD899394-13AD-4CAB-BCB3-CFFD79C9ADCC,FCC0CCFF-8E18-48D8-A5EB-3D0F81D68280", "7AE366CC-9BA8-4F39-B1F6-41A3ED5400E9" );

            // Attribute for BlockType: Person Profile (Obsolete):Attendance Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "48BBB7A7-1E1D-461E-9B64-E9CAD815E9E1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Attendance Detail Page", "AttendanceDetailPage", "Attendance Detail Page", @"Page used to manage check-in locations", 0, @"758ECFCD-9E20-48B5-827B-973492E39C0D", "78CA378C-E9D0-4974-ABD1-657B51796F2B" );

            // Attribute for BlockType: Person Profile (Obsolete):Show Share Person Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "48BBB7A7-1E1D-461E-9B64-E9CAD815E9E1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Share Person Button", "ShowSharePersonButton", "Show Share Person Button", @"", 8, @"True", "976B5BE9-3654-4104-8F4D-D88C1E62489C" );


            // Add Block Attribute Value
            //   Block: Room List
            //   BlockType: Room List
            //   Block Location: Page=Room List, Site=Rock Check-in Manager
            //   Attribute: Area Select Page
            //   Attribute Value: 62c70118-0a6f-432a-9d84-a5296655cb9e
            RockMigrationHelper.AddBlockAttributeValue( "F1226BEC-BAD5-4297-BF9E-FF9A9D456F81", "11F9DEC7-5FAB-4005-9101-AE38703F891E", @"62c70118-0a6f-432a-9d84-a5296655cb9e" );

            // Add Block Attribute Value
            //   Block: Room List
            //   BlockType: Room List
            //   Block Location: Page=Room List, Site=Rock Check-in Manager
            //   Attribute: Check-in Area
            //   Attribute Value: fedd389a-616f-4a53-906c-63d8255631c5
            RockMigrationHelper.AddBlockAttributeValue( "F1226BEC-BAD5-4297-BF9E-FF9A9D456F81", "2C9B39CE-3AF2-4070-8DC0-E85AA61C8929", @"fedd389a-616f-4a53-906c-63d8255631c5" );

            // Add Block Attribute Value
            //   Block: Room List
            //   BlockType: Room List
            //   Block Location: Page=Room List, Site=Rock Check-in Manager
            //   Attribute: Roster Page
            //   Attribute Value: ba04bf01-5244-4637-b12d-7a962d2a9e77,a2b6ea1c-9e46-42c8-abe4-0fd32d562b48
            RockMigrationHelper.AddBlockAttributeValue( "F1226BEC-BAD5-4297-BF9E-FF9A9D456F81", "619F0DDD-B7CC-4500-99BE-BA5108B087BD", @"ba04bf01-5244-4637-b12d-7a962d2a9e77,a2b6ea1c-9e46-42c8-abe4-0fd32d562b48" );

            // Add Block Attribute Value
            //   Block: Person Recent Attendances
            //   BlockType: Person Recent Attendances
            //   Block Location: Page=Person Profile, Site=Rock Check-in Manager
            //   Attribute: Manager Page
            //   Attribute Value: ba04bf01-5244-4637-b12d-7a962d2a9e77,a2b6ea1c-9e46-42c8-abe4-0fd32d562b48
            RockMigrationHelper.AddBlockAttributeValue( "087749E0-3C35-4093-980A-483D8D4C3142", "826301C5-E639-47B7-ACF5-8759EBFD9B6E", @"ba04bf01-5244-4637-b12d-7a962d2a9e77,a2b6ea1c-9e46-42c8-abe4-0fd32d562b48" );

            // Add Block Attribute Value
            //   Block: Person Recent Attendances
            //   BlockType: Person Recent Attendances
            //   Block Location: Page=Person Profile, Site=Rock Check-in Manager
            //   Attribute: Attendance Detail Page
            //   Attribute Value: 758ecfcd-9e20-48b5-827b-973492e39c0d
            RockMigrationHelper.AddBlockAttributeValue( "087749E0-3C35-4093-980A-483D8D4C3142", "77D94E03-B90B-4303-A4AE-3C1F3ED431D2", @"758ecfcd-9e20-48b5-827b-973492e39c0d" );

            RockMigrationHelper.UpdateFieldType( "Attendance", "", "Rock", "Rock.Field.Types.AttendanceFieldType", "45F2BE0A-43C2-40D6-9888-68A2E72ACD06" );

            // Add/Update PageContext for Page:Attendance Detail, Entity: Rock.Model.Person, Parameter: Person
            RockMigrationHelper.UpdatePageContext( "758ECFCD-9E20-48B5-827B-973492E39C0D", "Rock.Model.Person", "Person", "6B77984D-7113-4412-AF12-27C13F28049B" );
        }

        /// <summary>
        /// Changes to Pages and Blocks (Not code generated)
        /// </summary>
        private void PagesBlockChanges_Up()
        {
            // Remove page display options for Check-in AttendanceDetail, RoomList page
            Sql( $@"UPDATE [Page]
                SET [PageDisplayBreadCrumb] = 0, [PageDisplayDescription] = 0, [PageDisplayIcon] = 0, [PageDisplayTitle] = 0
                WHERE [Guid] IN ( 
                    '{Rock.SystemGuid.Page.CHECK_IN_MANAGER_ATTENDANCE_DETAIL}', 
                    '{Rock.SystemGuid.Page.CHECK_IN_MANAGER_ROOM_LIST}'
                )" );

            // Change CheckinManager Navigation block to a Site block (so it shows on all layouts/pages on the CheckinManager site)
            Sql( $@"UPDATE [Block]
SET [LayoutId] = NULL, [SiteId] = (
		SELECT TOP 1 [Id]
		FROM [Site]
		WHERE [Guid] = '{Rock.SystemGuid.Site.CHECK_IN_MANAGER}'
		)
WHERE [Guid] = '{Rock.SystemGuid.Block.CHECKIN_MANAGER_NAVIGATION}'" );

            // delete obsolete CheckinManager Person block
            RockMigrationHelper.DeleteBlock( "1D33D2F9-D19C-495B-BBC8-4379AEF416FE" );

            // change Layout for CheckinManager Person page to Left Sidebar
            Sql( $@"UPDATE [Page]
SET LayoutId = (
		SELECT TOP 1 Id
		FROM Layout
		WHERE Guid = '{Rock.SystemGuid.Layout.CHECKIN_MANAGER_LEFT_SIDEBAR}'
		)
WHERE [Guid] = '{Rock.SystemGuid.Page.PERSON_PROFILE_CHECK_IN_MANAGER}'" );
        }

        /// <summary>
        /// Does the Pages/Blocks migration down.
        /// </summary>
        private void PagesBlocksMigration_Down()
        {
            // Enable Mark Present Attribute for BlockType: Roster
            RockMigrationHelper.DeleteAttribute( "6FA98257-1952-4CE1-B2E7-1BA51A1F9871" );

            // Enable Not Present Button Attribute for BlockType: Roster
            RockMigrationHelper.DeleteAttribute( "EDD21152-CFB3-4D1A-A8BC-B30550D095C5" );

            // Enable Staying Button Attribute for BlockType: Roster
            RockMigrationHelper.DeleteAttribute( "C6E6B620-62FE-48EF-A7E6-5E97CD0C91DE" );

            // Enable Checkout All Attribute for BlockType: Roster
            RockMigrationHelper.DeleteAttribute( "8BB0C826-FA7E-4BF1-AFAC-70325AB0A328" );

            // Enable Group Column Attribute for BlockType: Roster
            RockMigrationHelper.DeleteAttribute( "98A9419E-9196-4552-9279-8D61D3D6C251" );

            // Attendance Detail Page Attribute for BlockType: Person Recent Attendances
            RockMigrationHelper.DeleteAttribute( "77D94E03-B90B-4303-A4AE-3C1F3ED431D2" );

            // Badges - Right Attribute for BlockType: Person Recent Attendances
            RockMigrationHelper.DeleteAttribute( "2E18EB74-6973-4100-87AA-772801EAC959" );

            // Badges - Left Attribute for BlockType: Person Recent Attendances
            RockMigrationHelper.DeleteAttribute( "BD2B1623-1836-4CDD-A091-2FA4214C7F3A" );

            // Allow Label Reprinting Attribute for BlockType: Person Recent Attendances
            RockMigrationHelper.DeleteAttribute( "29AC77F0-5496-40F7-87B5-C836523B36DF" );

            // Manager Page Attribute for BlockType: Person Recent Attendances
            RockMigrationHelper.DeleteAttribute( "826301C5-E639-47B7-ACF5-8759EBFD9B6E" );

            // Share Person Page Attribute for BlockType: Person Profile 
            RockMigrationHelper.DeleteAttribute( "8D6E2F07-7F17-46BA-9371-E2F474B6456F" );

            // Show Share Person Button Attribute for BlockType: Person Profile 
            RockMigrationHelper.DeleteAttribute( "59F28558-89E1-48D1-96A7-7A2FEAED4555" );

            // Adult Attribute Category Attribute for BlockType: Person Profile 
            RockMigrationHelper.DeleteAttribute( "53210698-04E8-4BE2-9CDC-D1D9478DF696" );

            // Child Attribute Category Attribute for BlockType: Person Profile 
            RockMigrationHelper.DeleteAttribute( "223FFE2A-00EA-4EA6-8EC1-58F2AD721F49" );

            // Send SMS From Attribute for BlockType: Person Profile 
            RockMigrationHelper.DeleteAttribute( "56198D10-8AD7-4D51-B923-5A50FBB8F5D6" );

            // Show Related People Attribute for BlockType: Person Profile 
            RockMigrationHelper.DeleteAttribute( "3F6FAAE7-D6AF-48EF-94DD-D57A66C8A543" );

            // Attendance Detail Page Attribute for BlockType: Person Profile (Obsolete)
            RockMigrationHelper.DeleteAttribute( "F2C0BEE6-105A-4284-9677-834DA34BE1C1" );

            // Share Person Page Attribute for BlockType: Person Profile (Obsolete)
            RockMigrationHelper.DeleteAttribute( "13CC483A-446E-43BD-9CC3-870C48F7D35E" );

            // Show Share Person Button Attribute for BlockType: Person Profile (Obsolete)
            RockMigrationHelper.DeleteAttribute( "D0422A0C-5F03-4B65-8732-799353AA8EEE" );

            // Roster Page Attribute for BlockType: Room List
            RockMigrationHelper.DeleteAttribute( "619F0DDD-B7CC-4500-99BE-BA5108B087BD" );

            // Check-in Area Attribute for BlockType: Room List
            RockMigrationHelper.DeleteAttribute( "2C9B39CE-3AF2-4070-8DC0-E85AA61C8929" );

            // Area Select Page Attribute for BlockType: Room List
            RockMigrationHelper.DeleteAttribute( "11F9DEC7-5FAB-4005-9101-AE38703F891E" );

            // Show All Areas Attribute for BlockType: Room List
            RockMigrationHelper.DeleteAttribute( "C375A3E4-B03C-4F14-9F82-1EE5A4C4A252" );

            // Remove Block: Person Profile, from Page: Attendance Detail, Site: Rock Check-in Manager
            RockMigrationHelper.DeleteBlock( "296190D4-6702-499C-A434-9F49F61FBE54" );

            // Remove Block: Person Recent Attendances, from Page: Person Profile, Site: Rock Check-in Manager
            RockMigrationHelper.DeleteBlock( "087749E0-3C35-4093-980A-483D8D4C3142" );

            // Remove Block: Person Profile, from Page: Person Profile, Site: Rock Check-in Manager
            RockMigrationHelper.DeleteBlock( "02B35C67-CA4C-4E0B-968E-CD23EBB6B1B6" );

            // Remove Block: Attendance Detail, from Page: Attendance Detail, Site: Rock Check-in Manager
            RockMigrationHelper.DeleteBlock( "8A9E16B9-B909-4897-A853-AE9F572A7AE8" );

            // Remove Block: Room List, from Page: Room List, Site: Rock Check-in Manager
            RockMigrationHelper.DeleteBlock( "F1226BEC-BAD5-4297-BF9E-FF9A9D456F81" );

            // Delete BlockType Person Recent Attendances
            RockMigrationHelper.DeleteBlockType( "486892AE-B5FD-447C-9E27-15A4BF3667CB" ); // Person Recent Attendances

            // Delete BlockType Person Profile 
            RockMigrationHelper.DeleteBlockType( "D54909DB-8A5D-4665-97ED-E2C8577E3C64" ); // Person Profile 

            // Delete BlockType Attendance Detail
            RockMigrationHelper.DeleteBlockType( "CA59CE67-9313-4B9F-8593-380044E5AE6A" ); // Attendance Detail

            // Delete BlockType Room List
            RockMigrationHelper.DeleteBlockType( "2DEA7808-9AC1-4913-BF58-1CDC7922C901" ); // Room List

            // Delete Page Attendance Detail from Site:Rock Check-in Manager
            RockMigrationHelper.DeletePage( "758ECFCD-9E20-48B5-827B-973492E39C0D" ); //  Page: Attendance Detail, Layout: Left Sidebar, Site: Rock Check-in Manager

            // Delete Page Room List from Site:Rock Check-in Manager
            RockMigrationHelper.DeletePage( "CF03D854-AC02-412C-9B21-FB27B9F56BAB" ); //  Page: Room List, Layout: Full Width, Site: Rock Check-in Manager

            // Delete PageContext for Page:Attendance Detail, Entity: Rock.Model.Person, Parameter: Person
            RockMigrationHelper.DeletePageContext( "6B77984D-7113-4412-AF12-27C13F28049B" );
        }
    }
}
