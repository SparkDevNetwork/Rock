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
    public partial class CodeGenerated_20240801 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Bus.BusStatus
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Bus.BusStatus", "Bus Status", "Rock.Blocks.Bus.BusStatus, Rock.Blocks, Version=1.16.6.7, Culture=neutral, PublicKeyToken=null", false, false, "9DFA8FD4-C3AA-440A-B1D6-1F8695C4AD5A" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.CheckIn.CloudPrintMonitor
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.CheckIn.CloudPrintMonitor", "Cloud Print Monitor", "Rock.Blocks.CheckIn.CloudPrintMonitor, Rock.Blocks, Version=1.16.6.7, Culture=neutral, PublicKeyToken=null", false, false, "3FA3B98C-89D7-4157-B3AF-F2C27F8A70AA" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.AuthClientDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.AuthClientDetail", "Auth Client Detail", "Rock.Blocks.Core.AuthClientDetail, Rock.Blocks, Version=1.16.6.7, Culture=neutral, PublicKeyToken=null", false, false, "D7B56608-70A9-42D2-9542-3301DDBEC48B" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Crm.PhotoVerify
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Crm.PhotoVerify", "Photo Verify", "Rock.Blocks.Crm.PhotoVerify, Rock.Blocks, Version=1.16.6.7, Culture=neutral, PublicKeyToken=null", false, false, "0582B38F-C622-4F5B-A4BB-FD04B07EEE1B" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Event.RegistrationInstancePaymentList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Event.RegistrationInstancePaymentList", "Registration Instance Payment List", "Rock.Blocks.Event.RegistrationInstancePaymentList, Rock.Blocks, Version=1.16.6.7, Culture=neutral, PublicKeyToken=null", false, false, "3842853C-75B2-4568-8397-2B9E4409FD44" );

            // Add/Update Obsidian Block Type
            //   Name:Bus Status
            //   Category:Bus
            //   EntityType:Rock.Blocks.Bus.BusStatus
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Bus Status", "Gives insight into the message bus.", "Rock.Blocks.Bus.BusStatus", "Bus", "C472300C-781F-4D73-B530-8C9F8A9927D4" );

            // Add/Update Obsidian Block Type
            //   Name:Cloud Print Monitor
            //   Category:Check-in
            //   EntityType:Rock.Blocks.CheckIn.CloudPrintMonitor
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Cloud Print Monitor", "Monitors the cloud printing connections in Rock.", "Rock.Blocks.CheckIn.CloudPrintMonitor", "Check-in", "8F436A19-482A-41A7-AAB3-E5EC34D15D19" );

            // Add/Update Obsidian Block Type
            //   Name:OpenID Connect Client Detail
            //   Category:Security > OIDC
            //   EntityType:Rock.Blocks.Core.AuthClientDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "OpenID Connect Client Detail", "Displays the details of the given OpenID Connect Client.", "Rock.Blocks.Core.AuthClientDetail", "Security > OIDC", "8246EF8B-27E9-449E-9CAB-1C267B31DBC2" );

            // Add/Update Obsidian Block Type
            //   Name:Verify Photo
            //   Category:CRM
            //   EntityType:Rock.Blocks.Crm.PhotoVerify
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Verify Photo", "Allows uploaded photos to be verified.", "Rock.Blocks.Crm.PhotoVerify", "CRM", "1228F248-6AA1-4871-AF9E-195CF0FDA724" );

            // Add/Update Obsidian Block Type
            //   Name:Registration Instance - Payment List
            //   Category:Event
            //   EntityType:Rock.Blocks.Event.RegistrationInstancePaymentList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Registration Instance - Payment List", "Displays the payments related to an event registration instance.", "Rock.Blocks.Event.RegistrationInstancePaymentList", "Event", "E804F6B4-E4C2-47E5-B1DE-2147222BF3A2" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Security
            //   Attribute: Require Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Campus", "RequireCampus", "Require Campus", @"Require that a campus be selected. The campus will not be displayed if there is only one available campus, in which case if this is set to true then the single campus is automatically used.", 33, @"False", "9BCFA8E8-1DA4-4D0B-A7C2-3B9CED719856" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Security
            //   Attribute: Show Birth Date
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5C34503-DDAD-4881-8463-0E1E20B1675D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Birth Date", "ShowBirthDate", "Show Birth Date", @"Determine if the birth date field should be shown.", 34, @"False", "94F72872-9492-4937-B586-35CB2E732C9C" );

            // Attribute for BlockType
            //   BlockType: Bus Status
            //   Category: Bus
            //   Attribute: Transport Select Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C472300C-781F-4D73-B530-8C9F8A9927D4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Transport Select Page", "TransportSelectPage", "Transport Select Page", @"The page where the transport for the bus can be selected", 1, @"10E34A5D-D967-457D-9DF1-A1D33DA9D100", "978065B6-8583-4256-9858-8ADF0C48449E" );

            // Attribute for BlockType
            //   BlockType: Verify Photo
            //   Category: CRM
            //   Attribute: Photo Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1228F248-6AA1-4871-AF9E-195CF0FDA724", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Photo Size", "PhotoSize", "Photo Size", @"The size of the preview photo. Default is 65.", 0, @"150", "5808CCF2-17D6-4D16-9458-9282DE492674" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Payment List
            //   Category: Event
            //   Attribute: Transaction Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E804F6B4-E4C2-47E5-B1DE-2147222BF3A2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Transaction Detail Page", "TransactionDetailPage", "Transaction Detail Page", @"The page for viewing details about a payment", 1, @"B67E38CB-2EF1-43EA-863A-37DAA1C7340F", "53849C91-F089-4211-B747-19D14E5F4023" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Payment List
            //   Category: Event
            //   Attribute: Registration Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E804F6B4-E4C2-47E5-B1DE-2147222BF3A2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "Registration Page", @"The page for editing registration and registrant information", 2, @"FC81099A-2F98-4EBA-AC5A-8300B2FE46C4", "548E8C13-C3A5-4917-BF55-D399A8C49C03" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Payment List
            //   Category: Event
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E804F6B4-E4C2-47E5-B1DE-2147222BF3A2", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "A995FB0F-4BAB-49BF-97C8-2D24CA30EA75" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Payment List
            //   Category: Event
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E804F6B4-E4C2-47E5-B1DE-2147222BF3A2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "833FF295-FC38-4BB5-8FF9-EB6C7FB3F836" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Registration Instance - Payment List
            //   Category: Event
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "833FF295-FC38-4BB5-8FF9-EB6C7FB3F836" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Payment List
            //   Category: Event
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "A995FB0F-4BAB-49BF-97C8-2D24CA30EA75" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Payment List
            //   Category: Event
            //   Attribute: Registration Page
            RockMigrationHelper.DeleteAttribute( "548E8C13-C3A5-4917-BF55-D399A8C49C03" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Payment List
            //   Category: Event
            //   Attribute: Transaction Detail Page
            RockMigrationHelper.DeleteAttribute( "53849C91-F089-4211-B747-19D14E5F4023" );

            // Attribute for BlockType
            //   BlockType: Verify Photo
            //   Category: CRM
            //   Attribute: Photo Size
            RockMigrationHelper.DeleteAttribute( "5808CCF2-17D6-4D16-9458-9282DE492674" );

            // Attribute for BlockType
            //   BlockType: Bus Status
            //   Category: Bus
            //   Attribute: Transport Select Page
            RockMigrationHelper.DeleteAttribute( "978065B6-8583-4256-9858-8ADF0C48449E" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Security
            //   Attribute: Show Birth Date
            RockMigrationHelper.DeleteAttribute( "94F72872-9492-4937-B586-35CB2E732C9C" );

            // Attribute for BlockType
            //   BlockType: Account Entry
            //   Category: Security
            //   Attribute: Require Campus
            RockMigrationHelper.DeleteAttribute( "9BCFA8E8-1DA4-4D0B-A7C2-3B9CED719856" );

            // Delete BlockType 
            //   Name: Registration Instance - Payment List
            //   Category: Event
            //   Path: -
            //   EntityType: Registration Instance Payment List
            RockMigrationHelper.DeleteBlockType( "E804F6B4-E4C2-47E5-B1DE-2147222BF3A2" );

            // Delete BlockType 
            //   Name: Verify Photo
            //   Category: CRM
            //   Path: -
            //   EntityType: Photo Verify
            RockMigrationHelper.DeleteBlockType( "1228F248-6AA1-4871-AF9E-195CF0FDA724" );

            // Delete BlockType 
            //   Name: OpenID Connect Client Detail
            //   Category: Security > OIDC
            //   Path: -
            //   EntityType: Auth Client Detail
            RockMigrationHelper.DeleteBlockType( "8246EF8B-27E9-449E-9CAB-1C267B31DBC2" );

            // Delete BlockType 
            //   Name: Cloud Print Monitor
            //   Category: Check-in
            //   Path: -
            //   EntityType: Cloud Print Monitor
            RockMigrationHelper.DeleteBlockType( "8F436A19-482A-41A7-AAB3-E5EC34D15D19" );

            // Delete BlockType 
            //   Name: Bus Status
            //   Category: Bus
            //   Path: -
            //   EntityType: Bus Status
            RockMigrationHelper.DeleteBlockType( "C472300C-781F-4D73-B530-8C9F8A9927D4" );
        }
    }
}
