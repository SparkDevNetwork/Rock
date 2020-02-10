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
    public partial class Rollup_0121 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            AddWarningToLegacySystemEmailBlocks();
            RemoveAssessmentBlockAttributeAllowRetakes();
            StructuredContentToolDefinedTypeUp();
            DeviceTypeAttribute();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            StructuredContentToolDefinedTypeDown();
            CodeGenMigrationsDown();
        }

        /// <summary>
        /// Script generated "Up" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            // Attrib for BlockType: Calendar View:Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE8A6786-B002-4E91-AF99-2970C09E54D6", "EC0D9528-1A22-404E-A776-566404987363", "Calendar", "Calendar", "Calendar", @"The calendar to pull events from", 0, @"", "A39797B3-61FE-41C8-A717-847BE778237D" );
            // Attrib for BlockType: Calendar View:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE8A6786-B002-4E91-AF99-2970C09E54D6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to push onto the navigation stack when viewing details of an event.", 1, @"", "A221789F-674E-4D91-BE7F-7509B7172251" );
            // Attrib for BlockType: Calendar View:Audience Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE8A6786-B002-4E91-AF99-2970C09E54D6", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Audience Filter", "AudienceFilter", "Audience Filter", @"Determines which audiences should be displayed in the filter.", 2, @"", "1399A25B-AB59-4913-BE04-D1051753436C" );
            // Attrib for BlockType: Calendar View:Event Summary
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE8A6786-B002-4E91-AF99-2970C09E54D6", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Event Summary", "EventSummary", "Event Summary", @"The XAML to use when rendering the event summaries below the calendar.", 3, @"<Frame HasShadow=""false"" StyleClass=""calendar-event"">
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
", "347C6DE1-A972-47CD-9C67-A1ABA1D237A6" );
            // Attrib for BlockType: Calendar View:Show Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AE8A6786-B002-4E91-AF99-2970C09E54D6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Filter", "ShowFilter", "Show Filter", @"If enabled then the user will be able to apply custom filtering.", 4, @"True", "A4436F1C-B907-428C-B5EA-8B2AE97DF5E4" );
            // Attrib for BlockType: Group Attendance Entry:Number of Days Forward to Allow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5C7FF9AD-582F-4D2F-AA7E-6EF401B66D68", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Days Forward to Allow", "NumberOfDaysForwardToAllow", "Number of Days Forward to Allow", @"", 0, @"0", "DA1918FA-871C-49CB-911C-9745F9F8F342" );
            // Attrib for BlockType: Group Attendance Entry:Number of Days Back to Allow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5C7FF9AD-582F-4D2F-AA7E-6EF401B66D68", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Days Back to Allow", "NumberOfDaysBackToAllow", "Number of Days Back to Allow", @"", 1, @"30", "13B29580-9A0D-4916-B6FC-E7B515663948" );
            // Attrib for BlockType: Group Attendance Entry:Save Redirect Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5C7FF9AD-582F-4D2F-AA7E-6EF401B66D68", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Save Redirect Page", "SaveRedirectPage", "Save Redirect Page", @"If set, redirect user to this page on save. If not set, page is popped off the navigation stack.", 2, @"", "83FA5BD1-181C-4CD9-9F22-D153DE319E67" );
            // Attrib for BlockType: Group Attendance Entry:Show Save Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5C7FF9AD-582F-4D2F-AA7E-6EF401B66D68", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Save Button", "ShowSaveButton", "Show Save Button", @"If enabled a save button will be shown (recommended for large groups), otherwise no save button will be displayed and a save will be triggered with each selection (recommended for smaller groups).", 3, @"False", "344B0582-03D1-47CA-96BA-F8A9BF6984B6" );
            // Attrib for BlockType: Group Attendance Entry:Allow Any Date Selection
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5C7FF9AD-582F-4D2F-AA7E-6EF401B66D68", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Any Date Selection", "AllowAnyDateSelection", "Allow Any Date Selection", @"If enabled a date picker will be shown, otherwise a dropdown with only the valid dates will be shown.", 4, @"False", "AEB89129-2406-4D8A-B2CA-2507898D71C1" );
            // Attrib for BlockType: Group Edit:Show Group Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C1F40F48-19CC-40A2-AF96-C5BFB0AB437C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Name", "ShowGroupName", "Show Group Name", @"", 0, @"True", "6C3DD530-9468-4A38-A323-DE164C052AE6" );
            // Attrib for BlockType: Group Edit:Enable Group Name Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C1F40F48-19CC-40A2-AF96-C5BFB0AB437C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Group Name Edit", "EnableGroupNameEdit", "Enable Group Name Edit", @"", 1, @"True", "0D7FB0C7-37FC-427D-A4E7-D29876A00B0B" );
            // Attrib for BlockType: Group Edit:Show Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C1F40F48-19CC-40A2-AF96-C5BFB0AB437C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Description", "ShowDescription", "Show Description", @"", 2, @"True", "D5A629F5-176E-471C-BED1-C55134668447" );
            // Attrib for BlockType: Group Edit:Enable Description Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C1F40F48-19CC-40A2-AF96-C5BFB0AB437C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Description Edit", "EnableDescriptionEdit", "Enable Description Edit", @"", 3, @"True", "DF7532D9-7073-4F42-9397-71BF8EF549CB" );
            // Attrib for BlockType: Group Edit:Show Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C1F40F48-19CC-40A2-AF96-C5BFB0AB437C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus", "ShowCampus", "Show Campus", @"", 4, @"True", "C42B8997-FC26-4BCE-93AC-AEE17616A89E" );
            // Attrib for BlockType: Group Edit:Enable Campus Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C1F40F48-19CC-40A2-AF96-C5BFB0AB437C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Campus Edit", "EnableCampusEdit", "Enable Campus Edit", @"", 5, @"True", "4BF2E1E4-C6D2-4700-AC0F-D1196B871C89" );
            // Attrib for BlockType: Group Edit:Show Group Capacity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C1F40F48-19CC-40A2-AF96-C5BFB0AB437C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Capacity", "ShowGroupCapacity", "Show Group Capacity", @"", 6, @"True", "BD232927-D71A-4EE9-BD27-D7A8FD75BD29" );
            // Attrib for BlockType: Group Edit:Enable Group Capacity Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C1F40F48-19CC-40A2-AF96-C5BFB0AB437C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Group Capacity Edit", "EnableGroupCapacityEdit", "Enable Group Capacity Edit", @"", 7, @"True", "23D0366C-4AE6-4E6D-AD05-0534CAB52818" );
            // Attrib for BlockType: Group Edit:Show Active Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C1F40F48-19CC-40A2-AF96-C5BFB0AB437C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Active Status", "ShowActiveStatus", "Show Active Status", @"", 8, @"True", "A45ADA2C-F0C2-4375-9261-92BD171096F8" );
            // Attrib for BlockType: Group Edit:Enable Active Status Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C1F40F48-19CC-40A2-AF96-C5BFB0AB437C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Active Status Edit", "EnableActiveStatusEdit", "Enable Active Status Edit", @"", 9, @"True", "93F3D753-4E75-41C0-AD07-9D5CCAAB83EB" );
            // Attrib for BlockType: Group Edit:Show Public Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C1F40F48-19CC-40A2-AF96-C5BFB0AB437C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Public Status", "ShowPublicStatus", "Show Public Status", @"", 10, @"True", "9848DCE8-7CE5-4D74-88BA-64CB3C1C72E5" );
            // Attrib for BlockType: Group Edit:Enable Public Status Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C1F40F48-19CC-40A2-AF96-C5BFB0AB437C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Public Status Edit", "EnablePublicStatusEdit", "Enable Public Status Edit", @"", 11, @"True", "CD8C603B-6DB2-475B-9762-9170C757A5A7" );
            // Attrib for BlockType: Group Edit:Attribute Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C1F40F48-19CC-40A2-AF96-C5BFB0AB437C", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Attribute Category", "AttributeCategory", "Attribute Category", @"Category of attributes to show and allow editing on.", 12, @"", "C7417D6B-76BA-488D-B2E3-755429BA3D17" );
            // Attrib for BlockType: Group Edit:Group Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C1F40F48-19CC-40A2-AF96-C5BFB0AB437C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "Group Detail Page", @"The group detail page to return to, if not set then the edit page is popped off the navigation stack.", 13, @"", "D15A36FA-1ACA-4F24-9E50-BD8DBE50EB34" );
            // Attrib for BlockType: Group Member Edit:Allow Role Change
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "50EBC88C-62B5-45D8-B5FD-33F338F1C973", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Role Change", "AllowRoleChange", "Allow Role Change", @"", 0, @"True", "7E617C42-99C9-46F0-936B-4B87EE06256D" );
            // Attrib for BlockType: Group Member Edit:Allow Member Status Change
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "50EBC88C-62B5-45D8-B5FD-33F338F1C973", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Member Status Change", "AllowMemberStatusChange", "Allow Member Status Change", @"", 1, @"True", "D9BD93EB-9F3A-4B0C-A899-5640CE1508B5" );
            // Attrib for BlockType: Group Member Edit:Allow Note Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "50EBC88C-62B5-45D8-B5FD-33F338F1C973", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Note Edit", "AllowNoteEdit", "Allow Note Edit", @"", 2, @"True", "681BB863-9788-4F8E-8A39-5AEBA96FB431" );
            // Attrib for BlockType: Group Member Edit:Attribute Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "50EBC88C-62B5-45D8-B5FD-33F338F1C973", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Attribute Category", "AttributeCategory", "Attribute Category", @"Category of attributes to show and allow editing on.", 3, @"", "9B998F41-CE10-4945-98A9-1A690F5ABCCB" );
            // Attrib for BlockType: Group Member Edit:Member Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "50EBC88C-62B5-45D8-B5FD-33F338F1C973", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Member Detail Page", "MemberDetailsPage", "Member Detail Page", @"The group member page to return to, if not set then the edit page is popped off the navigation stack.", 4, @"", "FFDAD6E5-10A7-48EF-BFB3-2AA192FF0BEC" );
            // Attrib for BlockType: Group Member List:Group Member Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D695B899-F1A9-4EB1-B4D8-86D875E72D4E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Member Detail Page", "GroupMemberDetailPage", "Group Member Detail Page", @"The page to that will display the group member details when selecting a member.", 0, @"", "635BB1B8-BD29-4FF4-8A88-178623539BC8" );
            // Attrib for BlockType: Group Member List:Title Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D695B899-F1A9-4EB1-B4D8-86D875E72D4E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title Template", "TitleTemplate", "Title Template", @"The value to use when rendering the title text. <span class='tip tip-lava'></span>", 1, @"{{ Group.Name }} Group Roster", "E226DC34-CAC1-43E3-98F0-6AE0AC0C1741" );
            // Attrib for BlockType: Group Member List:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D695B899-F1A9-4EB1-B4D8-86D875E72D4E", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 2, @"", "782BB4F1-8FB0-48A6-B429-742D21316048" );
            // Attrib for BlockType: Prayer Request Details:Show Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56A97F0B-9E8D-4A67-BDA4-49B4E506B264", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Category", "EnableCategory", "Show Category", @"If disabled, then the user will not be able to select a category and the default category will be used exclusively.", 0, @"True", "5645E84B-C0F1-49FC-B57A-8875923AA9F7" );
            // Attrib for BlockType: Prayer Request Details:Parent Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56A97F0B-9E8D-4A67-BDA4-49B4E506B264", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Parent Category", "ParentCategory", "Parent Category", @"A top level category. This controls which categories the person can choose from when entering their prayer request.", 1, @"", "4B711B43-53FB-4140-9E87-DD658A123D40" );
            // Attrib for BlockType: Prayer Request Details:Default Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56A97F0B-9E8D-4A67-BDA4-49B4E506B264", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Default Category", "DefaultCategory", "Default Category", @"The default category to use for all new prayer requests.", 2, @"", "CD6992F5-C1F0-4C50-A62F-2C8C735C902E" );
            // Attrib for BlockType: Prayer Request Details:Enable Auto Approve
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56A97F0B-9E8D-4A67-BDA4-49B4E506B264", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Auto Approve", "EnableAutoApprove", "Enable Auto Approve", @"If enabled, prayer requests are automatically approved; otherwise they must be approved by an admin before they can be seen by the prayer team.", 0, @"True", "1209FAEE-910E-4C67-8FEF-7381369C7A68" );
            // Attrib for BlockType: Prayer Request Details:Expires After (Days)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56A97F0B-9E8D-4A67-BDA4-49B4E506B264", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Expires After (Days)", "ExpiresAfterDays", "Expires After (Days)", @"Number of days until the request will expire (only applies when auto-approved is enabled).", 1, @"14", "98F4F327-7428-418A-9587-44E381CA2068" );
            // Attrib for BlockType: Prayer Request Details:Show Urgent Flag
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56A97F0B-9E8D-4A67-BDA4-49B4E506B264", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Urgent Flag", "EnableUrgentFlag", "Show Urgent Flag", @"If enabled, requestors will be able to flag prayer requests as urgent.", 2, @"False", "5CF14B20-0108-44BD-B12D-874F45C62D98" );
            // Attrib for BlockType: Prayer Request Details:Show Public Display Flag
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56A97F0B-9E8D-4A67-BDA4-49B4E506B264", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Public Display Flag", "EnablePublicDisplayFlag", "Show Public Display Flag", @"If enabled, requestors will be able set whether or not they want their request displayed on the public website.", 3, @"False", "1D9FEFE9-68CE-400E-B3E2-BEEDC4DF02CA" );
            // Attrib for BlockType: Prayer Request Details:Default To Public
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56A97F0B-9E8D-4A67-BDA4-49B4E506B264", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default To Public", "DefaultToPublic", "Default To Public", @"If enabled, all prayers will be set to public by default.", 4, @"False", "61FD7017-03E9-4C83-B456-BAA8A7ACED8D" );
            // Attrib for BlockType: Prayer Request Details:Character Limit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56A97F0B-9E8D-4A67-BDA4-49B4E506B264", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Character Limit", "CharacterLimit", "Character Limit", @"If set to something other than 0, this will limit the number of characters allowed when entering a new prayer request.", 5, @"250", "33EA26F2-ACFA-49BB-A0F3-C4F1CF87CED3" );
            // Attrib for BlockType: Prayer Request Details:Show Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56A97F0B-9E8D-4A67-BDA4-49B4E506B264", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus", "EnableCampus", "Show Campus", @"Should the campus field be displayed? If there is only one active campus then the campus field will not show.", 6, @"True", "471640E1-D37E-4E47-BACE-059550A2D931" );
            // Attrib for BlockType: Prayer Request Details:Require Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56A97F0B-9E8D-4A67-BDA4-49B4E506B264", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Campus", "RequireCampus", "Require Campus", @"Require that a campus be selected. The campus will not be displayed if there is only one available campus, in which case if this is set to true then the single campus is automatically used.", 7, @"False", "BAA123F7-476F-43EF-A948-3E82D90FC94F" );
            // Attrib for BlockType: Prayer Request Details:Require Last Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56A97F0B-9E8D-4A67-BDA4-49B4E506B264", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Last Name", "RequireLastName", "Require Last Name", @"Require that a last name be entered. First name is always required.", 8, @"True", "8A550979-1F89-4ABD-A391-236ABEE49527" );
            // Attrib for BlockType: Prayer Request Details:Enable Person Matching
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56A97F0B-9E8D-4A67-BDA4-49B4E506B264", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Person Matching", "EnablePersonMatching", "Enable Person Matching", @"If enabled, the request will be linked to an existing person if a match can be made between the requester and an existing person.", 9, @"False", "7D7D6009-35C8-440B-9056-90E8AD4045AF" );
            // Attrib for BlockType: Prayer Request Details:Completion Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56A97F0B-9E8D-4A67-BDA4-49B4E506B264", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Completion Action", "CompletionAction", "Completion Action", @"What action to perform after saving the prayer request.", 0, @"0", "EB14BC94-EE3D-4933-A981-CE9DD7EBBA88" );
            // Attrib for BlockType: Prayer Request Details:Completion Xaml
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56A97F0B-9E8D-4A67-BDA4-49B4E506B264", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Completion Xaml", "CompletionXaml", "Completion Xaml", @"The XAML markup that will be used if the. <span class='tip tip-lava'></span>", 1, @"<Rock:NotificationBox NotificationType=""Success"">
    Thank you for allowing us to pray for you.
</Rock:NotificationBox>", "227A4870-BE30-4A34-8F4B-EA8CEFED57A7" );
            // Attrib for BlockType: Prayer Request Details:Workflow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56A97F0B-9E8D-4A67-BDA4-49B4E506B264", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "Workflow", @"An optional workflow to start when prayer request is created. The PrayerRequest will be set as the workflow 'Entity' attribute when processing is started.", 2, @"", "56711BF1-19A0-43B2-BC7B-422A62EA9AAC" );
            // Attrib for BlockType: Welcome:iPad Camera Barcode Configuration
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "iPad Camera Barcode Configuration", "CameraBarcodeConfiguration", "iPad Camera Barcode Configuration", @"desc", 7, @"Available", "B7BD8FDE-479E-4450-8DE7-F53A6C37F19F" );
            // Attrib for BlockType: Transaction Matching:Default Person Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1A8BEE2A-E5BE-4BA5-AFDB-E9C9278419BA", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Default Person Connection Status", "DefaultPersonConnectionStatus", "Default Person Connection Status", @"The connection status to default the connection status dropdown to.", 7, @"", "F25A121C-9034-4506-B5CF-D0F633334A1A" );
            // Attrib for BlockType: Transaction Matching:Person Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1A8BEE2A-E5BE-4BA5-AFDB-E9C9278419BA", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Person Record Status", "PersonRecordStatus", "Person Record Status", @"The default status to use when adding a person. This is not shown as a selection.", 8, @"618F906C-C33D-4FA3-8AEF-E58CB7B63F1E", "67450E73-CF6D-4E09-809D-5566E46CA2AA" );
            // Attrib for BlockType: Transaction Matching:Show Family Role
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1A8BEE2A-E5BE-4BA5-AFDB-E9C9278419BA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Family Role", "ShowFamilyRole", "Show Family Role", @"Determines if the Adult/Child toggle should be shown. If hidden, the role will be set to adult. The default will always be adult.", 9, @"True", "12C9276C-3926-47E4-9E0E-2BC2D6EB8F82" );
            // Attrib for BlockType: Transaction Matching:Show Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1A8BEE2A-E5BE-4BA5-AFDB-E9C9278419BA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Email", "ShowEmail", "Show Email", @"Determines if the email address field should be shown.", 9, @"False", "E4E724F4-90CF-4AEA-A114-F3DFF5BD0AF7" );
            RockMigrationHelper.UpdateFieldType("Block Template","","Rock","Rock.Field.Types.BlockTemplateFieldType","CCD73456-C83B-4D6E-BD69-8133D2EB996D");
            RockMigrationHelper.UpdateFieldType("Structure Content Editor","","Rock","Rock.Field.Types.StructureContentEditorFieldType","92C88D02-CE12-4217-80FB-19422B758437");

        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Transaction Matching:Show Email
            RockMigrationHelper.DeleteAttribute("E4E724F4-90CF-4AEA-A114-F3DFF5BD0AF7");
            // Attrib for BlockType: Transaction Matching:Show Family Role
            RockMigrationHelper.DeleteAttribute("12C9276C-3926-47E4-9E0E-2BC2D6EB8F82");
            // Attrib for BlockType: Transaction Matching:Person Record Status
            RockMigrationHelper.DeleteAttribute("67450E73-CF6D-4E09-809D-5566E46CA2AA");
            // Attrib for BlockType: Transaction Matching:Default Person Connection Status
            RockMigrationHelper.DeleteAttribute("F25A121C-9034-4506-B5CF-D0F633334A1A");
            // Attrib for BlockType: Welcome:iPad Camera Barcode Configuration
            RockMigrationHelper.DeleteAttribute("B7BD8FDE-479E-4450-8DE7-F53A6C37F19F");
            // Attrib for BlockType: Prayer Request Details:Workflow
            RockMigrationHelper.DeleteAttribute("56711BF1-19A0-43B2-BC7B-422A62EA9AAC");
            // Attrib for BlockType: Prayer Request Details:Completion Xaml
            RockMigrationHelper.DeleteAttribute("227A4870-BE30-4A34-8F4B-EA8CEFED57A7");
            // Attrib for BlockType: Prayer Request Details:Completion Action
            RockMigrationHelper.DeleteAttribute("EB14BC94-EE3D-4933-A981-CE9DD7EBBA88");
            // Attrib for BlockType: Prayer Request Details:Enable Person Matching
            RockMigrationHelper.DeleteAttribute("7D7D6009-35C8-440B-9056-90E8AD4045AF");
            // Attrib for BlockType: Prayer Request Details:Require Last Name
            RockMigrationHelper.DeleteAttribute("8A550979-1F89-4ABD-A391-236ABEE49527");
            // Attrib for BlockType: Prayer Request Details:Require Campus
            RockMigrationHelper.DeleteAttribute("BAA123F7-476F-43EF-A948-3E82D90FC94F");
            // Attrib for BlockType: Prayer Request Details:Show Campus
            RockMigrationHelper.DeleteAttribute("471640E1-D37E-4E47-BACE-059550A2D931");
            // Attrib for BlockType: Prayer Request Details:Character Limit
            RockMigrationHelper.DeleteAttribute("33EA26F2-ACFA-49BB-A0F3-C4F1CF87CED3");
            // Attrib for BlockType: Prayer Request Details:Default To Public
            RockMigrationHelper.DeleteAttribute("61FD7017-03E9-4C83-B456-BAA8A7ACED8D");
            // Attrib for BlockType: Prayer Request Details:Show Public Display Flag
            RockMigrationHelper.DeleteAttribute("1D9FEFE9-68CE-400E-B3E2-BEEDC4DF02CA");
            // Attrib for BlockType: Prayer Request Details:Show Urgent Flag
            RockMigrationHelper.DeleteAttribute("5CF14B20-0108-44BD-B12D-874F45C62D98");
            // Attrib for BlockType: Prayer Request Details:Expires After (Days)
            RockMigrationHelper.DeleteAttribute("98F4F327-7428-418A-9587-44E381CA2068");
            // Attrib for BlockType: Prayer Request Details:Enable Auto Approve
            RockMigrationHelper.DeleteAttribute("1209FAEE-910E-4C67-8FEF-7381369C7A68");
            // Attrib for BlockType: Prayer Request Details:Default Category
            RockMigrationHelper.DeleteAttribute("CD6992F5-C1F0-4C50-A62F-2C8C735C902E");
            // Attrib for BlockType: Prayer Request Details:Parent Category
            RockMigrationHelper.DeleteAttribute("4B711B43-53FB-4140-9E87-DD658A123D40");
            // Attrib for BlockType: Prayer Request Details:Show Category
            RockMigrationHelper.DeleteAttribute("5645E84B-C0F1-49FC-B57A-8875923AA9F7");
            // Attrib for BlockType: Group Member List:Template
            RockMigrationHelper.DeleteAttribute("782BB4F1-8FB0-48A6-B429-742D21316048");
            // Attrib for BlockType: Group Member List:Title Template
            RockMigrationHelper.DeleteAttribute("E226DC34-CAC1-43E3-98F0-6AE0AC0C1741");
            // Attrib for BlockType: Group Member List:Group Member Detail Page
            RockMigrationHelper.DeleteAttribute("635BB1B8-BD29-4FF4-8A88-178623539BC8");
            // Attrib for BlockType: Group Member Edit:Member Detail Page
            RockMigrationHelper.DeleteAttribute("FFDAD6E5-10A7-48EF-BFB3-2AA192FF0BEC");
            // Attrib for BlockType: Group Member Edit:Attribute Category
            RockMigrationHelper.DeleteAttribute("9B998F41-CE10-4945-98A9-1A690F5ABCCB");
            // Attrib for BlockType: Group Member Edit:Allow Note Edit
            RockMigrationHelper.DeleteAttribute("681BB863-9788-4F8E-8A39-5AEBA96FB431");
            // Attrib for BlockType: Group Member Edit:Allow Member Status Change
            RockMigrationHelper.DeleteAttribute("D9BD93EB-9F3A-4B0C-A899-5640CE1508B5");
            // Attrib for BlockType: Group Member Edit:Allow Role Change
            RockMigrationHelper.DeleteAttribute("7E617C42-99C9-46F0-936B-4B87EE06256D");
            // Attrib for BlockType: Group Edit:Group Detail Page
            RockMigrationHelper.DeleteAttribute("D15A36FA-1ACA-4F24-9E50-BD8DBE50EB34");
            // Attrib for BlockType: Group Edit:Attribute Category
            RockMigrationHelper.DeleteAttribute("C7417D6B-76BA-488D-B2E3-755429BA3D17");
            // Attrib for BlockType: Group Edit:Enable Public Status Edit
            RockMigrationHelper.DeleteAttribute("CD8C603B-6DB2-475B-9762-9170C757A5A7");
            // Attrib for BlockType: Group Edit:Show Public Status
            RockMigrationHelper.DeleteAttribute("9848DCE8-7CE5-4D74-88BA-64CB3C1C72E5");
            // Attrib for BlockType: Group Edit:Enable Active Status Edit
            RockMigrationHelper.DeleteAttribute("93F3D753-4E75-41C0-AD07-9D5CCAAB83EB");
            // Attrib for BlockType: Group Edit:Show Active Status
            RockMigrationHelper.DeleteAttribute("A45ADA2C-F0C2-4375-9261-92BD171096F8");
            // Attrib for BlockType: Group Edit:Enable Group Capacity Edit
            RockMigrationHelper.DeleteAttribute("23D0366C-4AE6-4E6D-AD05-0534CAB52818");
            // Attrib for BlockType: Group Edit:Show Group Capacity
            RockMigrationHelper.DeleteAttribute("BD232927-D71A-4EE9-BD27-D7A8FD75BD29");
            // Attrib for BlockType: Group Edit:Enable Campus Edit
            RockMigrationHelper.DeleteAttribute("4BF2E1E4-C6D2-4700-AC0F-D1196B871C89");
            // Attrib for BlockType: Group Edit:Show Campus
            RockMigrationHelper.DeleteAttribute("C42B8997-FC26-4BCE-93AC-AEE17616A89E");
            // Attrib for BlockType: Group Edit:Enable Description Edit
            RockMigrationHelper.DeleteAttribute("DF7532D9-7073-4F42-9397-71BF8EF549CB");
            // Attrib for BlockType: Group Edit:Show Description
            RockMigrationHelper.DeleteAttribute("D5A629F5-176E-471C-BED1-C55134668447");
            // Attrib for BlockType: Group Edit:Enable Group Name Edit
            RockMigrationHelper.DeleteAttribute("0D7FB0C7-37FC-427D-A4E7-D29876A00B0B");
            // Attrib for BlockType: Group Edit:Show Group Name
            RockMigrationHelper.DeleteAttribute("6C3DD530-9468-4A38-A323-DE164C052AE6");
            // Attrib for BlockType: Group Attendance Entry:Allow Any Date Selection
            RockMigrationHelper.DeleteAttribute("AEB89129-2406-4D8A-B2CA-2507898D71C1");
            // Attrib for BlockType: Group Attendance Entry:Show Save Button
            RockMigrationHelper.DeleteAttribute("344B0582-03D1-47CA-96BA-F8A9BF6984B6");
            // Attrib for BlockType: Group Attendance Entry:Save Redirect Page
            RockMigrationHelper.DeleteAttribute("83FA5BD1-181C-4CD9-9F22-D153DE319E67");
            // Attrib for BlockType: Group Attendance Entry:Number of Days Back to Allow
            RockMigrationHelper.DeleteAttribute("13B29580-9A0D-4916-B6FC-E7B515663948");
            // Attrib for BlockType: Group Attendance Entry:Number of Days Forward to Allow
            RockMigrationHelper.DeleteAttribute("DA1918FA-871C-49CB-911C-9745F9F8F342");
            // Attrib for BlockType: Calendar View:Show Filter
            RockMigrationHelper.DeleteAttribute("A4436F1C-B907-428C-B5EA-8B2AE97DF5E4");
            // Attrib for BlockType: Calendar View:Event Summary
            RockMigrationHelper.DeleteAttribute("347C6DE1-A972-47CD-9C67-A1ABA1D237A6");
            // Attrib for BlockType: Calendar View:Audience Filter
            RockMigrationHelper.DeleteAttribute("1399A25B-AB59-4913-BE04-D1051753436C");
            // Attrib for BlockType: Calendar View:Detail Page
            RockMigrationHelper.DeleteAttribute("A221789F-674E-4D91-BE7F-7509B7172251");
            // Attrib for BlockType: Calendar View:Calendar
            RockMigrationHelper.DeleteAttribute("A39797B3-61FE-41C8-A717-847BE778237D");
            RockMigrationHelper.DeleteBlockType("56A97F0B-9E8D-4A67-BDA4-49B4E506B264"); // Prayer Request Details
            RockMigrationHelper.DeleteBlockType("D695B899-F1A9-4EB1-B4D8-86D875E72D4E"); // Group Member List
            RockMigrationHelper.DeleteBlockType("50EBC88C-62B5-45D8-B5FD-33F338F1C973"); // Group Member Edit
            RockMigrationHelper.DeleteBlockType("C1F40F48-19CC-40A2-AF96-C5BFB0AB437C"); // Group Edit
            RockMigrationHelper.DeleteBlockType("5C7FF9AD-582F-4D2F-AA7E-6EF401B66D68"); // Group Attendance Entry
            RockMigrationHelper.DeleteBlockType("AE8A6786-B002-4E91-AF99-2970C09E54D6"); // Calendar View
        }
    
        /// <summary>
        /// NA: Add PreHtml Warning on Legacy SystemEmailList and SystemEmailDetails.
        /// </summary>
        private void AddWarningToLegacySystemEmailBlocks()
        {
            Sql( @"
            UPDATE [Block]
                SET [PreHtml] = CONCAT( '<div class=''alert alert-warning''>All of Rock''s core ''system emails'' should now managed using the System Communications page. This page is still available for you to manage any plugin created system emails. Once plugin developers have updated their code to use System Communications (and after you update the plugin) you will also manage any of those system emails using the System Communications page.</div>', [PreHtml] )
            WHERE 
                [BlockTypeId] IN (
                       SELECT [Id] FROM [BlockType] WHERE [Path] = '~/Blocks/Communication/SystemEmailDetail.ascx' OR [Path] = '~/Blocks/Communication/SystemEmailList.ascx'
                )
            AND [PreHtml] NOT LIKE '% should now managed using the System Communications page.%'" );
        }
    
        /// <summary>
        /// ED: Remove the assessment block attribute allow retakes.
        /// </summary>
        private void RemoveAssessmentBlockAttributeAllowRetakes()
        {
            RockMigrationHelper.DeleteBlockAttribute( "3A07B385-A3C1-4C0B-80F9-F50432503C0A" ); // Attrib for BlockType: Motivators Assessment:Allow Retakes
            RockMigrationHelper.DeleteBlockAttribute( "A0905767-79C9-4567-BA76-A3FFEE71E0B3" ); // Attrib for BlockType: EQ Assessment:Allow Retakes
            RockMigrationHelper.DeleteBlockAttribute( "E3965E46-603C-40E5-AB28-1B53E44561DE" ); // Attrib for BlockType: Conflict Profile:Allow Retakes
            RockMigrationHelper.DeleteBlockAttribute( "9DC69746-7AD4-4BC9-B6EE-27E24774CE5B" ); // Attrib for BlockType: Spirtual Gifts:Allow Retakes
        }

        /// <summary>
        /// SK: Add Structured Content Tool Defined Type for structure content editor
        /// </summary>
        private void StructuredContentToolDefinedTypeUp()
        {
            RockMigrationHelper.AddDefinedType( "Tools", "Structured Content Editor Tools", "", "E43AD92C-4DD4-4D78-9852-FCFAEFDF52CA", @"" );
            RockMigrationHelper.UpdateDefinedValue( "E43AD92C-4DD4-4D78-9852-FCFAEFDF52CA", "Default", "{     header: {     class: Header,     inlineToolbar: ['link'],     config: {         placeholder: 'Header'     },     shortcut: 'CMD+SHIFT+H'     },     image: {     class: ImageTool,     inlineToolbar: ['link'],     },     list: {     class: List,     inlineToolbar: true,     shortcut: 'CMD+SHIFT+L'     },     checklist: {     class: Checklist,     inlineToolbar: true,     },     quote: {     class: Quote,     inlineToolbar: true,     config: {         quotePlaceholder: 'Enter a quote',         captionPlaceholder: 'Quote\\'s author',     },     shortcut: 'CMD+SHIFT+O'     },     warning: Warning,     marker: {     class:  Marker,     shortcut: 'CMD+SHIFT+M'     },     code: {     class:  CodeTool,     shortcut: 'CMD+SHIFT+C'     },     delimiter: Delimiter,     inlineCode: {     class: InlineCode,     shortcut: 'CMD+SHIFT+C'     },     linkTool: LinkTool,     embed: Embed,     table: {     class: Table,     inlineToolbar: true,     shortcut: 'CMD+ALT+T'     } }", "31C63FB9-1365-4EEF-851D-8AB9A188A06C", false );
        }
    
        /// <summary>
        /// SK: Add Structured Content Tool Defined Type for structure content editor
        /// </summary>
        private void StructuredContentToolDefinedTypeDown()
        {
            RockMigrationHelper.DeleteDefinedValue( "31C63FB9-1365-4EEF-851D-8AB9A188A06C" ); // Default
            RockMigrationHelper.DeleteDefinedType( "E43AD92C-4DD4-4D78-9852-FCFAEFDF52CA" );
        }

        /// <summary>
        /// Devices the type attribute.
        /// </summary>
        private void DeviceTypeAttribute()
        {
             RockMigrationHelper.AddDefinedTypeAttribute( Rock.SystemGuid.DefinedType.DEVICE_TYPE,
                Rock.SystemGuid.FieldType.BOOLEAN,
                "Supports Camera",
                "core_SupportsCameras",
                "Used to determine if these devices have the capability of a camera.",
                0,
                string.Empty,
                Rock.SystemGuid.Attribute.DEFINED_VALUE_DEVICE_TYPE_SUPPORTS_CAMERAS );

            RockMigrationHelper.UpdateDefinedValueAttributeValue( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK,
                Rock.SystemGuid.Attribute.DEFINED_VALUE_DEVICE_TYPE_SUPPORTS_CAMERAS,
                true.ToString() );

            RockMigrationHelper.UpdateDefinedValueAttributeValue( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_GIVING_KIOSK,
                Rock.SystemGuid.Attribute.DEFINED_VALUE_DEVICE_TYPE_SUPPORTS_CAMERAS,
                true.ToString() );

        }
    }
}
