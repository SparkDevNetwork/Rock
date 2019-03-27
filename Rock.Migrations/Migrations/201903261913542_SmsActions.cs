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
    public partial class SmsActions : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateSmsActionUp();
            PagesAndBlocksUp();
            PersonsDefaultAccountUp();
            FutureTransactionUp();
            
            Sql( $@"
                UPDATE [DefinedType]
                SET 
                    [Name] = 'Text To Workflow (Legacy)', 
                    [Description] = 'Matches SMS phones and keywords to launch workflows of various types. This method of launching workflows from incoming text messages is now considered legacy. Please look at migrating to the new SMS Actions instead.'
                WHERE [Guid] = '{ Rock.SystemGuid.DefinedType.TEXT_TO_WORKFLOW }'" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CreateSmsActionDown();
            PagesAndBlocksDown();
            PersonsDefaultAccountDown();
            FutureTransactionDown();

            Sql( $@"
                UPDATE [DefinedType]
                SET 
                    [Name] = 'Text To Workflow', 
                    [Description] = 'Matches SMS phones and keywords to launch workflows of various types'
                WHERE [Guid] = '{ Rock.SystemGuid.DefinedType.TEXT_TO_WORKFLOW }'" );
        }

        private void FutureTransactionUp()
        {
            AddColumn( "dbo.FinancialTransaction", "FutureProcessingDateTime", c => c.DateTime( nullable: true ) );
            CreateIndex( "dbo.FinancialTransaction", "FutureProcessingDateTime" );
        }

        private void FutureTransactionDown()
        {
            DropIndex( "dbo.FinancialTransaction", new [] { "FutureProcessingDateTime" } );
            DropColumn( "dbo.FinancialTransaction", "FutureProcessingDateTime" );
        }

        private void PersonsDefaultAccountUp()
        {
            AddColumn( "dbo.Person", "ContributionFinancialAccountId", c => c.Int( nullable: true ) );
            AddForeignKey( "dbo.Person", "ContributionFinancialAccountId", "dbo.FinancialAccount", "Id" );
        }

        private void PersonsDefaultAccountDown()
        {
            DropForeignKey( "dbo.Person", "ContributionFinancialAccountId", "dbo.FinancialAccount" );
            DropColumn( "dbo.Person", "ContributionFinancialAccountId" );
        }

        private void CreateSmsActionUp()
        {
            CreateTable(
                "dbo.SmsAction",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String(),
                    IsActive = c.Boolean( nullable: false ),
                    Order = c.Int( nullable: false ),
                    SmsActionComponentEntityTypeId = c.Int( nullable: false ),
                    ContinueAfterProcessing = c.Boolean( nullable: false ),
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
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );
        }

        private void CreateSmsActionDown()
        {
            DropForeignKey( "dbo.SmsAction", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.SmsAction", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropIndex( "dbo.SmsAction", new[] { "Guid" } );
            DropIndex( "dbo.SmsAction", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.SmsAction", new[] { "CreatedByPersonAliasId" } );
            DropTable( "dbo.SmsAction" );
        }

        private void PagesAndBlocksUp()
        {
            RockMigrationHelper.AddPage( true, "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "SMS Actions", "", "2277986A-F53D-4E46-B6EC-6BAD1111DA39", "fa fa-sms" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Communication > SMS Actions", "", "~/Blocks/Communication/SmsActions.ascx", "", "44C32EB7-4DA3-4577-AC41-E3517442E269" );
            // Add Block to Page: SMS Actions Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "2277986A-F53D-4E46-B6EC-6BAD1111DA39".AsGuid(), null, Rock.SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), "44C32EB7-4DA3-4577-AC41-E3517442E269".AsGuid(), "Sms Actions", "Main", @"", @"", 0, "F6CA6D07-DDF4-47DD-AA7E-29F5DCCC2DDC" );
        }

        private void PagesAndBlocksDown()
        {
            // Remove Block: Sms Actions, from Page: SMS Actions, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "F6CA6D07-DDF4-47DD-AA7E-29F5DCCC2DDC" );
            RockMigrationHelper.DeleteBlockType( "44C32EB7-4DA3-4577-AC41-E3517442E269" ); // Communication > Sms Actions
            RockMigrationHelper.DeletePage( "2277986A-F53D-4E46-B6EC-6BAD1111DA39" ); //  Page: SMS Actions, Layout: Full Width, Site: Rock RMS
        }
    }
}
