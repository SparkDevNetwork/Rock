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
    public partial class CodeGenerated_20260819 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attribute for BlockType
            //   BlockType: Prayer Request List
            //   Category: Prayer
            //   Attribute: Expires After (days)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4D6B686A-79DF-4EFC-A8BA-9841C248BF74", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Expires After (days)", "ExpireDays", "Expires After (days)", @"Number of days until the request will expire.", 1, @"14", "832713F3-1E12-4359-B5FC-B7F42B2665B3" );

            // Attribute for BlockType
            //   BlockType: Prayer Request List
            //   Category: Prayer
            //   Attribute: Show Prayer Count
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4D6B686A-79DF-4EFC-A8BA-9841C248BF74", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Prayer Count", "ShowPrayerCount", "Show Prayer Count", @"If enabled, the block will show the current prayer count for each request in the list.", 2, @"False", "967812CB-BEB4-40ED-B302-5EEFFBA0661E" );

            // Attribute for BlockType
            //   BlockType: Prayer Request List
            //   Category: Prayer
            //   Attribute: Show 'Approved' column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4D6B686A-79DF-4EFC-A8BA-9841C248BF74", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show 'Approved' column", "ShowApprovedColumn", "Show 'Approved' column", @"If enabled, the Approved column will be shown with a Yes/No toggle button.", 3, @"True", "C49F91F8-BD19-458C-B9A4-EB7A1AB9FD0B" );

            // Attribute for BlockType
            //   BlockType: Prayer Request List
            //   Category: Prayer
            //   Attribute: Show Grid Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4D6B686A-79DF-4EFC-A8BA-9841C248BF74", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Grid Filter", "ShowGridFilter", "Show Grid Filter", @"If enabled, the grid filter will be visible.", 4, @"True", "6C57A19E-BB2F-4F99-8019-597A743A525A" );

            // Attribute for BlockType
            //   BlockType: Prayer Request List
            //   Category: Prayer
            //   Attribute: Show Public Only
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4D6B686A-79DF-4EFC-A8BA-9841C248BF74", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Public Only", "ShowPublicOnly", "Show Public Only", @"If enabled, it will limit the list only to the prayer requests that are public.", 5, @"False", "D6DF9A94-A118-4664-BB2C-91F597C4B7C6" );

            // Attribute for BlockType
            //   BlockType: Chat Bot
            //   Category: AI
            //   Attribute: Default Agent
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "91A66C59-830E-49B5-A196-DCF93D0DDE92", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Default Agent", "DefaultAgent", "Default Agent", @"The default AI agent to use for this chat bot. If not specified then the first available chat agent will be used.", 0, @"", "44C0E822-4651-4E52-B3AD-61896D98336C" );

            // Attribute for BlockType
            //   BlockType: Content Channel Item Metrics
            //   Category: CMS
            //   Attribute: UTM Metrics to Show
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "447960A5-276E-4D5A-9AF0-133F90AA43C0", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "UTM Metrics to Show", "UtmMetricsToShow", "UTM Metrics to Show", @"The UTM dimensions (Source, Medium, Campaign, Term, Content) to include in the metric. If none are selected, all dimensions with captured data will be shown.", 0, @"", "01F764BC-D876-4AC9-AF96-6CE23F1A9EC8" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Prayer Request List
            //   Category: Prayer
            //   Attribute: Show Public Only
            RockMigrationHelper.DeleteAttribute( "D6DF9A94-A118-4664-BB2C-91F597C4B7C6" );

            // Attribute for BlockType
            //   BlockType: Prayer Request List
            //   Category: Prayer
            //   Attribute: Show Grid Filter
            RockMigrationHelper.DeleteAttribute( "6C57A19E-BB2F-4F99-8019-597A743A525A" );

            // Attribute for BlockType
            //   BlockType: Prayer Request List
            //   Category: Prayer
            //   Attribute: Show 'Approved' column
            RockMigrationHelper.DeleteAttribute( "C49F91F8-BD19-458C-B9A4-EB7A1AB9FD0B" );

            // Attribute for BlockType
            //   BlockType: Prayer Request List
            //   Category: Prayer
            //   Attribute: Show Prayer Count
            RockMigrationHelper.DeleteAttribute( "967812CB-BEB4-40ED-B302-5EEFFBA0661E" );

            // Attribute for BlockType
            //   BlockType: Prayer Request List
            //   Category: Prayer
            //   Attribute: Expires After (days)
            RockMigrationHelper.DeleteAttribute( "832713F3-1E12-4359-B5FC-B7F42B2665B3" );

            // Attribute for BlockType
            //   BlockType: Content Channel Item Metrics
            //   Category: CMS
            //   Attribute: UTM Metrics to Show
            RockMigrationHelper.DeleteAttribute( "01F764BC-D876-4AC9-AF96-6CE23F1A9EC8" );

            // Attribute for BlockType
            //   BlockType: Chat Bot
            //   Category: AI
            //   Attribute: Default Agent
            RockMigrationHelper.DeleteAttribute( "44C0E822-4651-4E52-B3AD-61896D98336C" );
        }
    }
}
