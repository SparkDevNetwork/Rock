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
    public partial class ConnectionChangesAndSampleData : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey( "dbo.ConnectionOpportunityCampus", "ConnectorGroupId", "dbo.Group" );
            DropIndex( "dbo.ConnectionOpportunityCampus", new[] { "ConnectorGroupId" } );
            CreateTable(
                "dbo.ConnectionOpportunityGroupCampus",
                c => new
                    {
                        Id = c.Int( nullable: false, identity: true ),
                        ConnectionOpportunityId = c.Int( nullable: false ),
                        CampusId = c.Int( nullable: false ),
                        ConnectorGroupId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid( nullable: false ),
                        ForeignId = c.String( maxLength: 100 ),
                    } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.Campus", t => t.CampusId, cascadeDelete: true )
                .ForeignKey( "dbo.ConnectionOpportunity", t => t.ConnectionOpportunityId, cascadeDelete: true )
                .ForeignKey( "dbo.Group", t => t.ConnectorGroupId, cascadeDelete: true )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.ConnectionOpportunityId )
                .Index( t => t.CampusId )
                .Index( t => t.ConnectorGroupId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true )
                .Index( t => t.ForeignId );

            AlterColumn( "dbo.ConnectionStatus", "Description", c => c.String() );
            DropColumn( "dbo.ConnectionOpportunity", "GroupMemberStatusId" );
            DropColumn( "dbo.ConnectionOpportunityCampus", "ConnectorGroupId" );

            RockMigrationHelper.UpdateEntityType( "Rock.Model.ConnectionActivityType", Rock.SystemGuid.EntityType.CONNECTION_ACTIVITY_TYPE, true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.ConnectionOpportunity", Rock.SystemGuid.EntityType.CONNECTION_OPPORTUNITY, true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.ConnectionOpportunityCampus", Rock.SystemGuid.EntityType.CONNECTION_OPPORTUNITY_CAMPUS, true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.ConnectionOpportunityGroup", Rock.SystemGuid.EntityType.CONNECTION_OPPORTUNITY_GROUP, true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.ConnectionOpportunityGroupCampus", Rock.SystemGuid.EntityType.CONNECTION_OPPORTUNITY_CONNECTOR_GROUP, true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.ConnectionRequest", Rock.SystemGuid.EntityType.CONNECTION_REQUEST, true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.ConnectionRequestActivity", Rock.SystemGuid.EntityType.CONNECTION_REQUEST_ACTIVITY, true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.ConnectionRequestWorkflow", Rock.SystemGuid.EntityType.CONNECTION_REQUEST_WORKFLOW, true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.ConnectionStatus", Rock.SystemGuid.EntityType.CONNECTION_STATUS, true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.ConnectionType", Rock.SystemGuid.EntityType.CONNECTION_TYPE, true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.ConnectionWorkflow", Rock.SystemGuid.EntityType.CONNECTION_WORKFLOW, true, true );

            RockMigrationHelper.AddBlockAttributeValue( "94187C5A-7F6A-4D45-B5C2-C3C8673E8817", "5280472E-303C-446F-BD88-2490417220AD", @"dcb18a76-6dff-48a5-a66e-2caa10d2ca1a" ); // Workflow Configuration Page

            RockMigrationHelper.DeleteAttribute( "06DD6554-AD0E-40E5-9F36-925107FC5642" );

            RockMigrationHelper.DeleteAttribute( "BCCD15FA-DF2C-455E-A73F-BE6183FB0F74" );
            RockMigrationHelper.DeleteAttribute( "E967EC6C-7DFD-42B4-94B9-AFC093BE8093" );
            RockMigrationHelper.DeleteAttribute( "AD7359D0-CFF3-435F-98E2-F552A75DDD89" );
            RockMigrationHelper.DeleteAttribute( "B6DD8328-5D75-444C-AE8D-05011165668D" );
            RockMigrationHelper.DeleteAttribute( "0F56AAD0-A625-476F-BC72-0305AA125543" );

            RockMigrationHelper.AddBlockTypeAttribute( "B8CA0630-29E7-41B9-B4F1-EB6DE043EBDC", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Signup Page", "SignupPage", "", "The page used to sign up for an opportunity", 0, @"", "B838FF25-9FC2-4931-8A38-D7DB491FFAFC" );

            RockMigrationHelper.AddBlockAttributeValue( "7467A0E6-5348-47A5-A6A3-DFAD2F19236E", "B838FF25-9FC2-4931-8A38-D7DB491FFAFC", @"15436d89-9d88-43e9-85a9-98b79da0b823" ); // Signup Page

            // Page: Connection Signup Page
            RockMigrationHelper.AddPage( "EDA56877-D431-4067-9DB0-569E93BC7258", "325B7BFD-8B80-44FD-A951-4E4763DA6C0D", "Connection Signup Page", "", "15436D89-9D88-43E9-85A9-98B79DA0B823", "" ); // Site:External Website
            RockMigrationHelper.UpdateBlockType( "External Connection Opportunity Signup", "Displays the details of the given opportunity for the external website.", "~/Blocks/Connection/ExternalConnectionOpportunitySignup.ascx", "Connection", "C7FCE3B7-704B-43C0-AF96-5A70EB7F70D9" );
            RockMigrationHelper.AddBlock( "15436D89-9D88-43E9-85A9-98B79DA0B823", "", "C7FCE3B7-704B-43C0-AF96-5A70EB7F70D9", "External Connection Opportunity Signup", "Main", "", "", 0, "7225BF3E-15E0-44A6-B63B-93C18A539C9B" );
            RockMigrationHelper.AddBlock( "15436D89-9D88-43E9-85A9-98B79DA0B823", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Sidebar1", "", "", 0, "F53E563D-910A-4F20-AD54-90D6B5F1D9E7" );

            RockMigrationHelper.AddBlockTypeAttribute( "C7FCE3B7-704B-43C0-AF96-5A70EB7F70D9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Mobile Phone", "DisplayMobilePhone", "", "Whether to display mobile phone", 0, @"True", "ADAE7CEC-080E-42D8-81DD-585C0219D272" );

            RockMigrationHelper.AddBlockTypeAttribute( "C7FCE3B7-704B-43C0-AF96-5A70EB7F70D9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Home Phone", "DisplayHomePhone", "", "Whether to display home phone", 0, @"True", "9B4DE175-CD1A-40DF-923F-E14392E9C8CD" );

            RockMigrationHelper.AddBlockTypeAttribute( "C7FCE3B7-704B-43C0-AF96-5A70EB7F70D9", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "", "The connection status to use for new individuals (default: 'Web Prospect'.)", 0, @"368DD475-242C-49C4-A42C-7278BE690CC2", "13FCAD6F-F388-4B45-9DEB-79C0BC1E26DA" );

            RockMigrationHelper.AddBlockTypeAttribute( "C7FCE3B7-704B-43C0-AF96-5A70EB7F70D9", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "", "The record status to use for new individuals (default: 'Pending'.)", 0, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "67A530C0-195E-4F92-907E-76E69602F77C" );

            RockMigrationHelper.AddBlockTypeAttribute( "C7FCE3B7-704B-43C0-AF96-5A70EB7F70D9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Set Page Title", "SetPageTitle", "", "Determines if the block should set the page title with the opportunity name.", 0, @"True", "512AAB20-62AB-465C-8A09-45728A9B819A" );

            RockMigrationHelper.AddBlockTypeAttribute( "C7FCE3B7-704B-43C0-AF96-5A70EB7F70D9", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "Lava template to use to display the response message.", 2, @"{% include '~/Themes/Stark/Assets/Lava/ExternalOpportunityResponseMessage.lava' %}", "2DA45081-2E5B-47DF-B82B-1B8461FECBA7" );

            RockMigrationHelper.AddBlockTypeAttribute( "C7FCE3B7-704B-43C0-AF96-5A70EB7F70D9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Display a list of merge fields available for lava.", 3, @"False", "1304D60C-8EF7-437A-AEA6-844DC053FABE" );

            RockMigrationHelper.AddBlockAttributeValue( "7225BF3E-15E0-44A6-B63B-93C18A539C9B", "ADAE7CEC-080E-42D8-81DD-585C0219D272", @"True" ); // Display Mobile Phone

            RockMigrationHelper.AddBlockAttributeValue( "7225BF3E-15E0-44A6-B63B-93C18A539C9B", "2DA45081-2E5B-47DF-B82B-1B8461FECBA7", @"{% include '~/Themes/Stark/Assets/Lava/ExternalOpportunityResponseMessage.lava' %}" ); // Lava Template

            RockMigrationHelper.AddBlockAttributeValue( "7225BF3E-15E0-44A6-B63B-93C18A539C9B", "1304D60C-8EF7-437A-AEA6-844DC053FABE", @"False" ); // Enable Debug

            RockMigrationHelper.AddBlockAttributeValue( "7225BF3E-15E0-44A6-B63B-93C18A539C9B", "9B4DE175-CD1A-40DF-923F-E14392E9C8CD", @"True" ); // Display Home Phone

            RockMigrationHelper.AddBlockAttributeValue( "7225BF3E-15E0-44A6-B63B-93C18A539C9B", "13FCAD6F-F388-4B45-9DEB-79C0BC1E26DA", @"368dd475-242c-49c4-a42c-7278be690cc2" ); // Connection Status

            RockMigrationHelper.AddBlockAttributeValue( "7225BF3E-15E0-44A6-B63B-93C18A539C9B", "67A530C0-195E-4F92-907E-76E69602F77C", @"283999ec-7346-42e3-b807-bce9b2babb49" ); // Record Status

            RockMigrationHelper.AddBlockAttributeValue( "F53E563D-910A-4F20-AD54-90D6B5F1D9E7", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" ); // CSS File

            RockMigrationHelper.AddBlockAttributeValue( "F53E563D-910A-4F20-AD54-90D6B5F1D9E7", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" ); // Include Current Parameters

            RockMigrationHelper.AddBlockAttributeValue( "F53E563D-910A-4F20-AD54-90D6B5F1D9E7", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageSubNav.lava' %}" ); // Template

            RockMigrationHelper.AddBlockAttributeValue( "F53E563D-910A-4F20-AD54-90D6B5F1D9E7", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"7625a63e-6650-4886-b605-53c2234fa5e1" ); // Root Page

            RockMigrationHelper.AddBlockAttributeValue( "F53E563D-910A-4F20-AD54-90D6B5F1D9E7", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" ); // Number of Levels

            RockMigrationHelper.AddBlockAttributeValue( "F53E563D-910A-4F20-AD54-90D6B5F1D9E7", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" ); // Include Current QueryString

            RockMigrationHelper.AddBlockAttributeValue( "F53E563D-910A-4F20-AD54-90D6B5F1D9E7", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" ); // Enable Debug

            RockMigrationHelper.AddBlockAttributeValue( "F53E563D-910A-4F20-AD54-90D6B5F1D9E7", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" ); // Is Secondary Block

            Sql( MigrationSQL._201507181918494_ConnectionChangesAndSampleData );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "512AAB20-62AB-465C-8A09-45728A9B819A" );
            RockMigrationHelper.DeleteAttribute( "67A530C0-195E-4F92-907E-76E69602F77C" );
            RockMigrationHelper.DeleteAttribute( "13FCAD6F-F388-4B45-9DEB-79C0BC1E26DA" );
            RockMigrationHelper.DeleteAttribute( "9B4DE175-CD1A-40DF-923F-E14392E9C8CD" );
            RockMigrationHelper.DeleteAttribute( "1304D60C-8EF7-437A-AEA6-844DC053FABE" );
            RockMigrationHelper.DeleteAttribute( "2DA45081-2E5B-47DF-B82B-1B8461FECBA7" );
            RockMigrationHelper.DeleteAttribute( "ADAE7CEC-080E-42D8-81DD-585C0219D272" );
            RockMigrationHelper.DeleteBlock( "F53E563D-910A-4F20-AD54-90D6B5F1D9E7" );
            RockMigrationHelper.DeleteBlock( "7225BF3E-15E0-44A6-B63B-93C18A539C9B" );
            RockMigrationHelper.DeleteBlockType( "C7FCE3B7-704B-43C0-AF96-5A70EB7F70D9" );
            RockMigrationHelper.DeletePage( "15436D89-9D88-43E9-85A9-98B79DA0B823" ); //  Page: Connection Signup Page

            RockMigrationHelper.DeleteAttribute( "B838FF25-9FC2-4931-8A38-D7DB491FFAFC" );

            RockMigrationHelper.AddBlockTypeAttribute( "B8CA0630-29E7-41B9-B4F1-EB6DE043EBDC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Mobile Phone", "DisplayMobilePhone", "", "Whether to display mobile phone", 0, @"True", "0F56AAD0-A625-476F-BC72-0305AA125543" );

            RockMigrationHelper.AddBlockTypeAttribute( "B8CA0630-29E7-41B9-B4F1-EB6DE043EBDC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Home Phone", "DisplayHomePhone", "", "Whether to display home phone", 0, @"True", "AD7359D0-CFF3-435F-98E2-F552A75DDD89" );

            RockMigrationHelper.AddBlockTypeAttribute( "B8CA0630-29E7-41B9-B4F1-EB6DE043EBDC", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "", "The record status to use for new individuals (default: 'Pending'.)", 0, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "E967EC6C-7DFD-42B4-94B9-AFC093BE8093" );

            RockMigrationHelper.AddBlockTypeAttribute( "B8CA0630-29E7-41B9-B4F1-EB6DE043EBDC", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "", "The connection status to use for new individuals (default: 'Web Prospect'.)", 0, @"368DD475-242C-49C4-A42C-7278BE690CC2", "BCCD15FA-DF2C-455E-A73F-BE6183FB0F74" );

            RockMigrationHelper.AddBlockTypeAttribute( "B8CA0630-29E7-41B9-B4F1-EB6DE043EBDC", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "Lava template to use to display the list of opportunities.", 2, @"{% include '~/Themes/Stark/Assets/Lava/ExternalOpportunityResponseMessage.lava' %}", "B6DD8328-5D75-444C-AE8D-05011165668D" );

            RockMigrationHelper.AddBlockTypeAttribute( "3F69E04F-F966-4CAE-B89D-F97DFEF6407A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Connection Type Id", "ConnectionTypeId", "", "The Id of the connection type that determines the opportunities listed.", 0, @"1", "06DD6554-AD0E-40E5-9F36-925107FC5642" );
            RockMigrationHelper.AddBlockAttributeValue( "80710A2C-9B90-40AE-B887-B885AAA43538", "06DD6554-AD0E-40E5-9F36-925107FC5642", @"1" ); // Connection Type Id

            AddColumn( "dbo.ConnectionOpportunityCampus", "ConnectorGroupId", c => c.Int() );
            AddColumn( "dbo.ConnectionOpportunity", "GroupMemberStatusId", c => c.Int() );
            DropForeignKey( "dbo.ConnectionOpportunityGroupCampus", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.ConnectionOpportunityGroupCampus", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.ConnectionOpportunityGroupCampus", "ConnectorGroupId", "dbo.Group" );
            DropForeignKey( "dbo.ConnectionOpportunityGroupCampus", "ConnectionOpportunityId", "dbo.ConnectionOpportunity" );
            DropForeignKey( "dbo.ConnectionOpportunityGroupCampus", "CampusId", "dbo.Campus" );
            DropIndex( "dbo.ConnectionOpportunityGroupCampus", new[] { "ForeignId" } );
            DropIndex( "dbo.ConnectionOpportunityGroupCampus", new[] { "Guid" } );
            DropIndex( "dbo.ConnectionOpportunityGroupCampus", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.ConnectionOpportunityGroupCampus", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.ConnectionOpportunityGroupCampus", new[] { "ConnectorGroupId" } );
            DropIndex( "dbo.ConnectionOpportunityGroupCampus", new[] { "CampusId" } );
            DropIndex( "dbo.ConnectionOpportunityGroupCampus", new[] { "ConnectionOpportunityId" } );
            AlterColumn( "dbo.ConnectionStatus", "Description", c => c.String( nullable: false ) );
            DropTable( "dbo.ConnectionOpportunityGroupCampus" );
            CreateIndex( "dbo.ConnectionOpportunityCampus", "ConnectorGroupId" );
            AddForeignKey( "dbo.ConnectionOpportunityCampus", "ConnectorGroupId", "dbo.Group", "Id", cascadeDelete: true );
        }
    }
}
