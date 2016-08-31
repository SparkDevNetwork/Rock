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
    public partial class WorkflowActionCriteria : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.WorkflowActionType", "CriteriaAttributeGuid", c => c.Guid());
            AddColumn("dbo.WorkflowActionType", "CriteriaComparisonType", c => c.Int(nullable: false));
            AddColumn("dbo.WorkflowActionType", "CriteriaValue", c => c.String(maxLength: 100));
            AlterColumn("dbo.WorkflowActionForm", "Actions", c => c.String(maxLength: 2000));

            Sql( @"
  UPDATE [Page]
  SET [PageTitle] = 'Site Detail'
  WHERE [Guid] = 'A2991117-0B85-4209-9008-254929C6E00F'
" );

            Sql( @"
    UPDATE [AttributeValue]
      SET [Value] = '[
        {
            ""stylers"": [
                {
                    ""saturation"": -100
                },
                {
                    ""gamma"": 1
                }
            ]
        },
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
    ]' WHERE [Guid] = '40F1895B-C358-4F81-992A-9CF4102D28EE'

    UPDATE [AttributeValue]
      SET [Value] = 'http://maps.googleapis.com/maps/api/staticmap?style=feature:all|element:all|saturation:-100|gamma:1|&style=feature:all|element:labels.text.stroke|visibility:off|&style=feature:poi|element:all|visibility:off|&style=feature:road|element:geometry|visibility:simplified|&style=feature:water|element:all|visibility:on|color:0xc6dfec|&style=feature:administrative.neighborhood|element:labels.text.fill|visibility:off|&style=feature:road.local|element:labels.text|weight:0.5|color:0x333333|&style=feature:transit.station|element:labels.icon|visibility:off|&markers=color:0xe71e22|{MarkerPoints}&visual_refresh=true&path=fillcolor:0xe71e2255|color:0xe71e2255|{PolygonPoints}'
      WHERE [Guid] = 'C45676C4-3864-4970-B8E3-4E088C940AC7'

    UPDATE [AttributeValue]
      SET [Value] = '#e71e22'
      WHERE [Guid] = '140D6EE0-C2B0-4706-A464-BE63A9C775DF'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AlterColumn("dbo.WorkflowActionForm", "Actions", c => c.String(maxLength: 300));
            DropColumn("dbo.WorkflowActionType", "CriteriaValue");
            DropColumn("dbo.WorkflowActionType", "CriteriaComparisonType");
            DropColumn("dbo.WorkflowActionType", "CriteriaAttributeGuid");
        }
    }
}
