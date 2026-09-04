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
    public partial class AddCelebrationReportPage : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Page 
            //  Internal Name: Celebrations Report
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "2A0C135A-8421-4125-A484-83C8B4FB3D34", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Celebrations Report", "", "E59810B6-5225-4CF6-A239-F2757A4369B1", "" );

            // Add Page Route
            //   Page:Celebrations Report
            //   Route:people/connections/celebrations
            RockMigrationHelper.AddOrUpdatePageRoute( "E59810B6-5225-4CF6-A239-F2757A4369B1", "people/connections/celebrations", "64A935C3-DE67-4E1B-97A6-7136FCAD8C7F" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Connection.CelebrationsReport
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Connection.CelebrationsReport", "Celebrations Report", "Rock.Blocks.Connection.CelebrationsReport, Rock.Blocks, Version=20.0.2.0, Culture=neutral, PublicKeyToken=null", false, false, "B5C0F4D7-2A1E-4C8B-9F3D-6E0A7B2C5D9F" );

            // Add/Update Obsidian Block Type
            //   Name:Connection Celebrations Report
            //   Category:Connection
            //   EntityType:Rock.Blocks.Connection.CelebrationsReport
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Connection Celebrations Report", "Displays a list of connection celebrations.", "Rock.Blocks.Connection.CelebrationsReport", "Connection", "8D3E5A9C-7B2F-4E1D-A6C0-3F9B8D2E5A7C" );

            // Add Block 
            //  Block Name: Connection Celebrations Report
            //  Page Name: Celebrations Report
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "E59810B6-5225-4CF6-A239-F2757A4369B1".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "8D3E5A9C-7B2F-4E1D-A6C0-3F9B8D2E5A7C".AsGuid(), "Connection Celebrations Report", "Main", @"", @"", 0, "32AD2827-E829-4650-95C3-1085B7AFF54B" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Navigation
            //   Category: Connection
            //   Attribute: Celebrations Report Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "91080C44-AFBF-4A02-AD0D-BD7E01F9D1DE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Celebrations Report Page", "CelebrationsReportPage", "Celebrations Report Page", @"Select the page that the celebrations button should open to view the celebrations report.", 2, @"", "A9EBA0F4-2F5A-4947-8364-9D11F1D2E874" );


            // Attribute for BlockType
            //   BlockType: Connection Celebrations Report
            //   Category: Connection
            //   Attribute: Connection Request Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8D3E5A9C-7B2F-4E1D-A6C0-3F9B8D2E5A7C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Connection Request Detail Page", "ConnectionRequestDetailPage", "Connection Request Detail Page", @"The page that will show the connection request details.", 0, @"", "E609A568-9F7F-464E-9DCF-6C280165CF4C" );

            // Attribute for BlockType
            //   BlockType: Connection Celebrations Report
            //   Category: Connection
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8D3E5A9C-7B2F-4E1D-A6C0-3F9B8D2E5A7C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "41CCA488-DAC7-4D37-8A08-FB6385E6115B" );

            // Attribute for BlockType
            //   BlockType: Connection Celebrations Report
            //   Category: Connection
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8D3E5A9C-7B2F-4E1D-A6C0-3F9B8D2E5A7C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "BB9B4297-9C52-4303-86C5-4CDD51C3A2A1" );

            // Add Block Attribute Value
            //   Block: Connection Opportunity Navigation
            //   BlockType: Connection Opportunity Navigation
            //   Category: Connection
            //   Block Location: Page=Connections Opportunities, Site=Rock RMS
            //   Attribute: Celebrations Report Page
            /*   Attribute Value: e59810b6-5225-4cf6-a239-f2757a4369b1 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "D5130BD5-92A1-4904-ACEB-5CC6D9E8CDA5", "A9EBA0F4-2F5A-4947-8364-9D11F1D2E874", @"e59810b6-5225-4cf6-a239-f2757a4369b1" );

            // Attribute for BlockType
            //   BlockType: Connection Type Navigation
            //   Category: Connection
            //   Attribute: Celebrations Report Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "23438CBC-105B-4ADB-8B9A-D5DDDCDD7643", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Celebrations Report Page", "CelebrationsReportPage", "Celebrations Report Page", @"Select the page that the celebrations button should open to view the celebrations report.", 4, @"", "0DFB63AF-958D-4A58-8001-8C4D255A7EBE" );


            // Add Block Attribute Value
            //   Block: Connection Type Navigation
            //   BlockType: Connection Type Navigation
            //   Category: Connection
            //   Block Location: Page=Connections, Site=Rock RMS
            //   Attribute: Celebrations Report Page
            /*   Attribute Value: e59810b6-5225-4cf6-a239-f2757a4369b1,64a935c3-de67-4e1b-97a6-7136fcad8c7f */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "340FBA54-FC54-4EA1-8DD2-301536405034", "0DFB63AF-958D-4A58-8001-8C4D255A7EBE", @"e59810b6-5225-4cf6-a239-f2757a4369b1,64a935c3-de67-4e1b-97a6-7136fcad8c7f" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType
            //   BlockType: Connection Celebrations Report
            //   Category: Connection
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "BB9B4297-9C52-4303-86C5-4CDD51C3A2A1" );

            // Attribute for BlockType
            //   BlockType: Connection Celebrations Report
            //   Category: Connection
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "41CCA488-DAC7-4D37-8A08-FB6385E6115B" );

            // Attribute for BlockType
            //   BlockType: Connection Celebrations Report
            //   Category: Connection
            //   Attribute: Connection Request Detail Page
            RockMigrationHelper.DeleteAttribute( "E609A568-9F7F-464E-9DCF-6C280165CF4C" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Navigation
            //   Category: Connection
            //   Attribute: Celebrations Report Page
            RockMigrationHelper.DeleteAttribute( "A9EBA0F4-2F5A-4947-8364-9D11F1D2E874" );

            // Attribute for BlockType
            //   BlockType: Connection Type Navigation
            //   Category: Connection
            //   Attribute: Celebrations Report Page
            RockMigrationHelper.DeleteAttribute( "0DFB63AF-958D-4A58-8001-8C4D255A7EBE" );

            // Remove Block
            //  Name: Connection Celebrations Report, from Page: Celebrations Report, Site: Rock RMS
            //  from Page: Celebrations Report, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "32AD2827-E829-4650-95C3-1085B7AFF54B" );

            // Delete BlockType 
            //   Name: Connection Celebrations Report
            //   Category: Connection
            //   Path: -
            //   EntityType: Celebrations Report
            RockMigrationHelper.DeleteBlockType( "8D3E5A9C-7B2F-4E1D-A6C0-3F9B8D2E5A7C" );

            // Delete Page 
            //  Internal Name: Celebrations Report
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "E59810B6-5225-4CF6-A239-F2757A4369B1" );
        }
    }
}
