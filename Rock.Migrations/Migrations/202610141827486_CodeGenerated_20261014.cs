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
    public partial class CodeGenerated_20261014 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attribute for BlockType
            //   BlockType: Badges
            //   Category: CRM > Person Detail
            //   Attribute: Bottom Left Badges
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FC8AF928-C4AF-40C7-A667-4B24390F03A1", "602F273B-7EC2-42E6-9AA7-A36A268192A3", "Bottom Left Badges", "BottomLeftBadges", "Bottom Left Badges", @"The badges that displayed in the bottom left section of the badge bar.", 3, @"", "A606899D-753F-408E-A3C7-35B92CC503F7" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Minimum Short Link Token Length
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F7D464E2-5F7C-47BA-84DB-7CC7B0B623C0", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Minimum Short Link Token Length", "MinimumShortLinkTokenLength", "Minimum Short Link Token Length", @"The minimum number of characters for short link tokens.", 15, @"7", "5551A5B7-79F4-40A2-BAC8-18192E47A6F1" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Allow Unrestricted Uploads
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F7D464E2-5F7C-47BA-84DB-7CC7B0B623C0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Unrestricted Uploads", "AllowUnrestrictedUploads", "Allow Unrestricted Uploads", @"If true, anyone with access to send messages can upload images or files. Otherwise, it'll use the permissions set for Communication Attachment binary file type.", 11, @"True", "0116711C-5540-4E12-B033-F92EC9BEE268" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Personal / Need-to-Know Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F7D464E2-5F7C-47BA-84DB-7CC7B0B623C0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Personal / Need-to-Know Title", "IsNotBulkOptionTitle", "Personal / Need-to-Know Title", @"The title text to display for the Personal / Need-to-Know option.", 0, @"Personal / Need-to-Know", "61DD5EC5-06CC-49C5-9DE9-72FFF31C945D" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Personal / Need-to-Know Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F7D464E2-5F7C-47BA-84DB-7CC7B0B623C0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Personal / Need-to-Know Description", "IsNotBulkOptionDescription", "Personal / Need-to-Know Description", @"The description text to display for the Personal / Need-to-Know option.", 1, @"Direct messages an individual expects or considers important or timely.", "320BE637-FC81-47FD-B9A9-AC8483047BE0" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Bulk / Marketing Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F7D464E2-5F7C-47BA-84DB-7CC7B0B623C0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Bulk / Marketing Title", "IsBulkOptionTitle", "Bulk / Marketing Title", @"The title text to display for the Bulk / Marketing option.", 2, @"Bulk / Marketing", "2B2157BD-376C-4EA2-A4C9-A9789CE1EB51" );

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Bulk / Marketing Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F7D464E2-5F7C-47BA-84DB-7CC7B0B623C0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Bulk / Marketing Description", "IsBulkOptionDescription", "Bulk / Marketing Description", @"The description text to display for the Bulk / Marketing option.", 3, @"Marketing messages sent to large groups; regulated by law, and misuse can cause fines and reputational harm.", "B718A7B1-3ACA-4FA9-9B46-B43463C951D5" );

            // Attribute for BlockType
            //   BlockType: Connections Hub
            //   Category: Engagement
            //   Attribute: Connection Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8674FB3A-9E0E-421C-821C-2DA862A20ED2", "50DA6F25-E81E-46E8-A773-4B479B4FB9E6", "Connection Type", "ConnectionType", "Connection Type", @"Optional connection type to limit the block to. When set, the block always displays this type and ignores the ConnectionType page parameter.", 8, @"", "B19F0B96-00F9-48C7-BB05-2FE93B1C888D" );

            // Attribute for BlockType
            //   BlockType: Connections Hub
            //   Category: Engagement
            //   Attribute: Limit to Assigned Connections
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8674FB3A-9E0E-421C-821C-2DA862A20ED2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Assigned Connections", "OnlyShowAssigned", "Limit to Assigned Connections", @"When enabled, only requests assigned to the current person will be shown.", 9, @"False", "B968FF93-5310-4428-8078-A0E3DD0D6AF7" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Distance Calculation
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Distance Calculation", "DistanceCalculation", "Distance Calculation", @"How far each group is from the visitor, how the visitor shares where they are, and how the distance is measured. Each mode adds to the one before it: My Current Location shares the visitor's location through the browser and measures a direct line, Address or Zip Code adds an address box (which requires geocoding), and Driving Distance adds drive time and driving miles (which calls a routing provider). None offers no location search.", 70, @"StraightLineCurrentLocation", "B5DBCF4B-00A2-4839-B404-E5086064DE0D" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Distance Calculation
            RockMigrationHelper.DeleteAttribute("B5DBCF4B-00A2-4839-B404-E5086064DE0D");

            // Attribute for BlockType
            //   BlockType: Connections Hub
            //   Category: Engagement
            //   Attribute: Limit to Assigned Connections
            RockMigrationHelper.DeleteAttribute("B968FF93-5310-4428-8078-A0E3DD0D6AF7");

            // Attribute for BlockType
            //   BlockType: Connections Hub
            //   Category: Engagement
            //   Attribute: Connection Type
            RockMigrationHelper.DeleteAttribute("B19F0B96-00F9-48C7-BB05-2FE93B1C888D");

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Bulk / Marketing Description
            RockMigrationHelper.DeleteAttribute("B718A7B1-3ACA-4FA9-9B46-B43463C951D5");

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Bulk / Marketing Title
            RockMigrationHelper.DeleteAttribute("2B2157BD-376C-4EA2-A4C9-A9789CE1EB51");

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Personal / Need-to-Know Description
            RockMigrationHelper.DeleteAttribute("320BE637-FC81-47FD-B9A9-AC8483047BE0");

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Personal / Need-to-Know Title
            RockMigrationHelper.DeleteAttribute("61DD5EC5-06CC-49C5-9DE9-72FFF31C945D");

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Allow Unrestricted Uploads
            RockMigrationHelper.DeleteAttribute("0116711C-5540-4E12-B033-F92EC9BEE268");

            // Attribute for BlockType
            //   BlockType: Communication Entry Wizard
            //   Category: Communication
            //   Attribute: Minimum Short Link Token Length
            RockMigrationHelper.DeleteAttribute("5551A5B7-79F4-40A2-BAC8-18192E47A6F1");

            // Attribute for BlockType
            //   BlockType: Badges
            //   Category: CRM > Person Detail
            //   Attribute: Bottom Left Badges
            RockMigrationHelper.DeleteAttribute("A606899D-753F-408E-A3C7-35B92CC503F7");
        }
    }
}
