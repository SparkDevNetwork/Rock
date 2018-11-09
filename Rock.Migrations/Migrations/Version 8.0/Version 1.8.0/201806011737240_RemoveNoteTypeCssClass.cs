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
    public partial class RemoveNoteTypeCssClass : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropColumn( "dbo.NoteType", "CssClass" );

            // JE: Route for Homepage
            RockMigrationHelper.AddPageRoute( "117B547B-9D71-4EE9-8047-176676F5DC8C", "ContentChannel/{ChannelGuid}" );

            // NA: New workflow action AddContentChannelItem
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.AddContentChannelItem", "B61F6FF1-7376-4150-B8A1-8DB246613834", false, true );

            // ED: Label Fixes
            LabelFixes_Up();

            // SK:Data Integrity Settings -> Data Automation
            DataIntegritySettings_Up();

            // MP: Add GroupMemberHistory to GroupMemberDetail
            GroupMemberHistory_Up();

            // NA: Add two new attributes on GroupAttendanceDetail block and reorder all of them
            GroupAttendanceDetail_Up();
        }

        /// <summary>
        /// NA: Add two new attributes on GroupAttendanceDetail block and reorder all of them
        /// </summary>
        private void GroupAttendanceDetail_Up()
        {
            // Attrib for BlockType: Group Attendance Detail:Allow Add
            RockMigrationHelper.UpdateBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Add", "AllowAdd", "", "Should block support adding new attendance dates outside of the group's configured schedule and group type's exclusion dates?", 0, @"True", "D24A540E-3E6B-4790-AB77-6661F8DA292E" );

            // Attrib for BlockType: Group Attendance Detail:Allow Adding Person
            RockMigrationHelper.UpdateBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Adding Person", "AllowAddingPerson", "", "Should block support adding new people as attendees?", 1, @"False", "92F47ABB-C051-4D87-9266-C8DAE77736D4" );

            // Attrib for BlockType: Group Attendance Detail:Add Person As
            RockMigrationHelper.UpdateBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Add Person As", "AddPersonAs", "", "'Attendee' will only add the person to attendance. 'Group Member' will add them to the group with the default group role.", 2, @"Attendee", "599E4583-F25B-42E6-95D3-1FF8E58B1D9A" );

            // Attrib for BlockType: Group Attendance Detail:Group Member Add Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Member Add Page", "GroupMemberAddPage", "", "Page to use for adding a new group member. If no page is provided the built in group member edit panel will be used. This panel allows the individual to search the database.", 3, @"", "EE1A251D-1F89-4DCF-9819-07F68E78E459" );

            // Attrib for BlockType: Group Attendance Detail:Allow Campus Filter
            RockMigrationHelper.UpdateBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Campus Filter", "AllowCampusFilter", "", "Should block add an option to allow filtering people and attendance counts by campus?", 4, @"False", "36646052-A994-4A8A-8F2B-70686EC1266C" );

            // Attrib for BlockType: Group Attendance Detail:Workflow
            RockMigrationHelper.UpdateBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "", "An optional workflow type to launch whenever attendance is saved. The Group will be used as the workflow 'Entity' when processing is started. Additionally if a 'StartDateTime' and/or 'Schedule' attribute exist, their values will be set with the corresponding saved attendance values.", 5, @"", "63376AC6-6DCC-4FC2-9E9C-5207A1B90F26" );

            // Attrib for BlockType: Group Attendance Detail:Attendance Roster Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "890BAA33-5EBB-4343-A8AA-42E0E6C7467A", "Attendance Roster Template", "AttendanceRosterTemplate", "", "", 6, @"", "D255FB71-5D55-490B-92A0-3C48F3AE95BF" );

            // Attrib for BlockType: Group Attendance Detail:Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "An optional lava template to appear next to each person in the list.", 7, @"", "132DA055-1358-4FA1-8B56-64FACB7A97BC" );

            // Attrib for BlockType: Group Attendance Detail:Restrict Future Occurrence Date
            RockMigrationHelper.UpdateBlockTypeAttribute( "FC6B5DC8-3A90-4D78-8DC2-7F7698A6E73B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Restrict Future Occurrence Date", "RestrictFutureOccurrenceDate", "", "Should user be prevented from selecting a future Occurrence date?", 8, @"False", "8ACF5559-F44F-454E-B447-7245E1B986C8" );
        }

        /// <summary>
        /// MP: Add GroupMemberHistory to GroupMemberDetail
        /// </summary>
        private void GroupMemberHistory_Up()
        {
            // Attrib for BlockType: Notes:Expand Replies
            RockMigrationHelper.UpdateBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Expand Replies", "ExpandReplies", "", @"Should replies to automatically expanded?", 14, @"False", "84E53A88-32D2-432C-8BB5-600BDBA10949" );
            // Attrib for BlockType: Group Member History:Show Members Grid
            RockMigrationHelper.UpdateBlockTypeAttribute( "EA6EA2E7-6504-41FE-AB55-0B1E7D04B226", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Members Grid", "ShowMembersGrid", "", @"Show Members Grid if GroupMemberId is not specified in the URL", 4, @"True", "A79B23B7-7ED5-40B5-8434-9787CF84F183" );

            // Attrib Value for Block:Group Member History, Attribute:Show Members Grid Page: Group Member History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "03FB6DBD-3320-46A8-B3E1-662AE2C3FC41", "A79B23B7-7ED5-40B5-8434-9787CF84F183", @"True" );
            // Attrib Value for Block:Group Member History, Attribute:Show Members Grid Page: Group Member Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "78537580-241F-4B4A-97BE-5BA10802282B", "A79B23B7-7ED5-40B5-8434-9787CF84F183", @"False" );
            // Attrib Value for Block:Group Member History, Attribute:Group Member History Page Page: Group Member Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "78537580-241F-4B4A-97BE-5BA10802282B", "A1D22BA4-4D39-4187-9F6B-C0B8DC6D6896", @"eaab757e-524f-4db9-a124-d5efbcdca63b" );
            // Attrib Value for Block:Group Member History, Attribute:Timeline Lava Template Page: Group Member Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "78537580-241F-4B4A-97BE-5BA10802282B", "2928385B-09D9-4877-A35C-2A688F22DB22", @"{% include '~~/Assets/Lava/GroupHistoryTimeline.lava' %}" );
            // Attrib Value for Block:Group Member History, Attribute:Group History Grid Page Page: Group Member Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "78537580-241F-4B4A-97BE-5BA10802282B", "B72E26E2-5EC5-49BE-829B-18FB9AE12E47", @"fb9a6bc0-0b51-4a92-a32c-58ac822cd2d0" );
        }

        /// <summary>
        /// SK:Data Integrity Settings -> Data Automation
        /// </summary>
        private void DataIntegritySettings_Up()
        {
            Sql( @"
UPDATE 
	[Page]
SET
   [IconCssClass]='fa fa-sliders-h',
   [PageTitle]='Data Automation'
WHERE
	[Guid]='A2D5F989-1E30-47B9-AAFC-F7EC627AFF21'" );
        }

        /// <summary>
        /// ED: Label Fixes
        /// </summary>
        private void LabelFixes_Up()
        {
            Sql( @"
-- Parent Label
UPDATE [BinaryFileData]
SET [Content] = 0x1043547E7E43442C7E43435E7E43547E0A5E58417E54413030307E4A534E5E4C54305E4D4E575E4D54445E504F4E5E504D4E5E4C48302C305E4A4D415E5052362C367E534431355E4C524E5E4349305E585A0A5E58410A5E4D4D540A5E50573831320A5E4C4C303430360A5E4C53300A5E465432302C38305E41304E2C37332C37325E46423830302C302C302C4C5E46485C5E46444368696C64205069636B757020526563656970745E46530A5E46423335302C372C302C4C5E465433302C3339305E41304E2C33392C33385E4644315E46530A5E46423335302C372C302C4C5E46543431352C3339305E41304E2C33392C33385E4644325E46530A5E465431342C3336395E41304E2C32332C32345E46485C5E4644466F722074686520736166657479206F6620796F7572206368696C642C20796F75206D7573742070726573656E7420746869732072656365697074207768656E207069636B696E675E46530A5E465431352C3339335E41304E2C32332C32345E46485C5E4644757020796F7572206368696C642E20496620796F75206C6F7365207468697320706C6561736520736565207468652061726561206469726563746F722E5E46530A5E4C52595E464F302C305E47423831322C302C3130305E46535E4C524E0A5E5051312C302C312C595E585A
WHERE [Guid] = 'FFFCE2E5-CBA0-48A1-91BD-4705C472DD95'

-- Child Label
UPDATE [BinaryFileData]
SET [Content] = 0x1043547E7E43442C7E43435E7E43547E0A5E58417E54413030307E4A534E5E4C54305E4D4E575E4D54445E504F4E5E504D4E5E4C48302C305E4A4D415E5052362C367E534432345E4C524E5E4349305E585A0A5E58410A5E4D4D540A5E50573831320A5E4C4C303430360A5E4C53300A5E46543435322C3131395E41304E2C3133352C3133345E46423333332C312C302C525E46485C5E46445757575E46530A5E465431322C3235345E41304E2C3133352C3134365E46485C5E4644355E46530A5E465431342C3330395E41304E2C34352C34355E46485C5E4644365E46530A5E43575A2C453A524F433030302E464E545E46543239332C38325E415A4E2C37332C37330A5E46485C5E4644425E46530A5E43575A2C453A524F433030302E464E545E46543337382C38315E415A4E2C37332C37330A5E46485C5E4644465E46530A5E46543239392C3132305E41304E2C32382C32385E46485C5E4644345E46530A5E46423333302C322C302C4C5E4654382C3338325E41304E2C32382C32385E46485C5E4644395E46530A5E43575A2C453A524F433030302E464E545E46543630352C3338335E415A4E2C37332C37335E46485C5E4644375E46530A5E43575A2C453A524F433030302E464E545E46543731352C3338365E415A4E2C37332C37335E46485C5E4644385E46530A5E4C52595E464F302C305E47423831322C302C3133365E46535E4C524E0A5E5051312C302C312C595E585A0A
WHERE [Guid] = '673F9243-3702-4097-8E2D-F10036F48F18'" );

            // NA: Add new Block Attribute for Group Member List and a default block page setting
            // Attrib for BlockType: Group Member List:Data View Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "88B7EFA9-7419-4D05-9F88-38B936E61EDD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Data View Detail Page", "DataViewDetailPage", "", "Page used to view data views that are used with the group member sync.", 3, @"", "D8C39172-1C5A-4F47-9733-273117ADD7B3" );

            // Attrib Value for Block:Group Member List, Attribute:Data View Detail Page Page: Group Viewer, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BA0F3E7D-1C3A-47CB-9058-893DBAA35B89", "D8C39172-1C5A-4F47-9733-273117ADD7B3", Rock.SystemGuid.Page.DATA_VIEWS );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // NA: Add two new attributes on GroupAttendanceDetail block and reorder all of them
            RockMigrationHelper.DeleteAttribute( "EE1A251D-1F89-4DCF-9819-07F68E78E459" );
            RockMigrationHelper.DeleteAttribute( "599E4583-F25B-42E6-95D3-1FF8E58B1D9A" );

            // MP: Add GroupMemberHistory to GroupMemberDetail
            // Remove Block: Group Member History, from Page: Group Member Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "78537580-241F-4B4A-97BE-5BA10802282B" );

            // NA: Add new Block Attribute for Group Member List and a default block page setting
            RockMigrationHelper.DeleteAttribute( "D8C39172-1C5A-4F47-9733-273117ADD7B3" );

            AddColumn( "dbo.NoteType", "CssClass", c => c.String( maxLength: 100 ) );
        }
    }
}
