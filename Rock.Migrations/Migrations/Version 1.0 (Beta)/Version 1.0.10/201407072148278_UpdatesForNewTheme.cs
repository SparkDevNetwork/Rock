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
    public partial class UpdatesForNewTheme : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"

                /* update panels on homepage */
                UPDATE [Block]
                SET [PreHtml] = REPLACE([PreHtml], 'panel-default', 'panel-block')
                WHERE [Guid] IN ('6A648E77-ABA9-4AAF-A8BB-027A12261ED9', 'CB8F9152-08BB-4576-B7A1-B0DDD9880C44', '03FCBF5A-42E0-4F45-B670-BC8E324BD573')

                UPDATE [Block]
                SET [PreHtml] = REPLACE([PreHtml], 'panel-info', 'panel-block')
                WHERE [Guid] IN ('62B1DBE6-B3D9-4C0B-BD12-1DD8C4F2C6EB')


                /* update label type of connection status */
                UPDATE [AttributeValue]
                SET [Value] = REPLACE([Value], 'label-success', 'label-type')
                WHERE [Guid] IN ('E332FC25-799F-4A51-A1C2-BD9F68C98FD9')

                /* update map styles */
                UPDATE [AttributeValue]
                SET [Value] = 'http://maps.googleapis.com/maps/api/staticmap?style=feature:all|element:all|saturation:-100|gamma:1|&style=feature:all|element:labels.text.stroke|visibility:off|&style=feature:poi|element:all|visibility:off|&style=feature:road|element:geometry|visibility:simplified|&style=feature:water|element:all|visibility:on|color:0xc6dfec|&style=feature:administrative.neighborhood|element:labels.text.fill|visibility:off|&style=feature:road.local|element:labels.text|weight:0.5|color:0x333333|&style=feature:transit.station|element:labels.icon|visibility:off|&markers=color:0xee7624|{MarkerPoints}&visual_refresh=true&path=fillcolor:0x63959f|color:0x63959f|{PolygonPoints}'
                WHERE [Guid] IN ('C45676C4-3864-4970-B8E3-4E088C940AC7')

                UPDATE [AttributeValue]
                SET [Value] = '#ee7624'
                WHERE [Guid] IN ('140D6EE0-C2B0-4706-A464-BE63A9C775DF')

                /* page icon for the pages missing them */
                UPDATE [Page]
                SET [IconCssClass] = 'fa fa-copy'
                WHERE [Guid] = 'F0C4E25F-83DF-44FF-AB5A-EF6C3044FAD3'

                UPDATE [Page]
                SET [IconCssClass] = 'fa fa-file-text-o'
                WHERE [Guid] = '753D62FD-A06F-43A3-B9D2-0A728FF2809A'

                UPDATE [Page]
                SET [IconCssClass] = 'fa fa-file-text'
                WHERE [Guid] = '60C0C193-61CF-4B34-A0ED-67EF8FD44867'

                UPDATE [Page]
                SET [IconCssClass] = 'fa fa-shield'
                WHERE [Guid] = 'D376EFD7-5B0D-44BF-A44D-03C466D2D30D'

                UPDATE [Page]
                SET [IconCssClass] = 'fa fa-th'
                WHERE [Guid] = 'E6217A2B-B16F-4E84-BF67-795CA7F5F9AA'

                UPDATE [Page]
                SET [IconCssClass] = 'fa fa-files-o'
                WHERE [Guid] = '1C763885-291F-44B7-A5E3-539584E07085'

                UPDATE [Page]
                SET [IconCssClass] = 'fa fa-exchange'
                WHERE [Guid] = '7F5EF1AA-0E27-4AA1-A5E1-1CD6DDDCDDC5'

                UPDATE [Page]
                SET [IconCssClass] = 'fa fa-clock-o'
                WHERE [Guid] = 'E18AC09D-45CD-49CF-8874-157B32556B7D'

                UPDATE [Page]
                SET [IconCssClass] = 'fa fa-building-o'
                WHERE [Guid] = '2B630A3B-E081-4204-A3E4-17BB3A5F063D'

                UPDATE [Page]
                SET [IconCssClass] = 'fa fa-signal'
                WHERE [Guid] = '78D84825-EB1A-43C6-9AD5-5F0F84CC9A53'

                UPDATE [Page]
                SET [IconCssClass] = 'fa fa-signal'
                WHERE [Guid] = '64E16878-D5AE-40A5-94FE-C2E8BE62DF61'

                UPDATE [Page]
                SET [IconCssClass] = 'fa fa-check-square-o'
                WHERE [Guid] = '7A3CF259-1090-403C-83B7-2DB3A53DEE26'

                UPDATE [Page]
                SET [IconCssClass] = 'fa fa-exchange'
                WHERE [Guid] = '649A2B1E-7A15-4DA8-AF67-17874B6FE98F'

                UPDATE [Page]
                SET [IconCssClass] = 'fa fa-gears'
                WHERE [Guid] = 'F3FA9EBE-A540-4106-90E5-2DFB2D72BBF0'

                /* delete unneeded block titles from prayer pages */
                DELETE FROM [Block]
                WHERE [Guid] = '7869E018-832D-455D-A493-D8B5C3B32D5D'

                DELETE FROM [Block]
                WHERE [Guid] = '3E15815E-EFC0-4963-AF21-7E5AC173A885'

                /* add css class to the site page */
                UPDATE [Block]
                SET [CssClass] = 'margin-b-md'
                WHERE [Guid] = '34EC5861-84EF-4D1A-8C89-D207B3004FDC'

                UPDATE [Block]
                SET [CssClass] = 'margin-b-md'
                WHERE [Guid] = '6E8216F2-6A48-4EF8-ABA3-736C2468610D'

                /* update chart colors */
                UPDATE [AttributeValue]
	            SET [Value] = '{
	""SeriesColors"": [
	""#416d78"",
	""#649dac"",
	""#82d3e6"",
	""#eef3f5"",
	""#eaf7fc""
	],
	""GoalSeriesColor"": ""red"",
	""Grid"": {
	""ColorGradient"": null,
	""Color"": null,
	""BackgroundColorGradient"": null,
	""BackgroundColor"": ""transparent"",
	""BorderWidth"": {
		""top"": 0,
		""right"": 0,
		""bottom"": 1,
		""left"": 1
	},
	""BorderColor"": null
	},
	""XAxis"": {
	""Color"": ""rgba(81, 81, 81, 0.2)"",
	""Font"": {
		""Size"": 10,
		""Family"": null,
		""Color"": ""#515151""
	},
	""DateTimeFormat"": ""%b %e,<br />%Y""
	},
	""YAxis"": {
	""Color"": ""rgba(81, 81, 81, 0.2)"",
	""Font"": {
		""Size"": null,
		""Family"": null,
		""Color"": ""#515151""
	},
	""DateTimeFormat"": null
	},
	""FillOpacity"": 0.2,
	""FillColor"": null,
	""Legend"": {
	""BackgroundColor"": ""transparent"",
	""BackgroundOpacity"": null,
	""LabelBoxBorderColor"": null
	},
	""Title"": {
	""Font"": {
		""Size"": 16,
		""Family"": null,
		""Color"": null
	},
	""Align"": ""left""
	},
	""Subtitle"": {
	""Font"": {
		""Size"": 12,
		""Family"": null,
		""Color"": null
	},
	""Align"": ""left""
	}
}'
	WHERE [Guid] = '752AA9E7-FC5C-4116-9FD5-B8C06F9CE80F'
                

                /* Delete Paneled Layouts */
                UPDATE [Page]
                SET [LayoutId] = 12
                WHERE [LayoutId] = 13

                UPDATE [Page]
                SET [LayoutId] = 14
                WHERE [LayoutId] = 15

                UPDATE [Page]
                SET [LayoutId] = 16
                WHERE [LayoutId] = 17
    

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
