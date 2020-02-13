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
    public partial class Rollup_0107 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            FixSubjectLineOnProfileChangeWorkflowEmail();
            SparklineShortcodeMigration();
            EasyPieChartShortcodeMigration();
            StepBulkEntryBlockSettingUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            CodeGenMigrationsDown();
        }

        /// <summary>
        /// Bulk entry block setting for step type list - Up.
        /// </summary>
        private void StepBulkEntryBlockSettingUp()
        {
            // Attrib Value for Block:Step Type List, Attribute:Bulk Entry Page: Step Program, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B7DFAB79-858E-4D44-BD74-38B273BA1EBB", "A5791226-ABCC-4BBA-BDE9-CE605B8AC2DD", @"8224d858-04b3-4dcd-9c73-f9868df29c95" );
        }

        /// <summary>
        /// Script generated "Up" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            // Attrib for BlockType: Calendar View:Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F6EF7AE-4A68-4531-8617-5EF1D82C8473", "EC0D9528-1A22-404E-A776-566404987363", "Calendar", "Calendar", "Calendar", @"The calendar to pull events from", 0, @"", "80A56628-9E4F-4A29-8947-70AC13DF7CDD" );
            // Attrib for BlockType: Calendar View:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F6EF7AE-4A68-4531-8617-5EF1D82C8473", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to push onto the navigation stack when viewing details of an event.", 1, @"", "C18AE17E-4D45-4EEB-AECB-461A994A36AF" );
            // Attrib for BlockType: Calendar View:Audience Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F6EF7AE-4A68-4531-8617-5EF1D82C8473", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Audience Filter", "AudienceFilter", "Audience Filter", @"Determines which audiences should be displayed in the filter.", 2, @"", "A01D3F5B-D1E6-4186-9F53-85891A2DB3ED" );
            // Attrib for BlockType: Calendar View:Event Summary
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F6EF7AE-4A68-4531-8617-5EF1D82C8473", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Event Summary", "EventSummary", "Event Summary", @"The XAML to use when rendering the event summaries below the calendar.", 3, @"<Frame HasShadow=""false"" StyleClass=""calendar-event"">
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
", "1F8796DE-E00B-422A-A423-D8530EA765B9" );
            // Attrib for BlockType: Calendar View:Show Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F6EF7AE-4A68-4531-8617-5EF1D82C8473", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Filter", "ShowFilter", "Show Filter", @"If enabled then the user will be able to apply custom filtering.", 4, @"True", "52830373-0DBC-4269-909F-0524725BC939" );
            // Attrib for BlockType: Group Attendance Entry:Number of Days Forward to Allow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "33319272-3B54-470E-B33C-1E498B34CC0C", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Days Forward to Allow", "NumberOfDaysForwardToAllow", "Number of Days Forward to Allow", @"", 0, @"0", "EA5D7719-70BB-4879-B777-BBFF8D602ED9" );
            // Attrib for BlockType: Group Attendance Entry:Number of Days Back to Allow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "33319272-3B54-470E-B33C-1E498B34CC0C", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Days Back to Allow", "NumberOfDaysBackToAllow", "Number of Days Back to Allow", @"", 1, @"30", "A7328AAF-15B7-4F43-8171-BF884FC7A275" );
            // Attrib for BlockType: Group Attendance Entry:Save Redirect Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "33319272-3B54-470E-B33C-1E498B34CC0C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Save Redirect Page", "SaveRedirectPage", "Save Redirect Page", @"If set, redirect user to this page on save. If not set, page is popped off the navigation stack.", 2, @"", "4E225DAC-32DC-438E-B660-98C714FCC252" );
            // Attrib for BlockType: Group Attendance Entry:Show Save Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "33319272-3B54-470E-B33C-1E498B34CC0C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Save Button", "ShowSaveButton", "Show Save Button", @"If enabled a save button will be shown (recommended for large groups), otherwise no save button will be displayed and a save will be triggered with each selection (recommended for smaller groups).", 3, @"False", "BD7EE7F5-9E15-491D-8973-92B6E82640CB" );
            // Attrib for BlockType: Group Attendance Entry:Allow Any Date Selection
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "33319272-3B54-470E-B33C-1E498B34CC0C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Any Date Selection", "AllowAnyDateSelection", "Allow Any Date Selection", @"If enabled a date picker will be shown, otherwise a dropdown with only the valid dates will be shown.", 4, @"False", "27AEC768-4F1A-4509-B04B-F941587DA4CE" );
            // Attrib for BlockType: Group Edit:Show Group Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE29F0AB-8CB5-429C-B89F-D00F180619FE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Name", "ShowGroupName", "Show Group Name", @"", 0, @"True", "4EE8AC73-1471-40C4-97DF-62E5E812A70D" );
            // Attrib for BlockType: Group Edit:Enable Group Name Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE29F0AB-8CB5-429C-B89F-D00F180619FE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Group Name Edit", "EnableGroupNameEdit", "Enable Group Name Edit", @"", 1, @"True", "A8B25240-F609-4296-B016-A99AC0693563" );
            // Attrib for BlockType: Group Edit:Show Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE29F0AB-8CB5-429C-B89F-D00F180619FE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Description", "ShowDescription", "Show Description", @"", 2, @"True", "6045B47F-4C92-4F02-AF76-84B232FE761A" );
            // Attrib for BlockType: Group Edit:Enable Description Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE29F0AB-8CB5-429C-B89F-D00F180619FE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Description Edit", "EnableDescriptionEdit", "Enable Description Edit", @"", 3, @"True", "B7F8C78E-0D08-41BB-8025-FE099060B57B" );
            // Attrib for BlockType: Group Edit:Show Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE29F0AB-8CB5-429C-B89F-D00F180619FE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus", "ShowCampus", "Show Campus", @"", 4, @"True", "6B676974-B5E3-48CC-9863-581D2C6B6BAD" );
            // Attrib for BlockType: Group Edit:Enable Campus Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE29F0AB-8CB5-429C-B89F-D00F180619FE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Campus Edit", "EnableCampusEdit", "Enable Campus Edit", @"", 5, @"True", "779D9AE7-7F62-44CD-A2CA-2D0CD3210292" );
            // Attrib for BlockType: Group Edit:Show Group Capacity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE29F0AB-8CB5-429C-B89F-D00F180619FE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Capacity", "ShowGroupCapacity", "Show Group Capacity", @"", 6, @"True", "158517D5-0C70-44C4-B2CE-1D29BCB6A970" );
            // Attrib for BlockType: Group Edit:Enable Group Capacity Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE29F0AB-8CB5-429C-B89F-D00F180619FE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Group Capacity Edit", "EnableGroupCapacityEdit", "Enable Group Capacity Edit", @"", 7, @"True", "F78D61A7-33CE-48AC-92C3-C8CEF5028A53" );
            // Attrib for BlockType: Group Edit:Show Active Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE29F0AB-8CB5-429C-B89F-D00F180619FE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Active Status", "ShowActiveStatus", "Show Active Status", @"", 8, @"True", "E7BC87A4-5ACB-4BF3-9D0C-6F97B6345801" );
            // Attrib for BlockType: Group Edit:Enable Active Status Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE29F0AB-8CB5-429C-B89F-D00F180619FE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Active Status Edit", "EnableActiveStatusEdit", "Enable Active Status Edit", @"", 9, @"True", "C0A5255B-AD4E-403D-B767-68927D0BA984" );
            // Attrib for BlockType: Group Edit:Show Public Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE29F0AB-8CB5-429C-B89F-D00F180619FE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Public Status", "ShowPublicStatus", "Show Public Status", @"", 10, @"True", "7DE26ECA-ED21-4DB1-92DD-73447AA851BB" );
            // Attrib for BlockType: Group Edit:Enable Public Status Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE29F0AB-8CB5-429C-B89F-D00F180619FE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Public Status Edit", "EnablePublicStatusEdit", "Enable Public Status Edit", @"", 11, @"True", "1FDAAAFC-FA43-446F-B5E7-C33769444353" );
            // Attrib for BlockType: Group Edit:Attribute Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE29F0AB-8CB5-429C-B89F-D00F180619FE", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Attribute Category", "AttributeCategory", "Attribute Category", @"Category of attributes to show and allow editing on.", 12, @"", "C5D4CF3C-EC06-4536-83BB-0E3604197732" );
            // Attrib for BlockType: Group Edit:Group Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE29F0AB-8CB5-429C-B89F-D00F180619FE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "Group Detail Page", @"The group detail page to return to, if not set then the edit page is popped off the navigation stack.", 13, @"", "51DDDF15-B31A-4031-8E66-D80D88F393F8" );
            // Attrib for BlockType: Group Member Edit:Allow Role Change
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4F348754-EB4F-48D3-834C-161B6DD910F2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Role Change", "AllowRoleChange", "Allow Role Change", @"", 0, @"True", "3D070497-2C10-4880-989C-AE7922DC802F" );
            // Attrib for BlockType: Group Member Edit:Allow Member Status Change
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4F348754-EB4F-48D3-834C-161B6DD910F2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Member Status Change", "AllowMemberStatusChange", "Allow Member Status Change", @"", 1, @"True", "53B030D0-7274-4BDA-ADBF-AEA35AFCA853" );
            // Attrib for BlockType: Group Member Edit:Allow Note Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4F348754-EB4F-48D3-834C-161B6DD910F2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Note Edit", "AllowNoteEdit", "Allow Note Edit", @"", 2, @"True", "375F46A1-58E9-444D-8D64-018DFDFF845D" );
            // Attrib for BlockType: Group Member Edit:Attribute Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4F348754-EB4F-48D3-834C-161B6DD910F2", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Attribute Category", "AttributeCategory", "Attribute Category", @"Category of attributes to show and allow editing on.", 3, @"", "C4261424-D89D-4D70-802B-CF12E7E97A68" );
            // Attrib for BlockType: Group Member Edit:Member Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4F348754-EB4F-48D3-834C-161B6DD910F2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Member Detail Page", "MemberDetailsPage", "Member Detail Page", @"The group member page to return to, if not set then the edit page is popped off the navigation stack.", 4, @"", "E61B1585-1DDF-46D3-B006-EA30FFB36133" );
            // Attrib for BlockType: Prayer Request Details:Show Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CF440880-B401-46D2-AD25-6E5F180B1D71", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Category", "EnableCategory", "Show Category", @"If disabled, then the user will not be able to select a category and the default category will be used exclusively.", 0, @"True", "3AA87EF5-A2D5-4E42-ADE2-5A4374953834" );
            // Attrib for BlockType: Prayer Request Details:Parent Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CF440880-B401-46D2-AD25-6E5F180B1D71", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Parent Category", "ParentCategory", "Parent Category", @"A top level category. This controls which categories the person can choose from when entering their prayer request.", 1, @"", "22947F18-BDAD-44F7-9D44-515CE756BFB7" );
            // Attrib for BlockType: Prayer Request Details:Default Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CF440880-B401-46D2-AD25-6E5F180B1D71", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Default Category", "DefaultCategory", "Default Category", @"The default category to use for all new prayer requests.", 2, @"", "B4157AA5-70EC-4275-964D-67BFAD400CD6" );
            // Attrib for BlockType: Prayer Request Details:Enable Auto Approve
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CF440880-B401-46D2-AD25-6E5F180B1D71", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Auto Approve", "EnableAutoApprove", "Enable Auto Approve", @"If enabled, prayer requests are automatically approved; otherwise they must be approved by an admin before they can be seen by the prayer team.", 0, @"True", "FFE51556-8F3C-4610-A47F-6945E71C4958" );
            // Attrib for BlockType: Prayer Request Details:Expires After (Days)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CF440880-B401-46D2-AD25-6E5F180B1D71", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Expires After (Days)", "ExpiresAfterDays", "Expires After (Days)", @"Number of days until the request will expire (only applies when auto-approved is enabled).", 1, @"14", "D8C9E990-A55A-4A0F-89A2-B0FBE8B1B59B" );
            // Attrib for BlockType: Prayer Request Details:Show Urgent Flag
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CF440880-B401-46D2-AD25-6E5F180B1D71", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Urgent Flag", "EnableUrgentFlag", "Show Urgent Flag", @"If enabled, requestors will be able to flag prayer requests as urgent.", 2, @"False", "24F4A146-198F-4B60-B7C1-24CEEE62627F" );
            // Attrib for BlockType: Prayer Request Details:Show Public Display Flag
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CF440880-B401-46D2-AD25-6E5F180B1D71", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Public Display Flag", "EnablePublicDisplayFlag", "Show Public Display Flag", @"If enabled, requestors will be able set whether or not they want their request displayed on the public website.", 3, @"False", "C256F846-6658-465E-8E6B-3CE5EB6A5B13" );
            // Attrib for BlockType: Prayer Request Details:Default To Public
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CF440880-B401-46D2-AD25-6E5F180B1D71", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default To Public", "DefaultToPublic", "Default To Public", @"If enabled, all prayers will be set to public by default.", 4, @"False", "AA8169C3-B45A-4052-8793-5A56D212ADAD" );
            // Attrib for BlockType: Prayer Request Details:Character Limit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CF440880-B401-46D2-AD25-6E5F180B1D71", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Character Limit", "CharacterLimit", "Character Limit", @"If set to something other than 0, this will limit the number of characters allowed when entering a new prayer request.", 5, @"250", "080FF335-FD4D-4726-A11B-FD5321E21697" );
            // Attrib for BlockType: Prayer Request Details:Show Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CF440880-B401-46D2-AD25-6E5F180B1D71", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus", "EnableCampus", "Show Campus", @"Should the campus field be displayed? If there is only one active campus then the campus field will not show.", 6, @"True", "17C4ABF6-9640-4834-9C11-FA11D9B84EA7" );
            // Attrib for BlockType: Prayer Request Details:Require Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CF440880-B401-46D2-AD25-6E5F180B1D71", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Campus", "RequireCampus", "Require Campus", @"Require that a campus be selected. The campus will not be displayed if there is only one available campus, in which case if this is set to true then the single campus is automatically used.", 7, @"False", "EF652B26-26F1-4DAC-A2D8-DEC670ECD8DD" );
            // Attrib for BlockType: Prayer Request Details:Require Last Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CF440880-B401-46D2-AD25-6E5F180B1D71", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Last Name", "RequireLastName", "Require Last Name", @"Require that a last name be entered. First name is always required.", 8, @"True", "C05502D6-F390-45D2-A6D6-A0B032EF57F8" );
            // Attrib for BlockType: Prayer Request Details:Enable Person Matching
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CF440880-B401-46D2-AD25-6E5F180B1D71", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Person Matching", "EnablePersonMatching", "Enable Person Matching", @"If enabled, the request will be linked to an existing person if a match can be made between the requester and an existing person.", 9, @"False", "1EE52F40-6047-4258-BADC-A4491DE80737" );
            // Attrib for BlockType: Prayer Request Details:Completion Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CF440880-B401-46D2-AD25-6E5F180B1D71", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Completion Action", "CompletionAction", "Completion Action", @"What action to perform after saving the prayer request.", 0, @"0", "1668E4A2-7472-4B87-B6A1-7E57A7E354A5" );
            // Attrib for BlockType: Prayer Request Details:Completion Xaml
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CF440880-B401-46D2-AD25-6E5F180B1D71", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Completion Xaml", "CompletionXaml", "Completion Xaml", @"The XAML markup that will be used if the. <span class='tip tip-lava'></span>", 1, @"<Rock:NotificationBox NotificationType=""Success"">
    Thank you for allowing us to pray for you.
</Rock:NotificationBox>", "2C9A82FF-7B41-4422-8BB0-5AFD02F94791" );
            // Attrib for BlockType: Prayer Request Details:Workflow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CF440880-B401-46D2-AD25-6E5F180B1D71", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "Workflow", @"An optional workflow to start when prayer request is created. The PrayerRequest will be set as the workflow 'Entity' attribute when processing is started.", 2, @"", "ED76ADA7-99C3-4DF7-A2AC-77F3D2514B3C" );
            RockMigrationHelper.UpdateFieldType("Metric","","Rock","Rock.Field.Types.MetricFieldType","AD52248B-B8C6-436E-9B57-E0BA4B42603E");

        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
                        // Attrib for BlockType: Prayer Request Details:Workflow
            RockMigrationHelper.DeleteAttribute("ED76ADA7-99C3-4DF7-A2AC-77F3D2514B3C");
            // Attrib for BlockType: Prayer Request Details:Completion Xaml
            RockMigrationHelper.DeleteAttribute("2C9A82FF-7B41-4422-8BB0-5AFD02F94791");
            // Attrib for BlockType: Prayer Request Details:Completion Action
            RockMigrationHelper.DeleteAttribute("1668E4A2-7472-4B87-B6A1-7E57A7E354A5");
            // Attrib for BlockType: Prayer Request Details:Enable Person Matching
            RockMigrationHelper.DeleteAttribute("1EE52F40-6047-4258-BADC-A4491DE80737");
            // Attrib for BlockType: Prayer Request Details:Require Last Name
            RockMigrationHelper.DeleteAttribute("C05502D6-F390-45D2-A6D6-A0B032EF57F8");
            // Attrib for BlockType: Prayer Request Details:Require Campus
            RockMigrationHelper.DeleteAttribute("EF652B26-26F1-4DAC-A2D8-DEC670ECD8DD");
            // Attrib for BlockType: Prayer Request Details:Show Campus
            RockMigrationHelper.DeleteAttribute("17C4ABF6-9640-4834-9C11-FA11D9B84EA7");
            // Attrib for BlockType: Prayer Request Details:Character Limit
            RockMigrationHelper.DeleteAttribute("080FF335-FD4D-4726-A11B-FD5321E21697");
            // Attrib for BlockType: Prayer Request Details:Default To Public
            RockMigrationHelper.DeleteAttribute("AA8169C3-B45A-4052-8793-5A56D212ADAD");
            // Attrib for BlockType: Prayer Request Details:Show Public Display Flag
            RockMigrationHelper.DeleteAttribute("C256F846-6658-465E-8E6B-3CE5EB6A5B13");
            // Attrib for BlockType: Prayer Request Details:Show Urgent Flag
            RockMigrationHelper.DeleteAttribute("24F4A146-198F-4B60-B7C1-24CEEE62627F");
            // Attrib for BlockType: Prayer Request Details:Expires After (Days)
            RockMigrationHelper.DeleteAttribute("D8C9E990-A55A-4A0F-89A2-B0FBE8B1B59B");
            // Attrib for BlockType: Prayer Request Details:Enable Auto Approve
            RockMigrationHelper.DeleteAttribute("FFE51556-8F3C-4610-A47F-6945E71C4958");
            // Attrib for BlockType: Prayer Request Details:Default Category
            RockMigrationHelper.DeleteAttribute("B4157AA5-70EC-4275-964D-67BFAD400CD6");
            // Attrib for BlockType: Prayer Request Details:Parent Category
            RockMigrationHelper.DeleteAttribute("22947F18-BDAD-44F7-9D44-515CE756BFB7");
            // Attrib for BlockType: Prayer Request Details:Show Category
            RockMigrationHelper.DeleteAttribute("3AA87EF5-A2D5-4E42-ADE2-5A4374953834");
            // Attrib for BlockType: Group Member Edit:Member Detail Page
            RockMigrationHelper.DeleteAttribute("E61B1585-1DDF-46D3-B006-EA30FFB36133");
            // Attrib for BlockType: Group Member Edit:Attribute Category
            RockMigrationHelper.DeleteAttribute("C4261424-D89D-4D70-802B-CF12E7E97A68");
            // Attrib for BlockType: Group Member Edit:Allow Note Edit
            RockMigrationHelper.DeleteAttribute("375F46A1-58E9-444D-8D64-018DFDFF845D");
            // Attrib for BlockType: Group Member Edit:Allow Member Status Change
            RockMigrationHelper.DeleteAttribute("53B030D0-7274-4BDA-ADBF-AEA35AFCA853");
            // Attrib for BlockType: Group Member Edit:Allow Role Change
            RockMigrationHelper.DeleteAttribute("3D070497-2C10-4880-989C-AE7922DC802F");
            // Attrib for BlockType: Group Edit:Group Detail Page
            RockMigrationHelper.DeleteAttribute("51DDDF15-B31A-4031-8E66-D80D88F393F8");
            // Attrib for BlockType: Group Edit:Attribute Category
            RockMigrationHelper.DeleteAttribute("C5D4CF3C-EC06-4536-83BB-0E3604197732");
            // Attrib for BlockType: Group Edit:Enable Public Status Edit
            RockMigrationHelper.DeleteAttribute("1FDAAAFC-FA43-446F-B5E7-C33769444353");
            // Attrib for BlockType: Group Edit:Show Public Status
            RockMigrationHelper.DeleteAttribute("7DE26ECA-ED21-4DB1-92DD-73447AA851BB");
            // Attrib for BlockType: Group Edit:Enable Active Status Edit
            RockMigrationHelper.DeleteAttribute("C0A5255B-AD4E-403D-B767-68927D0BA984");
            // Attrib for BlockType: Group Edit:Show Active Status
            RockMigrationHelper.DeleteAttribute("E7BC87A4-5ACB-4BF3-9D0C-6F97B6345801");
            // Attrib for BlockType: Group Edit:Enable Group Capacity Edit
            RockMigrationHelper.DeleteAttribute("F78D61A7-33CE-48AC-92C3-C8CEF5028A53");
            // Attrib for BlockType: Group Edit:Show Group Capacity
            RockMigrationHelper.DeleteAttribute("158517D5-0C70-44C4-B2CE-1D29BCB6A970");
            // Attrib for BlockType: Group Edit:Enable Campus Edit
            RockMigrationHelper.DeleteAttribute("779D9AE7-7F62-44CD-A2CA-2D0CD3210292");
            // Attrib for BlockType: Group Edit:Show Campus
            RockMigrationHelper.DeleteAttribute("6B676974-B5E3-48CC-9863-581D2C6B6BAD");
            // Attrib for BlockType: Group Edit:Enable Description Edit
            RockMigrationHelper.DeleteAttribute("B7F8C78E-0D08-41BB-8025-FE099060B57B");
            // Attrib for BlockType: Group Edit:Show Description
            RockMigrationHelper.DeleteAttribute("6045B47F-4C92-4F02-AF76-84B232FE761A");
            // Attrib for BlockType: Group Edit:Enable Group Name Edit
            RockMigrationHelper.DeleteAttribute("A8B25240-F609-4296-B016-A99AC0693563");
            // Attrib for BlockType: Group Edit:Show Group Name
            RockMigrationHelper.DeleteAttribute("4EE8AC73-1471-40C4-97DF-62E5E812A70D");
            // Attrib for BlockType: Group Attendance Entry:Allow Any Date Selection
            RockMigrationHelper.DeleteAttribute("27AEC768-4F1A-4509-B04B-F941587DA4CE");
            // Attrib for BlockType: Group Attendance Entry:Show Save Button
            RockMigrationHelper.DeleteAttribute("BD7EE7F5-9E15-491D-8973-92B6E82640CB");
            // Attrib for BlockType: Group Attendance Entry:Save Redirect Page
            RockMigrationHelper.DeleteAttribute("4E225DAC-32DC-438E-B660-98C714FCC252");
            // Attrib for BlockType: Group Attendance Entry:Number of Days Back to Allow
            RockMigrationHelper.DeleteAttribute("A7328AAF-15B7-4F43-8171-BF884FC7A275");
            // Attrib for BlockType: Group Attendance Entry:Number of Days Forward to Allow
            RockMigrationHelper.DeleteAttribute("EA5D7719-70BB-4879-B777-BBFF8D602ED9");
            // Attrib for BlockType: Calendar View:Show Filter
            RockMigrationHelper.DeleteAttribute("52830373-0DBC-4269-909F-0524725BC939");
            // Attrib for BlockType: Calendar View:Event Summary
            RockMigrationHelper.DeleteAttribute("1F8796DE-E00B-422A-A423-D8530EA765B9");
            // Attrib for BlockType: Calendar View:Audience Filter
            RockMigrationHelper.DeleteAttribute("A01D3F5B-D1E6-4186-9F53-85891A2DB3ED");
            // Attrib for BlockType: Calendar View:Detail Page
            RockMigrationHelper.DeleteAttribute("C18AE17E-4D45-4EEB-AECB-461A994A36AF");
            // Attrib for BlockType: Calendar View:Calendar
            RockMigrationHelper.DeleteAttribute("80A56628-9E4F-4A29-8947-70AC13DF7CDD");
            RockMigrationHelper.DeleteBlockType("CF440880-B401-46D2-AD25-6E5F180B1D71"); // Prayer Request Details
            RockMigrationHelper.DeleteBlockType("4F348754-EB4F-48D3-834C-161B6DD910F2"); // Group Member Edit
            RockMigrationHelper.DeleteBlockType("EE29F0AB-8CB5-429C-B89F-D00F180619FE"); // Group Edit
            RockMigrationHelper.DeleteBlockType("33319272-3B54-470E-B33C-1E498B34CC0C"); // Group Attendance Entry
            RockMigrationHelper.DeleteBlockType("5F6EF7AE-4A68-4531-8617-5EF1D82C8473"); // Calendar View
        }

        /// <summary>
        /// NA: Fix issue #4044 Incorrect Subject Line on Profile Change Request workflow email
        /// </summary>
        private void FixSubjectLineOnProfileChangeWorkflowEmail()
        {
            RockMigrationHelper.AddActionTypeAttributeValue( "EE021FFF-7D5E-49B5-94F9-0EFD2E3FC349", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Profile Change Request Completed" ); // Profile Change Request:Complete:Notify Requester:Subject
        }

        /// <summary>
        /// GJ: Sparkline Shortcode Migration
        /// </summary>
        private void SparklineShortcodeMigration()
        {
            Sql( MigrationSQL._202001071827547_Rollup_0107_SparklineShortcodeMigration );
        }

        /// <summary>
        /// GJ: Easy Pie Chart Shortcode Migration
        /// </summary>
        private void EasyPieChartShortcodeMigration()
        {
            Sql( MigrationSQL._202001071827547_Rollup_0107_EasyPieChartShortcodeMigration );
        }
    }
}
