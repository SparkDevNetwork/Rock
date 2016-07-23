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
    public partial class GoogleMapStyles : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Run Down() to clean up in case the database already has it 
            Down();
            
            AddDefinedType( "Global", "Map Styles", "Defines a listing of various map styles to be used as configuration for the various blocks that render maps. Check-out http://snazzymaps.com/ for inspiration.", "4EF89471-C049-49ED-AB50-677F689A4E4E" );

            AddDefinedValue( "4EF89471-C049-49ED-AB50-677F689A4E4E", "Rock", "The default styling for Rock maps.", "FDC5D6BA-A818-4A06-96B1-9EF31B4087AC" );
            AddDefinedValue( "4EF89471-C049-49ED-AB50-677F689A4E4E", "Subtle Grayscale", @"Created by: Paulo �vila
A nice, simple grayscale version of the map with color extremes that are never too harsh on the eyes. Originally created for http://barvinssurvins.fr/situer.

http://snazzymaps.com/style/15/subtle-grayscale", "B1B95FDC-BB41-429F-A5D0-04D4D8284E2C" );
            AddDefinedValue( "4EF89471-C049-49ED-AB50-677F689A4E4E", "Apple Maps-esque", @"Created by: Mike Fowler

A theme that largely resembles the Apple Maps theme, albeit somewhat flatter.
http://snazzymaps.com/style/42/apple-maps-esque", "C67A6551-C3A7-451A-AA64-9F5159D63D3D" );
            AddDefinedValue( "4EF89471-C049-49ED-AB50-677F689A4E4E", "Map Box", @"Created by: Sam Herbert

Light blue and grey color scheme for Google Maps, inspired by MapBox's default map color.
http://snazzymaps.com/style/44/mapbox", "54DB5C2B-D099-4A89-A1C1-60FB2EF4EFE6" );
            AddDefinedValue( "4EF89471-C049-49ED-AB50-677F689A4E4E", "Google Standard", @"The standard Google maps style.", "BFC46259-FB66-4427-BF05-2B030A582BEA" );
            AddDefinedValue( "4EF89471-C049-49ED-AB50-677F689A4E4E", "Midnight Commander", @"Created by: Adam Krogh
Inspired by CloudMade's style of the same name. A dark use of water and 'Tron' like colours results in a very unique style.
http://snazzymaps.com/style/2/midnight-commander", "0965072D-D7D5-41FC-A70B-6F69AA4E9EEB" );
            AddDefinedValue( "4EF89471-C049-49ED-AB50-677F689A4E4E", "Retro", @"Created by: Google
A retro style map from Google that has a ton of detail. Looks great zoomed in on a city with lots of features.
http://snazzymaps.com/style/18/retro", "E00AC5FE-C8FF-4499-82B7-507932DE2308" );
            AddDefinedValue( "4EF89471-C049-49ED-AB50-677F689A4E4E", "Old Timey", @"Created by: Google
An old timey color scheme provided by Google on their Maps API home page. Pure awesomeness in subtle browns and greys.
http://snazzymaps.com/style/22/old-timey", "9CB28B88-57A1-484F-BA2D-641CAF727CB2" );

            Sql( @"UPDATE [DefinedValue] set [Order] = 0 where Guid = 'BFC46259-FB66-4427-BF05-2B030A582BEA'" ); // google
            Sql( @"UPDATE [DefinedValue] set [Order] = 1 where Guid = 'FDC5D6BA-A818-4A06-96B1-9EF31B4087AC'" ); // rock
            Sql( @"UPDATE [DefinedValue] set [Order] = 2 where Guid = 'B1B95FDC-BB41-429F-A5D0-04D4D8284E2C'" ); // others in alpha order...
            Sql( @"UPDATE [DefinedValue] set [Order] = 2 where Guid = 'C67A6551-C3A7-451A-AA64-9F5159D63D3D'" );
            Sql( @"UPDATE [DefinedValue] set [Order] = 2 where Guid = '54DB5C2B-D099-4A89-A1C1-60FB2EF4EFE6'" );
            Sql( @"UPDATE [DefinedValue] set [Order] = 2 where Guid = '0965072D-D7D5-41FC-A70B-6F69AA4E9EEB'" );
            Sql( @"UPDATE [DefinedValue] set [Order] = 2 where Guid = 'E00AC5FE-C8FF-4499-82B7-507932DE2308'" );
            Sql( @"UPDATE [DefinedValue] set [Order] = 2 where Guid = '9CB28B88-57A1-484F-BA2D-641CAF727CB2'" );

            Sql( @"

-- Attributes
declare
    @fieldTypeCodeEditorId int = (select [Id] from [FieldType] where [Guid] = '1D0D3794-C210-48A8-8C68-3FBEC08A6BA5'),  
    @fieldTypeTextId int = (select [Id] from [FieldType] where [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA'),
    @entityTypeDefinedValueId int = (select [Id] from [EntityType] where [Name] = 'Rock.Model.DefinedValue'),
    @definedValueEntityId int = (select [Id] from [DefinedType] where [Guid] = '4EF89471-C049-49ED-AB50-677F689A4E4E') -- Map Styles

INSERT INTO [dbo].[Attribute] ([IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],[Key],[Name],[Description],[Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[Guid])
     VALUES 
        (1,@fieldTypeCodeEditorId,@entityTypeDefinedValueId,'DefinedTypeId',@definedValueEntityId,'DynamicMapStyle','Dynamic Map Style','',0,0,null,0,1,'33AA992E-F631-48CF-9055-8B06D6EDCA66'),
        (1,@fieldTypeTextId,      @entityTypeDefinedValueId,'DefinedTypeId',@definedValueEntityId,'MarkerColor','Marker Color','The color of the marker for the map (default #FE7569).',0,0,'#FE7569',0,0,'215AC212-DEA8-412D-BEA9-06A777D20DFD'),
        (1,@fieldTypeTextId,      @entityTypeDefinedValueId,'DefinedTypeId',@definedValueEntityId,'StaticMapStyle','Static Map Style','Be sure to include the following (customizing color and fillcolor) to support adding markers and polygons:
markers=color:0x779cb1|{MarkerPoints}&path=fillcolor:0x779cb155|{PolygonPoints}',0,0,'http://maps.googleapis.com/maps/api/staticmap?markers=color:0x779cb1|{MarkerPoints}&visual_refresh=true&path=fillcolor:0x779cb155|{PolygonPoints}',0,0,'68BA76A5-A013-4273-AEC6-E928F9FC6E04')

-- Attribute Values
declare
    @attributeDynamicMapStyleId int = (select [Id] from [Attribute] where [Guid] = '33AA992E-F631-48CF-9055-8B06D6EDCA66'),
    @attributeMarkerStyleId int = (select [Id] from [Attribute] where [Guid] = '215AC212-DEA8-412D-BEA9-06A777D20DFD'),
    @attributeStaticMapStyleId int = (select [Id] from [Attribute] where [Guid] = '68BA76A5-A013-4273-AEC6-E928F9FC6E04'),
    @definedValueMapStyleRockId int = (select [Id] from [DefinedValue] where [Guid] = 'FDC5D6BA-A818-4A06-96B1-9EF31B4087AC'),
    @definedValueMapStyleSubtleGrayscaleId int = (select [Id] from [DefinedValue] where [Guid] = 'B1B95FDC-BB41-429F-A5D0-04D4D8284E2C'),
    @definedValueMapStyleAppleMapsId int = (select [Id] from [DefinedValue] where [Guid] = 'C67A6551-C3A7-451A-AA64-9F5159D63D3D'),
    @definedValueMapStyleMapBoxId int = (select [Id] from [DefinedValue] where [Guid] = '54DB5C2B-D099-4A89-A1C1-60FB2EF4EFE6'),
    @definedValueMapStyleGoogleStandardId int = (select [Id] from [DefinedValue] where [Guid] = 'BFC46259-FB66-4427-BF05-2B030A582BEA'),
    @definedValueMapStyleMidnightCommanderId int = (select [Id] from [DefinedValue] where [Guid] = '0965072D-D7D5-41FC-A70B-6F69AA4E9EEB'),
    @definedValueMapStyleRetroId int = (select [Id] from [DefinedValue] where [Guid] = 'E00AC5FE-C8FF-4499-82B7-507932DE2308'),
    @definedValueMapStyleOldTimeyId int = (select [Id] from [DefinedValue] where [Guid] = '9CB28B88-57A1-484F-BA2D-641CAF727CB2')

-- DynamicMapStyle
INSERT INTO [dbo].[AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
     VALUES 
        (1, @attributeDynamicMapStyleId, @definedValueMapStyleAppleMapsId, 0, '[
    {
        ""featureType"": ""water"",
        ""elementType"": ""geometry"",
        ""stylers"": [
            {
                ""color"": ""#a2daf2""
            }
        ]
    },
    {
        ""featureType"": ""landscape.man_made"",
        ""elementType"": ""geometry"",
        ""stylers"": [
            {
                ""color"": ""#f7f1df""
            }
        ]
    },
    {
        ""featureType"": ""landscape.natural"",
        ""elementType"": ""geometry"",
        ""stylers"": [
            {
                ""color"": ""#d0e3b4""
            }
        ]
    },
    {
        ""featureType"": ""landscape.natural.terrain"",
        ""elementType"": ""geometry"",
        ""stylers"": [
            {
                ""visibility"": ""off""
            }
        ]
    },
    {
        ""featureType"": ""poi.park"",
        ""elementType"": ""geometry"",
        ""stylers"": [
            {
                ""color"": ""#bde6ab""
            }
        ]
    },
    {
        ""featureType"": ""poi"",
        ""elementType"": ""labels"",
        ""stylers"": [
            {
                ""visibility"": ""off""
            }
        ]
    },
    {
        ""featureType"": ""poi.medical"",
        ""elementType"": ""geometry"",
        ""stylers"": [
            {
                ""color"": ""#fbd3da""
            }
        ]
    },
    {
        ""featureType"": ""poi.business"",
        ""stylers"": [
            {
                ""visibility"": ""off""
            }
        ]
    },
    {
        ""featureType"": ""road"",
        ""elementType"": ""geometry.stroke"",
        ""stylers"": [
            {
                ""visibility"": ""off""
            }
        ]
    },
    {
        ""featureType"": ""road.highway"",
        ""elementType"": ""geometry.fill"",
        ""stylers"": [
            {
                ""color"": ""#ffe15f""
            }
        ]
    },
    {
        ""featureType"": ""road.highway"",
        ""elementType"": ""geometry.stroke"",
        ""stylers"": [
            {
                ""color"": ""#efd151""
            }
        ]
    },
    {
        ""featureType"": ""road.arterial"",
        ""elementType"": ""geometry.fill"",
        ""stylers"": [
            {
                ""color"": ""#ffffff""
            }
        ]
    },
    {
        ""featureType"": ""road.local"",
        ""elementType"": ""geometry.fill"",
        ""stylers"": [
            {
                ""color"": ""black""
            }
        ]
    },
    {
        ""featureType"": ""transit.station.airport"",
        ""elementType"": ""geometry.fill"",
        ""stylers"": [
            {
                ""color"": ""#cfb2db""
            }
        ]
    }
]', '3701D619-96DF-48A4-BA1B-63E845834D9D'),        
        (1, @attributeDynamicMapStyleId, @definedValueMapStyleGoogleStandardId, 0, '[]', '1131CAEF-E433-45C7-9141-46EDBA586C30'),
        (1, @attributeDynamicMapStyleId, @definedValueMapStyleMapBoxId, 0, '[
    {
        ""featureType"": ""water"",
        ""stylers"": [
            {
                ""saturation"": 43
            },
            {
                ""lightness"": -11
            },
            {
                ""hue"": ""#0088ff""
            }
        ]
    },
    {
        ""featureType"": ""road"",
        ""elementType"": ""geometry.fill"",
        ""stylers"": [
            {
                ""hue"": ""#ff0000""
            },
            {
                ""saturation"": -100
            },
            {
                ""lightness"": 99
            }
        ]
    },
    {
        ""featureType"": ""road"",
        ""elementType"": ""geometry.stroke"",
        ""stylers"": [
            {
                ""color"": ""#808080""
            },
            {
                ""lightness"": 54
            }
        ]
    },
    {
        ""featureType"": ""landscape.man_made"",
        ""elementType"": ""geometry.fill"",
        ""stylers"": [
            {
                ""color"": ""#ece2d9""
            }
        ]
    },
    {
        ""featureType"": ""poi.park"",
        ""elementType"": ""geometry.fill"",
        ""stylers"": [
            {
                ""color"": ""#ccdca1""
            }
        ]
    },
    {
        ""featureType"": ""road"",
        ""elementType"": ""labels.text.fill"",
        ""stylers"": [
            {
                ""color"": ""#767676""
            }
        ]
    },
    {
        ""featureType"": ""road"",
        ""elementType"": ""labels.text.stroke"",
        ""stylers"": [
            {
                ""color"": ""#ffffff""
            }
        ]
    },
    {
        ""featureType"": ""poi"",
        ""stylers"": [
            {
                ""visibility"": ""off""
            }
        ]
    },
    {
        ""featureType"": ""landscape.natural"",
        ""elementType"": ""geometry.fill"",
        ""stylers"": [
            {
                ""visibility"": ""on""
            },
            {
                ""color"": ""#b8cb93""
            }
        ]
    }
]', 'F9B421BD-964D-4BF0-BE33-DCF71B884DC0'),
        (1, @attributeDynamicMapStyleId, @definedValueMapStyleMidnightCommanderId, 0, '[
    {
        ""featureType"": ""water"",
        ""stylers"": [
            {
                ""color"": ""#021019""
            }
        ]
    },
    {
        ""featureType"": ""landscape"",
        ""stylers"": [
            {
                ""color"": ""#08304b""
            }
        ]
    },
    {
        ""featureType"": ""poi"",
        ""stylers"": [
            {
                ""visibility"": ""off""
            }
        ]
    },
    {
        ""featureType"": ""poi"",
        ""elementType"": ""geometry"",
        ""stylers"": [
            {
                ""color"": ""#0c4152""
            },
            {
                ""lightness"": 5
            }
        ]
    },
    {
        ""featureType"": ""road.highway"",
        ""elementType"": ""geometry.fill"",
        ""stylers"": [
            {
                ""color"": ""#000000""
            }
        ]
    },
    {
        ""featureType"": ""road.highway"",
        ""elementType"": ""geometry.stroke"",
        ""stylers"": [
            {
                ""color"": ""#0b434f""
            },
            {
                ""lightness"": 25
            }
        ]
    },
    {
        ""featureType"": ""road.arterial"",
        ""elementType"": ""geometry.fill"",
        ""stylers"": [
            {
                ""color"": ""#000000""
            }
        ]
    },
    {
        ""featureType"": ""road.arterial"",
        ""elementType"": ""geometry.stroke"",
        ""stylers"": [
            {
                ""color"": ""#0b3d51""
            },
            {
                ""lightness"": 16
            }
        ]
    },
    {
        ""featureType"": ""road.local"",
        ""elementType"": ""geometry"",
        ""stylers"": [
            {
                ""color"": ""#000000""
            }
        ]
    },
    {
        ""elementType"": ""labels.text.fill"",
        ""stylers"": [
            {
                ""color"": ""#ffffff""
            }
        ]
    },
    {
        ""elementType"": ""labels.text.stroke"",
        ""stylers"": [
            {
                ""color"": ""#000000""
            },
            {
                ""lightness"": 13
            }
        ]
    },
    {
        ""featureType"": ""transit"",
        ""stylers"": [
            {
                ""color"": ""#146474""
            }
        ]
    },
    {
        ""featureType"": ""administrative"",
        ""elementType"": ""geometry.fill"",
        ""stylers"": [
            {
                ""color"": ""#000000""
            }
        ]
    },
    {
        ""featureType"": ""administrative"",
        ""elementType"": ""geometry.stroke"",
        ""stylers"": [
            {
                ""color"": ""#144b53""
            },
            {
                ""lightness"": 14
            },
            {
                ""weight"": 1.4
            }
        ]
    }
]', 'D1EE9A34-3A27-4AA1-A48D-EBF813FBDF5F'),
        (1, @attributeDynamicMapStyleId, @definedValueMapStyleOldTimeyId, 0, '[
    {
        ""featureType"": ""administrative"",
        ""stylers"": [
            {
                ""visibility"": ""off""
            }
        ]
    },
    {
        ""featureType"": ""poi"",
        ""stylers"": [
            {
                ""visibility"": ""off""
            }
        ]
    },
    {
        ""featureType"": ""road"",
        ""stylers"": [
            {
                ""visibility"": ""simplified""
            }
        ]
    },
    {
        ""featureType"": ""water"",
        ""stylers"": [
            {
                ""visibility"": ""simplified""
            }
        ]
    },
    {
        ""featureType"": ""transit"",
        ""stylers"": [
            {
                ""visibility"": ""simplified""
            }
        ]
    },
    {
        ""featureType"": ""landscape"",
        ""stylers"": [
            {
                ""visibility"": ""simplified""
            }
        ]
    },
    {
        ""featureType"": ""road.local"",
        ""stylers"": [
            {
                ""visibility"": ""on""
            }
        ]
    },
    {
        ""featureType"": ""road.highway"",
        ""stylers"": [
            {
                ""visibility"": ""simplified""
            }
        ]
    },
    {
        ""featureType"": ""road.highway"",
        ""elementType"": ""geometry"",
        ""stylers"": [
            {
                ""visibility"": ""on""
            }
        ]
    },
    {
        ""featureType"": ""water"",
        ""stylers"": [
            {
                ""color"": ""#84afa3""
            },
            {
                ""lightness"": 52
            }
        ]
    },
    {
        ""stylers"": [
            {
                ""saturation"": -77
            }
        ]
    },
    {
        ""featureType"": ""road""
    }
]', 'E7686215-C3C0-46DA-B6E4-770313735826'),
        (1, @attributeDynamicMapStyleId, @definedValueMapStyleRetroId, 0, '[
    {
        ""featureType"": ""administrative"",
        ""stylers"": [
            {
                ""visibility"": ""off""
            }
        ]
    },
    {
        ""featureType"": ""poi"",
        ""stylers"": [
            {
                ""visibility"": ""off""
            }
        ]
    },
    {
        ""featureType"": ""road"",
        ""elementType"": ""labels"",
        ""stylers"": [
            {
                ""visibility"": ""simplified""
            }
        ]
    },
    {
        ""featureType"": ""road.highway"",
        ""stylers"": [
            {
                ""visibility"": ""off""
            }
        ]
    },
    {
        ""featureType"": ""water"",
        ""stylers"": [
            {
                ""visibility"": ""simplified""
            }
        ]
    },
    {
        ""featureType"": ""transit"",
        ""stylers"": [
            {
                ""visibility"": ""simplified""
            }
        ]
    },
    {
        ""featureType"": ""landscape"",
        ""stylers"": [
            {
                ""visibility"": ""simplified""
            }
        ]
    },
    {
        ""featureType"": ""road.local"",
        ""stylers"": [
            {
                ""visibility"": ""on""
            }
        ]
    },
    {
        ""featureType"": ""road.highway"",
        ""elementType"": ""geometry"",
        ""stylers"": [
            {
                ""visibility"": ""on""
            }
        ]
    },
    {
        ""featureType"": ""water"",
        ""stylers"": [
            {
                ""color"": ""#84afa3""
            },
            {
                ""lightness"": 52
            }
        ]
    },
    {
        ""stylers"": [
            {
                ""saturation"": -17
            },
            {
                ""gamma"": 0.36
            }
        ]
    },
    {
        ""featureType"": ""transit.line"",
        ""elementType"": ""geometry"",
        ""stylers"": [
            {
                ""color"": ""#3f518c""
            }
        ]
    }
]', '7357CFA6-5F69-4FEC-879F-B34BA4EAE64D'),
        (1, @attributeDynamicMapStyleId, @definedValueMapStyleRockId, 0, '[
	{""featureType"": ""all"",
		""stylers"":[
			{""saturation"": 0},
			{""hue"": ""#e7ecf0""}
		]
	},
	{""featureType"": ""road"",
		""stylers"":[
			{""saturation"": -70}
		]
	},
	{""featureType"": ""transit"",
		""stylers"":[
			{""visibility"": ""off""}
		]
	},
	{""featureType"": ""poi"",
		""stylers"":[
			{""visibility"": ""off""}
		]
	},
	{""featureType"": ""water"",
		""stylers"":[
			{""visibility"": ""simplified""},
			{""saturation"": -60}
		]
	}
]', '40F1895B-C358-4F81-992A-9CF4102D28EE'),
        (1, @attributeDynamicMapStyleId, @definedValueMapStyleSubtleGrayscaleId, 0, '[
    {
        ""featureType"": ""landscape"",
        ""stylers"": [
            {
                ""saturation"": -100
            },
            {
                ""lightness"": 65
            },
            {
                ""visibility"": ""on""
            }
        ]
    },
    {
        ""featureType"": ""poi"",
        ""stylers"": [
            {
                ""saturation"": -100
            },
            {
                ""lightness"": 51
            },
            {
                ""visibility"": ""simplified""
            }
        ]
    },
    {
        ""featureType"": ""road.highway"",
        ""stylers"": [
            {
                ""saturation"": -100
            },
            {
                ""visibility"": ""simplified""
            }
        ]
    },
    {
        ""featureType"": ""road.arterial"",
        ""stylers"": [
            {
                ""saturation"": -100
            },
            {
                ""lightness"": 30
            },
            {
                ""visibility"": ""on""
            }
        ]
    },
    {
        ""featureType"": ""road.local"",
        ""stylers"": [
            {
                ""saturation"": -100
            },
            {
                ""lightness"": 40
            },
            {
                ""visibility"": ""on""
            }
        ]
    },
    {
        ""featureType"": ""transit"",
        ""stylers"": [
            {
                ""saturation"": -100
            },
            {
                ""visibility"": ""simplified""
            }
        ]
    },
    {
        ""featureType"": ""administrative.province"",
        ""stylers"": [
            {
                ""visibility"": ""off""
            }
        ]
    },
    {
        ""featureType"": ""water"",
        ""elementType"": ""labels"",
        ""stylers"": [
            {
                ""visibility"": ""on""
            },
            {
                ""lightness"": -25
            },
            {
                ""saturation"": -100
            }
        ]
    },
    {
        ""featureType"": ""water"",
        ""elementType"": ""geometry"",
        ""stylers"": [
            {
                ""hue"": ""#ffff00""
            },
            {
                ""lightness"": -25
            },
            {
                ""saturation"": -97
            }
        ]
    }
]', 'EBE33EEA-A048-43E7-8AEC-71D666A0F9BF')

-- Marker Color
INSERT INTO [dbo].[AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
     VALUES 
        (1, @attributeMarkerStyleId, @definedValueMapStyleAppleMapsId, 0, '#0056c0', 'C0100D7E-27A6-4304-9501-C2B31D70891D'),        
        (1, @attributeMarkerStyleId, @definedValueMapStyleGoogleStandardId, 0, '#FE7569', '47B068F6-39F9-4103-82E5-ABC84543F922'),
        (1, @attributeMarkerStyleId, @definedValueMapStyleMapBoxId, 0, '#FE7569', 'C8C17CA3-685A-4331-A71C-76371D7B46E5'),
        (1, @attributeMarkerStyleId, @definedValueMapStyleMidnightCommanderId, 0, '#e8f64e', '692E08E1-3849-44E7-A736-72B45863DA02'),
        (1, @attributeMarkerStyleId, @definedValueMapStyleOldTimeyId, 0, '#79996e', 'E08DF679-891B-4FAD-8AB0-6060693D1C6C'),
        (1, @attributeMarkerStyleId, @definedValueMapStyleRetroId, 0, '#b26b00', '9144664A-2161-4BB9-A9B5-29D6DAFCCF72'),
        (1, @attributeMarkerStyleId, @definedValueMapStyleRockId, 0, '#779cb1', '140D6EE0-C2B0-4706-A464-BE63A9C775DF'),
        (1, @attributeMarkerStyleId, @definedValueMapStyleSubtleGrayscaleId, 0, '#d01f22', 'CCD87C47-FEF2-4A96-BBC3-E8300234D838')

-- StaticMapStyle
INSERT INTO [dbo].[AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
     VALUES 
        (1, @attributeStaticMapStyleId, @definedValueMapStyleAppleMapsId, 0,         'http://maps.googleapis.com/maps/api/staticmap?style=feature:water|element:geometry|color:0xa2daf2|&style=feature:landscape.man_made|element:geometry|color:0xf7f1df|&style=feature:landscape.natural|element:geometry|color:0xd0e3b4|&style=feature:landscape.natural.terrain|element:geometry|visibility:off|&style=feature:poi.park|element:geometry|color:0xbde6ab|&style=feature:poi|element:labels|visibility:off|&style=feature:poi.medical|element:geometry|color:0xfbd3da|&style=feature:poi.business|element:all|visibility:off|&style=feature:road|element:geometry.stroke|visibility:off|&style=feature:road.highway|element:geometry.fill|color:0xffe15f|&style=feature:road.highway|element:geometry.stroke|color:0xefd151|&style=feature:road.arterial|element:geometry.fill|color:0xffffff|&style=feature:road.local|element:geometry.fill|color:black|&style=feature:transit.station.airport|element:geometry.fill|color:0xcfb2db|&markers=color:0x0056c0|{MarkerPoints}&visual_refresh=true&path=fillcolor:0x73a8e955|{PolygonPoints}','C13B38D1-C4BB-4538-A539-F828F3C42060'),       
        (1, @attributeStaticMapStyleId, @definedValueMapStyleGoogleStandardId, 0,    'http://maps.googleapis.com/maps/api/staticmap?markers=color:0x779cb1|{MarkerPoints}&visual_refresh=true&path=fillcolor:0x779cb155|{PolygonPoints}','0F345909-50FA-46A3-AD0E-DAD52FE95C5B'),
        (1, @attributeStaticMapStyleId, @definedValueMapStyleMapBoxId, 0,            'http://maps.googleapis.com/maps/api/staticmap?style=feature:water|element:all|saturation:43|lightness:-11|hue:0x0088ff|&style=feature:road|element:geometry.fill|hue:0xff0000|saturation:-100|lightness:99|&style=feature:road|element:geometry.stroke|color:0x808080|lightness:54|&style=feature:landscape.man_made|element:geometry.fill|color:0xece2d9|&style=feature:poi.park|element:geometry.fill|color:0xccdca1|&style=feature:road|element:labels.text.fill|color:0x767676|&style=feature:road|element:labels.text.stroke|color:0xffffff|&style=feature:poi|element:all|visibility:off|&style=feature:landscape.natural|element:geometry.fill|visibility:on|color:0xb8cb93|&markers=color:0xFE7569|{MarkerPoints}&visual_refresh=true&path=fillcolor:0x8a8b8b55|{PolygonPoints}','2B1E755B-E31D-4932-8C18-29A511A570BF'),
        (1, @attributeStaticMapStyleId, @definedValueMapStyleMidnightCommanderId, 0, 'http://maps.googleapis.com/maps/api/staticmap?style=feature:water|element:all|color:0x021019|&style=feature:landscape|element:all|color:0x08304b|&style=feature:poi|element:all|visibility:off|&style=feature:poi|element:geometry|color:0x0c4152|lightness:5|&style=feature:road.highway|element:geometry.fill|color:0x000000|&style=feature:road.highway|element:geometry.stroke|color:0x0b434f|lightness:25|&style=feature:road.arterial|element:geometry.fill|color:0x000000|&style=feature:road.arterial|element:geometry.stroke|color:0x0b3d51|lightness:16|&style=feature:road.local|element:geometry|color:0x000000|&style=feature:all|element:labels.text.fill|color:0xffffff|&style=feature:all|element:labels.text.stroke|color:0x000000|lightness:13|&style=feature:transit|element:all|color:0x146474|&style=feature:administrative|element:geometry.fill|color:0x000000|&style=feature:administrative|element:geometry.stroke|color:0x144b53|lightness:14|weight:1.4|&markers=color:0xe8f64e|{MarkerPoints}&visual_refresh=true&path=fillcolor:0xf5fbae20|{PolygonPoints}','A9B4BCD8-DF5B-4652-8CD7-B8B8B371089F'),
        (1, @attributeStaticMapStyleId, @definedValueMapStyleOldTimeyId, 0,          'http://maps.googleapis.com/maps/api/staticmap?style=feature:administrative|element:all|visibility:off|&style=feature:poi|element:all|visibility:off|&style=feature:road|element:all|visibility:simplified|&style=feature:water|element:all|visibility:simplified|&style=feature:transit|element:all|visibility:simplified|&style=feature:landscape|element:all|visibility:simplified|&style=feature:road.local|element:all|visibility:on|&style=feature:road.highway|element:all|visibility:simplified|&style=feature:road.highway|element:geometry|visibility:on|&style=feature:water|element:all|color:0x84afa3|lightness:52|&style=feature:all|element:all|saturation:-77|&style=&markers=color:0x79996e|{MarkerPoints}&visual_refresh=true&path=fillcolor:0x9eaf9855|{PolygonPoints}','6166F5A0-3CEB-4C9D-91A6-330A4B2B12A0'),
        (1, @attributeStaticMapStyleId, @definedValueMapStyleRetroId, 0,             'http://maps.googleapis.com/maps/api/staticmap?style=feature:administrative|element:all|visibility:off|&style=feature:poi|element:all|visibility:off|&style=feature:road|element:labels|visibility:simplified|&style=feature:road.highway|element:all|visibility:off|&style=feature:water|element:all|visibility:simplified|&style=feature:transit|element:all|visibility:simplified|&style=feature:landscape|element:all|visibility:simplified|&style=feature:road.local|element:all|visibility:on|&style=feature:road.highway|element:geometry|visibility:on|&style=feature:water|element:all|color:0x84afa3|lightness:52|&style=feature:all|element:all|saturation:-17|gamma:0.36|&style=feature:transit.line|element:geometry|color:0x3f518c|&markers=color:0xb26b00|{MarkerPoints}&visual_refresh=true&path=fillcolor:0xb26b0055|{PolygonPoints}','D3C38B8E-A9BA-4A2D-A758-BD567A73610F'),
        (1, @attributeStaticMapStyleId, @definedValueMapStyleRockId, 0,              'http://maps.googleapis.com/maps/api/staticmap?style=feature:all|element:all|saturation:0|hue:0xe7ecf0|&style=feature:road|element:all|saturation:-70|&style=feature:transit|element:all|visibility:off|&style=feature:poi|element:all|visibility:off|&style=feature:water|element:all|visibility:simplified|saturation:-60|&markers=color:0x779cb1|{MarkerPoints}&visual_refresh=true&path=fillcolor:0x779cb155|{PolygonPoints}','C45676C4-3864-4970-B8E3-4E088C940AC7'),
        (1, @attributeStaticMapStyleId, @definedValueMapStyleSubtleGrayscaleId, 0,   'http://maps.googleapis.com/maps/api/staticmap?style=feature:landscape|element:all|saturation:-100|lightness:65|visibility:on|&style=feature:poi|element:all|saturation:-100|lightness:51|visibility:simplified|&style=feature:road.highway|element:all|saturation:-100|visibility:simplified|&style=feature:road.arterial|element:all|saturation:-100|lightness:30|visibility:on|&style=feature:road.local|element:all|saturation:-100|lightness:40|visibility:on|&style=feature:transit|element:all|saturation:-100|visibility:simplified|&style=feature:administrative.province|element:all|visibility:off|&style=feature:water|element:labels|visibility:on|lightness:-25|saturation:-100|&style=feature:water|element:geometry|hue:0xffff00|lightness:-25|saturation:-97|&markers=color:0xd01f22|{MarkerPoints}&visual_refresh=true&path=fillcolor:0xf2949655|{PolygonPoints}','716B5275-4032-4B4E-B55C-E347E93EEEC5')

" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"delete from Attribute where Guid in ('33AA992E-F631-48CF-9055-8B06D6EDCA66','215AC212-DEA8-412D-BEA9-06A777D20DFD','68BA76A5-A013-4273-AEC6-E928F9FC6E04')");
            
            DeleteDefinedValue( "FDC5D6BA-A818-4A06-96B1-9EF31B4087AC" );
            DeleteDefinedValue( "B1B95FDC-BB41-429F-A5D0-04D4D8284E2C" );
            DeleteDefinedValue( "C67A6551-C3A7-451A-AA64-9F5159D63D3D" );
            DeleteDefinedValue( "54DB5C2B-D099-4A89-A1C1-60FB2EF4EFE6" );
            DeleteDefinedValue( "BFC46259-FB66-4427-BF05-2B030A582BEA" );
            DeleteDefinedValue( "0965072D-D7D5-41FC-A70B-6F69AA4E9EEB" );
            DeleteDefinedValue( "E00AC5FE-C8FF-4499-82B7-507932DE2308" );
            DeleteDefinedValue( "9CB28B88-57A1-484F-BA2D-641CAF727CB2" );
            
            DeleteDefinedType( "4EF89471-C049-49ED-AB50-677F689A4E4E" );
        }
    }
}
