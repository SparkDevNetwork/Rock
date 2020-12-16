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
    public partial class WebFarmNodeMetric : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.WebFarmNodeMetric",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    WebFarmNodeId = c.Int( nullable: false ),
                    MetricType = c.Int( nullable: false ),
                    MetricValue = c.Decimal( nullable: false, precision: 18, scale: 2 ),
                    Note = c.String(),
                    MetricValueDateTime = c.DateTime( nullable: false ),
                    MetricValueDateKey = c.Int( nullable: false ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.WebFarmNode", t => t.WebFarmNodeId, cascadeDelete: true )
                .Index( t => new { t.WebFarmNodeId, t.MetricType, t.MetricValueDateTime }, name: "IX_WebFarmNode_MetricType_MetricValueDateTime" )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CmsUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CmsDown();

            DropForeignKey( "dbo.WebFarmNodeMetric", "WebFarmNodeId", "dbo.WebFarmNode" );
            DropForeignKey( "dbo.WebFarmNodeMetric", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.WebFarmNodeMetric", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropIndex( "dbo.WebFarmNodeMetric", new[] { "Guid" } );
            DropIndex( "dbo.WebFarmNodeMetric", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.WebFarmNodeMetric", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.WebFarmNodeMetric", "IX_WebFarmNode_MetricType_MetricValueDateTime" );
            DropTable( "dbo.WebFarmNodeMetric" );
        }

        /// <summary>
        /// CMSs up.
        /// </summary>
        private void CmsUp()
        {
            // Add Page Web Farm Node to Site:Rock RMS
            RockMigrationHelper.AddPage( true, "249BE98D-9DDE-4B19-9D97-9C76D9EA3056", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Web Farm Node", "", "63698D5C-7C73-44A4-A27D-A7EB777EB2A2", "" );

            // Add/Update BlockType Web Farm Node Detail
            RockMigrationHelper.UpdateBlockType( "Web Farm Node Detail", "Displays the details of the Web Farm Node.", "~/Blocks/Farm/NodeDetail.ascx", "Farm", "95F38562-6CEF-4798-8A4F-05EBCDFB07E0" );

            // Add Block Log to Page: Web Farm Node, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "63698D5C-7C73-44A4-A27D-A7EB777EB2A2".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "63ADDB5A-75D6-4E86-A031-98B3451C49A3".AsGuid(), "Log", "Main", @"", @"", 1, "F11880E1-621C-4793-8F68-21C728FE0A2B" );

            // Add Block Web Farm Node Detail to Page: Web Farm Node, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "63698D5C-7C73-44A4-A27D-A7EB777EB2A2".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "95F38562-6CEF-4798-8A4F-05EBCDFB07E0".AsGuid(), "Web Farm Node Detail", "Main", @"", @"", 0, "F77FA989-CD77-40B8-8254-3144A3F6E7F5" );

            // update block order for pages with new blocks if the page,zone has multiple blocks

            // Update Order for Page: Web Farm Node,  Zone: Main,  Block: Log
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'F11880E1-621C-4793-8F68-21C728FE0A2B'" );

            // Update Order for Page: Web Farm Node,  Zone: Main,  Block: Web Farm Node Detail
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'F77FA989-CD77-40B8-8254-3144A3F6E7F5'" );

            // Attribute for BlockType: Web Farm Settings:Farm Node Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4280625A-C69A-4B47-A4D3-89B61F43C967", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Farm Node Detail Page", "NodeDetailPage", "Farm Node Detail Page", @"The page where the node details can be seen", 1, @"63698D5C-7C73-44A4-A27D-A7EB777EB2A2", "A3E05855-B962-4447-BC8C-0B26A9F736E1" );
        }

        private void CmsDown()
        {
            // Farm Node Detail Page Attribute for BlockType: Web Farm Settings
            RockMigrationHelper.DeleteAttribute( "A3E05855-B962-4447-BC8C-0B26A9F736E1" );

            // Remove Block: Web Farm Node Detail, from Page: Web Farm Node, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "F77FA989-CD77-40B8-8254-3144A3F6E7F5" );

            // Remove Block: Log, from Page: Web Farm Node, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "F11880E1-621C-4793-8F68-21C728FE0A2B" );

            // Delete BlockType Web Farm Node Detail
            RockMigrationHelper.DeleteBlockType( "95F38562-6CEF-4798-8A4F-05EBCDFB07E0" ); // Web Farm Node Detail

            // Delete Page Web Farm Node from Site:Rock RMS
            RockMigrationHelper.DeletePage( "63698D5C-7C73-44A4-A27D-A7EB777EB2A2" ); //  Page: Web Farm Node, Layout: Full Width, Site: Rock RMS
        }
    }
}
