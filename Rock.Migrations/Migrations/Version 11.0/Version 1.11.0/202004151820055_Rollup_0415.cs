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
    public partial class Rollup_0415 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();

            UpdateGroupMemberForSystemCommunication();
            UpdateAssessmentResultsLavaBarCharts();
            RemovePagenamefromSystemCommunicationDetailBreadcrumbs();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();
        }

        /// <summary>
        /// Script generated "Up" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Cms.Hero", "Hero", "Rock.Blocks.Types.Mobile.Cms.Hero, Rock, Version=1.11.0.13, Culture=neutral, PublicKeyToken=null", false, false, SystemGuid.EntityType.MOBILE_CMS_HERO_BLOCK_TYPE );
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Events.CalendarEventList", "Calendar Event List", "Rock.Blocks.Types.Mobile.Events.CalendarEventList, Rock, Version=1.11.0.13, Culture=neutral, PublicKeyToken=null", false, false, SystemGuid.EntityType.MOBILE_EVENTS_CALENDAREVENTLIST_BLOCK_TYPE );
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Events.CalendarView", "Calendar View", "Rock.Blocks.Types.Mobile.Events.CalendarView, Rock, Version=1.11.0.13, Culture=neutral, PublicKeyToken=null", false, false, SystemGuid.EntityType.MOBILE_EVENTS_CALENDARVIEW_BLOCK_TYPE );
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Events.CommunicationListSubscribe", "Communication List Subscribe", "Rock.Blocks.Types.Mobile.Events.CommunicationListSubscribe, Rock, Version=1.11.0.13, Culture=neutral, PublicKeyToken=null", false, false, SystemGuid.EntityType.MOBILE_EVENTS_COMMUNICATION_LIST_SUBSCRIBE_BLOCK_TYPE );
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Events.PrayerSession", "Prayer Session", "Rock.Blocks.Types.Mobile.Events.PrayerSession, Rock, Version=1.11.0.13, Culture=neutral, PublicKeyToken=null", false, false, SystemGuid.EntityType.MOBILE_EVENTS_PRAYER_SESSION_BLOCK_TYPE );
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Events.PrayerSessionSetup", "Prayer Session Setup", "Rock.Blocks.Types.Mobile.Events.PrayerSessionSetup, Rock, Version=1.11.0.13, Culture=neutral, PublicKeyToken=null", false, false, SystemGuid.EntityType.MOBILE_EVENTS_PRAYER_SESSION_SETUP_BLOCK_TYPE );
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Groups.GroupAttendanceEntry", "Group Attendance Entry", "Rock.Blocks.Types.Mobile.Groups.GroupAttendanceEntry, Rock, Version=1.11.0.13, Culture=neutral, PublicKeyToken=null", false, false, SystemGuid.EntityType.MOBILE_GROUPS_GROUP_ATTENDANCE_ENTRY_BLOCK_TYPE );
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Groups.GroupEdit", "Group Edit", "Rock.Blocks.Types.Mobile.Groups.GroupEdit, Rock, Version=1.11.0.13, Culture=neutral, PublicKeyToken=null", false, false, SystemGuid.EntityType.MOBILE_GROUPS_GROUP_EDIT_BLOCK_TYPE );
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Groups.GroupMemberEdit", "Group Member Edit", "Rock.Blocks.Types.Mobile.Groups.GroupMemberEdit, Rock, Version=1.11.0.13, Culture=neutral, PublicKeyToken=null", false, false, SystemGuid.EntityType.MOBILE_GROUPS_GROUP_MEMBER_EDIT_BLOCK_TYPE );
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Groups.GroupMemberList", "Group Member List", "Rock.Blocks.Types.Mobile.Groups.GroupMemberList, Rock, Version=1.11.0.13, Culture=neutral, PublicKeyToken=null", false, false, SystemGuid.EntityType.MOBILE_GROUPS_GROUP_MEMBER_LIST_BLOCK_TYPE );
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Groups.GroupMemberView", "Group Member View", "Rock.Blocks.Types.Mobile.Groups.GroupMemberView, Rock, Version=1.11.0.13, Culture=neutral, PublicKeyToken=null", false, false, SystemGuid.EntityType.MOBILE_GROUPS_GROUP_MEMBER_VIEW_BLOCK_TYPE );
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Groups.GroupView", "Group View", "Rock.Blocks.Types.Mobile.Groups.GroupView, Rock, Version=1.11.0.13, Culture=neutral, PublicKeyToken=null", false, false, SystemGuid.EntityType.MOBILE_GROUPS_GROUP_VIEW_BLOCK_TYPE );
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Prayer.PrayerRequestDetails", "Prayer Request Details", "Rock.Blocks.Types.Mobile.Prayer.PrayerRequestDetails, Rock, Version=1.11.0.13, Culture=neutral, PublicKeyToken=null", false, false, SystemGuid.EntityType.MOBILE_PRAYER_PRAYER_REQUEST_DETAILS_BLOCK_TYPE );

            RockMigrationHelper.UpdateMobileBlockType( "Hero", "Displays an image with text overlay on the page.", "Rock.Blocks.Types.Mobile.Cms.Hero", "Mobile > Cms", "A8597994-BD47-4A15-8BB1-4B508977665F" );
            RockMigrationHelper.UpdateMobileBlockType( "Calendar Event List", "Displays a list of events from a calendar.", "Rock.Blocks.Types.Mobile.Events.CalendarEventList", "Mobile > Events", "A9149623-6A82-4F25-8F4D-0961557BE78C" );
            RockMigrationHelper.UpdateMobileBlockType( "Calendar View", "Views events from a calendar.", "Rock.Blocks.Types.Mobile.Events.CalendarView", "Mobile > Events", "14B447B3-6117-4142-92E7-E3F289106140" );
            RockMigrationHelper.UpdateMobileBlockType( "Communication List Subscribe", "Allows the user to subscribe or unsubscribe from specific communication lists.", "Rock.Blocks.Types.Mobile.Events.CommunicationListSubscribe", "Mobile > Communication", "D0C51784-71ED-46F3-86AB-972148B78BE8" );
            RockMigrationHelper.UpdateMobileBlockType( "Prayer Session", "Allows the user to read through and pray for prayer requests.", "Rock.Blocks.Types.Mobile.Events.PrayerSession", "Mobile > Prayer", "420DEA5F-9ABC-4E59-A9BD-DCA972657B84" );
            RockMigrationHelper.UpdateMobileBlockType( "Prayer Session Setup", "Displays a page to configure and prepare a prayer session.", "Rock.Blocks.Types.Mobile.Events.PrayerSessionSetup", "Mobile > Prayer", "4A3B0D13-FC32-4354-A224-9D450F860BE9" );
            RockMigrationHelper.UpdateMobileBlockType( "Group Attendance Entry", "Allows the user to mark attendance for a group.", "Rock.Blocks.Types.Mobile.Groups.GroupAttendanceEntry", "Mobile > Groups", "08AE409C-9E4C-42D1-A93C-A554A3EEA0C3" );
            RockMigrationHelper.UpdateMobileBlockType( "Group Edit", "Edits the basic settings of a group.", "Rock.Blocks.Types.Mobile.Groups.GroupEdit", "Mobile > Groups", "FEC66374-E38F-4651-BAA6-AC658409D9BD" );
            RockMigrationHelper.UpdateMobileBlockType( "Group Member Edit", "Edits a member of a group.", "Rock.Blocks.Types.Mobile.Groups.GroupMemberEdit", "Mobile > Groups", "514B533A-8970-4628-A4C8-35388CD869BC" );
            RockMigrationHelper.UpdateMobileBlockType( "Group Member List", "Allows the user to view a list of members in a group.", "Rock.Blocks.Types.Mobile.Groups.GroupMemberList", "Mobile > Groups", "5A6D2ADB-03A7-4B55-8EAA-26A37116BFF1" );
            RockMigrationHelper.UpdateMobileBlockType( "Group Member View", "Allows the user to view the details about a specific group member.", "Rock.Blocks.Types.Mobile.Groups.GroupMemberView", "Mobile > Groups", "6B3C23EA-A1C2-46FA-9F04-5B0BD004ED8B" );
            RockMigrationHelper.UpdateMobileBlockType( "Group View", "Allows the user to view the details about a group.", "Rock.Blocks.Types.Mobile.Groups.GroupView", "Mobile > Groups", "3F34AE03-9378-4363-A232-0318139C3BD3" );
            RockMigrationHelper.UpdateMobileBlockType( "Prayer Request Details", "Edits an existing prayer request or creates a new one.", "Rock.Blocks.Types.Mobile.Prayer.PrayerRequestDetails", "Mobile > Prayer", "EBB91B46-292E-4784-9E37-38781C714008" );
            // Attrib for BlockType: Hero:Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8597994-BD47-4A15-8BB1-4B508977665F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "Title", @"The main title to display over the image. <span class='tip tip-lava'></span>", 0, @"", "D9132EAB-91FC-4061-8881-1D2618E84F52" );
            // Attrib for BlockType: Hero:Subtitle
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8597994-BD47-4A15-8BB1-4B508977665F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subtitle", "Subtitle", "Subtitle", @"The subtitle to display over the image. <span class='tip tip-lava'></span>", 1, @"", "8BD8BC79-10F7-4889-B952-1789BE7A8804" );
            // Attrib for BlockType: Hero:Title Color
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8597994-BD47-4A15-8BB1-4B508977665F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title Color", "TitleColor", "Title Color", @"Will override the theme's hero title (.hero-title) color.", 7, @"", "9C95201E-64AC-4A36-8957-571C7BCA8F5B" );
            // Attrib for BlockType: Hero:Subtitle Color
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8597994-BD47-4A15-8BB1-4B508977665F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subtitle Color", "SubtitleColor", "Subtitle Color", @"Will override the theme's hero subtitle (.hero-subtitle) color.", 8, @"", "5374CF14-6A4F-4CA8-9561-0A1003F28504" );
            // Attrib for BlockType: Prayer Session:Prayed Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "420DEA5F-9ABC-4E59-A9BD-DCA972657B84", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Prayed Button Text", "PrayedButtonText", "Prayed Button Text", @"The text to display inside the Prayed button. Available in the XAML template as lava variable 'PrayedButtonText'.", 0, @"I've Prayed", "7D3AE194-069E-4CE8-97C9-EB7A9CADA7B9" );
            // Attrib for BlockType: Group Member List:Title Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5A6D2ADB-03A7-4B55-8EAA-26A37116BFF1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title Template", "TitleTemplate", "Title Template", @"The value to use when rendering the title text. <span class='tip tip-lava'></span>", 1, @"{{ Group.Name }} Group Roster", "FB6FA5A4-74C7-4E17-8764-118C01FCD192" );
            // Attrib for BlockType: Group Member List:Additional Fields
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5A6D2ADB-03A7-4B55-8EAA-26A37116BFF1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Additional Fields", "AdditionalFields", "Additional Fields", @"", 3, @"", "D5942AD0-5EA5-4ACA-8D5F-66FDE86E2E61" );
            // Attrib for BlockType: Logs:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6059FC03-E398-4359-8632-909B63FFA550", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "6EB8AD29-2129-4DFC-9A4C-08A886C1DA30" );
            // Attrib for BlockType: Page Views:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "38C775A7-5CDC-415E-9595-76221354A999", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "C8E2BA67-B482-4676-8B02-AE402008A17B" );
            // Attrib for BlockType: Achievement Attempt List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9C1236AE-4FF0-480C-A7DF-0E5277CA75FB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "40C93EB0-0A68-428A-85AE-EFD4FBE7F945" );
            // Attrib for BlockType: Achievement Type List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D2F918CF-F63C-4643-A02A-FD3DE7C5CFFD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "FD012DC0-9002-4967-B08B-B6C8D0DDBF92" );
            // Attrib for BlockType: Calendar View:Show Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "14B447B3-6117-4142-92E7-E3F289106140", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Filter", "ShowFilter", "Show Filter", @"If enabled then the user will be able to apply custom filtering.", 4, @"True", "9E75484E-9F6B-4E36-A73A-C1AE06A48CE1" );
            // Attrib for BlockType: Communication List Subscribe:Show Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D0C51784-71ED-46F3-86AB-972148B78BE8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Description", "ShowDescription", "Show Description", @"If enabled then the description of the communication list will be shown.", 1, @"False", "E12CE061-2475-41A2-BDB7-12F03853B18B" );
            // Attrib for BlockType: Prayer Session:Show Follow Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "420DEA5F-9ABC-4E59-A9BD-DCA972657B84", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Follow Button", "ShowFollowButton", "Show Follow Button", @"Indicates if the Follow button should be shown. Available in the XAML template as lava variable 'ShowFollowButton'.", 1, @"True", "2E37A96E-9BF5-484A-8146-6E54629080CC" );
            // Attrib for BlockType: Prayer Session:Show Inappropriate Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "420DEA5F-9ABC-4E59-A9BD-DCA972657B84", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Inappropriate Button", "ShowInappropriateButton", "Show Inappropriate Button", @"Indicates if the button to flag a request as inappropriate should be shown. Available in the XAML template as lava variable 'ShowInappropriateButton'.", 2, @"True", "52E1A831-4039-4185-8261-B39083B39C46" );
            // Attrib for BlockType: Prayer Session:Public Only
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "420DEA5F-9ABC-4E59-A9BD-DCA972657B84", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Public Only", "PublicOnly", "Public Only", @"If enabled then only prayer requests marked as public will be shown.", 3, @"False", "B10629F9-DCBC-4601-A717-89B34F7ED5DE" );
            // Attrib for BlockType: Prayer Session Setup:Show Campus Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4A3B0D13-FC32-4354-A224-9D450F860BE9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus Filter", "ShowCampusFilter", "Show Campus Filter", @"If enabled and the user has a primary campus, then the user will be offered to limit prayer requests to just their campus.", 2, @"False", "B0B72E6D-6D4B-4C29-A3B4-2B01A8E756B1" );
            // Attrib for BlockType: Group Attendance Entry:Show Save Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "08AE409C-9E4C-42D1-A93C-A554A3EEA0C3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Save Button", "ShowSaveButton", "Show Save Button", @"If enabled a save button will be shown (recommended for large groups), otherwise no save button will be displayed and a save will be triggered with each selection (recommended for smaller groups).", 3, @"False", "11AE7EDA-3EE4-4775-9812-E61E8DAD0681" );
            // Attrib for BlockType: Group Attendance Entry:Allow Any Date Selection
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "08AE409C-9E4C-42D1-A93C-A554A3EEA0C3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Any Date Selection", "AllowAnyDateSelection", "Allow Any Date Selection", @"If enabled a date picker will be shown, otherwise a dropdown with only the valid dates will be shown.", 4, @"False", "01816306-DE0C-491B-81C1-653EE29C37B2" );
            // Attrib for BlockType: Group Edit:Show Group Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FEC66374-E38F-4651-BAA6-AC658409D9BD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Name", "ShowGroupName", "Show Group Name", @"", 0, @"True", "EB6741F0-22E6-4B90-BEC9-1929CE215B45" );
            // Attrib for BlockType: Group Edit:Enable Group Name Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FEC66374-E38F-4651-BAA6-AC658409D9BD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Group Name Edit", "EnableGroupNameEdit", "Enable Group Name Edit", @"", 1, @"True", "1149943B-BF00-4338-AB46-B62933FDB557" );
            // Attrib for BlockType: Group Edit:Show Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FEC66374-E38F-4651-BAA6-AC658409D9BD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Description", "ShowDescription", "Show Description", @"", 2, @"True", "C894DB28-D08E-4649-9D41-54938D350950" );
            // Attrib for BlockType: Group Edit:Enable Description Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FEC66374-E38F-4651-BAA6-AC658409D9BD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Description Edit", "EnableDescriptionEdit", "Enable Description Edit", @"", 3, @"True", "86CAFA5C-086A-4993-ADCE-B82024F90959" );
            // Attrib for BlockType: Group Edit:Show Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FEC66374-E38F-4651-BAA6-AC658409D9BD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus", "ShowCampus", "Show Campus", @"", 4, @"True", "20BB29E2-45FA-469C-9266-88A30EA4D13A" );
            // Attrib for BlockType: Group Edit:Enable Campus Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FEC66374-E38F-4651-BAA6-AC658409D9BD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Campus Edit", "EnableCampusEdit", "Enable Campus Edit", @"", 5, @"True", "4875251A-8B4B-4C5D-B630-E8D809260945" );
            // Attrib for BlockType: Group Edit:Show Group Capacity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FEC66374-E38F-4651-BAA6-AC658409D9BD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Capacity", "ShowGroupCapacity", "Show Group Capacity", @"", 6, @"True", "DE4B5956-CB26-48BB-9C40-488A944B3701" );
            // Attrib for BlockType: Group Edit:Enable Group Capacity Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FEC66374-E38F-4651-BAA6-AC658409D9BD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Group Capacity Edit", "EnableGroupCapacityEdit", "Enable Group Capacity Edit", @"", 7, @"True", "24DD3040-6CE5-46A1-BC3C-5E4F1782436F" );
            // Attrib for BlockType: Group Edit:Show Active Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FEC66374-E38F-4651-BAA6-AC658409D9BD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Active Status", "ShowActiveStatus", "Show Active Status", @"", 8, @"True", "C0BAF201-4D44-47E2-91C8-9FD89CCFC558" );
            // Attrib for BlockType: Group Edit:Enable Active Status Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FEC66374-E38F-4651-BAA6-AC658409D9BD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Active Status Edit", "EnableActiveStatusEdit", "Enable Active Status Edit", @"", 9, @"True", "F4C2FAB6-33FB-4760-9721-7A1EBA714CEA" );
            // Attrib for BlockType: Group Edit:Show Public Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FEC66374-E38F-4651-BAA6-AC658409D9BD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Public Status", "ShowPublicStatus", "Show Public Status", @"", 10, @"True", "5043F4B3-1465-4BAD-8375-67CF0F2BC928" );
            // Attrib for BlockType: Group Edit:Enable Public Status Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FEC66374-E38F-4651-BAA6-AC658409D9BD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Public Status Edit", "EnablePublicStatusEdit", "Enable Public Status Edit", @"", 11, @"True", "DB9BFD83-5B62-4B07-9747-15A46656F23A" );
            // Attrib for BlockType: Group Member Edit:Allow Role Change
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "514B533A-8970-4628-A4C8-35388CD869BC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Role Change", "AllowRoleChange", "Allow Role Change", @"", 0, @"True", "A247DFCC-895A-41A5-B02E-4F0D9679C4A6" );
            // Attrib for BlockType: Group Member Edit:Allow Member Status Change
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "514B533A-8970-4628-A4C8-35388CD869BC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Member Status Change", "AllowMemberStatusChange", "Allow Member Status Change", @"", 1, @"True", "A7A59697-82A9-4368-9A32-103BB1C32609" );
            // Attrib for BlockType: Group Member Edit:Allow Note Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "514B533A-8970-4628-A4C8-35388CD869BC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Note Edit", "AllowNoteEdit", "Allow Note Edit", @"", 2, @"True", "AB00A40C-5AE0-42FD-8198-7B90C727A81C" );
            // Attrib for BlockType: Group View:Show Leader List
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3F34AE03-9378-4363-A232-0318139C3BD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Leader List", "ShowLeaderList", "Show Leader List", @"Specifies if the leader list should be shown, this value is made available to the Template as ShowLeaderList.", 1, @"True", "808D607F-D097-48C5-BC3A-988141A1C69C" );
            // Attrib for BlockType: Prayer Request Details:Show Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EBB91B46-292E-4784-9E37-38781C714008", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Category", "EnableCategory", "Show Category", @"If disabled, then the user will not be able to select a category and the default category will be used exclusively.", 0, @"True", "1D63D8F5-A810-477D-B9F7-24B33546990F" );
            // Attrib for BlockType: Prayer Request Details:Enable Auto Approve
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EBB91B46-292E-4784-9E37-38781C714008", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Auto Approve", "EnableAutoApprove", "Enable Auto Approve", @"If enabled, prayer requests are automatically approved; otherwise they must be approved by an admin before they can be seen by the prayer team.", 0, @"True", "42F19547-E84A-4CD8-9787-7A74255E73E4" );
            // Attrib for BlockType: Prayer Request Details:Show Urgent Flag
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EBB91B46-292E-4784-9E37-38781C714008", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Urgent Flag", "EnableUrgentFlag", "Show Urgent Flag", @"If enabled, requestors will be able to flag prayer requests as urgent.", 2, @"False", "50F41A0F-858E-46D9-AF3B-506E8AB2F9E3" );
            // Attrib for BlockType: Prayer Request Details:Show Public Display Flag
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EBB91B46-292E-4784-9E37-38781C714008", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Public Display Flag", "EnablePublicDisplayFlag", "Show Public Display Flag", @"If enabled, requestors will be able set whether or not they want their request displayed on the public website.", 3, @"False", "0C0898A4-0554-420E-BA3D-BD22AB8C44CA" );
            // Attrib for BlockType: Prayer Request Details:Default To Public
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EBB91B46-292E-4784-9E37-38781C714008", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default To Public", "DefaultToPublic", "Default To Public", @"If enabled, all prayers will be set to public by default.", 4, @"False", "28332C3D-E2B2-4D6D-B8B3-9BE11FF07504" );
            // Attrib for BlockType: Prayer Request Details:Show Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EBB91B46-292E-4784-9E37-38781C714008", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus", "EnableCampus", "Show Campus", @"Should the campus field be displayed? If there is only one active campus then the campus field will not show.", 6, @"True", "C91F9877-5605-48F8-B6CA-A797F998826D" );
            // Attrib for BlockType: Prayer Request Details:Require Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EBB91B46-292E-4784-9E37-38781C714008", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Campus", "RequireCampus", "Require Campus", @"Require that a campus be selected. The campus will not be displayed if there is only one available campus, in which case if this is set to true then the single campus is automatically used.", 7, @"False", "92258CC6-9E8E-4DF3-AB5B-48114BE28BA7" );
            // Attrib for BlockType: Prayer Request Details:Require Last Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EBB91B46-292E-4784-9E37-38781C714008", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Last Name", "RequireLastName", "Require Last Name", @"Require that a last name be entered. First name is always required.", 8, @"True", "E5681E4D-7CD7-475F-9F74-7E0C295BE538" );
            // Attrib for BlockType: Prayer Request Details:Enable Person Matching
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EBB91B46-292E-4784-9E37-38781C714008", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Person Matching", "EnablePersonMatching", "Enable Person Matching", @"If enabled, the request will be linked to an existing person if a match can be made between the requester and an existing person.", 9, @"False", "238CFE61-56D7-47E2-AC5D-66B3B645D3FE" );
            // Attrib for BlockType: Logs:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6059FC03-E398-4359-8632-909B63FFA550", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "A7679E6E-54DF-4906-B809-C84C3724B0EF" );
            // Attrib for BlockType: Page Views:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "38C775A7-5CDC-415E-9595-76221354A999", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "6083EF39-DB05-482B-ADC1-DC03F9D0F38B" );
            // Attrib for BlockType: Achievement Attempt List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9C1236AE-4FF0-480C-A7DF-0E5277CA75FB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "1E90E6A4-177A-447B-8552-6B1395344982" );
            // Attrib for BlockType: Achievement Type List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D2F918CF-F63C-4643-A02A-FD3DE7C5CFFD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "D7F5D3B3-BABD-49B8-A63D-94D24064666A" );
            // Attrib for BlockType: Hero:Text Align
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8597994-BD47-4A15-8BB1-4B508977665F", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Text Align", "HorizontalTextAlign", "Text Align", @"", 6, @"Center", "F3F7B68A-6926-4D89-BA28-0FB99E940A8E" );
            // Attrib for BlockType: Prayer Request Details:Completion Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EBB91B46-292E-4784-9E37-38781C714008", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Completion Action", "CompletionAction", "Completion Action", @"What action to perform after saving the prayer request.", 0, @"0", "22BCE7BA-ED45-480B-9833-03A503B825ED" );
            // Attrib for BlockType: Hero:Height - Phone
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8597994-BD47-4A15-8BB1-4B508977665F", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Height - Phone", "ImageHeightPhone", "Height - Phone", @"", 4, @"200", "6527442E-539A-4486-B0C8-6D8C48F77FE8" );
            // Attrib for BlockType: Hero:Height - Tablet
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8597994-BD47-4A15-8BB1-4B508977665F", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Height - Tablet", "ImageHeightTablet", "Height - Tablet", @"", 5, @"350", "BF0B4848-9193-45EA-9A13-E0DE5573A64F" );
            // Attrib for BlockType: Hero:Padding
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8597994-BD47-4A15-8BB1-4B508977665F", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Padding", "Padding", "Padding", @"The padding around the inside of the image.", 9, @"20", "92DBA471-CFBC-4BC2-BD17-022663670328" );
            // Attrib for BlockType: Prayer Session:Inappropriate Flag Limit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "420DEA5F-9ABC-4E59-A9BD-DCA972657B84", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Inappropriate Flag Limit", "InappropriateFlagLimit", "Inappropriate Flag Limit", @"The number of flags a prayer request has to get from the prayer team before it is automatically unapproved.", 4, @"", "7171FEA9-1711-4E40-965B-348D56B604C8" );
            // Attrib for BlockType: Group Attendance Entry:Number of Days Forward to Allow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "08AE409C-9E4C-42D1-A93C-A554A3EEA0C3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Days Forward to Allow", "NumberOfDaysForwardToAllow", "Number of Days Forward to Allow", @"", 0, @"0", "CF519120-1ABA-4261-B0ED-BC26C866F88F" );
            // Attrib for BlockType: Group Attendance Entry:Number of Days Back to Allow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "08AE409C-9E4C-42D1-A93C-A554A3EEA0C3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Days Back to Allow", "NumberOfDaysBackToAllow", "Number of Days Back to Allow", @"", 1, @"30", "F0E13D10-D713-454E-8607-02DD35CDDB46" );
            // Attrib for BlockType: Prayer Request Details:Expires After (Days)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EBB91B46-292E-4784-9E37-38781C714008", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Expires After (Days)", "ExpiresAfterDays", "Expires After (Days)", @"Number of days until the request will expire (only applies when auto-approved is enabled).", 1, @"14", "4389C4A7-CF8D-4750-812A-F4C2BD602E96" );
            // Attrib for BlockType: Prayer Request Details:Character Limit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EBB91B46-292E-4784-9E37-38781C714008", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Character Limit", "CharacterLimit", "Character Limit", @"If set to something other than 0, this will limit the number of characters allowed when entering a new prayer request.", 5, @"250", "54273807-6E5F-47DF-A803-6251C6E2480A" );
            // Attrib for BlockType: Calendar Event List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A9149623-6A82-4F25-8F4D-0961557BE78C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to push onto the navigation stack when viewing details of an event.", 1, @"", "3EE14F0B-C3EA-49CF-8F08-EF839F428762" );
            // Attrib for BlockType: Calendar View:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "14B447B3-6117-4142-92E7-E3F289106140", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to push onto the navigation stack when viewing details of an event.", 1, @"", "91A13A0A-2D7A-45AE-BF09-D897C280C4E1" );
            // Attrib for BlockType: Prayer Session Setup:Prayer Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4A3B0D13-FC32-4354-A224-9D450F860BE9", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Prayer Page", "PrayerPage", "Prayer Page", @"The page to push onto the navigation stack to begin the prayer session.", 0, @"", "6EEED035-2E74-44EF-8567-016C2CBC3E6D" );
            // Attrib for BlockType: Group Attendance Entry:Save Redirect Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "08AE409C-9E4C-42D1-A93C-A554A3EEA0C3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Save Redirect Page", "SaveRedirectPage", "Save Redirect Page", @"If set, redirect user to this page on save. If not set, page is popped off the navigation stack.", 2, @"", "D4E32F4E-4679-40CE-B19D-B252D170DEDF" );
            // Attrib for BlockType: Group Edit:Group Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FEC66374-E38F-4651-BAA6-AC658409D9BD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "Group Detail Page", @"The group detail page to return to, if not set then the edit page is popped off the navigation stack.", 13, @"", "1C3856C1-E5C9-4CDC-B759-9F902B458911" );
            // Attrib for BlockType: Group Member Edit:Member Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "514B533A-8970-4628-A4C8-35388CD869BC", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Member Detail Page", "MemberDetailsPage", "Member Detail Page", @"The group member page to return to, if not set then the edit page is popped off the navigation stack.", 4, @"", "C61DE82E-2B0D-49EC-865E-6507DAD1BAF2" );
            // Attrib for BlockType: Group Member List:Group Member Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5A6D2ADB-03A7-4B55-8EAA-26A37116BFF1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Member Detail Page", "GroupMemberDetailPage", "Group Member Detail Page", @"The page that will display the group member details when selecting a member.", 0, @"", "D5629B9E-59EE-40D4-B4BA-E23DFAC33D61" );
            // Attrib for BlockType: Group Member View:Group Member Edit Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6B3C23EA-A1C2-46FA-9F04-5B0BD004ED8B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Member Edit Page", "GroupMemberEditPage", "Group Member Edit Page", @"The page that will allow editing of a group member.", 0, @"", "636417C3-365C-488D-8072-406C3C2EA878" );
            // Attrib for BlockType: Group View:Group Edit Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3F34AE03-9378-4363-A232-0318139C3BD3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Edit Page", "GroupEditPage", "Group Edit Page", @"The page that will allow editing of the group.", 0, @"", "386360FF-9E79-4E0C-92D5-7285484FFADF" );
            // Attrib for BlockType: Calendar View:Audience Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "14B447B3-6117-4142-92E7-E3F289106140", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Audience Filter", "AudienceFilter", "Audience Filter", @"Determines which audiences should be displayed in the filter.", 2, @"", "0BA3BAB4-92EA-4768-9B5D-47E51B39FE04" );
            // Attrib for BlockType: Hero:Background Image - Phone
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8597994-BD47-4A15-8BB1-4B508977665F", "6F9E2DD0-E39E-4602-ADF9-EB710A75304A", "Background Image - Phone", "BackgroundImagePhone", "Background Image - Phone", @"Recommended size is at least 1024px wide and double the height specified below.", 2, @"", "D807B220-49A0-49EE-B4F0-F0A92ED3160F" );
            // Attrib for BlockType: Hero:Background Image - Tablet
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8597994-BD47-4A15-8BB1-4B508977665F", "6F9E2DD0-E39E-4602-ADF9-EB710A75304A", "Background Image - Tablet", "BackgroundImageTablet", "Background Image - Tablet", @"Recommended size is at least 2048px wide and double the height specified below.", 3, @"", "B7A56FAF-90C6-4F79-898C-6F1FAEDF19B5" );
            // Attrib for BlockType: Prayer Request Details:Workflow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EBB91B46-292E-4784-9E37-38781C714008", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "Workflow", @"An optional workflow to start when prayer request is created. The PrayerRequest will be set as the workflow 'Entity' attribute when processing is started.", 2, @"", "7DBEB6DA-8417-49C9-968A-154E1C41C8ED" );
            // Attrib for BlockType: Communication List Subscribe:Communication List Categories
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D0C51784-71ED-46F3-86AB-972148B78BE8", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Communication List Categories", "CommunicationListCategories", "Communication List Categories", @"Select the categories of the communication lists to display, or select none to show all that the user is authorized to view.", 0, @"", "1E983A67-A9DC-4F91-8862-3E55613A95DC" );
            // Attrib for BlockType: Prayer Session Setup:Parent Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4A3B0D13-FC32-4354-A224-9D450F860BE9", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Parent Category", "ParentCategory", "Parent Category", @"The parent category to use as the root category available for the user to pick from.", 1, @"", "E177F02C-B1BF-4139-B354-E3C4EFFF9A0B" );
            // Attrib for BlockType: Group Edit:Attribute Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FEC66374-E38F-4651-BAA6-AC658409D9BD", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Attribute Category", "AttributeCategory", "Attribute Category", @"Category of attributes to show and allow editing on.", 12, @"", "5C495667-1C02-4244-A1ED-1AB26AC35B5C" );
            // Attrib for BlockType: Group Member Edit:Attribute Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "514B533A-8970-4628-A4C8-35388CD869BC", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Attribute Category", "AttributeCategory", "Attribute Category", @"Category of attributes to show and allow editing on.", 3, @"", "E6C90ACE-57D9-441A-A764-F8E0D36B433E" );
            // Attrib for BlockType: Prayer Request Details:Parent Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EBB91B46-292E-4784-9E37-38781C714008", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Parent Category", "ParentCategory", "Parent Category", @"A top level category. This controls which categories the person can choose from when entering their prayer request.", 1, @"", "26EB9054-E675-4DD6-80EF-D5481F23CF51" );
            // Attrib for BlockType: Prayer Request Details:Default Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EBB91B46-292E-4784-9E37-38781C714008", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Default Category", "DefaultCategory", "Default Category", @"The default category to use for all new prayer requests.", 2, @"", "80ABC48F-99E7-404F-AAB3-0EA8551010CF" );
            // Attrib for BlockType: Calendar Event List:Day Header Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A9149623-6A82-4F25-8F4D-0961557BE78C", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Day Header Template", "DayHeaderTemplate", "Day Header Template", @"The XAML to use when rendering the day header above a grouping of events.", 3, @"<Frame HasShadow=""false"" StyleClass=""calendar-events-day"">
                <Label Text=""{Binding ., StringFormat=""{0:dddd MMMM d}""}"" />
            </Frame>
            ", "57556A66-7AA0-4096-8245-5C76D73F34FB" );
            // Attrib for BlockType: Calendar View:Event Summary
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "14B447B3-6117-4142-92E7-E3F289106140", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Event Summary", "EventSummary", "Event Summary", @"The XAML to use when rendering the event summaries below the calendar.", 3, @"<Frame HasShadow=""false"" StyleClass=""calendar-event"">
                <StackLayout Spacing=""0"">
                    <Label StyleClass=""calendar-event-title"" Text=""{Binding Name}"" />
                    {% if Item.EndDateTime == null %}
                        <Label StyleClass=""calendar-event-text"" Text=""{{ Item.StartDateTime | Date:'h:mm tt' }}"" LineBreakMode=""NoWrap"" />
                    {% else %}
                        <Label StyleClass=""calendar-event-text"" Text=""{{ Item.StartDateTime | Date:'h:mm tt' }} - {{ Item.EndDateTime | Date:'h:mm tt' }}"" LineBreakMode=""NoWrap"" />
                    {% endif %}
                    <StackLayout Orientation=""Horizontal"">
                        <Label HorizontalOptions=""FillAndExpand"" StyleClass=""calendar-event-audience"" Text=""{{ Item.Audiences | Select:'Name' | Join:', ' }}"" />
                        <Label StyleClass=""calendar-event-campus"" Text=""{{ Item.Campus }}"" HorizontalTextAlignment=""End"" LineBreakMode=""NoWrap"" />
                    </StackLayout>
                </StackLayout>
            </Frame>
            ", "497A6BD6-D36C-4AC8-AF83-9015EAF43C89" );
            // Attrib for BlockType: Prayer Request Details:Completion Xaml
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EBB91B46-292E-4784-9E37-38781C714008", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Completion Xaml", "CompletionXaml", "Completion Xaml", @"The XAML markup that will be used if the. <span class='tip tip-lava'></span>", 1, @"<Rock:NotificationBox NotificationType=""Success"">
                Thank you for allowing us to pray for you.
            </Rock:NotificationBox>", "949F7A76-8947-4457-B742-1AC1F2EC2486" );
            // Attrib for BlockType: Calendar Event List:Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A9149623-6A82-4F25-8F4D-0961557BE78C", "EC0D9528-1A22-404E-A776-566404987363", "Calendar", "Calendar", "Calendar", @"The calendar to pull events from", 0, @"", "8BCE51C8-3757-42A4-998D-F5919FF2BB16" );
            // Attrib for BlockType: Calendar View:Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "14B447B3-6117-4142-92E7-E3F289106140", "EC0D9528-1A22-404E-A776-566404987363", "Calendar", "Calendar", "Calendar", @"The calendar to pull events from", 0, @"", "694244DC-5067-4A34-98F3-85FED7052E18" );
            // Attrib for BlockType: Calendar Event List:Event Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A9149623-6A82-4F25-8F4D-0961557BE78C", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Event Template", "EventTemplate", "Event Template", @"The template to use when rendering event items.", 2, @"", "8B7B6464-4E2E-4312-8414-2118663E74D8" );
            // Attrib for BlockType: Prayer Session:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "420DEA5F-9ABC-4E59-A9BD-DCA972657B84", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering prayer requests.", 5, @"", "AF310A62-C62A-427A-A88E-48AA885CCCE7" );
            // Attrib for BlockType: Group Member List:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5A6D2ADB-03A7-4B55-8EAA-26A37116BFF1", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 2, @"", "4B1F3DAE-180E-45E6-BC36-2BEA0A03C674" );
            // Attrib for BlockType: Group Member View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6B3C23EA-A1C2-46FA-9F04-5B0BD004ED8B", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 1, @"", "AD4CCF58-A93D-49C7-860D-02BC0F966724" );
            // Attrib for BlockType: Group View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3F34AE03-9378-4363-A232-0318139C3BD3", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 2, @"", "A3826811-395A-4564-8101-EB95936065FB" );
        }

        /// <summary>
        /// SK: Create Similar Migration for SystemCommunication record too
        /// </summary>
        private void UpdateGroupMemberForSystemCommunication()
        {
            string newValue = "{%- assign mobilePhone = pendingIndividual.PhoneNumbers | Where:'NumberTypeValueId', 12 | Select:'NumberFormatted' -%}".Replace( "'", "''" );
            string oldValue = "{%- assign mobilePhone = pendingIndividual.PhoneNumbers | Where:'NumberTypeValueId', 136 | Select:'NumberFormatted' -%}".Replace( "'", "''" );

            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "Body" );

            Sql( $@"UPDATE [dbo].[SystemCommunication] 
                    SET [Body] = REPLACE({targetColumn}, '{oldValue}', '{newValue}')
                    WHERE {targetColumn} LIKE '%{oldValue}%'
                            AND [Guid] = '18521B26-1C7D-E287-487D-97D176CA4986'" );

            newValue = "{%- assign mobilePhone =absentMember.PhoneNumbers | Where:'NumberTypeValueId', 12 | Select:'NumberFormatted' -%}".Replace( "'", "''" );
            oldValue = "{%- assign mobilePhone =absentMember.PhoneNumbers | Where:'NumberTypeValueId', 136 | Select:'NumberFormatted' -%}".Replace( "'", "''" );

            Sql( $@"UPDATE [dbo].[SystemCommunication] 
                    SET [Body] = REPLACE({targetColumn}, '{oldValue}', '{newValue}')
                    WHERE {targetColumn} LIKE '%{oldValue}%'
                            AND [Guid] = '8747131E-3EDA-4FB0-A484-C2D2BE3918BA'" );
        }

        /// <summary>
        /// JH: Update bar charts within Assessment results Lava to use zero for the x and y minimum values.
        /// </summary>
        private void UpdateAssessmentResultsLavaBarCharts()
        {
            RockMigrationHelper.UpdateBlockTypeAttribute( SystemGuid.BlockType.CONFLICT_PROFILE, SystemGuid.FieldType.CODE_EDITOR, "Results Message", "ResultsMessage", "", @"The text (HTML) to display at the top of the results section.<span class='tip tip-lava'></span><span class='tip tip-html'></span>", 1, @"<h2>Conflict Engagement Profile Results</h2>
<p>
   {{ Person.NickName }}, here are your conflict engagement results.
   You will rank high, medium or low in each of the following five modes.
</p>

{[ chart type:'bar' yaxismin:'0' ]}
    [[ dataitem label:'Winning' value:'{{Winning}}' fillcolor:'#E15759' ]] [[ enddataitem ]]
    [[ dataitem label:'Resolving' value:'{{Resolving}}' fillcolor:'#5585B7' ]] [[ enddataitem ]]
    [[ dataitem label:'Compromising' value:'{{Compromising}}' fillcolor:'#6399D1' ]] [[ enddataitem ]]
    [[ dataitem label:'Avoiding' value:'{{Avoiding}}' fillcolor:'#94DB84' ]] [[ enddataitem ]]
    [[ dataitem label:'Yielding' value:'{{Yielding}}' fillcolor:'#A1ED90' ]] [[ enddataitem ]]
{[ endchart ]}

<h3>Conflict Engagement Modes</h3>

<h4>Winning</h4>
<p>
    Winning means you prefer competing over cooperating. You believe you have the right answer and you desire to
  prove you are right, whatever it takes. This may include standing up for your own rights, beliefs or position.
</p>

<h4>Resolving</h4>
<p>
    Resolving means you attempt to work with the other person in depth to find the best solution, regardless of
    who appears to get the most immediate benefit. This involves digging beneath the presenting issue to find a
    solution that offers benefit to both parties and can take more time than other approaches.
</p>

<h4>Compromising</h4>
<p>
    Compromising means you find a middle ground in the conflict. This often involves meeting in the middle or finding
    some mutually agreeable point between both positions. This is useful for quick solutions.
</p>

<h4>Avoiding</h4>
<p>
    Avoiding means not pursuing your own rights or those of the other person. You typically do not address the
    conflict at all, if possible. This may be diplomatically sidestepping an issue or staying away from a
    threatening situation.
</p>

<h4>Yielding</h4>
<p>
    Yielding means neglecting your own interests while giving in to those of the other person. This is
    self-sacrificing and maybe charitable; serving or choosing to obey another when you prefer not to.
</p>

<h3>Conflict Engagement Themes</h3>

<p>Often people find that they have a combined approach and gravitate toward one of the following themes.</p>

{[ chart type:'pie' ]}
    [[ dataitem label:'Solving' value:'{{EngagementProfileSolving}}' fillcolor:'#4E79A7' ]] [[ enddataitem ]]
    [[ dataitem label:'Accommodating' value:'{{EngagementProfileAccommodating}}' fillcolor:'#8CD17D' ]] [[ enddataitem ]]
    [[ dataitem label:'Winning' value:'{{EngagementProfileWinning}}' fillcolor:'#E15759' ]] [[ enddataitem ]]
{[ endchart ]}

<h4>Solving</h4>
<p>
    Solving describes those who seek to use both Resolving and Compromising modes for solving conflict. By combining
    these two modes, they seek to solve problems as a team. Their leadership styles are highly cooperative and
    empowering for the benefit of the entire group.
</p>

<h4>Accommodating</h4>
<p>
    Accommodating combines Avoiding and Yielding modes for solving conflict. They are most effective in roles
    where allowing others to have their way is better for the team, such as support roles or roles where an
    emphasis on the contribution of others is significant.
</p>

<h4>Winning</h4>
<p>
    Winning is not a combination of modes, but a theme that is based entirely on the Winning model alone for
    solving conflict. This theme is important for times when quick decisions need to be made and is helpful
    for roles such as sole-proprietor.
</p>", "1A855117-6489-4A15-846A-5A99F54E9747" );

            RockMigrationHelper.UpdateBlockTypeAttribute( SystemGuid.BlockType.GIFTS_ASSESSMENT, SystemGuid.FieldType.CODE_EDITOR, "Results Message", "ResultsMessage", "", @"The text (HTML) to display at the top of the results section.<span class='tip tip-lava'></span><span class='tip tip-html'></span>", 1, @"{% if DominantGifts != empty %}
    <div>
        <h2 class='h2'>Dominant Gifts</h2>
        <div class='table-responsive'>
            <table class='table'>
                <thead>
                    <tr>
                        <th>
                            Spiritual Gift
                        </th>
                        <th>
                            You are uniquely wired to:
                        </th>
                    </tr>
                </thead>
                <tbody>
                    {% for dominantGift in DominantGifts %}
                        <tr>
                            <td>
                                {{ dominantGift.Value }}
                            </td>
                            <td>
                                {{ dominantGift.Description }}
                            </td>
                        </tr>
                    {% endfor %}
                </tbody>
            </table>
        </div>
    </div>
{% endif %}
{% if SupportiveGifts != empty %}
    <div>
        <h2 class='h2'>Supportive Gifts</h2>
        <div class='table-responsive'>
            <table class='table'>
                <thead>
                    <tr>
                        <th>
                            Spiritual Gift
                        </th>
                        <th>
                            You are uniquely wired to:
                        </th>
                    </tr>
                </thead>
                <tbody>
                    {% for supportiveGift in SupportiveGifts %}
                        <tr>
                            <td>
                                {{ supportiveGift.Value }}
                            </td>
                            <td>
                                {{ supportiveGift.Description }}
                            </td>
                        </tr>
                    {% endfor %}
                </tbody>
            </table>
        </div>
    </div>
{% endif %}
{% if OtherGifts != empty %}
    <div>
        <h2 class='h2'>Other Gifts</h2>
        <div class='table-responsive'>
            <table class='table'>
                <thead>
                    <tr>
                        <th>
                            Spiritual Gift
                        </th>
                        <th>
                            You are uniquely wired to:
                        </th>
                    </tr>
                </thead>
                <tbody>
                    {% for otherGift in OtherGifts %}
                        <tr>
                            <td>
                                {{ otherGift.Value }}
                            </td>
                            <td>
                                {{ otherGift.Description }}
                            </td>
                        </tr>
                    {% endfor %}
                </tbody>
            </table>
        </div>
    </div>
{% endif %}
{% if GiftScores != null and GiftScores != empty %}
    <!-- The following empty h2 element is to mantain vertical spacing between sections. -->
    <h2 class='h2'></h2>
    <div>
        <p>
            The following graph shows your spiritual gifts ranked from top to bottom.
        </p>
        <div class='panel panel-default'>
            <div class='panel-heading'>
                <h2 class='panel-title'><b>Ranked Gifts</b></h2>
            </div>
            <div class='panel-body'>
                {[ chart type:'horizontalBar' xaxistype:'linearhorizontal0to100' ]}
                    {% assign sortedScores = GiftScores | OrderBy:'Percentage desc,SpiritualGiftName' %}
                    {% for score in sortedScores %}
                        [[ dataitem label:'{{ score.SpiritualGiftName }}' value:'{{ score.Percentage }}' fillcolor:'#709AC7' ]]
                        [[ enddataitem ]]
                    {% endfor %}
                {[ endchart ]}
            </div>
        </div>
    </div>
{% endif %}", "85256610-56EB-4E6F-B62B-A5517B54B39E" );

            // correct the ordering of Gifts Assessment Attributes
            Sql( "UPDATE [Attribute] SET [Order] = 0 WHERE ([Guid] = '86C9E794-B678-4453-A831-FE348A440646');" ); // Instructions
            // ResultsMessage [Order] has already been set above
            Sql( "UPDATE [Attribute] SET [Order] = 2 WHERE ([Guid] = '85107259-0A30-4F1A-A651-CBED5243B922');" ); // SetPageTitle
            Sql( "UPDATE [Attribute] SET [Order] = 3 WHERE ([Guid] = 'DA7752F5-9F21-4391-97F3-BB7D35F885CE');" ); // SetPageIcon
            Sql( "UPDATE [Attribute] SET [Order] = 4 WHERE ([Guid] = '861F4601-82B7-46E3-967F-2E03D769E2D2');" ); // NumberOfQuestions

            RockMigrationHelper.UpdateBlockTypeAttribute( SystemGuid.BlockType.MOTIVATORS, SystemGuid.FieldType.CODE_EDITOR, "Results Message", "ResultsMessage", "", @"The text (HTML) to display at the top of the results section.<span class='tip tip-lava'></span><span class='tip tip-html'></span>", 1, @"<p>
    {{ Person.NickName }}, here are your motivators results. We’ve listed your Top 5 Motivators, your
    growth propensity score, along with a complete listing of all 22 motivators and your results
    for each.
</p>
<h2>Growth Propensity</h2>
<p>
    Growth Propensity measures your perceived mindset on a continuum between a growth mindset and
    fixed mindset. These are two ends of a spectrum about how we view our own capacity and potential.
</p>
<div style='margin: 0;max-width:280px'>
    {[ chart type:'gauge' backgroundcolor:'#f13c1f,#f0e3ba,#0e9445,#3f56a1' gaugelimits:'0,2,17,85,100' chartheight:'150px']}
        [[ dataitem value:'{{ GrowthScore }}' fillcolor:'#484848' ]] [[ enddataitem ]]
    {[ endchart ]}
</div>
<h2>Individual Motivators</h2>
<p>
    There are 22 possible motivators in this assessment. While your Top 5 Motivators may be most helpful in understanding your results in a snapshot, you may also find it helpful to see your scores on each for a complete picture.
</p>
<!-- Theme Chart -->
<div class='panel panel-default'>
    <div class='panel-heading'>
        <h2 class='panel-title'><b>Composite Score</b></h2>
    </div>
    <div class='panel-body'>
        {[chart type:'horizontalBar' chartheight:'200px' xaxistype:'linearhorizontal0to100' ]}
            {% for motivatorThemeScore in MotivatorThemeScores %}
                [[dataitem label:'{{ motivatorThemeScore.DefinedValue.Value }}' value:'{{ motivatorThemeScore.Value }}' fillcolor:'{{ motivatorThemeScore.DefinedValue | Attribute:'Color' }}' ]]
                [[enddataitem]]
            {% endfor %}
        {[endchart]}
    </div>
</div>
<p>
    This graph is based on the average composite score for each Motivator Theme.
</p>
{% for motivatorThemeScore in MotivatorThemeScores %}
    <p>
        <b>{{ motivatorThemeScore.DefinedValue.Value }}</b>
        <br>
        {{ motivatorThemeScore.DefinedValue.Description }}
        <br>
        {{ motivatorThemeScore.DefinedValue | Attribute:'Summary' }}
    </p>
{% endfor %}
<p>
    The following graph shows your motivators ranked from top to bottom.
</p>
<div class='panel panel-default'>
    <div class='panel-heading'>
        <h2 class='panel-title'><b>Ranked Motivators</b></h2>
    </div>
    <div class='panel-body'>
        {[ chart type:'horizontalBar' xaxistype:'linearhorizontal0to100' ]}
            {% for motivatorScore in MotivatorScores %}
                {% assign theme = motivatorScore.DefinedValue | Attribute:'Theme' %}
                {% if theme and theme != empty %}
                    [[dataitem label:'{{ motivatorScore.DefinedValue.Value }}' value:'{{ motivatorScore.Value }}' fillcolor:'{{ motivatorScore.DefinedValue | Attribute:'Color' }}' ]]
                    [[enddataitem]]
                {% endif %}
            {% endfor %}
        {[endchart]}
    </div>
</div>", "BA51DFCD-B174-463F-AE3F-6EEE73DD9338" );
        }

        /// <summary>
        /// JH: Remove Page name from SystemCommunicationDetail Page's Breadcrumbs and fix typo in (legacy) System Email Detail Block's PreHtml field
        /// </summary>
        private void RemovePagenamefromSystemCommunicationDetailBreadcrumbs()
        {
            // remove Page name from System Communication Detail Page's breadcrumb trail
            Sql( $@"UPDATE [Page]
SET [BreadCrumbDisplayName] = 0
WHERE ([Guid] = '{SystemGuid.Page.SYSTEM_COMMUNICATION_DETAIL}');" );

            // fix typo in (legacy) System Email Detail Block's PreHtml field
            Sql( @"UPDATE [Block]
SET [PreHtml] = REPLACE([PreHtml], 'should now managed', 'should now be managed')
WHERE ([BlockTypeId] IN (SELECT [Id]
                         FROM [BlockType]
                         WHERE ([Path] = '~/Blocks/Communication/SystemEmailDetail.ascx') OR
                            ([Path] = '~/Blocks/Communication/SystemEmailList.ascx')));" );
        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Achievement Type List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("D7F5D3B3-BABD-49B8-A63D-94D24064666A");
            // Attrib for BlockType: Achievement Type List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("FD012DC0-9002-4967-B08B-B6C8D0DDBF92");
            // Attrib for BlockType: Achievement Attempt List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("1E90E6A4-177A-447B-8552-6B1395344982");
            // Attrib for BlockType: Achievement Attempt List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("40C93EB0-0A68-428A-85AE-EFD4FBE7F945");
            // Attrib for BlockType: Page Views:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("6083EF39-DB05-482B-ADC1-DC03F9D0F38B");
            // Attrib for BlockType: Page Views:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("C8E2BA67-B482-4676-8B02-AE402008A17B");
            // Attrib for BlockType: Logs:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("A7679E6E-54DF-4906-B809-C84C3724B0EF");
            // Attrib for BlockType: Logs:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("6EB8AD29-2129-4DFC-9A4C-08A886C1DA30");
            // Attrib for BlockType: Prayer Request Details:Workflow
            RockMigrationHelper.DeleteAttribute("7DBEB6DA-8417-49C9-968A-154E1C41C8ED");
            // Attrib for BlockType: Prayer Request Details:Completion Xaml
            RockMigrationHelper.DeleteAttribute("949F7A76-8947-4457-B742-1AC1F2EC2486");
            // Attrib for BlockType: Prayer Request Details:Completion Action
            RockMigrationHelper.DeleteAttribute("22BCE7BA-ED45-480B-9833-03A503B825ED");
            // Attrib for BlockType: Prayer Request Details:Enable Person Matching
            RockMigrationHelper.DeleteAttribute("238CFE61-56D7-47E2-AC5D-66B3B645D3FE");
            // Attrib for BlockType: Prayer Request Details:Require Last Name
            RockMigrationHelper.DeleteAttribute("E5681E4D-7CD7-475F-9F74-7E0C295BE538");
            // Attrib for BlockType: Prayer Request Details:Require Campus
            RockMigrationHelper.DeleteAttribute("92258CC6-9E8E-4DF3-AB5B-48114BE28BA7");
            // Attrib for BlockType: Prayer Request Details:Show Campus
            RockMigrationHelper.DeleteAttribute("C91F9877-5605-48F8-B6CA-A797F998826D");
            // Attrib for BlockType: Prayer Request Details:Character Limit
            RockMigrationHelper.DeleteAttribute("54273807-6E5F-47DF-A803-6251C6E2480A");
            // Attrib for BlockType: Prayer Request Details:Default To Public
            RockMigrationHelper.DeleteAttribute("28332C3D-E2B2-4D6D-B8B3-9BE11FF07504");
            // Attrib for BlockType: Prayer Request Details:Show Public Display Flag
            RockMigrationHelper.DeleteAttribute("0C0898A4-0554-420E-BA3D-BD22AB8C44CA");
            // Attrib for BlockType: Prayer Request Details:Show Urgent Flag
            RockMigrationHelper.DeleteAttribute("50F41A0F-858E-46D9-AF3B-506E8AB2F9E3");
            // Attrib for BlockType: Prayer Request Details:Expires After (Days)
            RockMigrationHelper.DeleteAttribute("4389C4A7-CF8D-4750-812A-F4C2BD602E96");
            // Attrib for BlockType: Prayer Request Details:Enable Auto Approve
            RockMigrationHelper.DeleteAttribute("42F19547-E84A-4CD8-9787-7A74255E73E4");
            // Attrib for BlockType: Prayer Request Details:Default Category
            RockMigrationHelper.DeleteAttribute("80ABC48F-99E7-404F-AAB3-0EA8551010CF");
            // Attrib for BlockType: Prayer Request Details:Parent Category
            RockMigrationHelper.DeleteAttribute("26EB9054-E675-4DD6-80EF-D5481F23CF51");
            // Attrib for BlockType: Prayer Request Details:Show Category
            RockMigrationHelper.DeleteAttribute("1D63D8F5-A810-477D-B9F7-24B33546990F");
            // Attrib for BlockType: Group View:Template
            RockMigrationHelper.DeleteAttribute("A3826811-395A-4564-8101-EB95936065FB");
            // Attrib for BlockType: Group View:Show Leader List
            RockMigrationHelper.DeleteAttribute("808D607F-D097-48C5-BC3A-988141A1C69C");
            // Attrib for BlockType: Group View:Group Edit Page
            RockMigrationHelper.DeleteAttribute("386360FF-9E79-4E0C-92D5-7285484FFADF");
            // Attrib for BlockType: Group Member View:Template
            RockMigrationHelper.DeleteAttribute("AD4CCF58-A93D-49C7-860D-02BC0F966724");
            // Attrib for BlockType: Group Member View:Group Member Edit Page
            RockMigrationHelper.DeleteAttribute("636417C3-365C-488D-8072-406C3C2EA878");
            // Attrib for BlockType: Group Member List:Additional Fields
            RockMigrationHelper.DeleteAttribute("D5942AD0-5EA5-4ACA-8D5F-66FDE86E2E61");
            // Attrib for BlockType: Group Member List:Template
            RockMigrationHelper.DeleteAttribute("4B1F3DAE-180E-45E6-BC36-2BEA0A03C674");
            // Attrib for BlockType: Group Member List:Title Template
            RockMigrationHelper.DeleteAttribute("FB6FA5A4-74C7-4E17-8764-118C01FCD192");
            // Attrib for BlockType: Group Member List:Group Member Detail Page
            RockMigrationHelper.DeleteAttribute("D5629B9E-59EE-40D4-B4BA-E23DFAC33D61");
            // Attrib for BlockType: Group Member Edit:Member Detail Page
            RockMigrationHelper.DeleteAttribute("C61DE82E-2B0D-49EC-865E-6507DAD1BAF2");
            // Attrib for BlockType: Group Member Edit:Attribute Category
            RockMigrationHelper.DeleteAttribute("E6C90ACE-57D9-441A-A764-F8E0D36B433E");
            // Attrib for BlockType: Group Member Edit:Allow Note Edit
            RockMigrationHelper.DeleteAttribute("AB00A40C-5AE0-42FD-8198-7B90C727A81C");
            // Attrib for BlockType: Group Member Edit:Allow Member Status Change
            RockMigrationHelper.DeleteAttribute("A7A59697-82A9-4368-9A32-103BB1C32609");
            // Attrib for BlockType: Group Member Edit:Allow Role Change
            RockMigrationHelper.DeleteAttribute("A247DFCC-895A-41A5-B02E-4F0D9679C4A6");
            // Attrib for BlockType: Group Edit:Group Detail Page
            RockMigrationHelper.DeleteAttribute("1C3856C1-E5C9-4CDC-B759-9F902B458911");
            // Attrib for BlockType: Group Edit:Attribute Category
            RockMigrationHelper.DeleteAttribute("5C495667-1C02-4244-A1ED-1AB26AC35B5C");
            // Attrib for BlockType: Group Edit:Enable Public Status Edit
            RockMigrationHelper.DeleteAttribute("DB9BFD83-5B62-4B07-9747-15A46656F23A");
            // Attrib for BlockType: Group Edit:Show Public Status
            RockMigrationHelper.DeleteAttribute("5043F4B3-1465-4BAD-8375-67CF0F2BC928");
            // Attrib for BlockType: Group Edit:Enable Active Status Edit
            RockMigrationHelper.DeleteAttribute("F4C2FAB6-33FB-4760-9721-7A1EBA714CEA");
            // Attrib for BlockType: Group Edit:Show Active Status
            RockMigrationHelper.DeleteAttribute("C0BAF201-4D44-47E2-91C8-9FD89CCFC558");
            // Attrib for BlockType: Group Edit:Enable Group Capacity Edit
            RockMigrationHelper.DeleteAttribute("24DD3040-6CE5-46A1-BC3C-5E4F1782436F");
            // Attrib for BlockType: Group Edit:Show Group Capacity
            RockMigrationHelper.DeleteAttribute("DE4B5956-CB26-48BB-9C40-488A944B3701");
            // Attrib for BlockType: Group Edit:Enable Campus Edit
            RockMigrationHelper.DeleteAttribute("4875251A-8B4B-4C5D-B630-E8D809260945");
            // Attrib for BlockType: Group Edit:Show Campus
            RockMigrationHelper.DeleteAttribute("20BB29E2-45FA-469C-9266-88A30EA4D13A");
            // Attrib for BlockType: Group Edit:Enable Description Edit
            RockMigrationHelper.DeleteAttribute("86CAFA5C-086A-4993-ADCE-B82024F90959");
            // Attrib for BlockType: Group Edit:Show Description
            RockMigrationHelper.DeleteAttribute("C894DB28-D08E-4649-9D41-54938D350950");
            // Attrib for BlockType: Group Edit:Enable Group Name Edit
            RockMigrationHelper.DeleteAttribute("1149943B-BF00-4338-AB46-B62933FDB557");
            // Attrib for BlockType: Group Edit:Show Group Name
            RockMigrationHelper.DeleteAttribute("EB6741F0-22E6-4B90-BEC9-1929CE215B45");
            // Attrib for BlockType: Group Attendance Entry:Allow Any Date Selection
            RockMigrationHelper.DeleteAttribute("01816306-DE0C-491B-81C1-653EE29C37B2");
            // Attrib for BlockType: Group Attendance Entry:Show Save Button
            RockMigrationHelper.DeleteAttribute("11AE7EDA-3EE4-4775-9812-E61E8DAD0681");
            // Attrib for BlockType: Group Attendance Entry:Save Redirect Page
            RockMigrationHelper.DeleteAttribute("D4E32F4E-4679-40CE-B19D-B252D170DEDF");
            // Attrib for BlockType: Group Attendance Entry:Number of Days Back to Allow
            RockMigrationHelper.DeleteAttribute("F0E13D10-D713-454E-8607-02DD35CDDB46");
            // Attrib for BlockType: Group Attendance Entry:Number of Days Forward to Allow
            RockMigrationHelper.DeleteAttribute("CF519120-1ABA-4261-B0ED-BC26C866F88F");
            // Attrib for BlockType: Prayer Session Setup:Show Campus Filter
            RockMigrationHelper.DeleteAttribute("B0B72E6D-6D4B-4C29-A3B4-2B01A8E756B1");
            // Attrib for BlockType: Prayer Session Setup:Parent Category
            RockMigrationHelper.DeleteAttribute("E177F02C-B1BF-4139-B354-E3C4EFFF9A0B");
            // Attrib for BlockType: Prayer Session Setup:Prayer Page
            RockMigrationHelper.DeleteAttribute("6EEED035-2E74-44EF-8567-016C2CBC3E6D");
            // Attrib for BlockType: Prayer Session:Template
            RockMigrationHelper.DeleteAttribute("AF310A62-C62A-427A-A88E-48AA885CCCE7");
            // Attrib for BlockType: Prayer Session:Inappropriate Flag Limit
            RockMigrationHelper.DeleteAttribute("7171FEA9-1711-4E40-965B-348D56B604C8");
            // Attrib for BlockType: Prayer Session:Public Only
            RockMigrationHelper.DeleteAttribute("B10629F9-DCBC-4601-A717-89B34F7ED5DE");
            // Attrib for BlockType: Prayer Session:Show Inappropriate Button
            RockMigrationHelper.DeleteAttribute("52E1A831-4039-4185-8261-B39083B39C46");
            // Attrib for BlockType: Prayer Session:Show Follow Button
            RockMigrationHelper.DeleteAttribute("2E37A96E-9BF5-484A-8146-6E54629080CC");
            // Attrib for BlockType: Prayer Session:Prayed Button Text
            RockMigrationHelper.DeleteAttribute("7D3AE194-069E-4CE8-97C9-EB7A9CADA7B9");
            // Attrib for BlockType: Communication List Subscribe:Show Description
            RockMigrationHelper.DeleteAttribute("E12CE061-2475-41A2-BDB7-12F03853B18B");
            // Attrib for BlockType: Communication List Subscribe:Communication List Categories
            RockMigrationHelper.DeleteAttribute("1E983A67-A9DC-4F91-8862-3E55613A95DC");
            // Attrib for BlockType: Calendar View:Show Filter
            RockMigrationHelper.DeleteAttribute("9E75484E-9F6B-4E36-A73A-C1AE06A48CE1");
            // Attrib for BlockType: Calendar View:Event Summary
            RockMigrationHelper.DeleteAttribute("497A6BD6-D36C-4AC8-AF83-9015EAF43C89");
            // Attrib for BlockType: Calendar View:Audience Filter
            RockMigrationHelper.DeleteAttribute("0BA3BAB4-92EA-4768-9B5D-47E51B39FE04");
            // Attrib for BlockType: Calendar View:Detail Page
            RockMigrationHelper.DeleteAttribute("91A13A0A-2D7A-45AE-BF09-D897C280C4E1");
            // Attrib for BlockType: Calendar View:Calendar
            RockMigrationHelper.DeleteAttribute("694244DC-5067-4A34-98F3-85FED7052E18");
            // Attrib for BlockType: Calendar Event List:Day Header Template
            RockMigrationHelper.DeleteAttribute("57556A66-7AA0-4096-8245-5C76D73F34FB");
            // Attrib for BlockType: Calendar Event List:Event Template
            RockMigrationHelper.DeleteAttribute("8B7B6464-4E2E-4312-8414-2118663E74D8");
            // Attrib for BlockType: Calendar Event List:Detail Page
            RockMigrationHelper.DeleteAttribute("3EE14F0B-C3EA-49CF-8F08-EF839F428762");
            // Attrib for BlockType: Calendar Event List:Calendar
            RockMigrationHelper.DeleteAttribute("8BCE51C8-3757-42A4-998D-F5919FF2BB16");
            // Attrib for BlockType: Hero:Padding
            RockMigrationHelper.DeleteAttribute("92DBA471-CFBC-4BC2-BD17-022663670328");
            // Attrib for BlockType: Hero:Subtitle Color
            RockMigrationHelper.DeleteAttribute("5374CF14-6A4F-4CA8-9561-0A1003F28504");
            // Attrib for BlockType: Hero:Title Color
            RockMigrationHelper.DeleteAttribute("9C95201E-64AC-4A36-8957-571C7BCA8F5B");
            // Attrib for BlockType: Hero:Text Align
            RockMigrationHelper.DeleteAttribute("F3F7B68A-6926-4D89-BA28-0FB99E940A8E");
            // Attrib for BlockType: Hero:Height - Tablet
            RockMigrationHelper.DeleteAttribute("BF0B4848-9193-45EA-9A13-E0DE5573A64F");
            // Attrib for BlockType: Hero:Height - Phone
            RockMigrationHelper.DeleteAttribute("6527442E-539A-4486-B0C8-6D8C48F77FE8");
            // Attrib for BlockType: Hero:Background Image - Tablet
            RockMigrationHelper.DeleteAttribute("B7A56FAF-90C6-4F79-898C-6F1FAEDF19B5");
            // Attrib for BlockType: Hero:Background Image - Phone
            RockMigrationHelper.DeleteAttribute("D807B220-49A0-49EE-B4F0-F0A92ED3160F");
            // Attrib for BlockType: Hero:Subtitle
            RockMigrationHelper.DeleteAttribute("8BD8BC79-10F7-4889-B952-1789BE7A8804");
            // Attrib for BlockType: Hero:Title
            RockMigrationHelper.DeleteAttribute("D9132EAB-91FC-4061-8881-1D2618E84F52");
            RockMigrationHelper.DeleteBlockType("EBB91B46-292E-4784-9E37-38781C714008"); // Prayer Request Details
            RockMigrationHelper.DeleteBlockType("3F34AE03-9378-4363-A232-0318139C3BD3"); // Group View
            RockMigrationHelper.DeleteBlockType("6B3C23EA-A1C2-46FA-9F04-5B0BD004ED8B"); // Group Member View
            RockMigrationHelper.DeleteBlockType("5A6D2ADB-03A7-4B55-8EAA-26A37116BFF1"); // Group Member List
            RockMigrationHelper.DeleteBlockType("514B533A-8970-4628-A4C8-35388CD869BC"); // Group Member Edit
            RockMigrationHelper.DeleteBlockType("FEC66374-E38F-4651-BAA6-AC658409D9BD"); // Group Edit
            RockMigrationHelper.DeleteBlockType("08AE409C-9E4C-42D1-A93C-A554A3EEA0C3"); // Group Attendance Entry
            RockMigrationHelper.DeleteBlockType("4A3B0D13-FC32-4354-A224-9D450F860BE9"); // Prayer Session Setup
            RockMigrationHelper.DeleteBlockType("420DEA5F-9ABC-4E59-A9BD-DCA972657B84"); // Prayer Session
            RockMigrationHelper.DeleteBlockType("D0C51784-71ED-46F3-86AB-972148B78BE8"); // Communication List Subscribe
            RockMigrationHelper.DeleteBlockType("14B447B3-6117-4142-92E7-E3F289106140"); // Calendar View
            RockMigrationHelper.DeleteBlockType("A9149623-6A82-4F25-8F4D-0961557BE78C"); // Calendar Event List
            RockMigrationHelper.DeleteBlockType("A8597994-BD47-4A15-8BB1-4B508977665F"); // Hero

            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.MOBILE_CMS_HERO_BLOCK_TYPE );
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.MOBILE_EVENTS_CALENDAREVENTLIST_BLOCK_TYPE );
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.MOBILE_EVENTS_CALENDARVIEW_BLOCK_TYPE );
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.MOBILE_EVENTS_COMMUNICATION_LIST_SUBSCRIBE_BLOCK_TYPE );
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.MOBILE_EVENTS_PRAYER_SESSION_BLOCK_TYPE );
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.MOBILE_EVENTS_PRAYER_SESSION_SETUP_BLOCK_TYPE );
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.MOBILE_GROUPS_GROUP_ATTENDANCE_ENTRY_BLOCK_TYPE );
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.MOBILE_GROUPS_GROUP_EDIT_BLOCK_TYPE );
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.MOBILE_GROUPS_GROUP_MEMBER_EDIT_BLOCK_TYPE );
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.MOBILE_GROUPS_GROUP_MEMBER_LIST_BLOCK_TYPE );
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.MOBILE_GROUPS_GROUP_MEMBER_VIEW_BLOCK_TYPE );
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.MOBILE_GROUPS_GROUP_VIEW_BLOCK_TYPE );
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.MOBILE_PRAYER_PRAYER_REQUEST_DETAILS_BLOCK_TYPE );
        }
    }
}
