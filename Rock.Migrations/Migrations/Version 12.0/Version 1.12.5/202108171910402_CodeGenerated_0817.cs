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
    public partial class CodeGenerated_0817 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            
            // Attribute for BlockType: Calendar View:Show All Events in Detail
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "14B447B3-6117-4142-92E7-E3F289106140", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show All Events in Detail", "ShowAllEventsInDetail", "Show All Events in Detail", @"Determines if all events for the month should be listed in the detail section or only the selected days events.", 5, @"False", "DD3BCE0E-BE48-4F68-B1E2-176426124FDB" );

            // Attribute for BlockType: Calendar View:Show Per Audience Event Indicators
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "14B447B3-6117-4142-92E7-E3F289106140", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Per Audience Event Indicators", "ShowPerAudienceEventIndicators", "Show Per Audience Event Indicators", @"Determines if multiple colored dots will be used on the calendar to indicate which audience types exist on that day.", 6, @"False", "37BC8C51-F20E-4FFC-9C3B-42D515C6FA94" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            
            // Show Per Audience Event Indicators Attribute for BlockType: Calendar View
            RockMigrationHelper.DeleteAttribute("37BC8C51-F20E-4FFC-9C3B-42D515C6FA94");

            // Show All Events in Detail Attribute for BlockType: Calendar View
            RockMigrationHelper.DeleteAttribute("DD3BCE0E-BE48-4F68-B1E2-176426124FDB");
        }
    }
}
