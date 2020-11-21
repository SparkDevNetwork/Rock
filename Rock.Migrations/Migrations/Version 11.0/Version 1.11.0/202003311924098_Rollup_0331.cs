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
    public partial class Rollup_0331 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            FixChartShortcode();
            GroupMemberNotificationSystemEmail();
            StructuredContentTools();
            CommunicationEntryEnablePersonParam();
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
            RockMigrationHelper.UpdateMobileBlockType("Hero", "Displays an image with text overlay on the page.", "Rock.Blocks.Types.Mobile.Cms.Hero", "Mobile > Cms", "F3B7A306-9B73-4E20-8BF6-16390807EADD");
            RockMigrationHelper.UpdateMobileBlockType("Calendar Event List", "Displays a list of events from a calendar.", "Rock.Blocks.Types.Mobile.Events.CalendarEventList", "Mobile > Events", "88BC5341-6574-427D-B48C-3BEB4D73CC1C");
            RockMigrationHelper.UpdateMobileBlockType("Calendar View", "Views events from a calendar.", "Rock.Blocks.Types.Mobile.Events.CalendarView", "Mobile > Events", "88980765-A47F-4D7A-BFE7-BD0536991B05");
            RockMigrationHelper.UpdateMobileBlockType("Communication List Subscribe", "Allows the user to subscribe or unsubscribe from specific communication lists.", "Rock.Blocks.Types.Mobile.Events.CommunicationListSubscribe", "Mobile > Communication", "4E1AA59A-EFEC-4A0F-B159-80F32E107558");
            RockMigrationHelper.UpdateMobileBlockType("Prayer Session", "Allows the user to read through and pray for prayer requests.", "Rock.Blocks.Types.Mobile.Events.PrayerSession", "Mobile > Prayer", "5488C3EB-EC76-44FB-B3CE-EDBA163341FF");
            RockMigrationHelper.UpdateMobileBlockType("Prayer Session Setup", "Displays a page to configure and prepare a prayer session.", "Rock.Blocks.Types.Mobile.Events.PrayerSessionSetup", "Mobile > Prayer", "5DC5967E-71E3-484D-A330-BF3CCE9C8406");
            RockMigrationHelper.UpdateMobileBlockType("Group Attendance Entry", "Allows the user to mark attendance for a group.", "Rock.Blocks.Types.Mobile.Groups.GroupAttendanceEntry", "Mobile > Groups", "1F8308AE-7A1B-4695-8442-DCF6FBFCAB7A");
            RockMigrationHelper.UpdateMobileBlockType("Group Edit", "Edits the basic settings of a group.", "Rock.Blocks.Types.Mobile.Groups.GroupEdit", "Mobile > Groups", "A6818D93-FB51-48BA-83BD-404B95CF2F8B");
            RockMigrationHelper.UpdateMobileBlockType("Group Member Edit", "Edits a member of a group.", "Rock.Blocks.Types.Mobile.Groups.GroupMemberEdit", "Mobile > Groups", "E2365240-F3FF-446A-83B0-3466478027E7");
            RockMigrationHelper.UpdateMobileBlockType("Group Member List", "Allows the user to view a list of members in a group.", "Rock.Blocks.Types.Mobile.Groups.GroupMemberList", "Mobile > Groups", "C7A85634-58E3-4D67-A22A-B035410B87A5");
            RockMigrationHelper.UpdateMobileBlockType("Group Member View", "Allows the user to view the details about a specific group member.", "Rock.Blocks.Types.Mobile.Groups.GroupMemberView", "Mobile > Groups", "07080D7A-6407-4F58-BDBF-FD273EAF6C8C");
            RockMigrationHelper.UpdateMobileBlockType("Group View", "Allows the user to view the details about a group.", "Rock.Blocks.Types.Mobile.Groups.GroupView", "Mobile > Groups", "A3AF2D06-D87F-485F-ABE4-3C0B1C52A57B");
            RockMigrationHelper.UpdateMobileBlockType("Prayer Request Details", "Edits an existing prayer request or creates a new one.", "Rock.Blocks.Types.Mobile.Prayer.PrayerRequestDetails", "Mobile > Prayer", "9FB75DC3-3C00-46C0-AE2A-37BA52A58E65");
            // Attrib for BlockType: Hero:Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F3B7A306-9B73-4E20-8BF6-16390807EADD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "Title", @"The main title to display over the image. <span class='tip tip-lava'></span>", 0, @"", "9614C5B8-3FAD-432E-922A-F556ACF8E06E" );
            // Attrib for BlockType: Hero:Subtitle
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F3B7A306-9B73-4E20-8BF6-16390807EADD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subtitle", "Subtitle", "Subtitle", @"The subtitle to display over the image. <span class='tip tip-lava'></span>", 1, @"", "3BFDB45E-B9CB-40D8-B265-54688CA79D57" );
            // Attrib for BlockType: Hero:Background Image - Phone
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F3B7A306-9B73-4E20-8BF6-16390807EADD", "6F9E2DD0-E39E-4602-ADF9-EB710A75304A", "Background Image - Phone", "BackgroundImagePhone", "Background Image - Phone", @"Recommended size is at least 1024px wide and double the height specified below.", 2, @"", "721AE421-60FB-4EB8-9DB4-5F4ED7B0AA2D" );
            // Attrib for BlockType: Hero:Background Image - Tablet
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F3B7A306-9B73-4E20-8BF6-16390807EADD", "6F9E2DD0-E39E-4602-ADF9-EB710A75304A", "Background Image - Tablet", "BackgroundImageTablet", "Background Image - Tablet", @"Recommended size is at least 2048px wide and double the height specified below.", 3, @"", "783735BB-9854-49E0-B79A-6F2F3241849E" );
            // Attrib for BlockType: Hero:Height - Phone
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F3B7A306-9B73-4E20-8BF6-16390807EADD", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Height - Phone", "ImageHeightPhone", "Height - Phone", @"", 4, @"200", "D7454FEC-0AEA-4EF2-AE44-970899DF0D2A" );
            // Attrib for BlockType: Hero:Height - Tablet
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F3B7A306-9B73-4E20-8BF6-16390807EADD", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Height - Tablet", "ImageHeightTablet", "Height - Tablet", @"", 5, @"350", "7BC408D8-1797-455E-8765-B612A3A913F8" );
            // Attrib for BlockType: Hero:Text Align
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F3B7A306-9B73-4E20-8BF6-16390807EADD", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Text Align", "HorizontalTextAlign", "Text Align", @"", 6, @"Center", "67D9419D-3A1E-4D41-BB47-43BDFBA01484" );
            // Attrib for BlockType: Hero:Title Color
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F3B7A306-9B73-4E20-8BF6-16390807EADD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title Color", "TitleColor", "Title Color", @"Will override the theme's hero title (.hero-title) color.", 7, @"", "6F60ECF1-EE6E-4D67-BF76-37DAA6FE3A24" );
            // Attrib for BlockType: Hero:Subtitle Color
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F3B7A306-9B73-4E20-8BF6-16390807EADD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subtitle Color", "SubtitleColor", "Subtitle Color", @"Will override the theme's hero subtitle (.hero-subtitle) color.", 8, @"", "B51E7458-7D74-4656-9500-444A801B25F0" );
            // Attrib for BlockType: Hero:Padding
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F3B7A306-9B73-4E20-8BF6-16390807EADD", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Padding", "Padding", "Padding", @"The padding around the inside of the image.", 9, @"20", "C62FE9BF-BA0B-4479-808D-72D2BC29D217" );
            // Attrib for BlockType: Calendar Event List:Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "88BC5341-6574-427D-B48C-3BEB4D73CC1C", "EC0D9528-1A22-404E-A776-566404987363", "Calendar", "Calendar", "Calendar", @"The calendar to pull events from", 0, @"", "BBF1879B-1116-4558-9437-EF66E21E4F34" );
            // Attrib for BlockType: Calendar Event List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "88BC5341-6574-427D-B48C-3BEB4D73CC1C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to push onto the navigation stack when viewing details of an event.", 1, @"", "3170A2A8-9E4F-4C71-9950-E7A419C0F3D8" );
            // Attrib for BlockType: Calendar Event List:Event Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "88BC5341-6574-427D-B48C-3BEB4D73CC1C", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Event Template", "EventTemplate", "Event Template", @"The template to use when rendering event items.", 2, @"", "37F8EA70-1983-4072-928A-356F2F4925EA" );
            // Attrib for BlockType: Calendar Event List:Day Header Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "88BC5341-6574-427D-B48C-3BEB4D73CC1C", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Day Header Template", "DayHeaderTemplate", "Day Header Template", @"The XAML to use when rendering the day header above a grouping of events.", 3, @"<Frame HasShadow=""false"" StyleClass=""calendar-events-day"">
    <Label Text=""{Binding ., StringFormat=""{0:dddd MMMM d}""}"" />
</Frame>
", "A172CBD9-C5A8-4164-AF14-10C532164F03" );
            // Attrib for BlockType: Calendar View:Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "88980765-A47F-4D7A-BFE7-BD0536991B05", "EC0D9528-1A22-404E-A776-566404987363", "Calendar", "Calendar", "Calendar", @"The calendar to pull events from", 0, @"", "12DB27FF-7A03-4DA3-99DC-B2F7DE41DD96" );
            // Attrib for BlockType: Calendar View:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "88980765-A47F-4D7A-BFE7-BD0536991B05", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to push onto the navigation stack when viewing details of an event.", 1, @"", "61AA5DE0-3097-479F-9302-5FB3CD76F4C3" );
            // Attrib for BlockType: Calendar View:Audience Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "88980765-A47F-4D7A-BFE7-BD0536991B05", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Audience Filter", "AudienceFilter", "Audience Filter", @"Determines which audiences should be displayed in the filter.", 2, @"", "2FB5BEA9-BF76-424B-B17B-8CB95F635B43" );
            // Attrib for BlockType: Calendar View:Event Summary
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "88980765-A47F-4D7A-BFE7-BD0536991B05", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Event Summary", "EventSummary", "Event Summary", @"The XAML to use when rendering the event summaries below the calendar.", 3, @"<Frame HasShadow=""false"" StyleClass=""calendar-event"">
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
", "FB0CFD3B-97B7-424C-AF00-4AB63E7CFFC6" );
            // Attrib for BlockType: Calendar View:Show Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "88980765-A47F-4D7A-BFE7-BD0536991B05", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Filter", "ShowFilter", "Show Filter", @"If enabled then the user will be able to apply custom filtering.", 4, @"True", "8A5B4041-57C5-4B74-B368-7DE66A74C7CF" );
            // Attrib for BlockType: Communication List Subscribe:Communication List Categories
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4E1AA59A-EFEC-4A0F-B159-80F32E107558", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Communication List Categories", "CommunicationListCategories", "Communication List Categories", @"Select the categories of the communication lists to display, or select none to show all that the user is authorized to view.", 0, @"", "9C710F95-BF71-4DBB-BAEF-6180B5E9E473" );
            // Attrib for BlockType: Communication List Subscribe:Show Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4E1AA59A-EFEC-4A0F-B159-80F32E107558", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Description", "ShowDescription", "Show Description", @"If enabled then the description of the communication list will be shown.", 1, @"False", "32BC0E81-9654-4256-828D-865136F6C7F1" );
            // Attrib for BlockType: Prayer Session:Prayed Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5488C3EB-EC76-44FB-B3CE-EDBA163341FF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Prayed Button Text", "PrayedButtonText", "Prayed Button Text", @"The text to display inside the Prayed button. Available in the XAML template as lava variable 'PrayedButtonText'.", 0, @"I've Prayed", "086F2D73-6653-418D-A903-961BA8DAAA96" );
            // Attrib for BlockType: Prayer Session:Show Follow Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5488C3EB-EC76-44FB-B3CE-EDBA163341FF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Follow Button", "ShowFollowButton", "Show Follow Button", @"Indicates if the Follow button should be shown. Available in the XAML template as lava variable 'ShowFollowButton'.", 1, @"True", "2D7AE299-1806-4B49-BC92-3729EAB9FAA9" );
            // Attrib for BlockType: Prayer Session:Show Inappropriate Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5488C3EB-EC76-44FB-B3CE-EDBA163341FF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Inappropriate Button", "ShowInappropriateButton", "Show Inappropriate Button", @"Indicates if the button to flag a request as inappropriate should be shown. Available in the XAML template as lava variable 'ShowInappropriateButton'.", 2, @"True", "76F083A0-68FD-45FA-9018-27F468B47467" );
            // Attrib for BlockType: Prayer Session:Public Only
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5488C3EB-EC76-44FB-B3CE-EDBA163341FF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Public Only", "PublicOnly", "Public Only", @"If enabled then only prayer requests marked as public will be shown.", 3, @"False", "3F45287F-5456-4A15-A4E8-930C04DF3740" );
            // Attrib for BlockType: Prayer Session:Inappropriate Flag Limit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5488C3EB-EC76-44FB-B3CE-EDBA163341FF", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Inappropriate Flag Limit", "InappropriateFlagLimit", "Inappropriate Flag Limit", @"The number of flags a prayer request has to get from the prayer team before it is automatically unapproved.", 4, @"", "2B3B7CE2-4464-4EE9-8F02-D37987B760B4" );
            // Attrib for BlockType: Prayer Session:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5488C3EB-EC76-44FB-B3CE-EDBA163341FF", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering prayer requests.", 5, @"", "36DF8D73-4F49-4CCC-B646-07BA86BF2274" );
            // Attrib for BlockType: Prayer Session Setup:Prayer Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5DC5967E-71E3-484D-A330-BF3CCE9C8406", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Prayer Page", "PrayerPage", "Prayer Page", @"The page to push onto the navigation stack to begin the prayer session.", 0, @"", "E4BAF9B2-43EB-4B86-AD58-E174A79B43C6" );
            // Attrib for BlockType: Prayer Session Setup:Parent Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5DC5967E-71E3-484D-A330-BF3CCE9C8406", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Parent Category", "ParentCategory", "Parent Category", @"The parent category to use as the root category available for the user to pick from.", 1, @"", "07DF5E9C-34EC-45EA-8E9B-5EE7E98DDA7C" );
            // Attrib for BlockType: Prayer Session Setup:Show Campus Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5DC5967E-71E3-484D-A330-BF3CCE9C8406", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus Filter", "ShowCampusFilter", "Show Campus Filter", @"If enabled and the user has a primary campus, then the user will be offered to limit prayer requests to just their campus.", 2, @"False", "21A77A81-1D9E-454F-9E3C-BDC9575CC6C6" );
            // Attrib for BlockType: Group Attendance Entry:Number of Days Forward to Allow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1F8308AE-7A1B-4695-8442-DCF6FBFCAB7A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Days Forward to Allow", "NumberOfDaysForwardToAllow", "Number of Days Forward to Allow", @"", 0, @"0", "C469CB11-E064-40CF-A1CE-42D93C4AFD84" );
            // Attrib for BlockType: Group Attendance Entry:Number of Days Back to Allow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1F8308AE-7A1B-4695-8442-DCF6FBFCAB7A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Days Back to Allow", "NumberOfDaysBackToAllow", "Number of Days Back to Allow", @"", 1, @"30", "C6968BC8-F0B0-45A9-AAC2-11E4435CD6A3" );
            // Attrib for BlockType: Group Attendance Entry:Save Redirect Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1F8308AE-7A1B-4695-8442-DCF6FBFCAB7A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Save Redirect Page", "SaveRedirectPage", "Save Redirect Page", @"If set, redirect user to this page on save. If not set, page is popped off the navigation stack.", 2, @"", "9D48677F-D7B8-44C6-B9FB-A000EA81459E" );
            // Attrib for BlockType: Group Attendance Entry:Show Save Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1F8308AE-7A1B-4695-8442-DCF6FBFCAB7A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Save Button", "ShowSaveButton", "Show Save Button", @"If enabled a save button will be shown (recommended for large groups), otherwise no save button will be displayed and a save will be triggered with each selection (recommended for smaller groups).", 3, @"False", "8F87D311-D309-4C5A-BC04-64EBA46A6BC7" );
            // Attrib for BlockType: Group Attendance Entry:Allow Any Date Selection
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1F8308AE-7A1B-4695-8442-DCF6FBFCAB7A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Any Date Selection", "AllowAnyDateSelection", "Allow Any Date Selection", @"If enabled a date picker will be shown, otherwise a dropdown with only the valid dates will be shown.", 4, @"False", "6DE420C7-D07C-4FB4-A44B-8B25A72B9D4A" );
            // Attrib for BlockType: Group Edit:Show Group Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A6818D93-FB51-48BA-83BD-404B95CF2F8B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Name", "ShowGroupName", "Show Group Name", @"", 0, @"True", "8AD4569C-CC59-4C4F-8F0F-240599215671" );
            // Attrib for BlockType: Group Edit:Enable Group Name Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A6818D93-FB51-48BA-83BD-404B95CF2F8B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Group Name Edit", "EnableGroupNameEdit", "Enable Group Name Edit", @"", 1, @"True", "F9978A25-DA35-4E05-856A-EEAF7491A1C0" );
            // Attrib for BlockType: Group Edit:Show Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A6818D93-FB51-48BA-83BD-404B95CF2F8B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Description", "ShowDescription", "Show Description", @"", 2, @"True", "BFD327BA-1BD6-4CED-A75F-6BC1F30733A3" );
            // Attrib for BlockType: Group Edit:Enable Description Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A6818D93-FB51-48BA-83BD-404B95CF2F8B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Description Edit", "EnableDescriptionEdit", "Enable Description Edit", @"", 3, @"True", "BE6EB808-9CD2-4F40-BAE0-A9AF0F87DB4D" );
            // Attrib for BlockType: Group Edit:Show Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A6818D93-FB51-48BA-83BD-404B95CF2F8B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus", "ShowCampus", "Show Campus", @"", 4, @"True", "BEFF20AD-108F-4D5E-99EE-2187F668490F" );
            // Attrib for BlockType: Group Edit:Enable Campus Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A6818D93-FB51-48BA-83BD-404B95CF2F8B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Campus Edit", "EnableCampusEdit", "Enable Campus Edit", @"", 5, @"True", "534C93DB-EA75-474E-9552-4BE33CA95F2F" );
            // Attrib for BlockType: Group Edit:Show Group Capacity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A6818D93-FB51-48BA-83BD-404B95CF2F8B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Capacity", "ShowGroupCapacity", "Show Group Capacity", @"", 6, @"True", "6116C22C-B276-4D39-977B-FAAFD3D14E50" );
            // Attrib for BlockType: Group Edit:Enable Group Capacity Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A6818D93-FB51-48BA-83BD-404B95CF2F8B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Group Capacity Edit", "EnableGroupCapacityEdit", "Enable Group Capacity Edit", @"", 7, @"True", "4AF18B7A-3853-48D0-BFC3-B8E41A86F1C3" );
            // Attrib for BlockType: Group Edit:Show Active Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A6818D93-FB51-48BA-83BD-404B95CF2F8B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Active Status", "ShowActiveStatus", "Show Active Status", @"", 8, @"True", "3B85348E-D2D4-427B-9B36-7929CC9B7059" );
            // Attrib for BlockType: Group Edit:Enable Active Status Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A6818D93-FB51-48BA-83BD-404B95CF2F8B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Active Status Edit", "EnableActiveStatusEdit", "Enable Active Status Edit", @"", 9, @"True", "9C55CE57-444E-4544-A9BD-92FA44771F07" );
            // Attrib for BlockType: Group Edit:Show Public Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A6818D93-FB51-48BA-83BD-404B95CF2F8B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Public Status", "ShowPublicStatus", "Show Public Status", @"", 10, @"True", "825B8EC7-8AE2-4765-B72D-35F31190A329" );
            // Attrib for BlockType: Group Edit:Enable Public Status Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A6818D93-FB51-48BA-83BD-404B95CF2F8B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Public Status Edit", "EnablePublicStatusEdit", "Enable Public Status Edit", @"", 11, @"True", "91475885-4387-48F4-B5B8-796E790A55E2" );
            // Attrib for BlockType: Group Edit:Attribute Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A6818D93-FB51-48BA-83BD-404B95CF2F8B", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Attribute Category", "AttributeCategory", "Attribute Category", @"Category of attributes to show and allow editing on.", 12, @"", "5877C76A-0AF9-478E-A7F9-9EBAA4874D08" );
            // Attrib for BlockType: Group Edit:Group Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A6818D93-FB51-48BA-83BD-404B95CF2F8B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "Group Detail Page", @"The group detail page to return to, if not set then the edit page is popped off the navigation stack.", 13, @"", "047D604E-4A39-4F01-9F88-E9B2DDE91A46" );
            // Attrib for BlockType: Group Member Edit:Allow Role Change
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E2365240-F3FF-446A-83B0-3466478027E7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Role Change", "AllowRoleChange", "Allow Role Change", @"", 0, @"True", "6042D09F-EE19-4103-BFE7-301AE3A37ABC" );
            // Attrib for BlockType: Group Member Edit:Allow Member Status Change
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E2365240-F3FF-446A-83B0-3466478027E7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Member Status Change", "AllowMemberStatusChange", "Allow Member Status Change", @"", 1, @"True", "BD21661A-C1F0-4746-B772-57086C989834" );
            // Attrib for BlockType: Group Member Edit:Allow Note Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E2365240-F3FF-446A-83B0-3466478027E7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Note Edit", "AllowNoteEdit", "Allow Note Edit", @"", 2, @"True", "EF8BDC54-7223-4C6F-AD04-1CF29CF19858" );
            // Attrib for BlockType: Group Member Edit:Attribute Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E2365240-F3FF-446A-83B0-3466478027E7", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Attribute Category", "AttributeCategory", "Attribute Category", @"Category of attributes to show and allow editing on.", 3, @"", "06524E30-40D3-4A3F-A30F-50627381CB24" );
            // Attrib for BlockType: Group Member Edit:Member Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E2365240-F3FF-446A-83B0-3466478027E7", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Member Detail Page", "MemberDetailsPage", "Member Detail Page", @"The group member page to return to, if not set then the edit page is popped off the navigation stack.", 4, @"", "2BF7187C-7874-4982-8F2A-58E8989A9D24" );
            // Attrib for BlockType: Group Member List:Group Member Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C7A85634-58E3-4D67-A22A-B035410B87A5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Member Detail Page", "GroupMemberDetailPage", "Group Member Detail Page", @"The page that will display the group member details when selecting a member.", 0, @"", "E69D0625-0A5F-41DA-B32B-F8153883FA5E" );
            // Attrib for BlockType: Group Member List:Title Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C7A85634-58E3-4D67-A22A-B035410B87A5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title Template", "TitleTemplate", "Title Template", @"The value to use when rendering the title text. <span class='tip tip-lava'></span>", 1, @"{{ Group.Name }} Group Roster", "182C35D7-0B40-481A-A73A-A2C30D05129D" );
            // Attrib for BlockType: Group Member List:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C7A85634-58E3-4D67-A22A-B035410B87A5", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 2, @"", "4F71CDA9-D3E0-45AB-B890-F47DC132E795" );
            // Attrib for BlockType: Group Member List:Additional Fields
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C7A85634-58E3-4D67-A22A-B035410B87A5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Additional Fields", "AdditionalFields", "Additional Fields", @"", 3, @"", "6988CA4A-1372-40FE-BFD6-35A16E58D7F8" );
            // Attrib for BlockType: Group Member View:Group Member Edit Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "07080D7A-6407-4F58-BDBF-FD273EAF6C8C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Member Edit Page", "GroupMemberEditPage", "Group Member Edit Page", @"The page that will allow editing of a group member.", 0, @"", "EC03AE63-A41C-4767-87D8-C1558F1723F1" );
            // Attrib for BlockType: Group Member View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "07080D7A-6407-4F58-BDBF-FD273EAF6C8C", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 1, @"", "18FE73C7-2E38-4B24-9D28-19F8B8AC71D7" );
            // Attrib for BlockType: Group View:Group Edit Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A3AF2D06-D87F-485F-ABE4-3C0B1C52A57B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Edit Page", "GroupEditPage", "Group Edit Page", @"The page that will allow editing of the group.", 0, @"", "29DF7D94-168B-45C1-BF61-BBC1891E0732" );
            // Attrib for BlockType: Group View:Show Leader List
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A3AF2D06-D87F-485F-ABE4-3C0B1C52A57B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Leader List", "ShowLeaderList", "Show Leader List", @"Specifies if the leader list should be shown, this value is made available to the Template as ShowLeaderList.", 1, @"True", "7E24475D-5E67-49DF-B185-B5A5D549BB5C" );
            // Attrib for BlockType: Group View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A3AF2D06-D87F-485F-ABE4-3C0B1C52A57B", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 2, @"", "2132CC26-6517-49D7-910C-8792D84105E6" );
            // Attrib for BlockType: Prayer Request Details:Show Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FB75DC3-3C00-46C0-AE2A-37BA52A58E65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Category", "EnableCategory", "Show Category", @"If disabled, then the user will not be able to select a category and the default category will be used exclusively.", 0, @"True", "1CFD1678-C84A-4982-8ADD-8A80184B69E1" );
            // Attrib for BlockType: Prayer Request Details:Parent Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FB75DC3-3C00-46C0-AE2A-37BA52A58E65", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Parent Category", "ParentCategory", "Parent Category", @"A top level category. This controls which categories the person can choose from when entering their prayer request.", 1, @"", "699B0D26-FAFC-42CB-B34D-B4FF9A8595CB" );
            // Attrib for BlockType: Prayer Request Details:Default Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FB75DC3-3C00-46C0-AE2A-37BA52A58E65", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Default Category", "DefaultCategory", "Default Category", @"The default category to use for all new prayer requests.", 2, @"", "0C9168AE-D49C-4AF9-92FF-7A4572550E1F" );
            // Attrib for BlockType: Prayer Request Details:Enable Auto Approve
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FB75DC3-3C00-46C0-AE2A-37BA52A58E65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Auto Approve", "EnableAutoApprove", "Enable Auto Approve", @"If enabled, prayer requests are automatically approved; otherwise they must be approved by an admin before they can be seen by the prayer team.", 0, @"True", "1DABDB8E-5148-48E9-9D45-53092A475CAC" );
            // Attrib for BlockType: Prayer Request Details:Expires After (Days)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FB75DC3-3C00-46C0-AE2A-37BA52A58E65", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Expires After (Days)", "ExpiresAfterDays", "Expires After (Days)", @"Number of days until the request will expire (only applies when auto-approved is enabled).", 1, @"14", "A5B7DF97-E286-4DA4-B8B5-6CCA57731F20" );
            // Attrib for BlockType: Prayer Request Details:Show Urgent Flag
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FB75DC3-3C00-46C0-AE2A-37BA52A58E65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Urgent Flag", "EnableUrgentFlag", "Show Urgent Flag", @"If enabled, requestors will be able to flag prayer requests as urgent.", 2, @"False", "BF87CE4C-5257-47CF-9FE0-267FB89C805F" );
            // Attrib for BlockType: Prayer Request Details:Show Public Display Flag
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FB75DC3-3C00-46C0-AE2A-37BA52A58E65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Public Display Flag", "EnablePublicDisplayFlag", "Show Public Display Flag", @"If enabled, requestors will be able set whether or not they want their request displayed on the public website.", 3, @"False", "52A93801-E402-470F-8DEC-7BD15DB43900" );
            // Attrib for BlockType: Prayer Request Details:Default To Public
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FB75DC3-3C00-46C0-AE2A-37BA52A58E65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default To Public", "DefaultToPublic", "Default To Public", @"If enabled, all prayers will be set to public by default.", 4, @"False", "FD44C438-451A-4AEB-AA1C-975E150ACB79" );
            // Attrib for BlockType: Prayer Request Details:Character Limit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FB75DC3-3C00-46C0-AE2A-37BA52A58E65", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Character Limit", "CharacterLimit", "Character Limit", @"If set to something other than 0, this will limit the number of characters allowed when entering a new prayer request.", 5, @"250", "A80AF504-B62C-4FF1-AD17-DDC002801DF4" );
            // Attrib for BlockType: Prayer Request Details:Show Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FB75DC3-3C00-46C0-AE2A-37BA52A58E65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus", "EnableCampus", "Show Campus", @"Should the campus field be displayed? If there is only one active campus then the campus field will not show.", 6, @"True", "FBD1D521-907D-4525-8A2D-08A62953293F" );
            // Attrib for BlockType: Prayer Request Details:Require Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FB75DC3-3C00-46C0-AE2A-37BA52A58E65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Campus", "RequireCampus", "Require Campus", @"Require that a campus be selected. The campus will not be displayed if there is only one available campus, in which case if this is set to true then the single campus is automatically used.", 7, @"False", "76F4E6C2-3222-4871-A44E-A633AEA8CE5C" );
            // Attrib for BlockType: Prayer Request Details:Require Last Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FB75DC3-3C00-46C0-AE2A-37BA52A58E65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Last Name", "RequireLastName", "Require Last Name", @"Require that a last name be entered. First name is always required.", 8, @"True", "973EFF60-422C-4953-A66B-0E66E0BDF952" );
            // Attrib for BlockType: Prayer Request Details:Enable Person Matching
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FB75DC3-3C00-46C0-AE2A-37BA52A58E65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Person Matching", "EnablePersonMatching", "Enable Person Matching", @"If enabled, the request will be linked to an existing person if a match can be made between the requester and an existing person.", 9, @"False", "381CA157-F4F2-40FA-96B9-1C39E6729396" );
            // Attrib for BlockType: Prayer Request Details:Completion Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FB75DC3-3C00-46C0-AE2A-37BA52A58E65", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Completion Action", "CompletionAction", "Completion Action", @"What action to perform after saving the prayer request.", 0, @"0", "FF1A1691-D50E-4EDE-BFDF-0D359D759895" );
            // Attrib for BlockType: Prayer Request Details:Completion Xaml
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FB75DC3-3C00-46C0-AE2A-37BA52A58E65", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Completion Xaml", "CompletionXaml", "Completion Xaml", @"The XAML markup that will be used if the. <span class='tip tip-lava'></span>", 1, @"<Rock:NotificationBox NotificationType=""Success"">
    Thank you for allowing us to pray for you.
</Rock:NotificationBox>", "302B6D6E-7A22-4BAE-9D6A-390BFFEFF94E" );
            // Attrib for BlockType: Prayer Request Details:Workflow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9FB75DC3-3C00-46C0-AE2A-37BA52A58E65", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "Workflow", @"An optional workflow to start when prayer request is created. The PrayerRequest will be set as the workflow 'Entity' attribute when processing is started.", 2, @"", "188120EB-DC7C-4141-84B1-AE6F0FDFBAE6" );
            // Attrib for BlockType: Achievement Attempt List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9C1236AE-4FF0-480C-A7DF-0E5277CA75FB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "A577E7A9-0C18-497A-A060-2AF6E62D744C" );
            // Attrib for BlockType: Achievement Attempt List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9C1236AE-4FF0-480C-A7DF-0E5277CA75FB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "8EEB019C-3625-4ABC-A18F-D8897F3E922C" );
            // Attrib for BlockType: Achievement Type List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D2F918CF-F63C-4643-A02A-FD3DE7C5CFFD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "21C2AFDD-2D39-4AAB-AE34-B106B4892588" );
            // Attrib for BlockType: Achievement Type List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D2F918CF-F63C-4643-A02A-FD3DE7C5CFFD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "24A35C14-A736-4A1F-B8C1-0B489B3A6AD4" );
            RockMigrationHelper.UpdateFieldType("Step","","Rock","Rock.Field.Types.StepFieldType","829803DB-7CA3-44F6-B1CB-669D61ED6E92");

        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Achievement Type List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("24A35C14-A736-4A1F-B8C1-0B489B3A6AD4");
            // Attrib for BlockType: Achievement Type List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("21C2AFDD-2D39-4AAB-AE34-B106B4892588");
            // Attrib for BlockType: Achievement Attempt List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("8EEB019C-3625-4ABC-A18F-D8897F3E922C");
            // Attrib for BlockType: Achievement Attempt List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("A577E7A9-0C18-497A-A060-2AF6E62D744C");
            // Attrib for BlockType: Prayer Request Details:Workflow
            RockMigrationHelper.DeleteAttribute("188120EB-DC7C-4141-84B1-AE6F0FDFBAE6");
            // Attrib for BlockType: Prayer Request Details:Completion Xaml
            RockMigrationHelper.DeleteAttribute("302B6D6E-7A22-4BAE-9D6A-390BFFEFF94E");
            // Attrib for BlockType: Prayer Request Details:Completion Action
            RockMigrationHelper.DeleteAttribute("FF1A1691-D50E-4EDE-BFDF-0D359D759895");
            // Attrib for BlockType: Prayer Request Details:Enable Person Matching
            RockMigrationHelper.DeleteAttribute("381CA157-F4F2-40FA-96B9-1C39E6729396");
            // Attrib for BlockType: Prayer Request Details:Require Last Name
            RockMigrationHelper.DeleteAttribute("973EFF60-422C-4953-A66B-0E66E0BDF952");
            // Attrib for BlockType: Prayer Request Details:Require Campus
            RockMigrationHelper.DeleteAttribute("76F4E6C2-3222-4871-A44E-A633AEA8CE5C");
            // Attrib for BlockType: Prayer Request Details:Show Campus
            RockMigrationHelper.DeleteAttribute("FBD1D521-907D-4525-8A2D-08A62953293F");
            // Attrib for BlockType: Prayer Request Details:Character Limit
            RockMigrationHelper.DeleteAttribute("A80AF504-B62C-4FF1-AD17-DDC002801DF4");
            // Attrib for BlockType: Prayer Request Details:Default To Public
            RockMigrationHelper.DeleteAttribute("FD44C438-451A-4AEB-AA1C-975E150ACB79");
            // Attrib for BlockType: Prayer Request Details:Show Public Display Flag
            RockMigrationHelper.DeleteAttribute("52A93801-E402-470F-8DEC-7BD15DB43900");
            // Attrib for BlockType: Prayer Request Details:Show Urgent Flag
            RockMigrationHelper.DeleteAttribute("BF87CE4C-5257-47CF-9FE0-267FB89C805F");
            // Attrib for BlockType: Prayer Request Details:Expires After (Days)
            RockMigrationHelper.DeleteAttribute("A5B7DF97-E286-4DA4-B8B5-6CCA57731F20");
            // Attrib for BlockType: Prayer Request Details:Enable Auto Approve
            RockMigrationHelper.DeleteAttribute("1DABDB8E-5148-48E9-9D45-53092A475CAC");
            // Attrib for BlockType: Prayer Request Details:Default Category
            RockMigrationHelper.DeleteAttribute("0C9168AE-D49C-4AF9-92FF-7A4572550E1F");
            // Attrib for BlockType: Prayer Request Details:Parent Category
            RockMigrationHelper.DeleteAttribute("699B0D26-FAFC-42CB-B34D-B4FF9A8595CB");
            // Attrib for BlockType: Prayer Request Details:Show Category
            RockMigrationHelper.DeleteAttribute("1CFD1678-C84A-4982-8ADD-8A80184B69E1");
            // Attrib for BlockType: Group View:Template
            RockMigrationHelper.DeleteAttribute("2132CC26-6517-49D7-910C-8792D84105E6");
            // Attrib for BlockType: Group View:Show Leader List
            RockMigrationHelper.DeleteAttribute("7E24475D-5E67-49DF-B185-B5A5D549BB5C");
            // Attrib for BlockType: Group View:Group Edit Page
            RockMigrationHelper.DeleteAttribute("29DF7D94-168B-45C1-BF61-BBC1891E0732");
            // Attrib for BlockType: Group Member View:Template
            RockMigrationHelper.DeleteAttribute("18FE73C7-2E38-4B24-9D28-19F8B8AC71D7");
            // Attrib for BlockType: Group Member View:Group Member Edit Page
            RockMigrationHelper.DeleteAttribute("EC03AE63-A41C-4767-87D8-C1558F1723F1");
            // Attrib for BlockType: Group Member List:Additional Fields
            RockMigrationHelper.DeleteAttribute("6988CA4A-1372-40FE-BFD6-35A16E58D7F8");
            // Attrib for BlockType: Group Member List:Template
            RockMigrationHelper.DeleteAttribute("4F71CDA9-D3E0-45AB-B890-F47DC132E795");
            // Attrib for BlockType: Group Member List:Title Template
            RockMigrationHelper.DeleteAttribute("182C35D7-0B40-481A-A73A-A2C30D05129D");
            // Attrib for BlockType: Group Member List:Group Member Detail Page
            RockMigrationHelper.DeleteAttribute("E69D0625-0A5F-41DA-B32B-F8153883FA5E");
            // Attrib for BlockType: Group Member Edit:Member Detail Page
            RockMigrationHelper.DeleteAttribute("2BF7187C-7874-4982-8F2A-58E8989A9D24");
            // Attrib for BlockType: Group Member Edit:Attribute Category
            RockMigrationHelper.DeleteAttribute("06524E30-40D3-4A3F-A30F-50627381CB24");
            // Attrib for BlockType: Group Member Edit:Allow Note Edit
            RockMigrationHelper.DeleteAttribute("EF8BDC54-7223-4C6F-AD04-1CF29CF19858");
            // Attrib for BlockType: Group Member Edit:Allow Member Status Change
            RockMigrationHelper.DeleteAttribute("BD21661A-C1F0-4746-B772-57086C989834");
            // Attrib for BlockType: Group Member Edit:Allow Role Change
            RockMigrationHelper.DeleteAttribute("6042D09F-EE19-4103-BFE7-301AE3A37ABC");
            // Attrib for BlockType: Group Edit:Group Detail Page
            RockMigrationHelper.DeleteAttribute("047D604E-4A39-4F01-9F88-E9B2DDE91A46");
            // Attrib for BlockType: Group Edit:Attribute Category
            RockMigrationHelper.DeleteAttribute("5877C76A-0AF9-478E-A7F9-9EBAA4874D08");
            // Attrib for BlockType: Group Edit:Enable Public Status Edit
            RockMigrationHelper.DeleteAttribute("91475885-4387-48F4-B5B8-796E790A55E2");
            // Attrib for BlockType: Group Edit:Show Public Status
            RockMigrationHelper.DeleteAttribute("825B8EC7-8AE2-4765-B72D-35F31190A329");
            // Attrib for BlockType: Group Edit:Enable Active Status Edit
            RockMigrationHelper.DeleteAttribute("9C55CE57-444E-4544-A9BD-92FA44771F07");
            // Attrib for BlockType: Group Edit:Show Active Status
            RockMigrationHelper.DeleteAttribute("3B85348E-D2D4-427B-9B36-7929CC9B7059");
            // Attrib for BlockType: Group Edit:Enable Group Capacity Edit
            RockMigrationHelper.DeleteAttribute("4AF18B7A-3853-48D0-BFC3-B8E41A86F1C3");
            // Attrib for BlockType: Group Edit:Show Group Capacity
            RockMigrationHelper.DeleteAttribute("6116C22C-B276-4D39-977B-FAAFD3D14E50");
            // Attrib for BlockType: Group Edit:Enable Campus Edit
            RockMigrationHelper.DeleteAttribute("534C93DB-EA75-474E-9552-4BE33CA95F2F");
            // Attrib for BlockType: Group Edit:Show Campus
            RockMigrationHelper.DeleteAttribute("BEFF20AD-108F-4D5E-99EE-2187F668490F");
            // Attrib for BlockType: Group Edit:Enable Description Edit
            RockMigrationHelper.DeleteAttribute("BE6EB808-9CD2-4F40-BAE0-A9AF0F87DB4D");
            // Attrib for BlockType: Group Edit:Show Description
            RockMigrationHelper.DeleteAttribute("BFD327BA-1BD6-4CED-A75F-6BC1F30733A3");
            // Attrib for BlockType: Group Edit:Enable Group Name Edit
            RockMigrationHelper.DeleteAttribute("F9978A25-DA35-4E05-856A-EEAF7491A1C0");
            // Attrib for BlockType: Group Edit:Show Group Name
            RockMigrationHelper.DeleteAttribute("8AD4569C-CC59-4C4F-8F0F-240599215671");
            // Attrib for BlockType: Group Attendance Entry:Allow Any Date Selection
            RockMigrationHelper.DeleteAttribute("6DE420C7-D07C-4FB4-A44B-8B25A72B9D4A");
            // Attrib for BlockType: Group Attendance Entry:Show Save Button
            RockMigrationHelper.DeleteAttribute("8F87D311-D309-4C5A-BC04-64EBA46A6BC7");
            // Attrib for BlockType: Group Attendance Entry:Save Redirect Page
            RockMigrationHelper.DeleteAttribute("9D48677F-D7B8-44C6-B9FB-A000EA81459E");
            // Attrib for BlockType: Group Attendance Entry:Number of Days Back to Allow
            RockMigrationHelper.DeleteAttribute("C6968BC8-F0B0-45A9-AAC2-11E4435CD6A3");
            // Attrib for BlockType: Group Attendance Entry:Number of Days Forward to Allow
            RockMigrationHelper.DeleteAttribute("C469CB11-E064-40CF-A1CE-42D93C4AFD84");
            // Attrib for BlockType: Prayer Session Setup:Show Campus Filter
            RockMigrationHelper.DeleteAttribute("21A77A81-1D9E-454F-9E3C-BDC9575CC6C6");
            // Attrib for BlockType: Prayer Session Setup:Parent Category
            RockMigrationHelper.DeleteAttribute("07DF5E9C-34EC-45EA-8E9B-5EE7E98DDA7C");
            // Attrib for BlockType: Prayer Session Setup:Prayer Page
            RockMigrationHelper.DeleteAttribute("E4BAF9B2-43EB-4B86-AD58-E174A79B43C6");
            // Attrib for BlockType: Prayer Session:Template
            RockMigrationHelper.DeleteAttribute("36DF8D73-4F49-4CCC-B646-07BA86BF2274");
            // Attrib for BlockType: Prayer Session:Inappropriate Flag Limit
            RockMigrationHelper.DeleteAttribute("2B3B7CE2-4464-4EE9-8F02-D37987B760B4");
            // Attrib for BlockType: Prayer Session:Public Only
            RockMigrationHelper.DeleteAttribute("3F45287F-5456-4A15-A4E8-930C04DF3740");
            // Attrib for BlockType: Prayer Session:Show Inappropriate Button
            RockMigrationHelper.DeleteAttribute("76F083A0-68FD-45FA-9018-27F468B47467");
            // Attrib for BlockType: Prayer Session:Show Follow Button
            RockMigrationHelper.DeleteAttribute("2D7AE299-1806-4B49-BC92-3729EAB9FAA9");
            // Attrib for BlockType: Prayer Session:Prayed Button Text
            RockMigrationHelper.DeleteAttribute("086F2D73-6653-418D-A903-961BA8DAAA96");
            // Attrib for BlockType: Communication List Subscribe:Show Description
            RockMigrationHelper.DeleteAttribute("32BC0E81-9654-4256-828D-865136F6C7F1");
            // Attrib for BlockType: Communication List Subscribe:Communication List Categories
            RockMigrationHelper.DeleteAttribute("9C710F95-BF71-4DBB-BAEF-6180B5E9E473");
            // Attrib for BlockType: Calendar View:Show Filter
            RockMigrationHelper.DeleteAttribute("8A5B4041-57C5-4B74-B368-7DE66A74C7CF");
            // Attrib for BlockType: Calendar View:Event Summary
            RockMigrationHelper.DeleteAttribute("FB0CFD3B-97B7-424C-AF00-4AB63E7CFFC6");
            // Attrib for BlockType: Calendar View:Audience Filter
            RockMigrationHelper.DeleteAttribute("2FB5BEA9-BF76-424B-B17B-8CB95F635B43");
            // Attrib for BlockType: Calendar View:Detail Page
            RockMigrationHelper.DeleteAttribute("61AA5DE0-3097-479F-9302-5FB3CD76F4C3");
            // Attrib for BlockType: Calendar View:Calendar
            RockMigrationHelper.DeleteAttribute("12DB27FF-7A03-4DA3-99DC-B2F7DE41DD96");
            // Attrib for BlockType: Calendar Event List:Day Header Template
            RockMigrationHelper.DeleteAttribute("A172CBD9-C5A8-4164-AF14-10C532164F03");
            // Attrib for BlockType: Calendar Event List:Event Template
            RockMigrationHelper.DeleteAttribute("37F8EA70-1983-4072-928A-356F2F4925EA");
            // Attrib for BlockType: Calendar Event List:Detail Page
            RockMigrationHelper.DeleteAttribute("3170A2A8-9E4F-4C71-9950-E7A419C0F3D8");
            // Attrib for BlockType: Calendar Event List:Calendar
            RockMigrationHelper.DeleteAttribute("BBF1879B-1116-4558-9437-EF66E21E4F34");
            // Attrib for BlockType: Hero:Padding
            RockMigrationHelper.DeleteAttribute("C62FE9BF-BA0B-4479-808D-72D2BC29D217");
            // Attrib for BlockType: Hero:Subtitle Color
            RockMigrationHelper.DeleteAttribute("B51E7458-7D74-4656-9500-444A801B25F0");
            // Attrib for BlockType: Hero:Title Color
            RockMigrationHelper.DeleteAttribute("6F60ECF1-EE6E-4D67-BF76-37DAA6FE3A24");
            // Attrib for BlockType: Hero:Text Align
            RockMigrationHelper.DeleteAttribute("67D9419D-3A1E-4D41-BB47-43BDFBA01484");
            // Attrib for BlockType: Hero:Height - Tablet
            RockMigrationHelper.DeleteAttribute("7BC408D8-1797-455E-8765-B612A3A913F8");
            // Attrib for BlockType: Hero:Height - Phone
            RockMigrationHelper.DeleteAttribute("D7454FEC-0AEA-4EF2-AE44-970899DF0D2A");
            // Attrib for BlockType: Hero:Background Image - Tablet
            RockMigrationHelper.DeleteAttribute("783735BB-9854-49E0-B79A-6F2F3241849E");
            // Attrib for BlockType: Hero:Background Image - Phone
            RockMigrationHelper.DeleteAttribute("721AE421-60FB-4EB8-9DB4-5F4ED7B0AA2D");
            // Attrib for BlockType: Hero:Subtitle
            RockMigrationHelper.DeleteAttribute("3BFDB45E-B9CB-40D8-B265-54688CA79D57");
            // Attrib for BlockType: Hero:Title
            RockMigrationHelper.DeleteAttribute("9614C5B8-3FAD-432E-922A-F556ACF8E06E");
            RockMigrationHelper.DeleteBlockType("9FB75DC3-3C00-46C0-AE2A-37BA52A58E65"); // Prayer Request Details
            RockMigrationHelper.DeleteBlockType("A3AF2D06-D87F-485F-ABE4-3C0B1C52A57B"); // Group View
            RockMigrationHelper.DeleteBlockType("07080D7A-6407-4F58-BDBF-FD273EAF6C8C"); // Group Member View
            RockMigrationHelper.DeleteBlockType("C7A85634-58E3-4D67-A22A-B035410B87A5"); // Group Member List
            RockMigrationHelper.DeleteBlockType("E2365240-F3FF-446A-83B0-3466478027E7"); // Group Member Edit
            RockMigrationHelper.DeleteBlockType("A6818D93-FB51-48BA-83BD-404B95CF2F8B"); // Group Edit
            RockMigrationHelper.DeleteBlockType("1F8308AE-7A1B-4695-8442-DCF6FBFCAB7A"); // Group Attendance Entry
            RockMigrationHelper.DeleteBlockType("5DC5967E-71E3-484D-A330-BF3CCE9C8406"); // Prayer Session Setup
            RockMigrationHelper.DeleteBlockType("5488C3EB-EC76-44FB-B3CE-EDBA163341FF"); // Prayer Session
            RockMigrationHelper.DeleteBlockType("4E1AA59A-EFEC-4A0F-B159-80F32E107558"); // Communication List Subscribe
            RockMigrationHelper.DeleteBlockType("88980765-A47F-4D7A-BFE7-BD0536991B05"); // Calendar View
            RockMigrationHelper.DeleteBlockType("88BC5341-6574-427D-B48C-3BEB4D73CC1C"); // Calendar Event List
            RockMigrationHelper.DeleteBlockType("F3B7A306-9B73-4E20-8BF6-16390807EADD"); // Hero
        }
    
        /// <summary>
        /// GJ: Fix Chart Shortcode YAxis Min/Max Value (Fixes #4157)
        /// </summary>
        private void FixChartShortcode()
        {
            Sql( MigrationSQL._202003311924098_Rollup_0331_FixChartYAxisShortcode );
        }

        /// <summary>
        /// SK: Updated Group Member Notification system email template to correctly define NumberTypeValueId for mobilePhone (Fixes #4156)
        /// </summary>
        private void GroupMemberNotificationSystemEmail ()
        {
            string newValue = "{%- assign mobilePhone = pendingIndividual.PhoneNumbers | Where:'NumberTypeValueId', 12 | Select:'NumberFormatted' -%}".Replace( "'", "''" );
            string oldValue = "{%- assign mobilePhone = pendingIndividual.PhoneNumbers | Where:'NumberTypeValueId', 136 | Select:'NumberFormatted' -%}".Replace( "'", "''" );

            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "Body" );

            Sql( $@"UPDATE [dbo].[SystemEmail] 
                    SET [Body] = REPLACE({targetColumn}, '{oldValue}', '{newValue}')
                    WHERE {targetColumn} LIKE '%{oldValue}%'
                            AND [Guid] = '18521B26-1C7D-E287-487D-97D176CA4986'" );

            newValue = "{%- assign mobilePhone =absentMember.PhoneNumbers | Where:'NumberTypeValueId', 12 | Select:'NumberFormatted' -%}".Replace( "'", "''" );
            oldValue = "{%- assign mobilePhone =absentMember.PhoneNumbers | Where:'NumberTypeValueId', 136 | Select:'NumberFormatted' -%}".Replace( "'", "''" );

            Sql( $@"UPDATE [dbo].[SystemEmail] 
                    SET [Body] = REPLACE({targetColumn}, '{oldValue}', '{newValue}')
                    WHERE {targetColumn} LIKE '%{oldValue}%'
                            AND [Guid] = '8747131E-3EDA-4FB0-A484-C2D2BE3918BA'" );
        }

        /// <summary>
        /// GJ: Change Structured Content Image Tool, Note Tool, and Fillin
        /// </summary>
        private void StructuredContentTools()
        {
            RockMigrationHelper.UpdateDefinedValue( "E43AD92C-4DD4-4D78-9852-FCFAEFDF52CA", "Default", "{     header: {     class: Header,     inlineToolbar: ['link'],     config: {         placeholder: 'Header'     },     shortcut: 'CMD+SHIFT+H'     },     image: {     class: SimpleImage,     inlineToolbar: ['link'],     },     list: {     class: List,     inlineToolbar: true,     shortcut: 'CMD+SHIFT+L'     },     checklist: {     class: Checklist,     inlineToolbar: true,     },     quote: {     class: Quote,     inlineToolbar: true,     config: {         quotePlaceholder: 'Enter a quote',         captionPlaceholder: 'Quote\\'s author',     },     shortcut: 'CMD+SHIFT+O'     },     warning: Warning,     marker: {     class:  Marker,     shortcut: 'CMD+SHIFT+M'     },     fillin: {     class:  FillIn,     shortcut: 'CMD+SHIFT+F'     },     code: {     class:  CodeTool,     shortcut: 'CMD+SHIFT+C'     },     note: {     class:  NoteTool,     shortcut: 'CMD+SHIFT+N'     },     delimiter: Delimiter,     inlineCode: {     class: InlineCode,     shortcut: 'CMD+SHIFT+C'     },     linkTool: LinkTool,     embed: Embed,     table: {     class: Table,     inlineToolbar: true,     shortcut: 'CMD+ALT+T'     } }", "31C63FB9-1365-4EEF-851D-8AB9A188A06C", false );
        }

        /// <summary>
        /// ED: Communication Entry Enable Person Param
        /// </summary>
        private void CommunicationEntryEnablePersonParam()
        {
            // Attrib for BlockType: Communication Entry:Enable Person Parameter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D9834641-7F39-4CFA-8CB2-E64068127565", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Person Parameter", "EnablePersonParameter", "Enable Person Parameter", @"When enabled, allows passing a 'person' querystring parameter with a person Id to the block to create a communication for that person.", 2, @"False", "B9C0511F-9C95-4DD1-93F0-EDCCF7CD0471" );
            // Attrib Value for Block:Communication, Attribute:Enable Person Parameter Page: Simple Communication, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("BD9B2F32-AB18-4761-80C9-FDA4DBEEA9EC","B9C0511F-9C95-4DD1-93F0-EDCCF7CD0471",@"True");
        }
    }
}
