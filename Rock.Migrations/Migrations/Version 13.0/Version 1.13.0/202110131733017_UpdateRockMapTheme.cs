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
    public partial class UpdateRockMapTheme : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddDefinedValueAttributeValue("FDC5D6BA-A818-4A06-96B1-9EF31B4087AC","215AC212-DEA8-412D-BEA9-06A777D20DFD",@"#ED5151|#149ECE|#A7C636|#9E559C|#FC921F|");
            RockMigrationHelper.AddDefinedValueAttributeValue("FDC5D6BA-A818-4A06-96B1-9EF31B4087AC","33AA992E-F631-48CF-9055-8B06D6EDCA66",@"[
    {
        ""elementType"": ""labels.text.stroke"",
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
        ""elementType"": ""geometry"",
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
                ""visibility"": ""on""
            },
        
            {
                ""color"": ""#c6dfec""
            }
        ]
    },
    {
        ""featureType"": ""administrative.neighborhood"",
        ""elementType"": ""labels.text.fill"",
        ""stylers"": [
            {
                ""visibility"": ""off""
            }
        ]
    },
    {
        ""featureType"": ""road.local"",
        ""elementType"": ""labels.text"",
        ""stylers"": [
            {
                ""weight"": 0.5
            },
            {
                ""color"": ""#333333""
            }
        ]
    },
    {
        ""featureType"": ""transit.station"",
        ""elementType"": ""labels.icon"",
        ""stylers"": [
            {
                ""visibility"": ""off""
            }
        ]
    }
]");
            RockMigrationHelper.AddDefinedValueAttributeValue("FDC5D6BA-A818-4A06-96B1-9EF31B4087AC","68BA76A5-A013-4273-AEC6-E928F9FC6E04",@"//maps.googleapis.com/maps/api/staticmap?markers=color:0xeb4034|{MarkerPoints}&visual_refresh=true&path=fillcolor:0x779cb155|color:0x779cb155|{PolygonPoints}&style=feature:poi|visibility:off&style=feature:administrative|visibility:off&scale=2");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
