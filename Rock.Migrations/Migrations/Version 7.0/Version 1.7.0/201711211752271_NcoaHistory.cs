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
    public partial class NcoaHistory : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.NcoaHistory",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonAliasId = c.Int(nullable: false),
                        FamilyId = c.Int(nullable: false),
                        LocationId = c.Int(),
                        MoveType = c.Int(nullable: false),
                        NcoaType = c.Int(nullable: false),
                        AddressStatus = c.Int(nullable: false),
                        AddressInvalidReason = c.Int(nullable: false),
                        OriginalStreet1 = c.String(maxLength: 100),
                        OriginalStreet2 = c.String(maxLength: 100),
                        OriginalCity = c.String(maxLength: 50),
                        OriginalState = c.String(maxLength: 50),
                        OriginalPostalCode = c.String(maxLength: 50),
                        UpdatedStreet1 = c.String(maxLength: 100),
                        UpdatedStreet2 = c.String(maxLength: 100),
                        UpdatedCity = c.String(maxLength: 50),
                        UpdatedState = c.String(maxLength: 50),
                        UpdatedPostalCode = c.String(maxLength: 50),
                        UpdatedCountry = c.String(maxLength: 50),
                        UpdatedBarcode = c.String(maxLength: 40),
                        UpdatedAddressType = c.Int(nullable: false),
                        MoveDate = c.DateTime(),
                        MoveDistance = c.Decimal(precision: 6, scale: 2),
                        MatchFlag = c.Int(nullable: false),
                        Processed = c.Int(nullable: false),
                        NcoaRunDateTime = c.DateTime(nullable: false),
                        NcoaNote = c.String(),
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

            // MP: Background Check and FinancialBatch model changes
            RenameColumn( "dbo.BackgroundCheck", "ResponseXml", "ResponseData" );

            AddColumn("dbo.BackgroundCheck", "ProcessorEntityTypeId", c => c.Int());
            AddColumn("dbo.BackgroundCheck", "Status", c => c.String(maxLength: 25));
            AddColumn("dbo.BackgroundCheck", "PackageName", c => c.String(maxLength: 100));
            AddColumn( "dbo.BackgroundCheck", "ResponseId", c => c.String( maxLength: 100 ) );
            AddColumn( "dbo.BackgroundCheck", "RequestId", c => c.String( maxLength: 100 ) );

            AddColumn("dbo.FinancialBatch", "IsAutomated", c => c.Boolean(nullable: false));

            CreateIndex("dbo.BackgroundCheck", "ProcessorEntityTypeId");
            AddForeignKey("dbo.BackgroundCheck", "ProcessorEntityTypeId", "dbo.EntityType", "Id");

            // Update Communication Templates
            // NOTE: Order is important here
            Sql( MigrationSQL._201711211752271_NcoaHistory_UpdateCommunicationTemplateBinaryFiles );
            Sql( MigrationSQL._201711211752271_NcoaHistory_UpdateCommunicationTemplates );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RenameColumn( "dbo.BackgroundCheck", "ResponseData", "ResponseXml" );
            
            DropForeignKey("dbo.NcoaHistory", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.NcoaHistory", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.BackgroundCheck", "ProcessorEntityTypeId", "dbo.EntityType");
            DropIndex("dbo.NcoaHistory", new[] { "Guid" });
            DropIndex("dbo.NcoaHistory", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.NcoaHistory", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.BackgroundCheck", new[] { "ProcessorEntityTypeId" });
            DropColumn("dbo.FinancialBatch", "IsAutomated");
            DropColumn("dbo.BackgroundCheck", "PackageName");
            DropColumn("dbo.BackgroundCheck", "Status");
            DropColumn("dbo.BackgroundCheck", "ProcessorEntityTypeId");
            DropColumn( "dbo.BackgroundCheck", "ResponseId" );
            DropColumn( "dbo.BackgroundCheck", "RequestId" );

            DropTable("dbo.NcoaHistory");
        }
    }
}
