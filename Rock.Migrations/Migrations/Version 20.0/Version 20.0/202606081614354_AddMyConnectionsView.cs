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
    public partial class AddMyConnectionsView : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Page 
            //  Internal Name: My Connections
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "2A0C135A-8421-4125-A484-83C8B4FB3D34", "C2467799-BB45-4251-8EE6-F0BF27201535", "My Connections", "", SystemGuid.Page.MY_CONNECTIONS, "" );

            // Add Page Route
            //   Page:My Connections
            //   Route:people/connections/my-connections
            RockMigrationHelper.AddOrUpdatePageRoute( Rock.SystemGuid.Page.MY_CONNECTIONS, "people/connections/my-connections", "C524F4D5-A1C0-4AC9-8E6E-D054A0AE12A5" );

            // Add Block 
            //  Block Name: Connections Hub
            //  Page Name: My Connections
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.MY_CONNECTIONS.AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "8674FB3A-9E0E-421C-821C-2DA862A20ED2".AsGuid(), "Connections Hub", "Main", @"", @"", 0, "54628553-464A-478E-B86B-73B2CFDB29B2" );

            // Attribute for BlockType
            //   BlockType: Connection Type Navigation
            //   Category: Connection
            //   Attribute: My Connections Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "23438CBC-105B-4ADB-8B9A-D5DDDCDD7643", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "My Connections Page", "MyConnectionsPage", "My Connections Page", @"Select the page that the My Connections button should open to view a personal Connections workspace.", 4, SystemGuid.Page.MY_CONNECTIONS, "0DE72D0D-843A-457F-8816-A6DA13FD259D" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Navigation
            //   Category: Connection
            //   Attribute: My Connections Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "91080C44-AFBF-4A02-AD0D-BD7E01F9D1DE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "My Connections Page", "MyConnectionsPage", "My Connections Page", @"Select the page that the My Connections button should open to view a personal Connections workspace.", 2, SystemGuid.Page.MY_CONNECTIONS, "D198FC92-7814-4D6E-8062-47E10C9250DB" );

            // Attribute for BlockType
            //   BlockType: Connections Hub
            //   Category: Engagement
            //   Attribute: My Connections Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8674FB3A-9E0E-421C-821C-2DA862A20ED2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "My Connections Page", "MyConnectionsPage", "My Connections Page", @"Select the page that the My Connections button should open to view a personal Connections workspace.", 4, SystemGuid.Page.MY_CONNECTIONS, "74228E74-90DC-4CB4-9721-525EEDC9CB1F" );

            // Add Block Attribute Value
            //   Block: Connection Type Navigation
            //   BlockType: Connection Type Navigation
            //   Category: Connection
            //   Block Location: Page=Connections, Site=Rock RMS
            //   Attribute: My Connections Page
            RockMigrationHelper.AddBlockAttributeValue( false, "340FBA54-FC54-4EA1-8DD2-301536405034", "0DE72D0D-843A-457F-8816-A6DA13FD259D", $"{Rock.SystemGuid.Page.MY_CONNECTIONS},C524F4D5-A1C0-4AC9-8E6E-D054A0AE12A5" );

            // Add Block Attribute Value
            //   Block: Connection Opportunity Navigation
            //   BlockType: Connection Opportunity Navigation
            //   Category: Connection
            //   Block Location: Page=Connections Opportunities, Site=Rock RMS
            //   Attribute: My Connections Page
            RockMigrationHelper.AddBlockAttributeValue( false, "D5130BD5-92A1-4904-ACEB-5CC6D9E8CDA5", "D198FC92-7814-4D6E-8062-47E10C9250DB", $"{Rock.SystemGuid.Page.MY_CONNECTIONS},C524F4D5-A1C0-4AC9-8E6E-D054A0AE12A5" );

            // Add Block Attribute Value
            //   Block: Connections Hub
            //   BlockType: Connections Hub
            //   Category: Connection
            //   Block Location: Page=Connections Hub, Site=Rock RMS
            //   Attribute: My Connections Page
            RockMigrationHelper.AddBlockAttributeValue( false, "1422636F-548F-4F50-BF2A-D494FB936A5C", "74228E74-90DC-4CB4-9721-525EEDC9CB1F", $"{Rock.SystemGuid.Page.MY_CONNECTIONS},C524F4D5-A1C0-4AC9-8E6E-D054A0AE12A5" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Connections Hub
            //   Category: Engagement
            //   Attribute: My Connections Page
            RockMigrationHelper.DeleteAttribute( "74228E74-90DC-4CB4-9721-525EEDC9CB1F" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Navigation
            //   Category: Connection
            //   Attribute: My Connections Page
            RockMigrationHelper.DeleteAttribute( "D198FC92-7814-4D6E-8062-47E10C9250DB" );

            // Attribute for BlockType
            //   BlockType: Connection Type Navigation
            //   Category: Connection
            //   Attribute: My Connections Page
            RockMigrationHelper.DeleteAttribute( "0DE72D0D-843A-457F-8816-A6DA13FD259D" );

            // Remove Block
            //  Name: Connections Hub, from Page: My Connections, Site: Rock RMS
            //  from Page: My Connections, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "54628553-464A-478E-B86B-73B2CFDB29B2" );

            RockMigrationHelper.DeletePageRoute( "C524F4D5-A1C0-4AC9-8E6E-D054A0AE12A5" );

            // Delete Page
            //  Internal Name: My Connections
            //  Site: Rock RMS
            RockMigrationHelper.DeletePage( SystemGuid.Page.MY_CONNECTIONS );
        }
    }
}
