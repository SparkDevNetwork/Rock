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
    public partial class RegistrationTemplateFeeItem : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.RegistrationTemplateFeeItem",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RegistrationTemplateFeeId = c.Int(nullable: false),
                        Order = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MaximumUsageCount = c.Int(),
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
                .ForeignKey("dbo.RegistrationTemplateFee", t => t.RegistrationTemplateFeeId, cascadeDelete: true)
                .Index(t => t.RegistrationTemplateFeeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            AddColumn("dbo.RegistrationInstance", "DefaultPayment", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.RegistrationRegistrantFee", "RegistrationTemplateFeeItemId", c => c.Int());
            AddColumn("dbo.RegistrationTemplate", "DefaultPayment", c => c.Decimal(precision: 18, scale: 2));
            CreateIndex("dbo.RegistrationRegistrantFee", "RegistrationTemplateFeeItemId");
            AddForeignKey("dbo.RegistrationRegistrantFee", "RegistrationTemplateFeeItemId", "dbo.RegistrationTemplateFeeItem", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.RegistrationRegistrantFee", "RegistrationTemplateFeeItemId", "dbo.RegistrationTemplateFeeItem");
            DropForeignKey("dbo.RegistrationTemplateFeeItem", "RegistrationTemplateFeeId", "dbo.RegistrationTemplateFee");
            DropForeignKey("dbo.RegistrationTemplateFeeItem", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RegistrationTemplateFeeItem", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.RegistrationTemplateFeeItem", new[] { "Guid" });
            DropIndex("dbo.RegistrationTemplateFeeItem", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.RegistrationTemplateFeeItem", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.RegistrationTemplateFeeItem", new[] { "RegistrationTemplateFeeId" });
            DropIndex("dbo.RegistrationRegistrantFee", new[] { "RegistrationTemplateFeeItemId" });
            DropColumn("dbo.RegistrationTemplate", "DefaultPayment");
            DropColumn("dbo.RegistrationRegistrantFee", "RegistrationTemplateFeeItemId");
            DropColumn("dbo.RegistrationInstance", "DefaultPayment");
            DropTable("dbo.RegistrationTemplateFeeItem");
        }
    }
}
