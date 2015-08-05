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
    public partial class ConnectorGroup : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RenameTable( name: "dbo.ConnectionOpportunityGroupCampus", newName: "ConnectionOpportunityConnectorGroup" );
            DropForeignKey( "dbo.ConnectionOpportunity", "ConnectorGroupId", "dbo.Group" );
            DropIndex( "dbo.ConnectionOpportunity", new[] { "ConnectorGroupId" } );
            DropIndex( "dbo.ConnectionOpportunityConnectorGroup", new[] { "CampusId" } );
            DropIndex( "dbo.ConnectionOpportunityConnectorGroup", new[] { "ConnectorGroupId" } );
            AlterColumn( "dbo.ConnectionOpportunityConnectorGroup", "CampusId", c => c.Int() );
            AlterColumn( "dbo.ConnectionOpportunityConnectorGroup", "ConnectorGroupId", c => c.Int( nullable: false ) );
            CreateIndex( "dbo.ConnectionOpportunityConnectorGroup", "CampusId" );
            CreateIndex( "dbo.ConnectionOpportunityConnectorGroup", "ConnectorGroupId" );

            Sql( @"
    INSERT INTO [ConnectionOpportunityConnectorGroup] ( [ConnectionOpportunityId], [ConnectorGroupId], [Guid] )
    SELECT [Id], [ConnectorGroupId], NEWID()
    FROM [ConnectionOpportunity] 
    WHERE [ConnectorGroupId] IS NOT NULL
" );

            DropColumn( "dbo.ConnectionOpportunity", "ConnectorGroupId" );

            // Attrib for BlockType: Connection Request Detail:Workflow Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "A7961C9C-2EF5-44DF-BEA5-C334B42A90E2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Detail Page", "WorkflowDetailPage", "", "Page used to display details about a workflow.", 1, @"", "34D77237-E7BA-4567-A7EA-48EA798836B8" );
            // Attrib for BlockType: Connection Request Detail:Workflow Entry Page
            RockMigrationHelper.AddBlockTypeAttribute( "A7961C9C-2EF5-44DF-BEA5-C334B42A90E2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Entry Page", "WorkflowEntryPage", "", "Page used to launch a new workflow of the selected type.", 2, @"", "A039F571-4851-4066-981D-B768427A178E" );
            // Attrib for BlockType: Connection Request Detail:Group Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "A7961C9C-2EF5-44DF-BEA5-C334B42A90E2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "", "Page used to display group details.", 3, @"", "FBA3EE5D-A329-4126-9F49-4A610B17064A" );

            // Attrib Value for Block:Connection Request Detail, Attribute:Workflow Detail Page Page: Connection Request Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "94187C5A-7F6A-4D45-B5C2-C3C8673E8817", "34D77237-E7BA-4567-A7EA-48EA798836B8", @"ba547eed-5537-49cf-bd4e-c583d760788c,54eaf83b-4e58-403e-b02d-022581f68244" );
            // Attrib Value for Block:Connection Request Detail, Attribute:Workflow Entry Page Page: Connection Request Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "94187C5A-7F6A-4D45-B5C2-C3C8673E8817", "A039F571-4851-4066-981D-B768427A178E", @"0550d2aa-a705-4400-81ff-ab124fdf83d7,7a6b4e25-9d12-4ccc-8e8a-3bb216b08a45" );
            // Attrib Value for Block:Connection Request Detail, Attribute:Group Detail Page Page: Connection Request Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "94187C5A-7F6A-4D45-B5C2-C3C8673E8817", "FBA3EE5D-A329-4126-9F49-4A610B17064A", @"4e237286-b715-4109-a578-c1445ec02707" );

            Sql( @"
    INSERT [dbo].[ConnectionActivityType] ( [Name], [IsActive], [Guid] ) 
	    VALUES ( N'Assigned', 0, 'DB4C6D25-211C-4995-B194-9FEF7551F26B' )
" );

            RockMigrationHelper.AddPage( "550A898C-EDEA-48B5-9C58-B20EC13AF13B", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Installed Plugins", "", "5B6DBC42-8B03-4D15-8D92-AAFA28FD8616", "fa fa-plug" ); // Site:Rock RMS

            // Add Block to Page: Installed Plugins, Site: Rock RMS
            RockMigrationHelper.AddBlock( "5B6DBC42-8B03-4D15-8D92-AAFA28FD8616", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Main", "", "", 0, "F70D218E-3871-4327-853E-1F054D3AF6A4" );

            // Attrib Value for Block:Page Menu, Attribute:Enable Debug Page: Installed Plugins, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F70D218E-3871-4327-853E-1F054D3AF6A4", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Is Secondary Block Page: Installed Plugins, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F70D218E-3871-4327-853E-1F054D3AF6A4", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Template Page: Installed Plugins, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F70D218E-3871-4327-853E-1F054D3AF6A4", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}" );

            // Attrib Value for Block:Page Menu, Attribute:Number of Levels Page: Installed Plugins, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F70D218E-3871-4327-853E-1F054D3AF6A4", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"3" );

            // Attrib Value for Block:Page Menu, Attribute:Include Current QueryString Page: Installed Plugins, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F70D218E-3871-4327-853E-1F054D3AF6A4", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:CSS File Page: Installed Plugins, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F70D218E-3871-4327-853E-1F054D3AF6A4", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" );

            // Attrib Value for Block:Page Menu, Attribute:Include Current Parameters Page: Installed Plugins, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F70D218E-3871-4327-853E-1F054D3AF6A4", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Page Menu, from Page: Installed Plugins, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "F70D218E-3871-4327-853E-1F054D3AF6A4" );
            RockMigrationHelper.DeletePage( "5B6DBC42-8B03-4D15-8D92-AAFA28FD8616" ); //  Page: Installed Plugins, Layout: Full Width, Site: Rock RMS

            AddColumn("dbo.ConnectionOpportunity", "ConnectorGroupId", c => c.Int());

            Sql( @"
    UPDATE O
    SET [ConnectorGroupId] = ( 
        SELECT TOP 1 [ConnectorGroupId] 
        FROM [ConnectionOpportunityConnectorGroup]
        WHERE [ConnectionOpportunityId] = O.[Id]
        AND [CampusId] IS NULL
    )
" );

            DropIndex("dbo.ConnectionOpportunityConnectorGroup", new[] { "ConnectorGroupId" });
            DropIndex("dbo.ConnectionOpportunityConnectorGroup", new[] { "CampusId" });
            AlterColumn("dbo.ConnectionOpportunityConnectorGroup", "ConnectorGroupId", c => c.Int());
            AlterColumn("dbo.ConnectionOpportunityConnectorGroup", "CampusId", c => c.Int(nullable: false));
            CreateIndex("dbo.ConnectionOpportunityConnectorGroup", "ConnectorGroupId");
            CreateIndex("dbo.ConnectionOpportunityConnectorGroup", "CampusId");
            CreateIndex("dbo.ConnectionOpportunity", "ConnectorGroupId");
            AddForeignKey("dbo.ConnectionOpportunity", "ConnectorGroupId", "dbo.Group", "Id");
            RenameTable(name: "dbo.ConnectionOpportunityConnectorGroup", newName: "ConnectionOpportunityGroupCampus");
        }
    }
}
