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
    public partial class Rollup_0218 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            RapidAttendanceEntryUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();
            RapidAttendanceEntryDown();
        }

        /// <summary>
        /// Script generated "Up" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            RockMigrationHelper.UpdateBlockType("Registration Instance - Discount List","Displays the discounts related to an event registration instance.","~/Blocks/Event/RegistrationInstanceDiscountList.ascx","Event","846CAEB3-81AB-4519-8332-E953A59149F5");
            RockMigrationHelper.UpdateBlockType("Registration Instance - Fee List","Displays the fees related to an event registration instance.","~/Blocks/Event/RegistrationInstanceFeeList.ascx","Event","9B22C8F7-3917-4958-9D68-38F1AEA49A6B");
            RockMigrationHelper.UpdateBlockType("Registration Group Placement","Block to manage group placement for Registration Instances.","~/Blocks/Event/RegistrationInstanceGroupPlacement.ascx","Event","9C889FEA-0C77-4FF1-8E6B-C9710B8C3A06");
            RockMigrationHelper.UpdateBlockType("Registration Instance - Linkage List","Displays the linkages associated with an event registration instance.","~/Blocks/Event/RegistrationInstanceLinkageList.ascx","Event","3D2B3ADA-97C7-4B1C-A9F1-11BC5A684B94");
            RockMigrationHelper.UpdateBlockType("Registration Instance - Navigation","Provides the navigation for the tabs navigation section of the Registration Instance Page/Layout","~/Blocks/Event/RegistrationInstanceNavigation.ascx","Event","9EB0D9D3-69EA-41E1-BAC6-094778E62B69");
            RockMigrationHelper.UpdateBlockType("Registration Instance - Payment List","Displays the payments related to an event registration instance.","~/Blocks/Event/RegistrationInstancePaymentList.ascx","Event","845A4CA0-4447-4D8B-A6C8-C80F1EA39838");
            RockMigrationHelper.UpdateBlockType("Registration Instance - Registrant List","Displays the list of Registrants related to a Registration Instance.","~/Blocks/Event/RegistrationInstanceRegistrantList.ascx","Event","440B61C1-A5B2-4988-B38F-C6C8317DE3DA");
            RockMigrationHelper.UpdateBlockType("Registration Instance - Registration List","Displays the list of Registrations related to a Registration Instance.","~/Blocks/Event/RegistrationInstanceRegistrationList.ascx","Event","2D392830-7DD2-4911-BDE3-7D659A20AE50");
            RockMigrationHelper.UpdateBlockType("Registration Instance - Wait List","Block for editing the wait list associated with an event registration instance.","~/Blocks/Event/RegistrationInstanceWaitList.ascx","Event","577BAC39-511E-43D6-9A73-E33AE6EFD65B");
            // Attrib for BlockType: Calendar View:Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "63972831-6E21-4502-8AC4-0E0FC1124C21", "EC0D9528-1A22-404E-A776-566404987363", "Calendar", "Calendar", "Calendar", @"The calendar to pull events from", 0, @"", "A63B0E1A-BA81-4108-A1BB-C9A6E77CEDD5" );
            // Attrib for BlockType: Calendar View:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "63972831-6E21-4502-8AC4-0E0FC1124C21", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to push onto the navigation stack when viewing details of an event.", 1, @"", "45C87830-96A4-460F-B3F1-680C44C1B789" );
            // Attrib for BlockType: Calendar View:Audience Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "63972831-6E21-4502-8AC4-0E0FC1124C21", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Audience Filter", "AudienceFilter", "Audience Filter", @"Determines which audiences should be displayed in the filter.", 2, @"", "88E84CDE-64E0-4BAE-8314-58F3E508494C" );
            // Attrib for BlockType: Calendar View:Event Summary
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "63972831-6E21-4502-8AC4-0E0FC1124C21", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Event Summary", "EventSummary", "Event Summary", @"The XAML to use when rendering the event summaries below the calendar.", 3, @"<Frame HasShadow=""false"" StyleClass=""calendar-event"">
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
", "35B42217-F7EF-4576-B111-213F2E22B706" );
            // Attrib for BlockType: Calendar View:Show Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "63972831-6E21-4502-8AC4-0E0FC1124C21", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Filter", "ShowFilter", "Show Filter", @"If enabled then the user will be able to apply custom filtering.", 4, @"True", "768DF96C-BFF1-4FA7-A1DA-151B6AB741EE" );
            // Attrib for BlockType: Group Attendance Entry:Number of Days Forward to Allow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1CD2902D-8E23-4580-AA5B-EF167778FD2E", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Days Forward to Allow", "NumberOfDaysForwardToAllow", "Number of Days Forward to Allow", @"", 0, @"0", "A6FB657F-3D72-470C-883A-ED8EBC0FA6B3" );
            // Attrib for BlockType: Group Attendance Entry:Number of Days Back to Allow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1CD2902D-8E23-4580-AA5B-EF167778FD2E", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Days Back to Allow", "NumberOfDaysBackToAllow", "Number of Days Back to Allow", @"", 1, @"30", "DAA2525F-8041-4C72-B18A-BDFC2B8408BB" );
            // Attrib for BlockType: Group Attendance Entry:Save Redirect Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1CD2902D-8E23-4580-AA5B-EF167778FD2E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Save Redirect Page", "SaveRedirectPage", "Save Redirect Page", @"If set, redirect user to this page on save. If not set, page is popped off the navigation stack.", 2, @"", "9495A743-D257-434E-A603-5507E6553CE8" );
            // Attrib for BlockType: Group Attendance Entry:Show Save Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1CD2902D-8E23-4580-AA5B-EF167778FD2E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Save Button", "ShowSaveButton", "Show Save Button", @"If enabled a save button will be shown (recommended for large groups), otherwise no save button will be displayed and a save will be triggered with each selection (recommended for smaller groups).", 3, @"False", "54A51A5F-C742-4494-BFED-5B26EE951FDB" );
            // Attrib for BlockType: Group Attendance Entry:Allow Any Date Selection
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1CD2902D-8E23-4580-AA5B-EF167778FD2E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Any Date Selection", "AllowAnyDateSelection", "Allow Any Date Selection", @"If enabled a date picker will be shown, otherwise a dropdown with only the valid dates will be shown.", 4, @"False", "5CFC480B-99EC-426F-B045-D76824F9D2C5" );
            // Attrib for BlockType: Group Edit:Show Group Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7F16D6A0-7B88-46E6-BD68-BEC02BB66F39", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Name", "ShowGroupName", "Show Group Name", @"", 0, @"True", "D879AF2B-253C-4F52-A3B2-DB81FB67F3DB" );
            // Attrib for BlockType: Group Edit:Enable Group Name Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7F16D6A0-7B88-46E6-BD68-BEC02BB66F39", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Group Name Edit", "EnableGroupNameEdit", "Enable Group Name Edit", @"", 1, @"True", "9540D132-D07B-4FBE-9AEB-756F5BA51C75" );
            // Attrib for BlockType: Group Edit:Show Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7F16D6A0-7B88-46E6-BD68-BEC02BB66F39", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Description", "ShowDescription", "Show Description", @"", 2, @"True", "E3A5C1BA-9BB1-4FD0-B38C-528FAEE423FA" );
            // Attrib for BlockType: Group Edit:Enable Description Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7F16D6A0-7B88-46E6-BD68-BEC02BB66F39", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Description Edit", "EnableDescriptionEdit", "Enable Description Edit", @"", 3, @"True", "971E3DED-3577-4F82-A909-199F7ADAFBE3" );
            // Attrib for BlockType: Group Edit:Show Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7F16D6A0-7B88-46E6-BD68-BEC02BB66F39", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus", "ShowCampus", "Show Campus", @"", 4, @"True", "7D65D669-E31E-425A-A140-BEA5930E12E5" );
            // Attrib for BlockType: Group Edit:Enable Campus Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7F16D6A0-7B88-46E6-BD68-BEC02BB66F39", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Campus Edit", "EnableCampusEdit", "Enable Campus Edit", @"", 5, @"True", "2398687C-5F07-4D71-AA66-997B23DD044F" );
            // Attrib for BlockType: Group Edit:Show Group Capacity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7F16D6A0-7B88-46E6-BD68-BEC02BB66F39", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Capacity", "ShowGroupCapacity", "Show Group Capacity", @"", 6, @"True", "1DA06F36-B291-40D4-8921-5F2214D4DFE1" );
            // Attrib for BlockType: Group Edit:Enable Group Capacity Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7F16D6A0-7B88-46E6-BD68-BEC02BB66F39", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Group Capacity Edit", "EnableGroupCapacityEdit", "Enable Group Capacity Edit", @"", 7, @"True", "3F8E850A-69F9-4032-81CE-AB19094C44FC" );
            // Attrib for BlockType: Group Edit:Show Active Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7F16D6A0-7B88-46E6-BD68-BEC02BB66F39", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Active Status", "ShowActiveStatus", "Show Active Status", @"", 8, @"True", "A26227EB-A8D2-449C-AC4A-F71464F5CB5B" );
            // Attrib for BlockType: Group Edit:Enable Active Status Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7F16D6A0-7B88-46E6-BD68-BEC02BB66F39", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Active Status Edit", "EnableActiveStatusEdit", "Enable Active Status Edit", @"", 9, @"True", "2178BA0B-6169-44F4-B422-E4563062F702" );
            // Attrib for BlockType: Group Edit:Show Public Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7F16D6A0-7B88-46E6-BD68-BEC02BB66F39", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Public Status", "ShowPublicStatus", "Show Public Status", @"", 10, @"True", "A2D9858D-A70D-445F-BADF-89D2E7C62F1A" );
            // Attrib for BlockType: Group Edit:Enable Public Status Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7F16D6A0-7B88-46E6-BD68-BEC02BB66F39", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Public Status Edit", "EnablePublicStatusEdit", "Enable Public Status Edit", @"", 11, @"True", "91825532-977E-45F7-80EB-4DD0A237195C" );
            // Attrib for BlockType: Group Edit:Attribute Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7F16D6A0-7B88-46E6-BD68-BEC02BB66F39", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Attribute Category", "AttributeCategory", "Attribute Category", @"Category of attributes to show and allow editing on.", 12, @"", "0C39F560-13FC-4A0F-B6F6-F976D1AF61EB" );
            // Attrib for BlockType: Group Edit:Group Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7F16D6A0-7B88-46E6-BD68-BEC02BB66F39", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "Group Detail Page", @"The group detail page to return to, if not set then the edit page is popped off the navigation stack.", 13, @"", "154474F3-0C93-44D6-82A6-CC236DAAE3B3" );
            // Attrib for BlockType: Group Member Edit:Allow Role Change
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AADAB61-706F-4A42-8206-AB0AE08B3A01", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Role Change", "AllowRoleChange", "Allow Role Change", @"", 0, @"True", "B54B4A61-5658-4691-8B1D-B2BCDBBAE75C" );
            // Attrib for BlockType: Group Member Edit:Allow Member Status Change
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AADAB61-706F-4A42-8206-AB0AE08B3A01", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Member Status Change", "AllowMemberStatusChange", "Allow Member Status Change", @"", 1, @"True", "67DD755C-72E2-4852-9D27-E51A07AD4E04" );
            // Attrib for BlockType: Group Member Edit:Allow Note Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AADAB61-706F-4A42-8206-AB0AE08B3A01", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Note Edit", "AllowNoteEdit", "Allow Note Edit", @"", 2, @"True", "BD6780E1-ED9E-4BCD-9A8F-A6F556C824AB" );
            // Attrib for BlockType: Group Member Edit:Attribute Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AADAB61-706F-4A42-8206-AB0AE08B3A01", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Attribute Category", "AttributeCategory", "Attribute Category", @"Category of attributes to show and allow editing on.", 3, @"", "D8D61A66-04E6-4E2A-8931-B5F5EA5D5F5E" );
            // Attrib for BlockType: Group Member Edit:Member Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AADAB61-706F-4A42-8206-AB0AE08B3A01", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Member Detail Page", "MemberDetailsPage", "Member Detail Page", @"The group member page to return to, if not set then the edit page is popped off the navigation stack.", 4, @"", "30AFF148-24C5-4D29-83D4-56B77331770F" );
            // Attrib for BlockType: Group Member List:Group Member Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5EA2262-805A-4C96-BF3D-6B478353F480", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Member Detail Page", "GroupMemberDetailPage", "Group Member Detail Page", @"The page that will display the group member details when selecting a member.", 0, @"", "BE294148-4A3D-4535-9BB5-1EF40557766C" );
            // Attrib for BlockType: Group Member List:Title Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5EA2262-805A-4C96-BF3D-6B478353F480", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title Template", "TitleTemplate", "Title Template", @"The value to use when rendering the title text. <span class='tip tip-lava'></span>", 1, @"{{ Group.Name }} Group Roster", "19881923-BB71-4533-9124-1CD25815C58A" );
            // Attrib for BlockType: Group Member List:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5EA2262-805A-4C96-BF3D-6B478353F480", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 2, @"", "62133349-E538-42DD-9039-F345A1A42BA1" );
            // Attrib for BlockType: Group Member List:Additional Fields
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5EA2262-805A-4C96-BF3D-6B478353F480", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Additional Fields", "AdditionalFields", "Additional Fields", @"", 3, @"", "92688FCA-6956-4E75-9E83-2DB229F01E55" );
            // Attrib for BlockType: Group Member View:Group Member Edit Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "015B6EC3-5567-4046-87E4-9D629A884B9B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Member Edit Page", "GroupMemberEditPage", "Group Member Edit Page", @"The page that will allow editing of a group member.", 0, @"", "67CB4A0D-50BE-4F25-9E96-4C1EFEAC8C54" );
            // Attrib for BlockType: Group Member View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "015B6EC3-5567-4046-87E4-9D629A884B9B", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 1, @"", "0D797698-B793-4B0D-A231-19E4433E9D64" );
            // Attrib for BlockType: Group View:Group Edit Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "02E7BF8D-8B63-4F0C-A6CF-3D40CE67F0CB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Edit Page", "GroupEditPage", "Group Edit Page", @"The page that will allow editing of the group.", 0, @"", "1351DA44-2D82-4093-8269-3211AA8B55D8" );
            // Attrib for BlockType: Group View:Show Leader List
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "02E7BF8D-8B63-4F0C-A6CF-3D40CE67F0CB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Leader List", "ShowLeaderList", "Show Leader List", @"Specifies if the leader list should be shown, this value is made available to the Template as ShowLeaderList.", 1, @"True", "5E673EEB-EB2B-4142-942D-10E12E95452C" );
            // Attrib for BlockType: Group View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "02E7BF8D-8B63-4F0C-A6CF-3D40CE67F0CB", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 2, @"", "26BC6187-7C45-4580-9676-2981940EC5A3" );
            // Attrib for BlockType: Prayer Request Details:Show Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C332A3FA-7944-4DFC-8476-BADA1EF61558", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Category", "EnableCategory", "Show Category", @"If disabled, then the user will not be able to select a category and the default category will be used exclusively.", 0, @"True", "2FB0BD32-30C2-4FB9-86C6-55A90512A57E" );
            // Attrib for BlockType: Prayer Request Details:Parent Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C332A3FA-7944-4DFC-8476-BADA1EF61558", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Parent Category", "ParentCategory", "Parent Category", @"A top level category. This controls which categories the person can choose from when entering their prayer request.", 1, @"", "B9236947-A6F3-4D00-925D-2097B34A0EF6" );
            // Attrib for BlockType: Prayer Request Details:Default Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C332A3FA-7944-4DFC-8476-BADA1EF61558", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Default Category", "DefaultCategory", "Default Category", @"The default category to use for all new prayer requests.", 2, @"", "33F8648B-ABEC-4A42-9DA9-213B5C764647" );
            // Attrib for BlockType: Prayer Request Details:Enable Auto Approve
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C332A3FA-7944-4DFC-8476-BADA1EF61558", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Auto Approve", "EnableAutoApprove", "Enable Auto Approve", @"If enabled, prayer requests are automatically approved; otherwise they must be approved by an admin before they can be seen by the prayer team.", 0, @"True", "718AC302-8CB1-4140-AC10-6AF56B966763" );
            // Attrib for BlockType: Prayer Request Details:Expires After (Days)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C332A3FA-7944-4DFC-8476-BADA1EF61558", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Expires After (Days)", "ExpiresAfterDays", "Expires After (Days)", @"Number of days until the request will expire (only applies when auto-approved is enabled).", 1, @"14", "F7D6C6E9-2DDC-4472-AA64-D574CB05F571" );
            // Attrib for BlockType: Prayer Request Details:Show Urgent Flag
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C332A3FA-7944-4DFC-8476-BADA1EF61558", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Urgent Flag", "EnableUrgentFlag", "Show Urgent Flag", @"If enabled, requestors will be able to flag prayer requests as urgent.", 2, @"False", "43E05756-3C52-435B-8CD2-A9ACB1B09995" );
            // Attrib for BlockType: Prayer Request Details:Show Public Display Flag
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C332A3FA-7944-4DFC-8476-BADA1EF61558", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Public Display Flag", "EnablePublicDisplayFlag", "Show Public Display Flag", @"If enabled, requestors will be able set whether or not they want their request displayed on the public website.", 3, @"False", "A5628013-4389-42DE-A474-4B1F0A755D4B" );
            // Attrib for BlockType: Prayer Request Details:Default To Public
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C332A3FA-7944-4DFC-8476-BADA1EF61558", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default To Public", "DefaultToPublic", "Default To Public", @"If enabled, all prayers will be set to public by default.", 4, @"False", "D7AE64F1-37C3-4AC1-816E-9A62594A492D" );
            // Attrib for BlockType: Prayer Request Details:Character Limit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C332A3FA-7944-4DFC-8476-BADA1EF61558", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Character Limit", "CharacterLimit", "Character Limit", @"If set to something other than 0, this will limit the number of characters allowed when entering a new prayer request.", 5, @"250", "876EB0F1-9ACA-4DED-8C01-FC71C877BCA9" );
            // Attrib for BlockType: Prayer Request Details:Show Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C332A3FA-7944-4DFC-8476-BADA1EF61558", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus", "EnableCampus", "Show Campus", @"Should the campus field be displayed? If there is only one active campus then the campus field will not show.", 6, @"True", "B46FD954-F9F6-41E6-B962-50E0F7159A8E" );
            // Attrib for BlockType: Prayer Request Details:Require Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C332A3FA-7944-4DFC-8476-BADA1EF61558", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Campus", "RequireCampus", "Require Campus", @"Require that a campus be selected. The campus will not be displayed if there is only one available campus, in which case if this is set to true then the single campus is automatically used.", 7, @"False", "3F1A326A-56F7-49F5-A325-7908BBABFF21" );
            // Attrib for BlockType: Prayer Request Details:Require Last Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C332A3FA-7944-4DFC-8476-BADA1EF61558", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Last Name", "RequireLastName", "Require Last Name", @"Require that a last name be entered. First name is always required.", 8, @"True", "932BAFE8-3242-4400-BFEC-B15520051868" );
            // Attrib for BlockType: Prayer Request Details:Enable Person Matching
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C332A3FA-7944-4DFC-8476-BADA1EF61558", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Person Matching", "EnablePersonMatching", "Enable Person Matching", @"If enabled, the request will be linked to an existing person if a match can be made between the requester and an existing person.", 9, @"False", "3B325FF4-BE09-4402-BDAB-6F64C2392CBF" );
            // Attrib for BlockType: Prayer Request Details:Completion Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C332A3FA-7944-4DFC-8476-BADA1EF61558", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Completion Action", "CompletionAction", "Completion Action", @"What action to perform after saving the prayer request.", 0, @"0", "50DC0766-8154-4C45-82C3-77F69085FE3E" );
            // Attrib for BlockType: Prayer Request Details:Completion Xaml
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C332A3FA-7944-4DFC-8476-BADA1EF61558", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Completion Xaml", "CompletionXaml", "Completion Xaml", @"The XAML markup that will be used if the. <span class='tip tip-lava'></span>", 1, @"<Rock:NotificationBox NotificationType=""Success"">
    Thank you for allowing us to pray for you.
</Rock:NotificationBox>", "F78BDBEE-3D8D-4C5E-B2A7-062B4AAF7C83" );
            // Attrib for BlockType: Prayer Request Details:Workflow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C332A3FA-7944-4DFC-8476-BADA1EF61558", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "Workflow", @"An optional workflow to start when prayer request is created. The PrayerRequest will be set as the workflow 'Entity' attribute when processing is started.", 2, @"", "257FA2AC-8603-45E6-9927-1F87582A36EB" );
            // Attrib for BlockType: Registration Group Placement:Group Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9C889FEA-0C77-4FF1-8E6B-C9710B8C3A06", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "Group Detail Page", @"", 0, @"4E237286-B715-4109-A578-C1445EC02707", "117762F8-CC99-4230-9499-189A6B15D232" );
            // Attrib for BlockType: Registration Group Placement:Group Member Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9C889FEA-0C77-4FF1-8E6B-C9710B8C3A06", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Member Detail Page", "GroupMemberDetailPage", "Group Member Detail Page", @"", 1, @"3905C63F-4D57-40F0-9721-C60A2F681911", "1F4D5168-90CD-4A76-8E43-8C6756015148" );
            // Attrib for BlockType: Registration Instance - Linkage List:Content Item Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3D2B3ADA-97C7-4B1C-A9F1-11BC5A684B94", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Content Item Page", "ContentItemDetailPage", "Content Item Page", @"The page for viewing details about a content channel item", 4, @"D18E837C-9E65-4A38-8647-DFF04A595D97", "F197ECB1-2B3D-4B1A-8C01-FA15E085E92A" );
            // Attrib for BlockType: Registration Instance - Linkage List:Calendar Item Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3D2B3ADA-97C7-4B1C-A9F1-11BC5A684B94", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Calendar Item Page", "CalendarItemDetailPage", "Calendar Item Page", @"The page to view calendar item details", 3, @"7FB33834-F40A-4221-8849-BB8C06903B04", "713A1439-0A6A-49A3-B71B-4FCD0CBC2231" );
            // Attrib for BlockType: Registration Instance - Linkage List:Linkage Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3D2B3ADA-97C7-4B1C-A9F1-11BC5A684B94", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Linkage Page", "LinkagePage", "Linkage Page", @"The page for viewing details about a registration linkage", 1, @"DE4B12F0-C3E6-451C-9E35-7E9E66A01F4E", "AA8F5410-080A-4537-9500-E3E787A9BA05" );
            // Attrib for BlockType: Registration Instance - Linkage List:Group Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3D2B3ADA-97C7-4B1C-A9F1-11BC5A684B94", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "Group Detail Page", @"The page for viewing details about a group", 2, @"4E237286-B715-4109-A578-C1445EC02707", "126BCB07-8B59-4A3A-BA47-AE2869551E3D" );
            // Attrib for BlockType: Registration Instance - Navigation:Group Placement Tool Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9EB0D9D3-69EA-41E1-BAC6-094778E62B69", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Placement Tool Page", "GroupPlacementToolPage", "Group Placement Tool Page", @"The Page that shows Group Placements for the selected placement type", 1, @"0CD950D7-033D-42B1-A53E-108F311DC5BF", "16E4D1F4-2961-4DA7-8CF3-106F91ED94B9" );
            // Attrib for BlockType: Registration Instance - Navigation:Wait List Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9EB0D9D3-69EA-41E1-BAC6-094778E62B69", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Wait List Page", "WaitListPage", "Wait List Page", @"The Page that shows the Wait List", 0, @"E17883C2-6442-4AE5-B561-2C783F7F89C9", "2C0DB494-1ABD-47BC-95B8-52AB5BB57555" );
            // Attrib for BlockType: Registration Instance - Payment List:Transaction Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "845A4CA0-4447-4D8B-A6C8-C80F1EA39838", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Transaction Detail Page", "TransactionDetailPage", "Transaction Detail Page", @"The page for viewing details about a payment", 1, @"B67E38CB-2EF1-43EA-863A-37DAA1C7340F", "3C2FD00E-3186-4F6A-B535-D8675FF6249A" );
            // Attrib for BlockType: Registration Instance - Payment List:Registration Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "845A4CA0-4447-4D8B-A6C8-C80F1EA39838", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "Registration Page", @"The page for editing registration and registrant information", 2, @"FC81099A-2F98-4EBA-AC5A-8300B2FE46C4", "74E046AC-CEDD-44F0-8672-E59648264D68" );
            // Attrib for BlockType: Registration Instance - Registrant List:Group Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "440B61C1-A5B2-4988-B38F-C6C8317DE3DA", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "Group Detail Page", @"The page for viewing details about a group", 3, @"4E237286-B715-4109-A578-C1445EC02707", "16C264F7-F550-4E0B-86A4-40845538CE85" );
            // Attrib for BlockType: Registration Instance - Registrant List:Registration Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "440B61C1-A5B2-4988-B38F-C6C8317DE3DA", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "Registration Page", @"The page for editing registration and registrant information", 1, @"FC81099A-2F98-4EBA-AC5A-8300B2FE46C4", "DAF8AD85-19F1-4059-B286-9A88E35791C1" );
            // Attrib for BlockType: Registration Instance - Registrant List:Group Placement Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "440B61C1-A5B2-4988-B38F-C6C8317DE3DA", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Placement Page", "GroupPlacementPage", "Group Placement Page", @"The page for managing the registrant's group placements", 2, @"0CD950D7-033D-42B1-A53E-108F311DC5BF", "4C3801B0-F518-403D-B9EC-B07D1B44BD92" );
            // Attrib for BlockType: Registration Instance - Registration List:Display Discount Codes
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2D392830-7DD2-4911-BDE3-7D659A20AE50", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Discount Codes", "DisplayDiscountCodes", "Display Discount Codes", @"Display the discount code used with a payment", 2, @"False", "192F21A9-4037-4F1B-993E-151D83A14A60" );
            // Attrib for BlockType: Registration Instance - Registration List:Registration Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2D392830-7DD2-4911-BDE3-7D659A20AE50", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "Registration Page", @"The page for editing registration and registrant information", 1, @"FC81099A-2F98-4EBA-AC5A-8300B2FE46C4", "E3377739-72CA-4F3B-AF2E-5973F830C60D" );
            // Attrib for BlockType: Registration Instance - Wait List:Wait List Process Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "577BAC39-511E-43D6-9A73-E33AE6EFD65B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Wait List Process Page", "WaitListProcessingPage", "Wait List Process Page", @"The page for moving a person from the wait list to a full registrant.", 1, @"4BF84D3F-DE7B-4F8B-814A-1E728E69C105", "9AC2AA61-5340-482A-90C3-33F9D3529A60" );
            // Attrib for BlockType: Registration Instance - Wait List:Registration Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "577BAC39-511E-43D6-9A73-E33AE6EFD65B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "Registration Page", @"The page for editing registration and registrant information", 2, @"FC81099A-2F98-4EBA-AC5A-8300B2FE46C4", "A1CAE870-011C-45C2-B125-C85975349B3A" );

        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Registration Instance - Wait List:Registration Page
            RockMigrationHelper.DeleteAttribute("A1CAE870-011C-45C2-B125-C85975349B3A");
            // Attrib for BlockType: Registration Instance - Wait List:Wait List Process Page
            RockMigrationHelper.DeleteAttribute("9AC2AA61-5340-482A-90C3-33F9D3529A60");
            // Attrib for BlockType: Registration Instance - Registration List:Registration Page
            RockMigrationHelper.DeleteAttribute("E3377739-72CA-4F3B-AF2E-5973F830C60D");
            // Attrib for BlockType: Registration Instance - Registration List:Display Discount Codes
            RockMigrationHelper.DeleteAttribute("192F21A9-4037-4F1B-993E-151D83A14A60");
            // Attrib for BlockType: Registration Instance - Registrant List:Group Placement Page
            RockMigrationHelper.DeleteAttribute("4C3801B0-F518-403D-B9EC-B07D1B44BD92");
            // Attrib for BlockType: Registration Instance - Registrant List:Registration Page
            RockMigrationHelper.DeleteAttribute("DAF8AD85-19F1-4059-B286-9A88E35791C1");
            // Attrib for BlockType: Registration Instance - Registrant List:Group Detail Page
            RockMigrationHelper.DeleteAttribute("16C264F7-F550-4E0B-86A4-40845538CE85");
            // Attrib for BlockType: Registration Instance - Payment List:Registration Page
            RockMigrationHelper.DeleteAttribute("74E046AC-CEDD-44F0-8672-E59648264D68");
            // Attrib for BlockType: Registration Instance - Payment List:Transaction Detail Page
            RockMigrationHelper.DeleteAttribute("3C2FD00E-3186-4F6A-B535-D8675FF6249A");
            // Attrib for BlockType: Registration Instance - Navigation:Wait List Page
            RockMigrationHelper.DeleteAttribute("2C0DB494-1ABD-47BC-95B8-52AB5BB57555");
            // Attrib for BlockType: Registration Instance - Navigation:Group Placement Tool Page
            RockMigrationHelper.DeleteAttribute("16E4D1F4-2961-4DA7-8CF3-106F91ED94B9");
            // Attrib for BlockType: Registration Instance - Linkage List:Group Detail Page
            RockMigrationHelper.DeleteAttribute("126BCB07-8B59-4A3A-BA47-AE2869551E3D");
            // Attrib for BlockType: Registration Instance - Linkage List:Linkage Page
            RockMigrationHelper.DeleteAttribute("AA8F5410-080A-4537-9500-E3E787A9BA05");
            // Attrib for BlockType: Registration Instance - Linkage List:Calendar Item Page
            RockMigrationHelper.DeleteAttribute("713A1439-0A6A-49A3-B71B-4FCD0CBC2231");
            // Attrib for BlockType: Registration Instance - Linkage List:Content Item Page
            RockMigrationHelper.DeleteAttribute("F197ECB1-2B3D-4B1A-8C01-FA15E085E92A");
            // Attrib for BlockType: Registration Group Placement:Group Member Detail Page
            RockMigrationHelper.DeleteAttribute("1F4D5168-90CD-4A76-8E43-8C6756015148");
            // Attrib for BlockType: Registration Group Placement:Group Detail Page
            RockMigrationHelper.DeleteAttribute("117762F8-CC99-4230-9499-189A6B15D232");
            // Attrib for BlockType: Prayer Request Details:Workflow
            RockMigrationHelper.DeleteAttribute("257FA2AC-8603-45E6-9927-1F87582A36EB");
            // Attrib for BlockType: Prayer Request Details:Completion Xaml
            RockMigrationHelper.DeleteAttribute("F78BDBEE-3D8D-4C5E-B2A7-062B4AAF7C83");
            // Attrib for BlockType: Prayer Request Details:Completion Action
            RockMigrationHelper.DeleteAttribute("50DC0766-8154-4C45-82C3-77F69085FE3E");
            // Attrib for BlockType: Prayer Request Details:Enable Person Matching
            RockMigrationHelper.DeleteAttribute("3B325FF4-BE09-4402-BDAB-6F64C2392CBF");
            // Attrib for BlockType: Prayer Request Details:Require Last Name
            RockMigrationHelper.DeleteAttribute("932BAFE8-3242-4400-BFEC-B15520051868");
            // Attrib for BlockType: Prayer Request Details:Require Campus
            RockMigrationHelper.DeleteAttribute("3F1A326A-56F7-49F5-A325-7908BBABFF21");
            // Attrib for BlockType: Prayer Request Details:Show Campus
            RockMigrationHelper.DeleteAttribute("B46FD954-F9F6-41E6-B962-50E0F7159A8E");
            // Attrib for BlockType: Prayer Request Details:Character Limit
            RockMigrationHelper.DeleteAttribute("876EB0F1-9ACA-4DED-8C01-FC71C877BCA9");
            // Attrib for BlockType: Prayer Request Details:Default To Public
            RockMigrationHelper.DeleteAttribute("D7AE64F1-37C3-4AC1-816E-9A62594A492D");
            // Attrib for BlockType: Prayer Request Details:Show Public Display Flag
            RockMigrationHelper.DeleteAttribute("A5628013-4389-42DE-A474-4B1F0A755D4B");
            // Attrib for BlockType: Prayer Request Details:Show Urgent Flag
            RockMigrationHelper.DeleteAttribute("43E05756-3C52-435B-8CD2-A9ACB1B09995");
            // Attrib for BlockType: Prayer Request Details:Expires After (Days)
            RockMigrationHelper.DeleteAttribute("F7D6C6E9-2DDC-4472-AA64-D574CB05F571");
            // Attrib for BlockType: Prayer Request Details:Enable Auto Approve
            RockMigrationHelper.DeleteAttribute("718AC302-8CB1-4140-AC10-6AF56B966763");
            // Attrib for BlockType: Prayer Request Details:Default Category
            RockMigrationHelper.DeleteAttribute("33F8648B-ABEC-4A42-9DA9-213B5C764647");
            // Attrib for BlockType: Prayer Request Details:Parent Category
            RockMigrationHelper.DeleteAttribute("B9236947-A6F3-4D00-925D-2097B34A0EF6");
            // Attrib for BlockType: Prayer Request Details:Show Category
            RockMigrationHelper.DeleteAttribute("2FB0BD32-30C2-4FB9-86C6-55A90512A57E");
            // Attrib for BlockType: Group View:Template
            RockMigrationHelper.DeleteAttribute("26BC6187-7C45-4580-9676-2981940EC5A3");
            // Attrib for BlockType: Group View:Show Leader List
            RockMigrationHelper.DeleteAttribute("5E673EEB-EB2B-4142-942D-10E12E95452C");
            // Attrib for BlockType: Group View:Group Edit Page
            RockMigrationHelper.DeleteAttribute("1351DA44-2D82-4093-8269-3211AA8B55D8");
            // Attrib for BlockType: Group Member View:Template
            RockMigrationHelper.DeleteAttribute("0D797698-B793-4B0D-A231-19E4433E9D64");
            // Attrib for BlockType: Group Member View:Group Member Edit Page
            RockMigrationHelper.DeleteAttribute("67CB4A0D-50BE-4F25-9E96-4C1EFEAC8C54");
            // Attrib for BlockType: Group Member List:Additional Fields
            RockMigrationHelper.DeleteAttribute("92688FCA-6956-4E75-9E83-2DB229F01E55");
            // Attrib for BlockType: Group Member List:Template
            RockMigrationHelper.DeleteAttribute("62133349-E538-42DD-9039-F345A1A42BA1");
            // Attrib for BlockType: Group Member List:Title Template
            RockMigrationHelper.DeleteAttribute("19881923-BB71-4533-9124-1CD25815C58A");
            // Attrib for BlockType: Group Member List:Group Member Detail Page
            RockMigrationHelper.DeleteAttribute("BE294148-4A3D-4535-9BB5-1EF40557766C");
            // Attrib for BlockType: Group Member Edit:Member Detail Page
            RockMigrationHelper.DeleteAttribute("30AFF148-24C5-4D29-83D4-56B77331770F");
            // Attrib for BlockType: Group Member Edit:Attribute Category
            RockMigrationHelper.DeleteAttribute("D8D61A66-04E6-4E2A-8931-B5F5EA5D5F5E");
            // Attrib for BlockType: Group Member Edit:Allow Note Edit
            RockMigrationHelper.DeleteAttribute("BD6780E1-ED9E-4BCD-9A8F-A6F556C824AB");
            // Attrib for BlockType: Group Member Edit:Allow Member Status Change
            RockMigrationHelper.DeleteAttribute("67DD755C-72E2-4852-9D27-E51A07AD4E04");
            // Attrib for BlockType: Group Member Edit:Allow Role Change
            RockMigrationHelper.DeleteAttribute("B54B4A61-5658-4691-8B1D-B2BCDBBAE75C");
            // Attrib for BlockType: Group Edit:Group Detail Page
            RockMigrationHelper.DeleteAttribute("154474F3-0C93-44D6-82A6-CC236DAAE3B3");
            // Attrib for BlockType: Group Edit:Attribute Category
            RockMigrationHelper.DeleteAttribute("0C39F560-13FC-4A0F-B6F6-F976D1AF61EB");
            // Attrib for BlockType: Group Edit:Enable Public Status Edit
            RockMigrationHelper.DeleteAttribute("91825532-977E-45F7-80EB-4DD0A237195C");
            // Attrib for BlockType: Group Edit:Show Public Status
            RockMigrationHelper.DeleteAttribute("A2D9858D-A70D-445F-BADF-89D2E7C62F1A");
            // Attrib for BlockType: Group Edit:Enable Active Status Edit
            RockMigrationHelper.DeleteAttribute("2178BA0B-6169-44F4-B422-E4563062F702");
            // Attrib for BlockType: Group Edit:Show Active Status
            RockMigrationHelper.DeleteAttribute("A26227EB-A8D2-449C-AC4A-F71464F5CB5B");
            // Attrib for BlockType: Group Edit:Enable Group Capacity Edit
            RockMigrationHelper.DeleteAttribute("3F8E850A-69F9-4032-81CE-AB19094C44FC");
            // Attrib for BlockType: Group Edit:Show Group Capacity
            RockMigrationHelper.DeleteAttribute("1DA06F36-B291-40D4-8921-5F2214D4DFE1");
            // Attrib for BlockType: Group Edit:Enable Campus Edit
            RockMigrationHelper.DeleteAttribute("2398687C-5F07-4D71-AA66-997B23DD044F");
            // Attrib for BlockType: Group Edit:Show Campus
            RockMigrationHelper.DeleteAttribute("7D65D669-E31E-425A-A140-BEA5930E12E5");
            // Attrib for BlockType: Group Edit:Enable Description Edit
            RockMigrationHelper.DeleteAttribute("971E3DED-3577-4F82-A909-199F7ADAFBE3");
            // Attrib for BlockType: Group Edit:Show Description
            RockMigrationHelper.DeleteAttribute("E3A5C1BA-9BB1-4FD0-B38C-528FAEE423FA");
            // Attrib for BlockType: Group Edit:Enable Group Name Edit
            RockMigrationHelper.DeleteAttribute("9540D132-D07B-4FBE-9AEB-756F5BA51C75");
            // Attrib for BlockType: Group Edit:Show Group Name
            RockMigrationHelper.DeleteAttribute("D879AF2B-253C-4F52-A3B2-DB81FB67F3DB");
            // Attrib for BlockType: Group Attendance Entry:Allow Any Date Selection
            RockMigrationHelper.DeleteAttribute("5CFC480B-99EC-426F-B045-D76824F9D2C5");
            // Attrib for BlockType: Group Attendance Entry:Show Save Button
            RockMigrationHelper.DeleteAttribute("54A51A5F-C742-4494-BFED-5B26EE951FDB");
            // Attrib for BlockType: Group Attendance Entry:Save Redirect Page
            RockMigrationHelper.DeleteAttribute("9495A743-D257-434E-A603-5507E6553CE8");
            // Attrib for BlockType: Group Attendance Entry:Number of Days Back to Allow
            RockMigrationHelper.DeleteAttribute("DAA2525F-8041-4C72-B18A-BDFC2B8408BB");
            // Attrib for BlockType: Group Attendance Entry:Number of Days Forward to Allow
            RockMigrationHelper.DeleteAttribute("A6FB657F-3D72-470C-883A-ED8EBC0FA6B3");
            // Attrib for BlockType: Calendar View:Show Filter
            RockMigrationHelper.DeleteAttribute("768DF96C-BFF1-4FA7-A1DA-151B6AB741EE");
            // Attrib for BlockType: Calendar View:Event Summary
            RockMigrationHelper.DeleteAttribute("35B42217-F7EF-4576-B111-213F2E22B706");
            // Attrib for BlockType: Calendar View:Audience Filter
            RockMigrationHelper.DeleteAttribute("88E84CDE-64E0-4BAE-8314-58F3E508494C");
            // Attrib for BlockType: Calendar View:Detail Page
            RockMigrationHelper.DeleteAttribute("45C87830-96A4-460F-B3F1-680C44C1B789");
            // Attrib for BlockType: Calendar View:Calendar
            RockMigrationHelper.DeleteAttribute("A63B0E1A-BA81-4108-A1BB-C9A6E77CEDD5");
            RockMigrationHelper.DeleteBlockType("577BAC39-511E-43D6-9A73-E33AE6EFD65B"); // Registration Instance - Wait List
            RockMigrationHelper.DeleteBlockType("2D392830-7DD2-4911-BDE3-7D659A20AE50"); // Registration Instance - Registration List
            RockMigrationHelper.DeleteBlockType("440B61C1-A5B2-4988-B38F-C6C8317DE3DA"); // Registration Instance - Registrant List
            RockMigrationHelper.DeleteBlockType("845A4CA0-4447-4D8B-A6C8-C80F1EA39838"); // Registration Instance - Payment List
            RockMigrationHelper.DeleteBlockType("9EB0D9D3-69EA-41E1-BAC6-094778E62B69"); // Registration Instance - Navigation
            RockMigrationHelper.DeleteBlockType("3D2B3ADA-97C7-4B1C-A9F1-11BC5A684B94"); // Registration Instance - Linkage List
            RockMigrationHelper.DeleteBlockType("9C889FEA-0C77-4FF1-8E6B-C9710B8C3A06"); // Registration Group Placement
            RockMigrationHelper.DeleteBlockType("9B22C8F7-3917-4958-9D68-38F1AEA49A6B"); // Registration Instance - Fee List
            RockMigrationHelper.DeleteBlockType("846CAEB3-81AB-4519-8332-E953A59149F5"); // Registration Instance - Discount List
            RockMigrationHelper.DeleteBlockType("C332A3FA-7944-4DFC-8476-BADA1EF61558"); // Prayer Request Details
            RockMigrationHelper.DeleteBlockType("02E7BF8D-8B63-4F0C-A6CF-3D40CE67F0CB"); // Group View
            RockMigrationHelper.DeleteBlockType("015B6EC3-5567-4046-87E4-9D629A884B9B"); // Group Member View
            RockMigrationHelper.DeleteBlockType("A5EA2262-805A-4C96-BF3D-6B478353F480"); // Group Member List
            RockMigrationHelper.DeleteBlockType("5AADAB61-706F-4A42-8206-AB0AE08B3A01"); // Group Member Edit
            RockMigrationHelper.DeleteBlockType("7F16D6A0-7B88-46E6-BD68-BEC02BB66F39"); // Group Edit
            RockMigrationHelper.DeleteBlockType("1CD2902D-8E23-4580-AA5B-EF167778FD2E"); // Group Attendance Entry
            RockMigrationHelper.DeleteBlockType("63972831-6E21-4502-8AC4-0E0FC1124C21"); // Calendar View
        }

        /// <summary>
        /// SK: Updated Rapid Attendance Entry block setting (Up)
        /// </summary>
        private void RapidAttendanceEntryUp()
        {
            RockMigrationHelper.AddPage( true, "78B79290-3234-4D8C-96D3-1901901BA1DD", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Attendance List", "", "D56CD916-C3C7-4277-BEBA-0FA4C21A758D", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Attendance List", "Block for displaying the attendance history of a person or a group.", "~/Blocks/CheckIn/AttendanceList.ascx", "Checkin", "678ED4B6-D76F-4D43-B069-659E352C9BD8" );
            RockMigrationHelper.AddBlock( true, "D56CD916-C3C7-4277-BEBA-0FA4C21A758D".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"678ED4B6-D76F-4D43-B069-659E352C9BD8".AsGuid(), "Attendance List","Main",@"",@"",0,"9590C855-903E-4CA4-9296-FB9D6F02FA41");

            // Attrib for BlockType: Rapid Attendance Entry:Add Family Page              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Family Page", "AddFamilyPage", "Add Family Page", @"Page used for adding new families.", 0, @"", "CB4EDB5A-2919-48C3-BB9F-434C1024AF92" );
            // Attrib for BlockType: Rapid Attendance Entry:Attendance List Page             
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C2ED1FA-218B-4ACC-B661-A2618F310CD4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Attendance List Page", "AttendanceListPage", "Attendance List Page", @"Page used to show the attendance list.", 1, @"", "2AA8EFA0-2B21-4A36-9326-5F71C621FBD7" );  
            // Attrib Value for Block:Rapid Attendance Entry, Attribute:Add Family Page Page: Rapid Attendance Entry, Site: Rock RMS           
            RockMigrationHelper.AddBlockAttributeValue("24560306-8535-4119-BA4A-CBC172C3832C","CB4EDB5A-2919-48C3-BB9F-434C1024AF92",@"6a11a13d-05ab-4982-a4c2-67a8b1950c74");
            // Attrib Value for Block:Rapid Attendance Entry, Attribute:Attendance List Page Page: Rapid Attendance Entry, Site: Rock RMS   
            RockMigrationHelper.AddBlockAttributeValue("24560306-8535-4119-BA4A-CBC172C3832C","2AA8EFA0-2B21-4A36-9326-5F71C621FBD7",@"d56cd916-c3c7-4277-beba-0fa4c21a758d");  
        }
    
        /// <summary>
        /// SK: Updated Rapid Attendance Entry block setting (Down)
        /// </summary>
        private void RapidAttendanceEntryDown()
        {
            // Attrib for BlockType: Rapid Attendance Entry:Attendance List Page
            RockMigrationHelper.DeleteAttribute("2AA8EFA0-2B21-4A36-9326-5F71C621FBD7");
            // Attrib for BlockType: Rapid Attendance Entry:Add Family Page
            RockMigrationHelper.DeleteAttribute("CB4EDB5A-2919-48C3-BB9F-434C1024AF92");

            RockMigrationHelper.DeleteBlock("9590C855-903E-4CA4-9296-FB9D6F02FA41");
            RockMigrationHelper.DeleteBlockType( "678ED4B6-D76F-4D43-B069-659E352C9BD8" ); // Attendance List
            RockMigrationHelper.DeletePage( "D56CD916-C3C7-4277-BEBA-0FA4C21A758D" ); //  Page: Attendance List, Layout: Full Width, Site: Rock RMS
        }
    }
}
