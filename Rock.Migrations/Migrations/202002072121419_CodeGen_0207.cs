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
    public partial class CodeGen_0207 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
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
            RockMigrationHelper.UpdateBlockType("Attendance List","Block for displaying the attendance history of a person or a group.","~/Blocks/CheckIn/AttendanceList.ascx","Checkin","1300DE1C-0599-4FE7-9943-8944F09B0C7C");
            // Attrib for BlockType: Calendar View:Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FE004110-D090-4E13-8301-03DA73304BDC", "EC0D9528-1A22-404E-A776-566404987363", "Calendar", "Calendar", "Calendar", @"The calendar to pull events from", 0, @"", "301D56E8-7BE6-4B16-96D9-1ACBB75ED1A4" );
            // Attrib for BlockType: Calendar View:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FE004110-D090-4E13-8301-03DA73304BDC", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to push onto the navigation stack when viewing details of an event.", 1, @"", "6158058C-256A-4F17-8835-8DD4FC854F7B" );
            // Attrib for BlockType: Calendar View:Audience Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FE004110-D090-4E13-8301-03DA73304BDC", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Audience Filter", "AudienceFilter", "Audience Filter", @"Determines which audiences should be displayed in the filter.", 2, @"", "440C7E61-38DE-4315-B22C-393FA9CDDE38" );
            // Attrib for BlockType: Calendar View:Event Summary
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FE004110-D090-4E13-8301-03DA73304BDC", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Event Summary", "EventSummary", "Event Summary", @"The XAML to use when rendering the event summaries below the calendar.", 3, @"<Frame HasShadow=""false"" StyleClass=""calendar-event"">
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
", "0CC26B4D-95FA-413C-BF66-AB4076763BE5" );
            // Attrib for BlockType: Calendar View:Show Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FE004110-D090-4E13-8301-03DA73304BDC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Filter", "ShowFilter", "Show Filter", @"If enabled then the user will be able to apply custom filtering.", 4, @"True", "73C354D7-EE12-475F-BB30-67C5D794B964" );
            // Attrib for BlockType: Group Attendance Entry:Number of Days Forward to Allow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BCFF3505-472E-4086-8615-C77603FADE58", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Days Forward to Allow", "NumberOfDaysForwardToAllow", "Number of Days Forward to Allow", @"", 0, @"0", "61B90B3D-0683-4D9A-AAF5-FF3D87086C6A" );
            // Attrib for BlockType: Group Attendance Entry:Number of Days Back to Allow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BCFF3505-472E-4086-8615-C77603FADE58", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Days Back to Allow", "NumberOfDaysBackToAllow", "Number of Days Back to Allow", @"", 1, @"30", "BA4C6A38-F778-461D-B12C-E82EA7F9CD49" );
            // Attrib for BlockType: Group Attendance Entry:Save Redirect Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BCFF3505-472E-4086-8615-C77603FADE58", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Save Redirect Page", "SaveRedirectPage", "Save Redirect Page", @"If set, redirect user to this page on save. If not set, page is popped off the navigation stack.", 2, @"", "59F95697-A9B3-44B8-9EBC-986B56BA49FF" );
            // Attrib for BlockType: Group Attendance Entry:Show Save Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BCFF3505-472E-4086-8615-C77603FADE58", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Save Button", "ShowSaveButton", "Show Save Button", @"If enabled a save button will be shown (recommended for large groups), otherwise no save button will be displayed and a save will be triggered with each selection (recommended for smaller groups).", 3, @"False", "1D2990F1-A4A2-4AEB-8367-B9BF43C6A4A8" );
            // Attrib for BlockType: Group Attendance Entry:Allow Any Date Selection
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BCFF3505-472E-4086-8615-C77603FADE58", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Any Date Selection", "AllowAnyDateSelection", "Allow Any Date Selection", @"If enabled a date picker will be shown, otherwise a dropdown with only the valid dates will be shown.", 4, @"False", "F3BB1BFF-760E-4094-A75B-CEF954570508" );
            // Attrib for BlockType: Group Edit:Show Group Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "38FBC780-7C84-45DF-BA13-A11BD097EC2D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Name", "ShowGroupName", "Show Group Name", @"", 0, @"True", "980F15A5-C1BA-47F4-BE64-E83B56EC67DF" );
            // Attrib for BlockType: Group Edit:Enable Group Name Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "38FBC780-7C84-45DF-BA13-A11BD097EC2D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Group Name Edit", "EnableGroupNameEdit", "Enable Group Name Edit", @"", 1, @"True", "256F0966-F51B-4FD2-B8B1-A98B95D7F3DB" );
            // Attrib for BlockType: Group Edit:Show Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "38FBC780-7C84-45DF-BA13-A11BD097EC2D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Description", "ShowDescription", "Show Description", @"", 2, @"True", "543CBD9B-E17C-4825-A279-5EFE9AA60436" );
            // Attrib for BlockType: Group Edit:Enable Description Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "38FBC780-7C84-45DF-BA13-A11BD097EC2D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Description Edit", "EnableDescriptionEdit", "Enable Description Edit", @"", 3, @"True", "1E0C809D-00FE-41B1-BEF5-9238EABA7A14" );
            // Attrib for BlockType: Group Edit:Show Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "38FBC780-7C84-45DF-BA13-A11BD097EC2D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus", "ShowCampus", "Show Campus", @"", 4, @"True", "D0C42BF8-1699-458B-9BA4-7107DAF15F6A" );
            // Attrib for BlockType: Group Edit:Enable Campus Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "38FBC780-7C84-45DF-BA13-A11BD097EC2D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Campus Edit", "EnableCampusEdit", "Enable Campus Edit", @"", 5, @"True", "D26F6F03-44A7-445F-A13F-FF5577A6B4E8" );
            // Attrib for BlockType: Group Edit:Show Group Capacity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "38FBC780-7C84-45DF-BA13-A11BD097EC2D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Capacity", "ShowGroupCapacity", "Show Group Capacity", @"", 6, @"True", "8B64D554-C908-47AC-B1B6-A7EDA77EC490" );
            // Attrib for BlockType: Group Edit:Enable Group Capacity Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "38FBC780-7C84-45DF-BA13-A11BD097EC2D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Group Capacity Edit", "EnableGroupCapacityEdit", "Enable Group Capacity Edit", @"", 7, @"True", "249A4230-F475-46ED-AC3E-2BD69D810A9F" );
            // Attrib for BlockType: Group Edit:Show Active Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "38FBC780-7C84-45DF-BA13-A11BD097EC2D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Active Status", "ShowActiveStatus", "Show Active Status", @"", 8, @"True", "FBA74918-E760-417E-8026-5362263FD99D" );
            // Attrib for BlockType: Group Edit:Enable Active Status Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "38FBC780-7C84-45DF-BA13-A11BD097EC2D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Active Status Edit", "EnableActiveStatusEdit", "Enable Active Status Edit", @"", 9, @"True", "F73AC9AB-1C67-44A5-91DA-2065373AB1D5" );
            // Attrib for BlockType: Group Edit:Show Public Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "38FBC780-7C84-45DF-BA13-A11BD097EC2D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Public Status", "ShowPublicStatus", "Show Public Status", @"", 10, @"True", "23DE91CA-888C-4147-BAFA-084B032EB638" );
            // Attrib for BlockType: Group Edit:Enable Public Status Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "38FBC780-7C84-45DF-BA13-A11BD097EC2D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Public Status Edit", "EnablePublicStatusEdit", "Enable Public Status Edit", @"", 11, @"True", "F2EE0338-A26A-42AF-910A-179105EB313C" );
            // Attrib for BlockType: Group Edit:Attribute Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "38FBC780-7C84-45DF-BA13-A11BD097EC2D", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Attribute Category", "AttributeCategory", "Attribute Category", @"Category of attributes to show and allow editing on.", 12, @"", "B72185B6-633B-4410-941B-DDE59B40276D" );
            // Attrib for BlockType: Group Edit:Group Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "38FBC780-7C84-45DF-BA13-A11BD097EC2D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "Group Detail Page", @"The group detail page to return to, if not set then the edit page is popped off the navigation stack.", 13, @"", "C894D395-1DBD-42FE-B1CC-8349FCBE10D9" );
            // Attrib for BlockType: Group Member Edit:Allow Role Change
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "657EC06A-E37F-4480-803B-B9EDC64A19B9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Role Change", "AllowRoleChange", "Allow Role Change", @"", 0, @"True", "169FC952-CF64-458E-9E23-61F439E99E5B" );
            // Attrib for BlockType: Group Member Edit:Allow Member Status Change
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "657EC06A-E37F-4480-803B-B9EDC64A19B9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Member Status Change", "AllowMemberStatusChange", "Allow Member Status Change", @"", 1, @"True", "C38D12C5-E2F9-4E10-A6B5-AF2144F662F7" );
            // Attrib for BlockType: Group Member Edit:Allow Note Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "657EC06A-E37F-4480-803B-B9EDC64A19B9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Note Edit", "AllowNoteEdit", "Allow Note Edit", @"", 2, @"True", "CA277734-7726-46A9-8DF8-0935FE920705" );
            // Attrib for BlockType: Group Member Edit:Attribute Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "657EC06A-E37F-4480-803B-B9EDC64A19B9", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Attribute Category", "AttributeCategory", "Attribute Category", @"Category of attributes to show and allow editing on.", 3, @"", "7DD21667-D15B-4AA5-BAFD-167F0FD03B19" );
            // Attrib for BlockType: Group Member Edit:Member Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "657EC06A-E37F-4480-803B-B9EDC64A19B9", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Member Detail Page", "MemberDetailsPage", "Member Detail Page", @"The group member page to return to, if not set then the edit page is popped off the navigation stack.", 4, @"", "CC986570-2377-45CE-BA12-2DEA9216C8EF" );
            // Attrib for BlockType: Group Member List:Group Member Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "06DE01ED-F670-465F-BA1C-963322259EF1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Member Detail Page", "GroupMemberDetailPage", "Group Member Detail Page", @"The page that will display the group member details when selecting a member.", 0, @"", "BAD4A61F-9B47-4DCB-9576-51F4ED441E09" );
            // Attrib for BlockType: Group Member List:Title Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "06DE01ED-F670-465F-BA1C-963322259EF1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title Template", "TitleTemplate", "Title Template", @"The value to use when rendering the title text. <span class='tip tip-lava'></span>", 1, @"{{ Group.Name }} Group Roster", "9F78DE6F-6F90-4BF4-AC08-9A346B1AACAD" );
            // Attrib for BlockType: Group Member List:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "06DE01ED-F670-465F-BA1C-963322259EF1", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 2, @"", "FA9DED98-C9ED-45DA-B612-5512C1088795" );
            // Attrib for BlockType: Group Member List:Additional Fields
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "06DE01ED-F670-465F-BA1C-963322259EF1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Additional Fields", "AdditionalFields", "Additional Fields", @"", 3, @"", "264F1203-132C-4AF7-8B52-9CA79EB168B8" );
            // Attrib for BlockType: Group Member View:Group Member Edit Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D7A3A968-07FF-413C-A782-AA89A94FD2D2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Member Edit Page", "GroupMemberEditPage", "Group Member Edit Page", @"The page that will allow editing of a group member.", 0, @"", "B797CD4A-078C-44DB-B705-1BFD4AE6AD71" );
            // Attrib for BlockType: Group Member View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D7A3A968-07FF-413C-A782-AA89A94FD2D2", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 1, @"", "46D84322-6A94-4CF5-A025-A415EC1A1208" );
            // Attrib for BlockType: Group View:Group Edit Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "824B85FB-16A3-4609-877D-EF00F3DAA72F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Edit Page", "GroupEditPage", "Group Edit Page", @"The page that will allow editing of the group.", 0, @"", "2080D04B-A280-40F1-84CA-109B2CD4F975" );
            // Attrib for BlockType: Group View:Show Leader List
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "824B85FB-16A3-4609-877D-EF00F3DAA72F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Leader List", "ShowLeaderList", "Show Leader List", @"Specifies if the leader list should be shown, this value is made available to the Template as ShowLeaderList.", 1, @"True", "FE3AE226-B9A9-49F0-BBEF-2BCC8B73182A" );
            // Attrib for BlockType: Group View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "824B85FB-16A3-4609-877D-EF00F3DAA72F", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 2, @"", "E890EBF8-5933-4C9B-9C0E-96797BC3DCE3" );
            // Attrib for BlockType: Prayer Request Details:Show Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1CC3E675-680A-4FBC-9225-C4870159D45A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Category", "EnableCategory", "Show Category", @"If disabled, then the user will not be able to select a category and the default category will be used exclusively.", 0, @"True", "3210DDB6-DDFD-452A-A899-FC6D79E5905F" );
            // Attrib for BlockType: Prayer Request Details:Parent Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1CC3E675-680A-4FBC-9225-C4870159D45A", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Parent Category", "ParentCategory", "Parent Category", @"A top level category. This controls which categories the person can choose from when entering their prayer request.", 1, @"", "8F87AB66-D9DF-46D6-AEAD-48EA08927225" );
            // Attrib for BlockType: Prayer Request Details:Default Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1CC3E675-680A-4FBC-9225-C4870159D45A", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Default Category", "DefaultCategory", "Default Category", @"The default category to use for all new prayer requests.", 2, @"", "0797E05B-BBAD-4033-9D2D-1EF3EE9EFC39" );
            // Attrib for BlockType: Prayer Request Details:Enable Auto Approve
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1CC3E675-680A-4FBC-9225-C4870159D45A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Auto Approve", "EnableAutoApprove", "Enable Auto Approve", @"If enabled, prayer requests are automatically approved; otherwise they must be approved by an admin before they can be seen by the prayer team.", 0, @"True", "D3AA6BF6-2452-4D55-819E-464B81186FC4" );
            // Attrib for BlockType: Prayer Request Details:Expires After (Days)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1CC3E675-680A-4FBC-9225-C4870159D45A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Expires After (Days)", "ExpiresAfterDays", "Expires After (Days)", @"Number of days until the request will expire (only applies when auto-approved is enabled).", 1, @"14", "992D7CBC-8AB7-43D3-9793-70E3A8105342" );
            // Attrib for BlockType: Prayer Request Details:Show Urgent Flag
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1CC3E675-680A-4FBC-9225-C4870159D45A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Urgent Flag", "EnableUrgentFlag", "Show Urgent Flag", @"If enabled, requestors will be able to flag prayer requests as urgent.", 2, @"False", "CB66B1F7-4B0E-414F-A98B-EE9B34D934B2" );
            // Attrib for BlockType: Prayer Request Details:Show Public Display Flag
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1CC3E675-680A-4FBC-9225-C4870159D45A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Public Display Flag", "EnablePublicDisplayFlag", "Show Public Display Flag", @"If enabled, requestors will be able set whether or not they want their request displayed on the public website.", 3, @"False", "97E3C2A6-CE65-4F09-A4FD-B242CB23D14B" );
            // Attrib for BlockType: Prayer Request Details:Default To Public
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1CC3E675-680A-4FBC-9225-C4870159D45A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default To Public", "DefaultToPublic", "Default To Public", @"If enabled, all prayers will be set to public by default.", 4, @"False", "6392DC69-96EF-4E3B-BAE6-DFA635BC4729" );
            // Attrib for BlockType: Prayer Request Details:Character Limit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1CC3E675-680A-4FBC-9225-C4870159D45A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Character Limit", "CharacterLimit", "Character Limit", @"If set to something other than 0, this will limit the number of characters allowed when entering a new prayer request.", 5, @"250", "689514C8-5BF8-4EFF-B281-7943448FC160" );
            // Attrib for BlockType: Prayer Request Details:Show Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1CC3E675-680A-4FBC-9225-C4870159D45A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus", "EnableCampus", "Show Campus", @"Should the campus field be displayed? If there is only one active campus then the campus field will not show.", 6, @"True", "DBA85AA1-6BCA-4A99-AAE4-56409549DA10" );
            // Attrib for BlockType: Prayer Request Details:Require Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1CC3E675-680A-4FBC-9225-C4870159D45A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Campus", "RequireCampus", "Require Campus", @"Require that a campus be selected. The campus will not be displayed if there is only one available campus, in which case if this is set to true then the single campus is automatically used.", 7, @"False", "31CD3A9B-02D7-48FA-B5E3-F770C8423729" );
            // Attrib for BlockType: Prayer Request Details:Require Last Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1CC3E675-680A-4FBC-9225-C4870159D45A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Last Name", "RequireLastName", "Require Last Name", @"Require that a last name be entered. First name is always required.", 8, @"True", "FFE0F114-6930-4B79-9BAB-30CC66A3C710" );
            // Attrib for BlockType: Prayer Request Details:Enable Person Matching
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1CC3E675-680A-4FBC-9225-C4870159D45A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Person Matching", "EnablePersonMatching", "Enable Person Matching", @"If enabled, the request will be linked to an existing person if a match can be made between the requester and an existing person.", 9, @"False", "FF0EEAFB-EB88-4EC8-BA74-13340D242314" );
            // Attrib for BlockType: Prayer Request Details:Completion Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1CC3E675-680A-4FBC-9225-C4870159D45A", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Completion Action", "CompletionAction", "Completion Action", @"What action to perform after saving the prayer request.", 0, @"0", "41D99C08-F2B1-444E-AFD8-76BB5F6813AC" );
            // Attrib for BlockType: Prayer Request Details:Completion Xaml
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1CC3E675-680A-4FBC-9225-C4870159D45A", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Completion Xaml", "CompletionXaml", "Completion Xaml", @"The XAML markup that will be used if the. <span class='tip tip-lava'></span>", 1, @"<Rock:NotificationBox NotificationType=""Success"">
    Thank you for allowing us to pray for you.
</Rock:NotificationBox>", "E641A942-8D40-450F-82DD-97C298EA1597" );
            // Attrib for BlockType: Prayer Request Details:Workflow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1CC3E675-680A-4FBC-9225-C4870159D45A", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "Workflow", @"An optional workflow to start when prayer request is created. The PrayerRequest will be set as the workflow 'Entity' attribute when processing is started.", 2, @"", "3C0FFD8B-8107-41BE-B71F-F8F0BB409337" );
            // Attrib for BlockType: Rapid Attendance Entry:Enable Prayer Request Entry
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Prayer Request Entry", "EnablePrayerRequestEntry", "Enable Prayer Request Entry", @"If enabled, will show a section for entering a prayer request for the person.", 1, @"True", "2B415B33-D7A6-4452-9F01-051CAD04769C" );
            // Attrib for BlockType: Rapid Attendance Entry:Attendance Group
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Attendance Group", "AttendanceGroup", "Attendance Group", @"If selected will lock the block to the selected group. The individual would then only be able to select Schedule and Date when configuring.", 3, @"", "A99984CD-FD53-4CFF-A88D-1C15FFD657F0" );
            // Attrib for BlockType: Rapid Attendance Entry:Show Can Check-In Relationships
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Can Check-In Relationships", "ShowCanCheckInRelationships", "Show Can Check-In Relationships", @"If enabled, people who have a 'Can check-in' relationship will be shown.", 4, @"True", "E0EA0AA3-E253-4E5B-B000-F7A021E94D12" );
            // Attrib for BlockType: Rapid Attendance Entry:Attendance Age Limit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Attendance Age Limit", "AttendanceAgeLimit", "Attendance Age Limit", @"Individuals under this age will not be allowed to be marked as attended.", 5, @"", "A7C5307E-DF6D-4EFD-8624-3894C5AC0F35" );
            // Attrib for BlockType: Rapid Attendance Entry:Family Attributes
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Family Attributes", "FamilyAttributes", "Family Attributes", @"The family attributes to display when editing a family.", 1, @"", "D6A15358-37E5-4F5D-BBBF-D861ACDC7FCA" );
            // Attrib for BlockType: Rapid Attendance Entry:Header Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Header Lava Template", "FamilyHeaderLavaTemplate", "Header Lava Template", @"Lava for the family header at the top of the page.", 2, @"
<h4 class='margin-t-none'>{{ Family.Name }}</h4>
	{% if Family.GroupLocations != null %}
	{% assign groupLocations = Family.GroupLocations %}
	{% assign locationCount = groupLocations | Size %}
	    {% if locationCount > 0  %}
		{% for groupLocation in groupLocations %}
		{% if groupLocation.GroupLocationTypeValue.Value == 'Home' and groupLocation.Location.FormattedHtmlAddress != null and groupLocation.Location.FormattedHtmlAddress != ''  %}
		{{ groupLocation.Location.FormattedHtmlAddress }}
		{% endif %}
		{% endfor %}
		{% endif %}
	{% endif %}", "34132377-6B22-488F-BD95-99B1F03947E0" );
            // Attrib for BlockType: Rapid Attendance Entry:Header Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Header Lava Template", "IndividualHeaderLavaTemplate", "Header Lava Template", @"Lava template for the contents of the personal detail when viewing.", 1, @"
<div class='row margin-v-lg'>
    <div class='col-md-6'>
        {%- if Person.Age != null and Person.Age != '' -%}
        {{ Person.Age }} yrs old ({{ Person.BirthDate | Date:'M/d/yyyy' }}) <br />
        {%- endif -%}
        {%- if Person.Email != '' -%}
		<a href='mailto:{{ Person.Email }}'>{{ Person.Email }}</a>
        {%- endif -%}
		{% if Person.RecordStatusValue.Value != empty and Person.RecordStatusValue.Value == 'Inactive' -%}
		    <br/>
		    <span class='label label-danger' title='{{ Person.RecordStatusReasonValue.Value }}' data-toggle='tooltip'>{{ Person.RecordStatusValue.Value }}</span>
		    {% elseif Person.RecordStatusValue.Value != empty and Person.RecordStatusValue.Value == 'Pending' -%}
		    <span class='label label-warning' title='{{ Person.RecordStatusReasonValue.Value }}' data-toggle='tooltip'>{{ Person.RecordStatusValue.Value }}</span>
        {% endif -%}
    </div>
    <div class='col-md-6'>
        {% for phone in Person.PhoneNumbers %}
        {% if phone.IsUnlisted != true %}<a href='tel:{{ phone.NumberFormatted}}'>{{ phone.NumberFormatted }}</a>{% else %}Unlisted{% endif %}  <small>({{ phone.NumberTypeValue.Value }})</small><br />
		{% endfor %}
    </div>
</div>
", "D811B70C-A3F6-4AA0-B6A0-AC8D6CED19CE" );
            // Attrib for BlockType: Rapid Attendance Entry:Adult Phone Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Adult Phone Types", "AdultPhoneTypes", "Adult Phone Types", @"The types of phone numbers to display / edit.", 2, @"", "DB0814C7-3E17-474A-801F-9BBACB12A8F0" );
            // Attrib for BlockType: Rapid Attendance Entry:Adult Person Attributes
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Adult Person Attributes", "AdultPersonAttributes", "Adult Person Attributes", @"The attributes to display when editing a person that is an adult.", 3, @"", "DAE0953B-C23B-4540-B7D8-81667A03A45E" );
            // Attrib for BlockType: Rapid Attendance Entry:Show Communication Preference(Adults)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Communication Preference(Adults)", "ShowCommunicationPreference", "Show Communication Preference(Adults)", @"Shows the communication preference and allow it to be edited.", 4, @"True", "88F610EE-5021-4E22-BCC4-90C1B3D39171" );
            // Attrib for BlockType: Rapid Attendance Entry:Child Phone Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Child Phone Types", "ChildPhoneTypes", "Child Phone Types", @"The types of phone numbers to display / edit.", 5, @"", "2FF55982-A196-4D9F-A220-471F10330E58" );
            // Attrib for BlockType: Rapid Attendance Entry:Child Person Attributes
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Child Person Attributes", "ChildPersonAttributes", "Child Person Attributes", @"The attributes to display when editing a person that is a child.", 6, @"", "8E932FB5-63C0-45F6-AEC0-A4A056D1E724" );
            // Attrib for BlockType: Rapid Attendance Entry:Child Allow Email Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Child Allow Email Edit", "ChildAllowEmailEdit", "Child Allow Email Edit", @"If enabled, a child's email address will be visible/editable.", 7, @"True", "527B68AE-C61E-4B7E-AC6F-DE8F5EC3AB85" );
            // Attrib for BlockType: Rapid Attendance Entry:Enabled Urgent Flag
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enabled Urgent Flag", "ShowUrgentFlag", "Enabled Urgent Flag", @"If enabled, the request can be flagged as urgent by checking a checkbox.", 2, @"True", "D1E9181B-AF79-47E6-A39E-6E138F9B319C" );
            // Attrib for BlockType: Rapid Attendance Entry:Expires After (Days)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Expires After (Days)", "ExpiresAfter", "Expires After (Days)", @"Number of days until the request will expire.", 4, @"14", "494E9BD3-B636-4492-B700-21D3000213A4" );
            // Attrib for BlockType: Rapid Attendance Entry:Default Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Default Category", "DefaultCategory", "Default Category", @"The default category to use for all the new prayer requests.", 5, @"", "40B19834-9156-4B15-AAA7-CEE0CD4E0248" );
            // Attrib for BlockType: Rapid Attendance Entry:Display To Public
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display To Public", "DisplayToPublic", "Display To Public", @"If enabled, all prayers will be set to public by default.", 6, @"True", "B6C594A3-2F48-43CD-9353-CA9424A7F0B4" );
            // Attrib for BlockType: Rapid Attendance Entry:Default Allow Comments
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default Allow Comments", "DefaultAllowComments", "Default Allow Comments", @"Controls whether or not prayer requests are flagged to allow comments during prayer session.", 7, @"False", "2B0AD0A0-D603-4FD2-BF55-09FE668613E1" );
            // Attrib for BlockType: Rapid Attendance Entry:Category Selection
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Category Selection", "CategorySelection", "Category Selection", @"A top level category. This controls which categories the person can choose from when entering their prayer request.", 8, @"", "01F07E33-18F1-4165-97AC-D7697220CA53" );
            // Attrib for BlockType: Rapid Attendance Entry:Workflow List Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Workflow List Title", "WorkflowListTitle", "Workflow List Title", @"The text to show above the workflow list. (For example: I'm Interested in.)", 0, @"I'm interested in", "33C3D601-7867-49A4-9647-F7C154D80220" );
            // Attrib for BlockType: Rapid Attendance Entry:Workflow Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "1D50399B-0D6E-480B-A71A-E7BD37DD83F0", "Workflow Types", "WorkflowTypes", "Workflow Types", @"A list of workflows to display as a checkbox that can be selected and fired on behalf of the selected person on save.", 1, @"", "7A021BB0-65ED-42D9-A76E-7B7F1CC7A0AE" );
            // Attrib for BlockType: Rapid Attendance Entry:Note Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "276CCA63-5670-48CA-8B5A-2AAC97E8EE5E", "Note Types", "NoteTypes", "Note Types", @"The type of notes available to select on the form.", 1, @"", "B29FDE19-6653-49DB-96D7-D6480BDE76CB" );
            // Attrib for BlockType: Rapid Attendance Entry:Show Public Flag
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Public Flag", "ShowPublicFlag", "Show Public Flag", @"If enabled, a checkbox will be shown displayed on the public website.", 3, @"True", "9B156EB6-E043-4902-ABC5-DFCB6FBAF987" );
            // Attrib for BlockType: Rapid Attendance Entry:Add Family Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Family Page", "AddFamilyPage", "Add Family Page", @"Page used for adding new families.", 0, @"", "E627A965-404C-430F-A8C9-05DCD44668E7" );
            // Attrib for BlockType: Rapid Attendance Entry:Attendance List Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Attendance List Page", "AttendanceListPage", "Attendance List Page", @"Page used to show the attendance list.", 1, @"", "5874E75C-7514-456B-9AF0-FA7E1BAC23EE" );
            // Attrib for BlockType: Rapid Attendance Entry:Enable Attendance
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Attendance", "EnableAttendance", "Enable Attendance", @"If enabled, allows the individual to select a group, schedule, and attendance date at the start of the session and take attendance for family members.", 1, @"True", "B54BD16E-4871-4181-B7C3-80C945C6FE37" );
            // Attrib for BlockType: Welcome:Scan Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Scan Button Text", "ScanButtonText", "Scan Button Text", @"The text to display on the scan barcode button. Defaults to a pretty barcode SVG if left blank.", 14, @"", "024AF8D2-CAA3-422D-98EB-CB2C78AC3078" );
            // Attrib for BlockType: Registration Entry:Enabled Saved Account
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CABD2BFB-DFFF-42CD-BF1A-14F3BEE583DD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enabled Saved Account", "EnableSavedAccount", "Enabled Saved Account", @"Set this to false to disable the using Saved Account as a payment option, and to also disable the option to create saved account for future use.", 11, @"True", "B8999E7D-4B23-439A-9775-2969410E37E8" );
            // Attrib for BlockType: Transaction Entry (V2):Give Button Text - Scheduled
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Give Button Text - Scheduled", "GiveButtonScheduledText", "Give Button Text - Scheduled", @"", 5, @"Schedule Your Gift", "A12371CE-C56E-409C-AE55-26ABEF7BAFA0" );
            // Attrib for BlockType: Transaction Entry (V2):Amount Summary Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Amount Summary Template", "AmountSummaryTemplate", "Amount Summary Template", @"The text (HTML) to display on the amount summary page. <span class='tip tip-lava'></span>", 6, @"
{% assign sortedAccounts = Accounts | Sort:'Order,PublicName' %}

<span class='account-names'>{{ sortedAccounts | Map:'PublicName' | Join:', ' | ReplaceLast:',',' and' }}</span>
-
<span class='account-campus'>{{ Campus.Name }}</span>", "3450E373-248A-48DF-9D29-3D0AB558B8BD" );
            // Attrib for BlockType: Transaction Entry (V2):Give Button Text - Now 
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Give Button Text - Now ", "GiveButtonNowText", "Give Button Text - Now ", @"", 4, @"Give Now", "41F7E205-C81E-4A8E-A1D1-036EABAC03FE" );
            // Attrib for BlockType: Transaction Entry (V2):Include Inactive Campuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Inactive Campuses", "IncludeInactiveCampuses", "Include Inactive Campuses", @"Set this to true to include inactive campuses", 10, @"False", "84D72BE6-CBC5-4A6E-8087-4D4B5CBC4F33" );
            // Attrib for BlockType: Transaction Entry (V2):Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "IncludedCampusTypes", "Campus Types", @"Set this to limit campuses by campus type.", 11, @"", "6B670CA4-E3A9-4224-B5C8-7B8F23FB25A4" );
            // Attrib for BlockType: Transaction Entry (V2):Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6316D801-40C0-4EED-A2AD-55C13870664D", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "IncludedCampusStatuses", "Campus Statuses", @"Set this to limit campuses by campus status.", 12, @"", "87F50FA6-3131-4632-89E1-F5F694CE0505" );
            // Attrib for BlockType: Group Detail Lava:Enable Communication Preference
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "218B057F-B214-4317-8E84-7A95CF88067E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Communication Preference", "EnableCommunicationPreference", "Enable Communication Preference", @"Determines if the currently logged in individual should be allow to set their communication preference for the group.", 19, @"False", "AB9BA8A8-93AA-403A-A92D-DDE5A005A69C" );

        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Group Detail Lava:Enable Communication Preference
            RockMigrationHelper.DeleteAttribute("AB9BA8A8-93AA-403A-A92D-DDE5A005A69C");
            // Attrib for BlockType: Transaction Entry (V2):Campus Statuses
            RockMigrationHelper.DeleteAttribute("87F50FA6-3131-4632-89E1-F5F694CE0505");
            // Attrib for BlockType: Transaction Entry (V2):Campus Types
            RockMigrationHelper.DeleteAttribute("6B670CA4-E3A9-4224-B5C8-7B8F23FB25A4");
            // Attrib for BlockType: Transaction Entry (V2):Include Inactive Campuses
            RockMigrationHelper.DeleteAttribute("84D72BE6-CBC5-4A6E-8087-4D4B5CBC4F33");
            // Attrib for BlockType: Transaction Entry (V2):Give Button Text - Now 
            RockMigrationHelper.DeleteAttribute("41F7E205-C81E-4A8E-A1D1-036EABAC03FE");
            // Attrib for BlockType: Transaction Entry (V2):Amount Summary Template
            RockMigrationHelper.DeleteAttribute("3450E373-248A-48DF-9D29-3D0AB558B8BD");
            // Attrib for BlockType: Transaction Entry (V2):Give Button Text - Scheduled
            RockMigrationHelper.DeleteAttribute("A12371CE-C56E-409C-AE55-26ABEF7BAFA0");
            // Attrib for BlockType: Registration Entry:Enabled Saved Account
            RockMigrationHelper.DeleteAttribute("B8999E7D-4B23-439A-9775-2969410E37E8");
            // Attrib for BlockType: Welcome:Scan Button Text
            RockMigrationHelper.DeleteAttribute("024AF8D2-CAA3-422D-98EB-CB2C78AC3078");
            // Attrib for BlockType: Rapid Attendance Entry:Enable Attendance
            RockMigrationHelper.DeleteAttribute("B54BD16E-4871-4181-B7C3-80C945C6FE37");
            // Attrib for BlockType: Rapid Attendance Entry:Attendance List Page
            RockMigrationHelper.DeleteAttribute("5874E75C-7514-456B-9AF0-FA7E1BAC23EE");
            // Attrib for BlockType: Rapid Attendance Entry:Add Family Page
            RockMigrationHelper.DeleteAttribute("E627A965-404C-430F-A8C9-05DCD44668E7");
            // Attrib for BlockType: Rapid Attendance Entry:Show Public Flag
            RockMigrationHelper.DeleteAttribute("9B156EB6-E043-4902-ABC5-DFCB6FBAF987");
            // Attrib for BlockType: Rapid Attendance Entry:Note Types
            RockMigrationHelper.DeleteAttribute("B29FDE19-6653-49DB-96D7-D6480BDE76CB");
            // Attrib for BlockType: Rapid Attendance Entry:Workflow Types
            RockMigrationHelper.DeleteAttribute("7A021BB0-65ED-42D9-A76E-7B7F1CC7A0AE");
            // Attrib for BlockType: Rapid Attendance Entry:Workflow List Title
            RockMigrationHelper.DeleteAttribute("33C3D601-7867-49A4-9647-F7C154D80220");
            // Attrib for BlockType: Rapid Attendance Entry:Category Selection
            RockMigrationHelper.DeleteAttribute("01F07E33-18F1-4165-97AC-D7697220CA53");
            // Attrib for BlockType: Rapid Attendance Entry:Default Allow Comments
            RockMigrationHelper.DeleteAttribute("2B0AD0A0-D603-4FD2-BF55-09FE668613E1");
            // Attrib for BlockType: Rapid Attendance Entry:Display To Public
            RockMigrationHelper.DeleteAttribute("B6C594A3-2F48-43CD-9353-CA9424A7F0B4");
            // Attrib for BlockType: Rapid Attendance Entry:Default Category
            RockMigrationHelper.DeleteAttribute("40B19834-9156-4B15-AAA7-CEE0CD4E0248");
            // Attrib for BlockType: Rapid Attendance Entry:Expires After (Days)
            RockMigrationHelper.DeleteAttribute("494E9BD3-B636-4492-B700-21D3000213A4");
            // Attrib for BlockType: Rapid Attendance Entry:Enabled Urgent Flag
            RockMigrationHelper.DeleteAttribute("D1E9181B-AF79-47E6-A39E-6E138F9B319C");
            // Attrib for BlockType: Rapid Attendance Entry:Child Allow Email Edit
            RockMigrationHelper.DeleteAttribute("527B68AE-C61E-4B7E-AC6F-DE8F5EC3AB85");
            // Attrib for BlockType: Rapid Attendance Entry:Child Person Attributes
            RockMigrationHelper.DeleteAttribute("8E932FB5-63C0-45F6-AEC0-A4A056D1E724");
            // Attrib for BlockType: Rapid Attendance Entry:Child Phone Types
            RockMigrationHelper.DeleteAttribute("2FF55982-A196-4D9F-A220-471F10330E58");
            // Attrib for BlockType: Rapid Attendance Entry:Show Communication Preference(Adults)
            RockMigrationHelper.DeleteAttribute("88F610EE-5021-4E22-BCC4-90C1B3D39171");
            // Attrib for BlockType: Rapid Attendance Entry:Adult Person Attributes
            RockMigrationHelper.DeleteAttribute("DAE0953B-C23B-4540-B7D8-81667A03A45E");
            // Attrib for BlockType: Rapid Attendance Entry:Adult Phone Types
            RockMigrationHelper.DeleteAttribute("DB0814C7-3E17-474A-801F-9BBACB12A8F0");
            // Attrib for BlockType: Rapid Attendance Entry:Header Lava Template
            RockMigrationHelper.DeleteAttribute("D811B70C-A3F6-4AA0-B6A0-AC8D6CED19CE");
            // Attrib for BlockType: Rapid Attendance Entry:Header Lava Template
            RockMigrationHelper.DeleteAttribute("34132377-6B22-488F-BD95-99B1F03947E0");
            // Attrib for BlockType: Rapid Attendance Entry:Family Attributes
            RockMigrationHelper.DeleteAttribute("D6A15358-37E5-4F5D-BBBF-D861ACDC7FCA");
            // Attrib for BlockType: Rapid Attendance Entry:Attendance Age Limit
            RockMigrationHelper.DeleteAttribute("A7C5307E-DF6D-4EFD-8624-3894C5AC0F35");
            // Attrib for BlockType: Rapid Attendance Entry:Show Can Check-In Relationships
            RockMigrationHelper.DeleteAttribute("E0EA0AA3-E253-4E5B-B000-F7A021E94D12");
            // Attrib for BlockType: Rapid Attendance Entry:Attendance Group
            RockMigrationHelper.DeleteAttribute("A99984CD-FD53-4CFF-A88D-1C15FFD657F0");
            // Attrib for BlockType: Rapid Attendance Entry:Enable Prayer Request Entry
            RockMigrationHelper.DeleteAttribute("2B415B33-D7A6-4452-9F01-051CAD04769C");
            // Attrib for BlockType: Prayer Request Details:Workflow
            RockMigrationHelper.DeleteAttribute("3C0FFD8B-8107-41BE-B71F-F8F0BB409337");
            // Attrib for BlockType: Prayer Request Details:Completion Xaml
            RockMigrationHelper.DeleteAttribute("E641A942-8D40-450F-82DD-97C298EA1597");
            // Attrib for BlockType: Prayer Request Details:Completion Action
            RockMigrationHelper.DeleteAttribute("41D99C08-F2B1-444E-AFD8-76BB5F6813AC");
            // Attrib for BlockType: Prayer Request Details:Enable Person Matching
            RockMigrationHelper.DeleteAttribute("FF0EEAFB-EB88-4EC8-BA74-13340D242314");
            // Attrib for BlockType: Prayer Request Details:Require Last Name
            RockMigrationHelper.DeleteAttribute("FFE0F114-6930-4B79-9BAB-30CC66A3C710");
            // Attrib for BlockType: Prayer Request Details:Require Campus
            RockMigrationHelper.DeleteAttribute("31CD3A9B-02D7-48FA-B5E3-F770C8423729");
            // Attrib for BlockType: Prayer Request Details:Show Campus
            RockMigrationHelper.DeleteAttribute("DBA85AA1-6BCA-4A99-AAE4-56409549DA10");
            // Attrib for BlockType: Prayer Request Details:Character Limit
            RockMigrationHelper.DeleteAttribute("689514C8-5BF8-4EFF-B281-7943448FC160");
            // Attrib for BlockType: Prayer Request Details:Default To Public
            RockMigrationHelper.DeleteAttribute("6392DC69-96EF-4E3B-BAE6-DFA635BC4729");
            // Attrib for BlockType: Prayer Request Details:Show Public Display Flag
            RockMigrationHelper.DeleteAttribute("97E3C2A6-CE65-4F09-A4FD-B242CB23D14B");
            // Attrib for BlockType: Prayer Request Details:Show Urgent Flag
            RockMigrationHelper.DeleteAttribute("CB66B1F7-4B0E-414F-A98B-EE9B34D934B2");
            // Attrib for BlockType: Prayer Request Details:Expires After (Days)
            RockMigrationHelper.DeleteAttribute("992D7CBC-8AB7-43D3-9793-70E3A8105342");
            // Attrib for BlockType: Prayer Request Details:Enable Auto Approve
            RockMigrationHelper.DeleteAttribute("D3AA6BF6-2452-4D55-819E-464B81186FC4");
            // Attrib for BlockType: Prayer Request Details:Default Category
            RockMigrationHelper.DeleteAttribute("0797E05B-BBAD-4033-9D2D-1EF3EE9EFC39");
            // Attrib for BlockType: Prayer Request Details:Parent Category
            RockMigrationHelper.DeleteAttribute("8F87AB66-D9DF-46D6-AEAD-48EA08927225");
            // Attrib for BlockType: Prayer Request Details:Show Category
            RockMigrationHelper.DeleteAttribute("3210DDB6-DDFD-452A-A899-FC6D79E5905F");
            // Attrib for BlockType: Group View:Template
            RockMigrationHelper.DeleteAttribute("E890EBF8-5933-4C9B-9C0E-96797BC3DCE3");
            // Attrib for BlockType: Group View:Show Leader List
            RockMigrationHelper.DeleteAttribute("FE3AE226-B9A9-49F0-BBEF-2BCC8B73182A");
            // Attrib for BlockType: Group View:Group Edit Page
            RockMigrationHelper.DeleteAttribute("2080D04B-A280-40F1-84CA-109B2CD4F975");
            // Attrib for BlockType: Group Member View:Template
            RockMigrationHelper.DeleteAttribute("46D84322-6A94-4CF5-A025-A415EC1A1208");
            // Attrib for BlockType: Group Member View:Group Member Edit Page
            RockMigrationHelper.DeleteAttribute("B797CD4A-078C-44DB-B705-1BFD4AE6AD71");
            // Attrib for BlockType: Group Member List:Additional Fields
            RockMigrationHelper.DeleteAttribute("264F1203-132C-4AF7-8B52-9CA79EB168B8");
            // Attrib for BlockType: Group Member List:Template
            RockMigrationHelper.DeleteAttribute("FA9DED98-C9ED-45DA-B612-5512C1088795");
            // Attrib for BlockType: Group Member List:Title Template
            RockMigrationHelper.DeleteAttribute("9F78DE6F-6F90-4BF4-AC08-9A346B1AACAD");
            // Attrib for BlockType: Group Member List:Group Member Detail Page
            RockMigrationHelper.DeleteAttribute("BAD4A61F-9B47-4DCB-9576-51F4ED441E09");
            // Attrib for BlockType: Group Member Edit:Member Detail Page
            RockMigrationHelper.DeleteAttribute("CC986570-2377-45CE-BA12-2DEA9216C8EF");
            // Attrib for BlockType: Group Member Edit:Attribute Category
            RockMigrationHelper.DeleteAttribute("7DD21667-D15B-4AA5-BAFD-167F0FD03B19");
            // Attrib for BlockType: Group Member Edit:Allow Note Edit
            RockMigrationHelper.DeleteAttribute("CA277734-7726-46A9-8DF8-0935FE920705");
            // Attrib for BlockType: Group Member Edit:Allow Member Status Change
            RockMigrationHelper.DeleteAttribute("C38D12C5-E2F9-4E10-A6B5-AF2144F662F7");
            // Attrib for BlockType: Group Member Edit:Allow Role Change
            RockMigrationHelper.DeleteAttribute("169FC952-CF64-458E-9E23-61F439E99E5B");
            // Attrib for BlockType: Group Edit:Group Detail Page
            RockMigrationHelper.DeleteAttribute("C894D395-1DBD-42FE-B1CC-8349FCBE10D9");
            // Attrib for BlockType: Group Edit:Attribute Category
            RockMigrationHelper.DeleteAttribute("B72185B6-633B-4410-941B-DDE59B40276D");
            // Attrib for BlockType: Group Edit:Enable Public Status Edit
            RockMigrationHelper.DeleteAttribute("F2EE0338-A26A-42AF-910A-179105EB313C");
            // Attrib for BlockType: Group Edit:Show Public Status
            RockMigrationHelper.DeleteAttribute("23DE91CA-888C-4147-BAFA-084B032EB638");
            // Attrib for BlockType: Group Edit:Enable Active Status Edit
            RockMigrationHelper.DeleteAttribute("F73AC9AB-1C67-44A5-91DA-2065373AB1D5");
            // Attrib for BlockType: Group Edit:Show Active Status
            RockMigrationHelper.DeleteAttribute("FBA74918-E760-417E-8026-5362263FD99D");
            // Attrib for BlockType: Group Edit:Enable Group Capacity Edit
            RockMigrationHelper.DeleteAttribute("249A4230-F475-46ED-AC3E-2BD69D810A9F");
            // Attrib for BlockType: Group Edit:Show Group Capacity
            RockMigrationHelper.DeleteAttribute("8B64D554-C908-47AC-B1B6-A7EDA77EC490");
            // Attrib for BlockType: Group Edit:Enable Campus Edit
            RockMigrationHelper.DeleteAttribute("D26F6F03-44A7-445F-A13F-FF5577A6B4E8");
            // Attrib for BlockType: Group Edit:Show Campus
            RockMigrationHelper.DeleteAttribute("D0C42BF8-1699-458B-9BA4-7107DAF15F6A");
            // Attrib for BlockType: Group Edit:Enable Description Edit
            RockMigrationHelper.DeleteAttribute("1E0C809D-00FE-41B1-BEF5-9238EABA7A14");
            // Attrib for BlockType: Group Edit:Show Description
            RockMigrationHelper.DeleteAttribute("543CBD9B-E17C-4825-A279-5EFE9AA60436");
            // Attrib for BlockType: Group Edit:Enable Group Name Edit
            RockMigrationHelper.DeleteAttribute("256F0966-F51B-4FD2-B8B1-A98B95D7F3DB");
            // Attrib for BlockType: Group Edit:Show Group Name
            RockMigrationHelper.DeleteAttribute("980F15A5-C1BA-47F4-BE64-E83B56EC67DF");
            // Attrib for BlockType: Group Attendance Entry:Allow Any Date Selection
            RockMigrationHelper.DeleteAttribute("F3BB1BFF-760E-4094-A75B-CEF954570508");
            // Attrib for BlockType: Group Attendance Entry:Show Save Button
            RockMigrationHelper.DeleteAttribute("1D2990F1-A4A2-4AEB-8367-B9BF43C6A4A8");
            // Attrib for BlockType: Group Attendance Entry:Save Redirect Page
            RockMigrationHelper.DeleteAttribute("59F95697-A9B3-44B8-9EBC-986B56BA49FF");
            // Attrib for BlockType: Group Attendance Entry:Number of Days Back to Allow
            RockMigrationHelper.DeleteAttribute("BA4C6A38-F778-461D-B12C-E82EA7F9CD49");
            // Attrib for BlockType: Group Attendance Entry:Number of Days Forward to Allow
            RockMigrationHelper.DeleteAttribute("61B90B3D-0683-4D9A-AAF5-FF3D87086C6A");
            // Attrib for BlockType: Calendar View:Show Filter
            RockMigrationHelper.DeleteAttribute("73C354D7-EE12-475F-BB30-67C5D794B964");
            // Attrib for BlockType: Calendar View:Event Summary
            RockMigrationHelper.DeleteAttribute("0CC26B4D-95FA-413C-BF66-AB4076763BE5");
            // Attrib for BlockType: Calendar View:Audience Filter
            RockMigrationHelper.DeleteAttribute("440C7E61-38DE-4315-B22C-393FA9CDDE38");
            // Attrib for BlockType: Calendar View:Detail Page
            RockMigrationHelper.DeleteAttribute("6158058C-256A-4F17-8835-8DD4FC854F7B");
            // Attrib for BlockType: Calendar View:Calendar
            RockMigrationHelper.DeleteAttribute("301D56E8-7BE6-4B16-96D9-1ACBB75ED1A4");
            RockMigrationHelper.DeleteBlockType("1300DE1C-0599-4FE7-9943-8944F09B0C7C"); // Attendance List
            RockMigrationHelper.DeleteBlockType("1CC3E675-680A-4FBC-9225-C4870159D45A"); // Prayer Request Details
            RockMigrationHelper.DeleteBlockType("824B85FB-16A3-4609-877D-EF00F3DAA72F"); // Group View
            RockMigrationHelper.DeleteBlockType("D7A3A968-07FF-413C-A782-AA89A94FD2D2"); // Group Member View
            RockMigrationHelper.DeleteBlockType("06DE01ED-F670-465F-BA1C-963322259EF1"); // Group Member List
            RockMigrationHelper.DeleteBlockType("657EC06A-E37F-4480-803B-B9EDC64A19B9"); // Group Member Edit
            RockMigrationHelper.DeleteBlockType("38FBC780-7C84-45DF-BA13-A11BD097EC2D"); // Group Edit
            RockMigrationHelper.DeleteBlockType("BCFF3505-472E-4086-8615-C77603FADE58"); // Group Attendance Entry
            RockMigrationHelper.DeleteBlockType("FE004110-D090-4E13-8301-03DA73304BDC"); // Calendar View
        }
    
    }
}
