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
    public partial class FinancialTransactionAlertTypeAccount : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.FinancialTransactionAlertType", "FinancialAccountId", c => c.Int());
            AddColumn("dbo.FinancialTransactionAlertType", "IncludeChildFinancialAccounts", c => c.Boolean(nullable: false));
            AddColumn("dbo.FinancialTransactionAlertType", "AccountParticipantSystemCommunicationId", c => c.Int());
            CreateIndex("dbo.FinancialTransactionAlertType", "FinancialAccountId");
            CreateIndex("dbo.FinancialTransactionAlertType", "AccountParticipantSystemCommunicationId");
            AddForeignKey("dbo.FinancialTransactionAlertType", "AccountParticipantSystemCommunicationId", "dbo.SystemCommunication", "Id");
            AddForeignKey("dbo.FinancialTransactionAlertType", "FinancialAccountId", "dbo.FinancialAccount", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.FinancialTransactionAlertType", "FinancialAccountId", "dbo.FinancialAccount");
            DropForeignKey("dbo.FinancialTransactionAlertType", "AccountParticipantSystemCommunicationId", "dbo.SystemCommunication");
            DropIndex("dbo.FinancialTransactionAlertType", new[] { "AccountParticipantSystemCommunicationId" });
            DropIndex("dbo.FinancialTransactionAlertType", new[] { "FinancialAccountId" });
            DropColumn("dbo.FinancialTransactionAlertType", "AccountParticipantSystemCommunicationId");
            DropColumn("dbo.FinancialTransactionAlertType", "IncludeChildFinancialAccounts");
            DropColumn("dbo.FinancialTransactionAlertType", "FinancialAccountId");
        }
    }
}
