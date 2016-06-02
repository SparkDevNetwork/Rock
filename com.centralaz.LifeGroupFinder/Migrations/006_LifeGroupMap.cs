// <copyright>
// Copyright by Central Christian Church
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;
using Rock.Web.Cache;

namespace com.centralaz.LifeGroupFinder.Migrations
{
    [MigrationNumber( 6, "1.3.4" )]
    public class LifeGroupMap : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Page: Life Group Map
            RockMigrationHelper.AddPage( "EC20A91E-49F4-4F77-BD36-35EAA51C0A12", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Life Group Map", "", "E3C4A92C-3026-4983-9403-A2A78CA513B3", "" ); // Site:External Website
            RockMigrationHelper.UpdateBlockType( " Life Group Map", "Displays a group (and any child groups) on a map.", "~/Plugins/com_centralaz/LifeGroupFinder/LifeGroupMap.ascx", "com_centralaz > Groups", "0BEB5923-62FA-4142-9D63-5DB0D296F6C6" );
            RockMigrationHelper.AddBlock( "E3C4A92C-3026-4983-9403-A2A78CA513B3", "", "0BEB5923-62FA-4142-9D63-5DB0D296F6C6", "Life Group Map", "Main", "", "", 0, "1694FE4B-FE9A-4415-A44F-D9F7755A099D" );

            RockMigrationHelper.AddBlockTypeAttribute( "0BEB5923-62FA-4142-9D63-5DB0D296F6C6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "", "The page to display group details.", 0, @"", "BFE5BB78-A7B2-4CA9-B011-E1E408A862DC" );

            RockMigrationHelper.AddBlockTypeAttribute( "0BEB5923-62FA-4142-9D63-5DB0D296F6C6", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Map Style", "MapStyle", "", "The map theme that should be used for styling the map.", 3, @"BFC46259-FB66-4427-BF05-2B030A582BEA", "DE4315D1-135D-4764-BE7D-BF2A9C49CC36" );

            RockMigrationHelper.AddBlockTypeAttribute( "0BEB5923-62FA-4142-9D63-5DB0D296F6C6", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Map Height", "MapHeight", "", "Height of the map in pixels (default value is 600px)", 4, @"600", "B3B26D76-00F6-4E50-B820-3C479CB02C0B" );

            RockMigrationHelper.AddBlockTypeAttribute( "0BEB5923-62FA-4142-9D63-5DB0D296F6C6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Polygon Colors", "PolygonColors", "", "Comma-Delimited list of colors to use when displaying multiple polygons (e.g. #f37833,#446f7a,#afd074,#649dac,#f8eba2,#92d0df,#eaf7fc).", 5, @"#f37833,#446f7a,#afd074,#649dac,#f8eba2,#92d0df,#eaf7fc", "8183947E-CC0F-4995-A072-02E99BBE9F5B" );

            RockMigrationHelper.AddBlockTypeAttribute( "0BEB5923-62FA-4142-9D63-5DB0D296F6C6", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Info Window Contents", "InfoWindowContents", "", "Liquid template for the info window. To suppress the window provide a blank template.", 6, @"
<h4 class='margin-t-none'>{{ Group.Name }}</h4> 

<div class='margin-v-sm'>
{% if Location.FormattedHtmlAddress && Location.FormattedHtmlAddress != '' %}
	{{ Location.FormattedHtmlAddress }}
{% endif %}
</div>

{% if LinkedPages.GroupDetailPage != '' %}
    <a class='btn btn-xs btn-action margin-r-sm' href='{{ LinkedPages.GroupDetailPage }}?GroupId={{ Group.Id }}'>View {{ Group.GroupType.GroupTerm }}</a>
{% endif %}
", "991C2F17-C9CA-48A3-A41D-9BB5E8DA3E18" );

            RockMigrationHelper.AddBlockAttributeValue( "1694FE4B-FE9A-4415-A44F-D9F7755A099D", "B3B26D76-00F6-4E50-B820-3C479CB02C0B", @"600" ); // Map Height

            RockMigrationHelper.AddBlockAttributeValue( "1694FE4B-FE9A-4415-A44F-D9F7755A099D", "8183947E-CC0F-4995-A072-02E99BBE9F5B", @"#f37833,#446f7a,#afd074,#649dac,#f8eba2,#92d0df,#eaf7fc" ); // Polygon Colors

            RockMigrationHelper.AddBlockAttributeValue( "1694FE4B-FE9A-4415-A44F-D9F7755A099D", "991C2F17-C9CA-48A3-A41D-9BB5E8DA3E18", @"<h4 class='margin-t-none'>{{ Group.Name }}</h4> 

<div class='margin-v-sm'>
{% if Location.FormattedHtmlAddress && Location.FormattedHtmlAddress != '' %}
	{{ Location.FormattedHtmlAddress }}
{% endif %}
</div>

{% if LinkedPages.GroupDetailPage != '' %}
    <a class='btn btn-xs btn-action margin-r-sm' href='{{ LinkedPages.GroupDetailPage }}?GroupId={{ Group.Id }}'>View {{ Group.GroupType.GroupTerm }}</a>
{% endif %}
" ); // Info Window Contents

            RockMigrationHelper.AddBlockAttributeValue( "1694FE4B-FE9A-4415-A44F-D9F7755A099D", "BFE5BB78-A7B2-4CA9-B011-E1E408A862DC", @"803a7786-6a83-43df-a247-bb4dff50aae8" ); // Group Detail Page

            RockMigrationHelper.AddBlockAttributeValue( "1694FE4B-FE9A-4415-A44F-D9F7755A099D", "DE4315D1-135D-4764-BE7D-BF2A9C49CC36", @"bfc46259-fb66-4427-bf05-2b030a582bea" ); // Map Style

            RockMigrationHelper.AddBlockTypeAttribute( "205531A1-C1BC-494C-911E-EE88D29969FB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Life Group Map Page", "LifeGroupMapPage", "", "The page to navigate to for the group list.", 0, @"", "86CECEF8-46CB-45DA-8BFF-5174EAFD9D32" );

            Sql( @"
            Update [Attribute]
            Set [Description] = 'The short summary of your group that will show up in search results'
            Where [Guid] = '8F0A6B55-8DA5-42EC-9369-E1BF11C903E8'

            Update [Attribute]
            Set [Description] = 'A photo of you that will override your profile picture on your group details page.'
            Where [Guid] = '36F6FFFE-6BD9-4DC6-81F3-F125492EECD5'

            Update [Attribute]
            Set [Description] = 'A video of you that will override your profile picture and main photo on your group details page.'
            Where [Guid] = '6775AD18-BA1A-4696-9D62-F950751537B2'

            Update [Attribute]
            Set [Description] = 'A photo of your group.'
            Where [Guid] = 'D578D2B5-A549-4E7E-98C9-C44A4E5A654D'

            Update [Attribute]
            Set [Description] = 'A photo of your group.'
            Where [Guid] = '667C780E-1543-499A-82FC-7B415820977D'

            Update [Attribute]
            Set [Description] = 'A photo of your group.'
            Where [Guid] = '9DA055F3-9B8A-4399-BB93-9783502273DF'
" );

            RockMigrationHelper.UpdateBlockType( "Fire Workflow Button", "Allows a user to fire off a workflow", "~/Plugins/com_centralaz/LifeGroupFinder/FireWorkflowButton.ascx", "com_centralaz > Groups", "33677897-2A3C-46BF-81C7-F5A61788B63C" );
            RockMigrationHelper.AddBlockTypeAttribute( "33677897-2A3C-46BF-81C7-F5A61788B63C", "1D50399B-0D6E-480B-A71A-E7BD37DD83F0", "Workflow Actions", "WorkflowActions", "", "The workflows to make available as actions.", 1, @"", "B662ADD2-96DE-43CA-91F4-B2B2EE431B71" );


        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "B662ADD2-96DE-43CA-91F4-B2B2EE431B71" );
            RockMigrationHelper.DeleteBlockType( "33677897-2A3C-46BF-81C7-F5A61788B63C" );

            RockMigrationHelper.DeleteAttribute( "DE4315D1-135D-4764-BE7D-BF2A9C49CC36" );
            RockMigrationHelper.DeleteAttribute( "BFE5BB78-A7B2-4CA9-B011-E1E408A862DC" );
            RockMigrationHelper.DeleteAttribute( "991C2F17-C9CA-48A3-A41D-9BB5E8DA3E18" );
            RockMigrationHelper.DeleteAttribute( "8183947E-CC0F-4995-A072-02E99BBE9F5B" );
            RockMigrationHelper.DeleteAttribute( "B3B26D76-00F6-4E50-B820-3C479CB02C0B" );
            RockMigrationHelper.DeleteBlock( "1694FE4B-FE9A-4415-A44F-D9F7755A099D" );
            RockMigrationHelper.DeleteBlockType( "0BEB5923-62FA-4142-9D63-5DB0D296F6C6" );
            RockMigrationHelper.DeletePage( "E3C4A92C-3026-4983-9403-A2A78CA513B3" ); //  Page: Life Group Map
        }
    }
}
