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
    public partial class Rollup_1217 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            ContributionStatementLavaBlock();
            AudienceTypeDefinedTypeAddHighlightColorAttribute();
            UpdateAssessmentStatus();
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
            // Attrib for BlockType: Calendar View:Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2C32B005-8370-4580-9646-B4C127C91F9D", "EC0D9528-1A22-404E-A776-566404987363", "Calendar", "Calendar", "Calendar", @"The calendar to pull events from", 0, @"", "33DB3DDD-2E78-47DA-B35F-F3C90A07638E" );
            // Attrib for BlockType: Calendar View:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2C32B005-8370-4580-9646-B4C127C91F9D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to push onto the navigation stack when viewing details of an event.", 1, @"", "79D8D7E4-FD7B-4DB6-B38E-C6A84DB2C007" );
            // Attrib for BlockType: Calendar View:Audience Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2C32B005-8370-4580-9646-B4C127C91F9D", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Audience Filter", "AudienceFilter", "Audience Filter", @"Determines which audiences should be displayed in the filter.", 2, @"", "314CCDD8-3209-44BA-A708-48712EA70E05" );
            // Attrib for BlockType: Calendar View:Event Summary
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2C32B005-8370-4580-9646-B4C127C91F9D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Event Summary", "EventSummary", "Event Summary", @"The XAML to use when rendering the event summaries below the calendar.", 3, @"<Frame HasShadow=""false"" StyleClass=""calendar-event"">
    <StackLayout Spacing=""0"">
        <Label StyleClass=""calendar-event-title"" Text=""{Binding Name}"" />
        {% if Item.EndDateTime == null %}
            <Label StyleClass=""calendar-event-text"" Text=""{{ Item.StartDateTime | Date:'h:mm tt' }}"" />
        {% else %}
            <Label StyleClass=""calendar-event-text"" Text=""{{ Item.StartDateTime | Date:'h:mm tt' }} - {{ Item.EndDateTime | Date:'h:mm tt' }}"" />
        {% endif %}
        <Grid>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width=""*"" />
                <ColumnDefinition Width=""Auto"" />
            </Grid.ColumnDefinitions>

            <Label Grid.Row=""0"" Grid.Column=""0"" StyleClass=""calendar-event-audience"" Text=""{{ Item.AudienceNames | Join:', ' }}"" />
            <Label Grid.Row=""0"" Grid.Column=""1"" StyleClass=""calendar-event-campus"" Text=""{{ Item.Campus }}"" HorizontalTextAlignment=""End"" />
        </Grid>
    </StackLayout>
</Frame>
", "1D9C2A57-132B-475C-945F-1C3A58D9994C" );
            // Attrib for BlockType: Calendar View:Show Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2C32B005-8370-4580-9646-B4C127C91F9D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Filter", "ShowFilter", "Show Filter", @"If enabled then the user will be able to apply custom filtering.", 4, @"True", "DDA6E0DB-04ED-4B68-9E64-E16B640CBC6F" );
            // Attrib for BlockType: Group Edit:Show Group Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29C5F0F8-D137-4BBD-A1BC-964FE42232FC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Name", "ShowGroupName", "Show Group Name", @"", 0, @"True", "2093314D-C932-42E1-A31E-F62A507BB1D3" );
            // Attrib for BlockType: Group Edit:Enable Group Name Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29C5F0F8-D137-4BBD-A1BC-964FE42232FC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Group Name Edit", "EnableGroupNameEdit", "Enable Group Name Edit", @"", 1, @"True", "BB78C538-F105-4ECA-8AC9-34FDD08B90D0" );
            // Attrib for BlockType: Group Edit:Show Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29C5F0F8-D137-4BBD-A1BC-964FE42232FC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Description", "ShowDescription", "Show Description", @"", 2, @"True", "854743D8-B84A-4B0B-80AD-136F4FCB6582" );
            // Attrib for BlockType: Group Edit:Enable Description Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29C5F0F8-D137-4BBD-A1BC-964FE42232FC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Description Edit", "EnableDescriptionEdit", "Enable Description Edit", @"", 3, @"True", "47B28689-252D-450F-BE21-786818702592" );
            // Attrib for BlockType: Group Edit:Show Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29C5F0F8-D137-4BBD-A1BC-964FE42232FC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus", "ShowCampus", "Show Campus", @"", 4, @"True", "2F309C5E-B818-4E4C-9D63-E05C35D15F7A" );
            // Attrib for BlockType: Group Edit:Enable Campus Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29C5F0F8-D137-4BBD-A1BC-964FE42232FC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Campus Edit", "EnableCampusEdit", "Enable Campus Edit", @"", 5, @"True", "4B8DAA04-4EBB-433B-96DE-42B74E2794DE" );
            // Attrib for BlockType: Group Edit:Show Group Capacity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29C5F0F8-D137-4BBD-A1BC-964FE42232FC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Capacity", "ShowGroupCapacity", "Show Group Capacity", @"", 6, @"True", "06B69385-6BA3-4961-BAA0-7B0CC111CEFD" );
            // Attrib for BlockType: Group Edit:Enable Group Capacity Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29C5F0F8-D137-4BBD-A1BC-964FE42232FC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Group Capacity Edit", "EnableGroupCapacityEdit", "Enable Group Capacity Edit", @"", 7, @"True", "A1315E00-F13C-4ECA-A420-F1ED581DFBD3" );
            // Attrib for BlockType: Group Edit:Show Active Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29C5F0F8-D137-4BBD-A1BC-964FE42232FC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Active Status", "ShowActiveStatus", "Show Active Status", @"", 8, @"True", "5D06CA00-7B95-43D0-9372-35C3260A1889" );
            // Attrib for BlockType: Group Edit:Enable Active Status Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29C5F0F8-D137-4BBD-A1BC-964FE42232FC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Active Status Edit", "EnableActiveStatusEdit", "Enable Active Status Edit", @"", 9, @"True", "CBF7DB3A-4F4D-4811-A82F-EB82B8EBEF1A" );
            // Attrib for BlockType: Group Edit:Show Public Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29C5F0F8-D137-4BBD-A1BC-964FE42232FC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Public Status", "ShowPublicStatus", "Show Public Status", @"", 10, @"True", "9E3384AE-9F19-415B-9D9D-0E7890909F13" );
            // Attrib for BlockType: Group Edit:Enable Public Status Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29C5F0F8-D137-4BBD-A1BC-964FE42232FC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Public Status Edit", "EnablePublicStatusEdit", "Enable Public Status Edit", @"", 11, @"True", "36308A1A-7166-4F4E-96C2-858849EFFDC5" );
            // Attrib for BlockType: Group Edit:Attribute Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29C5F0F8-D137-4BBD-A1BC-964FE42232FC", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Attribute Category", "AttributeCategory", "Attribute Category", @"Category of attributes to show and allow editing on.", 12, @"", "5D683C41-C76A-4BF0-B9BC-2A53470A6DDA" );
            // Attrib for BlockType: Group Edit:Group Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "29C5F0F8-D137-4BBD-A1BC-964FE42232FC", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "Group Detail Page", @"The group detail page to return to, if not set then the edit page is popped off the navigation stack.", 13, @"", "1AAB6481-437B-4986-BA8C-3D3BDF4F5BB0" );
            // Attrib for BlockType: Group Member Edit:Allow Role Change
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BFFF482C-8C10-4350-A5C3-BC99F73BB28F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Role Change", "AllowRoleChange", "Allow Role Change", @"", 0, @"True", "0D24AE9B-9B7C-4C6B-A925-1B0AE2224A60" );
            // Attrib for BlockType: Group Member Edit:Allow Member Status Change
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BFFF482C-8C10-4350-A5C3-BC99F73BB28F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Member Status Change", "AllowMemberStatusChange", "Allow Member Status Change", @"", 1, @"True", "F5F0F68A-8447-4CB3-8331-2CD3F6B85D69" );
            // Attrib for BlockType: Group Member Edit:Allow Note Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BFFF482C-8C10-4350-A5C3-BC99F73BB28F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Note Edit", "AllowNoteEdit", "Allow Note Edit", @"", 2, @"True", "A7903B08-04B4-44D7-AE24-877A6ACC0EB4" );
            // Attrib for BlockType: Group Member Edit:Attribute Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BFFF482C-8C10-4350-A5C3-BC99F73BB28F", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Attribute Category", "AttributeCategory", "Attribute Category", @"Category of attributes to show and allow editing on.", 3, @"", "9EFED24C-2BE1-4317-9214-B8F10B7684B5" );
            // Attrib for BlockType: Group Member Edit:Member Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BFFF482C-8C10-4350-A5C3-BC99F73BB28F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Member Detail Page", "MemberDetailsPage", "Member Detail Page", @"The group member page to return to, if not set then the edit page is popped off the navigation stack.", 4, @"", "A9FE43BA-1E2D-4455-8CE2-4D5FA4C8678C" );
            // Attrib for BlockType: Prayer Request Details:Show Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DF70DFE3-3FE5-46B1-87A5-250DD95E8D81", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Category", "EnableCategory", "Show Category", @"If disabled, then the user will not be able to select a category and the default category will be used exclusively.", 0, @"True", "EF811ADD-F9CD-4D17-B70B-DCB4FCC4518E" );
            // Attrib for BlockType: Prayer Request Details:Parent Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DF70DFE3-3FE5-46B1-87A5-250DD95E8D81", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Parent Category", "ParentCategory", "Parent Category", @"A top level category. This controls which categories the person can choose from when entering their prayer request.", 1, @"", "1C3324DA-D9AE-42AA-97F0-7F4B1F21ED54" );
            // Attrib for BlockType: Prayer Request Details:Default Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DF70DFE3-3FE5-46B1-87A5-250DD95E8D81", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Default Category", "DefaultCategory", "Default Category", @"The default category to use for all new prayer requests.", 2, @"", "C2BCCD33-2E35-4FD7-A580-46C3D9403248" );
            // Attrib for BlockType: Prayer Request Details:Enable Auto Approve
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DF70DFE3-3FE5-46B1-87A5-250DD95E8D81", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Auto Approve", "EnableAutoApprove", "Enable Auto Approve", @"If enabled, prayer requests are automatically approved; otherwise they must be approved by an admin before they can be seen by the prayer team.", 0, @"True", "9A0CEB8A-9EBA-4BB5-A114-41F2E6209CFC" );
            // Attrib for BlockType: Prayer Request Details:Expires After (Days)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DF70DFE3-3FE5-46B1-87A5-250DD95E8D81", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Expires After (Days)", "ExpiresAfterDays", "Expires After (Days)", @"Number of days until the request will expire (only applies when auto-approved is enabled).", 1, @"14", "0EDDE512-23AF-4F5B-93E4-0116CBE18547" );
            // Attrib for BlockType: Prayer Request Details:Show Urgent Flag
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DF70DFE3-3FE5-46B1-87A5-250DD95E8D81", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Urgent Flag", "EnableUrgentFlag", "Show Urgent Flag", @"If enabled, requestors will be able to flag prayer requests as urgent.", 2, @"False", "E69944FA-BDAB-47C4-B676-50EEE4F0124E" );
            // Attrib for BlockType: Prayer Request Details:Show Public Display Flag
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DF70DFE3-3FE5-46B1-87A5-250DD95E8D81", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Public Display Flag", "EnablePublicDisplayFlag", "Show Public Display Flag", @"If enabled, requestors will be able set whether or not they want their request displayed on the public website.", 3, @"False", "B5F985E6-0FDC-4649-A55A-84EEFE4BA9C3" );
            // Attrib for BlockType: Prayer Request Details:Default To Public
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DF70DFE3-3FE5-46B1-87A5-250DD95E8D81", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default To Public", "DefaultToPublic", "Default To Public", @"If enabled, all prayers will be set to public by default.", 4, @"False", "5460FE2A-A4CB-4FA4-BD26-53F0573AAEAA" );
            // Attrib for BlockType: Prayer Request Details:Character Limit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DF70DFE3-3FE5-46B1-87A5-250DD95E8D81", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Character Limit", "CharacterLimit", "Character Limit", @"If set to something other than 0, this will limit the number of characters allowed when entering a new prayer request.", 5, @"250", "E8D851BC-2500-4537-A554-CD66DF391CF5" );
            // Attrib for BlockType: Prayer Request Details:Show Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DF70DFE3-3FE5-46B1-87A5-250DD95E8D81", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus", "EnableCampus", "Show Campus", @"Should the campus field be displayed? If there is only one active campus then the campus field will not show.", 6, @"True", "F3E80FB4-284A-4163-9480-3B20A6FB672A" );
            // Attrib for BlockType: Prayer Request Details:Require Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DF70DFE3-3FE5-46B1-87A5-250DD95E8D81", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Campus", "RequireCampus", "Require Campus", @"Require that a campus be selected. The campus will not be displayed if there is only one available campus, in which case if this is set to true then the single campus is automatically used.", 7, @"False", "6098219F-7E43-4785-A64E-6DBA04DED29F" );
            // Attrib for BlockType: Prayer Request Details:Require Last Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DF70DFE3-3FE5-46B1-87A5-250DD95E8D81", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Last Name", "RequireLastName", "Require Last Name", @"Require that a last name be entered. First name is always required.", 8, @"True", "7D2FB605-82E9-42EC-A5E0-6AB1DDC14095" );
            // Attrib for BlockType: Prayer Request Details:Enable Person Matching
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DF70DFE3-3FE5-46B1-87A5-250DD95E8D81", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Person Matching", "EnablePersonMatching", "Enable Person Matching", @"If enabled, the request will be linked to an existing person if a match can be made between the requester and an existing person.", 9, @"False", "FACB5609-FFFE-42D1-BEED-C28219F4BF10" );
            // Attrib for BlockType: Prayer Request Details:Completion Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DF70DFE3-3FE5-46B1-87A5-250DD95E8D81", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Completion Action", "CompletionAction", "Completion Action", @"What action to perform after saving the prayer request.", 0, @"0", "B0EEE797-8B28-48AF-86DE-D9749D4E0B7E" );
            // Attrib for BlockType: Prayer Request Details:Completion Xaml
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DF70DFE3-3FE5-46B1-87A5-250DD95E8D81", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Completion Xaml", "CompletionXaml", "Completion Xaml", @"The XAML markup that will be used if the. <span class='tip tip-lava'></span>", 1, @"<Rock:NotificationBox NotificationType=""Success"">
    Thank you for allowing us to pray for you.
</Rock:NotificationBox>", "20CD9FDF-0E96-4057-84D3-ADF6F7D775BB" );
            // Attrib for BlockType: Prayer Request Details:Workflow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DF70DFE3-3FE5-46B1-87A5-250DD95E8D81", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "Workflow", @"An optional workflow to start when prayer request is created. The PrayerRequest will be set as the workflow 'Entity' attribute when processing is started.", 2, @"", "8E8F6D9D-73A8-4B06-AC65-1AE06F3CAD2F" );

        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Prayer Request Details:Workflow
            RockMigrationHelper.DeleteAttribute("8E8F6D9D-73A8-4B06-AC65-1AE06F3CAD2F");
            // Attrib for BlockType: Prayer Request Details:Completion Xaml
            RockMigrationHelper.DeleteAttribute("20CD9FDF-0E96-4057-84D3-ADF6F7D775BB");
            // Attrib for BlockType: Prayer Request Details:Completion Action
            RockMigrationHelper.DeleteAttribute("B0EEE797-8B28-48AF-86DE-D9749D4E0B7E");
            // Attrib for BlockType: Prayer Request Details:Enable Person Matching
            RockMigrationHelper.DeleteAttribute("FACB5609-FFFE-42D1-BEED-C28219F4BF10");
            // Attrib for BlockType: Prayer Request Details:Require Last Name
            RockMigrationHelper.DeleteAttribute("7D2FB605-82E9-42EC-A5E0-6AB1DDC14095");
            // Attrib for BlockType: Prayer Request Details:Require Campus
            RockMigrationHelper.DeleteAttribute("6098219F-7E43-4785-A64E-6DBA04DED29F");
            // Attrib for BlockType: Prayer Request Details:Show Campus
            RockMigrationHelper.DeleteAttribute("F3E80FB4-284A-4163-9480-3B20A6FB672A");
            // Attrib for BlockType: Prayer Request Details:Character Limit
            RockMigrationHelper.DeleteAttribute("E8D851BC-2500-4537-A554-CD66DF391CF5");
            // Attrib for BlockType: Prayer Request Details:Default To Public
            RockMigrationHelper.DeleteAttribute("5460FE2A-A4CB-4FA4-BD26-53F0573AAEAA");
            // Attrib for BlockType: Prayer Request Details:Show Public Display Flag
            RockMigrationHelper.DeleteAttribute("B5F985E6-0FDC-4649-A55A-84EEFE4BA9C3");
            // Attrib for BlockType: Prayer Request Details:Show Urgent Flag
            RockMigrationHelper.DeleteAttribute("E69944FA-BDAB-47C4-B676-50EEE4F0124E");
            // Attrib for BlockType: Prayer Request Details:Expires After (Days)
            RockMigrationHelper.DeleteAttribute("0EDDE512-23AF-4F5B-93E4-0116CBE18547");
            // Attrib for BlockType: Prayer Request Details:Enable Auto Approve
            RockMigrationHelper.DeleteAttribute("9A0CEB8A-9EBA-4BB5-A114-41F2E6209CFC");
            // Attrib for BlockType: Prayer Request Details:Default Category
            RockMigrationHelper.DeleteAttribute("C2BCCD33-2E35-4FD7-A580-46C3D9403248");
            // Attrib for BlockType: Prayer Request Details:Parent Category
            RockMigrationHelper.DeleteAttribute("1C3324DA-D9AE-42AA-97F0-7F4B1F21ED54");
            // Attrib for BlockType: Prayer Request Details:Show Category
            RockMigrationHelper.DeleteAttribute("EF811ADD-F9CD-4D17-B70B-DCB4FCC4518E");
            // Attrib for BlockType: Group Member Edit:Member Detail Page
            RockMigrationHelper.DeleteAttribute("A9FE43BA-1E2D-4455-8CE2-4D5FA4C8678C");
            // Attrib for BlockType: Group Member Edit:Attribute Category
            RockMigrationHelper.DeleteAttribute("9EFED24C-2BE1-4317-9214-B8F10B7684B5");
            // Attrib for BlockType: Group Member Edit:Allow Note Edit
            RockMigrationHelper.DeleteAttribute("A7903B08-04B4-44D7-AE24-877A6ACC0EB4");
            // Attrib for BlockType: Group Member Edit:Allow Member Status Change
            RockMigrationHelper.DeleteAttribute("F5F0F68A-8447-4CB3-8331-2CD3F6B85D69");
            // Attrib for BlockType: Group Member Edit:Allow Role Change
            RockMigrationHelper.DeleteAttribute("0D24AE9B-9B7C-4C6B-A925-1B0AE2224A60");
            // Attrib for BlockType: Group Edit:Group Detail Page
            RockMigrationHelper.DeleteAttribute("1AAB6481-437B-4986-BA8C-3D3BDF4F5BB0");
            // Attrib for BlockType: Group Edit:Attribute Category
            RockMigrationHelper.DeleteAttribute("5D683C41-C76A-4BF0-B9BC-2A53470A6DDA");
            // Attrib for BlockType: Group Edit:Enable Public Status Edit
            RockMigrationHelper.DeleteAttribute("36308A1A-7166-4F4E-96C2-858849EFFDC5");
            // Attrib for BlockType: Group Edit:Show Public Status
            RockMigrationHelper.DeleteAttribute("9E3384AE-9F19-415B-9D9D-0E7890909F13");
            // Attrib for BlockType: Group Edit:Enable Active Status Edit
            RockMigrationHelper.DeleteAttribute("CBF7DB3A-4F4D-4811-A82F-EB82B8EBEF1A");
            // Attrib for BlockType: Group Edit:Show Active Status
            RockMigrationHelper.DeleteAttribute("5D06CA00-7B95-43D0-9372-35C3260A1889");
            // Attrib for BlockType: Group Edit:Enable Group Capacity Edit
            RockMigrationHelper.DeleteAttribute("A1315E00-F13C-4ECA-A420-F1ED581DFBD3");
            // Attrib for BlockType: Group Edit:Show Group Capacity
            RockMigrationHelper.DeleteAttribute("06B69385-6BA3-4961-BAA0-7B0CC111CEFD");
            // Attrib for BlockType: Group Edit:Enable Campus Edit
            RockMigrationHelper.DeleteAttribute("4B8DAA04-4EBB-433B-96DE-42B74E2794DE");
            // Attrib for BlockType: Group Edit:Show Campus
            RockMigrationHelper.DeleteAttribute("2F309C5E-B818-4E4C-9D63-E05C35D15F7A");
            // Attrib for BlockType: Group Edit:Enable Description Edit
            RockMigrationHelper.DeleteAttribute("47B28689-252D-450F-BE21-786818702592");
            // Attrib for BlockType: Group Edit:Show Description
            RockMigrationHelper.DeleteAttribute("854743D8-B84A-4B0B-80AD-136F4FCB6582");
            // Attrib for BlockType: Group Edit:Enable Group Name Edit
            RockMigrationHelper.DeleteAttribute("BB78C538-F105-4ECA-8AC9-34FDD08B90D0");
            // Attrib for BlockType: Group Edit:Show Group Name
            RockMigrationHelper.DeleteAttribute("2093314D-C932-42E1-A31E-F62A507BB1D3");
            // Attrib for BlockType: Calendar View:Show Filter
            RockMigrationHelper.DeleteAttribute("DDA6E0DB-04ED-4B68-9E64-E16B640CBC6F");
            // Attrib for BlockType: Calendar View:Event Summary
            RockMigrationHelper.DeleteAttribute("1D9C2A57-132B-475C-945F-1C3A58D9994C");
            // Attrib for BlockType: Calendar View:Audience Filter
            RockMigrationHelper.DeleteAttribute("314CCDD8-3209-44BA-A708-48712EA70E05");
            // Attrib for BlockType: Calendar View:Detail Page
            RockMigrationHelper.DeleteAttribute("79D8D7E4-FD7B-4DB6-B38E-C6A84DB2C007");
            // Attrib for BlockType: Calendar View:Calendar
            RockMigrationHelper.DeleteAttribute("33DB3DDD-2E78-47DA-B35F-F3C90A07638E");
            RockMigrationHelper.DeleteBlockType("DF70DFE3-3FE5-46B1-87A5-250DD95E8D81"); // Prayer Request Details
            RockMigrationHelper.DeleteBlockType("BFFF482C-8C10-4350-A5C3-BC99F73BB28F"); // Group Member Edit
            RockMigrationHelper.DeleteBlockType("29C5F0F8-D137-4BBD-A1BC-964FE42232FC"); // Group Edit
            RockMigrationHelper.DeleteBlockType("2C32B005-8370-4580-9646-B4C127C91F9D"); // Calendar View
        }

        /// <summary>
        /// NA: Fix TYPO in ContributionStatementLava block attribute
        /// </summary>
        private void ContributionStatementLavaBlock()
        {
            Sql( @"
                UPDATE [Attribute]
                SET [Description] = 'A selection of accounts to include on the statement. If none are selected all accounts that are tax-deductible will be used.'
                WHERE [Guid] = 'EA87DB7F-F053-40A9-B8FB-064D691ACA9D'" );
        }

        /// <summary>
        /// SK: Add "Highlight Color" attribute to Audience Type defined type
        /// </summary>
        private void AudienceTypeDefinedTypeAddHighlightColorAttribute()
        {
            RockMigrationHelper.AddDefinedTypeAttribute( "799301A3-2026-4977-994E-45DC68502559", "D747E6AE-C383-4E22-8846-71518E3DD06F", "Highlight Color", "HighlightColor", "", 1039, "#c4c4c4", "7CE41BAD-58A3-4A1B-B06D-3277F2C816A5" );
            RockMigrationHelper.AddAttributeQualifier( "7CE41BAD-58A3-4A1B-B06D-3277F2C816A5", "selectiontype", "Color Picker", "2EF74C40-7316-432D-82AE-A89E2DD08D0F" );
        }

        /// <summary>
        /// ED: Untaken assessments should have a status of "Pending" (0) instead of complete (1)
        /// </summary>
        private void UpdateAssessmentStatus()
        {
            Sql( @"
                UPDATE [Assessment]
                SET [Status] = 0
                WHERE [Status] = 1 AND [CompletedDateTime] IS NULL" );
        }

    }
}
