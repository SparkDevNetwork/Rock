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
    public partial class RemoveIsNamedLoc : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Location", "IsGeoPointLocked", c => c.Boolean());
            CreateIndex("dbo.Location", "LocationTypeValueId");
            AddForeignKey("dbo.Location", "LocationTypeValueId", "dbo.DefinedValue", "Id");
            DropColumn("dbo.Location", "IsNamedLocation");

            AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "6AC471A3-9B0E-459B-ADA2-F6E18F970803", "Locations", "", "2BECFB85-D566-464F-B6AC-0BE90189A418", "fa fa-map-marker" ); // Site:Rock RMS
            AddBlockType( "Location Tree View", "Creates a navigation tree for named locations.", "~/Blocks/Core/LocationTreeView.ascx", "468B99CE-D276-4D30-84A9-7842933BDBCD" );
            AddBlockType( "Location Detail", "Displays the details of the given location.", "~/Blocks/Core/LocationDetail.ascx", "08189564-1245-48F8-86CC-560F4DD48733" );
            // Add Block to Page: Named Locations, Site: Rock RMS
            AddBlock( "2BECFB85-D566-464F-B6AC-0BE90189A418", "", "468B99CE-D276-4D30-84A9-7842933BDBCD", "Location Tree View", "Sidebar1", "", "", 0, "AD6A533A-CBFF-4386-8A47-EB446E959541" );
            // Add Block to Page: Named Locations, Site: Rock RMS
            AddBlock( "2BECFB85-D566-464F-B6AC-0BE90189A418", "", "08189564-1245-48F8-86CC-560F4DD48733", "Location Detail", "Main", "", "", 0, "625CF250-7049-40D4-A381-FFD346BCFF64" );
            // Attrib for BlockType: Location Tree View:Treeview Title
            AddBlockTypeAttribute( "468B99CE-D276-4D30-84A9-7842933BDBCD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Treeview Title", "TreeviewTitle", "", "Location Tree View", 0, @"", "EC259AD8-CE80-401D-948B-66322ED5B5D0" );
            // Attrib for BlockType: Location Tree View:Detail Page
            AddBlockTypeAttribute( "468B99CE-D276-4D30-84A9-7842933BDBCD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "BA252362-F8D3-4D2D-91B5-7E90BDB154DA" );
            // Attrib for BlockType: Location Detail:Map HTML
            AddBlockTypeAttribute( "08189564-1245-48F8-86CC-560F4DD48733", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Map HTML", "MapHTML", "", "The HTML to use for displaying group location maps. Liquid syntax is used to render data from the following data structure: points[type, latitude, longitude], polygons[type, polygon_wkt, google_encoded_polygon]", 0, @"
    {% if point or polygon %}
        <div class='group-location-map'>
            <img src='http://maps.googleapis.com/maps/api/staticmap?sensor=false&size=350x200&format=png&style=feature:all|saturation:0|hue:0xe7ecf0&style=feature:road|saturation:-70&style=feature:transit|visibility:off&style=feature:poi|visibility:off&style=feature:water|visibility:simplified|saturation:-60{% if point %}&markers=color:0x779cb1|{{ point.latitude }},{{ point.longitude }}{% endif %}{% if polygon %}&path=fillcolor:0x779cb155|color:0xFFFFFF00|enc:{{ polygon.google_encoded_polygon }}{% endif %}&visual_refresh=true'/>
        </div>
    {% endif %}
", "A4BE7F16-A8E0-4F2F-AFFD-9323AEE51F1D" );
            // Attrib Value for Block:Location Tree View, Attribute:Detail Page Page: Named Locations, Site: Rock RMS
            AddBlockAttributeValue( "AD6A533A-CBFF-4386-8A47-EB446E959541", "BA252362-F8D3-4D2D-91B5-7E90BDB154DA", @"2becfb85-d566-464f-b6ac-0be90189a418" );
            // Attrib Value for Block:Location Tree View, Attribute:Treeview Title Page: Named Locations, Site: Rock RMS
            AddBlockAttributeValue( "AD6A533A-CBFF-4386-8A47-EB446E959541", "EC259AD8-CE80-401D-948B-66322ED5B5D0", @"Locations" );

            Sql( @"
    UPDATE [Page] SET [Order] = 1 WHERE [Guid] = 'A3990266-CB0D-4FB5-882C-3852ED5D96AB'		--Rock Update
    UPDATE [Page] SET [Order] = 2 WHERE [Guid] = 'A2753E03-96B1-4C83-AA11-FCD68C631571'		--Global Attributes
    UPDATE [Page] SET [Order] = 3 WHERE [Guid] = 'E0E1DE66-B825-4BFB-A0B3-6E069AA9AA40'		--Defined Types
    UPDATE [Page] SET [Order] = 4 WHERE [Guid] = '40899BCD-82B0-47F2-8F2A-B6AA3877B445'		--Group Types
    UPDATE [Page] SET [Order] = 5 WHERE [Guid] = '5EE91A54-C750-48DC-9392-F1F0F0581C3A'		--Campuses
    UPDATE [Page] SET [Order] = 6 WHERE [Guid] = 'F111791B-6A58-4388-8533-00E913F48F41'		--Tags
    UPDATE [Page] SET [Order] = 7 WHERE [Guid] = 'DCB18A76-6DFF-48A5-A66E-2CAA10D2CA1A'		--Workflow Configuration
    UPDATE [Page] SET [Order] = 8 WHERE [Guid] = '1A233978-5BF4-4A09-9B86-6CC4C081F48B'		--Workflow Triggers
    UPDATE [Page] SET [Order] = 9 WHERE [Guid] = '66031C31-B397-4F78-8AB2-389B7D8731AA'		--File Types
    UPDATE [Page] SET [Order] = 10 WHERE [Guid] = '2BECFB85-D566-464F-B6AC-0BE90189A418'		--Locations
    UPDATE [Page] SET [Order] = 11 WHERE [Guid] = '7E660A4D-72C5-4CF8-B144-16CFC2ADD4D7'		--Devices
    UPDATE [Page] SET [Order] = 12 WHERE [Guid] = 'C646A95A-D12D-4A67-9BE6-C9695C0267ED'		--Check-in Configuration
    UPDATE [Page] SET [Order] = 13 WHERE [Guid] = '7C093A63-F2AC-4FE3-A826-8BF06D204EA2'		--Check-in Labels
    UPDATE [Page] SET [Order] = 14 WHERE [Guid] = 'F5D6D7DD-FD5F-494C-83DC-E2AF63C705D1'		--Schedules
    UPDATE [Page] SET [Order] = 15 WHERE [Guid] = '220D72F5-B589-4378-9852-BBB6F145AD7F'		--Attribute Categories
    UPDATE [Page] SET [Order] = 16 WHERE [Guid] = '84DB9BA0-2725-40A5-A3CA-9A1C043C31B0'		--Metrics
    UPDATE [Page] SET [Order] = 17 WHERE [Guid] = 'FA2A1171-9308-41C7-948C-C9EBEA5BD668'		--Prayer Categories
    UPDATE [Page] SET [Order] = 18 WHERE [Guid] = '7BA1FAF4-B63C-4423-A818-CC794DDB14E3'		--Person Attributes
    UPDATE [Page] SET [Order] = 19 WHERE [Guid] = '26547B83-A92D-4D7E-82ED-691F403F16B6'		--Person Profile Badges
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            DeleteAttribute( "A4BE7F16-A8E0-4F2F-AFFD-9323AEE51F1D" );
            DeleteAttribute( "BA252362-F8D3-4D2D-91B5-7E90BDB154DA" );
            DeleteAttribute( "EC259AD8-CE80-401D-948B-66322ED5B5D0" );
            DeleteBlock( "AD6A533A-CBFF-4386-8A47-EB446E959541" );
            DeleteBlock( "625CF250-7049-40D4-A381-FFD346BCFF64" );
            DeleteBlockType( "08189564-1245-48F8-86CC-560F4DD48733" ); // Location Detail
            DeleteBlockType( "468B99CE-D276-4D30-84A9-7842933BDBCD" ); // Location Tree View
            DeletePage( "2BECFB85-D566-464F-B6AC-0BE90189A418" ); // Page: Named LocationsLayout: Left Sidebar Panel, Site: Rock RMS

            AddColumn("dbo.Location", "IsNamedLocation", c => c.Boolean(nullable: false));
            DropForeignKey("dbo.Location", "LocationTypeValueId", "dbo.DefinedValue");
            DropIndex("dbo.Location", new[] { "LocationTypeValueId" });
            DropColumn("dbo.Location", "IsGeoPointLocked");
        }
    }
}
