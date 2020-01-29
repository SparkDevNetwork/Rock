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
    public partial class AddedSmsPipelineAndUpdatedSmsAction : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.SmsPipeline",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            AddColumn("dbo.SmsAction", "SmsPipelineId", c => c.Int(nullable: true));
            
            AddDefaultSmsPipelineAndUpdateSmsAction();

            AlterColumn( "dbo.SmsAction", "SmsPipelineId", c => c.Int( nullable: false ) );
            CreateIndex( "dbo.SmsAction", "SmsPipelineId" );
            AddForeignKey( "dbo.SmsAction", "SmsPipelineId", "dbo.SmsPipeline", "Id", cascadeDelete: true );

            UpdatePageAndBlockSettings();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RemoveNonDefaultSmsActions();

            RemovePageAndBlockSettings();

            DropForeignKey( "dbo.SmsAction", "SmsPipelineId", "dbo.SmsPipeline");
            DropForeignKey("dbo.SmsPipeline", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.SmsPipeline", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.SmsPipeline", new[] { "Guid" });
            DropIndex("dbo.SmsPipeline", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.SmsPipeline", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.SmsAction", new[] { "SmsPipelineId" });
            DropColumn("dbo.SmsAction", "SmsPipelineId");
            DropTable("dbo.SmsPipeline");
        }

        private void AddDefaultSmsPipelineAndUpdateSmsAction()
        {
            Sql( @"IF (SELECT COUNT(*) FROM [dbo].[SmsAction]) > 0
                    BEGIN
                        INSERT INTO [dbo].[SmsPipeline] ([Name], [IsActive], [Guid])
                        VALUES ('Default', 1, NEWID())
                    
                        DECLARE @smsPipelineId AS INT = @@IDENTITY

                        UPDATE [dbo].[SmsAction] SET SmsPipelineID = @smsPipelineId
                    END" );
        }

        private void RemoveNonDefaultSmsActions()
        {
            Sql( @"DECLARE @defaultSmsPipelineId AS INT

                    SELECT @defaultSmsPipelineId = MIN(Id)
                    FROM [dbo].[SmsPipeline]
                    
                    IF @defaultSmsPipelineId IS NOT NULL
                    BEGIN
                        DELETE SmsAction
                        WHERE SmsPipelineId != @defaultSmsPipelineId
                    END" );
        }

        private void UpdatePageAndBlockSettings()
        {
            // Rename block to SmsPipelineDetail.ascx
            RockMigrationHelper.UpdateBlockTypeByGuid( "SMS Pipeline Detail", "Configures the pipeline that processes an incoming SMS message.", "~/Blocks/Communication/SmsPipelineDetail.ascx", "Communication", "44C32EB7-4DA3-4577-AC41-E3517442E269" );
            
            RockMigrationHelper.AddPage( true, "2277986A-F53D-4E46-B6EC-6BAD1111DA39", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "SMS Pipeline Detail", "", "FCE39659-4D86-48D7-9C48-D837D3588C42", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "SMS Pipeline List", "Lists the SMS Pipelines currently in the system.", "~/Blocks/Communication/SmsPipelineList.ascx", "Communication", "DB6FD0BF-FDCE-48DA-919C-240F029518A2" );
            
            // Remove Block: SMS Pipeline Detail, from Page: SMS Pipeline List, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "F6CA6D07-DDF4-47DD-AA7E-29F5DCCC2DDC" );

            // Add Block to Page: SMS Pipeline Detail Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "FCE39659-4D86-48D7-9C48-D837D3588C42".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "44C32EB7-4DA3-4577-AC41-E3517442E269".AsGuid(), "SMS Pipeline Detail", "Main", @"", @"", 0, "B191F27B-2A1D-4A2F-9E06-49BC2D8B1E5B" );

            // Add Block to Page: SMS Pipeline List Detail Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "2277986A-F53D-4E46-B6EC-6BAD1111DA39".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "DB6FD0BF-FDCE-48DA-919C-240F029518A2".AsGuid(), "SMS Pipeline List", "Main", @"", @"", 0, "EC3BDDFC-4688-485B-9EF3-890B9EC39527" );
            
            // Attrib for BlockType: SMS Pipeline List:SMS Pipeline Detail
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DB6FD0BF-FDCE-48DA-919C-240F029518A2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "SMS Pipeline Detail", "SMSPipelineDetail", "SMS Pipeline Detail", @"", 0, @"", "FA40D714-9D6F-4620-97B1-07BFFFC08F96" );

            // Attrib Value for Block:SMS Pipeline List, Attribute:core.CustomGridEnableStickyHeaders Page: SMS Pipeline, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "EC3BDDFC-4688-485B-9EF3-890B9EC39527", "27549AA7-AE69-4121-AAAC-DBA8EE5ED6EE", @"False" );

            // Attrib Value for Block:SMS Pipeline List, Attribute:SMS Pipeline Detail Page: SMS Pipeline, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "EC3BDDFC-4688-485B-9EF3-890B9EC39527", "FA40D714-9D6F-4620-97B1-07BFFFC08F96", @"fce39659-4d86-48d7-9c48-d837d3588c42" );
        }

        private void RemovePageAndBlockSettings()
        {
            // Attrib for BlockType: SMS Pipeline List:SMS Pipeline Detail
            RockMigrationHelper.DeleteAttribute( "FA40D714-9D6F-4620-97B1-07BFFFC08F96" );

            // Remove Block: SMS Pipeline List, from Page: SMS Pipeline, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "EC3BDDFC-4688-485B-9EF3-890B9EC39527" );
            
            // Remove Block: SMS Pipeline Detail, from Page: SMS Pipeline Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B191F27B-2A1D-4A2F-9E06-49BC2D8B1E5B" );
            
            // SMS Pipeline List
            RockMigrationHelper.DeleteBlockType( "DB6FD0BF-FDCE-48DA-919C-240F029518A2" );
            
            //  Page: SMS Pipeline Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "FCE39659-4D86-48D7-9C48-D837D3588C42" );

            // Add Block to Page: SMS Pipeline Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "2277986A-F53D-4E46-B6EC-6BAD1111DA39".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "44C32EB7-4DA3-4577-AC41-E3517442E269".AsGuid(), "SMS Pipeline", "Main", @"", @"", 0, "F6CA6D07-DDF4-47DD-AA7E-29F5DCCC2DDC" );

            // Rename block to SmsPipeline.ascx
            RockMigrationHelper.UpdateBlockTypeByGuid( "SMS Pipeline", "Configures the pipeline that processes an incoming SMS message.", "~/Blocks/Communication/SmsPipeline.ascx", "Communication", "44C32EB7-4DA3-4577-AC41-E3517442E269" );
        }
    }
}
