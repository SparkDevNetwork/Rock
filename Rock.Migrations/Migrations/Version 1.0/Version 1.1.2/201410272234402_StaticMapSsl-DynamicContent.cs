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
    public partial class StaticMapSslDynamicContent : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // update static map urls to work with both http and https
            Sql( @"
                    UPDATE [AttributeValue] SET [Value] = '//maps.googleapis.com/maps/api/staticmap?style=feature:water|element:geometry|color:0xa2daf2|&style=feature:landscape.man_made|element:geometry|color:0xf7f1df|&style=feature:landscape.natural|element:geometry|color:0xd0e3b4|&style=feature:landscape.natural.terrain|element:geometry|visibility:off|&style=feature:poi.park|element:geometry|color:0xbde6ab|&style=feature:poi|element:labels|visibility:off|&style=feature:poi.medical|element:geometry|color:0xfbd3da|&style=feature:poi.business|element:all|visibility:off|&style=feature:road|element:geometry.stroke|visibility:off|&style=feature:road.highway|element:geometry.fill|color:0xffe15f|&style=feature:road.highway|element:geometry.stroke|color:0xefd151|&style=feature:road.arterial|element:geometry.fill|color:0xffffff|&style=feature:road.local|element:geometry.fill|color:black|&style=feature:transit.station.airport|element:geometry.fill|color:0xcfb2db|&markers=color:0x0056c0|{MarkerPoints}&visual_refresh=true&path=fillcolor:0x73a8e955|color:0x73a8e955|{PolygonPoints}' WHERE [Guid] = 'C13B38D1-C4BB-4538-A539-F828F3C42060'
                    UPDATE [AttributeValue] SET [Value] = '//maps.googleapis.com/maps/api/staticmap?markers=color:0x779cb1|{MarkerPoints}&visual_refresh=true&path=fillcolor:0x779cb155|color:0x779cb155|{PolygonPoints}' WHERE [Guid] = '0F345909-50FA-46A3-AD0E-DAD52FE95C5B'
                    UPDATE [AttributeValue] SET [Value] = '//maps.googleapis.com/maps/api/staticmap?style=feature:water|element:all|saturation:43|lightness:-11|hue:0x0088ff|&style=feature:road|element:geometry.fill|hue:0xff0000|saturation:-100|lightness:99|&style=feature:road|element:geometry.stroke|color:0x808080|lightness:54|&style=feature:landscape.man_made|element:geometry.fill|color:0xece2d9|&style=feature:poi.park|element:geometry.fill|color:0xccdca1|&style=feature:road|element:labels.text.fill|color:0x767676|&style=feature:road|element:labels.text.stroke|color:0xffffff|&style=feature:poi|element:all|visibility:off|&style=feature:landscape.natural|element:geometry.fill|visibility:on|color:0xb8cb93|&markers=color:0xFE7569|{MarkerPoints}&visual_refresh=true&path=fillcolor:0x8a8b8b55|color:0x8a8b8b55|{PolygonPoints}' WHERE [Guid] = '2B1E755B-E31D-4932-8C18-29A511A570BF'
                    UPDATE [AttributeValue] SET [Value] = '//maps.googleapis.com/maps/api/staticmap?style=feature:water|element:all|color:0x021019|&style=feature:landscape|element:all|color:0x08304b|&style=feature:poi|element:all|visibility:off|&style=feature:poi|element:geometry|color:0x0c4152|lightness:5|&style=feature:road.highway|element:geometry.fill|color:0x000000|&style=feature:road.highway|element:geometry.stroke|color:0x0b434f|lightness:25|&style=feature:road.arterial|element:geometry.fill|color:0x000000|&style=feature:road.arterial|element:geometry.stroke|color:0x0b3d51|lightness:16|&style=feature:road.local|element:geometry|color:0x000000|&style=feature:all|element:labels.text.fill|color:0xffffff|&style=feature:all|element:labels.text.stroke|color:0x000000|lightness:13|&style=feature:transit|element:all|color:0x146474|&style=feature:administrative|element:geometry.fill|color:0x000000|&style=feature:administrative|element:geometry.stroke|color:0x144b53|lightness:14|weight:1.4|&markers=color:0xe8f64e|{MarkerPoints}&visual_refresh=true&path=fillcolor:0xf5fbae20|color:0xf5fbae20|{PolygonPoints}' WHERE [Guid] = 'A9B4BCD8-DF5B-4652-8CD7-B8B8B371089F'
                    UPDATE [AttributeValue] SET [Value] = '//maps.googleapis.com/maps/api/staticmap?style=feature:administrative|element:all|visibility:off|&style=feature:poi|element:all|visibility:off|&style=feature:road|element:all|visibility:simplified|&style=feature:water|element:all|visibility:simplified|&style=feature:transit|element:all|visibility:simplified|&style=feature:landscape|element:all|visibility:simplified|&style=feature:road.local|element:all|visibility:on|&style=feature:road.highway|element:all|visibility:simplified|&style=feature:road.highway|element:geometry|visibility:on|&style=feature:water|element:all|color:0x84afa3|lightness:52|&style=feature:all|element:all|saturation:-77|&style=&markers=color:0x79996e|{MarkerPoints}&visual_refresh=true&path=fillcolor:0x9eaf9855|color:0x9eaf9855|{PolygonPoints}' WHERE [Guid] = '6166F5A0-3CEB-4C9D-91A6-330A4B2B12A0'
                    UPDATE [AttributeValue] SET [Value] = '//maps.googleapis.com/maps/api/staticmap?style=feature:administrative|element:all|visibility:off|&style=feature:poi|element:all|visibility:off|&style=feature:road|element:labels|visibility:simplified|&style=feature:road.highway|element:all|visibility:off|&style=feature:water|element:all|visibility:simplified|&style=feature:transit|element:all|visibility:simplified|&style=feature:landscape|element:all|visibility:simplified|&style=feature:road.local|element:all|visibility:on|&style=feature:road.highway|element:geometry|visibility:on|&style=feature:water|element:all|color:0x84afa3|lightness:52|&style=feature:all|element:all|saturation:-17|gamma:0.36|&style=feature:transit.line|element:geometry|color:0x3f518c|&markers=color:0xb26b00|{MarkerPoints}&visual_refresh=true&path=fillcolor:0xb26b0055|color:0xb26b0055|{PolygonPoints}' WHERE [Guid] = 'D3C38B8E-A9BA-4A2D-A758-BD567A73610F'
                    UPDATE [AttributeValue] SET [Value] = '//maps.googleapis.com/maps/api/staticmap?style=feature:landscape|element:all|saturation:-100|lightness:65|visibility:on|&style=feature:poi|element:all|saturation:-100|lightness:51|visibility:simplified|&style=feature:road.highway|element:all|saturation:-100|visibility:simplified|&style=feature:road.arterial|element:all|saturation:-100|lightness:30|visibility:on|&style=feature:road.local|element:all|saturation:-100|lightness:40|visibility:on|&style=feature:transit|element:all|saturation:-100|visibility:simplified|&style=feature:administrative.province|element:all|visibility:off|&style=feature:water|element:labels|visibility:on|lightness:-25|saturation:-100|&style=feature:water|element:geometry|hue:0xffff00|lightness:-25|saturation:-97|&markers=color:0xd01f22|{MarkerPoints}&visual_refresh=true&path=fillcolor:0xf2949655|color:0xf2949655|{PolygonPoints}' WHERE [Guid] = '716B5275-4032-4B4E-B55C-E347E93EEEC5'
                    UPDATE [AttributeValue] SET [Value] = '//maps.googleapis.com/maps/api/staticmap?style=feature:all|element:all|saturation:-100|gamma:1|&style=feature:all|element:labels.text.stroke|visibility:off|&style=feature:poi|element:all|visibility:off|&style=feature:road|element:geometry|visibility:simplified|&style=feature:water|element:all|visibility:on|color:0xc6dfec|&style=feature:administrative.neighborhood|element:labels.text.fill|visibility:off|&style=feature:road.local|element:labels.text|weight:0.5|color:0x333333|&style=feature:transit.station|element:labels.icon|visibility:off|&markers=color:0xee7624|{MarkerPoints}&visual_refresh=true&path=fillcolor:0xe71e2255|color:0xe71e2255|{PolygonPoints}' WHERE [Guid] = 'C45676C4-3864-4970-B8E3-4E088C940AC7'
            " );

            // fixed person emails in workflows
            Sql( @"
                  DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '9F5F7CEC-F369-4FDF-802A-99074CE7A7FC')
                  DECLARE @ActionId int = (SELECT TOP 1 [Id] FROM [WorkflowActionType] WHERE [Guid] = 'FF1538E8-2357-4E47-9735-D382622FF3FD')

                  UPDATE [AttributeValue]
	                SET [Value] = 'info@rocksolidchurchdemo.com'
	                WHERE [AttributeId] = @AttributeId AND [EntityId] = @ActionId
            " );

            Sql( @"
                  DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '0C4C13B8-7076-4872-925A-F950886B5E16')
                  DECLARE @ActionId int = (SELECT TOP 1 [Id] FROM [WorkflowActionType] WHERE [Guid] = '72269A5F-AB1C-4756-9D28-A3CE9051AD13')

                  UPDATE [AttributeValue]
	                SET [Value] = 'hr@rocksolidchurchdemo.com'
	                WHERE [AttributeId] = @AttributeId AND [EntityId] = @ActionId
            " );

            // set ad rotator to use theme's liquid
            Sql( @"
                  DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '8026FEA1-35C1-41CF-9D09-E8B1DB6CBDA8')
                  DECLARE @BlockId int = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '095027CB-9114-4CD5-ABE8-1E8882422DCF')

                  UPDATE [AttributeValue]
	                SET [Value] = '{% include ''AdRotator'' %}'
	                WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId AND [ModifiedDateTime] IS NULL
            " );

            // set ad list to use theme's liquid
            Sql( @"
                  DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '8026FEA1-35C1-41CF-9D09-E8B1DB6CBDA8')
                  DECLARE @BlockId int = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '2E0FFD29-B4AF-4A5E-B528-667168762ABC')

                  UPDATE [AttributeValue]
	                SET [Value] = '{% include ''AdList'' %}'
	                WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId AND [ModifiedDateTime] IS NULL
            " );

            // mp: update pledge receipt
            Sql( @"
                UPDATE [AttributeValue]
                SET [Value] = '<h1>Thank You!</h1>
<p>
{{Person.NickName}}, thank you for your commitment of ${{FinancialPledge.TotalAmount}} to {{Account.Name}}.  To make your commitment even easier, you might consider making a scheduled giving profile.
</p>
<p>
    <a href=''~/page/186?PledgeId={{ FinancialPledge.Id  }}'' class=''btn btn-default'' >Setup a Giving Profile</a>
</p>
'
FROM AttributeValue v
JOIN Attribute a ON v.AttributeId = a.Id
WHERE v.Value = '<h1>Thank You!</h1>
<p>
{{Person.NickName}}, thank you for your commitment of ${{FinancialPledge.TotalAmount}} to {{Account.Name}}.  To make your commitment even easier, you might consider making a scheduled giving profile.
</p>
<p>
    <a href=''/page/186?PledgeId={{ FinancialPledge.Id  }}'' class=''btn btn-default'' >Setup a Giving Profile</a>
</p>
'
    AND a.[Guid] = 'CA69B385-696E-4BDB-8091-4B3956C7E5EA'
            " );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
