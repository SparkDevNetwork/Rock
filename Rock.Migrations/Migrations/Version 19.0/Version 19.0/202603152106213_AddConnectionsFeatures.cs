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
    public partial class AddConnectionsFeatures : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateIndex("dbo.ConnectionActivityType", "PersonNoteTypeId");
            AddForeignKey("dbo.ConnectionActivityType", "PersonNoteTypeId", "dbo.NoteType", "Id");

            JPH_SeedEnabledViewsAndFeaturesForExistingConnectionType_Up();
            JPH_AddConnectionsPages_Up();
            JPH_AddConnectionNavigationViewBlocks_Up();
            KH_AddConnectionsListBlockUp();
            KH_AddConnectionRequestNoteType_Up();
            KH_UpdateConnectionProperties();
            JMH_AddConnectionOperationalSnapshotBlock_Up();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JMH_AddConnectionOperationalSnapshotBlock_Down();
            JPH_AddConnectionNavigationViewBlocks_Down();
            JPH_AddConnectionsPages_Down();
            JPH_SeedEnabledViewsAndFeaturesForExistingConnectionType_Down();

            DropForeignKey("dbo.ConnectionActivityType", "PersonNoteTypeId", "dbo.NoteType");
            DropIndex("dbo.ConnectionActivityType", new[] { "PersonNoteTypeId" });
        }

        /// <summary>
        /// JPH: Seed enabled views and features for all existing connection types - up.
        /// </summary>
        private void JPH_SeedEnabledViewsAndFeaturesForExistingConnectionType_Up()
        {
            Sql( @"
UPDATE [ConnectionType]
SET [EnabledFeatures] = 1 | 2 | 4
    , [EnabledViews] = 1 | 2 | 4 | 8 | 16;" );
        }

        /// <summary>
        /// JPH: Seed enabled views and features for all existing connection types - down.
        /// </summary>
        private void JPH_SeedEnabledViewsAndFeaturesForExistingConnectionType_Down()
        {
            Sql( @"
UPDATE [ConnectionType]
SET [EnabledFeatures] = 0
    , [EnabledViews] = 0;" );
        }

        /// <summary>
        /// JPH: Add the connections pages - up.
        /// </summary>
        private void JPH_AddConnectionsPages_Up()
        {
            // Add Page 
            //  Internal Name: Connections List
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, Rock.SystemGuid.Page.CONNECTION_OPPORTUNITY_SELECT, "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Connections List", "", Rock.SystemGuid.Page.CONNECTIONS_LIST, "" );

            // Add Page Route
            //   Page:Connections List
            //   Route:people/connections/list
            RockMigrationHelper.AddOrUpdatePageRoute( Rock.SystemGuid.Page.CONNECTIONS_LIST, "people/connections/list", "565DFC73-E223-4C52-9174-11BB65700B7B" );

            // ----------------------------------

            // Add Page 
            //  Internal Name: Operational Snapshot
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, Rock.SystemGuid.Page.CONNECTION_OPPORTUNITY_SELECT, "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Operational Snapshot", "", Rock.SystemGuid.Page.CONNECTIONS_OPERATIONAL_SNAPSHOT, "" );

            // Add Page Route
            //   Page:Operational Snapshot
            //   Route:people/connections/snapshot
            RockMigrationHelper.AddOrUpdatePageRoute( Rock.SystemGuid.Page.CONNECTIONS_OPERATIONAL_SNAPSHOT, "people/connections/snapshot", "75077C7C-79AD-4041-A460-B4BFF9AFC8CF" );

            // ----------------------------------

            // Add Page 
            //  Internal Name: Connections Opportunities
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, Rock.SystemGuid.Page.CONNECTION_OPPORTUNITY_SELECT, "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Connections Opportunities", "", Rock.SystemGuid.Page.CONNECTIONS_OPPORTUNITIES, "" );

            // Add Page Route
            //   Page:Connections Opportunities
            //   Route:people/connections/opportunities
            RockMigrationHelper.AddOrUpdatePageRoute( Rock.SystemGuid.Page.CONNECTIONS_OPPORTUNITIES, "people/connections/opportunities", "92F02BD8-F7B2-47C6-99BE-F9E009D081E2" );
        }

        /// <summary>
        /// JPH: Add the connections navigation view blocks - up.
        /// </summary>
        private void JPH_AddConnectionNavigationViewBlocks_Up()
        {
            /*
                1/27/2026 - JPH

                The ConnectionOpportunitySelect block is being chopped in favor of the new Obsidian
                ConnectionTypeNavigation block.

                The following block settings will remain:
                -----
                ConfigurationPage
                ConnectionTypes

                The following block settings are no longer needed and will be removed in this migration:
                -----
                OpportunityDetailPage
                StatusTemplate
                OpportunitySummaryTemplate

                The following block settings are new and will be added in this migration:
                -----
                OpportunitiesPage
                ConnectionsListPage
                ConnectionBoardPage
                OperationalSnapshotPage

                Reason: Explain block chop details.
            */

            // -----
            // Update the Legacy attributes that will remain:

            // [Updated] Attribute for BlockType: Connection Type Navigation:Configuration Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "23438CBC-105B-4ADB-8B9A-D5DDDCDD7643", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Configuration Page", "ConfigurationPage", "Configuration Page", @"Select the page that the configuration button should open to create and modify connection types.", 1, "", "C170AC54-47B3-4B25-A149-742627D254CE" );

            // [Updated] Attribute for BlockType: Connection Type Navigation:Connection Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "23438CBC-105B-4ADB-8B9A-D5DDDCDD7643", "E4E72958-4604-498F-956B-BA095976A60B", "Connection Types", "ConnectionTypes", "Connection Types", @"Optional list of connection types to limit the display to (All will be displayed by default).", 5, "", "9D1A73B7-79A9-43FB-A465-DE9A0BD61A09" );

            // -----
            // Remove the Legacy attributes that are no longer needed:

            // Opportunity Detail Page Attribute for BlockType: Connection Opportunity Select
            RockMigrationHelper.DeleteAttribute( "4CB57D26-4848-4F24-BB5B-2D3C44ED9434" );

            // Status Template Attribute for BlockType: Connection Opportunity Select
            RockMigrationHelper.DeleteAttribute( "DD58A091-CBB2-4A09-BC1A-19A5DA7FD6C1" );

            // Opportunity Summary Template Attribute for BlockType: Connection Opportunity Select
            RockMigrationHelper.DeleteAttribute( "D042660B-00C2-440D-A700-B8B41398BD48" );

            // -----
            // Add the new entity type and attributes needed for the Obsidian version of the block.

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Connection.ConnectionTypeNavigation
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Connection.ConnectionTypeNavigation", "Connection Type Navigation", "Rock.Blocks.Connection.ConnectionTypeNavigation, Rock.Blocks, Version=19.0.4.0, Culture=neutral, PublicKeyToken=null", false, false, "E8C57557-31B7-4846-8F63-36BDDBB88719" );

            // Attribute for BlockType
            //   BlockType: Connection Type Navigation
            //   Category: Connection
            //   Attribute: Opportunities Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "23438CBC-105B-4ADB-8B9A-D5DDDCDD7643", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Opportunities Page", "OpportunitiesPage", "Opportunities Page", @"Select the page that should open to view opportunities when a connection type is selected.", 1, Rock.SystemGuid.Page.CONNECTIONS_OPPORTUNITIES, "39E56CE0-0154-4A80-902C-0EFAAD5A4483" );

            // Attribute for BlockType
            //   BlockType: Connection Type Navigation
            //   Category: Connection
            //   Attribute: Connections List Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "23438CBC-105B-4ADB-8B9A-D5DDDCDD7643", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Connections List Page", "ConnectionsListPage", "Connections List Page", @"Select the page that the list button should open to view the connections list.", 2, Rock.SystemGuid.Page.CONNECTIONS_LIST, "A783108E-D015-49B1-AA86-B7F18F438BCA" );

            // Attribute for BlockType
            //   BlockType: Connection Type Navigation
            //   Category: Connection
            //   Attribute: Connection Board Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "23438CBC-105B-4ADB-8B9A-D5DDDCDD7643", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Connection Board Page", "ConnectionBoardPage", "Connection Board Page", @"Select the page that the board and grid buttons should open to view the connection board in board or grid view.", 3, Rock.SystemGuid.Page.CONNECTIONS_BOARD, "30415B17-54DD-4632-A8DD-96BB218E938C" );

            // Attribute for BlockType
            //   BlockType: Connection Type Navigation
            //   Category: Connection
            //   Attribute: Operational Snapshot Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "23438CBC-105B-4ADB-8B9A-D5DDDCDD7643", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Operational Snapshot Page", "OperationalSnapshotPage", "Operational Snapshot Page", @"Select the page that the snapshot button should open to view the operational snapshot.", 4, Rock.SystemGuid.Page.CONNECTIONS_OPERATIONAL_SNAPSHOT, "573840DA-75F8-4664-A08F-6F3F0813F749" );

            // -----
            // Add the attribute values for page settings so we can include the preferred route.

            // Add Block Attribute Value
            //   Block: Connection Type Navigation
            //   BlockType: Connection Type Navigation
            //   Category: Connection
            //   Block Location: Page=Connections, Site=Rock RMS
            //   Attribute: Configuration Page
            /*   Attribute Value: 9cc19684-7ad2-4d4e-a7c4-10dae56e7fa6,197d9507-795b-3f50-92f0-e6e0d41661a2 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( false, "340FBA54-FC54-4EA1-8DD2-301536405034", "C170AC54-47B3-4B25-A149-742627D254CE", $"{Rock.SystemGuid.Page.CONNECTION_TYPES},197d9507-795b-3f50-92f0-e6e0d41661a2" );

            // Add Block Attribute Value
            //   Block: Connection Type Navigation
            //   BlockType: Connection Type Navigation
            //   Category: Connection
            //   Block Location: Page=Connections, Site=Rock RMS
            //   Attribute: Opportunities Page
            /*   Attribute Value: f8b0e0ce-76a3-4449-b4eb-28dd9a42d71f,92f02bd8-f7b2-47c6-99be-f9e009d081e2 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( false, "340FBA54-FC54-4EA1-8DD2-301536405034", "39E56CE0-0154-4A80-902C-0EFAAD5A4483", $"{Rock.SystemGuid.Page.CONNECTIONS_OPPORTUNITIES},92f02bd8-f7b2-47c6-99be-f9e009d081e2" );

            // Add Block Attribute Value
            //   Block: Connection Type Navigation
            //   BlockType: Connection Type Navigation
            //   Category: Connection
            //   Block Location: Page=Connections, Site=Rock RMS
            //   Attribute: Connections List Page
            /*   Attribute Value: 8b5f2875-0d36-4625-8ee4-b738ae8e12f5,565dfc73-e223-4c52-9174-11bb65700b7b */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( false, "340FBA54-FC54-4EA1-8DD2-301536405034", "A783108E-D015-49B1-AA86-B7F18F438BCA", $"{Rock.SystemGuid.Page.CONNECTIONS_LIST},565dfc73-e223-4c52-9174-11bb65700b7b" );

            // Add Block Attribute Value
            //   Block: Connection Type Navigation
            //   BlockType: Connection Type Navigation
            //   Category: Connection
            //   Block Location: Page=Connections, Site=Rock RMS
            //   Attribute: Connection Board Page
            /*   Attribute Value: 4fbceb52-8892-4035-bdea-112a494be81f,18b7ff61-442c-00db-8b20-191dfba92b2c */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( false, "340FBA54-FC54-4EA1-8DD2-301536405034", "30415B17-54DD-4632-A8DD-96BB218E938C", $"{Rock.SystemGuid.Page.CONNECTIONS_BOARD},18b7ff61-442c-00db-8b20-191dfba92b2c" );

            // Add Block Attribute Value
            //   Block: Connection Type Navigation
            //   BlockType: Connection Type Navigation
            //   Category: Connection
            //   Block Location: Page=Connections, Site=Rock RMS
            //   Attribute: Operational Snapshot Page
            /*   Attribute Value: 3421fd03-018f-457d-a0b6-9326c5d5a5f4,75077c7c-79ad-4041-a460-b4bff9afc8cf */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( false, "340FBA54-FC54-4EA1-8DD2-301536405034", "573840DA-75F8-4664-A08F-6F3F0813F749", $"{Rock.SystemGuid.Page.CONNECTIONS_OPERATIONAL_SNAPSHOT},75077c7c-79ad-4041-a460-b4bff9afc8cf" );

            // ----------------------------------

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Connection.ConnectionOpportunityNavigation
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Connection.ConnectionOpportunityNavigation", "Connection Opportunity Navigation", "Rock.Blocks.Connection.ConnectionOpportunityNavigation, Rock.Blocks, Version=19.0.4.0, Culture=neutral, PublicKeyToken=null", false, false, "6A3E1450-486E-45CF-8979-E280DACAEFEA" );

            // Add/Update Obsidian Block Type
            //   Name:Connection Opportunity Navigation
            //   Category:Connection
            //   EntityType:Rock.Blocks.Connection.ConnectionOpportunityNavigation
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Connection Opportunity Navigation", "Displays metrics of a connection type's combined opportunities and provides easy navigation into each opportunity's connection requests.", "Rock.Blocks.Connection.ConnectionOpportunityNavigation", "Connection", "91080C44-AFBF-4A02-AD0D-BD7E01F9D1DE" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Navigation
            //   Category: Connection
            //   Attribute: Connections List Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "91080C44-AFBF-4A02-AD0D-BD7E01F9D1DE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Connections List Page", "ConnectionsListPage", "Connections List Page", @"Select the page that the ""View Requests"" and list buttons should open to view the connections list.", 0, Rock.SystemGuid.Page.CONNECTIONS_LIST, "D43E5BE5-3375-44E9-9FCC-93D5B7A5C7CC" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Navigation
            //   Category: Connection
            //   Attribute: Connection Board Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "91080C44-AFBF-4A02-AD0D-BD7E01F9D1DE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Connection Board Page", "ConnectionBoardPage", "Connection Board Page", @"Select the page that the board and grid buttons should open to view the connection board in board or grid view.", 1, Rock.SystemGuid.Page.CONNECTIONS_BOARD, "294BC369-5706-4179-AA62-9DFB68070667" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Navigation
            //   Category: Connection
            //   Attribute: Operational Snapshot Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "91080C44-AFBF-4A02-AD0D-BD7E01F9D1DE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Operational Snapshot Page", "OperationalSnapshotPage", "Operational Snapshot Page", @"Select the page that the snapshot button should open to view the operational snapshot.", 2, Rock.SystemGuid.Page.CONNECTIONS_OPERATIONAL_SNAPSHOT, "63EBC197-9519-4A39-9ABB-DBB1DC9A67B8" );

            // ----------------------------------

            // Add Block 
            //  Block Name: Connection Opportunity Navigation
            //  Page Name: Connections Opportunities
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, Rock.SystemGuid.Page.CONNECTIONS_OPPORTUNITIES.AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "91080C44-AFBF-4A02-AD0D-BD7E01F9D1DE".AsGuid(), "Connection Opportunity Navigation", "Main", @"", @"", 0, "D5130BD5-92A1-4904-ACEB-5CC6D9E8CDA5" );

            // -----
            // Add the attribute values for page settings so we can include the preferred route.

            // Add Block Attribute Value
            //   Block: Connection Opportunity Navigation
            //   BlockType: Connection Opportunity Navigation
            //   Category: Connection
            //   Block Location: Page=Connections Opportunities, Site=Rock RMS
            //   Attribute: Connections List Page
            /*   Attribute Value: 8b5f2875-0d36-4625-8ee4-b738ae8e12f5,565dfc73-e223-4c52-9174-11bb65700b7b */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( false, "D5130BD5-92A1-4904-ACEB-5CC6D9E8CDA5", "D43E5BE5-3375-44E9-9FCC-93D5B7A5C7CC", $"{Rock.SystemGuid.Page.CONNECTIONS_LIST},565dfc73-e223-4c52-9174-11bb65700b7b" );

            // Add Block Attribute Value
            //   Block: Connection Opportunity Navigation
            //   BlockType: Connection Opportunity Navigation
            //   Category: Connection
            //   Block Location: Page=Connections Opportunities, Site=Rock RMS
            //   Attribute: Connection Board Page
            /*   Attribute Value: 4fbceb52-8892-4035-bdea-112a494be81f,18b7ff61-442c-00db-8b20-191dfba92b2c */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( false, "D5130BD5-92A1-4904-ACEB-5CC6D9E8CDA5", "294BC369-5706-4179-AA62-9DFB68070667", $"{Rock.SystemGuid.Page.CONNECTIONS_BOARD},18b7ff61-442c-00db-8b20-191dfba92b2c" );

            // Add Block Attribute Value
            //   Block: Connection Opportunity Navigation
            //   BlockType: Connection Opportunity Navigation
            //   Category: Connection
            //   Block Location: Page=Connections Opportunities, Site=Rock RMS
            //   Attribute: Operational Snapshot Page
            /*   Attribute Value: 3421fd03-018f-457d-a0b6-9326c5d5a5f4,75077c7c-79ad-4041-a460-b4bff9afc8cf */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( false, "D5130BD5-92A1-4904-ACEB-5CC6D9E8CDA5", "63EBC197-9519-4A39-9ABB-DBB1DC9A67B8", $"{Rock.SystemGuid.Page.CONNECTIONS_OPERATIONAL_SNAPSHOT},75077c7c-79ad-4041-a460-b4bff9afc8cf" );

            // ----------------------------------
            // Update the preexisting Legacy ConnectionOpportunitySelect block type and any instances to reflect the new ConnectionTypeNavigation block type.
            // If they've changed the name of any instances from the previous default, leave their names as-is.

            Sql( @"
DECLARE @BlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '23438CBC-105B-4ADB-8B9A-D5DDDCDD7643');

IF @BlockTypeId IS NOT NULL
BEGIN
    UPDATE [BlockType]
    SET [Name] = 'Connection Type Navigation'
        , [Description] = 'Displays connection types that the user is authorized to view and provides easy navigation into each type''s connection opportunities and requests.'
    WHERE [Id] = @BlockTypeId;

    UPDATE [Block]
    SET [Name] = 'Connection Type Navigation'
    WHERE [BlockTypeId] = @BlockTypeId
        AND [Name] = 'Connection Opportunity Select';
END" );
        }

        /// <summary>
        /// JPH: Add the connections navigation view blocks - down.
        /// </summary>
        private void JPH_AddConnectionNavigationViewBlocks_Down()
        {
            // Remove Block
            //  Name: Connection Opportunity Navigation, from Page: Connections Opportunities, Site: Rock RMS
            //  from Page: Connections Opportunities, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "D5130BD5-92A1-4904-ACEB-5CC6D9E8CDA5" );

            // ----------------------------------

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Navigation
            //   Category: Connection
            //   Attribute: Operational Snapshot Page
            RockMigrationHelper.DeleteAttribute( "63EBC197-9519-4A39-9ABB-DBB1DC9A67B8" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Navigation
            //   Category: Connection
            //   Attribute: Connection Board Page
            RockMigrationHelper.DeleteAttribute( "294BC369-5706-4179-AA62-9DFB68070667" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Navigation
            //   Category: Connection
            //   Attribute: Connections List Page
            RockMigrationHelper.DeleteAttribute( "D43E5BE5-3375-44E9-9FCC-93D5B7A5C7CC" );

            // Delete BlockType 
            //   Name: Connection Opportunity Navigation
            //   Category: Connection
            //   Path: -
            //   EntityType: Connection Opportunity Navigation
            RockMigrationHelper.DeleteBlockType( "91080C44-AFBF-4A02-AD0D-BD7E01F9D1DE" );

            // Delete Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Connection.ConnectionOpportunityNavigation
            RockMigrationHelper.DeleteEntityType( "6A3E1450-486E-45CF-8979-E280DACAEFEA" );

            // ----------------------------------

            // Attribute for BlockType
            //   BlockType: Connection Type Navigation
            //   Category: Connection
            //   Attribute: Operational Snapshot Page
            RockMigrationHelper.DeleteAttribute( "573840DA-75F8-4664-A08F-6F3F0813F749" );

            // Attribute for BlockType
            //   BlockType: Connection Type Navigation
            //   Category: Connection
            //   Attribute: Connection Board Page
            RockMigrationHelper.DeleteAttribute( "30415B17-54DD-4632-A8DD-96BB218E938C" );

            // Attribute for BlockType
            //   BlockType: Connection Type Navigation
            //   Category: Connection
            //   Attribute: Connections List Page
            RockMigrationHelper.DeleteAttribute( "A783108E-D015-49B1-AA86-B7F18F438BCA" );

            // Attribute for BlockType
            //   BlockType: Connection Type Navigation
            //   Category: Connection
            //   Attribute: Opportunities Page
            RockMigrationHelper.DeleteAttribute( "39E56CE0-0154-4A80-902C-0EFAAD5A4483" );

            // -----
            // Re-add the Legacy block type name, description and attributes that were removed in the up migration:

            Sql( @"
DECLARE @BlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '23438CBC-105B-4ADB-8B9A-D5DDDCDD7643');

IF @BlockTypeId IS NOT NULL
BEGIN
    UPDATE [BlockType]
    SET [EntityTypeId] = NULL
        , [Name] = 'Connection Opportunity Select'
        , [Description] = 'Block to display the connection opportunities that the user is authorized to view.'
    WHERE [Id] = @BlockTypeId;

    UPDATE [Block]
    SET [Name] = 'Connection Opportunity Select'
    WHERE [BlockTypeId] = @BlockTypeId
        AND [Name] = 'Connection Type Navigation';
END" );

            // Delete Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Connection.ConnectionTypeNavigation
            RockMigrationHelper.DeleteEntityType( "E8C57557-31B7-4846-8F63-36BDDBB88719" );

            // Attribute for BlockType: Connection Opportunity Select:Configuration Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "23438CBC-105B-4ADB-8B9A-D5DDDCDD7643", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Configuration Page", "ConfigurationPage", "Configuration Page", @"Page used to modify and create connection opportunities.", 1, @"9CC19684-7AD2-4D4E-A7C4-10DAE56E7FA6", "C170AC54-47B3-4B25-A149-742627D254CE" );

            // Attribute for BlockType: Connection Opportunity Select:Opportunity Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "23438CBC-105B-4ADB-8B9A-D5DDDCDD7643", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Opportunity Detail Page", "OpportunityDetailPage", "Opportunity Detail Page", @"Page to go to when an opportunity is selected.", 2, @"4FBCEB52-8892-4035-BDEA-112A494BE81F", "4CB57D26-4848-4F24-BB5B-2D3C44ED9434" );

            // Attribute for BlockType: Connection Opportunity Select:Connection Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "23438CBC-105B-4ADB-8B9A-D5DDDCDD7643", "E4E72958-4604-498F-956B-BA095976A60B", "Connection Types", "ConnectionTypes", "Connection Types", @"Optional list of connection types to limit the display to (All will be displayed by default).", 3, @"", "9D1A73B7-79A9-43FB-A465-DE9A0BD61A09" );

            // Attribute for BlockType: Connection Opportunity Select:Status Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "23438CBC-105B-4ADB-8B9A-D5DDDCDD7643", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Status Template", "StatusTemplate", "Status Template", @"Lava Template that can be used to customize what is displayed in the status bar. Includes common merge fields plus ConnectionOpportunities, ConnectionTypes and the default IdleTooltip.", 4, @"
<div class='badge-legend expand-on-hover padding-r-md'>
    <span class='badge badge-info badge-circle js-legend-badge'>Assigned To You</span>
    <span class='badge badge-warning badge-circle js-legend-badge'>Unassigned Item</span>
    <span class='badge badge-critical badge-circle js-legend-badge'>Critical Status</span>
    <span class='badge badge-danger badge-circle js-legend-badge'>{{ IdleTooltip }}</span>
</div>", "DD58A091-CBB2-4A09-BC1A-19A5DA7FD6C1" );

            // Attribute for BlockType: Connection Opportunity Select:Opportunity Summary Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "23438CBC-105B-4ADB-8B9A-D5DDDCDD7643", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Opportunity Summary Template", "OpportunitySummaryTemplate", "Opportunity Summary Template", @"Lava Template that can be used to customize what is displayed in each Opportunity Summary. Includes common merge fields plus the OpportunitySummary and ConnectionOpportunity.", 5, @"
<i class='{{ OpportunitySummary.IconCssClass }}'></i>
<h3>{{ OpportunitySummary.Name }}</h3>
<div class='status-list'>
    <span class='badge badge-info'>{{ OpportunitySummary.AssignedToYou | Format:'#,###,###' }}</span>
    <span class='badge badge-warning'>{{ OpportunitySummary.UnassignedCount | Format:'#,###,###' }}</span>
    <span class='badge badge-critical'>{{ OpportunitySummary.CriticalCount | Format:'#,###,###' }}</span>
    <span class='badge badge-danger'>{{ OpportunitySummary.IdleCount | Format:'#,###,###' }}</span>
</div>
", "D042660B-00C2-440D-A700-B8B41398BD48" );
        }

        /// <summary>
        /// JPH: Add the connections pages - down.
        /// </summary>
        private void JPH_AddConnectionsPages_Down()
        {
            // Delete Page 
            //  Internal Name: Connections Opportunities
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( Rock.SystemGuid.Page.CONNECTIONS_OPPORTUNITIES );

            // ----------------------------------

            // Delete Page 
            //  Internal Name: Operational Snapshot
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( Rock.SystemGuid.Page.CONNECTIONS_OPERATIONAL_SNAPSHOT );

            // ----------------------------------

            // Delete Page 
            //  Internal Name: Connections List
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( Rock.SystemGuid.Page.CONNECTIONS_LIST );
        }

        /// <summary>
        /// Temporary migration to add Connections List Block to page.
        /// </summary>
        private void KH_AddConnectionsListBlockUp()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Engagement.ConnectionsHub
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.ConnectionsHub", "Connections Hub", "Rock.Blocks.Engagement.ConnectionsHub, Rock.Blocks, Version=19.0.5.0, Culture=neutral, PublicKeyToken=null", false, false, "CEE15B88-3B23-4378-9CB1-E59A97A94D1B" );

            // Add/Update Obsidian Block Type
            //   Name:Connections Hub
            //   Category:Engagement
            //   EntityType:Rock.Blocks.Engagement.ConnectionsHub
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Connections Hub", "Displays the Connections Hub.", "Rock.Blocks.Engagement.ConnectionsHub", "Engagement", "8674FB3A-9E0E-421C-821C-2DA862A20ED2" );

            // Attribute for BlockType
            //   BlockType: Connections Hub
            //   Category: Engagement
            //   Attribute: Person Profile Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8674FB3A-9E0E-421C-821C-2DA862A20ED2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "Person Profile Page", @"Page used for viewing a person's profile. If set a view profile button will show for each grid item.", 0, @"08DBD8A5-2C35-4146-B4A8-0F7652348B25", "47675A23-7999-4111-A5F9-7D650E0814F1" );

            // Attribute for BlockType
            //   BlockType: Connections Hub
            //   Category: Engagement
            //   Attribute: Group Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8674FB3A-9E0E-421C-821C-2DA862A20ED2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "Group Detail Page", @"Page used to display group details.", 1, @"4E237286-B715-4109-A578-C1445EC02707", "B80C5CE4-01C2-48E7-90CD-1612615E33CE" );

            // Attribute for BlockType
            //   BlockType: Connections Hub
            //   Category: Engagement
            //   Attribute: Workflow Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8674FB3A-9E0E-421C-821C-2DA862A20ED2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Detail Page", "WorkflowDetailPage", "Workflow Detail Page", @"Page used to display details about a workflow.", 2, @"BA547EED-5537-49CF-BD4E-C583D760788C", "E57C4C30-0956-46B6-9DE0-E1B8A493A56D" );

            // Attribute for BlockType
            //   BlockType: Connections Hub
            //   Category: Engagement
            //   Attribute: Workflow Entry Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8674FB3A-9E0E-421C-821C-2DA862A20ED2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Entry Page", "WorkflowEntryPage", "Workflow Entry Page", @"Page used to launch a new workflow of the selected type.", 3, @"0550D2AA-A705-4400-81FF-AB124FDF83D7", "E9642065-A794-413C-9B86-D3854C6FE8AA" );

            // Attribute for BlockType
            //   BlockType: Connections Hub
            //   Category: Engagement
            //   Attribute: Lava Heading Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8674FB3A-9E0E-421C-821C-2DA862A20ED2", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Heading Template", "LavaHeadingTemplate", "Lava Heading Template", @"The HTML Content to render above the person’s name. Includes merge fields ConnectionRequest and Person. <span class='tip tip-lava'></span>", 5, @"", "34F63F4B-67A6-4C63-B9AE-57D7C1D6A577" );

            // Attribute for BlockType
            //   BlockType: Connections Hub
            //   Category: Engagement
            //   Attribute: Lava Badge Bar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8674FB3A-9E0E-421C-821C-2DA862A20ED2", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Badge Bar", "LavaBadgeBar", "Lava Badge Bar", @"The HTML Content intended to be used as a kind of custom badge bar for the connection request. Includes merge fields ConnectionRequest and Person. <span class='tip tip-lava'></span>", 6, @"", "EEFAE946-C1B0-4FB0-A17A-BEA1BB4E4C60" );

            // Attribute for BlockType
            //   BlockType: Connections Hub
            //   Category: Engagement
            //   Attribute: Badges
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8674FB3A-9E0E-421C-821C-2DA862A20ED2", "602F273B-7EC2-42E6-9AA7-A36A268192A3", "Badges", "Badges", "Badges", @"The badges to display in this block.", 4, @"", "F3627C9A-A65B-4AB3-83D7-387FC781571C" );

            // Add Block 
            //  Block Name: Connections List
            //  Page Name: Connections List
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "8B5F2875-0D36-4625-8EE4-B738AE8E12F5".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "8674FB3A-9E0E-421C-821C-2DA862A20ED2".AsGuid(), "Connections List", "Main", @"", @"", 0, "1422636F-548F-4F50-BF2A-D494FB936A5C" );
        }

        /// <summary>
        /// KH: Adds the Connection Request note type.
        /// </summary>
        private void KH_AddConnectionRequestNoteType_Up()
        {
            RockMigrationHelper.AddOrUpdateNoteTypeByMatchingNameAndEntityType(
                "Connection Request Note",
                "Rock.Model.ConnectionRequest",
                true,
                Rock.SystemGuid.NoteType.CONNECTION_REQUEST_NOTE,
                true,
                "ti ti-clipboard-list",
                false );
        }

        private void KH_UpdateConnectionProperties()
        {
            // Update Due Offset Days for all Connection Types.
            Sql( @"UPDATE [ConnectionType]
SET [RequestDueDateOffsetInDays] = 7,
    [RequestDueSoonOffsetInDays] = 5" );

            // Update Due Offset Days for all Connection Opportunities.
            Sql( @"UPDATE [ConnectionOpportunity]
SET [RequestDueDateOffsetInDays] = 7,
    [RequestDueSoonOffsetInDays] = 5" );

            // Update Due Offset Days for all Connection Statuses.
            Sql( @"UPDATE [ConnectionStatus]
SET [RequestStatusDueDateOffsetInDays] = 7,
    [RequestStatusDueSoonOffsetInDays] = 5" );

            // Update Connection Status Highlight Color if NULL.
            Sql( @"UPDATE [ConnectionStatus]
SET [HighlightColor] = '#00a6f4'
WHERE [HighlightColor] IS NULL" );
        }

        private void JMH_AddConnectionOperationalSnapshotBlock_Up()
        {

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Engagement.ConnectionOperationalSnapshot
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.ConnectionOperationalSnapshot", "Connection Operational Snapshot", "Rock.Blocks.Engagement.ConnectionOperationalSnapshot, Rock.Blocks, Version=19.0.5.0, Culture=neutral, PublicKeyToken=null", false, false, "92236EAD-C18C-4484-9685-6792B51FB7F7" );

            // Add/Update Obsidian Block Type
            //   Name:Connection Operational Snapshot
            //   Category:Engagement
            //   EntityType:Rock.Blocks.Engagement.ConnectionOperationalSnapshot
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Connection Operational Snapshot", "Displays analytics and operational metrics for Connection Requests and Connectors.", "Rock.Blocks.Engagement.ConnectionOperationalSnapshot", "Engagement", "B5FAF2A4-8195-4972-AA09-F65615939EA8" );

            // Add Block 
            //  Block Name: Connection Operational Snapshot
            //  Page Name: Operational Snapshot
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.CONNECTIONS_OPERATIONAL_SNAPSHOT.AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "B5FAF2A4-8195-4972-AA09-F65615939EA8".AsGuid(), "Connection Operational Snapshot", "Main", @"", @"", 0, "BAD25336-28FF-4012-8078-C9C34C62FE7F" );

            // Attribute for BlockType
            //   BlockType: Connection Operational Snapshot
            //   Category: Engagement
            //   Attribute: Connections Hub Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B5FAF2A4-8195-4972-AA09-F65615939EA8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Connections Hub Page", "ConnectionsHubPage", "Connections Hub Page", @"The page to navigate to if a Connectors grid row is clicked.", 0, @"8B5F2875-0D36-4625-8EE4-B738AE8E12F5", "0EA453F6-5984-4760-A6D3-B5DF513B1EEC" );

            // Add Block Attribute Value
            //   Block: Connection Operational Snapshot
            //   BlockType: Connection Operational Snapshot
            //   Category: Engagement
            //   Block Location: Page=Operational Snapshot, Site=Rock RMS
            //   Attribute: Connections Hub Page
            /*   Attribute Value: 8b5f2875-0d36-4625-8ee4-b738ae8e12f5,565dfc73-e223-4c52-9174-11bb65700b7b */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "BAD25336-28FF-4012-8078-C9C34C62FE7F", "0EA453F6-5984-4760-A6D3-B5DF513B1EEC", $@"{SystemGuid.Page.CONNECTIONS_LIST},565dfc73-e223-4c52-9174-11bb65700b7b" );
        }

        private void JMH_AddConnectionOperationalSnapshotBlock_Down()
        {
            // Attribute for BlockType
            //   BlockType: Connection Operational Snapshot
            //   Category: Engagement
            //   Attribute: Connections Hub Page
            RockMigrationHelper.DeleteAttribute( "0EA453F6-5984-4760-A6D3-B5DF513B1EEC" );

            // Remove Block
            //  Name: Connection Operational Snapshot, from Page: Operational Snapshot, Site: Rock RMS
            //  from Page: Operational Snapshot, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "BAD25336-28FF-4012-8078-C9C34C62FE7F" );

            // Delete BlockType 
            //   Name: Connection Operational Snapshot
            //   Category: Engagement
            //   Path: -
            //   EntityType: Connection Operational Snapshot
            RockMigrationHelper.DeleteBlockType( "B5FAF2A4-8195-4972-AA09-F65615939EA8" );
        }
    }
}
