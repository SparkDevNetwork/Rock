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
    public partial class GroupMapStyles : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DeleteAttribute( "0D459868-02FD-4AB7-9A9C-92ACFCBB0FDC" );

            UpdateBlockTypeAttribute( "582BEEA1-5B27-444D-BC0A-F60CEB053981", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Map Style", "MapStyle", "", "The style of maps to use", 4, "", "E50B6C24-930C-4D9C-BD94-0AD6BC018C4D" );
            AddBlockAttributeValue( "88344FE3-737E-4741-A38D-D2D3A1653818", "E50B6C24-930C-4D9C-BD94-0AD6BC018C4D", "FDC5D6BA-A818-4A06-96B1-9EF31B4087AC" );
            AddBlockAttributeValue( "B58919B6-0947-4FE6-A9AE-FB28194643E7", "E50B6C24-930C-4D9C-BD94-0AD6BC018C4D", "FDC5D6BA-A818-4A06-96B1-9EF31B4087AC" );

            Sql( @"
    UPDATE [AttributeValue] SET [Value] = 'http://maps.googleapis.com/maps/api/staticmap?style=feature:water|element:geometry|color:0xa2daf2|&style=feature:landscape.man_made|element:geometry|color:0xf7f1df|&style=feature:landscape.natural|element:geometry|color:0xd0e3b4|&style=feature:landscape.natural.terrain|element:geometry|visibility:off|&style=feature:poi.park|element:geometry|color:0xbde6ab|&style=feature:poi|element:labels|visibility:off|&style=feature:poi.medical|element:geometry|color:0xfbd3da|&style=feature:poi.business|element:all|visibility:off|&style=feature:road|element:geometry.stroke|visibility:off|&style=feature:road.highway|element:geometry.fill|color:0xffe15f|&style=feature:road.highway|element:geometry.stroke|color:0xefd151|&style=feature:road.arterial|element:geometry.fill|color:0xffffff|&style=feature:road.local|element:geometry.fill|color:black|&style=feature:transit.station.airport|element:geometry.fill|color:0xcfb2db|&markers=color:0x0056c0|{MarkerPoints}&visual_refresh=true&path=fillcolor:0x73a8e955|color:0x73a8e955|{PolygonPoints}' WHERE [Guid] = 'C13B38D1-C4BB-4538-A539-F828F3C42060'
    UPDATE [AttributeValue] SET [Value] = 'http://maps.googleapis.com/maps/api/staticmap?markers=color:0x779cb1|{MarkerPoints}&visual_refresh=true&path=fillcolor:0x779cb155|color:0x779cb155|{PolygonPoints}' WHERE [Guid] = '0F345909-50FA-46A3-AD0E-DAD52FE95C5B'
    UPDATE [AttributeValue] SET [Value] = 'http://maps.googleapis.com/maps/api/staticmap?style=feature:water|element:all|saturation:43|lightness:-11|hue:0x0088ff|&style=feature:road|element:geometry.fill|hue:0xff0000|saturation:-100|lightness:99|&style=feature:road|element:geometry.stroke|color:0x808080|lightness:54|&style=feature:landscape.man_made|element:geometry.fill|color:0xece2d9|&style=feature:poi.park|element:geometry.fill|color:0xccdca1|&style=feature:road|element:labels.text.fill|color:0x767676|&style=feature:road|element:labels.text.stroke|color:0xffffff|&style=feature:poi|element:all|visibility:off|&style=feature:landscape.natural|element:geometry.fill|visibility:on|color:0xb8cb93|&markers=color:0xFE7569|{MarkerPoints}&visual_refresh=true&path=fillcolor:0x8a8b8b55|color:0x8a8b8b55|{PolygonPoints}' WHERE [Guid] = '2B1E755B-E31D-4932-8C18-29A511A570BF'
    UPDATE [AttributeValue] SET [Value] = 'http://maps.googleapis.com/maps/api/staticmap?style=feature:water|element:all|color:0x021019|&style=feature:landscape|element:all|color:0x08304b|&style=feature:poi|element:all|visibility:off|&style=feature:poi|element:geometry|color:0x0c4152|lightness:5|&style=feature:road.highway|element:geometry.fill|color:0x000000|&style=feature:road.highway|element:geometry.stroke|color:0x0b434f|lightness:25|&style=feature:road.arterial|element:geometry.fill|color:0x000000|&style=feature:road.arterial|element:geometry.stroke|color:0x0b3d51|lightness:16|&style=feature:road.local|element:geometry|color:0x000000|&style=feature:all|element:labels.text.fill|color:0xffffff|&style=feature:all|element:labels.text.stroke|color:0x000000|lightness:13|&style=feature:transit|element:all|color:0x146474|&style=feature:administrative|element:geometry.fill|color:0x000000|&style=feature:administrative|element:geometry.stroke|color:0x144b53|lightness:14|weight:1.4|&markers=color:0xe8f64e|{MarkerPoints}&visual_refresh=true&path=fillcolor:0xf5fbae20|color:0xf5fbae20|{PolygonPoints}' WHERE [Guid] = 'A9B4BCD8-DF5B-4652-8CD7-B8B8B371089F'
    UPDATE [AttributeValue] SET [Value] = 'http://maps.googleapis.com/maps/api/staticmap?style=feature:administrative|element:all|visibility:off|&style=feature:poi|element:all|visibility:off|&style=feature:road|element:all|visibility:simplified|&style=feature:water|element:all|visibility:simplified|&style=feature:transit|element:all|visibility:simplified|&style=feature:landscape|element:all|visibility:simplified|&style=feature:road.local|element:all|visibility:on|&style=feature:road.highway|element:all|visibility:simplified|&style=feature:road.highway|element:geometry|visibility:on|&style=feature:water|element:all|color:0x84afa3|lightness:52|&style=feature:all|element:all|saturation:-77|&style=&markers=color:0x79996e|{MarkerPoints}&visual_refresh=true&path=fillcolor:0x9eaf9855|color:0x9eaf9855|{PolygonPoints}' WHERE [Guid] = '6166F5A0-3CEB-4C9D-91A6-330A4B2B12A0'
    UPDATE [AttributeValue] SET [Value] = 'http://maps.googleapis.com/maps/api/staticmap?style=feature:administrative|element:all|visibility:off|&style=feature:poi|element:all|visibility:off|&style=feature:road|element:labels|visibility:simplified|&style=feature:road.highway|element:all|visibility:off|&style=feature:water|element:all|visibility:simplified|&style=feature:transit|element:all|visibility:simplified|&style=feature:landscape|element:all|visibility:simplified|&style=feature:road.local|element:all|visibility:on|&style=feature:road.highway|element:geometry|visibility:on|&style=feature:water|element:all|color:0x84afa3|lightness:52|&style=feature:all|element:all|saturation:-17|gamma:0.36|&style=feature:transit.line|element:geometry|color:0x3f518c|&markers=color:0xb26b00|{MarkerPoints}&visual_refresh=true&path=fillcolor:0xb26b0055|color:0xb26b0055|{PolygonPoints}' WHERE [Guid] = 'D3C38B8E-A9BA-4A2D-A758-BD567A73610F'
    UPDATE [AttributeValue] SET [Value] = 'http://maps.googleapis.com/maps/api/staticmap?style=feature:all|element:all|saturation:0|hue:0xe7ecf0|&style=feature:road|element:all|saturation:-70|&style=feature:transit|element:all|visibility:off|&style=feature:poi|element:all|visibility:off|&style=feature:water|element:all|visibility:simplified|saturation:-60|&markers=color:0x779cb1|{MarkerPoints}&visual_refresh=true&path=fillcolor:0x779cb155|color:0x779cb155|{PolygonPoints}' WHERE [Guid] = 'C45676C4-3864-4970-B8E3-4E088C940AC7'
    UPDATE [AttributeValue] SET [Value] = 'http://maps.googleapis.com/maps/api/staticmap?style=feature:landscape|element:all|saturation:-100|lightness:65|visibility:on|&style=feature:poi|element:all|saturation:-100|lightness:51|visibility:simplified|&style=feature:road.highway|element:all|saturation:-100|visibility:simplified|&style=feature:road.arterial|element:all|saturation:-100|lightness:30|visibility:on|&style=feature:road.local|element:all|saturation:-100|lightness:40|visibility:on|&style=feature:transit|element:all|saturation:-100|visibility:simplified|&style=feature:administrative.province|element:all|visibility:off|&style=feature:water|element:labels|visibility:on|lightness:-25|saturation:-100|&style=feature:water|element:geometry|hue:0xffff00|lightness:-25|saturation:-97|&markers=color:0xd01f22|{MarkerPoints}&visual_refresh=true&path=fillcolor:0xf2949655|color:0xf2949655|{PolygonPoints}' WHERE [Guid] = '716B5275-4032-4B4E-B55C-E347E93EEEC5'
" );

            AddGlobalAttribute( "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "Currency Symbol", "The currency symbol to use for money fields", 0, "$", "9DA83BD4-772E-451B-9D0E-EF563700601D" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "9DA83BD4-772E-451B-9D0E-EF563700601D" );

            DeleteBlockAttribute( "E50B6C24-930C-4D9C-BD94-0AD6BC018C4D" );

            // Attrib for BlockType: Group Detail:Map HTML
            AddBlockTypeAttribute( "582BEEA1-5B27-444D-BC0A-F60CEB053981", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Map HTML", "MapHTML", "", "The HTML to use for displaying group location maps. Liquid syntax is used to render data from the following data structure: points[type, latitude, longitude], polygons[type, polygon_wkt, google_encoded_polygon]", 3, @"
    {% for point in points %}
        <div class='group-location-map'>
            <h4>{{ point.type }}</h4>
            <img src='http://maps.googleapis.com/maps/api/staticmap?sensor=false&size=350x200&zoom=13&format=png&style=feature:all|saturation:0|hue:0xe7ecf0&style=feature:road|saturation:-70&style=feature:transit|visibility:off&style=feature:poi|visibility:off&style=feature:water|visibility:simplified|saturation:-60&markers=color:0x779cb1|{{ point.latitude }},{{ point.longitude }}&visual_refresh=true'/>
        </div>
    {% endfor %}
    {% for polygon in polygons %}
        <div class='group-location-map'>
            <h4>{{ polygon.type }}</h4>
            <img src='http://maps.googleapis.com/maps/api/staticmap?sensor=false&size=350x200&format=png&style=feature:all|saturation:0|hue:0xe7ecf0&style=feature:road|saturation:-70&style=feature:transit|visibility:off&style=feature:poi|visibility:off&style=feature:water|visibility:simplified|saturation:-60&visual_refresh=true&path=fillcolor:0x779cb155|color:0xFFFFFF00|enc:{{ polygon.google_encoded_polygon }}'/>
        </div>
    {% endfor %}
", "0D459868-02FD-4AB7-9A9C-92ACFCBB0FDC" );
        }
    }
}
