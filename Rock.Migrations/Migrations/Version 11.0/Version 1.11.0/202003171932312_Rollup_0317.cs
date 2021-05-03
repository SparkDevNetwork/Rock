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
    public partial class Rollup_0317 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            RemoveCommunicationPreference();
            UpdateEmailMediumAttributeNonHtmlContent();
            UpdateCheckinSuccessMessage();
            UpdateEventListTemplateType();
            UpdatePrayerSessionTemplateType();
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
            // Attrib for BlockType: Hero:Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8D6116C-BB8A-49DD-BEDA-12FCEA7BE2FB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "Title", @"The main title to display over the image. <span class='tip tip-lava'></span>", 0, @"", "62C96016-C476-46B1-BB5C-94745548BD99" );
            // Attrib for BlockType: Hero:Subtitle
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8D6116C-BB8A-49DD-BEDA-12FCEA7BE2FB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subtitle", "Subtitle", "Subtitle", @"The subtitle to display over the image. <span class='tip tip-lava'></span>", 1, @"", "4F233235-608F-482F-BBC5-7AFFDE6A67C8" );
            // Attrib for BlockType: Hero:Background Image - Phone
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8D6116C-BB8A-49DD-BEDA-12FCEA7BE2FB", "6F9E2DD0-E39E-4602-ADF9-EB710A75304A", "Background Image - Phone", "BackgroundImagePhone", "Background Image - Phone", @"Recommended size is at least 1024px wide and double the height specified below.", 2, @"", "BE1D377F-08FA-48CC-AD4C-40F81565643A" );
            // Attrib for BlockType: Hero:Background Image - Tablet
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8D6116C-BB8A-49DD-BEDA-12FCEA7BE2FB", "6F9E2DD0-E39E-4602-ADF9-EB710A75304A", "Background Image - Tablet", "BackgroundImageTablet", "Background Image - Tablet", @"Recommended size is at least 2048px wide and double the height specified below.", 3, @"", "0BDBE3CB-525A-4EA4-BD29-29357DD73580" );
            // Attrib for BlockType: Hero:Height - Phone
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8D6116C-BB8A-49DD-BEDA-12FCEA7BE2FB", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Height - Phone", "ImageHeightPhone", "Height - Phone", @"", 4, @"200", "601C566C-0968-4D26-B1F2-813E4F12FD68" );
            // Attrib for BlockType: Hero:Height - Tablet
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8D6116C-BB8A-49DD-BEDA-12FCEA7BE2FB", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Height - Tablet", "ImageHeightTablet", "Height - Tablet", @"", 5, @"350", "1CD979FC-ED76-453E-BDE0-BE790E5E4F40" );
            // Attrib for BlockType: Hero:Text Align
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8D6116C-BB8A-49DD-BEDA-12FCEA7BE2FB", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Text Align", "HorizontalTextAlign", "Text Align", @"", 6, @"Center", "E98C20DE-AFB0-4E10-8198-D187A49C4E4C" );
            // Attrib for BlockType: Hero:Title Color
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8D6116C-BB8A-49DD-BEDA-12FCEA7BE2FB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title Color", "TitleColor", "Title Color", @"Will override the theme's hero title (.hero-title) color.", 7, @"", "FFF48796-9037-46F0-8A00-16B120264B39" );
            // Attrib for BlockType: Hero:Subtitle Color
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8D6116C-BB8A-49DD-BEDA-12FCEA7BE2FB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subtitle Color", "SubtitleColor", "Subtitle Color", @"Will override the theme's hero subtitle (.hero-subtitle) color.", 8, @"", "5499A384-FE0B-4FDF-8478-589B8CC5D1E4" );
            // Attrib for BlockType: Hero:Padding
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8D6116C-BB8A-49DD-BEDA-12FCEA7BE2FB", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Padding", "Padding", "Padding", @"The padding around the inside of the image.", 9, @"20", "9E7EC130-84B7-4C9A-B43E-4A4CCA491B57" );
            // Attrib for BlockType: Calendar Event List:Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C2E0A17C-C4D0-4178-BE62-7818AB154B27", "EC0D9528-1A22-404E-A776-566404987363", "Calendar", "Calendar", "Calendar", @"The calendar to pull events from", 0, @"", "2DB08B4B-D3A7-45FD-87D9-1BB78F2DC307" );
            // Attrib for BlockType: Calendar Event List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C2E0A17C-C4D0-4178-BE62-7818AB154B27", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to push onto the navigation stack when viewing details of an event.", 1, @"", "251A2B03-1D2A-43C0-A4CD-4F015291A9AD" );
            // Attrib for BlockType: Calendar Event List:Event Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C2E0A17C-C4D0-4178-BE62-7818AB154B27", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Event Template", "EventTemplate", "Event Template", @"The template to use when rendering event items.", 2, @"", "0F2CAE76-9500-4B41-93AC-7351071F0BC3" );
            // Attrib for BlockType: Calendar Event List:Day Header Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C2E0A17C-C4D0-4178-BE62-7818AB154B27", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Day Header Template", "DayHeaderTemplate", "Day Header Template", @"The XAML to use when rendering the day header above a grouping of events.", 3, @"<Frame HasShadow=""false"" StyleClass=""calendar-events-day"">
    <Label Text=""{Binding ., StringFormat=""{0:dddd MMMM d}""}"" />
</Frame>
", "1C01F417-9F4F-4C5E-81FF-B8B5CAF7F678" );
            // Attrib for BlockType: Calendar View:Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D7F829CC-941C-43BD-AA65-FFA5F8A232F1", "EC0D9528-1A22-404E-A776-566404987363", "Calendar", "Calendar", "Calendar", @"The calendar to pull events from", 0, @"", "83ADA17D-8BEE-48A6-A43A-46AE51224460" );
            // Attrib for BlockType: Calendar View:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D7F829CC-941C-43BD-AA65-FFA5F8A232F1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to push onto the navigation stack when viewing details of an event.", 1, @"", "B3A6468B-2B56-44B0-BF44-CE04B0C8ABF8" );
            // Attrib for BlockType: Calendar View:Audience Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D7F829CC-941C-43BD-AA65-FFA5F8A232F1", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Audience Filter", "AudienceFilter", "Audience Filter", @"Determines which audiences should be displayed in the filter.", 2, @"", "1C40411B-080E-4843-9D6D-24578EB0F168" );
            // Attrib for BlockType: Calendar View:Event Summary
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D7F829CC-941C-43BD-AA65-FFA5F8A232F1", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Event Summary", "EventSummary", "Event Summary", @"The XAML to use when rendering the event summaries below the calendar.", 3, @"<Frame HasShadow=""false"" StyleClass=""calendar-event"">
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
", "AA2E281E-FD07-4B6E-956A-7018E7CAD292" );
            // Attrib for BlockType: Calendar View:Show Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D7F829CC-941C-43BD-AA65-FFA5F8A232F1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Filter", "ShowFilter", "Show Filter", @"If enabled then the user will be able to apply custom filtering.", 4, @"True", "5FF22A0B-2A62-4C0E-B0AA-3A846AC2EAB5" );
            // Attrib for BlockType: Communication List Subscribe:Communication List Categories
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E7976940-2622-47E0-85EA-AFDD189C8CB3", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Communication List Categories", "CommunicationListCategories", "Communication List Categories", @"Select the categories of the communication lists to display, or select none to show all that the user is authorized to view.", 0, @"", "C1F3E7E3-BB63-419A-A299-6FD8E12B08CF" );
            // Attrib for BlockType: Communication List Subscribe:Show Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E7976940-2622-47E0-85EA-AFDD189C8CB3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Description", "ShowDescription", "Show Description", @"If enabled then the description of the communication list will be shown.", 1, @"False", "4B48BC6B-3754-498A-ADEC-D7782B188F8D" );
            // Attrib for BlockType: Prayer Session:Prayed Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "39FD7EAD-A4C9-41C4-8E0D-5EF301691DBC", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Prayed Button Text", "PrayedButtonText", "Prayed Button Text", @"The text to display inside the Prayed button. Available in the XAML template as lava variable 'PrayedButtonText'.", 0, @"I've Prayed", "1ACC691E-4000-4261-9732-7AEFB8B245F8" );
            // Attrib for BlockType: Prayer Session:Show Follow Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "39FD7EAD-A4C9-41C4-8E0D-5EF301691DBC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Follow Button", "ShowFollowButton", "Show Follow Button", @"Indicates if the Follow button should be shown. Available in the XAML template as lava variable 'ShowFollowButton'.", 1, @"True", "25B5E9A4-D503-45F2-8912-FC0D655F6E9B" );
            // Attrib for BlockType: Prayer Session:Show Inappropriate Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "39FD7EAD-A4C9-41C4-8E0D-5EF301691DBC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Inappropriate Button", "ShowInappropriateButton", "Show Inappropriate Button", @"Indicates if the button to flag a request as inappropriate should be shown. Available in the XAML template as lava variable 'ShowInappropriateButton'.", 2, @"True", "51B80681-89CD-4BDB-879F-FBB7B012B93F" );
            // Attrib for BlockType: Prayer Session:Public Only
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "39FD7EAD-A4C9-41C4-8E0D-5EF301691DBC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Public Only", "PublicOnly", "Public Only", @"If enabled then only prayer requests marked as public will be shown.", 3, @"False", "4E870E79-34BB-4A08-AD48-B524F91F22E9" );
            // Attrib for BlockType: Prayer Session:Inappropriate Flag Limit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "39FD7EAD-A4C9-41C4-8E0D-5EF301691DBC", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Inappropriate Flag Limit", "InappropriateFlagLimit", "Inappropriate Flag Limit", @"The number of flags a prayer request has to get from the prayer team before it is automatically unapproved.", 4, @"", "76779F3D-1754-4522-ABEA-32CE76496D02" );
            // Attrib for BlockType: Prayer Session:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "39FD7EAD-A4C9-41C4-8E0D-5EF301691DBC", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering prayer requests.", 5, @"", "A4A09897-14FE-481F-B0A5-43C73B1B3721" );
            // Attrib for BlockType: Prayer Session Setup:Prayer Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "55604B68-F12A-446D-8BA9-F44580A7C425", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Prayer Page", "PrayerPage", "Prayer Page", @"The page to push onto the navigation stack to begin the prayer session.", 0, @"", "E0AF9F7B-DCDA-4423-B202-753648018F7A" );
            // Attrib for BlockType: Prayer Session Setup:Parent Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "55604B68-F12A-446D-8BA9-F44580A7C425", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Parent Category", "ParentCategory", "Parent Category", @"The parent category to use as the root category available for the user to pick from.", 1, @"", "4A26D15B-807A-4B59-B166-7190F226B6DD" );
            // Attrib for BlockType: Prayer Session Setup:Show Campus Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "55604B68-F12A-446D-8BA9-F44580A7C425", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus Filter", "ShowCampusFilter", "Show Campus Filter", @"If enabled and the user has a primary campus, then the user will be offered to limit prayer requests to just their campus.", 2, @"False", "5AB03E37-D2C7-47EE-A788-051BFF949133" );
            // Attrib for BlockType: Group Attendance Entry:Number of Days Forward to Allow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56B67C32-63BC-4ECE-A0CC-332D7EF56E75", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Days Forward to Allow", "NumberOfDaysForwardToAllow", "Number of Days Forward to Allow", @"", 0, @"0", "7926A67F-3F01-42F8-BF93-39E1111861E1" );
            // Attrib for BlockType: Group Attendance Entry:Number of Days Back to Allow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56B67C32-63BC-4ECE-A0CC-332D7EF56E75", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Days Back to Allow", "NumberOfDaysBackToAllow", "Number of Days Back to Allow", @"", 1, @"30", "5B0E8292-25BB-4761-9E56-CAA0B6E1E946" );
            // Attrib for BlockType: Group Attendance Entry:Save Redirect Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56B67C32-63BC-4ECE-A0CC-332D7EF56E75", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Save Redirect Page", "SaveRedirectPage", "Save Redirect Page", @"If set, redirect user to this page on save. If not set, page is popped off the navigation stack.", 2, @"", "F33BE557-DC3D-4EF1-86ED-E92713661A50" );
            // Attrib for BlockType: Group Attendance Entry:Show Save Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56B67C32-63BC-4ECE-A0CC-332D7EF56E75", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Save Button", "ShowSaveButton", "Show Save Button", @"If enabled a save button will be shown (recommended for large groups), otherwise no save button will be displayed and a save will be triggered with each selection (recommended for smaller groups).", 3, @"False", "962E6C36-D053-4198-A232-3D47CE4A95F3" );
            // Attrib for BlockType: Group Attendance Entry:Allow Any Date Selection
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56B67C32-63BC-4ECE-A0CC-332D7EF56E75", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Any Date Selection", "AllowAnyDateSelection", "Allow Any Date Selection", @"If enabled a date picker will be shown, otherwise a dropdown with only the valid dates will be shown.", 4, @"False", "0BF5CAD0-3BE2-4734-B369-AB3E9F37B03D" );
            // Attrib for BlockType: Group Edit:Show Group Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DCBA7095-9619-4199-9F04-926CFB3A5125", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Name", "ShowGroupName", "Show Group Name", @"", 0, @"True", "D96332EF-3BA2-4A07-87B8-8ADA71D374E5" );
            // Attrib for BlockType: Group Edit:Enable Group Name Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DCBA7095-9619-4199-9F04-926CFB3A5125", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Group Name Edit", "EnableGroupNameEdit", "Enable Group Name Edit", @"", 1, @"True", "59AADEC9-E127-4FFE-8485-9CACD520A1FD" );
            // Attrib for BlockType: Group Edit:Show Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DCBA7095-9619-4199-9F04-926CFB3A5125", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Description", "ShowDescription", "Show Description", @"", 2, @"True", "AA7FC391-F0BD-4BA0-AB29-8B85AE57FA53" );
            // Attrib for BlockType: Group Edit:Enable Description Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DCBA7095-9619-4199-9F04-926CFB3A5125", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Description Edit", "EnableDescriptionEdit", "Enable Description Edit", @"", 3, @"True", "CB660D2C-7BE5-45E3-8FCB-C1F2069F376C" );
            // Attrib for BlockType: Group Edit:Show Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DCBA7095-9619-4199-9F04-926CFB3A5125", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus", "ShowCampus", "Show Campus", @"", 4, @"True", "51A1D1E2-FA15-49CC-A455-C0C9CA503948" );
            // Attrib for BlockType: Group Edit:Enable Campus Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DCBA7095-9619-4199-9F04-926CFB3A5125", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Campus Edit", "EnableCampusEdit", "Enable Campus Edit", @"", 5, @"True", "FE73696C-50B3-442F-939A-A76F848B62F1" );
            // Attrib for BlockType: Group Edit:Show Group Capacity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DCBA7095-9619-4199-9F04-926CFB3A5125", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Capacity", "ShowGroupCapacity", "Show Group Capacity", @"", 6, @"True", "EFF8DF79-9C24-4A4C-A076-B784959BA218" );
            // Attrib for BlockType: Group Edit:Enable Group Capacity Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DCBA7095-9619-4199-9F04-926CFB3A5125", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Group Capacity Edit", "EnableGroupCapacityEdit", "Enable Group Capacity Edit", @"", 7, @"True", "60975D2A-5F60-4195-A20D-D8503A9E7139" );
            // Attrib for BlockType: Group Edit:Show Active Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DCBA7095-9619-4199-9F04-926CFB3A5125", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Active Status", "ShowActiveStatus", "Show Active Status", @"", 8, @"True", "8EA4FFA0-D103-4E08-9A33-2774C1270CE1" );
            // Attrib for BlockType: Group Edit:Enable Active Status Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DCBA7095-9619-4199-9F04-926CFB3A5125", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Active Status Edit", "EnableActiveStatusEdit", "Enable Active Status Edit", @"", 9, @"True", "CE84FEBB-0DA2-48D2-BF7F-64962AC1E6F9" );
            // Attrib for BlockType: Group Edit:Show Public Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DCBA7095-9619-4199-9F04-926CFB3A5125", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Public Status", "ShowPublicStatus", "Show Public Status", @"", 10, @"True", "C67B74BF-6C74-4179-ACDE-2FD6DB47FEB8" );
            // Attrib for BlockType: Group Edit:Enable Public Status Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DCBA7095-9619-4199-9F04-926CFB3A5125", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Public Status Edit", "EnablePublicStatusEdit", "Enable Public Status Edit", @"", 11, @"True", "5B2A1026-8CC0-46F8-80B3-F724FB3AA122" );
            // Attrib for BlockType: Group Edit:Attribute Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DCBA7095-9619-4199-9F04-926CFB3A5125", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Attribute Category", "AttributeCategory", "Attribute Category", @"Category of attributes to show and allow editing on.", 12, @"", "28EC8ACA-0868-43BC-AEE7-714053CE6D0F" );
            // Attrib for BlockType: Group Edit:Group Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DCBA7095-9619-4199-9F04-926CFB3A5125", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "Group Detail Page", @"The group detail page to return to, if not set then the edit page is popped off the navigation stack.", 13, @"", "F6FD5634-3577-466F-B4F4-23D8393F9A09" );
            // Attrib for BlockType: Group Member Edit:Allow Role Change
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AB2334B-5EE9-46E9-AE8D-C3039BF8BE24", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Role Change", "AllowRoleChange", "Allow Role Change", @"", 0, @"True", "9881CAD7-9ADF-4D49-B57E-F471154350D5" );
            // Attrib for BlockType: Group Member Edit:Allow Member Status Change
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AB2334B-5EE9-46E9-AE8D-C3039BF8BE24", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Member Status Change", "AllowMemberStatusChange", "Allow Member Status Change", @"", 1, @"True", "5CBA77A7-61A3-4EE5-B6FD-415229623CA1" );
            // Attrib for BlockType: Group Member Edit:Allow Note Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AB2334B-5EE9-46E9-AE8D-C3039BF8BE24", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Note Edit", "AllowNoteEdit", "Allow Note Edit", @"", 2, @"True", "28256F6B-D8D6-478F-B527-EC8DF0B6606B" );
            // Attrib for BlockType: Group Member Edit:Attribute Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AB2334B-5EE9-46E9-AE8D-C3039BF8BE24", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Attribute Category", "AttributeCategory", "Attribute Category", @"Category of attributes to show and allow editing on.", 3, @"", "8EFA540A-D4DE-4A84-9EC5-798320DC72E3" );
            // Attrib for BlockType: Group Member Edit:Member Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5AB2334B-5EE9-46E9-AE8D-C3039BF8BE24", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Member Detail Page", "MemberDetailsPage", "Member Detail Page", @"The group member page to return to, if not set then the edit page is popped off the navigation stack.", 4, @"", "73C62B55-8735-4175-A2CF-817C30CEE88A" );
            // Attrib for BlockType: Group Member List:Group Member Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BE6043E9-BD74-417A-86B5-7C31EECB8078", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Member Detail Page", "GroupMemberDetailPage", "Group Member Detail Page", @"The page that will display the group member details when selecting a member.", 0, @"", "F09F305E-C9F5-4CC1-8B8C-DCC051927A59" );
            // Attrib for BlockType: Group Member List:Title Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BE6043E9-BD74-417A-86B5-7C31EECB8078", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title Template", "TitleTemplate", "Title Template", @"The value to use when rendering the title text. <span class='tip tip-lava'></span>", 1, @"{{ Group.Name }} Group Roster", "8D93EFE8-C780-4B03-990B-5631ABAF9057" );
            // Attrib for BlockType: Group Member List:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BE6043E9-BD74-417A-86B5-7C31EECB8078", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 2, @"", "528E2A27-6615-46CE-A5E3-36686E456DDB" );
            // Attrib for BlockType: Group Member List:Additional Fields
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BE6043E9-BD74-417A-86B5-7C31EECB8078", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Additional Fields", "AdditionalFields", "Additional Fields", @"", 3, @"", "F9E81AAE-D248-4FF6-88A1-F476206DEE07" );
            // Attrib for BlockType: Group Member View:Group Member Edit Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8740BC0E-4704-4047-9673-748107586E22", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Member Edit Page", "GroupMemberEditPage", "Group Member Edit Page", @"The page that will allow editing of a group member.", 0, @"", "919967CA-A702-4B34-9B74-5EFAA863C37F" );
            // Attrib for BlockType: Group Member View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8740BC0E-4704-4047-9673-748107586E22", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 1, @"", "63979A6A-AF6A-44CA-AC7A-24D03393D375" );
            // Attrib for BlockType: Group View:Group Edit Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0617CF60-017F-4840-AC1F-16D920C35175", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Edit Page", "GroupEditPage", "Group Edit Page", @"The page that will allow editing of the group.", 0, @"", "838AD553-7CCB-4976-AE25-EE679350E651" );
            // Attrib for BlockType: Group View:Show Leader List
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0617CF60-017F-4840-AC1F-16D920C35175", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Leader List", "ShowLeaderList", "Show Leader List", @"Specifies if the leader list should be shown, this value is made available to the Template as ShowLeaderList.", 1, @"True", "BA8226AB-3240-4F8D-BF86-9BF1F4D8A0E5" );
            // Attrib for BlockType: Group View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0617CF60-017F-4840-AC1F-16D920C35175", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 2, @"", "1B8FA6F0-8953-40D0-8F86-3B269075988D" );
            // Attrib for BlockType: Prayer Request Details:Show Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B9C08FA0-296C-4779-B898-C701BD0747AE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Category", "EnableCategory", "Show Category", @"If disabled, then the user will not be able to select a category and the default category will be used exclusively.", 0, @"True", "E976DCD2-F576-4A71-BFFC-61DB866C871F" );
            // Attrib for BlockType: Prayer Request Details:Parent Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B9C08FA0-296C-4779-B898-C701BD0747AE", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Parent Category", "ParentCategory", "Parent Category", @"A top level category. This controls which categories the person can choose from when entering their prayer request.", 1, @"", "086C103A-62EA-4E81-9B39-2E67ED70BC1D" );
            // Attrib for BlockType: Prayer Request Details:Default Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B9C08FA0-296C-4779-B898-C701BD0747AE", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Default Category", "DefaultCategory", "Default Category", @"The default category to use for all new prayer requests.", 2, @"", "D53AC043-A398-4155-8099-245F81E284E2" );
            // Attrib for BlockType: Prayer Request Details:Enable Auto Approve
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B9C08FA0-296C-4779-B898-C701BD0747AE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Auto Approve", "EnableAutoApprove", "Enable Auto Approve", @"If enabled, prayer requests are automatically approved; otherwise they must be approved by an admin before they can be seen by the prayer team.", 0, @"True", "4168FC31-3B74-4431-8444-C90A5FD876D5" );
            // Attrib for BlockType: Prayer Request Details:Expires After (Days)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B9C08FA0-296C-4779-B898-C701BD0747AE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Expires After (Days)", "ExpiresAfterDays", "Expires After (Days)", @"Number of days until the request will expire (only applies when auto-approved is enabled).", 1, @"14", "409D90E6-3EAF-413E-B658-0BA83C01B85A" );
            // Attrib for BlockType: Prayer Request Details:Show Urgent Flag
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B9C08FA0-296C-4779-B898-C701BD0747AE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Urgent Flag", "EnableUrgentFlag", "Show Urgent Flag", @"If enabled, requestors will be able to flag prayer requests as urgent.", 2, @"False", "998B3087-D0A9-43F2-9E6C-815124072253" );
            // Attrib for BlockType: Prayer Request Details:Show Public Display Flag
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B9C08FA0-296C-4779-B898-C701BD0747AE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Public Display Flag", "EnablePublicDisplayFlag", "Show Public Display Flag", @"If enabled, requestors will be able set whether or not they want their request displayed on the public website.", 3, @"False", "AD1D018C-3809-4421-8F02-6AD403826C6F" );
            // Attrib for BlockType: Prayer Request Details:Default To Public
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B9C08FA0-296C-4779-B898-C701BD0747AE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default To Public", "DefaultToPublic", "Default To Public", @"If enabled, all prayers will be set to public by default.", 4, @"False", "5638FE4D-FA4C-4174-823D-7CE58C1D7632" );
            // Attrib for BlockType: Prayer Request Details:Character Limit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B9C08FA0-296C-4779-B898-C701BD0747AE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Character Limit", "CharacterLimit", "Character Limit", @"If set to something other than 0, this will limit the number of characters allowed when entering a new prayer request.", 5, @"250", "691E3439-4F12-4C2A-ADCE-8DB3B6F0AAE4" );
            // Attrib for BlockType: Prayer Request Details:Show Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B9C08FA0-296C-4779-B898-C701BD0747AE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus", "EnableCampus", "Show Campus", @"Should the campus field be displayed? If there is only one active campus then the campus field will not show.", 6, @"True", "193C5739-472F-42DC-A5F4-17EBB61CED4E" );
            // Attrib for BlockType: Prayer Request Details:Require Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B9C08FA0-296C-4779-B898-C701BD0747AE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Campus", "RequireCampus", "Require Campus", @"Require that a campus be selected. The campus will not be displayed if there is only one available campus, in which case if this is set to true then the single campus is automatically used.", 7, @"False", "0E4D59E5-9B35-4895-84D1-F8C8396699A5" );
            // Attrib for BlockType: Prayer Request Details:Require Last Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B9C08FA0-296C-4779-B898-C701BD0747AE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Last Name", "RequireLastName", "Require Last Name", @"Require that a last name be entered. First name is always required.", 8, @"True", "5EC2370E-5BD3-4CA6-942E-55ACD054A7D4" );
            // Attrib for BlockType: Prayer Request Details:Enable Person Matching
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B9C08FA0-296C-4779-B898-C701BD0747AE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Person Matching", "EnablePersonMatching", "Enable Person Matching", @"If enabled, the request will be linked to an existing person if a match can be made between the requester and an existing person.", 9, @"False", "0F745407-5D85-463A-94CB-A35ADF645A50" );
            // Attrib for BlockType: Prayer Request Details:Completion Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B9C08FA0-296C-4779-B898-C701BD0747AE", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Completion Action", "CompletionAction", "Completion Action", @"What action to perform after saving the prayer request.", 0, @"0", "8472F1D8-71B0-4955-8855-7AE1CA20C4AF" );
            // Attrib for BlockType: Prayer Request Details:Completion Xaml
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B9C08FA0-296C-4779-B898-C701BD0747AE", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Completion Xaml", "CompletionXaml", "Completion Xaml", @"The XAML markup that will be used if the. <span class='tip tip-lava'></span>", 1, @"<Rock:NotificationBox NotificationType=""Success"">
    Thank you for allowing us to pray for you.
</Rock:NotificationBox>", "C022741C-CF60-4EE2-B32B-68622749937A" );
            // Attrib for BlockType: Prayer Request Details:Workflow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B9C08FA0-296C-4779-B898-C701BD0747AE", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "Workflow", @"An optional workflow to start when prayer request is created. The PrayerRequest will be set as the workflow 'Entity' attribute when processing is started.", 2, @"", "839CE48F-B05C-4034-BCA1-C14DEE132397" );
            // Attrib for BlockType: Communication Entry:Enable Lava
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D9834641-7F39-4CFA-8CB2-E64068127565", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Lava", "EnableLava", "Enable Lava", @"Remove the lava syntax from the message without resolving it.", 0, @"False", "FB5CCE40-EAC5-4FA5-9E90-89936CA272E6" );
            // Attrib for BlockType: Email Preference Entry:Unsubscribe from List Workflow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B3C076C7-1325-4453-9549-456C23702069", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Unsubscribe from List Workflow", "UnsubscribeWorkflow", "Unsubscribe from List Workflow", @"The workflow type to launch for person who wants to unsubscribe from one or more Communication Lists. The person will be passed in as the Entity and the communication list Ids will be passed as a comma delimited string to the workflow 'CommunicationListIds' attribute if it exists.", 0, @"", "0324F168-F64D-41B3-B9BA-56AADFE89E69" );
            // Attrib for BlockType: Connection Opportunity Signup:Exclude Non-Public Connection Request Attributes
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C7FCE3B7-704B-43C0-AF96-5A70EB7F70D9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Exclude Non-Public Connection Request Attributes", "ExcludeNonPublicAttributes", "Exclude Non-Public Connection Request Attributes", @"Attributes without 'Public' checked will not be displayed.", 10, @"True", "798A0AD7-13DA-4416-B89A-DABFFD413485" );
            // Attrib for BlockType: My Connection Opportunities:Enable Request Security
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3F69E04F-F966-4CAE-B89D-F97DFEF6407A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Request Security", "EnableRequestSecurity", "Enable Request Security", @"When enabled, the the security column for request would be displayed.", 8, @"False", "A67A9FB5-4B06-4EDE-BEC0-A60821A656D8" );
            // Attrib for BlockType: Group Schedule Toolbox:Decline Reason Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7F9CEA6F-DCE5-4F60-A551-924965289F1D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Decline Reason Page", "DeclineReasonPage", "Decline Reason Page", @"If the group type has enabled 'RequiresReasonIfDeclineSchedule' then specify the page to provide that reason here.", 0, @"EA14B522-E2A6-4CA7-8AF0-9CDF0B84C8CF", "5A6896C7-7838-4ADE-BAD3-F8EEB816E4B1" );
            // Attrib for BlockType: Checkr Request List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "53A28B56-B7B4-472C-9305-1DC66693A6C6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "73963958-4953-4F5B-9B5C-0D3CD586C6D2" );
            // Attrib for BlockType: Checkr Request List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "53A28B56-B7B4-472C-9305-1DC66693A6C6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "2AE30B01-0A9D-47AB-A498-14D9F4B516D5" );
            // Attrib for BlockType: Achievement Attempt List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9C1236AE-4FF0-480C-A7DF-0E5277CA75FB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "11B322C3-D2E3-4F0E-9C54-3A553AEC1950" );
            // Attrib for BlockType: Achievement Attempt List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9C1236AE-4FF0-480C-A7DF-0E5277CA75FB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "C30A9672-D368-4747-BBB1-F1DE3A383131" );
            // Attrib for BlockType: Achievement Type List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D2F918CF-F63C-4643-A02A-FD3DE7C5CFFD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "B4D7FB74-6D83-4415-A0E2-727BFDF37F97" );
            // Attrib for BlockType: Achievement Type List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D2F918CF-F63C-4643-A02A-FD3DE7C5CFFD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "03889099-AB94-4237-BD39-D0D814304612" );
            RockMigrationHelper.UpdateFieldType("Connection Request Activity","","Rock","Rock.Field.Types.ConnectionRequestActivityFieldType","10842787-7C17-413A-A562-9CA19E6FCE52");
        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Achievement Type List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("03889099-AB94-4237-BD39-D0D814304612");
            // Attrib for BlockType: Achievement Type List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("B4D7FB74-6D83-4415-A0E2-727BFDF37F97");
            // Attrib for BlockType: Achievement Attempt List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("C30A9672-D368-4747-BBB1-F1DE3A383131");
            // Attrib for BlockType: Achievement Attempt List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("11B322C3-D2E3-4F0E-9C54-3A553AEC1950");
            // Attrib for BlockType: Checkr Request List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("2AE30B01-0A9D-47AB-A498-14D9F4B516D5");
            // Attrib for BlockType: Checkr Request List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("73963958-4953-4F5B-9B5C-0D3CD586C6D2");
            // Attrib for BlockType: Group Schedule Toolbox:Decline Reason Page
            RockMigrationHelper.DeleteAttribute("5A6896C7-7838-4ADE-BAD3-F8EEB816E4B1");
            // Attrib for BlockType: My Connection Opportunities:Enable Request Security
            RockMigrationHelper.DeleteAttribute("A67A9FB5-4B06-4EDE-BEC0-A60821A656D8");
            // Attrib for BlockType: Connection Opportunity Signup:Exclude Non-Public Connection Request Attributes
            RockMigrationHelper.DeleteAttribute("798A0AD7-13DA-4416-B89A-DABFFD413485");
            // Attrib for BlockType: Email Preference Entry:Unsubscribe from List Workflow
            RockMigrationHelper.DeleteAttribute("0324F168-F64D-41B3-B9BA-56AADFE89E69");
            // Attrib for BlockType: Communication Entry:Enable Lava
            RockMigrationHelper.DeleteAttribute("FB5CCE40-EAC5-4FA5-9E90-89936CA272E6");
            // Attrib for BlockType: Prayer Request Details:Workflow
            RockMigrationHelper.DeleteAttribute("839CE48F-B05C-4034-BCA1-C14DEE132397");
            // Attrib for BlockType: Prayer Request Details:Completion Xaml
            RockMigrationHelper.DeleteAttribute("C022741C-CF60-4EE2-B32B-68622749937A");
            // Attrib for BlockType: Prayer Request Details:Completion Action
            RockMigrationHelper.DeleteAttribute("8472F1D8-71B0-4955-8855-7AE1CA20C4AF");
            // Attrib for BlockType: Prayer Request Details:Enable Person Matching
            RockMigrationHelper.DeleteAttribute("0F745407-5D85-463A-94CB-A35ADF645A50");
            // Attrib for BlockType: Prayer Request Details:Require Last Name
            RockMigrationHelper.DeleteAttribute("5EC2370E-5BD3-4CA6-942E-55ACD054A7D4");
            // Attrib for BlockType: Prayer Request Details:Require Campus
            RockMigrationHelper.DeleteAttribute("0E4D59E5-9B35-4895-84D1-F8C8396699A5");
            // Attrib for BlockType: Prayer Request Details:Show Campus
            RockMigrationHelper.DeleteAttribute("193C5739-472F-42DC-A5F4-17EBB61CED4E");
            // Attrib for BlockType: Prayer Request Details:Character Limit
            RockMigrationHelper.DeleteAttribute("691E3439-4F12-4C2A-ADCE-8DB3B6F0AAE4");
            // Attrib for BlockType: Prayer Request Details:Default To Public
            RockMigrationHelper.DeleteAttribute("5638FE4D-FA4C-4174-823D-7CE58C1D7632");
            // Attrib for BlockType: Prayer Request Details:Show Public Display Flag
            RockMigrationHelper.DeleteAttribute("AD1D018C-3809-4421-8F02-6AD403826C6F");
            // Attrib for BlockType: Prayer Request Details:Show Urgent Flag
            RockMigrationHelper.DeleteAttribute("998B3087-D0A9-43F2-9E6C-815124072253");
            // Attrib for BlockType: Prayer Request Details:Expires After (Days)
            RockMigrationHelper.DeleteAttribute("409D90E6-3EAF-413E-B658-0BA83C01B85A");
            // Attrib for BlockType: Prayer Request Details:Enable Auto Approve
            RockMigrationHelper.DeleteAttribute("4168FC31-3B74-4431-8444-C90A5FD876D5");
            // Attrib for BlockType: Prayer Request Details:Default Category
            RockMigrationHelper.DeleteAttribute("D53AC043-A398-4155-8099-245F81E284E2");
            // Attrib for BlockType: Prayer Request Details:Parent Category
            RockMigrationHelper.DeleteAttribute("086C103A-62EA-4E81-9B39-2E67ED70BC1D");
            // Attrib for BlockType: Prayer Request Details:Show Category
            RockMigrationHelper.DeleteAttribute("E976DCD2-F576-4A71-BFFC-61DB866C871F");
            // Attrib for BlockType: Group View:Template
            RockMigrationHelper.DeleteAttribute("1B8FA6F0-8953-40D0-8F86-3B269075988D");
            // Attrib for BlockType: Group View:Show Leader List
            RockMigrationHelper.DeleteAttribute("BA8226AB-3240-4F8D-BF86-9BF1F4D8A0E5");
            // Attrib for BlockType: Group View:Group Edit Page
            RockMigrationHelper.DeleteAttribute("838AD553-7CCB-4976-AE25-EE679350E651");
            // Attrib for BlockType: Group Member View:Template
            RockMigrationHelper.DeleteAttribute("63979A6A-AF6A-44CA-AC7A-24D03393D375");
            // Attrib for BlockType: Group Member View:Group Member Edit Page
            RockMigrationHelper.DeleteAttribute("919967CA-A702-4B34-9B74-5EFAA863C37F");
            // Attrib for BlockType: Group Member List:Additional Fields
            RockMigrationHelper.DeleteAttribute("F9E81AAE-D248-4FF6-88A1-F476206DEE07");
            // Attrib for BlockType: Group Member List:Template
            RockMigrationHelper.DeleteAttribute("528E2A27-6615-46CE-A5E3-36686E456DDB");
            // Attrib for BlockType: Group Member List:Title Template
            RockMigrationHelper.DeleteAttribute("8D93EFE8-C780-4B03-990B-5631ABAF9057");
            // Attrib for BlockType: Group Member List:Group Member Detail Page
            RockMigrationHelper.DeleteAttribute("F09F305E-C9F5-4CC1-8B8C-DCC051927A59");
            // Attrib for BlockType: Group Member Edit:Member Detail Page
            RockMigrationHelper.DeleteAttribute("73C62B55-8735-4175-A2CF-817C30CEE88A");
            // Attrib for BlockType: Group Member Edit:Attribute Category
            RockMigrationHelper.DeleteAttribute("8EFA540A-D4DE-4A84-9EC5-798320DC72E3");
            // Attrib for BlockType: Group Member Edit:Allow Note Edit
            RockMigrationHelper.DeleteAttribute("28256F6B-D8D6-478F-B527-EC8DF0B6606B");
            // Attrib for BlockType: Group Member Edit:Allow Member Status Change
            RockMigrationHelper.DeleteAttribute("5CBA77A7-61A3-4EE5-B6FD-415229623CA1");
            // Attrib for BlockType: Group Member Edit:Allow Role Change
            RockMigrationHelper.DeleteAttribute("9881CAD7-9ADF-4D49-B57E-F471154350D5");
            // Attrib for BlockType: Group Edit:Group Detail Page
            RockMigrationHelper.DeleteAttribute("F6FD5634-3577-466F-B4F4-23D8393F9A09");
            // Attrib for BlockType: Group Edit:Attribute Category
            RockMigrationHelper.DeleteAttribute("28EC8ACA-0868-43BC-AEE7-714053CE6D0F");
            // Attrib for BlockType: Group Edit:Enable Public Status Edit
            RockMigrationHelper.DeleteAttribute("5B2A1026-8CC0-46F8-80B3-F724FB3AA122");
            // Attrib for BlockType: Group Edit:Show Public Status
            RockMigrationHelper.DeleteAttribute("C67B74BF-6C74-4179-ACDE-2FD6DB47FEB8");
            // Attrib for BlockType: Group Edit:Enable Active Status Edit
            RockMigrationHelper.DeleteAttribute("CE84FEBB-0DA2-48D2-BF7F-64962AC1E6F9");
            // Attrib for BlockType: Group Edit:Show Active Status
            RockMigrationHelper.DeleteAttribute("8EA4FFA0-D103-4E08-9A33-2774C1270CE1");
            // Attrib for BlockType: Group Edit:Enable Group Capacity Edit
            RockMigrationHelper.DeleteAttribute("60975D2A-5F60-4195-A20D-D8503A9E7139");
            // Attrib for BlockType: Group Edit:Show Group Capacity
            RockMigrationHelper.DeleteAttribute("EFF8DF79-9C24-4A4C-A076-B784959BA218");
            // Attrib for BlockType: Group Edit:Enable Campus Edit
            RockMigrationHelper.DeleteAttribute("FE73696C-50B3-442F-939A-A76F848B62F1");
            // Attrib for BlockType: Group Edit:Show Campus
            RockMigrationHelper.DeleteAttribute("51A1D1E2-FA15-49CC-A455-C0C9CA503948");
            // Attrib for BlockType: Group Edit:Enable Description Edit
            RockMigrationHelper.DeleteAttribute("CB660D2C-7BE5-45E3-8FCB-C1F2069F376C");
            // Attrib for BlockType: Group Edit:Show Description
            RockMigrationHelper.DeleteAttribute("AA7FC391-F0BD-4BA0-AB29-8B85AE57FA53");
            // Attrib for BlockType: Group Edit:Enable Group Name Edit
            RockMigrationHelper.DeleteAttribute("59AADEC9-E127-4FFE-8485-9CACD520A1FD");
            // Attrib for BlockType: Group Edit:Show Group Name
            RockMigrationHelper.DeleteAttribute("D96332EF-3BA2-4A07-87B8-8ADA71D374E5");
            // Attrib for BlockType: Group Attendance Entry:Allow Any Date Selection
            RockMigrationHelper.DeleteAttribute("0BF5CAD0-3BE2-4734-B369-AB3E9F37B03D");
            // Attrib for BlockType: Group Attendance Entry:Show Save Button
            RockMigrationHelper.DeleteAttribute("962E6C36-D053-4198-A232-3D47CE4A95F3");
            // Attrib for BlockType: Group Attendance Entry:Save Redirect Page
            RockMigrationHelper.DeleteAttribute("F33BE557-DC3D-4EF1-86ED-E92713661A50");
            // Attrib for BlockType: Group Attendance Entry:Number of Days Back to Allow
            RockMigrationHelper.DeleteAttribute("5B0E8292-25BB-4761-9E56-CAA0B6E1E946");
            // Attrib for BlockType: Group Attendance Entry:Number of Days Forward to Allow
            RockMigrationHelper.DeleteAttribute("7926A67F-3F01-42F8-BF93-39E1111861E1");
            // Attrib for BlockType: Prayer Session Setup:Show Campus Filter
            RockMigrationHelper.DeleteAttribute("5AB03E37-D2C7-47EE-A788-051BFF949133");
            // Attrib for BlockType: Prayer Session Setup:Parent Category
            RockMigrationHelper.DeleteAttribute("4A26D15B-807A-4B59-B166-7190F226B6DD");
            // Attrib for BlockType: Prayer Session Setup:Prayer Page
            RockMigrationHelper.DeleteAttribute("E0AF9F7B-DCDA-4423-B202-753648018F7A");
            // Attrib for BlockType: Prayer Session:Template
            RockMigrationHelper.DeleteAttribute("A4A09897-14FE-481F-B0A5-43C73B1B3721");
            // Attrib for BlockType: Prayer Session:Inappropriate Flag Limit
            RockMigrationHelper.DeleteAttribute("76779F3D-1754-4522-ABEA-32CE76496D02");
            // Attrib for BlockType: Prayer Session:Public Only
            RockMigrationHelper.DeleteAttribute("4E870E79-34BB-4A08-AD48-B524F91F22E9");
            // Attrib for BlockType: Prayer Session:Show Inappropriate Button
            RockMigrationHelper.DeleteAttribute("51B80681-89CD-4BDB-879F-FBB7B012B93F");
            // Attrib for BlockType: Prayer Session:Show Follow Button
            RockMigrationHelper.DeleteAttribute("25B5E9A4-D503-45F2-8912-FC0D655F6E9B");
            // Attrib for BlockType: Prayer Session:Prayed Button Text
            RockMigrationHelper.DeleteAttribute("1ACC691E-4000-4261-9732-7AEFB8B245F8");
            // Attrib for BlockType: Communication List Subscribe:Show Description
            RockMigrationHelper.DeleteAttribute("4B48BC6B-3754-498A-ADEC-D7782B188F8D");
            // Attrib for BlockType: Communication List Subscribe:Communication List Categories
            RockMigrationHelper.DeleteAttribute("C1F3E7E3-BB63-419A-A299-6FD8E12B08CF");
            // Attrib for BlockType: Calendar View:Show Filter
            RockMigrationHelper.DeleteAttribute("5FF22A0B-2A62-4C0E-B0AA-3A846AC2EAB5");
            // Attrib for BlockType: Calendar View:Event Summary
            RockMigrationHelper.DeleteAttribute("AA2E281E-FD07-4B6E-956A-7018E7CAD292");
            // Attrib for BlockType: Calendar View:Audience Filter
            RockMigrationHelper.DeleteAttribute("1C40411B-080E-4843-9D6D-24578EB0F168");
            // Attrib for BlockType: Calendar View:Detail Page
            RockMigrationHelper.DeleteAttribute("B3A6468B-2B56-44B0-BF44-CE04B0C8ABF8");
            // Attrib for BlockType: Calendar View:Calendar
            RockMigrationHelper.DeleteAttribute("83ADA17D-8BEE-48A6-A43A-46AE51224460");
            // Attrib for BlockType: Calendar Event List:Day Header Template
            RockMigrationHelper.DeleteAttribute("1C01F417-9F4F-4C5E-81FF-B8B5CAF7F678");
            // Attrib for BlockType: Calendar Event List:Event Template
            RockMigrationHelper.DeleteAttribute("0F2CAE76-9500-4B41-93AC-7351071F0BC3");
            // Attrib for BlockType: Calendar Event List:Detail Page
            RockMigrationHelper.DeleteAttribute("251A2B03-1D2A-43C0-A4CD-4F015291A9AD");
            // Attrib for BlockType: Calendar Event List:Calendar
            RockMigrationHelper.DeleteAttribute("2DB08B4B-D3A7-45FD-87D9-1BB78F2DC307");
            // Attrib for BlockType: Hero:Padding
            RockMigrationHelper.DeleteAttribute("9E7EC130-84B7-4C9A-B43E-4A4CCA491B57");
            // Attrib for BlockType: Hero:Subtitle Color
            RockMigrationHelper.DeleteAttribute("5499A384-FE0B-4FDF-8478-589B8CC5D1E4");
            // Attrib for BlockType: Hero:Title Color
            RockMigrationHelper.DeleteAttribute("FFF48796-9037-46F0-8A00-16B120264B39");
            // Attrib for BlockType: Hero:Text Align
            RockMigrationHelper.DeleteAttribute("E98C20DE-AFB0-4E10-8198-D187A49C4E4C");
            // Attrib for BlockType: Hero:Height - Tablet
            RockMigrationHelper.DeleteAttribute("1CD979FC-ED76-453E-BDE0-BE790E5E4F40");
            // Attrib for BlockType: Hero:Height - Phone
            RockMigrationHelper.DeleteAttribute("601C566C-0968-4D26-B1F2-813E4F12FD68");
            // Attrib for BlockType: Hero:Background Image - Tablet
            RockMigrationHelper.DeleteAttribute("0BDBE3CB-525A-4EA4-BD29-29357DD73580");
            // Attrib for BlockType: Hero:Background Image - Phone
            RockMigrationHelper.DeleteAttribute("BE1D377F-08FA-48CC-AD4C-40F81565643A");
            // Attrib for BlockType: Hero:Subtitle
            RockMigrationHelper.DeleteAttribute("4F233235-608F-482F-BBC5-7AFFDE6A67C8");
            // Attrib for BlockType: Hero:Title
            RockMigrationHelper.DeleteAttribute("62C96016-C476-46B1-BB5C-94745548BD99");
            RockMigrationHelper.DeleteBlockType("B9C08FA0-296C-4779-B898-C701BD0747AE"); // Prayer Request Details
            RockMigrationHelper.DeleteBlockType("0617CF60-017F-4840-AC1F-16D920C35175"); // Group View
            RockMigrationHelper.DeleteBlockType("8740BC0E-4704-4047-9673-748107586E22"); // Group Member View
            RockMigrationHelper.DeleteBlockType("BE6043E9-BD74-417A-86B5-7C31EECB8078"); // Group Member List
            RockMigrationHelper.DeleteBlockType("5AB2334B-5EE9-46E9-AE8D-C3039BF8BE24"); // Group Member Edit
            RockMigrationHelper.DeleteBlockType("DCBA7095-9619-4199-9F04-926CFB3A5125"); // Group Edit
            RockMigrationHelper.DeleteBlockType("56B67C32-63BC-4ECE-A0CC-332D7EF56E75"); // Group Attendance Entry
            RockMigrationHelper.DeleteBlockType("55604B68-F12A-446D-8BA9-F44580A7C425"); // Prayer Session Setup
            RockMigrationHelper.DeleteBlockType("39FD7EAD-A4C9-41C4-8E0D-5EF301691DBC"); // Prayer Session
            RockMigrationHelper.DeleteBlockType("E7976940-2622-47E0-85EA-AFDD189C8CB3"); // Communication List Subscribe
            RockMigrationHelper.DeleteBlockType("D7F829CC-941C-43BD-AA65-FFA5F8A232F1"); // Calendar View
            RockMigrationHelper.DeleteBlockType("C2E0A17C-C4D0-4178-BE62-7818AB154B27"); // Calendar Event List
            RockMigrationHelper.DeleteBlockType("A8D6116C-BB8A-49DD-BEDA-12FCEA7BE2FB"); // Hero
        }
    
        /// <summary>
        /// MB: Remove Communication Preference Attribute from Communication List.
        /// </summary>
        private void RemoveCommunicationPreference()
        {
            Sql( @"
                DECLARE @attributeId AS INT
                SELECT @attributeId = Id
                FROM [dbo].[Attribute] 
                WHERE [Guid] = 'D7941908-1F65-CC9B-416C-CCFABE4221B9'

                IF @attributeId IS NOT NULL
                BEGIN
	                UPDATE GroupMember
	                SET GroupMember.CommunicationPreference = CASE 
		                WHEN ValueAsNumeric = 1 THEN 1
		                WHEN ValueAsNumeric = 2 THEN 2
		                ELSE 0 END
	                FROM [dbo].[AttributeValue] 
	                WHERE AttributeId = @attributeId 
			                AND [AttributeValue].EntityId = GroupMember.Id
			                AND GroupMember.CommunicationPreference <> ValueAsNumeric

	                WHILE (SELECT COUNT(1) FROM [dbo].[AttributeValue]
			                WHERE AttributeId = @attributeId) > 0
	                BEGIN
		                DELETE TOP(100000) [dbo].[AttributeValue]
		                WHERE AttributeId = @attributeId
	                END
	
	                DELETE [dbo].[Attribute]
	                WHERE Id = @attributeId
                END" );
        }

        /// <summary>
        /// ED: Update Email Medium Attribute NonHtmlContent
        /// </summary>
        private void UpdateEmailMediumAttributeNonHtmlContent()
        {
            Sql( @"
                UPDATE [Attribute]
                SET [DefaultValue] = REPLACE([DefaultValue], 'GetCommunication.ashx?c={{ Communication.Id }}', 'GetCommunication.ashx?c={{ Communication.Guid }}')
                WHERE [Guid] = 'FDB3E4EB-DE16-4A43-AE92-B4EAA3D5DF88'
                UPDATE [AttributeValue]
                SET [Value] = REPLACE([Value], 'GetCommunication.ashx?c={{ Communication.Id }}', 'GetCommunication.ashx?c={{ Communication.Guid }}')
                WHERE [AttributeId] IN (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'FDB3E4EB-DE16-4A43-AE92-B4EAA3D5DF88')" );
        }

        #region MB: Update CheckIn Success Message

        private string GetNewLavaTemplate()
        {
            return @"<ol class='checkin-summary checkin-body-container'>
                {% for checkinResult in CheckinResultList %}
                    {% if RegistrationModeEnabled == true %}
                        <li>{{ checkinResult.DetailMessage }}</li>
                    {% else %}
                        <li>{{ checkinResult.DetailMessage }}</li>
                    {% endif %}
                {% endfor %}
            </ol>
            <ol class='checkin-error'>
                {% comment %}Display any error messages from the label printer{% endcomment %}
                {% for message in ZebraPrintMessageList %}
                    <li>{{ message }}</li>
                {% endfor %}
            </ol>";
        }

        private string GetPreviousLavaTemplate()
        {
            return @"<ol class=""checkin-summary checkin-body-container"">

            {% for checkinResult in CheckinResultList %}
                {% if RegistrationModeEnabled == true %}
                    <li>{{ checkinResult.DetailMessage }}</li>
                {% else %}
                    <li>{{ checkinResult.DetailMessage }}</li>
                {% endif %}
            {% endfor %}

            {% comment %}Display any error messages from the label printer{% endcomment %}
            {% for message in ZebraPrintMessageList %}
                <br/>{{ message }}
            {% endfor %}

            </ol>";
        }

        /// <summary>
        /// MB: Update CheckIn Success Message
        /// </summary>
        private void UpdateCheckinSuccessMessage()
        {
            var newLavaTemplate = GetNewLavaTemplate();
            var previousLavaTemplate = GetPreviousLavaTemplate();

            newLavaTemplate = newLavaTemplate.Replace( "'", "''" );
            previousLavaTemplate = previousLavaTemplate.Replace( "'", "''" );

            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "DefaultValue" );

            Sql( $@"UPDATE [dbo].[Attribute] 
                    SET [DefaultValue] = REPLACE({targetColumn}, '{previousLavaTemplate}', '{newLavaTemplate}')
                    WHERE {targetColumn} LIKE '%{previousLavaTemplate}%'
                            AND [Key] = 'core_checkin_SuccessLavaTemplate'" );
        }

        #endregion MB: Update CheckIn Success Message

        #region DH: Mobile Prayer and Event List block templates

        /// <summary>
        /// Updates the Defined Value of the event list template.
        /// </summary>
        private void UpdateEventListTemplateType()
        {
            RockMigrationHelper.UpdateDefinedValue(
                Rock.SystemGuid.DefinedType.TEMPLATE_BLOCK,
                "Mobile Calendar Event List",
                string.Empty,
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CALENDAR_EVENT_LIST );
        }

        /// <summary>
        /// Updates the Defined Value of the prayer session template.
        /// </summary>
        private void UpdatePrayerSessionTemplateType()
        {
            RockMigrationHelper.UpdateDefinedValue(
                Rock.SystemGuid.DefinedType.TEMPLATE_BLOCK,
                "Mobile Prayer Session",
                string.Empty,
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_PRAYER_SESSION );
        }

        #endregion DH: Mobile Prayer and Event List block templates
    }
}
