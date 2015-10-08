// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class CheckinWelcomeManager : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage( "CDF2C599-D341-42FD-B7DC-CD402EA96050", "66FA0143-F04C-4447-A67A-2A10A6BB1A2B", "Scheduled Locations", "", "4E210ABB-65B3-4816-BD77-D2C876FC0FD5", "" ); // Site:Rock Check-in
            RockMigrationHelper.AddPageRoute( "4E210ABB-65B3-4816-BD77-D2C876FC0FD5", "checkin/scheduledlocations" );// for Page:Scheduled Locations
            RockMigrationHelper.UpdateBlockType( "Check-in Scheduled Locations", "Helps to enable/disable schedules associated with the configured group types at a kiosk", "~/Blocks/CheckIn/CheckinScheduledLocations.ascx", "Check-in", "C8C4E323-C227-4EAA-938F-4B962BC2DD7E" );

            // Add Block to Page: Scheduled Locations, Site: Rock Check-in
            RockMigrationHelper.AddBlock( "4E210ABB-65B3-4816-BD77-D2C876FC0FD5", "", "C8C4E323-C227-4EAA-938F-4B962BC2DD7E", "Check-in Scheduled Locations", "Main", "", "", 0, "23372BE4-4697-4E86-8B5F-C9A535435C2C" );

            // Attrib for BlockType: Welcome:Enable Override
            RockMigrationHelper.AddBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Override", "EnableOverride", "", "Allows the override link to be used on the configuration page.", 0, @"True", "B4CF0964-F2AD-482F-A50C-570159FD1FFC" );

            // Attrib for BlockType: Welcome:Enable Manager
            RockMigrationHelper.AddBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Manager", "EnableManager", "", "Allows the manager link to be placed on the page.", 0, @"True", "4C3B1C57-AF71-48A7-A8FE-36702BD67E78" );

            // Attrib for BlockType: Welcome:Scheduled Locations Page
            RockMigrationHelper.AddBlockTypeAttribute( "E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Scheduled Locations Page", "ScheduledLocationsPage", "", "", 0, @"", "C79544CC-B79A-4D05-BFC2-DF78CCC3D4F4" );

            // Attrib Value for Block:Welcome, Attribute:Enable Override Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "296E50EB-AA26-4E9A-9981-FE1F37B1DFDE", "B4CF0964-F2AD-482F-A50C-570159FD1FFC", @"True" );

            // Attrib Value for Block:Welcome, Attribute:Enable Manager Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "296E50EB-AA26-4E9A-9981-FE1F37B1DFDE", "4C3B1C57-AF71-48A7-A8FE-36702BD67E78", @"True" );

            // Attrib Value for Block:Welcome, Attribute:Scheduled Locations Page Page: Welcome, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "296E50EB-AA26-4E9A-9981-FE1F37B1DFDE", "C79544CC-B79A-4D05-BFC2-DF78CCC3D4F4", @"4e210abb-65b3-4816-bd77-d2c876fc0fd5,ff3b1ad7-4f4e-4e29-b960-8e7db50c3837" );

            // Attrib for BlockType: Check-in Scheduled Locations:Home Page
            RockMigrationHelper.AddBlockTypeAttribute( "C8C4E323-C227-4EAA-938F-4B962BC2DD7E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 0, @"", "93DDA73B-5672-4099-9CB0-61DFF17A48DC" );

            // Attrib Value for Block:Check-in Scheduled Locations, Attribute:Home Page Page: Scheduled Locations, Site: Rock Check-in
            RockMigrationHelper.AddBlockAttributeValue( "23372BE4-4697-4E86-8B5F-C9A535435C2C", "93DDA73B-5672-4099-9CB0-61DFF17A48DC", @"432b615a-75ff-4b14-9c99-3e769f866950" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Check-in Scheduled Locations, from Page: Scheduled Locations, Site: Rock Check-in
            RockMigrationHelper.DeleteBlock( "23372BE4-4697-4E86-8B5F-C9A535435C2C" );
            RockMigrationHelper.DeleteBlockType( "C8C4E323-C227-4EAA-938F-4B962BC2DD7E" ); // Check-in Scheduled Locations
            RockMigrationHelper.DeletePage( "4E210ABB-65B3-4816-BD77-D2C876FC0FD5" ); //  Page: Scheduled Locations, Layout: Checkin, Site: Rock Check-in
        }
    }
}
