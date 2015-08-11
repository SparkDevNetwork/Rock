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
    public partial class FinancialPaymentDetails : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.FinancialTransaction", "CreditCardTypeValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.FinancialTransaction", "CurrencyTypeValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.FinancialScheduledTransaction", "CreditCardTypeValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.FinancialScheduledTransaction", "CurrencyTypeValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.FinancialPersonSavedAccount", "CreditCardTypeValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.FinancialPersonSavedAccount", "CurrencyTypeValueId", "dbo.DefinedValue");
            DropIndex("dbo.FinancialTransaction", new[] { "CurrencyTypeValueId" });
            DropIndex("dbo.FinancialTransaction", new[] { "CreditCardTypeValueId" });
            DropIndex("dbo.FinancialScheduledTransaction", new[] { "CurrencyTypeValueId" });
            DropIndex("dbo.FinancialScheduledTransaction", new[] { "CreditCardTypeValueId" });
            DropIndex("dbo.FinancialPersonSavedAccount", new[] { "CurrencyTypeValueId" });
            DropIndex("dbo.FinancialPersonSavedAccount", new[] { "CreditCardTypeValueId" });

            CreateTable(
                "dbo.FinancialPaymentDetail",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccountNumberMasked = c.String(),
                        CurrencyTypeValueId = c.Int(),
                        CreditCardTypeValueId = c.Int(),
                        NameOnCardEncrypted = c.String(maxLength: 256),
                        ExpirationMonthEncrypted = c.String(maxLength: 256),
                        ExpirationYearEncrypted = c.String(maxLength: 256),
                        BillingLocationId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.DefinedValue", t => t.CreditCardTypeValueId)
                .ForeignKey("dbo.DefinedValue", t => t.CurrencyTypeValueId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CurrencyTypeValueId)
                .Index(t => t.CreditCardTypeValueId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            AddColumn("dbo.FinancialTransaction", "FinancialPaymentDetailId", c => c.Int());
            AddColumn("dbo.FinancialScheduledTransaction", "FinancialPaymentDetailId", c => c.Int());
            AddColumn("dbo.FinancialPersonSavedAccount", "FinancialPaymentDetailId", c => c.Int());
            CreateIndex("dbo.FinancialTransaction", "FinancialPaymentDetailId");
            CreateIndex("dbo.FinancialScheduledTransaction", "FinancialPaymentDetailId");
            CreateIndex("dbo.FinancialPersonSavedAccount", "FinancialPaymentDetailId");
            AddForeignKey("dbo.FinancialTransaction", "FinancialPaymentDetailId", "dbo.FinancialPaymentDetail", "Id");
            AddForeignKey("dbo.FinancialScheduledTransaction", "FinancialPaymentDetailId", "dbo.FinancialPaymentDetail", "Id");
            AddForeignKey("dbo.FinancialPersonSavedAccount", "FinancialPaymentDetailId", "dbo.FinancialPaymentDetail", "Id");

            // Copy existing data to new table before removing columns
            Sql( MigrationSQL._201508111636214_FinancialPaymentDetails );

            DropColumn("dbo.FinancialTransaction", "CurrencyTypeValueId");
            DropColumn("dbo.FinancialTransaction", "CreditCardTypeValueId");
            DropColumn("dbo.FinancialScheduledTransaction", "CurrencyTypeValueId");
            DropColumn("dbo.FinancialScheduledTransaction", "CreditCardTypeValueId");
            DropColumn("dbo.FinancialPersonSavedAccount", "MaskedAccountNumber");
            DropColumn("dbo.FinancialPersonSavedAccount", "CurrencyTypeValueId");
            DropColumn("dbo.FinancialPersonSavedAccount", "CreditCardTypeValueId");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.FinancialPersonSavedAccount", "CreditCardTypeValueId", c => c.Int());
            AddColumn("dbo.FinancialPersonSavedAccount", "CurrencyTypeValueId", c => c.Int());
            AddColumn("dbo.FinancialPersonSavedAccount", "MaskedAccountNumber", c => c.String(maxLength: 100));
            AddColumn("dbo.FinancialScheduledTransaction", "CreditCardTypeValueId", c => c.Int());
            AddColumn("dbo.FinancialScheduledTransaction", "CurrencyTypeValueId", c => c.Int());
            AddColumn("dbo.FinancialTransaction", "CreditCardTypeValueId", c => c.Int());
            AddColumn("dbo.FinancialTransaction", "CurrencyTypeValueId", c => c.Int());
            DropForeignKey("dbo.FinancialPersonSavedAccount", "FinancialPaymentDetailId", "dbo.FinancialPaymentDetail");
            DropForeignKey("dbo.FinancialScheduledTransaction", "FinancialPaymentDetailId", "dbo.FinancialPaymentDetail");
            DropForeignKey("dbo.FinancialTransaction", "FinancialPaymentDetailId", "dbo.FinancialPaymentDetail");
            DropForeignKey("dbo.FinancialPaymentDetail", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.FinancialPaymentDetail", "CurrencyTypeValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.FinancialPaymentDetail", "CreditCardTypeValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.FinancialPaymentDetail", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.FinancialPersonSavedAccount", new[] { "FinancialPaymentDetailId" });
            DropIndex("dbo.FinancialScheduledTransaction", new[] { "FinancialPaymentDetailId" });
            DropIndex("dbo.FinancialPaymentDetail", new[] { "ForeignId" });
            DropIndex("dbo.FinancialPaymentDetail", new[] { "Guid" });
            DropIndex("dbo.FinancialPaymentDetail", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.FinancialPaymentDetail", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.FinancialPaymentDetail", new[] { "CreditCardTypeValueId" });
            DropIndex("dbo.FinancialPaymentDetail", new[] { "CurrencyTypeValueId" });
            DropIndex("dbo.FinancialTransaction", new[] { "FinancialPaymentDetailId" });
            DropColumn("dbo.FinancialPersonSavedAccount", "FinancialPaymentDetailId");
            DropColumn("dbo.FinancialScheduledTransaction", "FinancialPaymentDetailId");
            DropColumn("dbo.FinancialTransaction", "FinancialPaymentDetailId");
            DropTable("dbo.FinancialPaymentDetail");
            CreateIndex("dbo.FinancialPersonSavedAccount", "CreditCardTypeValueId");
            CreateIndex("dbo.FinancialPersonSavedAccount", "CurrencyTypeValueId");
            CreateIndex("dbo.FinancialScheduledTransaction", "CreditCardTypeValueId");
            CreateIndex("dbo.FinancialScheduledTransaction", "CurrencyTypeValueId");
            CreateIndex("dbo.FinancialTransaction", "CreditCardTypeValueId");
            CreateIndex("dbo.FinancialTransaction", "CurrencyTypeValueId");
            AddForeignKey("dbo.FinancialPersonSavedAccount", "CurrencyTypeValueId", "dbo.DefinedValue", "Id");
            AddForeignKey("dbo.FinancialPersonSavedAccount", "CreditCardTypeValueId", "dbo.DefinedValue", "Id");
            AddForeignKey("dbo.FinancialScheduledTransaction", "CurrencyTypeValueId", "dbo.DefinedValue", "Id");
            AddForeignKey("dbo.FinancialScheduledTransaction", "CreditCardTypeValueId", "dbo.DefinedValue", "Id");
            AddForeignKey("dbo.FinancialTransaction", "CurrencyTypeValueId", "dbo.DefinedValue", "Id");
            AddForeignKey("dbo.FinancialTransaction", "CreditCardTypeValueId", "dbo.DefinedValue", "Id");
        }
    }
}
