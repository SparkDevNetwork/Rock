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
    public partial class Rollup_03171 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            RemoveAllAuthenticatedUsersFromHtmlEditorPluginsRockFileBrowser();
            RockCheckinManager();
            GoogleMapsSpellingFix();
            AddConnectionRequestHistoryCategory();
            AddFollowUpCompleteConnectionActivityType();
            AddIconCssClassToWebFarmCategory();
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
            // Add/Update BlockType Giving Configuration
            RockMigrationHelper.UpdateBlockType("Giving Configuration","Block used to view the giving.","~/Blocks/Crm/PersonDetail/GivingConfiguration.ascx","CRM > Person Detail","74F21000-67EF-42DD-B0B5-330AEF570094");

            // Attribute for BlockType: File Manager:Enable Zip Upload
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BA327D25-BD8A-4B67-B04C-17B499DDA4B6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Zip Upload", "ZipUploaderEnabled", "Enable Zip Upload", @"Set this to true to enable the Zip File uploader.", 3, @"False", "DEA74208-39C8-4684-AA77-B6901CD51E68" );

            // Attribute for BlockType: Attendance Detail:Profile Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CA59CE67-9313-4B9F-8593-380044E5AE6A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Profile Page", "PersonProfilePage", "Profile Page", @"The page to go back to after deleting this attendance.", 6, @"F3062622-C6AD-48F3-ADD7-7F58E4BD4EF3", "0C75C1DA-81A8-4929-89C4-81BDE91BC894" );

            // Attribute for BlockType: Giving Configuration:Add Transaction Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74F21000-67EF-42DD-B0B5-330AEF570094", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Transaction Page", "AddTransactionPage", "Add Transaction Page", @"", 0, @"B1CA86DC-9890-4D26-8EBD-488044E1B3DD", "A71DDD47-CEB1-4A18-AB9B-1B34161D1F7B" );

            // Attribute for BlockType: Giving Configuration:Pledge Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74F21000-67EF-42DD-B0B5-330AEF570094", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Pledge Detail Page", "PledgeDetailPage", "Pledge Detail Page", @"", 4, @"EF7AA296-CA69-49BC-A28B-901A8AAA9466", "C43A4F93-A845-41EA-804C-0BC0315A8630" );

            // Attribute for BlockType: Giving Configuration:Contribution Statement Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74F21000-67EF-42DD-B0B5-330AEF570094", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Contribution Statement Detail Page", "ContributionStatementDetailPage", "Contribution Statement Detail Page", @"The contribution statement detail page.", 6, @"", "1DD75518-B57C-4A38-BB81-909606D305E2" );

            // Attribute for BlockType: Giving Configuration:Accounts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74F21000-67EF-42DD-B0B5-330AEF570094", "17033CDD-EF97-4413-A483-7B85A787A87F", "Accounts", "Accounts", "Accounts", @"A selection of accounts to use for checking if transactions for the current user exist.", 3, @"", "F8E71FD0-B519-4B59-A25C-87166DD76F22" );

            // Attribute for BlockType: Giving Configuration:Person Token Expire Minutes
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74F21000-67EF-42DD-B0B5-330AEF570094", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Person Token Expire Minutes", "PersonTokenExpireMinutes", "Person Token Expire Minutes", @"The number of minutes the person token for the transaction is valid after it is issued.", 1, @"60", "5E9E0BF4-25F0-4C03-B3AC-F5CB661D5226" );

            // Attribute for BlockType: Giving Configuration:Person Token Usage Limit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74F21000-67EF-42DD-B0B5-330AEF570094", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Person Token Usage Limit", "PersonTokenUsageLimit", "Person Token Usage Limit", @"The maximum number of times the person token for the transaction can be used.", 2, @"1", "393F9660-647F-42F3-9044-3AB34E4CC2E7" );

            // Attribute for BlockType: Giving Configuration:Max Years To Display
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74F21000-67EF-42DD-B0B5-330AEF570094", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Years To Display", "MaxYearsToDisplay", "Max Years To Display", @"The maximum number of years to display (including the current year).", 5, @"3", "D2BD2293-DAD1-49DD-ACC9-7C7F6341995C" );
        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Contribution Statement Detail Page Attribute for BlockType: Giving Configuration
            RockMigrationHelper.DeleteAttribute("1DD75518-B57C-4A38-BB81-909606D305E2");

            // Max Years To Display Attribute for BlockType: Giving Configuration
            RockMigrationHelper.DeleteAttribute("D2BD2293-DAD1-49DD-ACC9-7C7F6341995C");

            // Pledge Detail Page Attribute for BlockType: Giving Configuration
            RockMigrationHelper.DeleteAttribute("C43A4F93-A845-41EA-804C-0BC0315A8630");

            // Accounts Attribute for BlockType: Giving Configuration
            RockMigrationHelper.DeleteAttribute("F8E71FD0-B519-4B59-A25C-87166DD76F22");

            // Person Token Usage Limit Attribute for BlockType: Giving Configuration
            RockMigrationHelper.DeleteAttribute("393F9660-647F-42F3-9044-3AB34E4CC2E7");

            // Person Token Expire Minutes Attribute for BlockType: Giving Configuration
            RockMigrationHelper.DeleteAttribute("5E9E0BF4-25F0-4C03-B3AC-F5CB661D5226");

            // Add Transaction Page Attribute for BlockType: Giving Configuration
            RockMigrationHelper.DeleteAttribute("A71DDD47-CEB1-4A18-AB9B-1B34161D1F7B");

            // Enable Zip Upload Attribute for BlockType: File Manager
            RockMigrationHelper.DeleteAttribute("DEA74208-39C8-4684-AA77-B6901CD51E68");

            // Profile Page Attribute for BlockType: Attendance Detail
            RockMigrationHelper.DeleteAttribute("0C75C1DA-81A8-4929-89C4-81BDE91BC894");

            // Delete BlockType Giving Configuration
            RockMigrationHelper.DeleteBlockType("74F21000-67EF-42DD-B0B5-330AEF570094"); // Giving Configuration
        }

        /// <summary>
        /// NA: Add a plugin migration to remove "All Authenticated Users" from the /htmleditorplugins/RockFileBrowser page
        /// </summary>
        private void RemoveAllAuthenticatedUsersFromHtmlEditorPluginsRockFileBrowser()
        {
            Sql( @"DELETE FROM [Auth] WHERE [Guid] = '08138684-f4dc-4848-a8d5-342eed87fd85'" );
        }

        /// <summary>
        /// GJ: Migration for Rock Checkin Manager
        /// </summary>
        private void RockCheckinManager()
        {
            // Add Block Back Button to Layout: Full Width, Site: Rock Check-in Manager 
            RockMigrationHelper.AddBlock( true, null,"8305704F-928D-4379-967A-253E576E0923".AsGuid(),"A5FA7C3C-A238-4E0B-95DE-B540144321EC".AsGuid(),"19B61D65-37E3-459F-A44F-DEF0089118A3".AsGuid(), "Back Button","Login",@"",@"",0,"B62CBF17-7FD1-42C8-9E98-00270A34400D");
            // Add/Update HtmlContent for Block: Back Button 
            RockMigrationHelper.UpdateHtmlContentBlock("B62CBF17-7FD1-42C8-9E98-00270A34400D",@"<a href=""javascript:history.back();"" class=""btn btn-default"">Back</a>","26988382-5547-41E4-B737-99F0C079A788");

            // Add Block Back Button to Layout: Left Sidebar, Site: Rock Check-in Manager 
            RockMigrationHelper.AddBlock( true, null,"2669A579-48A5-4160-88EA-C3A10024E1E1".AsGuid(),"A5FA7C3C-A238-4E0B-95DE-B540144321EC".AsGuid(),"19B61D65-37E3-459F-A44F-DEF0089118A3".AsGuid(), "Back Button","Login",@"",@"",0,"A9A5FF01-2263-4CE3-82EB-326528BAAD98");
            // Add Block Campus Context Setter to Layout: Left Sidebar, Site: Rock Check-in Manager 
            RockMigrationHelper.AddBlock( true, null,"2669A579-48A5-4160-88EA-C3A10024E1E1".AsGuid(),"A5FA7C3C-A238-4E0B-95DE-B540144321EC".AsGuid(),"4A5AAFFC-B1C7-4EFD-A9E4-84363242EA85".AsGuid(), "Campus Context Setter","Login",@"",@"",0,"EC16F292-8FF8-44A4-84A3-9F64991C3BEB");
            // Add/Update HtmlContent for Block: Back Button 
            RockMigrationHelper.UpdateHtmlContentBlock("A9A5FF01-2263-4CE3-82EB-326528BAAD98",@"<a href=""javascript:history.back();"" class=""btn btn-default"">Back</a>","89A52AED-0245-40DF-87D5-C692761B7E5E");
        }

        /// <summary>
        /// GJ: Fix spelling on Google Maps
        /// </summary>
        /// <value>
        /// The google maps spelling fix.
        /// </value>
        private void GoogleMapsSpellingFix()
        {
            string sql = @"UPDATE [LavaShortcode] SET [Documentation]=N'<p>
    Adding a Google map to your page always starts out sounding easy… until… you get to the details. Soon the whole day is wasted and you don''t have much to 
    show. This shortcode makes it easy to add responsive Google Maps to your site. Let''s start with a simple example and work our way to more complex use cases.
</p>

<p>
    Note: Due to the javascript requirements of this shortcode you will need to do a full page reload before changes to the 
    shortcode appear on your page.
</p>

<pre>{[ googlemap ]}
    [[ marker location:''33.640705,-112.280198'' ]] [[ endmarker ]]
{[ endgooglemap ]}</pre>

<p>
    In the example above we mapped a single point to our map. Pretty easy, but not very helpful. We can add additional points by providing more markers.
</p>

<pre>{[ googlemap ]}
     [[ marker location:''33.640705,-112.280198'' ]] [[ endmarker ]]
     [[ marker location:'' 33.52764, -112.262571'' ]] [[ endmarker ]]
{[ endgooglemap ]}</pre>

<p>
    Ok… we''re getting there, but what if we wanted titles and information windows for our markers, oh and custom markers 
    too? You can provide optional parameters for each marker as shown below.
</p>

<pre>[[ marker location:''latitude,longitude'' title:''My Title'' icon:''icon url'' ]] info window content [[ endmarker ]]</pre>

<p><strong>Example:</strong></p>

<pre>{[ googlemap ]}
    [[ marker location:''33.640705,-112.280198'' title:''Spark Global Headquarters'']]
        &lt;strong&gt;Spark Global Headquarters&lt;/strong&gt;
        It''s not as grand as it sounds.&lt;br&gt;
        &lt;img src=""https://rockrms.blob.core.windows.net/misc/spark-logo.png"" width=""179"" height=""47""&gt;          
    [[ endmarker ]]
    [[ marker location:''33.52764, -112.262571'']][[ endmarker ]]
{[ endgooglemap ]}</pre>

<p></p><p>
    Note: A list of great resources for custom map markers is below:
</p>

<ul>
    <li><a href=""http://map-icons.com/"">Map Icons</a></li>
    <li><a href=""https://mapicons.mapsmarker.com/"">Map Icons Collection</a></li>
    <li><a href=""https://github.com/Concept211/Google-Maps-Markers"">Google Maps Markers</a></li>
</ul>

<p>
    There are several other parameters for you to use to control the options on your map. They include:
</p>

<ul>
    <li><strong>height</strong> (600px) – The height of the map.</li>
    <li><strong>width</strong> (100%) – The responsive width of the map.</li>
    <li><strong>zoom</strong> (optional) – The zoom level of the map. Note when two or more points are provided the map will auto zoom to place all of the points on the map. The range of the zoom scale is 1 (the furthest out, largest) to 20 (the closest, smallest). The approximate zoom levels are: 
    <ul>
        <li>1 = world</li>
        <li>5 = continent</li>
        <li>10 = city</li>
        <li>15 = streets</li>
        <li>20 = buildings</li>
    </ul>
    </li>
    <li><strong>center</strong> (optional) – The center point on the map. If you do not provide a center a default will be calculated based on the points given.</li>
    <li><strong>maptype</strong> (roadmap) – The type of map to display. The options are ‘roadmap'', ‘hybrid'', ‘satellite'' or ‘terrain''.</li>
    <li><strong>showzoom</strong> (true) – Should the zoom control be displayed.</li>
    <li><strong>showstreetview</strong> (false) – Should he StreetView control be displayed.</li>
    <li><strong>showfullscreen</strong> (true) – Should the control to show the map full screen be displayed.</li>
    <li><strong>showmapttype</strong> (false) – Should the control to change the map type be shown.</li>
    <li><strong>markeranimation</strong> (none) – The marker animation type. Options include: ''none'', ‘bounce'' (markers bounce continuously) or ''drop'' (markers drop in).</li>
    <li><strong>scrollwheel</strong> (true) – Determines if the scroll wheel should control the zoom level when the mouse is over the map.</li>
    <li><strong>draggable</strong> (true) – Determines if the mouse should be allowed to drag the center point of the map (allow the map to be moved).</li>
    <li><strong>gesturehandling</strong> (cooperative) – Determines how the map should scroll. The default is not to scroll with the scroll wheel. Often times a person is using the scroll-wheel to scroll down the page. If the cursor happens to scroll over the map the map will then start zooming in. In ‘cooperative'' mode this will not occur and the guest will need to use [ctlr] + scroll to zoom the map. If you would like to disable this setting set the mode to ''greedy''.</li>
</ul>

<p>
    As you can see there are a lot of options in working with your map. You can also style your map by changing the colors. You do this by providing 
    the styling information in a separate [[ style ]] section. The styling settings for Google Maps is not pretty to look at or configure for that matter. Luckily, there 
    are several sites that allow you to download preconfigured map styles. Two of the best are called <a href=""https://snazzymaps.com"">SnazzyMaps</a> and 
    <a href=""https://mapstyle.withgoogle.com"">Map Style</a>. Below is an example showing how to add styling to your maps.
</p>

<pre>{[ googlemap ]}
     [[ marker location:''33.640705,-112.280198'' ]] [[ endmarker ]]
     [[ marker location:'' 33.52764, -112.262571'' ]] [[ endmarker ]] 
     [[ style ]]
        [{""featureType"":""all"",""elementType"":""all"",""stylers"":[{""visibility"":""on""}]},{""featureType"":""all"",""elementType"":""labels"",""stylers"":[{""visibility"":""off""},{""saturation"":""-100""}]},{""featureType"":""all"",""elementType"":""labels.text.fill"",""stylers"":[{""saturation"":36},{""color"":""#000000""},{""lightness"":40},{""visibility"":""off""}]},{""featureType"":""all"",""elementType"":""labels.text.stroke"",""stylers"":[{""visibility"":""off""},{""color"":""#000000""},{""lightness"":16}]},{""featureType"":""all"",""elementType"":""labels.icon"",""stylers"":[{""visibility"":""off""}]},{""featureType"":""administrative"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#000000""},{""lightness"":20}]},{""featureType"":""administrative"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#000000""},{""lightness"":17},{""weight"":1.2}]},{""featureType"":""landscape"",""elementType"":""geometry"",""stylers"":[{""color"":""#000000""},{""lightness"":20}]},{""featureType"":""landscape"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#4d6059""}]},{""featureType"":""landscape"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#4d6059""}]},{""featureType"":""landscape.natural"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#4d6059""}]},{""featureType"":""poi"",""elementType"":""geometry"",""stylers"":[{""lightness"":21}]},{""featureType"":""poi"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#4d6059""}]},{""featureType"":""poi"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#4d6059""}]},{""featureType"":""road"",""elementType"":""geometry"",""stylers"":[{""visibility"":""on""},{""color"":""#7f8d89""}]},{""featureType"":""road"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#7f8d89""}]},{""featureType"":""road.highway"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#7f8d89""},{""lightness"":17}]},{""featureType"":""road.highway"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#7f8d89""},{""lightness"":29},{""weight"":0.2}]},{""featureType"":""road.arterial"",""elementType"":""geometry"",""stylers"":[{""color"":""#000000""},{""lightness"":18}]},{""featureType"":""road.arterial"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#7f8d89""}]},{""featureType"":""road.arterial"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#7f8d89""}]},{""featureType"":""road.local"",""elementType"":""geometry"",""stylers"":[{""color"":""#000000""},{""lightness"":16}]},{""featureType"":""road.local"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#7f8d89""}]},{""featureType"":""road.local"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#7f8d89""}]},{""featureType"":""transit"",""elementType"":""geometry"",""stylers"":[{""color"":""#000000""},{""lightness"":19}]},{""featureType"":""water"",""elementType"":""all"",""stylers"":[{""color"":""#2b3638""},{""visibility"":""on""}]},{""featureType"":""water"",""elementType"":""geometry"",""stylers"":[{""color"":""#2b3638""},{""lightness"":17}]},{""featureType"":""water"",""elementType"":""geometry.fill"",""stylers"":[{""color"":""#24282b""}]},{""featureType"":""water"",""elementType"":""geometry.stroke"",""stylers"":[{""color"":""#24282b""}]},{""featureType"":""water"",""elementType"":""labels"",""stylers"":[{""visibility"":""off""}]},{""featureType"":""water"",""elementType"":""labels.text"",""stylers"":[{""visibility"":""off""}]},{""featureType"":""water"",""elementType"":""labels.text.fill"",""stylers"":[{""visibility"":""off""}]},{""featureType"":""water"",""elementType"":""labels.text.stroke"",""stylers"":[{""visibility"":""off""}]},{""featureType"":""water"",""elementType"":""labels.icon"",""stylers"":[{""visibility"":""off""}]}]
    [[ endstyle]]
{[ endgooglemap ]}</pre>

<p>
    Seem scary? Don''t worry, everything inside of the [[ style ]] tag was simply copy and pasted straight from SnazzyMaps!
</p>' WHERE ([Guid]='FE298210-1307-49DF-B28B-3735A414CCA0')";
            Sql( sql );
        }

        /// <summary>
        /// SK: Added Connection Request history Category
        /// </summary>
        private void AddConnectionRequestHistoryCategory()
        {
            RockMigrationHelper.UpdateCategory( "546D5F43-1184-47C9-8265-2D7BF4E1BCA5", "Connection Request", "", "", "A8542DD2-91B1-4CCA-873A-D052BCD6EE06" );
        }

        /// <summary>
        /// SK: Added Future Follow-up Complete Connection Activity Type
        /// </summary>
        private void AddFollowUpCompleteConnectionActivityType()
        {
            Sql( string.Format(
            @"
            IF NOT EXISTS (
               SELECT [Id]
               FROM [ConnectionActivityType]
               WHERE [Guid] = '{0}' )
            BEGIN
	            INSERT INTO
		            [dbo].[ConnectionActivityType]
		            ( [Name], [IsActive], [Guid])
	            VALUES
		            ( N'Future Follow-up Complete', 1, N'{0}')
            END
            ",
            "D0FBB866-9029-4705-B3BA-07364F3D7FC1" ) );
        }

        /// <summary>
        /// SK: Added Block Attribute Value to add IconCssClass to WebFarm Category
        /// </summary>
        private void AddIconCssClassToWebFarmCategory()
        {
            RockMigrationHelper.AddBlockAttributeValue("2583DE89-F028-4ACE-9E1F-2873340726AC","75A0C88F-7F5B-48A2-88A4-C3A62F0EDF9A",@"CMS^fa fa-code|Communication^fa fa-comment|Connection^fa fa-plug|Core^fa fa-gear|Event^fa fa-clipboard|Finance^fa fa-money|Group^fa fa-users|Prayer^fa fa-cloud-upload|Reporting^fa fa-list-alt|Workflow^fa fa-gears|Other^fa fa-question-circle|CRM^fa fa-user|Meta^fa fa-table|Engagement^fa fa-cogs|WebFarm^fa fa-network-wired");
        }
    }
}
