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
    public partial class CodeGenerated_20230323 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Groups.GroupAttendanceDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Groups.GroupAttendanceDetail", "Group Attendance Detail", "Rock.Blocks.Groups.GroupAttendanceDetail, Rock.Blocks, Version=1.15.0.14, Culture=neutral, PublicKeyToken=null", false, false, "64ECB2E0-218F-4EB4-8691-7DC94A767037" );

            // Add/Update Obsidian Block Type
            //   Name:Group Attendance Detail
            //   Category:Obsidian > Groups
            //   EntityType:Rock.Blocks.Groups.GroupAttendanceDetail
            RockMigrationHelper.UpdateMobileBlockType( "Group Attendance Detail", "Lists the group members for a specific occurrence date time and allows selecting if they attended or not.", "Rock.Blocks.Groups.GroupAttendanceDetail", "Obsidian > Groups", "308DBA32-F656-418E-A019-9D18235027C1" );

            // Add Block 
            //  Block Name: Membership
            //  Page Name: Extended Attributes V1
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "D089F13E-AA29-43A9-A9FC-6F5C78101E9B".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership", "SectionB1", @"", @"", 0, "3C95653C-FCF0-4C59-97D1-6206DDCF2762" );

            // Attribute for BlockType
            //   BlockType: Search
            //   Category: Mobile > Core
            //   Attribute: Stopped Typing Behavior Threshold
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "41174BEA-6567-430C-AAD4-A89A5CF70FB0", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Stopped Typing Behavior Threshold", "StoppedTypingBehaviorThreshold", "Stopped Typing Behavior Threshold", @"Changes the amount of time (in milliseconds) that a user must stop typing for the search command to execute. Set to 0 to disable entirely.", 10, @"200", "37B63CFC-9FDB-442E-ADC4-22E4FFDC045D" );

            // Attribute for BlockType
            //   BlockType: Search
            //   Category: Mobile > Core
            //   Attribute: Historical Result Item Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "41174BEA-6567-430C-AAD4-A89A5CF70FB0", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Historical Result Item Template", "HistoricalResultItemTemplate", "Historical Result Item Template", @"Lava template for rendering each historical result item. The Lava merge fields will be populated by the values provided in the 'AppendToSearchHistory' command.", 6, @"<StackLayout
    Spacing=""0"">
    <Frame StyleClass=""search-item-container"" 
        Margin=""0""
        BackgroundColor=""White""
        HasShadow=""false""
        HeightRequest=""40"">
            <StackLayout Orientation=""Horizontal""
                Spacing=""0""
                VerticalOptions=""Center"">
                <Rock:Image Source=""{{ PhotoUrl | Escape }}""
                    StyleClass=""search-image""
                    VerticalOptions=""Start""
                    Aspect=""AspectFit""
                    Margin=""0, 4, 8, 0"">
                    <Rock:CircleTransformation />
                </Rock:Image>
                
                <StackLayout Spacing=""0"" 
                    HorizontalOptions=""FillAndExpand"">
                    <StackLayout Orientation=""Horizontal""
                    VerticalOptions=""Center"">
                        <Label StyleClass=""search-item-name""
                            Text=""{{ Name }}""
                            LineBreakMode=""TailTruncation""
                            HorizontalOptions=""FillAndExpand"" />
                    </StackLayout>
                    {% if Text == null or Text == """" %}
                        {% assign Text = ""No Email"" %}
                    {% endif %}
                        <Label StyleClass=""search-item-text""
                            Grid.Column=""0""
                            MaxLines=""2""
                            LineBreakMode=""TailTruncation"">{{ Text | XamlWrap }}</Label> 
                </StackLayout>

                <Rock:Icon IconClass=""times""
                    VerticalTextAlignment=""Center""
                    Grid.Column=""1"" 
                    StyleClass=""note-read-more-icon""
                    HorizontalOptions=""End""
                    Padding=""8, 0"">
                    <Rock:Icon.GestureRecognizers>
                        <TapGestureRecognizer
                            Command=""{Binding DeleteFromSearchHistory}""
                            CommandParameter=""{{ Guid }}"" />
                    </Rock:Icon.GestureRecognizers>
                </Rock:Icon>
            </StackLayout>
        </Frame>
    <BoxView HorizontalOptions=""FillAndExpand""
        HeightRequest=""1""
        Color=""#cccccc"" />
</StackLayout>", "96576908-6125-4DF1-9A36-792ACA26E754" );

            // Attribute for BlockType
            //   BlockType: Live Experience Occurrences
            //   Category: Mobile > Events
            //   Attribute: Refresh Interval
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C45BA1C6-CE7F-4C37-82BF-A86D28BB28FE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Refresh Interval", "RefreshInterval", "Refresh Interval", @"If set to a value greater than 0 the block will automatically refresh itself every 'Refresh Interval' seconds. This only happens when the block is on the visible page. Even so care should be taken when using this and it should probably never be set below 60 (except 0 to disable it).", 5, @"0", "C3BB27B2-7AA2-4523-97BD-0C550A13A820" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Group Member Add Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "308DBA32-F656-418E-A019-9D18235027C1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Member Add Page", "GroupMemberAddPage", "Group Member Add Page", @"Page to use for adding a new group member. If no page is provided the built in group member edit panel will be used. This panel allows the individual to search the database.", 3, @"", "EA6E8E06-2E07-4B2E-B2FE-CA592002985F" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Configured Attendance Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "308DBA32-F656-418E-A019-9D18235027C1", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Configured Attendance Types", "AttendanceTypes", "Configured Attendance Types", @"The Attendance types that an occurrence can have. If no or one Attendance type is selected, then none will be shown.", 11, @"", "FC92A4A7-A24B-4DFB-B24C-635E6FEA90D1" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Workflow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "308DBA32-F656-418E-A019-9D18235027C1", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "Workflow", @"An optional workflow type to launch whenever attendance is saved. The Group will be used as the workflow 'Entity' when processing is started. Additionally if a 'StartDateTime' and/or 'Schedule' attribute exist, their values will be set with the corresponding saved attendance values.", 5, @"", "39E1B796-FA97-4F49-BCCD-8549C87126B0" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: List Item Details Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "308DBA32-F656-418E-A019-9D18235027C1", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "List Item Details Template", "ListItemDetailsTemplate", "List Item Details Template", @"An optional lava template to appear next to each person in the list.", 7, @"<div style=""display: flex; align-items: center; gap: 8px; padding: 12px;"">
    <img width=""80px"" height=""80px"" src=""{{ Person.PhotoUrl }}"" style=""border-radius: 80px; width: 80px; height: 80px"" />
    <div>
        <strong>{{ Person.LastName }}, {{ Person.NickName }}</strong>
        {% if GroupRoleName %}<div>{{ GroupRoleName }}</div>{% endif %}
        {% if GroupMember.GroupMemberStatus and GroupMember.GroupMemberStatus != 'Active' %}<span class=""label label-info"" style=""position: absolute; right: 10px; top: 10px;"">{{ GroupMember.GroupMemberStatus }}</span>{% endif %}
    </div>
</div>", "24CE976C-13F2-48CE-8FFD-8C281ABAD262" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Attendance Roster Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "308DBA32-F656-418E-A019-9D18235027C1", "890BAA33-5EBB-4343-A8AA-42E0E6C7467A", "Attendance Roster Template", "AttendanceRosterTemplate", "Attendance Roster Template", @"", 6, @"", "51C60463-64E1-46AC-A83D-92461C7F3F7A" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Attendance Note Label
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "308DBA32-F656-418E-A019-9D18235027C1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Attendance Note Label", "AttendanceNoteLabel", "Attendance Note Label", @"The text to use to describe the notes", 10, @"Notes", "4C643C42-9879-436F-8C8D-279F60361FE4" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Attendance Type Label
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "308DBA32-F656-418E-A019-9D18235027C1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Attendance Type Label", "AttendanceTypeLabel", "Attendance Type Label", @"The label that will be shown for the attendance types section.", 12, @"Attendance Location", "50F2D7A5-E62E-4F0D-8EFE-E9335B638920" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Allow Add
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "308DBA32-F656-418E-A019-9D18235027C1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Add", "AllowAdd", "Allow Add", @"Should block support adding new attendance dates outside of the group's configured schedule and group type's exclusion dates?", 0, @"True", "897CEB1C-27E1-436D-B6E9-691FDFEAA33D" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Allow Adding Person
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "308DBA32-F656-418E-A019-9D18235027C1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Adding Person", "AllowAddingPerson", "Allow Adding Person", @"Should block support adding new people as attendees?", 1, @"False", "845CB8A9-3B7A-4FCC-B118-BDC3CF0793BD" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Allow Campus Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "308DBA32-F656-418E-A019-9D18235027C1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Campus Filter", "AllowCampusFilter", "Allow Campus Filter", @"Should block add an option to allow filtering people and attendance counts by campus?", 4, @"False", "51C13F49-8031-4B2F-B4CF-2761606E3E2B" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Restrict Future Occurrence Date
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "308DBA32-F656-418E-A019-9D18235027C1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Restrict Future Occurrence Date", "RestrictFutureOccurrenceDate", "Restrict Future Occurrence Date", @"Should user be prevented from selecting a future Occurrence date?", 8, @"False", "2950A785-3D08-4208-8A1C-2DE6DFB42438" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Show Notes
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "308DBA32-F656-418E-A019-9D18235027C1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Notes", "ShowNotes", "Show Notes", @"Should the notes field be displayed?", 9, @"True", "9D5EE0B1-511C-4569-A099-A0AADB1C8E4D" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Disable Long-List
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "308DBA32-F656-418E-A019-9D18235027C1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Long-List", "DisableLongList", "Disable Long-List", @"Will disable the long-list feature which groups individuals by the first character of their last name. When enabled, this only shows when there are more than 50 individuals on the list.", 13, @"False", "6E6FB080-EF47-4F05-82CA-3F4C862366DE" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Disable Did Not Meet
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "308DBA32-F656-418E-A019-9D18235027C1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Did Not Meet", "DisableDidNotMeet", "Disable Did Not Meet", @"Allows for hiding the flag that the group did not meet.", 14, @"False", "1E5F4DE0-E0CF-484D-B1F4-8EEC3DAF64FE" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Hide Back Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "308DBA32-F656-418E-A019-9D18235027C1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Back Button", "HideBackButton", "Hide Back Button", @"Will hide the back button from the bottom of the block.", 15, @"False", "59870C86-71F8-4F59-810F-C16815757B77" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Add Person As
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "308DBA32-F656-418E-A019-9D18235027C1", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Add Person As", "AddPersonAs", "Add Person As", @"'Attendee' will only add the person to attendance. 'Group Member' will add them to the group with the default group role.", 2, @"Attendee", "3B06D84F-636F-4981-A40C-E9A632C96CDB" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Date Selection Mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "308DBA32-F656-418E-A019-9D18235027C1", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Date Selection Mode", "DateSelectionMode", "Date Selection Mode", @"'Date Picker' individual can pick any date. 'Current Date' locked to the current date. 'Pick From Schedule' drop down of dates from the schedule. This will need to be updated based on the location.", 16, @"1", "62F6D3F9-44FA-4CF0-A0CC-83D3538A6936" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Number of Previous Days To Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "308DBA32-F656-418E-A019-9D18235027C1", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Previous Days To Show", "NumberOfPreviousDaysToShow", "Number of Previous Days To Show", @"When the 'Pick From Schedule' option is used, this setting will control how many days back appear in the drop down list to choose from.", 17, @"14", "06B562ED-D2A1-4FA4-BCC3-D5A15CC8F74E" );

            // Add Block Attribute Value
            //   Block: Membership
            //   BlockType: Attribute Values
            //   Category: CRM > Person Detail
            //   Block Location: Page=Extended Attributes V1, Site=Rock RMS
            //   Attribute: Category
            /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */
            RockMigrationHelper.AddBlockAttributeValue( "3C95653C-FCF0-4C59-97D1-6206DDCF2762", "EC43CF32-3BDF-4544-8B6A-CE9208DD7C81", @"e919e722-f895-44a4-b86d-38db8fba1844" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType
            //   BlockType: Live Experience Occurrences
            //   Category: Mobile > Events
            //   Attribute: Refresh Interval
            RockMigrationHelper.DeleteAttribute( "C3BB27B2-7AA2-4523-97BD-0C550A13A820" );

            // Attribute for BlockType
            //   BlockType: Search
            //   Category: Mobile > Core
            //   Attribute: Stopped Typing Behavior Threshold
            RockMigrationHelper.DeleteAttribute( "37B63CFC-9FDB-442E-ADC4-22E4FFDC045D" );

            // Attribute for BlockType
            //   BlockType: Search
            //   Category: Mobile > Core
            //   Attribute: Historical Result Item Template
            RockMigrationHelper.DeleteAttribute( "96576908-6125-4DF1-9A36-792ACA26E754" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Number of Previous Days To Show
            RockMigrationHelper.DeleteAttribute( "06B562ED-D2A1-4FA4-BCC3-D5A15CC8F74E" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Date Selection Mode
            RockMigrationHelper.DeleteAttribute( "62F6D3F9-44FA-4CF0-A0CC-83D3538A6936" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Hide Back Button
            RockMigrationHelper.DeleteAttribute( "59870C86-71F8-4F59-810F-C16815757B77" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Disable Did Not Meet
            RockMigrationHelper.DeleteAttribute( "1E5F4DE0-E0CF-484D-B1F4-8EEC3DAF64FE" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Disable Long-List
            RockMigrationHelper.DeleteAttribute( "6E6FB080-EF47-4F05-82CA-3F4C862366DE" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Attendance Type Label
            RockMigrationHelper.DeleteAttribute( "50F2D7A5-E62E-4F0D-8EFE-E9335B638920" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Configured Attendance Types
            RockMigrationHelper.DeleteAttribute( "FC92A4A7-A24B-4DFB-B24C-635E6FEA90D1" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Attendance Note Label
            RockMigrationHelper.DeleteAttribute( "4C643C42-9879-436F-8C8D-279F60361FE4" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Show Notes
            RockMigrationHelper.DeleteAttribute( "9D5EE0B1-511C-4569-A099-A0AADB1C8E4D" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Restrict Future Occurrence Date
            RockMigrationHelper.DeleteAttribute( "2950A785-3D08-4208-8A1C-2DE6DFB42438" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: List Item Details Template
            RockMigrationHelper.DeleteAttribute( "24CE976C-13F2-48CE-8FFD-8C281ABAD262" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Attendance Roster Template
            RockMigrationHelper.DeleteAttribute( "51C60463-64E1-46AC-A83D-92461C7F3F7A" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Workflow
            RockMigrationHelper.DeleteAttribute( "39E1B796-FA97-4F49-BCCD-8549C87126B0" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Allow Campus Filter
            RockMigrationHelper.DeleteAttribute( "51C13F49-8031-4B2F-B4CF-2761606E3E2B" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Group Member Add Page
            RockMigrationHelper.DeleteAttribute( "EA6E8E06-2E07-4B2E-B2FE-CA592002985F" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Add Person As
            RockMigrationHelper.DeleteAttribute( "3B06D84F-636F-4981-A40C-E9A632C96CDB" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Allow Adding Person
            RockMigrationHelper.DeleteAttribute( "845CB8A9-3B7A-4FCC-B118-BDC3CF0793BD" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Attribute: Allow Add
            RockMigrationHelper.DeleteAttribute( "897CEB1C-27E1-436D-B6E9-691FDFEAA33D" );

            // Remove Block
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS
            //  from Page: Extended Attributes V1, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "3C95653C-FCF0-4C59-97D1-6206DDCF2762" );

            // Delete BlockType 
            //   Name: Group Attendance Detail
            //   Category: Obsidian > Groups
            //   Path: -
            //   EntityType: Group Attendance Detail
            RockMigrationHelper.DeleteBlockType( "308DBA32-F656-418E-A019-9D18235027C1" );
        }
    }
}
