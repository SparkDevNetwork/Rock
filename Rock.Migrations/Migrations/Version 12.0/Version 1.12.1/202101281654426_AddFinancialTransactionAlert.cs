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
    public partial class AddFinancialTransactionAlert : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.FinancialTransactionAlert",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TransactionId = c.Int(),
                        PersonAliasId = c.Int(nullable: false),
                        GivingId = c.String(maxLength: 50),
                        AlertTypeId = c.Int(nullable: false),
                        Amount = c.Decimal(precision: 18, scale: 2),
                        AmountCurrentMedian = c.Decimal(precision: 18, scale: 2),
                        AmountCurrentIqr = c.Decimal(precision: 18, scale: 2),
                        AmountIqrMultiplier = c.Decimal(precision: 6, scale: 1),
                        FrequencyCurrentMean = c.Decimal(precision: 6, scale: 1),
                        FrequencyCurrentStandardDeviation = c.Decimal(precision: 6, scale: 1),
                        FrequencyDifferenceFromMean = c.Decimal(precision: 6, scale: 1),
                        FrequencyZScore = c.Decimal(precision: 6, scale: 1),
                        ReasonsKey = c.String(maxLength: 2500),
                        AlertDateTime = c.DateTime(nullable: false),
                        AlertDateKey = c.Int(nullable: false),
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
                .ForeignKey("dbo.FinancialTransaction", t => t.TransactionId)
                .ForeignKey("dbo.FinancialTransactionAlertType", t => t.AlertTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId)
                .Index(t => t.TransactionId)
                .Index(t => t.PersonAliasId)
                .Index(t => t.AlertTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.FinancialTransactionAlertType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 250),
                        CampusId = c.Int(),
                        AlertType = c.Int(nullable: false),
                        ContinueIfMatched = c.Boolean(),
                        RepeatPreventionDuration = c.Int(),
                        FrequencySensitivityScale = c.Decimal(precision: 6, scale: 1),
                        AmountSensitivityScale = c.Decimal(precision: 6, scale: 2),
                        MinimumGiftAmount = c.Decimal(precision: 18, scale: 2),
                        MaximumGiftAmount = c.Decimal(precision: 18, scale: 2),
                        MinimumMedianGiftAmount = c.Decimal(precision: 18, scale: 2),
                        MaximumMedianGiftAmount = c.Decimal(precision: 18, scale: 2),
                        DataViewId = c.Int(),
                        WorkflowTypeId = c.Int(),
                        ConnectionOpportunityId = c.Int(),
                        SystemCommunicationId = c.Int(nullable: false),
                        SendBusEvent = c.Boolean(nullable: false),
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
                .ForeignKey("dbo.Campus", t => t.CampusId)
                .ForeignKey("dbo.ConnectionOpportunity", t => t.ConnectionOpportunityId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.DataView", t => t.DataViewId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.SystemCommunication", t => t.SystemCommunicationId)
                .ForeignKey("dbo.WorkflowType", t => t.WorkflowTypeId)
                .Index(t => t.CampusId)
                .Index(t => t.DataViewId)
                .Index(t => t.WorkflowTypeId)
                .Index(t => t.ConnectionOpportunityId)
                .Index(t => t.SystemCommunicationId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.FinancialTransactionAlert", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.FinancialTransactionAlert", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.FinancialTransactionAlert", "AlertTypeId", "dbo.FinancialTransactionAlertType");
            DropForeignKey("dbo.FinancialTransactionAlertType", "WorkflowTypeId", "dbo.WorkflowType");
            DropForeignKey("dbo.FinancialTransactionAlertType", "SystemCommunicationId", "dbo.SystemCommunication");
            DropForeignKey("dbo.FinancialTransactionAlertType", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.FinancialTransactionAlertType", "DataViewId", "dbo.DataView");
            DropForeignKey("dbo.FinancialTransactionAlertType", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.FinancialTransactionAlertType", "ConnectionOpportunityId", "dbo.ConnectionOpportunity");
            DropForeignKey("dbo.FinancialTransactionAlertType", "CampusId", "dbo.Campus");
            DropForeignKey("dbo.FinancialTransactionAlert", "TransactionId", "dbo.FinancialTransaction");
            DropForeignKey("dbo.FinancialTransactionAlert", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.FinancialTransactionAlertType", new[] { "Guid" });
            DropIndex("dbo.FinancialTransactionAlertType", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.FinancialTransactionAlertType", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.FinancialTransactionAlertType", new[] { "SystemCommunicationId" });
            DropIndex("dbo.FinancialTransactionAlertType", new[] { "ConnectionOpportunityId" });
            DropIndex("dbo.FinancialTransactionAlertType", new[] { "WorkflowTypeId" });
            DropIndex("dbo.FinancialTransactionAlertType", new[] { "DataViewId" });
            DropIndex("dbo.FinancialTransactionAlertType", new[] { "CampusId" });
            DropIndex("dbo.FinancialTransactionAlert", new[] { "Guid" });
            DropIndex("dbo.FinancialTransactionAlert", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.FinancialTransactionAlert", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.FinancialTransactionAlert", new[] { "AlertTypeId" });
            DropIndex("dbo.FinancialTransactionAlert", new[] { "PersonAliasId" });
            DropIndex("dbo.FinancialTransactionAlert", new[] { "TransactionId" });
            DropTable("dbo.FinancialTransactionAlertType");
            DropTable("dbo.FinancialTransactionAlert");
        }
    }
}
